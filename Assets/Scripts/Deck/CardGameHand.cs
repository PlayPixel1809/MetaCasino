using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class CardGameHand
{
    public CardGameHand(List<int> cardsIndexes)
    {
        this.cardsIndexes = cardsIndexes;
        cardsNo = new List<int>();
        for (int i = 0; i < cardsIndexes.Count; i++) { cardsNo.Add(GetCardNo(cardsIndexes[i])); }
        cardsNo.Sort();
        cardsNo.Reverse();
        for (int i = 0; i < cardsIndexes.Count; i++) { cardsSuits.Add(GetCardSuit(cardsIndexes[i])); }

        var query = cardsNo.GroupBy(x => x).Where(g => g.Count() >= 1).Select(y => new { Element = y.Key, Counter = y.Count() }).ToList();
        while (query.Count > 0)
        {
            if (query[0].Counter == 1) { oneOfKinds.Add(query[0].Element); }
            if (query[0].Counter == 2) { twoOfKinds.Add(query[0].Element); }
            if (query[0].Counter == 3) { threeOfKinds.Add(query[0].Element); }
            if (query[0].Counter == 4) { fourOfKinds.Add(query[0].Element); }

            query.Remove(query[0]);
        }

        oneOfKinds.Sort();
        oneOfKinds.Reverse();
        twoOfKinds.Sort();
        twoOfKinds.Reverse();
        threeOfKinds.Sort();
        threeOfKinds.Reverse();
        fourOfKinds.Sort();
        fourOfKinds.Reverse();
    }

    public List<int> cardsIndexes;
    public List<int> cardsSuits = new List<int>();
    public List<int> cardsNo;
    public List<int> oneOfKinds   = new List<int>();
    public List<int> twoOfKinds  = new List<int>();
    public List<int> threeOfKinds = new List<int>();
    public List<int> fourOfKinds  = new List<int>();

    int GetCardNo(int cardIndex)
    {
        int cardNo = cardIndex - 13 * Mathf.FloorToInt(cardIndex / 13.01f);
        if (cardNo == 1) { cardNo = 14; }
        return cardNo;
    }
    int GetCardSuit(int cardIndex) { return Mathf.FloorToInt(cardIndex / 13.01f); }

    public bool AreCardsSameColor()
    {
        for (int i = 0; i < 4; i++) { if (cardsSuits[i] != cardsSuits[i + 1]) { return false; } }
        return true;
    }

    public int GetSequence()
    {
        if (cardsNo[0] == 14 && cardsNo[1] == 5 && cardsNo[2] == 4 && cardsNo[3] == 3 && cardsNo[4] == 2) { return 5; }
        for (int i = 1; i < 5; i++) { if (cardsNo[i] - cardsNo[i - 1] != -1) { return 0; } }
        return cardsNo[0];
    }

    public int GetSequenceInSameColor()
    {
        if (AreCardsSameColor())
        {
            int sequencetIndex = GetSequence();
            if (sequencetIndex > 0) { return sequencetIndex; } else { return 0; }
        }
        else
        { return 0; }
    }


    public int GetHighestFourOfAKind()
    {
        if (fourOfKinds.Count > 0) { return fourOfKinds[0]; } else { return 0; }
    }

    public int GetHighestThreeOfAKind()
    {
        if (threeOfKinds.Count > 0) { return threeOfKinds[0]; } else { return 0; }

    }

    public List<int> GetPairs()
    {
        return twoOfKinds;
    }
}
