using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(BoxCollider))]
public class Pickup : NetworkBehaviour
{

    public ItemData item;
    private TextMeshPro ItemNameTMP;
    public Image item2D;

    void Start()
    {
        ItemNameTMP = gameObject.transform.GetChild(0).GetComponentInChildren<TextMeshPro>();
        item2D.sprite = item.itemSprite;
        ItemNameTMP.text = item.name;
    }

    void Update()
    {
        gameObject.transform.Rotate(0, 180 * Time.deltaTime, 0);
    }


    private void OnTriggerEnter(Collider other)
    {
        //   if (!IsServer) return;
        if (!other.gameObject.CompareTag("Player")) return;

        PlayerInventory _inventory = other.gameObject.GetComponent<PlayerInventory>();
        bool success = _inventory.AddItem(item);
        if (success == true) DeSpawnObjectsServerRPC();

    }

    [ServerRpc(RequireOwnership = false)]
    void DeSpawnObjectsServerRPC()
    {
        NetworkObject.Despawn(true);
    }

}
