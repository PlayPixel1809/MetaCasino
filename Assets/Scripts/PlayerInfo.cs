using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfo : MonoBehaviour
{
    public GameObject playerNamePanel;
    public Text playerNameTxt;

    public GameObject avatar3d;

    public Pocket balance;
    public Pocket betPlaced;
    

    public void SetInfo(Player player)
    {
        playerNamePanel.SetActive(true);
        playerNameTxt.text = ph.GetPlayerNickname(player);
        balance.AddAmount((float)ph.GetPlayerData(player, "balance"));
    }

    public void PlaceBet(float amount)
    {
        balance.SubtractAmount(amount);
        if (betPlaced != null) { betPlaced.AddAmount(amount); }
    }

   
}
