using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PokerControls : PlayerControls
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
    
    public void PlayerControlBtn(Transform btn)
    {
        StopCoroutine("DisableControlsAutomatically");
        DisableAllControls();

        Dictionary<byte, object> data = new Dictionary<byte, object>();
        string btnName = btn.GetChild(1).GetComponent<Text>().text;
        if (btnName.IndexOf("FOLD")   > -1) { data[1] = 0; }
        if (btnName.IndexOf("CHECK")  > -1) { data[1] = 1; }
        if (btnName.IndexOf("CALL")   > -1) { data[1] = 2; data[2] = callAmount; }
        if (btnName.IndexOf("BET")    > -1) { data[1] = 3; data[2] = raiseAmount; }
        if (btnName.IndexOf("RAISE")  > -1) { data[1] = 4; data[2] = raiseAmount; }
        if (btnName.IndexOf("All IN") > -1) { data[1] = 7; data[2] = raiseAmount; }

        //PhotonPlayers.ins.MakeMove(data);
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

    public void EvaluateControls(float controlsEnableTime, float currentBet, float currentPot, float playerPot, float playerBalance)
    {
        EnableControls(1, controlsEnableTime);
        EvaluateCallBtn(currentPot, playerPot, playerBalance);
        EvaluateRaiseBtn(currentBet, playerBalance);
    }


    public void EvaluateCallBtn(float currentPot, float playerPot, float playerBalance)
    {
        if (currentPot == 0 || playerPot == currentPot) 
        {
            callAmount = 0;
            callBtnTxt.text = "CHECK"; 
        }
        else
        {
            callAmount = currentPot - playerPot;
            if (playerBalance <= callAmount)
            {
                callAmount = playerBalance;
                callBtnTxt.text = "CALL [ALL IN]\n" + callAmount.ToString();
            }
            else
            {callBtnTxt.text = "CALL\n" + callAmount.ToString();}
        }
    }


    public void EvaluateRaiseBtn(float currentBet, float playerBalance)
    {
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
