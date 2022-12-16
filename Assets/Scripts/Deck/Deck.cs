using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Deck : MonoBehaviour
{
    public static Deck ins;
    void Awake() { ins = this; }

    public Transform deckModel;

    public Card cardPrefab;
    public Transform cardSpawnPoint;
    public AudioClip cardSound;

    public Sprite cardBack;
    public CardFront cardFront;
    public List<Sprite> cardsFront;

    private List<CardsHolder> cardsHolders = new List<CardsHolder>();
    private int cardsDrawnCount;

    [System.Serializable]
    public class CardFront
    {
        public Font font;

        public Sprite spade;
        public Sprite heart;
        public Sprite diamond;
        public Sprite club;
    }

    public void CreateNewCard(int cardIndex, CardsHolder cardsHolder)
    {
        cardsDrawnCount += 1;

        GameUtils.ins.PlaySound(cardSound);
        deckModel.localScale = new Vector3(deckModel.localScale.x, Mathf.Lerp(0,1, (float)(52 - cardsDrawnCount )/ 52), deckModel.localScale.z);
        cardsHolder.AddCard(cardIndex);
        cardsHolders.Add(cardsHolder);
    }

    public void Reset()
    {
        cardsDrawnCount = 0;
        deckModel.localScale = Vector3.one;
        for (int i = 0; i < cardsHolders.Count; i++)
        {
            for (int j = 0; j < cardsHolders[i].cards.Count; j++) { DestroyImmediate(cardsHolders[i].cards[j].gameObject); }
            cardsHolders[i].cards = new List<Card>();
        }
        cardsHolders = new List<CardsHolder>();
    }
}
