using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Poker : MonoBehaviour
{
    public void StartRound(int round)
    {
        this.round = round;
        playersBetsForRound = new float[NetworkRoom.ins.seats.Length];
        TurnGame.ins.turnRecievedPlayers = new bool[NetworkRoom.ins.seats.Length];

        
        for (int i = 0; i < TurnGame.ins.playersMoves.Length; i++) 
        { 
            if (TurnGame.ins.playersMoves[i].ToLower() == "fold" || TurnGame.ins.playersMoves[i].ToLower() == "all in") { continue; }
            TurnGame.ins.playersMoves[i] = string.Empty;
        }
        for (int i = 0; i < roundActiveSeats.Length; i++) { roundActiveSeats[i] = TurnGame.ins.turnEligiblePlayers[i]; }

        NetworkRoom.ins.SyncData(new ExitGames.Client.Photon.Hashtable() { { "round", round }, { "playersBetsForRound", playersBetsForRound }, { "turnRecievedPlayers", TurnGame.ins.turnRecievedPlayers }, 
            { "roundActiveSeats", roundActiveSeats }, { "playersMoves", TurnGame.ins.playersMoves } });

        if (round == 1) 
        {
            int smallBlindIndex = TurnGame.ins.GetNextTurnIndex(TurnGame.ins.dealer);
            int bigBlindIndex = TurnGame.ins.GetNextTurnIndex(smallBlindIndex);

            TurnGame.ins.playersMoves[smallBlindIndex] = "S-BLIND";
            TurnGame.ins.playersMoves[bigBlindIndex] = "B-BLIND";
            NetworkRoom.ins.SyncData("playersMoves", TurnGame.ins.playersMoves);

            NetworkGame.ins.playersBets[smallBlindIndex] = NetworkGame.ins.minBet;
            NetworkGame.ins.playersBets[bigBlindIndex] = NetworkGame.ins.minBet * 2;
            NetworkRoom.ins.SyncData("playersBets", NetworkGame.ins.playersBets);

            playersBetsForRound[smallBlindIndex] = NetworkGame.ins.minBet;
            playersBetsForRound[bigBlindIndex] = NetworkGame.ins.minBet * 2;
            NetworkRoom.ins.SyncData("playersBetsForRound", playersBetsForRound);

            ServerClientBridge.ins.HireClients("MakeInitialBlinds", new ExitGames.Client.Photon.Hashtable() 
            {
                { "smallBlindIndex", smallBlindIndex},
                { "bigBlindIndex", bigBlindIndex}
            });
        }
        
        if (round == 2) { CreateCommunityCards(3); }
        if (round == 3) { CreateCommunityCards(1); }
        if (round == 4) { CreateCommunityCards(1); }
        if (round == 5) { StartShowDowns(); }
    }

    

    public void StartNextTurn()
    {
        //If all players (players that were active or non-allin at the start of the round ) folds then last player should not get turn and should win or enter showdown with allin players of previous rounds
        if (GetRoundActiveSeatsCount() < 2) { EndRound(); return; }
        

        int nextTurnIndex = TurnGame.ins.GetNextTurnIndex(TurnGame.ins.turn); // gets next non-allin and non-folded seat
        if (nextTurnIndex < 0) { EndRound(); return; }

        if (!TurnGame.ins.turnRecievedPlayers[nextTurnIndex]) { TurnGame.ins.StartTurn(nextTurnIndex); }
        else
        {
            float nextPlayerBet = playersBetsForRound[nextTurnIndex];
            if (nextPlayerBet < GetHighestBet()) { TurnGame.ins.StartTurn(nextTurnIndex); } else { EndRound(); }
        }
    }

    public void EndRound() 
    {
        DataUtils dataUtils = new DataUtils();

        for (int i = 0; i < TurnGame.ins.playersMoves.Length; i++)
        {
            if (TurnGame.ins.playersMoves[i].ToLower() != "fold" && TurnGame.ins.playersMoves[i].ToLower() != "all in") { TurnGame.ins.playersMoves[i] = "Null"; }
        }
        dataUtils.AddDataForClients("playersMoves", TurnGame.ins.playersMoves);

        if (GetHighestBet() > 0)  { dataUtils.AddDataForClients("submitRoundBet", 1); }

        dataUtils.MergeData(CreateSidePots());
        dataUtils.MergeData(CreateMainPot());

        NetworkRoom.ins.SyncData(dataUtils.GetDataToSyncHashtable());
        ServerClientBridge.ins.HireClients("EndRound", dataUtils.GetDataForClientsHashtable());
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