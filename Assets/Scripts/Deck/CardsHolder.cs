using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardsHolder : MonoBehaviour
{
    public float cardAnimTime = 1;
    public bool revealCard;
    public AnimationCurve cardAnimCurve = AnimationCurve.Linear(0, 0, 1, 1);
    
    public Transform cardsParent;

    public List<Transform> cardsPositions;

    public List<Card> cards = new List<Card>();


    public void RevealCards()
    {
        gameObject.SetActive(true);
        for (int i = 0; i < cards.Count; i++) { cards[i].RevealCard(); }
    }

    public void HideCards()
    {
        for (int i = 0; i < cards.Count; i++) { cards[i].HideCard(); }
    }

    public void CopyCards(CardsHolder copyFrom)
    {
        for (int i = 0; i < copyFrom.cards.Count; i++)
        {
            cards[i].SetCard(copyFrom.cards[i].cardIndex, copyFrom.cards[i].deck);
            cards[i].gameObject.SetActive(true);
            if (revealCard) { cards[i].RevealCard(); }
        }
    }

    public void AddCard(Card card)
    {
        cards.Add(card);
        card.transform.parent = cardsParent;

        Transform target = cardsParent;
        if (cards.Count <= cardsPositions.Count) { target = cardsPositions[cards.Count - 1]; }

        StartCoroutine(AnimateCard(card.transform, target));
    }


    IEnumerator AnimateCard(Transform card, Transform target)
    {
        Vector3 fromPos = card.position;
        Quaternion fromRot = card.rotation;
        float val = 0;
        while (val < 1)
        {
            val += Time.deltaTime / cardAnimTime;

            card.position = Vector3.Lerp(fromPos, target.position, cardAnimCurve.Evaluate(val));
            card.rotation = Quaternion.Lerp(fromRot, target.rotation, cardAnimCurve.Evaluate(val));

            yield return null;
        }
        if (revealCard) { card.GetComponent<Card>().RevealCard(); }

    }
}
