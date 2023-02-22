using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using TMPro;


public class Server : NetworkBehaviour
{

    [System.NonSerialized] public NetworkVariable<bool> CanPlayersMove = new NetworkVariable<bool>();
    [System.NonSerialized] public NetworkVariable<ulong> HasBomb = new NetworkVariable<ulong>();
    [System.NonSerialized] public NetworkVariable<bool> BombGiven = new NetworkVariable<bool>();
    [System.NonSerialized] public NetworkVariable<short> BombTimer = new NetworkVariable<short>();

    private float second = 1f;
    private PlayerState BomberState;

    public GameObject ExplosionParticle;

    [Header("UI Stuffs")]
    public GameObject WaitScreen;
    public GameObject TabScreen;

    //scoreboard
    public GameObject ColumnPrefab;
    public Transform DefaultCloumnTransfrom;
    private GameObject[] CreatedColoumns = new GameObject[10];

    public TextMeshProUGUI Title;
    public TextMeshProUGUI Timer;

    public SoundManager sound;

    public override void OnNetworkSpawn()
    {
        if (IsServer) BombTimer.Value = 25;

        if(IsClient) ConnectionHandler.conn = IsSpawned;

    }

    private void Start()
    {
        sound = gameObject.transform.parent.GetComponent<SoundManager>();
        
    }

    void Update()
    {

        if (Input.GetKey(KeyCode.Tab))
        {
            TabScreen.SetActive(true);
            UpdateScoreboard();
        }
        else TabScreen.SetActive(false);


        if (WaitScreen.activeInHierarchy == false && CanPlayersMove.Value == false)
        {
            WaitScreen.SetActive(true);
        }
        else if (WaitScreen.activeInHierarchy == true && CanPlayersMove.Value == true) WaitScreen.SetActive(false);

        TitleAnimation();


        if (Timer.gameObject.activeInHierarchy == true)
        {
            Timer.text = BombTimer.Value.ToString();
        }

        if (!IsServer) return;

        int players = PlayerCount();

        if (players <= 1 && CanPlayersMove.Value == true) CanPlayersMove.Value = false;
        else if (players > 1 && CanPlayersMove.Value == false)
        {
            CanPlayersMove.Value = true;
            StartCoroutine(StartTheGame());
            GameObject go = GameObject.FindGameObjectWithTag("Player");
            Chatting chat = go.GetComponent<Chatting>();
            chat.SendMessageClientRPC("<color=yellow>The game starts in 5 seconds!</color>");
        }


        if (BombGiven.Value == true)
        {

            if (second > 0) second -= Time.deltaTime;
            else
            {
                second = 1f;
                BombTimer.Value--;
            }



            if(BombTimer.Value <= 0) Explode();


        }


    }


    IEnumerator StartTheGame()
    {

        yield return new WaitForSeconds(5);

        StopCoroutine(StartTheGame());
        GiveBombRandomly();
    }


    void TitleAnimation()
    {
        if (Title.gameObject.activeInHierarchy == false) return;
        if (Title.color.a > 0)
        {
            Color tc = Title.color;
            if (tc.a <= 0) Title.gameObject.SetActive(false);
            else
            {
                float alpha = tc.a - Time.deltaTime / 2;
                alpha = Mathf.Clamp(alpha, 0, 1);
                Title.color = new Color(tc.r, tc.g, tc.b, alpha);
            }

        }
    }




    void SetTitle(string title)
    {
        Title.text = title;
        Title.color = Color.yellow;
        Title.gameObject.SetActive(true);
    }



    [ClientRpc]
    void SetTitleClientRPC(string title, ClientRpcParams clientRpcParams = default)
    {
        SetTitle(title);
    }



    [ServerRpc(RequireOwnership = false)]
    public void GiveBombServerRPC(ulong player)
    {
        GiveBomb(player);
    }

    [ClientRpc]
    void ToggleTimerClientRPC(bool enabled, ClientRpcParams clientRpcParams = default)
    {
        Timer.gameObject.SetActive(enabled);
    }


    [ClientRpc]
    void PlaySoundClientRPC(short soundid, ClientRpcParams clientRpcParams = default)
    {
        sound.PlaySound(soundid);
    }


    void GiveBomb(ulong player, GameObject go = null)
    {


        if (go == null)
        {
            GameObject[] Players = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject p in Players)
            {
                if (p.GetComponent<PlayerState>().clientID.Value == player)
                {
                    go = p;
                    break;
                }
            }
        }

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { HasBomb.Value }
            }
        };

        if (BombGiven.Value == true)
        {
            BomberState.UpdateNameClientRPC(HasBomb.Value, BomberState.NickName.Value.ToString());



            ToggleTimerClientRPC(false, clientRpcParams);


        }

        if (IsServer) BomberState = go.GetComponent<PlayerState>();


        BombTimer.Value += (short)UnityEngine.Random.Range(1,4);
        BombTimer.Value = (short)Mathf.Clamp(BombTimer.Value, 0, 25);
        HasBomb.Value = player;
        BombGiven.Value = true;


        clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { player }
            }
        };

        SetTitleClientRPC("<color=red>You Took The Bomb!</color>", clientRpcParams);
        BomberState.UpdateNameClientRPC(HasBomb.Value, "<color=red>Bomber</color>");
        ToggleTimerClientRPC(true, clientRpcParams);
    }


    void GiveBombRandomly()
    {
        GameObject[] Players = GameObject.FindGameObjectsWithTag("Player");

        if (Players.Length <= 1 && CanPlayersMove.Value == true)
        {
            CanPlayersMove.Value = false;
            return;
        }


        for (int i = 0; i < Players.Length; i++)
        {
            int rand = UnityEngine.Random.Range(0, 4);

            if (rand == 0 || i == Players.Length - 1)
            {
                GiveBomb(Players[i].GetComponent<PlayerState>().clientID.Value, Players[i]);
                Chatting chat = Players[i].GetComponent<Chatting>();
                chat.SendMessageClientRPC("<color=yellow>Bomb has been given!</color>");
                break;
            }

        }
        BombTimer.Value = 25;
        CanPlayersMove.Value = true;
        PlaySoundClientRPC(0);
        SetTitleClientRPC("<color=green>Start!</color>");
    }



    public static PlayerState FindPlayerWithClientID(ulong playerid)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            PlayerState state = player.GetComponent<PlayerState>();
            if (state.clientID.Value == playerid) return state;
        }


        return null;
    }



    int PlayerCount()
    {
        GameObject[] Players = GameObject.FindGameObjectsWithTag("Player");

        return Players.Length;
    }


    void UpdateScoreboard()
    {
        if (TabScreen == null) return;

        foreach (GameObject go in CreatedColoumns)
        {
            if (go != null) Destroy(go);
        }

        int index = 0;


        GameObject[] Players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in Players)
        {

            GameObject column = Instantiate(ColumnPrefab);
            PlayerState State = player.GetComponent<PlayerState>();

            if (State.clientID.Value == NetworkManager.LocalClientId) column.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "<color=yellow>" + State.NickName.Value.ToString() + "</color>";
            else if (State.clientID.Value == HasBomb.Value) column.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "<color=red>" + State.NickName.Value.ToString() + "</color>";
            else column.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = State.NickName.Value.ToString();
            column.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = State.Score.Value.ToString();
            column.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = "NaN";
            column.transform.SetParent(DefaultCloumnTransfrom);
            CreatedColoumns[index] = column;
            index++;

        }

    }


    void Explode()
    {
        BombGiven.Value = false;


        BomberState.Score.Value--;
        GameObject[] Players = GameObject.FindGameObjectsWithTag("Player");
        foreach(GameObject p in Players)
        {
            PlayerState state = p.GetComponent<PlayerState>();
            if (p == BomberState) continue;
            state.Score.Value++;
        }


        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { HasBomb.Value }
            }
        };

        CreateExplosionClientRPC(BomberState.gameObject.transform.position);
        ToggleTimerClientRPC(false, clientRpcParams);
        BomberState.SpawnPlayerClientRPC(clientRpcParams);
        BomberState.UpdateNameClientRPC(HasBomb.Value, BomberState.NickName.Value.ToString());
        BomberState.gameObject.GetComponent<PlayerInventory>().ResetInventoryClientRPC(clientRpcParams);
        BomberState.gameObject.GetComponent<Chatting>().SendMessageClientRPC("<color=red>Bomb exploded!</color>");
        BomberState.gameObject.GetComponent<Chatting>().SendMessageClientRPC("<color=yellow>The game starts in 5 seconds!</color>");


        StartCoroutine(StartTheGame());
        HasBomb.Value = 500;
        BombTimer.Value = 25;
    }

    [ClientRpc]
    void CreateExplosionClientRPC(Vector3 Position)
    {
        GameObject go = Instantiate(ExplosionParticle, Position,  new Quaternion(0,0,0,0));
        Destroy(go, 3.5f);
    }


}
