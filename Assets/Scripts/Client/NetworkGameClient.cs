using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NetworkGameClient : MonoBehaviour
{
    public static NetworkGameClient ins;
    void Awake() { ins = this; }

    [HideInInspector]
    

    public Pot gamePot;

    public Pocket lpBalance;
    public GameObject lpControls;
    public Text gameStartCounter;

    [Header("Assigned During Game -")]
    public List<NetworkGameSeat> seats;

    void Start()
    {
        StartCoroutine("AssignSeats");

        if (NetworkRoomClient.IsConnectedToMaster()) 
        { ph.SetLocalPlayerData("balance", User.localUser.balance); }
        else 
        {NetworkRoomClient.ins.onConnectedToMaster += () => { ph.SetLocalPlayerData("balance", User.localUser.balance); };}
        NetworkRoomClient.ins.onRoomLeft += () => { ph.SetLocalPlayerData("balance", null); };

        ServerClientBridge.ins.onServerMsgRecieved += OnMsgRecieved;
    }

    IEnumerator AssignSeats()
    {
        yield return new WaitForEndOfFrame();
        seats = new List<NetworkGameSeat>();
        for (int i = 0; i < NetworkRoomClient.ins.seats.Count; i++) { seats.Add(NetworkRoomClient.ins.seats[i].GetComponent<NetworkGameSeat>()); }
    }

    public void OnMsgRecieved(ExitGames.Client.Photon.Hashtable hashtable)
    {
        if (hashtable.ContainsKey("gameStartCounter"))
        {
            int count = (int)hashtable["gameStartCounter"];
            gameStartCounter.gameObject.SetActive(true);
            gameStartCounter.text = count.ToString();
            if (count == 0) 
            { 
                gameStartCounter.gameObject.SetActive(false);
                //NetworkRoomClient.CloseRoom();
            }
        }
    }
    
}
