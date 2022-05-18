using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUtils : MonoBehaviour
{
    public static GameUtils ins;
    void Awake() { ins = this; }

    public AudioSource audioSource;
    public AudioClip btnSoundDefault;

    public bool exitOnEscape;

    public bool instantiateMusicPlayer = true;
    public bool instantiateNotifications = true;


    void Start()
    {
        if (!PlayerPrefs.HasKey("effectsVol")) { PlayerPrefs.SetFloat("effectsVol", .5f); }
        
        //if (MusicUtils.ins == null && instantiateMusicPlayer) { Instantiate(Resources.Load("MusicUtils")); }

        string notification = "NoticeUtilsTall";
        if (Screen.width > Screen.height) { notification = "NoticeUtilsWide"; }
        if (NoticeUtils.ins == null && instantiateNotifications) { Instantiate(Resources.Load(notification)); }
    }

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
