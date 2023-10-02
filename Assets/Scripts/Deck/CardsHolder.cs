using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardsHolder : MonoBehaviour
{
    public float cardAnimTime = 1;
    public bool revealCard;
    public AnimationCurve cardAnimCurve = AnimationCurve.Linear(0, 0, 1, 1);
    
    public Transform cardsParent;

    [Header("Assigned During Game -")]
    public List<Card> cards = new List<Card>();

    

    public void RevealCards()
    {
        for (int i = 0; i < cards.Count; i++) { cards[i].RevealCard(); }
    }

    public void HighlightCards()
    {
        for (int i = 0; i < cards.Count; i++) { cards[i].HighlightCard(); }
    }

    public void RemoveCardsHighlight()
    {
        for (int i = 0; i < cards.Count; i++) { cards[i].RemoveHighlight(); }
    }

    public void HideCards()
    {
        for (int i = 0; i < cards.Count; i++) { cards[i].HideCard(); }
    }

    public void RemoveCards()
    {
        for (int i = 0; i < cardsParent.childCount; i++)
        {
            if (cardsParent.GetChild(i).gameObject.activeInHierarchy)
            {
                Card activeCard = cardsParent.GetChild(i).GetChild(0).GetComponent<Card>();
                activeCard.RemoveHighlight();
                cardsParent.GetChild(i).gameObject.SetActive(false);
                cards.Remove(activeCard);
            }
        }
    }

    public void CopyCards(CardsHolder copyFrom, bool revealCards = false)
    {
        for (int i = 0; i < copyFrom.cards.Count; i++)
        {
            if (!cardsParent.GetChild(i).gameObject.activeSelf) { AddCard(copyFrom.cards[i].cardIndex, false); }
        }
        if (revealCards) { RevealCards(); } else { HideCards(); }
    }

    public Card AddCard(int cardIndex, bool animate = true)
    {
        for (int i = 0; i < cardsParent.childCount; i++)
        {
            if (!cardsParent.GetChild(i).gameObject.activeSelf)
            {
                Card inActiveCard = cardsParent.GetChild(i).GetChild(0).GetComponent<Card>();
                cardsParent.GetChild(i).gameObject.SetActive(true);
                inActiveCard.SetCard(cardIndex, Deck.ins);
                cards.Add(inActiveCard);
                if (animate) { StartCoroutine(AnimateCard(inActiveCard.transform)); } else 
                {
                    if (revealCard) { inActiveCard.GetComponent<Card>().RevealCard(); }
                }
                return inActiveCard;
            }
        }

        return null;
    }


    IEnumerator AnimateCard(Transform card)
    {
        AudioSource.PlayClipAtPoint(Deck.ins.cardSound, Camera.main.transform.position);
        card.position = Deck.ins.cardSpawnPoint.position;
        card.rotation = Deck.ins.cardSpawnPoint.rotation;

        Vector3 fromPos = card.localPosition;
        Quaternion fromRot = card.rotation;
        float val = 0;
        while (val < 1)
        {
            val += Time.deltaTime / cardAnimTime;

            card.localPosition = Vector3.Lerp(fromPos, Vector3.zero, cardAnimCurve.Evaluate(val));
            card.rotation = Quaternion.Lerp(fromRot, card.parent.rotation, cardAnimCurve.Evaluate(val));

            yield return null;
        }
        if (revealCard) { card.GetComponent<Card>().RevealCard(); }

    }

    public List<int> GetCardsIndexes()
    {
        List<int> cardsIndexes = new List<int>();
        for (int i = 0; i < cards.Count; i++)
        {
            cardsIndexes.Add(cards[i].cardIndex);
        }
        return cardsIndexes;
    }
}
