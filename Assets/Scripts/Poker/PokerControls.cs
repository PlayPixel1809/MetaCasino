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
    public Text raiseBtnAmountTxt;
    public Button raiseBtn;
    public Button raiseAmountUpBtn;
    public Button raiseAmountDownBtn;


    private float callAmount;
    private float raiseAmount;
    private float maxRaiseAmount;
    private float minRaiseAmount;

    private float playerbalance; 


    public void PlayerControlBtn(Transform btn)
    {
        gameObject.SetActive(false);

        string btnName = btn.GetChild(1).GetComponent<Text>().text;
        string moveName = GetMoveName(btnName);
        float moveAmount = GetMoveAmount(btnName);
        
        ExitGames.Client.Photon.Hashtable data = new ExitGames.Client.Photon.Hashtable();
        data.Add("moveMade", moveName);
        if (moveAmount > 0) { data.Add("moveAmount", moveAmount); }
        ServerClientBridge.NotifyServer(data);
    }

    string GetMoveName(string btnName)
    {
        if (btnName.IndexOf("ALL IN") > -1) { return "ALL IN"; }
        if (btnName.IndexOf("FOLD") > -1) { return "FOLD"; }
        if (btnName.IndexOf("CHECK") > -1) { return "CHECK"; }
        if (btnName.IndexOf("CALL") > -1) { return "CALL"; }
        if (btnName.IndexOf("BET") > -1) { return "BET"; }
        if (btnName.IndexOf("RAISE") > -1) { return "RAISE"; }
        
        return string.Empty;
    }

    float GetMoveAmount(string btnName)
    {
        if (btnName.IndexOf("CALL") > -1) { return callAmount; }
        if (btnName.IndexOf("BET") > -1 || btnName.IndexOf("RAISE") > -1)    { return raiseAmount; }
        return 0;
    }

    

    public void RaiseAmountUpBtn()
    {
        if (raiseAmount == maxRaiseAmount) { return; }
        raiseAmount = Mathf.Clamp(raiseAmount * 2, minRaiseAmount, maxRaiseAmount);
        if (raiseAmount == maxRaiseAmount) { raiseBtnTxt.text += " [ALL IN]"; }
        raiseBtnAmountTxt.text = raiseAmount.ToString();
    }

    public void RaiseAmountDownBtn()
    {
        if (raiseAmount == minRaiseAmount) { return; }
        raiseBtnTxt.text = raiseBtnTxt.text.Replace(" [ALL IN]","");
        raiseAmount = Mathf.Clamp(raiseAmount / 2, minRaiseAmount, maxRaiseAmount);
        raiseBtnAmountTxt.text = raiseAmount.ToString();
    }

   
    public void EvaluateCallBtn(float currentBet, float playerBet, float playerBalance)
    {
        //Debug.Log(currentBet);
        //Debug.Log(playerBet);
        //Debug.Log(playerBalance);

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


    public void EvaluateRaiseBtn(float currentBet, float playerBalance)
    {
        string btntext = "RAISE";
        raiseAmount = currentBet * 2;
        if (currentBet == 0) 
        { 
            btntext = "BET";
            raiseAmount = PokerClient.ins.smallBlind * 2;
        }
        
        if (callBtnTxt.text.IndexOf("ALL IN") > -1)
        {
            raiseBtnTxt.text = btntext;
            raiseBtn.interactable = false;
            raiseAmountUpBtn.interactable = false;
            raiseAmountDownBtn.interactable = false;
            return;
        }

        raiseBtnTxt.text = btntext;
        raiseBtnAmountTxt.text = raiseAmount.ToString();
        if (playerBalance <= raiseAmount)
        {
            raiseAmount = playerBalance;
            raiseBtnTxt.text += "ALL IN";
            raiseAmountUpBtn.interactable = false;
            raiseAmountDownBtn.interactable = false;
        }
        else
        {
            minRaiseAmount = raiseAmount;
            maxRaiseAmount = playerBalance;
        }
    }
}
