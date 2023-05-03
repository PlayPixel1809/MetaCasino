using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Poker : MonoBehaviour
{
    /// <summary>
    /// if a seat becomes folded it will be considered in-active <br/>
    /// if a seat becomes all-in in previous rounds it will be considered in-active <br/>
    /// if a seat becomes all-in in the middle of the present round it will be considered active
    /// </summary>
    public float GetRoundActiveSeatsCount()
    {
        int count = 0;
        for (int i = 0; i < roundActiveSeats.Length; i++)
        {
            if (roundActiveSeats[i]) { count += 1; }
        }
        return count;
    }

    public float GetHighestBet()
    {
        float highestBet = playersBetsForRound[0];
        for (int i = 1; i < playersBetsForRound.Length; i++)
        {
            if (playersBetsForRound[i] > highestBet) { highestBet = playersBetsForRound[i]; }
        }
        return highestBet;
    }

    public int GetLowestAllInSeat()
    {
        int lowestAllInSeat = -1;
        for (int i = 0; i < playersBetsForRound.Length; i++)
        {
            if (playersBetsForRound[i] > 0 && allInPlayers[i]) 
            {
                if (lowestAllInSeat < 0) { lowestAllInSeat = i; } 
                else 
                {
                    if (playersBetsForRound[i] < playersBetsForRound[lowestAllInSeat]) { lowestAllInSeat = i; }
                }
            }
        }
        return lowestAllInSeat;
    }
    
    



    public void RestoreData()
    {
        if (ph.GetRoomData("communityCards") != null)
        {
            int[] communityCardsArray = (int[])ph.GetRoomData("communityCards");
            communityCards = new List<int>();
            for (int i = 0; i < communityCardsArray.Length; i++) { communityCards.Add(communityCardsArray[i]); }
        }

        if (ph.GetRoomData("playersBetsForRound") != null) { playersBetsForRound = (float[])ph.GetRoomData("playersBetsForRound"); }
        if (ph.GetRoomData("roundActiveSeats") != null)    { roundActiveSeats = (bool[])ph.GetRoomData("roundActiveSeats"); }
        if (ph.GetRoomData("allInPlayers") != null)        { allInPlayers = (bool[])ph.GetRoomData("allInPlayers"); }

        if (ph.GetRoomData("sidePotsAmount") != null)
        {
            float[] sidePotsAmountArray = (float[])ph.GetRoomData("sidePotsAmount");
            sidePotsAmount = new List<float>();
            for (int i = 0; i < sidePotsAmountArray.Length; i++) { sidePotsAmount.Add(sidePotsAmountArray[i]); }
        }

        if (ph.GetRoomData("sidePotsSeats") != null)
        {
            string[] sidePotsSeatsArray = (string[])ph.GetRoomData("sidePotsSeats");
            sidePotsSeats = new List<string>();
            for (int i = 0; i < sidePotsSeatsArray.Length; i++) { sidePotsSeats.Add(sidePotsSeatsArray[i]); }
        }

        if (ph.GetRoomData("mainPot") != null)        { mainPot = (float)ph.GetRoomData("mainPot"); }
        if (ph.GetRoomData("mainPotFoldout") != null) { mainPotFoldout = (bool)ph.GetRoomData("mainPotFoldout"); }

        if (ph.GetRoomData("mainPotSeats") != null)
        {
            int[] mainPotSeatsArray = (int[])ph.GetRoomData("mainPotSeats");
            mainPotSeats = new List<int>();
            for (int i = 0; i < mainPotSeatsArray.Length; i++) { mainPotSeats.Add(mainPotSeatsArray[i]); }
        }

        if (ph.GetRoomData("round") != null) { round = (int)ph.GetRoomData("round"); }
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