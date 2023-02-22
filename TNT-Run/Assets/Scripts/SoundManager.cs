using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioClip[] sounds;

    public void PlaySound(int soundid, float volume = 1f)
    {
        GameObject go = new GameObject("Sound");
        if (go == null) return;
        AudioSource  source = go.AddComponent<AudioSource>();
        source.clip = sounds[soundid];
        source.volume = volume;
        source.Play();
        Destroy(go, sounds[soundid].length);
    }


}
