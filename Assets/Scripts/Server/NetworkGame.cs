using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkGame : MonoBehaviour
{
    public static NetworkGame ins;
    void Awake() { ins = this; }

    public bool nonAuthoritativeServer = true;
    public int minPlayersToStartGame = 2;
    public int gameStartTime = 5;

    public Action onGameStart;

    [Header("Assigned During Game -")]
    public bool[] gamePlayingSeats; // Sent To Clients
    public int gameStartCounter; // Sent To Clients
    public float[] playersBets;


    void Start()
    {
        NetworkRoom.ins.onSeatAssigned += OnSeatAssigned;
        NetworkRoom.ins.onSeatVaccated += OnSeatVaccated;

        ServerClientBridge.ins.onClientMsgRecieved += OnClientMsgRecieved;
    }

   
    void OnSeatAssigned(int seatIndex)
    {
        if (NetworkRoom.ins.GetFilledSeatsCount() == minPlayersToStartGame) { StartCoroutine("StartGameCounter", gameStartTime); }
    }

    void OnSeatVaccated(int seatIndex)
    {

    }

    void OnClientMsgRecieved(int sender, ExitGames.Client.Photon.Hashtable hashtable)
    {


    }



    IEnumerator StartGameCounter(int startFrom)
    {
        for (gameStartCounter = startFrom; gameStartCounter > -1; gameStartCounter--)
        {
            SyncData("gameStartCounter", gameStartCounter);
            ServerClientBridge.NotifyClients("gameStartCounter", gameStartCounter);
            yield return new WaitForSeconds(1);
        }

        gamePlayingSeats = new bool[NetworkRoom.ins.seats.Length];
        for (int i = 0; i < NetworkRoom.ins.seats.Length; i++)
        {
            if (NetworkRoom.ins.seats[i] != 0) { gamePlayingSeats[i] = true; }
        }

        playersBets = new float[NetworkRoom.ins.seats.Length];
        SyncData(new ExitGames.Client.Photon.Hashtable() { { "playersBets", playersBets }, { "gamePlayingSeats", gamePlayingSeats } });

        onGameStart?.Invoke();
    }

   

    public float GetHighestBet()
    {
        float highestBet = playersBets[0];
        for (int i = 1; i < playersBets.Length; i++)
        {
            if (playersBets[i] > highestBet) { highestBet = playersBets[i]; }
        }
        return highestBet;
    }

    public float GetTotalBet()
    {
        float totalBet = 0;
        for (int i = 1; i < playersBets.Length; i++) { totalBet += playersBets[i]; }
        return totalBet;
    }

    public void SyncData(string key, object val) { SyncData(new ExitGames.Client.Photon.Hashtable() { { key, val } }); }

    public void SyncData(ExitGames.Client.Photon.Hashtable data)
    {
        if (nonAuthoritativeServer) { ph.SetRoomData(data); }
    }

    
}
