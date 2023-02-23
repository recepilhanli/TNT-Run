using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;


namespace player.controller
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : NetworkBehaviour
    {

        public KeyCode jumpKey = KeyCode.Space;
        public KeyCode givingBombKey = KeyCode.Mouse0;
        public KeyCode runningKey = KeyCode.LeftShift;

        [System.NonSerialized]
        public CharacterController controller;
        [System.NonSerialized]
        public Vector3 playerVelocity;
        private bool groundedPlayer;
        private float playerSpeed = 6f;
        private float jumpHeight = 1.0f;
        private float gravityValue = -9.81f;

        private Cinemachine.CinemachineFreeLook cinemachine;
        private GameObject cam;
        private PlayerState state;
        private Slider EnergyProgressBar;
        public float Energy = 1f;

        private AudioSource GlidingEffect;
        private bool dash;
        public override void OnNetworkSpawn()
        {
            if (!IsOwner)
            {
             this.enabled = false;
            }

        }


        void Start()
        {
            SetCamera();
            controller = gameObject.GetComponent<CharacterController>();
            state = gameObject.GetComponent<PlayerState>();

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            EnergyProgressBar = GameObject.Find("RunBar").GetComponent<Slider>();
            EnergyProgressBar.gameObject.SetActive(false);
            Invoke("AdaptBarColor",0.3f);

            GlidingEffect = gameObject.GetComponent<AudioSource>();

        }

        void Update()
        {
            if (PlayerState.spawning == true) return;
            Move();
            Running();
            if (Chatting.Texting == true || PlayerState.spawning == true || PauseMenu.paused == true) return;
            PlayerOrientation();
            GivingBomb();
        }

        private void AdaptBarColor()
        {
            EnergyProgressBar.transform.GetChild(1).gameObject.transform.GetChild(0).GetComponent<Image>().color = state.Color.Value;
        }

        void Running()
        {
            
            if((Input.GetKey(runningKey) && playerVelocity.y != 0) || Energy > 1)
            {
                if (EnergyProgressBar.gameObject.activeInHierarchy == false) EnergyProgressBar.gameObject.SetActive(true);
                Energy -= Time.deltaTime/2.5F;
                Energy = Mathf.Clamp(Energy, 0f, 6f);
                EnergyProgressBar.value = Energy;
                playerSpeed = 10f;
                if (Energy <= 0.05) playerSpeed = 6F;
            }
            else if(!Input.GetKey(runningKey))
            {
                if(EnergyProgressBar.gameObject.activeInHierarchy == true && Energy >= 1f) EnergyProgressBar.gameObject.SetActive(false);
                if(Energy < 1f)
                {
                    Energy += Time.deltaTime/5;
                    Energy = Mathf.Clamp(Energy, 0f, 1f);
                    EnergyProgressBar.value = Energy;
                }
                if (playerSpeed > 6f) playerSpeed = 6f;
            }
        }

        void GivingBomb()
        {
            if (!Input.GetKeyDown(givingBombKey)) return;
            if (state.clientID.Value != PlayerState.game.HasBomb.Value) return;

            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, 2.5f))
            {
                if (hit.transform.gameObject.CompareTag("Player"))
                {
                    ulong id = hit.transform.gameObject.GetComponent<PlayerState>().clientID.Value;
                    PlayerState.game.GiveBombServerRPC(id);
                    PlayerData.Inventory.SetUsageEffect(Color.white);
                    gameObject.GetComponentInChildren<Animations>().PlayGiveAnimation();
                }


            }

        }


        void PlayerOrientation()
        {
            gameObject.transform.rotation = Quaternion.Euler(0, cam.transform.localRotation.eulerAngles.y, 0);
        }

        void SetCamera()
        {
            cam = GameObject.Find("MainCamera");
            cinemachine = GameObject.Find("CineMachine").GetComponent<Cinemachine.CinemachineFreeLook>();

            cinemachine.Follow = gameObject.transform;
            cinemachine.LookAt = gameObject.transform.GetChild(0);
        }

        void Move()
        {
            if (PlayerState.game.CanPlayersMove.Value == false) return;

            groundedPlayer = controller.isGrounded;
            if (groundedPlayer && playerVelocity.y < 0)
            {
                playerVelocity.y = 0f;
                dash = false;
            }
            else if (groundedPlayer) dash = false;

            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");

            if (Chatting.Texting == true || PauseMenu.paused == true)
            {
                h = 0;
                v = 0;
            }


            Transform orienttation = cam.transform;

            orienttation.rotation = Quaternion.Euler(0, orienttation.rotation.eulerAngles.y, 0);

            Vector3 move =  orienttation.forward * v  + orienttation.right * h;

            if ((h != 0 || v != 0 || Energy > 1) && GlidingEffect.enabled == false) GlidingEffect.enabled = true;
            else if ((h == 0 && v == 0 && Energy <= 1) && GlidingEffect.enabled == true) GlidingEffect.enabled = false;

            controller.Move(move * Time.deltaTime * playerSpeed);

            if (Input.GetKeyDown(jumpKey) && ((playerVelocity.y > -0.6 && playerVelocity.y < -0.01f) || groundedPlayer || dash == false) )
            {

                if (!groundedPlayer)
                {
                    playerVelocity.y += Mathf.Sqrt(jumpHeight /3 * -4.0f * gravityValue);
                    dash = true;
                }
                else playerVelocity.y += Mathf.Sqrt(jumpHeight * -4.0f * gravityValue);
            }



            playerVelocity.y += gravityValue * Time.deltaTime;


            if(playerVelocity.x != 0)
            {
                if(playerVelocity.x  > 0) playerVelocity.x += gravityValue * Time.deltaTime;
                else if (playerVelocity.x < 0) playerVelocity.x += -gravityValue  * Time.deltaTime;
                playerVelocity.x = Mathf.Clamp(playerVelocity.x, -500, 500);
                if (playerVelocity.x > -0.1 && playerVelocity.x < 0.1) playerVelocity.x = 0f;
            }

            if (playerVelocity.z != 0)
            {
                if (playerVelocity.z > 0) playerVelocity.z += gravityValue  * Time.deltaTime;
                else if (playerVelocity.z < 0) playerVelocity.z += -gravityValue  * Time.deltaTime;
                playerVelocity.z = Mathf.Clamp(playerVelocity.z, -500, 500);
                if(playerVelocity.z > -0.1 && playerVelocity.z < 0.1) playerVelocity.z = 0f;
            }

            controller.Move(playerVelocity * Time.deltaTime);
        }

    }
}