using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class PickupSpawner : NetworkBehaviour
{


    public override void OnNetworkSpawn()
    {
        if (!IsOwner) this.enabled = false;
    }

    public float Cooldown;
    public GameObject[] Pickups;
    private GameObject Created;

    private float _cooldown = 3f;

    private void FixedUpdate()
    {
        if (!IsServer) return;

        if(Created == null)
        {
            _cooldown -= Time.fixedDeltaTime;
            if(_cooldown <= 0)
            {
                int rand = Random.Range(0, Pickups.Length);
                Created = Instantiate(Pickups[rand], gameObject.transform);
                Created.GetComponent<NetworkObject>().Spawn();

                _cooldown = Cooldown;
            }


        }


    }



}
