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
        amountGraphicLocalPos = amountGraphic.transform.localPosition;
    }

    public void Reset()
    {
        amountTxt.text = "0";
        amountGraphic.transform.localPosition = amountGraphicLocalPos;
        gameObject.SetActive(false);
    }

    public void AddAmount(float amount)
    {
        float finalAmount = GetAmount() + amount;
        amountTxt.text = finalAmount.ToString();
    }

    public void SubtractAmount(float amount)
    {
        float finalAmount = GetAmount() - amount;
        if (finalAmount < 0) { finalAmount = 0; }
        amountTxt.text = finalAmount.ToString();

    }

    public float GetAmount()
    {
        return float.Parse(amountTxt.text);
    }
}
