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

    public Action<int> onDealerSet;
    public Action<int> onTurn;

    private Coroutine waitForTurnCoroutine;

    void Start()
    {
        ServerClientBridge.ins.onClientMsgRecieved += OnClientMsgRecieved;

        dealer = -1;

        NetworkGame.ins.onGameStart += () =>
        {
            turnEligiblePlayers = new bool[NetworkRoom.ins.seats.Length];
            for (int i = 0; i < NetworkRoom.ins.seats.Length; i++) { if (NetworkRoom.ins.seats[i] != 0) { turnEligiblePlayers[i] = true; } }

            turnRecievedPlayers = new bool[NetworkRoom.ins.seats.Length];
            dealer = GetNextTurnIndex(dealer);

            NetworkGame.ins.SyncData(new ExitGames.Client.Photon.Hashtable() { { "turnEligiblePlayers", turnEligiblePlayers }, { "turnRecievedPlayers", turnRecievedPlayers }, { "dealer", dealer } });
            onDealerSet?.Invoke(dealer);
        };
    }

    
    void OnClientMsgRecieved(int sender, ExitGames.Client.Photon.Hashtable hashtable)
    {
        if (hashtable.ContainsKey("moveMade"))
        {
            StopCoroutine(waitForTurnCoroutine);
        }

    }

    public void StartFirstTurn(Action onPlayerDidntRespond)
    {
        StartTurn(GetNextTurnIndex(dealer), onPlayerDidntRespond);
    }


    public void StartTurn(int turnIndex, Action onPlayerDidntRespond)
    {
        turn = turnIndex;
        turnRecievedPlayers[turn] = true;
        NetworkGame.ins.SyncData(new ExitGames.Client.Photon.Hashtable() { { "turn", turn }, { "turnRecievedPlayers", turnRecievedPlayers } });
        ServerClientBridge.NotifyClients("turn", turn);
        onTurn?.Invoke(turn);
        waitForTurnCoroutine = StartCoroutine(WaitForTurnResponse( onPlayerDidntRespond));
    }

    public IEnumerator WaitForTurnResponse(Action onPlayerDidntRespond)
    {
        yield return new WaitForSeconds(turnTime); 
        onPlayerDidntRespond?.Invoke();
    }

    public void RestartGame()
    {
        /*tableChips.SetActive(false);
        tablePot.gameObject.SetActive(false);

        playersBets = new float[turnGameSeats.Count];
        ph.SetRoomData("playersBets", playersBets);

        turnRecievedPlayers = new bool[turnGameSeats.Count];
        ph.SetRoomData("turnRecievedPlayers", turnRecievedPlayers);*/
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
