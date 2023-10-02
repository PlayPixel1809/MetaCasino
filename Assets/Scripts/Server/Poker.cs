using ExitGames.Client.Photon;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Poker : MonoBehaviour
{
    public static Poker ins;
    void Awake() { ins = this; }

    [Header("Assigned During Game -")]
    public List<int> communityCards;
    public float[] playersBetsForRound;

    public bool[] roundActiveSeats;
    public bool[] allInPlayers;
    public List<float> sidePotsAmount;
    public List<string> sidePotsSeats;
    public float mainPot;
    public bool mainPotFoldout;
    public List<int> mainPotSeats;
    public int round;


    public class PotInfo
    {
        public float potAmount;
        public List<int> showdownEligibleSeats = new List<int>();
        public string showdownEligibleSeatsString;
        public DataUtils dataUtils = new DataUtils();
    }

    void Start()
    {
        ServerClientBridge.ins.onClientMsgRecieved += OnClientMsgRecieved;
        ServerClientBridge.ins.onMadeMasterClient += RestoreData;
        

        NetworkGame.ins.onGameStart += () =>
        {
            allInPlayers = new bool[NetworkRoom.ins.seats.Length];
            roundActiveSeats = new bool[NetworkRoom.ins.seats.Length];
            mainPot = 0;
            mainPotSeats = new List<int>();
            mainPotFoldout = false;
            sidePotsSeats = new List<string>();
            sidePotsAmount = new List<float>();
            communityCards = new List<int>();
            NetworkRoom.ins.SyncData(new ExitGames.Client.Photon.Hashtable() {  { "allInPlayers", allInPlayers }, { "sidePotsSeats", sidePotsSeats.ToArray() }, { "sidePotsAmount", sidePotsAmount.ToArray() }, 
                { "communityCards", communityCards.ToArray() }, { "mainPot", mainPot }, { "mainPotSeats", mainPotSeats.ToArray() }, { "mainPotFoldout", mainPotFoldout } } );

        };

        TurnGame.ins.onTurnStarted += (i) => 
        {
            ServerClientBridge.ins.NotifyClients("StartPokerTurn",  new ExitGames.Client.Photon.Hashtable() { { "currentBet", GetHighestBet() }, { "playerBet", playersBetsForRound[i] } });
        };

        TurnGame.ins.onTurnCompleted += TurnCompleted;

        CardGame.ins.onCardsDistributed += () => { StartRound(1); };

        NetworkGame.ins.onGameComplete += () =>
        {
            NetworkRoom.ins.SyncData(new ExitGames.Client.Photon.Hashtable()
            {
                { "playersBetsForRound", null },
                { "communityCards", null }
            });
        };
    }

    public void StartShowDowns()
    {
        if (communityCards.Count < 5) 
        { 
            CreateCommunityCards(5 - communityCards.Count, true);
            return;
        }

        CardGame.ins.playersCardsCombinationType = new string[CardGame.ins.playersCards.Length];
        for(int i = 0; i < CardGame.ins.playersCardsCombinationType.Length; i++) { CardGame.ins.playersCardsCombinationType[i] = "Null"; }

        for (int i = 0; i < CardGame.ins.playersCards.Length; i++)
        {
            if (CardGame.ins.playersCards[i] != "Null" && !CardGame.ins.foldedPlayers[i]) 
            { CardGame.ins.playersCards[i] = PokerHands.GetBestFiveCardsCombination(CardGame.ins.GetPlayerCards(i),out CardGame.ins.playersCardsCombinationType[i]); }
        }
        NetworkRoom.ins.SyncData("playersCards", CardGame.ins.playersCards);
        NetworkRoom.ins.SyncData("playersCardsCombinationType", CardGame.ins.playersCardsCombinationType);

        ServerClientBridge.ins.HireClients("PrepareForShowDowns");
    }

    

    
    public void CreateCommunityCards(int count, bool forShowdown = false)
    {
        List<int> newCards = CardGame.ins.GetCards(count);
        communityCards.AddRange(newCards);

        NetworkRoom.ins.SyncData("communityCards", communityCards.ToArray());

        if (!forShowdown) { ServerClientBridge.ins.HireClients("CreateCommunityCards", "cards", newCards.ToArray()); }
        if (forShowdown)  { ServerClientBridge.ins.HireClients("CreateCommunityCardsForShowdown", "cards", newCards.ToArray()); }
    }


    public void CreateCardsBestCombination()
    {
        for (int i = 0; i < NetworkRoom.ins.seats.Length; i++)
        {
            if (NetworkRoom.ins.seats[i] > 0)
            {
                string combinationType = string.Empty;
                string cards = PokerHands.GetBestFiveCardsCombination(CardGame.ins.GetPlayerCards(i),out combinationType);

                string[] cardsStringArray = cards.Split(new string[1] { "," }, StringSplitOptions.RemoveEmptyEntries);
                int[] cardsIntArray = new int[cardsStringArray.Length];
                for (int j = 0; j < cardsStringArray.Length; j++) { cardsIntArray[j] = int.Parse(cardsStringArray[j]); }

                ExitGames.Client.Photon.Hashtable data = new ExitGames.Client.Photon.Hashtable()
                {
                    { "cards", cardsIntArray },
                    { "combinationType", combinationType }
                };

                ServerClientBridge.ins.NotifyClient(NetworkRoom.ins.seats[i], "ShowCardsBestCombination", data);
            }
        }
    }



    public void CreateReplay()
    {
        ExitGames.Client.Photon.Hashtable data = new ExitGames.Client.Photon.Hashtable()
        {
            { "playersBestFiveCards", CardGame.ins.playersCards },
            { "playersCardsCombinationType", CardGame.ins.playersCardsCombinationType }
        };

        ServerClientBridge.ins.HireClients("CreateReplay", data);
    }















}


/* After the round completes and bets have been placed we check for side pots and then the remaining money goes into main pot -
 * 
 * Side pots creation logic - we check for lowest all in amount in the round and and we create a side pot by taking that amount from every non-folded player.
 * suppose lowest allin amount is 10000 and a folded player did not bet 10000 and only betted 5000 so we will take 5000 from him.
 * 
 * Example  -
     * Round 2 , Main pot - 20000, p1 - 10000, allin, p2 - 15000 allin, p3 - 20000, p4 20000 , p5 5000 folded, p6 17000 folded
     * 
     * In this case two side pots will be created since there are two allins for two different amounts-
     * 
     * First we take 10000 from every one (5000 from p5 as he is folded) so SidePot1 (p1, p2, p3, p4) - 55000 
     * 
     * Since its 2nd round and there is already some money in Main pot so that money gets credited to the first side pot of the round and main pot becomes zero. so SidePot1  - 55000 + 20000 = 75000
     * 
     * and now new values are -
     * p1 - 0, allin, p2 - 5000 allin, p3 - 10000, p4 10000 , p5 0 folded, p6 7000 folded
     * 
     * Second we take 5000 from every player excluding p1 since sidePot1 has already been created for p1. so SidePot2 (p2, p3, p4) - 20000 
     * 
     * and now new values are -
     * p1 - 0, allin, p0 - 0 allin, p3 - 5000, p4 5000 , p5 0 folded, p6 2000 folded
     * 
     * All the remaining money goes into the main pot : p3 - 5000, p4 5000 , p6 2000 folded = Main pot (p3, p4) - 12000
     * (p3, p4) goes to the next round
 * 
 *
 *When main pot gets created and if there is only one player in main pot that means that player has won the mainpot. 
 *We show the foldout when there is some money in mainpot and every other player has folded.
 *
 *We wanna show foldout sequence when every other player who participated in the round (read GetRoundActiveSeatsCount() summary to understand participated in round means) folds and last player wins.
 * Example 1 -
     * Round 1 - p1 1000 small blind, p2 2000 big blind, p3
     * Suppose p3 folds and then p1 folds so p2 will win the game with foldout
     * 
     * 
 * Example 2 -
     * Round 1 - p1 10000 all in, p2 20000 , p3 20000, side pot(p1,p2,p3) - 30000
     * Round 2 - p2 folded, p3 turn did notcame
     * In this case although p3 has win the round but no foldout sequence will be shown as there is no money in the main pot
 * 
 * Example 3 -
     * Round 1 - p1 10000 all in, p2 20000 , p3 20000, side pot(p1,p2,p3) - 30000
     * Round 2 - p2 10000 folded, p3 20000 
     * We will show foldout with p3 as winner
 * 
 * Example 4 - 
     * Round 1 - p1 10000 all in, p2 20000 , p3 20000, p4 20000 side pot(p1,p2,p3,p4) - 40000 , main pot(p1,p2,p3) - 30000 
     * Round 2 - p2 20000 folded, p3 15000 allin, p4 10000 folded , side pot(p3,p4) - 40000, main pot(p1) - 5000
     * However there is only p1 in main pot he will get the amount but foldout will not be shown
 * */