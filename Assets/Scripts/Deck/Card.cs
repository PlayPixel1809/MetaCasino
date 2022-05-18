using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    public Image cardBg;
    public Image cardBack;
    public Image cardSuit;
    public Text cardNo;
    public Text cardNoDropShadow;

    public int cardIndex = 0;

    public void SetCard(int cardIndex)
    {
        this.cardIndex = cardIndex;
        int cardNo = CardsManager.GetCardNo(cardIndex);
        this.cardNo.text = cardNo.ToString();
        if (cardNo == 11) { this.cardNo.text = "J"; }
        if (cardNo == 12) { this.cardNo.text = "Q"; }
        if (cardNo == 13) { this.cardNo.text = "K"; }
        if (cardNo == 14) { this.cardNo.text = "A"; }
        cardNoDropShadow.text = this.cardNo.text;
        RefreshCard();
    }

    public void RefreshCard()
    {
        cardBack.sprite = CardsManager.ins.GetCardBack("");

        if (cardIndex == 0) { return; }
        CardsManager.CardFront cardFront = CardsManager.ins.GetCardFront("");
        cardNo.font = cardFront.font;
        
        int cardSuitInt = CardsManager.GetCardSuit(cardIndex);
        if (cardSuitInt == 0) 
        {
            cardSuit.sprite = cardFront.spade;
            cardNo.color = cardFront.spadeFontColor;
        }

        if (cardSuitInt == 1)
        {
            cardSuit.sprite = cardFront.heart;
            cardNo.color = cardFront.heartFontColor;
        }

        if (cardSuitInt == 2)
        {
            cardSuit.sprite = cardFront.diamond;
            cardNo.color = cardFront.diamondFontColor;
        }

        if (cardSuitInt == 3)
        {
            cardSuit.sprite = cardFront.club;
            cardNo.color = cardFront.clubFontColor;
        }
    }

    public void RefreshAndHideCard()
    {
        RefreshCard();
        HideCard();
    }

    public void HideCard()
    {
        cardBg.gameObject.SetActive(true);
        cardBack.gameObject.SetActive(true);
        cardSuit.gameObject.SetActive(false);
        cardNo.gameObject.SetActive(false);
        cardNoDropShadow.gameObject.SetActive(false);
    }

    public void RevealCard()
    {
        cardBg.gameObject.SetActive(true);
        cardBack.gameObject.SetActive(false);
        cardSuit.gameObject.SetActive(true);
        cardNo.gameObject.SetActive(true);
        cardNoDropShadow.gameObject.SetActive(true);
    }
}
