using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Poker : MonoBehaviour
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
    }

    void Start()
    {
        ServerClientBridge.ins.onClientMsgRecieved += OnClientMsgRecieved;

        NetworkGame.ins.onGameStart += () =>
        {
            allInPlayers = new bool[NetworkRoom.ins.seats.Length];
            roundActiveSeats = new bool[NetworkRoom.ins.seats.Length];
            sidePotsSeats = new List<string>();
            sidePotsAmount = new List<float>();
            communityCards = new List<int>();
            NetworkGame.ins.SyncData(new ExitGames.Client.Photon.Hashtable() {  { "allInPlayers", allInPlayers }, { "sidePotsSeats", sidePotsSeats.ToArray() }, { "sidePotsAmount", sidePotsAmount.ToArray() }, 
                { "communityCards", communityCards.ToArray() } } );

        };

        CardGame.ins.onCardsDistributed += ()=> { StartRound(1); };

        TurnGame.ins.onTurn += (s) => 
        {
            //Debug.Log(playersBetsForRound[s]);
            ServerClientBridge.NotifyClient(1, new ExitGames.Client.Photon.Hashtable() { { "evaluatePokerControls", 1 }, { "currentBet", GetHighestBet() }, { "playerBet", playersBetsForRound[s] } });
        };
    }

    void OnClientMsgRecieved(int senderActorNo, ExitGames.Client.Photon.Hashtable hashtable)
    {
        int senderSeatIndex = NetworkRoom.ins.GetSeatIndex(senderActorNo);

        if (hashtable.ContainsKey("moveMade") && (string)hashtable["moveMade"] == "FOLD")
        {
            roundActiveSeats[TurnGame.ins.turn] = false;
            NetworkGame.ins.SyncData("roundActiveSeats", roundActiveSeats);
        }

        if (hashtable.ContainsKey("moveMade"))
        {
            int moveMadeSeatIndex = TurnGame.ins.turn;

            ExitGames.Client.Photon.Hashtable dataToSync = new ExitGames.Client.Photon.Hashtable();
            
            if ((string)hashtable["moveMade"] == "ALL IN")
            {
                TurnGame.ins.turnEligiblePlayers[TurnGame.ins.turn] = false;
                allInPlayers[TurnGame.ins.turn] = true;
                dataToSync.Add("turnEligiblePlayers", TurnGame.ins.turnEligiblePlayers);
                dataToSync.Add("allInPlayers", allInPlayers);
            }

            if (hashtable.ContainsKey("moveAmount"))
            {
                float amount = (float)hashtable["moveAmount"];

                NetworkGame.ins.playersBets[TurnGame.ins.turn] += amount;
                dataToSync.Add("playersBets", NetworkGame.ins.playersBets);

                playersBetsForRound[TurnGame.ins.turn] += amount;
                dataToSync.Add("playersBetsForRound", playersBetsForRound);
            }

            NetworkGame.ins.SyncData(dataToSync);

            hashtable.Add("moveMadeBy", TurnGame.ins.turn);
            ServerClientBridge.NotifyClients(hashtable);

            Utils.InvokeDelayedAction(1, StartNextTurn);
        }
    }


    public void StartRound(int round)
    {
        this.round = round;
        playersBetsForRound = new float[NetworkRoom.ins.seats.Length];
        TurnGame.ins.turnRecievedPlayers = new bool[NetworkRoom.ins.seats.Length];

        for (int i = 0; i < roundActiveSeats.Length; i++) { roundActiveSeats[i] = TurnGame.ins.turnEligiblePlayers[i]; }

        NetworkGame.ins.SyncData(new ExitGames.Client.Photon.Hashtable() { { "round", round }, { "playersBetsForRound", playersBetsForRound }, { "turnRecievedPlayers", TurnGame.ins.turnRecievedPlayers }, 
            { "roundActiveSeats", roundActiveSeats } });

        if (round == 1) 
        {
            int smallBlindIndex = TurnGame.ins.GetNextTurnIndex(TurnGame.ins.dealer);
            int bigBlindIndex = TurnGame.ins.GetNextTurnIndex(smallBlindIndex);

            NetworkGame.ins.playersBets[smallBlindIndex] = NetworkRoom.ins.minBet;
            NetworkGame.ins.playersBets[bigBlindIndex] = NetworkRoom.ins.minBet * 2;
            NetworkGame.ins.SyncData("playersBets", NetworkGame.ins.playersBets);

            playersBetsForRound[smallBlindIndex] = NetworkRoom.ins.minBet;
            playersBetsForRound[bigBlindIndex] = NetworkRoom.ins.minBet * 2;
            NetworkGame.ins.SyncData("playersBetsForRound", playersBetsForRound);

            ServerClientBridge.NotifyClients(new ExitGames.Client.Photon.Hashtable() { { "smallBlindSeat", smallBlindIndex }, { "smallBlindAmount", NetworkRoom.ins.minBet } });
            ServerClientBridge.NotifyClients(new ExitGames.Client.Photon.Hashtable() { { "bigBlindSeat", bigBlindIndex }, { "bigBlindAmount", NetworkRoom.ins.minBet * 2 } });

            TurnGame.ins.StartTurn(TurnGame.ins.GetNextTurnIndex(bigBlindIndex), PlayerDidntRespondedToTurn);
        }
        
        if (round == 2) { CreateCommunityCards(3); }
        if (round == 3) { CreateCommunityCards(1); }
        if (round == 4) { CreateCommunityCards(1); }
        if (round == 5) { StartCoroutine("DeclarePotWinners"); }
    }

    

    public void StartNextTurn()
    {
        //If all players (players that were active or non-allin at the start of the round ) folds then last player should not get turn and should win or enter showdown with allin players of prvious rounds
        if (GetRoundActiveSeatsCount() < 2) { EndRound(); return; }
        

        int nextTurnIndex = TurnGame.ins.GetNextTurnIndex(TurnGame.ins.turn); // gets next non-allin and non-folded seat
        if (nextTurnIndex < 0) { EndRound(); return; }

        if (!TurnGame.ins.turnRecievedPlayers[nextTurnIndex]) { TurnGame.ins.StartTurn(nextTurnIndex, PlayerDidntRespondedToTurn); }
        else
        {
            float nextPlayerBet = playersBetsForRound[nextTurnIndex];
            if (nextPlayerBet < GetHighestBet()) { TurnGame.ins.StartTurn(nextTurnIndex, PlayerDidntRespondedToTurn); } else { EndRound(); }
        }
    }

    public void EndRound() { StartCoroutine("EndRoundCoroutine"); }

    IEnumerator EndRoundCoroutine()
    {
        ServerClientBridge.NotifyClients("endRound", 1);
        Debug.Log("EndRound");
        if (GetHighestBet() > 0) 
        {
            ServerClientBridge.NotifyClients("submitRoundBet", 1);
            yield return new WaitForSeconds(1);
        }

        CreateSidePots();
        CreateMainPot();

        yield return new WaitForSeconds(1);

        //mainpot is not created everytime so mainPotSeats.count cannot be used to detremine whether game should end or procedd to next round 
        //p1 - 10000, allin, p2 - 10000 allin, p3 - 10000 side pot (p1,p2,p3) 30000 
        //p1 - 0, allin, p2 - 0 allin, p3 - 0 , mainpot(p2,p3) 0, so as main pot amount is zero so mainpot will not be created
        if (TurnGame.ins.GetTurnEligiblePlayersCount() > 1)
        { StartRound(round + 1); }
        else
        { StartCoroutine("DeclarePotWinners"); }
    }

    public void CreateSidePots()
    {
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
            if (mainPot > 0)
            {
                potInfo.potAmount += mainPot;
                mainPotSeats.Clear();  
                mainPot = 0;
                mainPotFoldout = false;
            }
            sidePotsAmount.Add(potInfo.potAmount);
            sidePotsSeats.Add(potInfo.showdownEligibleSeatsString);

            NetworkGame.ins.SyncData(new ExitGames.Client.Photon.Hashtable() { { "mainPot", mainPot }, { "mainPotFoldout", mainPotFoldout }, { "sidePotsSeats", sidePotsSeats.ToArray() }, { "sidePotsAmount", sidePotsAmount.ToArray() }});
            ServerClientBridge.NotifyClients(new ExitGames.Client.Photon.Hashtable() { { "sidePotAmount", potInfo.potAmount }, { "sidePotSeats", potInfo.showdownEligibleSeats.ToArray() } });
        }
    }

    //All the bets that dosent goes into the side pots goes into the main pot
    public void CreateMainPot()
    {
        float highestBet = GetHighestBet();

        if (highestBet > 0)
        {
            PotInfo potInfo = CreatePot(highestBet);
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
        if (mainPot > 0) 
        {
            NetworkGame.ins.SyncData(new ExitGames.Client.Photon.Hashtable() { { "mainPotAmount", mainPot }, { "mainPotSeats", mainPotSeats.ToArray() }, { "mainPotFoldout", mainPotFoldout } });
            ServerClientBridge.NotifyClients(new ExitGames.Client.Photon.Hashtable() { { "mainPotAmount", mainPot }, { "mainPotSeats", mainPotSeats.ToArray() } });
        }
    }

    public IEnumerator DeclarePotWinners()
    {
        if (mainPotSeats.Count == 1)
        {
            ServerClientBridge.NotifyClients(new ExitGames.Client.Photon.Hashtable() { { "takeMainPotAmount", 1 }, { "mainPotFoldout", mainPotFoldout } });
            if (mainPotFoldout)  { yield return new WaitForSeconds(4); }
            if (!mainPotFoldout) { yield return new WaitForSeconds(1); }
        }

        if (mainPotSeats.Count > 1 || sidePotsSeats.Count > 0)
        { StartCoroutine("StartShowDowns"); }
        else 
        { ServerClientBridge.NotifyClients("gameComplete", 1); }
    }

    
    IEnumerator StartShowDowns()
    {
        if (communityCards.Count < 5)
        {
            ServerClientBridge.NotifyClients("showDownCommunityCards", 1);
            CreateCommunityCards(5 - communityCards.Count, true);
            yield return new WaitForSeconds(3);
        }

        Debug.Log("StartShowDowns");
        ServerClientBridge.NotifyClients("startShowDowns", 1);

        for (int i = 0; i < CardGame.ins.playersCards.Length; i++)
        {
            if (CardGame.ins.playersCards[i] != "Null"){ CardGame.ins.playersCards[i] = PokerHands.GetBestFiveCardsCombination(communityCards, CardGame.ins.GetPlayerCards(i)); }
        }
        NetworkGame.ins.SyncData("playersCards", CardGame.ins.playersCards);

        yield return new WaitForSeconds(2);

        if (mainPotSeats.Count > 1) 
        {
            StartShowDown(-1);
            yield return new WaitForSeconds(6);
        }

        for (int i = sidePotsAmount.Count - 1; i > -1; i--)
        {
            StartShowDown(i);
            yield return new WaitForSeconds(6);
        }

        ServerClientBridge.NotifyClients("gameComplete", 1);
    }
    
    public void StartShowDown(int potIndex)
    {
        PokerHands.WinInfo winInfo = null;
        if (potIndex == -1) 
        { winInfo = PokerHands.GetWinningSeats(mainPotSeats); } 
        else 
        { winInfo = PokerHands.GetWinningSeats(sidePotsSeats[potIndex]);}

        ServerClientBridge.NotifyClients(new ExitGames.Client.Photon.Hashtable() { { "startShowDown", 1 }, { "potIndex", potIndex }, { "winningSeats", winInfo.winners.ToArray() },
            { "winType", winInfo.winType.ToString() }, { "winningCards", winInfo.winningCards.ToArray() } });
    }

    public void PlayerDidntRespondedToTurn()
    {
        string moveName = "FOLD";
        if (GetHighestBet() == 0) { moveName = "CHECK"; }

        ServerClientBridge.ins.onClientMsgRecieved(NetworkRoom.ins.seats[TurnGame.ins.turn], new ExitGames.Client.Photon.Hashtable() { { "moveMade", moveName } }   );
    }

    public void CreateCommunityCards(int count, bool forShowdown = false)
    {
        List<int> newCards = CardGame.ins.GetCards(count);
        communityCards.AddRange(newCards);

        NetworkGame.ins.SyncData("communityCards", communityCards.ToArray());
        ServerClientBridge.NotifyClients(new ExitGames.Client.Photon.Hashtable() { { "newCommunityCards", newCards.ToArray() }, { "forShowdown", forShowdown } });

        Utils.InvokeDelayedAction(2, () => { if (!forShowdown) { TurnGame.ins.StartFirstTurn(PlayerDidntRespondedToTurn); }});
    }

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
        }
        return potInfo;
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