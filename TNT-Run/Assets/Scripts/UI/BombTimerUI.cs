using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class BombTimerUI : MonoBehaviour
{

    public TextMeshProUGUI Timer;
    public Server game;



    void Update()
    {

        if (Timer.gameObject.activeInHierarchy == true)
        {
            Timer.text = game.BombTimer.Value.ToString();
        }

    }
}
