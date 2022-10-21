using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NetworkGameClient : MonoBehaviour
{
    public static NetworkGameClient ins;
    void Awake() { ins = this; }

    public Transform cam;
    public Transform camLookAtPoint;


    public List<NetworkGameSeat> seats;

    public Pot tablePot;
    public GameObject tableChips;
    public AudioClip betSound;

    public Text counter;

    public GameObject lpControls;
    public PlayerInfoUi lpInfoUi;

    public Text gameStartCounter;

    //[Header("Assigned During Game -")]

    void Start()
    {
        if (NetworkRoomClient.IsConnectedToMaster()) 
        { ph.SetLocalPlayerData("balance", User.localUser.balance); }
        else 
        {NetworkRoomClient.ins.onConnectedToMaster += () => { ph.SetLocalPlayerData("balance", User.localUser.balance); };}
        NetworkRoomClient.ins.onRoomLeft += () => { ph.SetLocalPlayerData("balance", null); };

        ServerClientBridge.ins.onServerMsgRecieved += OnMsgRecieved;
    }

    public void OnMsgRecieved(ExitGames.Client.Photon.Hashtable hashtable)
    {
        if (hashtable.ContainsKey("seats"))
        {
            int[] modifiedSeats = (int[])hashtable["seats"];
            for (int i = 0; i < seats.Count; i++)
            {
                if (modifiedSeats[i] != 0 && seats[i].actorNo == 0) { seats[i].OccupySeat(modifiedSeats[i]);}
                if (modifiedSeats[i] == 0 && seats[i].actorNo != 0) { seats[i].VaccateSeat(); }

                seats[i].actorNo = modifiedSeats[i];
            }
        }

        if (hashtable.ContainsKey("gameStartCounter"))
        {
            int count = (int)hashtable["gameStartCounter"];
            gameStartCounter.gameObject.SetActive(true);
            gameStartCounter.text = count.ToString();
            if (count == 0) { gameStartCounter.gameObject.SetActive(false); }
        }
    }

    
}
