using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardGameSeat : MonoBehaviour
{
    public CardsHolder cards;
    public CardsHolder cards3D;

    [Header("Assigned During Game -")]
    public string[] cardsIndexes = new string[0];


    [HideInInspector] public NetworkRoomSeat networkRoomSeat;
    [HideInInspector] public NetworkGameSeat networkGameSeat;
    [HideInInspector] public TurnGameSeat turnGameSeat;

    void Start()
    {
        networkRoomSeat = GetComponent<NetworkRoomSeat>();
        networkGameSeat = GetComponent<NetworkGameSeat>();
        turnGameSeat = GetComponent<TurnGameSeat>();

        networkRoomSeat.onSeatOccupied += () =>
        {
            
        };

        networkRoomSeat.onSeatVaccated += () =>
        {
            cards3D.RemoveCards();
        };
    }

    public void CreateCards(string[] cardsIndexes, bool animateCards = true) { StartCoroutine(CreateCardsCoroutine(cardsIndexes, animateCards)); }

    IEnumerator CreateCardsCoroutine(string[] cardsIndexes, bool animateCards = true)
    {
        this.cardsIndexes = cardsIndexes;
        for (int i = 0; i < cardsIndexes.Length; i++)
        {
            Deck.ins.CreateNewCard(int.Parse(cardsIndexes[i]), cards3D, animateCards);
            if (animateCards) { yield return new WaitForSeconds(1); }
        }
        if (networkRoomSeat.player.IsLocal) { CardGameClient.ins.lpCards.CopyCards(cards3D, true); }
    }

    public void RemoveCards() 
    {
        cardsIndexes = new string[0];
        cards3D.RemoveCards();
        cards.RemoveCards();
    }
}
