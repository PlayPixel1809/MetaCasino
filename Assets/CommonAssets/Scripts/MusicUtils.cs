using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicUtils : MonoBehaviour
{
    public static MusicUtils ins;
    void Awake() { ins = this; }

    public AudioSource audioSource;

    public Coroutine animateVolume;


    void Start()
    {
        DontDestroyOnLoad(this);
        if (!PlayerPrefs.HasKey("musicVol")) { PlayerPrefs.SetFloat("musicVol", audioSource.volume); }
        AnimateVolume(0, PlayerPrefs.GetFloat("musicVol"), 10);
    }

    
    public void SetVolume(float vol)
    {
        if (animateVolume != null) 
        { 
            StopCoroutine(animateVolume);
            animateVolume = null;
        }
        audioSource.volume = vol;
    }

    public void AnimateVolume(float from, float to, float time = 1)
    {
        if (animateVolume != null)
        {
            StopCoroutine(animateVolume);
            animateVolume = null;
        }
        animateVolume = StartCoroutine(AnimateVolumeCoroutine(from, to, time));
    }

    IEnumerator AnimateVolumeCoroutine(float from, float to, float time)
    {
        float val = 0;
        while (val < 1)
        {
            val += Time.deltaTime / time;
            audioSource.volume = Mathf.Lerp(from, to, val);
            yield return null;
        }
    }
}
