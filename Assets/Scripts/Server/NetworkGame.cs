using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkGame : MonoBehaviour
{
    public static NetworkGame ins;
    void Awake() { ins = this; }

    public int minPlayersToStartGame = 2;
    public int gameStartTime = 5;

    public Action<int, int> onSeatAssigned;
    public Action<int, int> onSeatVaccated;
    public Action onGameStart;
    public Action onGameComplete;

    [Header("Assigned During Game -")]
    public int[] gamePlayingSeats = new int[0]; // Sent To Clients
    public float[] playersBets;
    public float minBet = 1000;
    public float minBalance = 10000;
    public bool gameCounterStarted;

    void Start()
    {
        NetworkRoom.ins.onRoomCreated += () => 
        {
            minBet = (float)ph.GetRoomData("minBet");
            minBalance = (float)ph.GetRoomData("minBalance");
        };


        NetworkRoom.ins.onSeatAssigned += OnSeatAssigned;
        NetworkRoom.ins.onSeatVaccated += OnSeatVaccated;

        ServerClientBridge.ins.onClientMsgRecieved += OnClientMsgRecieved;

        ServerClientBridge.ins.onMadeMasterClient += () =>
        {
            minBet = (float)ph.GetRoomData("minBet");
            minBalance = (float)ph.GetRoomData("minBalance");
            if (ph.GetRoomData("gamePlayingSeats") != null) { gamePlayingSeats = (int[])ph.GetRoomData("gamePlayingSeats"); }
            if (ph.GetRoomData("playersBets") != null)      { playersBets = (float[])ph.GetRoomData("playersBets"); }
            gameCounterStarted = (bool)ph.GetRoomData("gameCounterStarted");
        };
    }

    
    void OnSeatAssigned(int seatIndex, int actorNo)
    {
        if (!gameCounterStarted && NetworkRoom.ins.GetFilledSeatsCount() >= minPlayersToStartGame)
        {
            gameCounterStarted = true;
            NetworkRoom.ins.SyncData("gameCounterStarted", gameCounterStarted);
            ServerClientBridge.ins.HireClients("RunGameStartCounter", "counter", gameStartTime);
        }
    }

    void OnSeatVaccated(int seatIndex, int actorNo)
    {
        if (gamePlayingSeats.Contains(actorNo))
        {
            onSeatVaccated?.Invoke(seatIndex, actorNo);
        }
    }

    void OnClientMsgRecieved(string completedEvId, ExitGames.Client.Photon.Hashtable data)
    {
        if (completedEvId == "RunGameStartCounter")
        {
            if (NetworkRoom.ins.GetFilledSeatsCount() >= minPlayersToStartGame)
            {
                int counter = (int)data["counter"];
                if (counter == 0)
                {
                    gamePlayingSeats = new int[NetworkRoomClient.ins.seats.Count];
                    for (int i = 0; i < NetworkRoom.ins.seats.Length; i++) 
                    { 
                        gamePlayingSeats[i] = NetworkRoom.ins.seats[i];
                        onSeatAssigned?.Invoke(i, gamePlayingSeats[i]);
                    }
                    playersBets = new float[gamePlayingSeats.Length];

                    NetworkRoom.ins.SyncData(new ExitGames.Client.Photon.Hashtable() { { "gamePlayingSeats", gamePlayingSeats }, { "playersBets", playersBets } });
                    onGameStart?.Invoke();
                }
                else
                { ServerClientBridge.ins.HireClients("RunGameStartCounter", "counter", counter); }
            }
            else
            {
                gameCounterStarted = false;
                NetworkRoom.ins.SyncData("gameCounterStarted", gameCounterStarted);
                ServerClientBridge.ins.NotifyClients("HideGameStartCounter");
            }
        }
        

        if (completedEvId == "GameComplete") 
        {
            gameCounterStarted = false;
            NetworkRoom.ins.SyncData("gameCounterStarted", gameCounterStarted);

            if (NetworkRoom.ins.GetFilledSeatsCount() >= minPlayersToStartGame)
            {
                gameCounterStarted = true;
                NetworkRoom.ins.SyncData("gameCounterStarted", gameCounterStarted);
                ServerClientBridge.ins.HireClients("RunGameStartCounter", "counter", gameStartTime);
            }
        }
    }

    
    public void GameComplete()
    {
        RemovePlayersWithInvalidBalance();

        onGameComplete?.Invoke();
        ServerClientBridge.ins.HireClients("GameComplete");
    }

    public void RemovePlayersWithInvalidBalance()
    {
        for (int i = 0; i < gamePlayingSeats.Length; i++)
        {
            if (NetworkRoom.ins.seats[i] != 0 && (float)ph.GetPlayerData(gamePlayingSeats[i], "balance") < minBalance)
            {
                NetworkRoom.ins.PlayerLeftRoom(gamePlayingSeats[i]);
            }
        }
    }


    public float Restart()
    {
        float highestBet = playersBets[0];
        for (int i = 1; i < playersBets.Length; i++)
        {
            if (playersBets[i] > highestBet) { highestBet = playersBets[i]; }
        }
        return highestBet;
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

    
}
