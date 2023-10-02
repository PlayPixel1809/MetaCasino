using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PokerReplay : MonoBehaviour
{
    public GameObject replayScreen;
    public GameObject replayScreenBtn;
    public Transform playersInfo;

    private string[] playersBestFiveCards;
    private string[] playersCardsCombinationType;

    void Start()
    {
        ServerClientBridge.ins.onServerMsgRecieved += OnServerMsgRecieved;
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.M)) 
        {
            //OnServerMsgRecieved 

            ExitGames.Client.Photon.Hashtable data = new ExitGames.Client.Photon.Hashtable()
            {
                { "playersBestFiveCards", new string[]{ "1,2,3,4,5,","6,7,8,9,10,","11,12,13,14,15,","16,17,18,19,20,","21,22,23,24,25,"} },
                { "playersCardsCombinationType", new string[]{ "edfgesd","sdvsdv","sdvsdv","sdvsdv","sdvsdv"}  }
            };

            //OnServerMsgRecieved("CreateReplay", data);
        }
    }

    public void ReplayBtn()
    {
        replayScreen.SetActive(true);
    }

    public void OnServerMsgRecieved(string evId, ExitGames.Client.Photon.Hashtable data)
    {
        if (evId == "CreateReplay") 
        {
            replayScreenBtn.SetActive(true);
            
            for (int i = 0; i < playersInfo.childCount; i++) { playersInfo.GetChild(i).gameObject.SetActive(false); }


            playersBestFiveCards = (string[])data["playersBestFiveCards"];
            playersCardsCombinationType = (string[])data["playersCardsCombinationType"];

            for (int i = 0; i < CardGameClient.ins.seats.Count; i++)
            {
                Debug.Log(CardGameClient.ins.seats[i].cards3D.cards.Count);
                if (CardGameClient.ins.seats[i].cards3D.cards.Count > 0)
                {
                    ReplayScreenPlayerInfo playerInfoTab = GetInactivePlayersInfoTab();
                    playerInfoTab.gameObject.SetActive(true);

                    playerInfoTab.playerName.text = NetworkRoomClient.ins.seats[i].playerName.GetLabel();

                    playerInfoTab.playerCards.CopyCards(CardGameClient.ins.seats[i].cards3D);
                    playerInfoTab.communityCards.CopyCards(PokerClient.ins.communityCards);
                    playerInfoTab.communityCards.RevealCards();

                    playerInfoTab.winAmount.text = NetworkGameClient.ins.seats[i].winAmount.ToString();

                    if (TurnGameClient.ins.seats[i].moveMade.GetLabel().ToLower() == "fold")
                    {
                        playerInfoTab.winType.text = "Fold";
                        playerInfoTab.playerCards.HideCards();
                    }
                    else 
                    {
                        playerInfoTab.winType.text = playersCardsCombinationType[i];
                        playerInfoTab.playerCards.RevealCards();
                        

                        string[] playerBestFiveCards = playersBestFiveCards[i].Split(new string[1] { "," }, StringSplitOptions.RemoveEmptyEntries);

                        List<int> playerBestFiveCardsList = new List<int>();
                        for (int j = 0; j < playerBestFiveCards.Length; j++) { playerBestFiveCardsList.Add(int.Parse(playerBestFiveCards[j])); }

                        List<int> playerCards = playerInfoTab.playerCards.GetCardsIndexes();
                        for (int j = 0; j < playerCards.Count; j++)
                        {
                            if (playerBestFiveCardsList.Contains(playerCards[j])) { playerInfoTab.playerCards.cards[j].HighlightCard(); }
                        }

                        List<int> communityCards = playerInfoTab.communityCards.GetCardsIndexes();
                        for (int j = 0; j < communityCards.Count; j++)
                        {
                            if (playerBestFiveCardsList.Contains(communityCards[j])) { playerInfoTab.communityCards.cards[j].HighlightCard(); }
                        }
                    }
                }
            }

            Utils.InvokeDelayedAction(1,delegate { ServerClientBridge.ins.NotifyServerIfMasterClient("CreateReplay"); });
        }

        
    }



    ReplayScreenPlayerInfo GetInactivePlayersInfoTab()
    {
        for (int i = 0; i < playersInfo.childCount; i++)
        {
            if (!playersInfo.GetChild(i).gameObject.activeSelf) { return playersInfo.GetChild(i).GetComponent<ReplayScreenPlayerInfo>(); }
        }
        return null;
    }
}
