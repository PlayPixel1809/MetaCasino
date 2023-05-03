using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PokerClient : MonoBehaviour
{
    public static PokerClient ins;
    void Awake() { ins = this; }


    public PokerControls lpControls;

    public CardsHolder communityCards;
    public CardsHolder tableCommunityCards;
    public CardsHolder communityCards3D;
    public Transform cardsCombinationTypePanel;
    public Pot mainPot;
    public Pot[] sidePots;
    public GameObject restartBtn;


    [Header("Assigned During Game -"), Space]
    public List<PokerSeat> seats;
    public float currentBet;

    void Start()
    {
        StartCoroutine("AssignSeats");

        CardGameClient.ins.onRoomJoined += () =>
        {
            
        };


        CardGameClient.ins.onSeatAssigned += () =>
        {
            if (ph.GetRoomData("playersBetsForRound") != null)
            {
                float[] playersBetsForRound = (float[])ph.GetRoomData("playersBetsForRound");
                for (int i = 0; i < playersBetsForRound.Length; i++) { if (playersBetsForRound[i] > 0) { seats[i].MakeBet(playersBetsForRound[i]); } }
            }

            if (ph.GetRoomData("communityCards") != null) 
            {
                int[] cards = (int[])ph.GetRoomData("communityCards");
                for (int i = 0; i < cards.Length; i++) { Deck.ins.CreateNewCard(cards[i], communityCards3D, false); }
                communityCards.CopyCards(communityCards3D, true);
            }
        };


        ServerClientBridge.ins.onServerMsgRecieved += OnServerMsgRecieved;
        NetworkGame.ins.onGameStart += ResetData;

        TurnGameClient.ins.onTurnMissed += (data)=> 
        {
            string moveMade = "FOLD";
            if (currentBet == 0) { moveMade = "CHECK"; }
            ServerClientBridge.ins.NotifyServerIfMasterClient((string)data["evId"], "moveMade", moveMade);
        };

        NetworkGameClient.ins.onGameComplete += () =>
        {
            mainPot.Reset();
            for (int i = 0; i < sidePots.Length; i++) { sidePots[i].Reset(); }
            tableCommunityCards.RemoveCards();
            communityCards.RemoveCards();
            CardGameClient.ins.lpCards.RemoveCards();
            PokerWinManager.ins.winInfoPanel.SetActive(false);
            cardsCombinationTypePanel.gameObject.SetActive(false);
        };
        
    }

    public void ResetData()
    {
        
    }

    IEnumerator AssignSeats()
    {
        yield return new WaitForEndOfFrame();
        seats = new List<PokerSeat>();
        for (int i = 0; i < NetworkRoomClient.ins.seats.Count; i++) { seats.Add(NetworkRoomClient.ins.seats[i].GetComponent<PokerSeat>()); }
    }

    public void OnServerMsgRecieved(string evId, ExitGames.Client.Photon.Hashtable data)
    {
        //if (!NetworkGameClient.ins.playing) { return; }

        if (evId == "MakeInitialBlinds")
        {
            TurnGameClient.ins.seats[(int)data["smallBlindIndex"]].MakeMove("S-BLIND", NetworkGame.ins.minBet);
            seats[(int)data["smallBlindIndex"]].MakeBet(NetworkGame.ins.minBet);

            TurnGameClient.ins.seats[(int)data["bigBlindIndex"]].MakeMove("B-BLIND", NetworkGame.ins.minBet * 2);
            seats[(int)data["bigBlindIndex"]].MakeBet(NetworkGame.ins.minBet * 2);

            Utils.InvokeDelayedAction(1, () => { ServerClientBridge.ins.NotifyServerIfMasterClient("MakeInitialBlinds", "bigBlindIndex", data["bigBlindIndex"]); });
        }

        if (evId == "StartPokerTurn")
        {
            currentBet = (float)data["currentBet"];
            seats[TurnGameClient.ins.turn].StartTurn(currentBet, (float)data["playerBet"]);
        }


        if (evId == "MakeMove") 
        {
            if ((string)data["moveMade"] == "FOLD") { seats[(int)data["moveMadeBy"]].MakePlayerFoldInAllPots(); }
            if (data["moveAmount"] != null) { seats[(int)data["moveMadeBy"]].MakeBet((float)data["moveAmount"]); }
        }
        

        if (evId == "EndRound") { StartCoroutine("EndRound", data); }
        

        if (evId == "CreateCommunityCards" || evId == "CreateCommunityCardsForShowdown") { StartCoroutine(CreateCommunityCards(data)); }

        if (evId == "ShowCardsBestCombination") { ShowCardsBestCombination(data); }


        if (evId == "WinMainPotWithoutShowdown")  {  PokerWinManager.ins.WinMainPotWithoutShowdown((bool)data["mainPotFoldout"]); }
        
        if (evId == "PrepareForShowDowns") {  PokerWinManager.ins.PrepareForShowDowns(); }


        if (evId == "HighlightShowdownPot") { PokerWinManager.ins.HighlightShowdownPot((int)data["potIndex"]); }

        if (evId == "DeclarePotWinners")
        {
            int[] winningSeats = (int[])data["winningSeats"];
            string winType = (string)data["winType"];
            int[] winningCards = (int[])data["winningCards"];

            PokerWinManager.ins.DeclarePotWinners((int)data["potIndex"], winningSeats, winType, winningCards);
        }
    }


    IEnumerator EndRound(ExitGames.Client.Photon.Hashtable data)
    {
        for (int i = 0; i < seats.Count; i++) { seats[i].DeactivateMoveInfo(); }

        if (data.ContainsKey("submitRoundBet"))
        {
            for (int i = 0; i < seats.Count; i++) { seats[i].SubmitRoundBet(); }
            yield return new WaitForSeconds(1);
        }

        if (data.ContainsKey("mainPotAmount")) { mainPot.AddAmount((float)data["mainPotAmount"], (int[])data["mainPotSeats"]); }

        if (data.ContainsKey("sidePotsAmount"))
        {
            mainPot.gameObject.SetActive(false);
            float[] sidePotsAmount = (float[])data["sidePotsAmount"];
            string[] sidePotsSeats = (string[])data["sidePotsSeats"];
            for (int i = 0; i < sidePotsAmount.Length; i++) { Pot.GetInActivePot(sidePots).AddAmount(sidePotsAmount[i], sidePotsSeats[i]); }
        }

        yield return new WaitForSeconds(1);
        ServerClientBridge.ins.NotifyServerIfMasterClient("EndRound");
    }



    IEnumerator CreateCommunityCards(ExitGames.Client.Photon.Hashtable data)
    {
        int[] cards = (int[])data["cards"];
        for (int i = 0; i < cards.Length; i++) { Deck.ins.CreateNewCard(cards[i], communityCards3D); }
        
        yield return new WaitForSeconds(1);
        communityCards.CopyCards(communityCards3D, true);
        yield return new WaitForSeconds(1);
        ServerClientBridge.ins.NotifyServerIfMasterClient((string)data["evId"]);
    }

    void ShowCardsBestCombination(ExitGames.Client.Photon.Hashtable data)
    {
        CardGameClient.ins.lpCards.RemoveCardsHighlight();
        communityCards.RemoveCardsHighlight();
         
        int[] cards = (int[])data["cards"];
        string combinationType = (string)data["combinationType"];

        List<int> cardsList = new List<int>();
        for (int i = 0; i < cards.Length; i++) { cardsList.Add(cards[i]); }

        List<int> playerCards = CardGameClient.ins.lpCards.GetCardsIndexes();
        for (int j = 0; j < playerCards.Count; j++)
        {if (cardsList.Contains(playerCards[j])) { CardGameClient.ins.lpCards.cards[j].HighlightCard(); }}

        List<int> communityCardsIndexes = communityCards.GetCardsIndexes();
        for (int i = 0; i < communityCardsIndexes.Count; i++)
        {if (cardsList.Contains(communityCardsIndexes[i])) { communityCards.cards[i].HighlightCard(); }}

        cardsCombinationTypePanel.gameObject.SetActive(true);
        cardsCombinationTypePanel.GetChild(1).GetComponent<Image>().sprite = PokerWinManager.ins.GetWinTypeSprite(combinationType);
        cardsCombinationTypePanel.GetChild(1).GetComponent<Image>().SetNativeSize();
    }
}
