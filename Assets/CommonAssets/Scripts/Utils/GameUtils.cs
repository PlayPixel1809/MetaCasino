using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUtils : MonoBehaviour
{
    public static GameUtils ins;
    void Awake() 
    { 
        ins = this;
        if (fadeSprite != null) { AnimUtils.FadeUi(fadeSprite.transform, 1, 0, 1); }

        if (!PlayerPrefs.HasKey("effectsVol")) { PlayerPrefs.SetFloat("effectsVol", .5f); }

        //if (MusicUtils.ins == null && instantiateMusicPlayer) { Instantiate(Resources.Load("MusicUtils")); }

        string notification = "NoticeUtilsTall";
        if (Screen.width > Screen.height) { notification = "NoticeUtilsWide"; }
        if (NoticeUtils.ins == null && instantiateNotifications) { Instantiate(Resources.Load(notification)); }
    }

    public AudioSource audioSource;
    public AudioClip btnSoundDefault;

    public Image fadeSprite;

    public bool exitOnEscape;

    public bool instantiateAudioUtils = true;
    public bool instantiateNotifications = true;



    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape) && exitOnEscape) { Application.Quit(); }
    }

    public void PlaySound(AudioClip sound)
    {
        if (Time.timeSinceLevelLoad > 1) { audioSource.PlayOneShot(sound, PlayerPrefs.GetFloat("effectsVol")); }
    }

    public void PlayBtnSound() { PlaySound(btnSoundDefault); }
}
