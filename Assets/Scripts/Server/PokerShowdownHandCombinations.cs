using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PokerShowdownHandCombinations 
{
    public static List<List<int>> GetHoldemCombinations(List<int> holdemHand)
    {
        List<List<int>> combinations = new List<List<int>>();

        if (Poker.ins.communityCards.Count == 5) { combinations.Add(Poker.ins.communityCards); }
        if (Poker.ins.communityCards.Count >  3) { combinations.AddRange(GetCombinations(holdemHand[0])); }
        if (Poker.ins.communityCards.Count >  3) { combinations.AddRange(GetCombinations(holdemHand[1])); }
        combinations.AddRange(GetCombinations(holdemHand[0], holdemHand[1]));

        return combinations;
    }

    public static List<List<int>> GetOmahaCombinations(List<int> omahaHand)
    {
        List<List<int>> combinations = new List<List<int>>();

        combinations.AddRange(GetCombinations(omahaHand[0], omahaHand[1]));
        combinations.AddRange(GetCombinations(omahaHand[0], omahaHand[2]));
        combinations.AddRange(GetCombinations(omahaHand[0], omahaHand[3]));
        combinations.AddRange(GetCombinations(omahaHand[1], omahaHand[2]));
        combinations.AddRange(GetCombinations(omahaHand[1], omahaHand[3]));
        combinations.AddRange(GetCombinations(omahaHand[2], omahaHand[3]));

        combinations.AddRange(GetCombinations(omahaHand[0], omahaHand[1], omahaHand[2]));
        combinations.AddRange(GetCombinations(omahaHand[0], omahaHand[1], omahaHand[3]));
        combinations.AddRange(GetCombinations(omahaHand[0], omahaHand[2], omahaHand[3]));
        combinations.AddRange(GetCombinations(omahaHand[1], omahaHand[2], omahaHand[3]));

        combinations.AddRange(GetCombinations(omahaHand[0], omahaHand[1], omahaHand[2], omahaHand[3]));

        return combinations;
    }

    static List<List<int>> GetCombinations(int card1)
    {
        List<int> communityCards = Poker.ins.communityCards;
        List<List<int>> combinations = new List<List<int>>() { new List<int>() { communityCards[0], communityCards[1], communityCards[2], communityCards[3], card1 } };

        if (Poker.ins.communityCards.Count > 4)
        {
            combinations.Add(new List<int>() { card1, communityCards[1], communityCards[2], communityCards[3], communityCards[4] });
            combinations.Add(new List<int>() { communityCards[0], card1, communityCards[2], communityCards[3], communityCards[4] });
            combinations.Add(new List<int>() { communityCards[0], communityCards[1], card1, communityCards[3], communityCards[4] });
            combinations.Add(new List<int>() { communityCards[0], communityCards[1], communityCards[2], card1, communityCards[4] });
        }
        
        return combinations;
    }

    

    static List<List<int>> GetCombinations(int card1, int card2)
    {
        List<int> communityCards = Poker.ins.communityCards;

        List<List<int>> combinations = new List<List<int>>()
        {
            new List<int>(){ card1, card2, communityCards[0], communityCards[1], communityCards[2]  }
        };

        if (Poker.ins.communityCards.Count > 3)
        {
            combinations.Add(new List<int>() { card1, card2, communityCards[1], communityCards[2], communityCards[3] });
            combinations.Add(new List<int>() { card1, card2, communityCards[0], communityCards[1], communityCards[3] });
            combinations.Add(new List<int>() { card1, card2, communityCards[0], communityCards[2], communityCards[3] });
        }

        if (Poker.ins.communityCards.Count > 4)
        {
            combinations.Add(new List<int>() { card1, card2, communityCards[2], communityCards[3], communityCards[4] });
            combinations.Add(new List<int>() { card1, card2, communityCards[0], communityCards[1], communityCards[4] });
            combinations.Add(new List<int>() { card1, card2, communityCards[1], communityCards[2], communityCards[4] });
            combinations.Add(new List<int>() { card1, card2, communityCards[1], communityCards[3], communityCards[4] });
            combinations.Add(new List<int>() { card1, card2, communityCards[0], communityCards[2], communityCards[4] });
            combinations.Add(new List<int>() { card1, card2, communityCards[0], communityCards[3], communityCards[4] });
        }
        return combinations;
    }


    static List<List<int>> GetCombinations(int card1, int card2, int card3)
    {
        List<int> communityCards = Poker.ins.communityCards;

        List<List<int>> combinations = new List<List<int>>()
        {
            new List<int>(){ card1, card2, card3,  communityCards[0], communityCards[1]  },
            new List<int>(){ card1, card2, card3,  communityCards[0], communityCards[2]  },
            new List<int>(){ card1, card2, card3,  communityCards[1], communityCards[2]  }
        };

        if (Poker.ins.communityCards.Count > 3)
        {
            combinations.Add(new List<int>() { card1, card2, card3, communityCards[0], communityCards[3] });
            combinations.Add(new List<int>() { card1, card2, card3, communityCards[1], communityCards[3] });
            combinations.Add(new List<int>() { card1, card2, card3, communityCards[2], communityCards[3] });
        }

        if (Poker.ins.communityCards.Count > 4)
        {
            combinations.Add(new List<int>() { card1, card2, card3, communityCards[0], communityCards[4] });
            combinations.Add(new List<int>() { card1, card2, card3, communityCards[1], communityCards[4] });
            combinations.Add(new List<int>() { card1, card2, card3, communityCards[2], communityCards[4] });
            combinations.Add(new List<int>() { card1, card2, card3, communityCards[3], communityCards[4] });
        }
      
        return combinations;
    }

    static List<List<int>> GetCombinations(int card1, int card2, int card3, int card4)
    {
        List<int> communityCards = Poker.ins.communityCards;
        List<List<int>> combinations = new List<List<int>>()
        {
            new List<int>(){ card1, card2, card3, card4, communityCards[0]  },
            new List<int>(){ card1, card2, card3, card4, communityCards[1]  },
            new List<int>(){ card1, card2, card3, card4, communityCards[2]  }
        };

        if (Poker.ins.communityCards.Count > 3)
        {
            combinations.Add(new List<int>() { card1, card2, card3, card4, communityCards[3] });
        }

        if (Poker.ins.communityCards.Count > 4)
        {
            combinations.Add(new List<int>() { card1, card2, card3, card4, communityCards[4] });
        }


        return combinations;
    }
}
