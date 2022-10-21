using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnGameClient : MonoBehaviour
{
    public static TurnGameClient ins;
    void Awake() { ins = this; }

    [HideInInspector]
    public List<TurnGameSeat> seats = new List<TurnGameSeat>();

    public int turnTime = 5;

    public Timer lpTimer;


    void Start()
    {
        for (int i = 0; i < NetworkGameClient.ins.seats.Count; i++) { seats.Add(NetworkGameClient.ins.seats[i].GetComponent<TurnGameSeat>()); }
    
        ServerClientBridge.ins.onServerMsgRecieved += onServerMsgRecieved;
    }

    public void onServerMsgRecieved(ExitGames.Client.Photon.Hashtable hashtable)
    {
        if (hashtable.ContainsKey("turn"))
        {
            seats[(int)hashtable["turn"]].StartTurn();
        }
    }
}
