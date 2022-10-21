using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Poker : MonoBehaviour
{
    public static Poker ins;
    void Awake() { ins = this; }

    public CardsHolder communityCardsHolder;
    public CardsHolder communityCardsHolder3D;

    [Header("Assigned During Game -")]
    public List<int> communityCards;
    public float[] playersBetsForRound;

    public bool[] roundActiveSeats;
    public bool[] allInPlayers;
    public List<float> sidePotsAmount;
    public List<string> sidePotsSeats;
    public float mainPot;
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
            allInPlayers = new bool[NetworkGame.ins.seats.Length];
            roundActiveSeats = new bool[NetworkGame.ins.seats.Length];
            sidePotsSeats = new List<string>();
            sidePotsAmount = new List<float>();
            communityCards = new List<int>();
            NetworkGame.ins.SyncData(new ExitGames.Client.Photon.Hashtable() {  { "allInPlayers", allInPlayers }, { "sidePotsSeats", sidePotsSeats.ToArray() }, { "sidePotsAmount", sidePotsAmount.ToArray() }, 
                { "communityCards", communityCards.ToArray() } } );
        };

        CardGame.ins.onCardsDistributed += ()=> { StartRound(1); };

        TurnGame.ins.onTurn += (s) => 
        {
            int actorNo = NetworkGame.ins.seats[s];
            if (actorNo > 0)
            { 
                ServerClientBridge.NotifyClient(actorNo, new ExitGames.Client.Photon.Hashtable() { { "evaluatePokerControls", 1 }, { "currentBet", GetHighestBet() }, { "playerBet", playersBetsForRound[s] } }); 
            }
        };
    }

    void OnClientMsgRecieved(int senderActorNo, ExitGames.Client.Photon.Hashtable hashtable)
    {
        int senderSeatIndex = NetworkGame.ins.GetSeatIndex(senderActorNo);

        if (hashtable.ContainsKey("moveMade") && (string)hashtable["moveMade"] == "Fold")
        {
            roundActiveSeats[NetworkGame.ins.GetSeatIndex(senderActorNo)] = false;
            NetworkGame.ins.SyncData("roundActiveSeats", roundActiveSeats);
        }

        if (hashtable.ContainsKey("moveMade"))
        {
            ExitGames.Client.Photon.Hashtable dataToSync = new ExitGames.Client.Photon.Hashtable();
            
            if ((string)hashtable["moveMade"] == "AllIn")
            {
                TurnGame.ins.turnEligiblePlayers[senderSeatIndex] = false;
                allInPlayers[senderSeatIndex] = true;
                dataToSync.Add("allInPlayers", allInPlayers);
                dataToSync.Add("turnEligiblePlayers", TurnGame.ins.turnEligiblePlayers);
            }

            if (hashtable.ContainsKey("moveAmount"))
            {
                float amount = (float)hashtable["moveAmount"];

                NetworkGame.ins.playersBets[senderSeatIndex] += amount;
                dataToSync.Add("playersBets", NetworkGame.ins.playersBets);

                playersBetsForRound[senderSeatIndex] += amount;
                dataToSync.Add("playersBetsForRound", playersBetsForRound);
            }

            NetworkGame.ins.SyncData(dataToSync);

            hashtable.Add("moveMadeBy", senderActorNo);
            ServerClientBridge.NotifyClients(hashtable);

            Utils.InvokeDelayedAction(1, StartNextTurn);
        }
    }


    public void StartRound(int round)
    {
        this.round = round;
        playersBetsForRound = new float[NetworkGame.ins.seats.Length];
        TurnGame.ins.turnRecievedPlayers = new bool[NetworkGame.ins.seats.Length];

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

            ServerClientBridge.NotifyClients(new ExitGames.Client.Photon.Hashtable() { { "smallBlindSeat", smallBlindIndex }, { "smallBlindAmount", NetworkRoom.ins.minBet } });
            ServerClientBridge.NotifyClients(new ExitGames.Client.Photon.Hashtable() { { "bigBlindSeat", bigBlindIndex }, { "bigBlindAmount", NetworkRoom.ins.minBet * 2 } });

            TurnGame.ins.StartTurn(TurnGame.ins.GetNextTurnIndex(bigBlindIndex), PlayerDidntRespondedToTurn);
        }
        
        if (round == 2) { CreateCommunityCards(3, true); }
        if (round == 3) { CreateCommunityCards(1, true); }
        if (round == 4) { CreateCommunityCards(1, true); }
        if (round == 5) { StartShowDowns(); }
    }

    
    public void StartNextTurn()
    {
        if (GetRoundActiveSeatsCount() < 2) { EndRound(); return; }
        //if a seat becomes folded it will be considered in-active 
        //if a seat becomes all-in in previous rounds it will be considered in-active 
        //if a seat becomes all-in in the middle of the present round it will be considered active

        int nextTurnIndex = TurnGame.ins.GetNextTurnIndex(TurnGame.ins.turn); // gets next non-allin and non-folded seat
        if (nextTurnIndex < 0) { EndRound(); return; }

        if (!TurnGame.ins.turnRecievedPlayers[nextTurnIndex]) { TurnGame.ins.StartTurn(nextTurnIndex, PlayerDidntRespondedToTurn); }
        else
        {
            float nextPlayerBet = playersBetsForRound[nextTurnIndex];
            if (nextPlayerBet < GetHighestBet()) { TurnGame.ins.StartTurn(nextTurnIndex, PlayerDidntRespondedToTurn); } else { EndRound(); }
        }
    }

    public void EndRound()
    {
        CreateSidePots();
        CreateMainPot();

        if (mainPotSeats.Count > 1) { StartRound(round + 1); } else { StartShowDowns(); }
    }

    public void CreateSidePots()
    {
        while (true)
        {
            int lowestAllInSeat = GetLowestAllInSeat();
            if (lowestAllInSeat < 0) { break; }
            if (GetShowdownEligibleSeatsCount().Count < 2) { break; }

            PotInfo potInfo = CreatePot(playersBetsForRound[lowestAllInSeat]);
            if (mainPot > 0)
            {
                potInfo.potAmount += mainPot;
                mainPot = 0;
            }
            sidePotsAmount.Add(potInfo.potAmount);
            sidePotsSeats.Add(potInfo.showdownEligibleSeatsString);

            NetworkGame.ins.SyncData(new ExitGames.Client.Photon.Hashtable() { { "mainPot", mainPot }, { "sidePotsSeats", sidePotsSeats.ToArray() }, { "sidePotsAmount", sidePotsAmount.ToArray() } });
            ServerClientBridge.NotifyClients(new ExitGames.Client.Photon.Hashtable() { { "sidePotAmount", potInfo.potAmount }, { "sidePotSeats", potInfo.showdownEligibleSeats.ToArray() } });
        }
    }

    public void CreateMainPot()
    {
        PotInfo potInfo = CreatePot(GetHighestBet());
        mainPot = potInfo.potAmount;
        mainPotSeats = potInfo.showdownEligibleSeats;

        NetworkGame.ins.SyncData(new ExitGames.Client.Photon.Hashtable() { { "mainPotAmount", mainPot }, { "mainPotSeats", mainPotSeats.ToArray() } });
        ServerClientBridge.NotifyClients(new ExitGames.Client.Photon.Hashtable() { { "mainPotAmount", mainPot }, { "mainPotSeats", mainPotSeats.ToArray() } });
    }

    IEnumerator StartShowDowns()
    {
        for (int i = 0; i < CardGame.ins.playersCards.Length; i++)
        {
            if (!CardGame.ins.foldedPlayers[i]) { CardGame.ins.playersCards[i] = PokerHands.GetBestFiveCardsCombination(communityCards, CardGame.ins.GetPlayerCards(i)) ; }
        }
        NetworkGame.ins.SyncData("playersCards", CardGame.ins.playersCards);

        if (mainPotSeats.Count > 0) 
        {
            StartShowDown(-1);
            yield return new WaitForSeconds(3);
        }

        for (int i = sidePotsAmount.Count - 1; i > -1; i--)
        {
            StartShowDown(i);
            yield return new WaitForSeconds(3);
        }
    }

    public void StartShowDown(int potIndex)
    {
        PokerHands.WinningSeats winningSeats = null;
        if (potIndex < -1 && mainPotSeats.Count > 1) { winningSeats = PokerHands.EvaluatePot(mainPotSeats); } else { winningSeats = PokerHands.EvaluatePot(sidePotsSeats[potIndex]);}

        ServerClientBridge.NotifyClients(new ExitGames.Client.Photon.Hashtable() { { "potIndex", potIndex }, { "winningPlayers", winningSeats.winners.ToArray() }, { "winType", winningSeats.winType.ToString() } });
    }

    public void PlayerDidntRespondedToTurn()
    {
        string moveName = "FOLD";
        if (GetHighestBet() == 0) { moveName = "CHECK"; }

        ServerClientBridge.ins.onClientMsgRecieved(NetworkGame.ins.seats[TurnGame.ins.turn], new ExitGames.Client.Photon.Hashtable() { { "moveMade", moveName } }   );
    }

    public void CreateCommunityCards(int count, bool startTurn)
    {
        List<int> newCards = CardGame.ins.GetCards(count);
        communityCards.AddRange(newCards);

        NetworkGame.ins.SyncData("communityCards", communityCards.ToArray());
        ServerClientBridge.NotifyClients("newCommunityCards", newCards.ToArray());

        Utils.InvokeDelayedAction(2, () => { if (startTurn) { TurnGame.ins.GetNextTurnIndex(TurnGame.ins.dealer); }});
    }

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
        for (int i = 1; i < playersBetsForRound.Length; i++)
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

    public List<int> GetShowdownEligibleSeatsCount()
    {
        List<int> betsHigherThanZero = new List<int>();
        for (int i = 1; i < playersBetsForRound.Length; i++)
        {
            if (!CardGame.ins.foldedPlayers[i] && playersBetsForRound[i] > 0 ) { betsHigherThanZero.Add(i); }
        }
        return betsHigherThanZero;
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
