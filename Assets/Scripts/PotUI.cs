using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PotUI : MonoBehaviour
{
    public Text potAmount;
    public Transform contributors;
    public GameObject outline;

    [HideInInspector]
    public int[] seatIndexes;

    public void SetPot(float amount, int[] seatIndexes)
    {
        this.seatIndexes = seatIndexes;
        gameObject.SetActive(true);
        potAmount.text = amount.ToString();

        for (int i = 0; i < contributors.childCount; i++)
        {
            contributors.GetChild(i).gameObject.SetActive(false);
        }

        for (int i = 0; i < seatIndexes.Length; i++)
        {
            contributors.GetChild(i).gameObject.SetActive(true);

            contributors.GetChild(i).GetComponent<Text>().text = ph.GetPlayerNickname(NetworkRoomClient.ins.seats[seatIndexes[i]].player);
        }
    }

    public void Highlight()
    {
        outline.SetActive(true);
    }

    public void RemoveHighlight()
    {
        outline.SetActive(false);
    }

    public float GetPotAmount()
    {
        return float.Parse(potAmount.text);
    }
}
