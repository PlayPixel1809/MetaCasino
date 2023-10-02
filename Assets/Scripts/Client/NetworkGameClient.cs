using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NetworkGameClient : MonoBehaviour
{
    public static NetworkGameClient ins;
    void Awake() { ins = this; }

    public float minBet = 1000;
    public float minBalance = 10000;



    public Pot gamePot;
    public Pocket lpBalance;
    public GameObject lpControls;
    public Text gameStartCounter;

    public Action onRoomJoined;
    public Action onSeatAssigned;
    public Action onGameComplete;


    [Header("Assigned During Game -"),Space]
    public List<NetworkGameSeat> seats;

    void Start()
    {
        StartCoroutine("AssignSeats");

        NetworkRoomClient.ins.onJoinRoom += () => 
        {
            ph.SetLocalPlayerData("balance", User.localUser.balance);
            AddCustomRoomProperties();
        };

        NetworkRoomClient.ins.onSeatAssigned += () =>
        {
            onSeatAssigned?.Invoke();
        };

        ServerClientBridge.ins.onServerMsgRecieved += OnServerMsgRecieved;

        NetworkRoomClient.ins.onRoomLeft += () => { ph.SetLocalPlayerData("balance", null); };
    }

    
    IEnumerator AssignSeats()
    {
        yield return new WaitForEndOfFrame();
        seats = new List<NetworkGameSeat>();
        for (int i = 0; i < NetworkRoomClient.ins.seats.Count; i++) { seats.Add(NetworkRoomClient.ins.seats[i].GetComponent<NetworkGameSeat>()); }
    }

    public void OnServerMsgRecieved(string evId, ExitGames.Client.Photon.Hashtable data)
    {
        if (evId == "RunGameStartCounter") { StartCoroutine("RunGameStartCounter", data); }
        if (evId == "HideGameStartCounter") { gameStartCounter.gameObject.SetActive(false); }

        if (evId == "GameComplete") { StartCoroutine("GameComplete"); }
    }

    IEnumerator RunGameStartCounter(ExitGames.Client.Photon.Hashtable data)
    {
        gameStartCounter.gameObject.SetActive(true);
        gameStartCounter.text = ((int)data["counter"]).ToString();
        yield return new WaitForSeconds(1);
        data["counter"] = (int)data["counter"] - 1;
        if ((int)data["counter"] == 0){ gameStartCounter.gameObject.SetActive(false); }
        ServerClientBridge.ins.NotifyServerIfMasterClient("RunGameStartCounter", "counter", data["counter"]);
        //NetworkRoomClient.CloseRoom();
    }

    IEnumerator GameComplete()
    {
        for (int i = 0; i < seats.Count; i++) { seats[i].winAmount = 0; }
        onGameComplete?.Invoke();
        yield return new WaitForSeconds(2);
        ServerClientBridge.ins.NotifyServerIfMasterClient("GameComplete");
    }

    void AddCustomRoomProperties()
    {
        KeyValue.AddValueToList("minBet", KeyValue.ValueTypes.Float, minBet, NetworkRoomClient.ins.customRoomProperties);
        KeyValue.AddValueToList("minBalance", KeyValue.ValueTypes.Float, minBalance, NetworkRoomClient.ins.customRoomProperties);
        NetworkRoomClient.ins.customRoomPropertiesForLobby.AddRange(new List<string>() { "minBet", "minBalance" });

        KeyValue.AddValueToList("minBet", KeyValue.ValueTypes.Float, minBet, NetworkRoomClient.ins.expectedCustomRoomProperties);
        KeyValue.AddValueToList("minBalance", KeyValue.ValueTypes.Float, minBalance, NetworkRoomClient.ins.expectedCustomRoomProperties);
    }
}


