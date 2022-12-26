using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pot : MonoBehaviour
{
    public Text amountTxt;
    public GameObject amountGraphic;
    [HideInInspector]
    public Transform depositors;


    [SerializeField] private AudioClip depositSound;
    [SerializeField] private GameObject outline;

    [HideInInspector]
    public int[] seatIndexes;


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

        if (amountGraphic != null && GetPotAmount() > 0) { amountGraphic.gameObject.SetActive(true); }
    }

    public void SubtractAmount(float amount)
    {
        float finalAmount = GetPotAmount() - amount;
        if (finalAmount < 0) { finalAmount = 0; }
        amountTxt.text = finalAmount.ToString();
        if (amountGraphic != null && GetPotAmount() <= 0) { amountGraphic.gameObject.SetActive(false); }
    }

    public float GetPotAmount()
    {
        return float.Parse(amountTxt.text);
    }

    public void Reset()
    {
        amountTxt.text = "0";
        gameObject.SetActive(false);
        if (amountGraphic != null) { amountGraphic.gameObject.SetActive(false); }
    }

    public void Highlight()
    {
        outline.SetActive(true);
    }

    public void RemoveHighlight()
    {
        outline.SetActive(false);
    }
}
