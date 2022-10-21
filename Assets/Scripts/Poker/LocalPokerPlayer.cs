using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalPokerPlayer : MonoBehaviour
{
    public static LocalPokerPlayer ins;
    void Awake() { ins = this; }

    public GameObject lpControls;
    public PlayerInfoUi lpInfoUi;
    public Timer lpTimer;


    public void SetInfo()
    {
        
    }
}
