using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoPanel : MonoBehaviour
{
    public static PlayerInfoPanel ins;
    void Awake() { ins = this; }

    public Text playerName;
    public Text playerBalance;

    void Start()
    {
        if (User.localUser != null)
        {
            SetInfo();
        }
        else
        {
            User.onCreateLocalUser += SetInfo;
        }
    }


    public void SetInfo()
    {
        playerBalance.text = User.localUser.balance.ToString("N0", CultureInfo.InvariantCulture);
        playerName.text = User.localUser.username.ToUpper(); 
    }
}
