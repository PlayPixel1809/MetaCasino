using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PokerClient : MonoBehaviour
{
    public static PokerClient ins;
    void Awake() { ins = this; }


    public PokerControls lpControls;

    public CardsHolder communityCardsHolder;
    public CardsHolder communityCardsHolder3D;

    [HideInInspector]
    public List<PokerSeat> seats;

    void Start()
    {
        for (int i = 0; i < NetworkGameClient.ins.seats.Count; i++) { seats.Add(NetworkGameClient.ins.seats[i].GetComponent<PokerSeat>()); }


        ServerClientBridge.ins.onServerMsgRecieved += OnMsgRecieved;
    }

    public void OnMsgRecieved(ExitGames.Client.Photon.Hashtable hashtable)
    {
        if (hashtable.ContainsKey("smallBlindSeat")) { seats[(int)hashtable["smallBlindSeat"]].MakeMove("S-BLIND", (float)hashtable["smallBlindAmount"]); }
        if (hashtable.ContainsKey("bigBlindSeat"))   { seats[(int)hashtable["bigBlindSeat"]].MakeMove("B-BLIND", (float)hashtable["bigBlindAmount"]); }

        if (hashtable.ContainsKey("evaluatePokerControls"))
        {
            lpControls.EvaluateCallBtn((float)hashtable["currentBet"], (float)hashtable["playerBet"], (float)ph.GetLocalPlayerData("balance"));
            lpControls.EvaluateRaiseBtn((float)hashtable["currentBet"], (float)ph.GetLocalPlayerData("balance"));
        }

        if (hashtable.ContainsKey("newCommunityCards"))
        {
            int[] newCommunityCards = (int[])ph.GetRoomData("newCommunityCards");

            for (int i = 0; i < newCommunityCards.Length; i++) { Deck.ins.CreateNewCard(newCommunityCards[i], communityCardsHolder3D); }
            communityCardsHolder.CopyCards(communityCardsHolder3D);
        }
    }

    
}
