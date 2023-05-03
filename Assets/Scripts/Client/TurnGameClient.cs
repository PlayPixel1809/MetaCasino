using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TurnGameClient : MonoBehaviour
{
    public static TurnGameClient ins;
    void Awake() { ins = this; }

    public Transform camLookAtPoint;
    public Timer lpTimer;

    public Action onRoomJoined;
    public Action onSeatAssigned;
    public Action<ExitGames.Client.Photon.Hashtable> onTurnMissed;

    [Header("Assigned During Game -")]
    public List<TurnGameSeat> seats = new List<TurnGameSeat>();
    public int turnTime;
    public int turn;


    void Start()
    {
        StartCoroutine("AssignSeats");

        NetworkGameClient.ins.onRoomJoined += () =>
        {
            onRoomJoined?.Invoke();
        };


        NetworkGameClient.ins.onSeatAssigned += () =>
        {
            if (ph.GetRoomData("playersMoves") != null) 
            {
                string[] playersMoves = (string[])ph.GetRoomData("playersMoves");
                for (int i = 0; i < playersMoves.Length; i++) { if (playersMoves[i] != "Null") { seats[i].MakeMove(playersMoves[i]); }}
            }
            onSeatAssigned?.Invoke();
        };


        ServerClientBridge.ins.onServerMsgRecieved += OnServerMsgRecieved;

        NetworkGameClient.ins.onGameComplete += () =>
        {
            for (int i = 0; i < seats.Count; i++) { seats[i].ResetMoveMade(); }
        };
    }

    IEnumerator AssignSeats()
    {
        yield return new WaitForEndOfFrame();
        seats = new List<TurnGameSeat>();
        for (int i = 0; i < NetworkRoomClient.ins.seats.Count; i++) { seats.Add(NetworkRoomClient.ins.seats[i].GetComponent<TurnGameSeat>()); }
    }

    public void OnServerMsgRecieved(string evId, ExitGames.Client.Photon.Hashtable data)
    {
        //Debug.Log(evId);
        if (evId == "SetTurnGameData") { turnTime = (int)data["turnTime"]; }

        if (evId.IndexOf("ExecuteTurn") > -1)  { StartCoroutine("ExecuteTurn", data); }

        if (evId == "MakeMove") { StartCoroutine("MakeMove", data); }
    }


    IEnumerator ExecuteTurn(ExitGames.Client.Photon.Hashtable data)
    {
        Debug.Log("TurnRecieved");
        turn = (int)data["turnIndex"];
        seats[turn].ExecuteTurn();
        yield return new WaitForSeconds(turnTime);
        onTurnMissed?.Invoke(data);
        
    }

    IEnumerator MakeMove(ExitGames.Client.Photon.Hashtable data)
    {
        StopCoroutine("ExecuteTurn");
        seats[turn].StopTurn();

        float moveAmount = 0;
        if (data["moveAmount"] != null) { moveAmount = (float)data["moveAmount"]; }
        seats[(int)data["moveMadeBy"]].MakeMove((string)data["moveMade"], moveAmount);

        yield return new WaitForSeconds(1);
        ServerClientBridge.ins.NotifyServerIfMasterClient("MakeMove");
    }
}
