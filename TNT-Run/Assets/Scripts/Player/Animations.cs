using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animations : MonoBehaviour
{


    [SerializeField] private Transform up;
    [SerializeField] private Transform down;
    private bool gravityEffect = false;

    [SerializeField] private GameObject Arm1;
    [SerializeField] private GameObject Arm2;
    private float xAmount = 0f;
    private bool armAnimation = false;


    private float yAmount = 0f;
    private float zAmount = 0f;
    private float yAmount2 = 0f;
    private float zAmount2 = 0f;
    void Start()
    {
        PlayGiveAnimation();
    }


    void Update()
    {
        GravityEffect();
        ArmsAnim();

    }

    public void PlayGiveAnimation()
    {
        Quaternion rot = Arm1.transform.localRotation;
         yAmount = rot.eulerAngles.y;
         zAmount = rot.eulerAngles.z;


        Arm1.transform.localRotation = Quaternion.Euler(rot.eulerAngles.x, -90, 0);
        rot = Arm2.transform.localRotation;
        yAmount2 = rot.eulerAngles.y;
        zAmount2 = rot.eulerAngles.z;

        Arm2.transform.localRotation = Quaternion.Euler(rot.eulerAngles.x, 90, 180);
     
    }


    void GivingAnim()
    {


        float v = 0;
        float t = 0.03f;

        Quaternion rot = Arm1.transform.localRotation;
        Arm1.transform.localRotation = Quaternion.Euler(rot.eulerAngles.x, Mathf.SmoothDamp(rot.eulerAngles.y, yAmount, ref v, 0.1f), Mathf.SmoothDamp(rot.eulerAngles.z, zAmount, ref v, t));

        rot = Arm2.transform.localRotation;
        Arm2.transform.localRotation = Quaternion.Euler(rot.eulerAngles.x, Mathf.SmoothDamp(rot.eulerAngles.y, yAmount2, ref v, 0.1f), Mathf.SmoothDamp(rot.eulerAngles.z, zAmount2, ref v, t));

      //  if (rot.eulerAngles.y == yAmount2) giving = false;

    }

    void ArmsAnim()
    {
        GivingAnim();

        if (armAnimation == false) xAmount += Time.deltaTime *2;
        else xAmount -= Time.deltaTime *2;

        if (xAmount > 0.8f) armAnimation = true;
        else if(xAmount <= -1.25f) armAnimation = false;

        float accur = Time.deltaTime * 50;
        if (armAnimation == true) accur = -accur;

        Quaternion rot = Arm1.transform.localRotation;
        Arm1.transform.localRotation = Quaternion.Euler(rot.eulerAngles.x + accur, rot.eulerAngles.y, rot.eulerAngles.z);
        rot = Arm2.transform.localRotation;
        Arm2.transform.localRotation = Quaternion.Euler(rot.eulerAngles.x + accur, rot.eulerAngles.y, rot.eulerAngles.z);
    }

    void GravityEffect()
    {
        if (gameObject.transform.position.y >= up.position.y) gravityEffect = true;
        else if (gameObject.transform.position.y <= down.position.y) gravityEffect = false;

        Vector3 dvelocity = Vector3.up;
        if (gravityEffect == true) dvelocity = Vector3.down;

        if (gravityEffect == true) gameObject.transform.position = Vector3.SmoothDamp(gameObject.transform.position, up.position, ref dvelocity, 0.3f);
        else gameObject.transform.position = Vector3.SmoothDamp(gameObject.transform.position, down.position, ref dvelocity, 0.3f);

    }

}


