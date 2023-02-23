using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class Chatting : NetworkBehaviour
{

    private static string ChatString = " ";

    private static TextMeshProUGUI chat;
    private static TMP_InputField chatInput;
    public KeyCode TextingKey = KeyCode.Return;


    public static bool Texting = false;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) this.enabled = false;
    }

    private void Start()
    {
        if (!IsOwner) return;

        chat = GameObject.Find("Chat").GetComponent<TextMeshProUGUI>();
        chatInput = GameObject.Find("ChatInput").GetComponent<TMP_InputField>();
        chatInput.interactable = false;


        if (IsHost) SendtMessage("Server is started.");
        else if (IsClient) SendtMessage("Connected to server.");
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;
        if (Input.GetKeyDown(TextingKey))
        {

            if (Texting == false)
            {
                chatInput.interactable = true;
                chatInput.ActivateInputField();
                Texting = true;
                chat.color = new Color(1, 1, 1, 0.5f);
            }
            else
            {
                chat.color = Color.white;
                Texting = false;
                chatInput.DeactivateInputField();
                chatInput.interactable = false;
                string msg = chatInput.text;
                chatInput.text = null;
                if (msg.Length == 0) return;
                msg = msg.Insert(0, "<color=#" + Menu.ConnectedColor + ">" + Menu.ConnectedName + ":</color> ");
                SendMessageServerRPC(msg);
            }



        }

        else if (Input.GetKeyDown(KeyCode.Escape) && Texting == true)
        {
            chat.color = Color.white;
            Texting = false;
            chatInput.DeactivateInputField();
            chatInput.interactable = false;
        }


    }



    public void SendtMessage(string msg)
    {
        if (msg == null) return;
        msg += System.Environment.NewLine;
        ChatString = ChatString.Insert(0, msg);
        if(ChatString.Length > 256) ChatString = ChatString.Remove(ChatString.Length - (ChatString.Length -256));
        chat.text = ChatString;

    }

    [ServerRpc(RequireOwnership = false)]
    public void SendMessageServerRPC(string msg)
    {
        // if (IsHost) SendtMessage(msg);

        SendMessageClientRPC(msg);

        msg = msg.Insert(0, "[CHAT] ");
        Debug.Log(msg);

    }


    [ClientRpc]
    public void SendMessageClientRPC(string msg, ClientRpcParams clientRpcParams = default)
    {
        SendtMessage(msg);
    }




}
