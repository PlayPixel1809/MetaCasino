using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardGameClient : MonoBehaviour
{
    public static CardGameClient ins;
    void Awake() { ins = this; }

    
    public CardsHolder lpCards;

    public Action onRoomJoined;
    public Action onSeatAssigned;

    [Header("Assigned During Game -")]
    public List<CardGameSeat> seats;
    public string[] playersCards;
    public int cardsPerPlayer;

    void Start()
    {
        StartCoroutine("AssignSeats");

        TurnGameClient.ins.onRoomJoined += () =>
        {
            onRoomJoined?.Invoke();
        };


        TurnGameClient.ins.onSeatAssigned += () =>
        {
            if (ph.GetRoomData("playersCards") != null) { CreateCards((string[])ph.GetRoomData("playersCards"), false); }

            onSeatAssigned?.Invoke();
        };


        ServerClientBridge.ins.onServerMsgRecieved += OnServerMsgRecieved;

        NetworkGameClient.ins.onGameComplete += () =>
        {
            for (int i = 0; i < seats.Count; i++) { seats[i].cards.RemoveCards(); }
            Deck.ins.Reset();
        };
    }

    IEnumerator AssignSeats()
    {
        yield return new WaitForEndOfFrame();
        seats = new List<CardGameSeat>();
        for (int i = 0; i < NetworkRoomClient.ins.seats.Count; i++) { seats.Add(NetworkRoomClient.ins.seats[i].GetComponent<CardGameSeat>()); }
    }

    public void OnServerMsgRecieved(string evId, ExitGames.Client.Photon.Hashtable data)
    {
        if (evId == "SetCardGameData") { cardsPerPlayer = (int)data["cardsPerPlayer"]; }

        if (evId == "DistributeCards")
        {
            StartCoroutine("DistributeCards", data);
        }
    }

    IEnumerator DistributeCards (ExitGames.Client.Photon.Hashtable data)
    {
        CreateCards((string[])data["playersCards"], true);
        yield return new WaitForSeconds(cardsPerPlayer);
        ServerClientBridge.ins.NotifyServerIfMasterClient((string)data["evId"]);
    }

    void CreateCards(string[] playersCards, bool animateCards)
    {
        Debug.Log("CreateCards");
        this.playersCards = playersCards;
        for (int i = 0; i < seats.Count; i++)
        {
            if (playersCards[i] == "Null") { continue; }

            string[] playerCards = playersCards[i].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            seats[i].CreateCards(playerCards, animateCards);
        }
    }
}
