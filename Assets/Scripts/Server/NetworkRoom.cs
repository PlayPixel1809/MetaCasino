using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkRoom : MonoBehaviourPunCallbacks
{
    public static NetworkRoom ins;
    void Awake() { ins = this; }

    public bool nonAuthoritativeServer = true;

    public float minBet = 1000;
    public float minBalance = 10000;

    [Header("Assigned During Game -")]
    public int[] seats; // Sent To Clients

    public Action onGameStart;
    public Action onRoomCreated;

    public Action<Player> onPlayerEnteredRoom;
    public Action<Player> onPlayerLeftRoom;

    public Action<int> onSeatAssigned;
    public Action<int> onSeatVaccated;

    public override void OnCreatedRoom()
    {
        onPlayerEnteredRoom?.Invoke(PhotonNetwork.LocalPlayer);
        onRoomCreated?.Invoke();
    }

    public override void OnPlayerEnteredRoom(Player player)
    {
        if (!PhotonNetwork.IsMasterClient) { return; }

        List<int> availableSeats = new List<int>();
        for (int i = 0; i < seats.Length; i++) { if (seats[i] == 0) { availableSeats.Add(i); } }
        int randSeat = availableSeats[UnityEngine.Random.Range(0, availableSeats.Count)];

        //temp code
        randSeat = seats.Length - availableSeats.Count;
        //temp code

        availableSeats.Remove(randSeat);
        seats[randSeat] = player.ActorNumber;

        onSeatAssigned?.Invoke(randSeat);


        SyncData("seats", seats);
        ServerClientBridge.NotifyClients("seats", seats);
    }

    public override void OnPlayerLeftRoom(Player player)
    {
        if (!PhotonNetwork.IsMasterClient) { return; }

        int[] seats = (int[])ph.GetRoomData("seats");
        for (int i = 0; i < seats.Length; i++)
        {
            if (seats[i] == player.ActorNumber)
            {
                seats[i] = 0;
                onSeatVaccated?.Invoke(i);
                ph.SetRoomData("seats", seats);
                return;
            }
        }
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
        for (int i = 0; i < seats.Length; i++)
        {
            if (seats[i] == actorNo) { return i; }
        }
        return -1;
    }

    public void SyncData(string key, object val) { SyncData(new ExitGames.Client.Photon.Hashtable() { { key, val } }); }

    public void SyncData(ExitGames.Client.Photon.Hashtable data)
    {
        if (nonAuthoritativeServer) { ph.SetRoomData(data); }
    }

}
