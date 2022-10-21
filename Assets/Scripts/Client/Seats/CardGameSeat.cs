using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardGameSeat : MonoBehaviour
{
    public NetworkGameSeat networkGameSeat;
    public TurnGameSeat turnGameSeat;

    public CardsHolder cards;
    public CardsHolder cards3D;

    public string[] cardsIndexes;

    

    void Start()
    {
        networkGameSeat.onSeatOccupied += () =>
        {
            if (networkGameSeat.player.IsLocal) { cards = CardGame.ins.lpCards; }

            ServerClientBridge.ins.onServerMsgRecieved += OnServerMsgRecieved;
        };

        networkGameSeat.onSeatVaccated += () =>
        {
            ServerClientBridge.ins.onServerMsgRecieved -= OnServerMsgRecieved;
        };
    }


    public void OnServerMsgRecieved(ExitGames.Client.Photon.Hashtable properties)
    {
        if (properties["playersCards"] != null)
        {
            CardGame.ins.playersCards = (string[])properties["playersCards"];
            string[] cardsIndexes = CardGame.ins.playersCards[networkGameSeat.GetSeatIndex()].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

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
