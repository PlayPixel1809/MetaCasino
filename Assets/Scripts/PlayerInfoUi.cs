using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoUi : MonoBehaviour
{
    public Text username;
    public Pot balance;

    public void SetUi(Player player)
    {
        gameObject.SetActive(true);
        username.text = ph.GetPlayerNickname(player);
        //balance.SetPotAmount((float)ph.GetPlayerData(player, "balance"));
    }

    
}
