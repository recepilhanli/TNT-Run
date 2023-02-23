using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class UIRPC : NetworkBehaviour
{


    public BombTimerUI BT = null;
    public TitleUI Title = null;


    private void Start()
    {
        BT.gameObject.SetActive(false);
        Title.gameObject.SetActive(false);
    }



    [ClientRpc]
    public void SetTitleClientRPC(string title, ClientRpcParams clientRpcParams = default)
    {
        Title.SetTitle(title);
    }


    [ClientRpc]
    public void ToggleTimerClientRPC(bool enabled, ClientRpcParams clientRpcParams = default)
    {
        BT.gameObject.SetActive(enabled);
    }


}
