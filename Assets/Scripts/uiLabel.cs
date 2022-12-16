using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class uiLabel : MonoBehaviour
{
    public Text label;


    public string GetLabel()
    {
        return label.text;
    }

    public void SetLabel(string txt)
    {
        if (string.IsNullOrEmpty(txt)) { return; }

        gameObject.SetActive(true);
        label.text = txt;
    }

    

    public void Reset()
    {
        gameObject.SetActive(false);
        label.text = string.Empty;
    }

}
