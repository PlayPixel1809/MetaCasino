using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardGameClient : MonoBehaviour
{
    public static CardGameClient ins;
    void Awake() { ins = this; }

    [HideInInspector]
    public List<CardGameSeat> seats;


    void Start()
    {
        for (int i = 0; i < NetworkGameClient.ins.seats.Count; i++) { seats.Add(NetworkGameClient.ins.seats[i].GetComponent<CardGameSeat>()); }


        ServerClientBridge.ins.onServerMsgRecieved += OnMsgRecieved;
    }

    public void OnMsgRecieved(ExitGames.Client.Photon.Hashtable hashtable)
    {
        if (hashtable.ContainsKey("seats"))
        {
           
        }

        
    }
}
