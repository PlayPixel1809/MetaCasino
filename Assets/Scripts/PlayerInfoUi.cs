using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoUi : MonoBehaviour
{
    public Text username;
    public Pot balance;

    public GameObject muteBtn;
    public GameObject unmuteBtn;

    void Start()
    {
        if (PlayerPrefs.GetInt("mute", 0) == 0) { UnmuteBtn(); } else { MuteBtn(); }
    }


    public void SetUi(Player player)
    {
        gameObject.SetActive(true);
        username.text = ph.GetPlayerNickname(player);
        //balance.SetPotAmount((float)ph.GetPlayerData(player, "balance"));
    }

    public void MuteBtn()
    {
        PlayerPrefs.SetInt("mute", 1);
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
