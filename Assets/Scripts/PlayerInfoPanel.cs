using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoPanel : MonoBehaviour
{
    public static PlayerInfoPanel ins;
    void Awake() { ins = this; }

    public Text playerName;
    public Text playerBalance;

    public GameObject muteBtn;
    public GameObject unmuteBtn;

    void Start()
    {
        if (PlayerPrefs.GetInt("mute", 0) == 0) { UnmuteBtn(); } else { MuteBtn(); }
        
        if (User.localUser != null)
        {
            SetInfo();
        }
        else
        {
            User.onCreateLocalUser += SetInfo;
        }
    }


    public void SetInfo()
    {
        playerBalance.text = User.localUser.balance.ToString();
        playerName.text = User.localUser.username.ToUpper(); 
    }

    public void MuteBtn()
    {
        PlayerPrefs.SetInt("mute",1);
        AudioListener.volume = 0;
        muteBtn.SetActive(false);
        unmuteBtn.SetActive(true);
    }

    public void UnmuteBtn()
    {
        PlayerPrefs.SetInt("mute", 0);
        AudioListener.volume = 1;
        muteBtn.SetActive(true);
        unmuteBtn.SetActive(false);
    }
}
