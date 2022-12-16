using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pot : MonoBehaviour
{
    [SerializeField] private Text amountTxt;
    public GameObject amountGraphic;
    [SerializeField] private AudioClip depositSound;

    [SerializeField] private Transform depositors;
    [SerializeField] private GameObject outline;


    [HideInInspector]
    public int[] seatIndexes;


    /*public void DepositMoney(Pot takeFrom, float amount)
    {
        gameObject.SetActive(true);
        takeFrom.SubtractAmount(amount);
        AddAmount(amount);
    }



    public void TakeAmount(Pot takeFrom, float amount)
    {
        gameObject.SetActive(true);
        takeFrom.SubtractAmount(amount);
        AddAmount(amount);
    }

    public float GetPotAmount()
    {
        return float.Parse(potAmount.text);
    }

    public void SetPotAmount(float amount)
    {
        gameObject.SetActive(true);
        potAmount.text = amount.ToString();
    }*/

    public void AddAmount(float amount, int[] seatIndexes)
    {
        this.seatIndexes = seatIndexes;
        gameObject.SetActive(true);
        amountTxt.text = amount.ToString();

        for (int i = 0; i < depositors.childCount; i++)
        {
            depositors.GetChild(i).gameObject.SetActive(false);
        }

        for (int i = 0; i < seatIndexes.Length; i++)
        {
            depositors.GetChild(i).gameObject.SetActive(true);

            depositors.GetChild(i).GetComponent<Text>().text = ph.GetPlayerNickname(NetworkRoomClient.ins.seats[seatIndexes[i]].player);
        }
    }

    public void SubtractAmount(float amount)
    {
        float finalAmount = GetPotAmount() - amount;
        if (finalAmount < 0) { finalAmount = 0; }
        amountTxt.text = finalAmount.ToString();

    }

    public float GetPotAmount()
    {
        return float.Parse(amountTxt.text);
    }

    public void Reset()
    {
        amountTxt.text = "0";
        gameObject.SetActive(false);
    }
}
