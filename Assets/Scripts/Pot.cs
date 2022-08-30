using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pot : MonoBehaviour
{
    [SerializeField] private Text potAmount;

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
        potAmount.text = amount.ToString();
    }

    public void AddAmount(float amount)
    {
        float finalAmount = GetPotAmount() + amount;
        potAmount.text = finalAmount.ToString();
    }

    public void SubtractAmount(float amount)
    {
        float finalAmount = GetPotAmount() - amount;
        if (finalAmount < 0) { finalAmount = 0; }
        potAmount.text = finalAmount.ToString();

    }

    
}
