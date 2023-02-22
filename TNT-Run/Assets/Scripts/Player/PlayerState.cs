using player.controller;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Playables;

public class PlayerState : NetworkBehaviour
{
    // Start is called before the first frame update

    public TextMeshProUGUI PlayerNickText;
    public Renderer MeshRenderer;
    public static Server game;

    public NetworkVariable<FixedString32Bytes> NickName = new NetworkVariable<FixedString32Bytes>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<Color> Color = new NetworkVariable<Color>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<ulong> clientID = new NetworkVariable<ulong>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<short> Score = new NetworkVariable<short>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public static bool spawning = false;

    public override void OnNetworkSpawn()
    {
        if(IsOwner) SetupPlayer();
    }

    void SetupPlayer()
    {
        NickName.Value = Menu.ConnectedName;
        Color.Value = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
        Menu.ConnectedColor = ColorUtility.ToHtmlStringRGBA(Color.Value);
        clientID.Value = NetworkManager.LocalClientId;
        SpawnPlayer();
        Invoke("UpdateAllPlayerServerRPC", 0.3f);
        Invoke("CompleteLoading", 0.5f);
    }


    

     private void CompleteLoading()
    {
        if (!IsOwner) return;
        GameObject ls = GameObject.Find("LoadingScreen");
        Destroy(ls);

    }



    private void Start()
    {
     if(IsOwner)
        {
            game = GameObject.Find("Server").GetComponent<Server>();
            PlayerData.Inventory = gameObject.GetComponent<PlayerInventory>();

            PlayerData.controller = gameObject.GetComponent<PlayerController>();
            PlayerData.state = this;
        }
            
    }



    [ServerRpc(RequireOwnership = false)]
    public void UpdateAllPlayerServerRPC()
    { 

     //   if(IsHost) UpdatePlayers();

        UpdatePlyerClientRPC();

    }


    [ClientRpc]
    public void UpdatePlyerClientRPC(ClientRpcParams clientRpcParams = default)
    {
        UpdatePlayers();
    }



    [ClientRpc]
    public void UpdateNameClientRPC(ulong playerid, string name, ClientRpcParams clientRpcParams = default)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach(GameObject Player in players)
        {
            PlayerState state = Player.GetComponent<PlayerState>();
            if (state.clientID.Value == playerid)
            {
                PlayerNickText.text = name;
            }
        }

    }


    [ClientRpc]
    public void SpawnPlayerClientRPC(ClientRpcParams clientRpcParams = default)
    {
        SpawnPlayer();
        gameObject.GetComponent<PlayerInventory>().SetUsageEffect(new UnityEngine.Color(0, 0, 0, 1));
    }





    public void SpawnPlayer()
    {
        if (!IsOwner) return;
        spawning = true;
        GameObject spawns = GameObject.Find("PlayerSpawns");
        int rand = Random.Range(0, spawns.transform.childCount);
        Vector3 spawnPos = spawns.transform.GetChild(rand).position;
        gameObject.transform.position = spawnPos;
        Invoke("FinishSpawn", 0.05f);
    }

    public void FinishSpawn()
    {

        spawning = false;
    }

    void UpdatePlayers()
    {
        GameObject[] PlayerArray = GameObject.FindGameObjectsWithTag("Player");

        bool started = game.BombGiven.Value;
        ulong bomber = game.HasBomb.Value;

        foreach (GameObject player in PlayerArray)
        {

            PlayerState state = player.GetComponent<PlayerState>();

            if ((bomber != state.clientID.Value && started == true) || started == false) state.PlayerNickText.text = state.NickName.Value.ToString();
            else state.PlayerNickText.text = "<color=red>Bomber</color>";
            state.MeshRenderer.material.color = state.Color.Value;
            player.name = "Player_" + state.NickName.Value.ToString();
        }

    }




}
