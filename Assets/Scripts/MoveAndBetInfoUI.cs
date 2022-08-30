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
        bet.TakeAmount(balance, amount);
    }

}
