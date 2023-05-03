using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnGame : MonoBehaviour
{
    public static TurnGame ins;
    void Awake() { ins = this; }

    public int turnTime = 5;

    [Header("Assigned During Game -")]
    public int turn = -1;
    public int dealer = -1;
    public bool[] turnEligiblePlayers;
    public bool[] turnRecievedPlayers;
    public string[] playersMoves;

    public Action<int> onDealerSet;
    public Action<int> onTurnStarted;
    public Action<ExitGames.Client.Photon.Hashtable> onTurnCompleted;


    void Start()
    {
        dealer = -1;

        NetworkRoom.ins.onSeatAssigned += (seatIndex, actorNo) =>
        {
            if (actorNo > 0) { ServerClientBridge.ins.NotifyClient(actorNo, "SetTurnGameData", new ExitGames.Client.Photon.Hashtable() { { "turnTime", turnTime } }); }
        };

        NetworkGame.ins.onGameStart += () =>
        {
            turnEligiblePlayers = new bool[NetworkRoom.ins.seats.Length];
            for (int i = 0; i < NetworkRoom.ins.seats.Length; i++) { if (NetworkRoom.ins.seats[i] != 0) { turnEligiblePlayers[i] = true; } }

            turnRecievedPlayers = new bool[NetworkRoom.ins.seats.Length];
            dealer = GetNextTurnIndex(dealer);

            playersMoves = new string[NetworkRoom.ins.seats.Length];
            for (int i = 0; i < NetworkRoom.ins.seats.Length; i++) { playersMoves[i] = "Null"; }

            NetworkRoom.ins.SyncData(new ExitGames.Client.Photon.Hashtable() { { "turnEligiblePlayers", turnEligiblePlayers }, { "turnRecievedPlayers", turnRecievedPlayers }, { "dealer", dealer }, { "playersMoves", playersMoves } });
            onDealerSet?.Invoke(dealer);
        };

        ServerClientBridge.ins.onClientMsgRecieved += OnClientMsgRecieved;

        ServerClientBridge.ins.onMadeMasterClient += () =>
        {
            if (ph.GetRoomData("turn") != null)                 { turn = (int)ph.GetRoomData("turn"); }
            if (ph.GetRoomData("dealer") != null)               { dealer = (int)ph.GetRoomData("dealer"); }
            if (ph.GetRoomData("turnEligiblePlayers") != null)  { turnEligiblePlayers = (bool[])ph.GetRoomData("turnEligiblePlayers"); }
            if (ph.GetRoomData("turnRecievedPlayers") != null)  { turnRecievedPlayers = (bool[])ph.GetRoomData("turnRecievedPlayers"); }
            if (ph.GetRoomData("playersMoves") != null)         { playersMoves = (string[])ph.GetRoomData("playersMoves"); }
        };

        NetworkGame.ins.onSeatVaccated += (seatIndex, actorNo) =>
        {
            turnEligiblePlayers[seatIndex] = false;
            NetworkRoom.ins.SyncData("turnEligiblePlayers", turnEligiblePlayers);
        };

        NetworkGame.ins.onGameComplete += () =>
        {
            NetworkRoom.ins.SyncData(new ExitGames.Client.Photon.Hashtable()
            {
                { "playersMoves", null }
            });
        };
    }

    
    void OnClientMsgRecieved(string completedEvId, ExitGames.Client.Photon.Hashtable data)
    {
        if (completedEvId.IndexOf("ExecuteTurn") > -1)
        {
            onTurnCompleted?.Invoke(data);
        }

    }

    public void StartFirstTurn()
    {
        StartTurn(GetNextTurnIndex(dealer));
    }


    public void StartTurn(int turnIndex)
    {
        turn = turnIndex;
        turnRecievedPlayers[turn] = true;
        NetworkRoom.ins.SyncData(new ExitGames.Client.Photon.Hashtable() { { "turn", turn }, { "turnRecievedPlayers", turnRecievedPlayers } });
        ServerClientBridge.ins.HireClients("ExecuteTurn" + turn, "turnIndex", turn);
        onTurnStarted?.Invoke(turn);
    }
    
    public int GetNextTurnIndex(int currentTurnIndex)
    {
        int iterations = turnEligiblePlayers.Length - 1;
        int nextTurnIndex = currentTurnIndex;
        while (iterations > 0)
        {
            iterations -= 1;
            nextTurnIndex += 1;
            if (nextTurnIndex == NetworkRoom.ins.seats.Length) { nextTurnIndex = 0; }
            if (turnEligiblePlayers[nextTurnIndex]) { return nextTurnIndex; }
        }
        return -1;
    }

    public int GetTurnEligiblePlayersCount()
    {
        int count = 0;
        for (int i = 0; i < turnEligiblePlayers.Length; i++)
        {
            if (turnEligiblePlayers[i]) { count += 1; }
        }
        return count;
    }
}
