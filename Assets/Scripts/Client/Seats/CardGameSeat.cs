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
    public string[] cardsIndexes;


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

    }



    public IEnumerator CreateCards(string[] cardsIndexes)
    {
        this.cardsIndexes = cardsIndexes;
        for (int i = 0; i < cardsIndexes.Length; i++)
        {
            Deck.ins.CreateNewCard(int.Parse(cardsIndexes[i]), cards3D);
            yield return new WaitForSeconds(1);
        }
        
        //cards.CopyCards(cards3D);

        if (networkRoomSeat.player.IsLocal) { CardGameClient.ins.lpCards.CopyCards(cards3D, true); }
    }
}
