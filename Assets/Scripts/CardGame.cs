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
    public Deck deck;
    public CardsHolder lpCards;

    public bool[] foldedPlayers;
    public string[] playersCards;
    public List<int> cards;


    void Start()
    {
        playersCards = new string[cardGameSeats.Count];
        for (int i = 0; i < playersCards.Length; i++) { playersCards[i] = "null";} //Photon does not serializes empty strings or empty string arrays
        Room.ins.startProperties.Add("playersCards", playersCards);

        Room.ins.onRoomPropertiesChanged += OnRoomPropertiesChanged;
    }
    
    void OnRoomPropertiesChanged(ExitGames.Client.Photon.Hashtable hashtable)
    {
        if (hashtable.ContainsKey("dealer") && ph.IsMasterClient()) 
        {
            foldedPlayers = new bool[cardGameSeats.Count];
            Room.ins.startProperties.Add("foldedPlayers", foldedPlayers);
            CreateCards();  
        }

        if (hashtable.ContainsKey("playersCards") && ph.IsMasterClient())
        {
            Utils.InvokeDelayedAction(cardsPerPlayer, ()=> { ph.SetRoomData("cardsDistributed", true); });
        }

        if (hashtable.ContainsKey("moveMade") && ph.IsMasterClient()) 
        { 
            if ((string)hashtable["moveMade"] == "Fold") 
            {
                TurnGame.ins.turnEligiblePlayers = (bool[])ph.GetRoomData("turnEligiblePlayers");
                TurnGame.ins.turnEligiblePlayers[(int)ph.GetRoomData("turn")] = false;
                ph.SetRoomData("turnEligiblePlayers", TurnGame.ins.turnEligiblePlayers);

                foldedPlayers = (bool[])ph.GetRoomData("foldedPlayers");
                foldedPlayers[(int)ph.GetRoomData("turn")] = true;
                ph.SetRoomData("foldedPlayers", foldedPlayers);
            }
        }
    }
   
    void CreateCards()
    {
        cards = new List<int>();
        for (int i = 0; i < 52; i++) { cards.Add(i + 1); }
        
        for (int i = 0; i < cardsPerPlayer; i++)
        {
            for (int j = 0; j < TurnGame.ins.turnEligiblePlayers.Length; j++)
            {
                if (TurnGame.ins.turnEligiblePlayers[j]) 
                {
                    playersCards[j] = playersCards[j].Replace("null","");
                    int card = UnityEngine.Random.Range(0, cards.Count);
                    cards.Remove(card);
                    playersCards[j] += card + ","; 
                }
            }
        }
        ph.SetRoomData("cards", cards.ToArray());
        ph.SetRoomData("playersCards", playersCards);
    }


    public int GetNonFoldedPlayersCount()
    {
        int count = 0;
        for (int i = 0; i < foldedPlayers.Length; i++) { if (!foldedPlayers[i]) { count += 1; }}
        return count;
    }
}
