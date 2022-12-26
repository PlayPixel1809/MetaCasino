using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pocket : MonoBehaviour
{
    [SerializeField] private Text amountTxt;
    public GameObject amountGraphic;
    [SerializeField] private AudioClip depositSound;

    private Vector3 amountGraphicLocalPos;

    void Start()
    {
        if (amountGraphic != null) { amountGraphicLocalPos = amountGraphic.transform.localPosition; }
    }

    public void ResetAmount()
    {
        amountTxt.text = "0";
        gameObject.SetActive(false);
    }


    public void Reset()
    {
        amountTxt.text = "0";
        gameObject.SetActive(false);

        if (amountGraphic != null) 
        { 
            amountGraphic.gameObject.SetActive(false);
            amountGraphic.transform.localPosition = amountGraphicLocalPos;
        }
    }

    public void SetAmount(float amount)
    {
        gameObject.SetActive(true);
        amountTxt.text = amount.ToString();
        if (amountGraphic != null && GetAmount() > 0) { amountGraphic.gameObject.SetActive(true); }
    }

    public void AddAmount(float amount)
    {
        gameObject.SetActive(true);
        float finalAmount = GetAmount() + amount;
        amountTxt.text = finalAmount.ToString();
        if (amountGraphic != null && GetAmount() > 0) { amountGraphic.gameObject.SetActive(true); }
    }

    public void SubtractAmount(float amount)
    {
        float finalAmount = GetAmount() - amount;
        if (finalAmount < 0) { finalAmount = 0; }
        amountTxt.text = finalAmount.ToString();
        if (amountGraphic != null && GetAmount() <= 0) { amountGraphic.gameObject.SetActive(false); }
    }

    public float GetAmount()
    {
        return float.Parse(amountTxt.text);
    }
}
