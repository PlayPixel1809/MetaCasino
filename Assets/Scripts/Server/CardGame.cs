using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Photon.Realtime;
using Photon.Pun;
using UnityEngine.UI;

public class CardGame : MonoBehaviour
{
    public static CardGame ins;
    void Awake() { ins = this; }

    public List<CardGameSeat> cardGameSeats;


    public int cardsPerPlayer = 2;
    //public Deck deck;
    public CardsHolder lpCards;


    public Action onCardsDistributed;

    [Header("Assigned During Game -")]
    public bool[] foldedPlayers;
    public List<int> deck;

    [Header("Sent to clients -")]
    public string[] playersCards;

    

    void Start()
    {
        ServerClientBridge.ins.onClientMsgRecieved += OnClientMsgRecieved;

        NetworkGame.ins.onGameStart += () =>
        {
            foldedPlayers = new bool[NetworkGame.ins.seats.Length];
            NetworkGame.ins.SyncData("foldedPlayers", foldedPlayers);
        };

        TurnGame.ins.onDealerSet += (d) =>
        {
            ShuffleDeck();
            DistributeCards();
        };
    }



    void OnClientMsgRecieved(int senderActorNo, ExitGames.Client.Photon.Hashtable hashtable)
    {
        if (hashtable.ContainsKey("moveMade") && (string)hashtable["moveMade"] == "Fold")
        {
            TurnGame.ins.turnEligiblePlayers[NetworkGame.ins.GetSeatIndex(senderActorNo)] = false;
            foldedPlayers[NetworkGame.ins.GetSeatIndex(senderActorNo)] = true;

            NetworkGame.ins.SyncData(new ExitGames.Client.Photon.Hashtable() { { "turnEligiblePlayers", TurnGame.ins.turnEligiblePlayers }, { "foldedPlayers", foldedPlayers } });
        }

    }
    

    void ShuffleDeck()
    {
        List<int> unShuffledDeck = new List<int>();
        for (int i = 1; i < 53; i++) { unShuffledDeck.Add(i); }

        deck = new List<int>();
        while (unShuffledDeck.Count > 0) 
        {
            deck.Add(unShuffledDeck[new System.Random().Next(0, unShuffledDeck.Count)]);
            unShuffledDeck.Remove(deck[deck.Count - 1]);
        }
    }

    void DistributeCards()
    {
        playersCards = new string[NetworkGame.ins.seats.Length];
        for (int i = 0; i < cardsPerPlayer; i++)
        {
            for (int j = 0; j < TurnGame.ins.turnEligiblePlayers.Length; j++)
            {
                if (TurnGame.ins.turnEligiblePlayers[j])
                { playersCards[j] += GetCards(1, false)[0] + ","; }
                else 
                { playersCards[j] = "Null"; }
            }
        }

        foldedPlayers = new bool[cardGameSeats.Count];

        NetworkGame.ins.SyncData(new ExitGames.Client.Photon.Hashtable() 
        {
            { "playersCards", playersCards } ,
            { "foldedPlayers", foldedPlayers } ,
            { "deck", deck.ToArray() } ,
        });

        ServerClientBridge.NotifyClients("playersCards", playersCards);

        Utils.InvokeDelayedAction(cardsPerPlayer, () => { onCardsDistributed?.Invoke(); });
    }

    public int GetNonFoldedPlayersCount()
    {
        int count = 0;
        for (int i = 0; i < foldedPlayers.Length; i++) { if (!foldedPlayers[i]) { count += 1; } }
        return count;
    }

    public List<int> GetCards(int amount, bool syncDeckData = true)
    {
        List<int> cards = new List<int>();
        for (int i = 0; i < amount; i++) 
        {
            int card = deck[0];
            deck.Remove(card);
            cards.Add(card);
        }

        if (syncDeckData) { NetworkGame.ins.SyncData("deck", deck); }
        return cards;
    }

    public List<int> GetPlayerCards(int seatIndex)
    {
        string[] cardsArray = playersCards[seatIndex].Split(new string[1] { "," }, StringSplitOptions.RemoveEmptyEntries);
        List<int> cards = new List<int>();
        for (int i = 0; i < cardsArray.Length; i++) { cards.Add(int.Parse(cardsArray[i])); }
        return cards;
    }
}
