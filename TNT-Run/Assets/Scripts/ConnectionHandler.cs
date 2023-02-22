using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ConnectionHandler : MonoBehaviour
{
    public static NetworkManager netManager;
    public static bool conn = false;

    void Start()
    {
        netManager = GameObject.Find("Networking").GetComponent<NetworkManager>();
        if (Menu.netstate == 0)
        {
            netManager.GetComponent<UnityTransport>().ConnectionData.Address = Menu.ConnectedIP;
            NetworkManager.Singleton.StartClient();
        }
        else if (Menu.netstate == 1)
        {
            netManager.GetComponent<UnityTransport>().ConnectionData.Address = "127.0.0.1";
            NetworkManager.Singleton.StartHost();
        }
        else
        {
           
            NetworkManager.Singleton.StartServer();
            for (int i = 0; i < 30; i++) { Debug.Log(""); }
            Debug.Log("Server started.");
            Destroy(this);
        }


        Invoke("CheckConn", 2);
        


    }


    void CheckConn()
    {
        if(conn == false)
        {
            Destroy(netManager.gameObject);
            NetworkManager.Singleton.Shutdown();
            SceneManager.LoadScene("Menu");
            return;
        }
        Destroy(this);
    }



}
