using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardsManager : MonoBehaviour
{
    public static CardsManager ins;
    void Awake() { ins = this; }

    public Transform cardPrefab;
    public List<CardFront> cardsFront;
    public List<CardBack> cardsBack;

    [System.Serializable]
    public class CardBack
    {
        public string cardBackName;
        public Sprite cardBackSprite;
    }

    [System.Serializable]
    public class CardFront
    {
        public string cardFrontName;
        public Font font;
        public Color spadeFontColor = Color.black;
        public Color heartFontColor = Color.red;
        public Color diamondFontColor = Color.red;
        public Color clubFontColor = Color.black;

        public Sprite spade;
        public Sprite heart;
        public Sprite diamond;
        public Sprite club;
    }



    void Start()
    {
        if (!PlayerPrefs.HasKey("activeCardBack")) { PlayerPrefs.SetString("activeCardBack", "1"); }
        if (!PlayerPrefs.HasKey("activeCardFront")) { PlayerPrefs.SetString("activeCardFront", "1"); }
    }

    

    
    public Sprite GetCardBack(string cardBackName)
    {
        for (int i = 0; i < cardsBack.Count; i++)
        {
            if (cardsBack[i].cardBackName == cardBackName)
            {
                if (cardsBack[i].cardBackSprite == null) { cardsBack[i].cardBackSprite = Resources.Load("CardBacks/" + cardsBack[i].cardBackName) as Sprite; }
                return cardsBack[i].cardBackSprite;
            }
        }
        return null;
    }

    public CardFront GetCardFront(string cardFrontName)
    {
        for (int i = 0; i < cardsFront.Count; i++)
        {
            if (cardsFront[i].cardFrontName == cardFrontName)
            {
                if (cardsFront[i].spade == null)   { cardsBack[i].cardBackSprite = Resources.Load("CardSuits/" + cardsFront[i].cardFrontName + "/spade") as Sprite;   }
                if (cardsFront[i].heart == null)   { cardsBack[i].cardBackSprite = Resources.Load("CardSuits/" + cardsFront[i].cardFrontName + "/heart") as Sprite;   }
                if (cardsFront[i].diamond == null) { cardsBack[i].cardBackSprite = Resources.Load("CardSuits/" + cardsFront[i].cardFrontName + "/diamond") as Sprite; }
                if (cardsFront[i].club == null)    { cardsBack[i].cardBackSprite = Resources.Load("CardSuits/" + cardsFront[i].cardFrontName + "/club") as Sprite;    }
                return cardsFront[i];
            }
        }
        return null;
    }


    public static int GetCardNo(int cardIndex)
    {
        int cardNo = cardIndex - 13 * Mathf.FloorToInt(cardIndex / 13.01f);
        if (cardNo == 1) { cardNo = 14; }
        return cardNo;
    }
    public static int GetCardSuit(int cardIndex) { return Mathf.FloorToInt(cardIndex / 13.01f); }
}
