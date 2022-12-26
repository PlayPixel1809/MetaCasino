using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class PokerHands 
{
    public enum WinTypes
    {
        None, RoyalFlush, StraightFlush, FourOfAKind, FullHouse, Flush, Straight, ThreeOfAKind, ThreeOfAKindHighcard, TwoPairs, TwoPairsHighCard,
        Pairs, PairsHighCard, HighCard
    };

    private static WinTypes winType;

    [System.Serializable]
    public class WinningHands
    {
        public WinTypes winType;
        public List<List<int>> winners;
    }

    [System.Serializable]
    public class WinInfo
    {
        public WinTypes winType;
        public List<int> winners = new List<int>();
        public List<int> winningCards = new List<int>();
    }

    public static WinInfo GetWinningSeats(List<int> seats)
    {
        List<List<int>> hands = new List<List<int>>();
        for (int i = 0; i < seats.Count; i++) 
        {
            if (!CardGame.ins.foldedPlayers[seats[i]]) { hands.Add(CardGame.ins.GetPlayerCards(seats[i])); } 
        }

        WinningHands winningHands = GetWinningHands(hands);
        WinInfo winInfo = new WinInfo();

        for (int i = 0; i < winningHands.winners.Count; i++)
        {
            int winningHandIndex = hands.IndexOf(winningHands.winners[i]);
            winInfo.winningCards.AddRange(winningHands.winners[i]);
            winInfo.winners.Add(seats[winningHandIndex]);
        }
        winInfo.winType = winningHands.winType;
        return winInfo;
    }

    public static WinInfo GetWinningSeats(string seats)
    {
        string[] seatsArray = seats.Split(new string[1] { "," }, StringSplitOptions.RemoveEmptyEntries);
        List<int> seatsList = new List<int>();
        for (int i = 0; i < seatsArray.Length; i++) { seatsList.Add(int.Parse( seatsArray[i])); }
        return GetWinningSeats(seatsList);
    }


    public static string GetBestFiveCardsCombination (List<int> communityCards, List<int> holdemHand)
    {
        List<List<int>> possibleHands = new List<List<int>>()
        {
            new List<int>(){ communityCards[0], communityCards[1], communityCards[2], communityCards[3], communityCards[4] },
            new List<int>(){ communityCards[0], communityCards[1], communityCards[2], holdemHand[0], holdemHand[1] },
            new List<int>(){ communityCards[1], communityCards[2], communityCards[3], holdemHand[0], holdemHand[1] },
            new List<int>(){ communityCards[2], communityCards[3], communityCards[4], holdemHand[0], holdemHand[1] },
            new List<int>(){ communityCards[0], communityCards[1], communityCards[3], holdemHand[0], holdemHand[1] },
            new List<int>(){ communityCards[0], communityCards[1], communityCards[4], holdemHand[0], holdemHand[1] },
            new List<int>(){ communityCards[1], communityCards[2], communityCards[4], holdemHand[0], holdemHand[1] },
            new List<int>(){ communityCards[1], communityCards[3], communityCards[4], holdemHand[0], holdemHand[1] },
            new List<int>(){ communityCards[0], communityCards[2], communityCards[3], holdemHand[0], holdemHand[1] },
            new List<int>(){ communityCards[0], communityCards[2], communityCards[4], holdemHand[0], holdemHand[1] },
            new List<int>(){ communityCards[0], communityCards[3], communityCards[4], holdemHand[0], holdemHand[1] },

            new List<int>(){ holdemHand[0],     communityCards[1], communityCards[2], communityCards[3], communityCards[4] },
            new List<int>(){ communityCards[0], holdemHand[0],     communityCards[2], communityCards[3], communityCards[4] },
            new List<int>(){ communityCards[0], communityCards[1], holdemHand[0],     communityCards[3], communityCards[4] },
            new List<int>(){ communityCards[0], communityCards[1], communityCards[2], holdemHand[0],     communityCards[4] },
            new List<int>(){ communityCards[0], communityCards[1], communityCards[2], communityCards[3], holdemHand[0] },
            new List<int>(){ holdemHand[1],     communityCards[1], communityCards[2], communityCards[3], communityCards[4] },
            new List<int>(){ communityCards[0], holdemHand[1],     communityCards[2], communityCards[3], communityCards[4] },
            new List<int>(){ communityCards[0], communityCards[1], holdemHand[1],     communityCards[3], communityCards[4] },
            new List<int>(){ communityCards[0], communityCards[1], communityCards[2], holdemHand[1],     communityCards[4] },
            new List<int>(){ communityCards[0], communityCards[1], communityCards[2], communityCards[3], holdemHand[1] },
        };

        WinningHands winningHands = GetWinningHands(possibleHands);

        string cards = string.Empty;
        for (int i = 0; i < 5; i++)
        {
            cards += winningHands.winners[0][i] + ",";
        }

        return cards;
    }

    
    static WinningHands GetWinningHands(List<List<int>> hands)
    {
        if (hands.Count == 1) { return new WinningHands() { winType = WinTypes.None, winners = hands }; }

        List<CardGameHand> cardGameHands = new List<CardGameHand>();
        for (int i = 0; i < hands.Count; i++) { cardGameHands.Add(new CardGameHand(hands[i])); }

        CardGameHand bestHand = cardGameHands[0];
        WinningHands winner = new WinningHands() { winners = new List<List<int>>() { bestHand.cardsIndexes } };
        for (int i = 1; i < cardGameHands.Count; i++)
        {
            CardGameHand newBestHand = CompareHands(bestHand, cardGameHands[i]);
            if (newBestHand == cardGameHands[i])
            {
                winner.winners.Clear();
                bestHand = cardGameHands[i];
                winner.winners = new List<List<int>>() { bestHand.cardsIndexes };
            }

            if (newBestHand == null) { winner.winners.Add(cardGameHands[i].cardsIndexes); }
            winner.winType =  winType;
        }

        return winner;
    }

    static CardGameHand CompareHands(CardGameHand handA, CardGameHand handB)
    {
        winType = WinTypes.RoyalFlush;
        if (handA.GetSequenceInSameColor() == 14) { return handA; }
        if (handB.GetSequenceInSameColor() == 14) { return handB; }

        winType = WinTypes.StraightFlush;
        int handAStraightFlushIndex = handA.GetSequenceInSameColor();
        int handBStraightFlushIndex = handB.GetSequenceInSameColor();
        if (handAStraightFlushIndex > handBStraightFlushIndex) { return handA; }
        if (handBStraightFlushIndex > handAStraightFlushIndex) { return handB; }

        winType = WinTypes.FourOfAKind;
        int handAFourOfAKindIndex = handA.GetHighestFourOfAKind();
        int handBFourOfAKindIndex = handB.GetHighestFourOfAKind();
        if (handAFourOfAKindIndex > handBFourOfAKindIndex) { return handA; }
        if (handBFourOfAKindIndex > handAFourOfAKindIndex) { return handB; }

        winType = WinTypes.FullHouse;
        List<int> handAFullHouseStatus = IsFullHouse(handA);
        List<int> handBFullHouseStatus = IsFullHouse(handB);
        if (handAFullHouseStatus.Count == 2 && handBFullHouseStatus.Count == 2) { return GetHigherRankingHand(handA, handB, handAFullHouseStatus, handBFullHouseStatus); }
        if (handAFullHouseStatus.Count == 2 && handBFullHouseStatus.Count != 2) { return handA; }
        if (handAFullHouseStatus.Count != 2 && handBFullHouseStatus.Count == 2) { return handB; }

        winType = WinTypes.Flush;
        bool isHandAFlush = handA.AreCardsSameColor();
        bool isHandBFlush = handB.AreCardsSameColor();
        if (isHandAFlush && isHandBFlush) { return GetHigherRankingHand(handA, handB); }
        if (isHandAFlush) { return handA; }
        if (isHandBFlush) { return handB; }

        winType = WinTypes.Straight;
        int handAStraightIndex = handA.GetSequence();
        int handBStraightIndex = handB.GetSequence();
        if (handAStraightIndex > handBStraightIndex) { return handA; }
        if (handBStraightIndex > handAStraightIndex) { return handB; }
        if (handBStraightIndex == handAStraightIndex && handAStraightIndex > 0) { return null; }

        winType = WinTypes.ThreeOfAKind;
        int handAThreeOfAKindIndex = handA.GetHighestThreeOfAKind();
        int handBThreeOfAKindIndex = handB.GetHighestThreeOfAKind();
        if (handAThreeOfAKindIndex > handBThreeOfAKindIndex) { return handA; }
        if (handBThreeOfAKindIndex > handAThreeOfAKindIndex) { return handB; }
        if (handAThreeOfAKindIndex == handBThreeOfAKindIndex && handAThreeOfAKindIndex > 0)
        {
            winType = WinTypes.ThreeOfAKindHighcard;
            return GetHigherRankingHand(handA, handB, handA.oneOfKinds, handB.oneOfKinds);
        }

        winType = WinTypes.TwoPairs;
        List<int> handATwoPairsStatus = handA.GetPairs();
        List<int> handBTwoPairsStatus = handB.GetPairs();
        if (handATwoPairsStatus.Count == 2 && handBTwoPairsStatus.Count == 2)
        {
            CardGameHand winner = GetHigherRankingHand(handA, handB, handATwoPairsStatus, handBTwoPairsStatus);
            if (winner != null) { return winner; }
            winType = WinTypes.TwoPairsHighCard;
            return GetHigherRankingHand(handA, handB, handA.oneOfKinds, handB.oneOfKinds);
        }
        if (handATwoPairsStatus.Count == 2 && handBTwoPairsStatus.Count != 2) { return handA; }
        if (handATwoPairsStatus.Count != 2 && handBTwoPairsStatus.Count == 2) { return handB; }

        winType = WinTypes.Pairs;
        List<int> handAPairsStatus = handA.GetPairs();
        List<int> handBPairsStatus = handB.GetPairs();
        if (handAPairsStatus.Count == 1 && handBPairsStatus.Count == 1)
        {
            CardGameHand winner = GetHigherRankingHand(handA, handB, handAPairsStatus, handBPairsStatus);
            if (winner != null) { return winner; }
            winType = WinTypes.PairsHighCard;
            return GetHigherRankingHand(handA, handB, handA.oneOfKinds, handB.oneOfKinds);
        }
        if (handAPairsStatus.Count == 1 && handBPairsStatus.Count != 1) { return handA; }
        if (handAPairsStatus.Count != 1 && handBPairsStatus.Count == 1) { return handB; }

        winType = WinTypes.HighCard;
        return GetHigherRankingHand(handA, handB);
    }


    static CardGameHand GetHigherRankingHand(CardGameHand hand1, CardGameHand hand2, List<int> cards1 = null, List<int> cards2 = null)
    {
        if (cards1 == null) { cards1 = hand1.cardsNo; }
        if (cards2 == null) { cards2 = hand2.cardsNo; }

        if (cards1[0] > cards2[0]) { return hand1; }
        if (cards2[0] > cards1[0]) { return hand2; }
        if (cards1[0] == cards2[0] && cards1.Count > 1)
        {
            if (cards1[1] > cards2[1]) { return hand1; }
            if (cards2[1] > cards1[1]) { return hand2; }
            if (cards1[1] == cards2[1] && cards1.Count > 2)
            {
                if (cards1[2] > cards2[2]) { return hand1; }
                if (cards2[2] > cards1[2]) { return hand2; }
                if (cards1[2] == cards2[2] && cards1.Count > 3)
                {
                    if (cards1[3] > cards2[3]) { return hand1; }
                    if (cards2[3] > cards1[3]) { return hand2; }
                    if (cards1[3] == cards2[3] && cards1.Count > 4)
                    {
                        if (cards1[4] > cards2[4]) { return hand1; }
                        if (cards2[4] > cards1[4]) { return hand2; }
                    }
                }
            }
        }
        return null;
    }



    static List<int> IsFullHouse(CardGameHand cardGameHand)
    {
        List<int> fullHouse = new List<int>();
        if (cardGameHand.GetHighestThreeOfAKind() != 0) { fullHouse.Add(cardGameHand.GetHighestThreeOfAKind()); }
        if (cardGameHand.GetPairs().Count > 0) { fullHouse.Add(cardGameHand.GetPairs()[0]); }
        return fullHouse;
    }





}
