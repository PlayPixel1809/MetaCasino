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

    [Header("Assigned During Game -")]
    public int[] seats; // Sent To Clients
    public int gameStartCounter; // Sent To Clients
    public float[] playersBets;

    public Action onGameStart;

    void Start()
    {
        seats = new int[NetworkRoomClient.ins.maxPlayers];

        NetworkRoom.ins.onPlayerEnteredRoom += (p) => { AssignSeatToPlayer(p.ActorNumber); };
        NetworkRoom.ins.onPlayerLeftRoom += OnPlayerLeftRoom;

        ServerClientBridge.ins.onClientMsgRecieved += OnClientMsgRecieved;
    }

    void OnClientMsgRecieved(int sender, ExitGames.Client.Photon.Hashtable hashtable)
    {
        

    }


    public void AssignSeatToPlayer(int actorNumber)
    {
        if (!ph.IsMasterClient()) { return; }


        List<int> availableSeats = new List<int>();
        for (int i = 0; i < seats.Length; i++) { if (seats[i] == 0) { availableSeats.Add(i); } }
        int randSeat = availableSeats[UnityEngine.Random.Range(0, availableSeats.Count)];
        availableSeats.Remove(randSeat);
        seats[randSeat] = actorNumber;

        SyncData("seats", seats);
        ServerClientBridge.NotifyClients("seats", seats);

        int filledSeats = seats.Length - availableSeats.Count;
        if (filledSeats == minPlayersToStartGame) { StartCoroutine("StartGameCounter", gameStartTime); }
    }

    IEnumerator StartGameCounter(int startFrom)
    {
        for (gameStartCounter = startFrom; gameStartCounter > -1; gameStartCounter--)
        {
            SyncData("gameStartCounter", gameStartCounter);
            ServerClientBridge.NotifyClients("gameStartCounter", gameStartCounter);
            yield return new WaitForSeconds(1);
        }

        playersBets = new float[seats.Length];
        SyncData(new ExitGames.Client.Photon.Hashtable() { { "playersBets", playersBets } });

        onGameStart?.Invoke();
    }

    void OnPlayerLeftRoom(Player player)
    {
        if (!ph.IsMasterClient()) { return; }

        int[] seats = (int[])ph.GetRoomData("seats");
        for (int i = 0; i < seats.Length; i++)
        {
            if (seats[i] == player.ActorNumber)
            {
                seats[i] = 0;
                ph.SetRoomData("seats", seats);
                return;
            }
        }
        ph.SetRoomData("seats", seats);
    }

    public int GetFilledSeatsCount()
    {
        int count = 0;
        for (int i = 0; i < seats.Length; i++)
        {
            if (seats[i] != 0) { count += 1; }
        }
        return count;
    }

    

    public int GetSeatIndex(int actorNo)
    {
        for (int i = 1; i < seats.Length; i++)
        {
            if (seats[i] == actorNo) { return i; }
        }
        return -1;
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
