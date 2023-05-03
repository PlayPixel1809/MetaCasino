using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkRoom : MonoBehaviour
{
    public static NetworkRoom ins;
    void Awake() { ins = this; }

    public bool nonAuthoritativeServer = true;
    public bool assignRandomSeat;


    [Header("Assigned During Game -")]
    public int[] seats; // Sent To Clients

    public Action onRoomCreated;

    public Action<int, int> onSeatAssigned;
    public Action<int, int> onSeatVaccated;

    void Start()
    {
        seats = new int[NetworkRoomClient.ins.seats.Count];

        ServerClientBridge.ins.onPhotonMsgRecieved += OnPhotonMsgRecieved;

        ServerClientBridge.ins.onMadeMasterClient += ()=> 
        {
            if (ph.GetRoomData("seats") != null) { seats = (int[])ph.GetRoomData("seats"); }
        };
    }

    void OnPhotonMsgRecieved(int sender, string msg)
    {
        if (msg == "roomCreated")       { RoomCreated(sender); }
        if (msg == "playerEnteredRoom") { PlayerEnteredRoom(sender); }
        if (msg == "playerLeftRoom")    { PlayerLeftRoom(sender); }
    }


    public void RoomCreated(int creatorActorNo)
    {
        PlayerEnteredRoom(creatorActorNo);
        onRoomCreated?.Invoke();
    }

    public void PlayerEnteredRoom(int actorNo)
    {
        int seatIndex = -1;
        if (assignRandomSeat)
        {
            List<int> availableSeats = new List<int>();
            for (int i = 0; i < seats.Length; i++) { if (seats[i] == 0) { availableSeats.Add(i); } }
            if (availableSeats.Count > 0) { seatIndex = availableSeats[UnityEngine.Random.Range(0, availableSeats.Count)]; }
        }
        else
        {
            for (int i = 0; i < seats.Length; i++) 
            { 
                if (seats[i] == 0) { seatIndex = i; break; } 
            }
        }

        if (seatIndex > -1)
        {
            seats[seatIndex] = actorNo;
            SyncData("seats", seats);
            ServerClientBridge.ins.NotifyClients("PlayerEnteredRoom", "seats", seats);
            onSeatAssigned?.Invoke(seatIndex, actorNo);
        }
    }

    public void PlayerLeftRoom(int actorNo)
    {
        for (int i = 0; i < seats.Length; i++)
        {
            if (seats[i] == actorNo)
            {
                seats[i] = 0;

                SyncData("seats", seats);
                ServerClientBridge.ins.NotifyClients("PlayerLeftRoom", "seats", seats);

                onSeatVaccated?.Invoke(i, actorNo);
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
