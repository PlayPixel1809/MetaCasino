using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class  Poker : MonoBehaviour
{
    
    void OnClientMsgRecieved(string completedEvId, ExitGames.Client.Photon.Hashtable data)
    {
        if (completedEvId == "MakeInitialBlinds") { TurnGame.ins.StartTurn(TurnGame.ins.GetNextTurnIndex((int)data["bigBlindIndex"])); }

        if (completedEvId == "MakeMove") { StartNextTurn(); }

        if (completedEvId == "EndRound")
        {
            //mainpot is not created everytime so mainPotSeats.count cannot be used to determine whether game should end or proceed to next round 
            //p1 - 10000, allin, p2 - 10000 allin, p3 - 10000, p4 - 10000 side pot (p1,p2,p3,p4) 40000 
            //p1 - 0 allin, p2 - 0 allin, p3 - 0 , p4 - 0  mainpot(p3,p4) 0, so as main pot amount is zero so mainpot will not be created
            if (TurnGame.ins.GetTurnEligiblePlayersCount() > 1) { StartRound(round + 1); }
            else
            {
                if (mainPotSeats.Count == 1) { ServerClientBridge.ins.HireClients("WinMainPotWithoutShowdown", "mainPotFoldout", mainPotFoldout); }
                else 
                {
                    if (mainPotSeats.Count > 1 || sidePotsSeats.Count > 0) { StartCoroutine("StartShowDowns"); }
                }
            }
        }

        if (completedEvId == "WinMainPotWithoutShowdown")
        {
            if (sidePotsSeats.Count > 0) { StartCoroutine("StartShowDowns"); } else { NetworkGame.ins.GameComplete(); }
        }

        if (completedEvId == "CreateCommunityCards") { CreateCardsBestCombination(); TurnGame.ins.StartFirstTurn(); }

        

        if (completedEvId == "CreateCommunityCardsForShowdown"){ CreateCardsBestCombination(); StartShowDowns(); }

        if (completedEvId == "PrepareForShowDowns") 
        {
            if (mainPotSeats.Count > 1) { ServerClientBridge.ins.HireClients("HighlightShowdownPot","potIndex", -1); }
            else 
            {
                if (sidePotsAmount.Count > 0) { ServerClientBridge.ins.HireClients("HighlightShowdownPot", "potIndex", sidePotsAmount.Count - 1); }
            }
        }

        if (completedEvId == "HighlightShowdownPot") { DeclarePotWinners((int)data["potIndex"]); }

        if (completedEvId == "DeclarePotWinners")
        {
            int potIndex = (int)data["potIndex"];
            if (potIndex == -1)
            {
                if (sidePotsAmount.Count > 0)
                { ServerClientBridge.ins.HireClients("HighlightShowdownPot", "potIndex", sidePotsAmount.Count - 1); }
                else
                { NetworkGame.ins.GameComplete(); }
            }
            else
            {
                if (potIndex > 0) 
                { ServerClientBridge.ins.HireClients("HighlightShowdownPot", "potIndex", potIndex - 1); } 
                else 
                { NetworkGame.ins.GameComplete(); }
            }
        }
    }

    void TurnCompleted(ExitGames.Client.Photon.Hashtable data)
    {
        string moveMade = (string)data["moveMade"];

        if (moveMade == "FOLD")
        {
            roundActiveSeats[TurnGame.ins.turn] = false;
            NetworkRoom.ins.SyncData("roundActiveSeats", roundActiveSeats);
        }

        ExitGames.Client.Photon.Hashtable dataToSync = new ExitGames.Client.Photon.Hashtable();
        if (moveMade == "ALL IN")
        {
            TurnGame.ins.turnEligiblePlayers[TurnGame.ins.turn] = false;
            allInPlayers[TurnGame.ins.turn] = true;
            dataToSync.Add("turnEligiblePlayers", TurnGame.ins.turnEligiblePlayers);
            dataToSync.Add("allInPlayers", allInPlayers);
        }

        if (data.ContainsKey("moveAmount"))
        {
            float amount = (float)data["moveAmount"];

            NetworkGame.ins.playersBets[TurnGame.ins.turn] += amount;
            dataToSync.Add("playersBets", NetworkGame.ins.playersBets);

            playersBetsForRound[TurnGame.ins.turn] += amount;
            dataToSync.Add("playersBetsForRound", playersBetsForRound);
        }

        TurnGame.ins.playersMoves[TurnGame.ins.turn] = moveMade;
        dataToSync.Add("playersMoves", TurnGame.ins.playersMoves);
        NetworkRoom.ins.SyncData(dataToSync);

        data.Add("moveMadeBy", TurnGame.ins.turn);
        ServerClientBridge.ins.HireClients("MakeMove", data);
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