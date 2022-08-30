using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PokerRoom : MonoBehaviour
{
    public static PokerRoom ins;
    public void Awake() { ins = this; }

    public Room room;

    void Start()
    {
        room.startProperties.Add("pokerType", "omaha");
        room.startProperties.Add("minBet", 1000);
    }

    
}
