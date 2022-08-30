using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PokerControls : MonoBehaviour
{
    public static PokerControls ins;
    void Awake() { ins = this; }

    public Text callBtnTxt;
    public Text raiseBtnTxt;
    public Button raiseBtn;
    public Button raiseAmountUpBtn;
    public Button raiseAmountDownBtn;


    private float callAmount;
    private float raiseAmount;
    private float maxRaiseAmount;
    private float minRaiseAmount;

    void OnEnable()
    {
        EvaluateCallBtn();
        EvaluateRaiseBtn();
    }

    public void PlayerControlBtn(Transform btn)
    {
        gameObject.SetActive(false);
        
        string moveName = string.Empty;
        float moveAmount = 0;

        string btnName = btn.GetChild(1).GetComponent<Text>().text;
        if (btnName.IndexOf("FOLD")   > -1) { moveName = "FOLD";  }
        if (btnName.IndexOf("CHECK")  > -1) { moveName = "CHECK"; }
        if (btnName.IndexOf("CALL")   > -1) { moveName = "CALL";   moveAmount = callAmount;  }
        if (btnName.IndexOf("BET")    > -1) { moveName = "BET";    moveAmount = raiseAmount; }
        if (btnName.IndexOf("RAISE")  > -1) { moveName = "RAISE";  moveAmount = raiseAmount; }
        if (btnName.IndexOf("All IN") > -1) { moveName = "All IN"; moveAmount = raiseAmount; }


        ExitGames.Client.Photon.Hashtable data = new ExitGames.Client.Photon.Hashtable();
        data.Add("moveMade", moveName);
        if (moveAmount > 0) { data.Add("moveAmount", moveAmount); }
        ph.SetRoomData(data);
    }

    public void RaiseAmountUpBtn()
    {
        string raiseBtnTxt = this.raiseBtnTxt.text.Replace(raiseAmount.ToString(),"");
        raiseAmount = Mathf.Clamp(raiseAmount + 2000, minRaiseAmount, maxRaiseAmount);
        this.raiseBtnTxt.text = raiseBtnTxt + raiseAmount.ToString();
    }

    public void RaiseAmountDownBtn()
    {
        string raiseBtnTxt = this.raiseBtnTxt.text.Replace(raiseAmount.ToString(), "");
        raiseAmount = Mathf.Clamp(raiseAmount - 2000, minRaiseAmount, maxRaiseAmount);
        this.raiseBtnTxt.text = raiseBtnTxt + raiseAmount.ToString();
    }

   
    public void EvaluateCallBtn()
    {
        float currentBet = (float)ph.GetRoomData("currentBet");
        float playerBet = TurnGame.ins.playersBets[(int)ph.GetRoomData("turn")];
        float playerBalance = (float)ph.GetLocalPlayerData("balance");

        if (currentBet == 0 || playerBet == currentBet) 
        {
            callAmount = 0;
            callBtnTxt.text = "CHECK"; 
        }
        else
        {
            callAmount = currentBet - playerBet;
            if (playerBalance <= callAmount)
            {
                callAmount = playerBalance;
                callBtnTxt.text = "CALL [ALL IN]\n" + callAmount.ToString();
            }
            else
            {callBtnTxt.text = "CALL\n" + callAmount.ToString();}
        }
    }


    public void EvaluateRaiseBtn()
    {
        float currentBet = (float)ph.GetRoomData("currentBet");
        float playerBalance = (float)ph.GetLocalPlayerData("balance");

        string btntext = "RAISE";
        if (currentBet == 0) { btntext = "BET"; }
        
        if (callBtnTxt.text.IndexOf("ALL IN") > -1)
        {
            raiseBtnTxt.text = btntext;
            raiseBtn.interactable = false;
            raiseAmountUpBtn.interactable = false;
            raiseAmountDownBtn.interactable = false;
            return;
        }

        raiseAmount = currentBet * 2;
        if (playerBalance <= raiseAmount)
        {
            raiseAmount = playerBalance;
            raiseBtnTxt.text = btntext + " [ALL IN]\n" + raiseAmount.ToString();
            raiseAmountUpBtn.interactable = false;
            raiseAmountDownBtn.interactable = false;
        }
        else
        {
            raiseBtnTxt.text = btntext + "\n" + raiseAmount.ToString();
            minRaiseAmount = raiseAmount;
            maxRaiseAmount = playerBalance;
        }
    }
}
