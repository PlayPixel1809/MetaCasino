using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    public Image cardBg;
    public Image cardBack;
    public Image cardSuitImage;
    public Text cardNoTxt;

    [HideInInspector] public int cardIndex = 0;
    [HideInInspector] public Deck deck;

    private int cardNo = 0;
    private int cardSuit = 0;

    public void SetCard(int cardIndex, Deck deck)
    {
        this.cardIndex = cardIndex;
        this.deck = deck;

        cardNo = GetCardNo(cardIndex);
        
        cardNoTxt.text = cardNo.ToString();
        if (cardNo == 11) { cardNoTxt.text = "J"; }
        if (cardNo == 12) { cardNoTxt.text = "Q"; }
        if (cardNo == 13) { cardNoTxt.text = "K"; }
        if (cardNo == 14) { cardNoTxt.text = "A"; }


        cardSuit = GetCardSuit(cardIndex);
        SetCardDesign(deck);
    }

    public void SetCardDesign(Deck deck)
    {
        cardBack.sprite = deck.cardBack;
        cardNoTxt.font = deck.cardFront.font;

        if (cardSuit == 0)
        {
            cardSuitImage.sprite = deck.cardFront.spade;
            cardNoTxt.color = Color.black;
        }

        if (cardSuit == 1)
        {
            cardSuitImage.sprite = deck.cardFront.heart;
            cardNoTxt.color = Color.red;
        }

        if (cardSuit == 2)
        {
            cardSuitImage.sprite = deck.cardFront.diamond;
            cardNoTxt.color = Color.red;
        }

        if (cardSuit == 3)
        {
            cardSuitImage.sprite = deck.cardFront.club;
            cardNoTxt.color = Color.black;
        }
    }

    

    public void HideCard()
    {
        cardBg.gameObject.SetActive(true);
        cardBack.gameObject.SetActive(true);
        cardSuitImage.gameObject.SetActive(false);
        cardNoTxt.gameObject.SetActive(false);
    }

    public void RevealCard()
    {
        gameObject.SetActive(true);
        cardBg.gameObject.SetActive(true);
        cardBack.gameObject.SetActive(false);
        cardSuitImage.gameObject.SetActive(true);
        cardNoTxt.gameObject.SetActive(true);
    }

    int GetCardNo(int cardIndex)
    {
        int cardNo = cardIndex - 13 * Mathf.FloorToInt(cardIndex / 13.01f);
        if (cardNo == 1) { cardNo = 14; }
        return cardNo;
    }
    int GetCardSuit(int cardIndex) { return Mathf.FloorToInt(cardIndex / 13.01f); }
}
