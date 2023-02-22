using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillBoard : MonoBehaviour
{


    public GameObject LookAt;

    [Header("Optional")]
    public string ObjectName = null;


    void Start()
    {
        if(ObjectName != null) LookAt = GameObject.Find(ObjectName);
    }


  
    void Update()
    {

        float y = LookAt.transform.localRotation.eulerAngles.y;

        gameObject.transform.rotation = Quaternion.Euler(0f, y, 0f);



    }
}
