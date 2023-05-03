using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Poker : MonoBehaviour
{

    DataUtils CreateSidePots()
    {
        DataUtils dataUtils = new DataUtils();
        List<float> roundSidePotsAmount = new List<float>();
        List<string> roundSidePotsSeats = new List<string>();
        while (true)
        {
            int lowestAllInSeat = GetLowestAllInSeat();
            if (lowestAllInSeat < 0) { break; }
            float lowestAllInAmount = playersBetsForRound[lowestAllInSeat];
            
            int playersContainingLowestAllInAmountCount = 0;
            for (int i = 0; i < playersBetsForRound.Length; i++)
            {
                if (!CardGame.ins.foldedPlayers[i] && playersBetsForRound[i] >= lowestAllInAmount) { playersContainingLowestAllInAmountCount += 1; }
            }
            if (playersContainingLowestAllInAmountCount < 2) { break; }

            PotInfo potInfo = CreatePot(playersBetsForRound[lowestAllInSeat]);
            dataUtils.MergeData(potInfo.dataUtils);
            if (mainPot > 0)
            {
                potInfo.potAmount += mainPot;
                mainPotSeats.Clear();  
                mainPot = 0;
                mainPotFoldout = false;
                dataUtils.AddDataToSync(new List<string>() { "mainPot", "mainPotFoldout" }, new List<object>() { mainPot , mainPotFoldout });
            }

            roundSidePotsAmount.Add(potInfo.potAmount);
            roundSidePotsSeats.Add(potInfo.showdownEligibleSeatsString);
        }
        
        if (roundSidePotsAmount.Count > 0)
        {
            sidePotsAmount.AddRange(roundSidePotsAmount);
            sidePotsSeats.AddRange(roundSidePotsSeats);

            dataUtils.AddDataToSync(new List<string>() { "sidePotsAmount", "sidePotsSeats" }, new List<object>() { sidePotsAmount.ToArray(), sidePotsSeats.ToArray() });
            dataUtils.AddDataForClients(new List<string>() { "sidePotsAmount", "sidePotsSeats" }, new List<object>() { roundSidePotsAmount.ToArray(), roundSidePotsSeats.ToArray() });
        }
        return dataUtils;
    }

    //All the bets that dosent goes into the side pots goes into the main pot
    DataUtils CreateMainPot()
    {
        DataUtils dataUtils = new DataUtils();

        float highestBet = GetHighestBet();

        if (highestBet > 0)
        {
            PotInfo potInfo = CreatePot(highestBet);
            dataUtils.MergeData(potInfo.dataUtils);
            mainPot += potInfo.potAmount;
            mainPotSeats = potInfo.showdownEligibleSeats;
        }
        else
        {
            List<int> newSeats = new List<int>();
            for (int i = 0; i < mainPotSeats.Count; i++)
            {
                if (!CardGame.ins.IsSeatFolded(mainPotSeats[i])) { newSeats.Add(mainPotSeats[i]); }
            }
            mainPotSeats = newSeats; 
        }

        if (GetRoundActiveSeatsCount() == 1) { mainPotFoldout = true; }
        
        dataUtils.AddDataToSync(new List<string>() { "mainPot", "mainPotSeats" }, new List<object>() { mainPot, mainPotSeats.ToArray() });
        dataUtils.AddDataForClients(new List<string>() { "mainPotAmount", "mainPotSeats" }, new List<object>() { mainPot, mainPotSeats.ToArray() });
        
        return dataUtils;
    }


    public PotInfo CreatePot(float amountToTake)
    {
        PotInfo potInfo = new PotInfo();
        for (int i = 0; i < playersBetsForRound.Length; i++)
        {
            if (playersBetsForRound[i] == 0) { continue; }
            float amountToAdd = amountToTake;
            if (playersBetsForRound[i] < amountToTake) { amountToAdd = playersBetsForRound[i]; }

            playersBetsForRound[i] -= amountToAdd;
            potInfo.potAmount += amountToAdd;

            if (!CardGame.ins.foldedPlayers[i])
            {
                potInfo.showdownEligibleSeats.Add(i);
                potInfo.showdownEligibleSeatsString += i + ",";
            }
            potInfo.dataUtils.AddDataToSync("playersBetsForRound", playersBetsForRound);
        }
        return potInfo;
    }


    public void DeclarePotWinners(int potIndex)
    {
        PokerHands.WinInfo winInfo;

        if (potIndex == -1)
        { winInfo = PokerHands.GetWinningSeats(mainPotSeats); }
        else
        { winInfo = PokerHands.GetWinningSeats(sidePotsSeats[potIndex]); }

        ServerClientBridge.ins.HireClients("DeclarePotWinners", new ExitGames.Client.Photon.Hashtable() { { "potIndex", potIndex }, { "winningSeats", winInfo.winners.ToArray() },
            { "winType", winInfo.winType.ToString() }, { "winningCards", winInfo.winningCards.ToArray() } });
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