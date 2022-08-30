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

    public string[] cardsIndexes; 

    private TurnGameSeat turnGameSeat;

    void Start()
    {
        turnGameSeat = GetComponent<TurnGameSeat>();
        turnGameSeat.onSeatOccupy += SeatOccupied;
    }

    void SeatOccupied()
    {
        if (turnGameSeat.player.IsLocal) { cards = CardGame.ins.lpCards; }
        
        Room.ins.onRoomPropertiesChanged += OnRoomPropertiesChanged;
    }
    

    public void OnRoomPropertiesChanged(ExitGames.Client.Photon.Hashtable properties)
    {
        if (properties["playersCards"] != null)
        {
            

            CardGame.ins.playersCards = (string[])properties["playersCards"];
            string[] cardsIndexes = CardGame.ins.playersCards[turnGameSeat.GetSeatIndex()].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

            StartCoroutine("GetCards", cardsIndexes);
        }
    }

    IEnumerator GetCards(string[] cardsIndexes)
    {
        
        this.cardsIndexes = cardsIndexes;
        for (int i = 0; i < cardsIndexes.Length; i++)
        {
            Deck.ins.CreateNewCard(int.Parse(cardsIndexes[i]), cards3D);
            yield return new WaitForSeconds(1);
        }
        yield return new WaitForSeconds(1);
        cards.CopyCards(cards3D);
    }
}
