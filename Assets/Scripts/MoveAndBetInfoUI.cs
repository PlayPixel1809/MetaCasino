using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoveAndBetInfoUI : MonoBehaviour
{
    public Pot bet;
    public GameObject moveInfoPanel;
    public Text moveInfo;

    public void MakeMove(string moveName)
    {
        moveInfoPanel.SetActive(true);
        moveInfo.text = moveName;
    }

    public void MakeBet(Pot balance, float amount)
    {
        //bet.TakeAmount(balance, amount);
    }

    public void Reset()
    {
        //bet.SetPotAmount(0);
        moveInfoPanel.SetActive(false);
        bet.gameObject.SetActive(false);
    }

    public void ResetBetInfo()
    {
        //bet.SetPotAmount(0);
        bet.gameObject.SetActive(false);
    }

    public void ResetMoveInfo()
    {
        moveInfoPanel.SetActive(false);
    }
}
