using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionLight : MonoBehaviour
{
    // Start is called before the first frame update
    private Light pointLight;

    void Start()
    {
        pointLight = gameObject.GetComponent<Light>();
    }

    // Update is called once per frame
    void Update()
    {
        pointLight.intensity -= Time.deltaTime * 10;
    }
}
