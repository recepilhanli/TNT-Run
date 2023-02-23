using player.controller;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;


    public class PlayerInventory : NetworkBehaviour
    {



        private Chatting chat;
        public ItemData[] items = new ItemData[3];
        public Image[] InventoryImages;
        private PlayerState state = null;
        private Image InventoryEffect;

        [Header("Prefabs")]
        public GameObject BlockerPrefab;

        enum iteminfo
        {
            Teleporter,
            Blocker,
            Energy,
            Magnet
        };

        public override void OnNetworkSpawn()
        {
            if (!IsOwner) this.enabled = false;
        }


        void Start()
        {
            chat = gameObject.GetComponent<Chatting>();


            for (int i = 0; i < 3; i++)
            {
                InventoryImages[i] = GameObject.Find("invBox (" + i + ")").GetComponent<Image>();
            }

            state = gameObject.GetComponent<PlayerState>();
            InventoryEffect = GameObject.Find("InventoryEffect").GetComponent<Image>();
        }

        void Update()
        {

            int key = -1;
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) key = 0;
            else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) key = 1;
            else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) key = 2;

            if (key != -1) OnPlayerUseItem(key);

            UsageEffect();
        }




        void OnPlayerUseItem(int slot)
        {
            if (items[slot] == null) return;

            PlayerState.game.sound.PlaySound(1, 0.2f);
            int id = items[slot].itemID;
            items[slot] = null;
            InventoryImages[slot].sprite = null;




            if (id == (int)iteminfo.Teleporter)
            {
                state.SpawnPlayer();
                SetUsageEffect(Color.yellow);
            }



            else if (id == (int)iteminfo.Blocker)
            {

                CreateBlockerServerRPC(state.clientID.Value);
                SetUsageEffect(Color.cyan);
            }


            else if (id == (int)iteminfo.Energy)
            {
                gameObject.GetComponent<PlayerController>().Energy = 6f;
                SetUsageEffect(Color.green);
            }


            else if (id == (int)iteminfo.Magnet)
            {
                MagnetServerRPC(state.clientID.Value);
                SetUsageEffect(Color.magenta);
            }
        }


        [ServerRpc(RequireOwnership = false)]
        void CreateBlockerServerRPC(ulong player)
        {
            CreateBlockerClientRPC(player);
        }




        [ServerRpc(RequireOwnership = false)]
        void MagnetServerRPC(ulong player)
        {


            Vector3 magnet = Vector3.zero;
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");






            foreach (GameObject p in players)
            {
                PlayerState ps = p.GetComponent<PlayerState>();
                if (ps.clientID.Value == player)
                {
                    magnet = p.transform.position;
                    break;
                }
            }



            foreach (GameObject p in players)
            {
                PlayerState ps = p.GetComponent<PlayerState>();
                if (ps.clientID.Value == player) continue;
                if (Vector3.Distance(magnet, p.transform.position) > 12) continue;
                ClientRpcParams clientRpcParams = new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new ulong[] { ps.clientID.Value }
                    }
                };

                MagnetClientRPC(magnet, clientRpcParams);
            }


        }

        [ClientRpc]
        void MagnetClientRPC(Vector3 magnet, ClientRpcParams clientRpcParams = default)
        {




            PlayerController pc = PlayerData.controller;

            float dist = Vector3.Distance(pc.gameObject.transform.position, magnet);
            if (dist > 12) return;
            magnet -= pc.gameObject.transform.position;

            pc.playerVelocity += magnet * 10 / dist;

            PlayerData.Inventory.SetUsageEffect(Color.black);





        }





        [ClientRpc]
        void CreateBlockerClientRPC(ulong player)
        {



            GameObject[] Players = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject p in Players)
            {
                if (player == p.GetComponent<PlayerState>().clientID.Value)
                {
                    Transform t = p.transform;
                    GameObject go = Instantiate(BlockerPrefab, t.position - t.forward * 1.25f, t.rotation);
                    Destroy(go, 5f);
                    break;
                }
            }


        }


        public bool AddItem(ItemData item)
        {

            if (item == null) return false;
            int slot = FindFreeSpace();
            if (slot == -1)
            {
                chat.SendtMessage("<color=red>There is no space in your inventory!</color>");
                return false;
            }

            PlayerState.game.sound.PlaySound(2, 0.05f);
            items[slot] = item;


            InventoryImages[slot].sprite = item.itemSprite;

            return true; //making sure the item added to the inventory sucessfuly
        }


        int FindFreeSpace()
        {
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] == null)
                {
                    return i;
                }
            }
            return -1;
        }


        [ClientRpc]
        public void ResetInventoryClientRPC(ClientRpcParams clientRpcParams = default)
        {

            for (int i = 0; i < 3; i++)
            {
                items[i] = null;
                InventoryImages[i].sprite = null;
            }

        }

        public void SetUsageEffect(Color c)
        {
            InventoryEffect.color = c;
            InventoryEffect.gameObject.SetActive(true);
        }

        void UsageEffect()
        {
            if (InventoryEffect.gameObject.activeInHierarchy == false) return;
            Color uc = InventoryEffect.color;

            float alpha = uc.a;
            alpha = Mathf.Clamp(alpha, 0.0f, 0.5f);

            InventoryEffect.color = new Color(uc.r, uc.g, uc.b, alpha - Time.deltaTime);
        }


    }
