using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class TitleUI : MonoBehaviour
{
    private TextMeshProUGUI TitleTMP;



    void TitleAnimation()
    {
        if (TitleTMP.gameObject.activeInHierarchy == false) return;
        if (TitleTMP.color.a > 0)
        {
            Color tc = TitleTMP.color;
            if (tc.a <= 0) TitleTMP.gameObject.SetActive(false);
            else
            {
                float alpha = tc.a - Time.deltaTime / 2;
                alpha = Mathf.Clamp(alpha, 0, 1);
                TitleTMP.color = new Color(tc.r, tc.g, tc.b, alpha);
            }

        }
    }




    public void SetTitle(string title)
    {
        TitleTMP.text = title;
        TitleTMP.color = Color.yellow;
        TitleTMP.gameObject.SetActive(true);
    }


    void Awake()
    {
        TitleTMP = gameObject.GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        TitleAnimation();
    }
}
