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

    public int cardsPerPlayer = 2;
    //public Deck deck;


    public Action onCardsDistributed;

    [Header("Assigned During Game -")]
    public bool[] foldedPlayers;
    public List<int> deck;
    public string[] playersCards;

    

    void Start()
    {
        NetworkRoom.ins.onSeatAssigned += (seatIndex, actorNo) =>
        {
            if (actorNo > 0) { ServerClientBridge.ins.NotifyClient(actorNo, "SetCardGameData", new ExitGames.Client.Photon.Hashtable() { { "cardsPerPlayer", cardsPerPlayer } }); }
        };

        ServerClientBridge.ins.onClientMsgRecieved += OnClientMsgRecieved;

        ServerClientBridge.ins.onMadeMasterClient += () =>
        {
            if (ph.GetRoomData("foldedPlayers") != null) { foldedPlayers = (bool[])ph.GetRoomData("foldedPlayers"); }
            if (ph.GetRoomData("deck") != null)
            {
                int[] deckArray = (int[])ph.GetRoomData("deck");
                deck = new List<int>();
                for (int i = 0; i < deckArray.Length; i++) { deck.Add(deckArray[i]); }
            }
            if (ph.GetRoomData("playersCards") != null) { playersCards = (string[])ph.GetRoomData("playersCards"); }

        };

        NetworkGame.ins.onGameStart += () =>
        {
            foldedPlayers = new bool[NetworkRoom.ins.seats.Length];
            NetworkRoom.ins.SyncData("foldedPlayers", foldedPlayers);
        };

        TurnGame.ins.onDealerSet += (d) =>
        {
            ShuffleDeck();
            DistributeCards();
        };

        TurnGame.ins.onTurnCompleted += TurnCompleted;

        NetworkGame.ins.onGameComplete += () =>
        {
            NetworkRoom.ins.SyncData(new ExitGames.Client.Photon.Hashtable()
            {
                { "playersCards", null } 
            });
        };

        NetworkGame.ins.onSeatVaccated += (seatIndex, actorNo) =>
        {
            foldedPlayers[seatIndex] = true;
            NetworkRoom.ins.SyncData("foldedPlayers", foldedPlayers);
        };
    }



    void OnClientMsgRecieved(string completedEvId, ExitGames.Client.Photon.Hashtable data)
    {
        if (completedEvId == "DistributeCards") { onCardsDistributed?.Invoke(); }
    }

    void TurnCompleted(ExitGames.Client.Photon.Hashtable data)
    {
        if (((string)data["moveMade"]).ToLower() == "fold")
        {
            TurnGame.ins.turnEligiblePlayers[TurnGame.ins.turn] = false;
            foldedPlayers[TurnGame.ins.turn] = true;

            NetworkRoom.ins.SyncData(new ExitGames.Client.Photon.Hashtable() { { "turnEligiblePlayers", TurnGame.ins.turnEligiblePlayers }, { "foldedPlayers", foldedPlayers } });
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
        playersCards = new string[NetworkRoom.ins.seats.Length];
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

        foldedPlayers = new bool[NetworkRoom.ins.seats.Length];

        NetworkRoom.ins.SyncData(new ExitGames.Client.Photon.Hashtable() 
        {
            { "playersCards", playersCards } ,
            { "foldedPlayers", foldedPlayers } ,
            { "deck", deck.ToArray() } ,
        });

        ServerClientBridge.ins.HireClients("DistributeCards", "playersCards", playersCards);
    }

    public int GetNonFoldedPlayersCount()
    {
        int count = 0;
        for (int i = 0; i < foldedPlayers.Length; i++) { if (playersCards[i] != "Null" && !foldedPlayers[i]) { count += 1; } }
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

        if (syncDeckData) { NetworkRoom.ins.SyncData("deck", deck.ToArray()); }
        return cards;
    }

    public List<int> GetPlayerCards(int seatIndex)
    {
        string[] cardsArray = playersCards[seatIndex].Split(new string[1] { "," }, StringSplitOptions.RemoveEmptyEntries);
        List<int> cards = new List<int>();
        for (int i = 0; i < cardsArray.Length; i++)  { cards.Add(int.Parse(cardsArray[i]));  }
        return cards;
    }

    public bool IsSeatFolded(int seatIndex)
    {
        if (playersCards[seatIndex] != "Null" && !foldedPlayers[seatIndex]) { return false; }
        else { return true; }
    }
}
