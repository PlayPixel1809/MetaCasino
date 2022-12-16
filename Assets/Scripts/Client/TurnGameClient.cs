using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnGameClient : MonoBehaviour
{
    public static TurnGameClient ins;
    void Awake() { ins = this; }

    public int turnTime = 5;
    public Timer lpTimer;

    [HideInInspector]
    public List<TurnGameSeat> seats = new List<TurnGameSeat>();


    [Header("Assigned During Game -")]
    public int turn;

    void Start()
    {
        for (int i = 0; i < NetworkRoomClient.ins.seats.Count; i++) { seats.Add(NetworkRoomClient.ins.seats[i].GetComponent<TurnGameSeat>()); }
    
        ServerClientBridge.ins.onServerMsgRecieved += OnServerMsgRecieved;
    }

    public void OnServerMsgRecieved(ExitGames.Client.Photon.Hashtable hashtable)
    {
        if (hashtable["turn"] != null) 
        {
            turn = (int)hashtable["turn"];
            seats[turn].StartTurn(); 
        }

        if (hashtable["moveMade"] != null)
        {
            float moveAmount = 0;
            if (hashtable["moveAmount"] != null) { moveAmount = (float)hashtable["moveAmount"]; }
            seats[(int)hashtable["moveMadeBy"]].MakeMove((string)hashtable["moveMade"], moveAmount);
        }

    }


}
