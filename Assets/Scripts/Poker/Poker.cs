using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Poker : MonoBehaviour
{
    public static Poker ins;
    void Awake() { ins = this; }

    public CardsHolder communityCardsHolder;
    public CardsHolder communityCardsHolder3D;

    public int[] communityCards;
    public int round;

    void Start()
    {
        communityCards = new int[5];
        Room.ins.startProperties.Add("communityCards", communityCards);
        round = 1;
        Room.ins.startProperties.Add("round", round);

        Room.ins.onRoomPropertiesChanged += OnRoomPropertiesChanged;
    }

    void OnRoomPropertiesChanged(ExitGames.Client.Photon.Hashtable hashtable)
    {
        if (hashtable.ContainsKey("dealer") && ph.IsMasterClient())
        {
            int dealerIndex = (int)ph.GetRoomData("dealer");

            TurnGame.ins.playersBets[dealerIndex] = (float)ph.GetRoomData("minBet");
            TurnGame.ins.playersBets[TurnGame.ins.GetNextTurnIndex(dealerIndex)] = (float)ph.GetRoomData("minBet") * 2;
            ph.SetRoomData("playersBets", TurnGame.ins.playersBets);

            TurnGame.ins.currentBet = (float)ph.GetRoomData("minBet") * 2;
            ph.SetRoomData("currentBet", TurnGame.ins.currentBet);
        }

        if (hashtable.ContainsKey("cardsDistributed") && ph.IsMasterClient()) { StartFirstTurn(); }

        if (hashtable.ContainsKey("moveMade") && ph.IsMasterClient())
        {
            if ((string)hashtable["moveMade"] == "AllIn")
            {
                TurnGame.ins.turnEligiblePlayers = (bool[])ph.GetRoomData("turnEligiblePlayers");
                TurnGame.ins.turnEligiblePlayers[(int)ph.GetRoomData("turn")] = false;
                ph.SetRoomData("turnEligiblePlayers", TurnGame.ins.turnEligiblePlayers);
            }
        }

        if (hashtable.ContainsKey("canStartNextTurn") && ph.IsMasterClient()) { StartNextTurn(); }

        if (hashtable.ContainsKey("roundEnd") && ph.IsMasterClient()) { EndRound(); }

        if (hashtable.ContainsKey("round") && ph.IsMasterClient()) { StartRound(); }

        if (hashtable.ContainsKey("newCommunityCards")) { SpawnCommunityCards(); }

        if (hashtable.ContainsKey("communityCards"))
        {
            Utils.InvokeDelayedAction(2, StartFirstTurn);
        }
    }


    public void StartRound()
    {
        round = (int)ph.GetRoomData("round");

        if (round == 1) { StartFirstTurn(); }
        if (round == 2) { CreateCommunityCards(3); }
        if (round == 3) { CreateCommunityCards(1); }
        if (round == 4) { CreateCommunityCards(1); }
    }

    public void StartFirstTurn()
    {
        int dealer = (int)ph.GetRoomData("dealer");
        int startTurnIndex = TurnGame.ins.GetNextTurnIndex(dealer);
        if (round == 1) { startTurnIndex = TurnGame.ins.GetNextTurnIndex(startTurnIndex); }
        TurnGame.ins.StartTurn(startTurnIndex, PlayerDidntRespodedToTurn);
    }

    public void StartNextTurn()
    {
        if (CardGame.ins.GetNonFoldedPlayersCount() < 2) { ph.SetRoomData("roundEnd", true); return; }

        int nextTurnIndex = TurnGame.ins.GetNextTurnIndex((int)ph.GetRoomData("turn"));
        if (nextTurnIndex < 0) { ph.SetRoomData("roundEnd", true); return; }

        if (!TurnGame.ins.turnRecievedPlayers[nextTurnIndex]) { TurnGame.ins.StartTurn(nextTurnIndex, PlayerDidntRespodedToTurn); }
        else
        {
            float nextPlayerBet = TurnGame.ins.playersBets[nextTurnIndex];

            if (nextPlayerBet < (float)ph.GetRoomData("currentBet"))
            { TurnGame.ins.StartTurn(nextTurnIndex, PlayerDidntRespodedToTurn); }
            else
            { ph.SetRoomData("roundEnd", true); }
        }
    }

    public void EndRound()
    {
        for (int i = 0; i < TurnGame.ins.playersBets.Length; i++) { TurnGame.ins.pot += TurnGame.ins.playersBets[i]; }
        ph.SetRoomData("pot", TurnGame.ins.pot);

        if (ph.IsMasterClient())
        {
            TurnGame.ins.playersBets = new float[TurnGame.ins.turnGameSeats.Count];
            ph.SetRoomData("playersBets", TurnGame.ins.playersBets);

            TurnGame.ins.turnRecievedPlayers = new bool[TurnGame.ins.turnGameSeats.Count];
            ph.SetRoomData("turnRecievedPlayers", TurnGame.ins.turnRecievedPlayers);

            Utils.InvokeDelayedAction(2,()=> 
            {
                if (CardGame.ins.GetNonFoldedPlayersCount() < 2)
                { DeclareWinnerByFoldout(TurnGame.ins.GetNextTurnIndex((int)ph.GetRoomData("turn"))); }
                else
                { ph.SetRoomData("round", (int)ph.GetRoomData("round") + 1); }
            });
        }
    }

    
    public void StartShowDown()
    {
        TurnGame.ins.counter.text = "SHOW DOWN";
    }

    public void DeclareWinnerByFoldout(int winner)
    {
        TurnGame.ins.counter.text = TurnGame.ins.turnGameSeats[winner].playerInfo.username.text + " WON";
        //TurnGame.ins.turnGameSeats[winner].playerInfo.balance.TakeAmount(TurnGame.ins.pot);
    }

    public void PlayerDidntRespodedToTurn()
    {
        string moveName = "FOLD";
        if ((float)ph.GetRoomData("currentBet") == 0) { moveName = "CHECK"; }
        ph.SetRoomData("moveMade", moveName);
    }

    public void CreateCommunityCards(int count)
    {
        int[] newCards = new int[count];
        for (int i = 0; i < count; i++)
        {
            int card = Random.Range(0, CardGame.ins.cards.Count);
            CardGame.ins.cards.Remove(card);
            newCards[i] = card;
        }

        ph.SetRoomData("cards", CardGame.ins.cards.ToArray());
        ph.SetRoomData("newCommunityCards", newCards);
    }

    public void SpawnCommunityCards()
    {
        int[] newCommunityCards = (int[])ph.GetRoomData("newCommunityCards");

        for (int i = 0; i < newCommunityCards.Length; i++)
        {
            communityCards[communityCardsHolder3D.cards.Count] = newCommunityCards[i];
            Deck.ins.CreateNewCard(newCommunityCards[i], communityCardsHolder3D); 
        }
        communityCardsHolder.CopyCards(communityCardsHolder3D);

        ph.SetRoomData("communityCards", communityCards);
    }
}
