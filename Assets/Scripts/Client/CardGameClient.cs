using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardGameClient : MonoBehaviour
{
    public static CardGameClient ins;
    void Awake() { ins = this; }

    
    public CardsHolder lpCards;

    [Header("Assigned During Game -")]
    public List<CardGameSeat> seats;
    public string[] playersCards;

    void Start()
    {
        StartCoroutine("AssignSeats");
        ServerClientBridge.ins.onServerMsgRecieved += OnMsgRecieved;
    }

    IEnumerator AssignSeats()
    {
        yield return new WaitForEndOfFrame();
        seats = new List<CardGameSeat>();
        for (int i = 0; i < NetworkRoomClient.ins.seats.Count; i++) { seats.Add(NetworkRoomClient.ins.seats[i].GetComponent<CardGameSeat>()); }
    }

    public void OnMsgRecieved(ExitGames.Client.Photon.Hashtable hashtable)
    {
        
        
        if (hashtable["playersCards"] != null)
        {
            playersCards = (string[])hashtable["playersCards"];

            for (int i = 0; i < seats.Count; i++)
            {
                if (playersCards[i] == "Null") { continue; }
                string[] playerCards = playersCards[i].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                seats[i].StartCoroutine("CreateCards", playerCards);
            }
        }

        
    }
}
