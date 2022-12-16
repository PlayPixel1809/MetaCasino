using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PokerWinManager : MonoBehaviour
{
    public static PokerWinManager ins;
    void Awake() { ins = this; }

    public WinType[] winTypes;

    public GameObject winInfoPanel;
    public Text winningPlayer;
    public Image winTypeImage;

    [Header("Assigned During Game -")]
    public List<CardGameSeat> showDownSeats;
    private PotUI activePot;

    [System.Serializable]
    public class WinType
    {
        public string winType;
        public Sprite sprite;
    }

    public void TakeMainPotAmount(bool foldout)
    {
        StartCoroutine("TakeMainPotAmountCoroutine", foldout);
    }

    IEnumerator TakeMainPotAmountCoroutine(bool foldout)
    {
        PokerSeat winningSeat = PokerClient.ins.seats[PokerClient.ins.mainPot.seatIndexes[0]];
        CardGameClient.ins.lpCards.RemoveCards();
        PokerClient.ins.communityCards.RemoveCards();
        if (foldout)
        {
            PokerClient.ins.mainPot.Highlight();

            yield return new WaitForSeconds(2);

            SetWinningPlayerAndWinType(ph.GetPlayerNickname(winningSeat.networkRoomSeat.player), "Foldout");
            ph.ChangePlayerData(winningSeat.networkRoomSeat.player, "balance", PokerClient.ins.mainPot.GetPotAmount());
            winningSeat.networkGameSeat.playerBalance.AddAmount(PokerClient.ins.mainPot.GetPotAmount());
            PokerClient.ins.mainPot.potAmount.text = "";

            yield return new WaitForSeconds(2);

            PokerClient.ins.mainPot.RemoveHighlight();
        }
        else 
        {
            yield return new WaitForSeconds(1);

            ph.ChangePlayerData(winningSeat.networkRoomSeat.player, "balance", PokerClient.ins.mainPot.GetPotAmount());
            winningSeat.networkGameSeat.playerBalance.AddAmount(PokerClient.ins.mainPot.GetPotAmount());
        }
        
        PokerClient.ins.mainPot.gameObject.SetActive(false);
    }

    public void StartShowDowns()
    {
        winInfoPanel.SetActive(false);
        PokerClient.ins.tableCommunityCards.CopyCards(PokerClient.ins.communityCards3D, true);
        PokerClient.ins.communityCards.RemoveCards();
        PokerClient.ins.communityCards3D.RemoveCards();
        CardGameClient.ins.lpCards.RemoveCards();

        showDownSeats = new List<CardGameSeat>();
        for (int i = 0; i < NetworkGameClient.ins.seats.Count; i++)
        {
            if (CardGameClient.ins.seats[i].cards3D.cards.Count > 0)
            {
                PokerClient.ins.seats[i].roundBet.Reset();
                TurnGameClient.ins.seats[i].ResetMoveMade();

                CardGameClient.ins.seats[i].cards.CopyCards(CardGameClient.ins.seats[i].cards3D);
                CardGameClient.ins.seats[i].cards3D.RemoveCards();
                showDownSeats.Add(CardGameClient.ins.seats[i]);
            }
        }
    }

    public void StartShowDown(PotUI pot, int[] winningSeats, string winType, int[] winningCards)
    {
        StartCoroutine(StartShowDownCoroutine(pot, winningSeats, winType, winningCards));
    }

    IEnumerator StartShowDownCoroutine(PotUI pot, int[] winningSeats, string winType, int[] winningCards)
    {
        winInfoPanel.SetActive(false);
        if (activePot != null)
        {
            activePot.RemoveHighlight();
            activePot.gameObject.SetActive(false);
        }
        activePot = pot;
        activePot.Highlight();

        for (int i = 0; i < showDownSeats.Count; i++)  { showDownSeats[i].cards.HideCards(); }
        PokerClient.ins.tableCommunityCards.RemoveCardsHighlight(); 

        for (int i = 0; i < pot.seatIndexes.Length; i++)
        {
            NetworkGameSeat showDownSeat = NetworkGameClient.ins.seats[pot.seatIndexes[i]];
            showDownSeat.GetComponent<CardGameSeat>().cards.RevealCards();
        }

        yield return new WaitForSeconds(2);

        List<int> winningCardsList = new List<int>();
        for (int i = 0; i < winningCards.Length; i++) { winningCardsList.Add(winningCards[i]); }

        
        string winningPlayers = string.Empty;
        for (int i = 0; i < winningSeats.Length; i++)
        {
            PokerSeat winningSeat = PokerClient.ins.seats[winningSeats[i]];

            List<int> playerCards = winningSeat.cardGameSeat.cards.GetCardsIndexes();
            for (int j = 0; j < playerCards.Count; j++)
            {
                if (winningCardsList.Contains(playerCards[j])) { winningSeat.cardGameSeat.cards.cards[j].HighlightCard(); }
            }

            if (!string.IsNullOrEmpty(winningPlayers)) { winningPlayers += " , "; }
            winningPlayers += ph.GetPlayerNickname(winningSeat.networkRoomSeat.player);

            ph.ChangePlayerData(winningSeat.networkRoomSeat.player, "balance", pot.GetPotAmount()/ winningSeats.Length);
            winningSeat.networkGameSeat.playerBalance.AddAmount(pot.GetPotAmount() / winningSeats.Length);
        }
        SetWinningPlayerAndWinType(winningPlayers, winType);

        List<int> communityCards = PokerClient.ins.tableCommunityCards.GetCardsIndexes();
        for (int i = 0; i < communityCards.Count; i++)
        {
            if (winningCardsList.Contains(communityCards[i])) { PokerClient.ins.tableCommunityCards.cards[i].HighlightCard(); }
        }
        
    }

    void SetWinningPlayerAndWinType(string winningPlayer, string winType)
    {
        winInfoPanel.SetActive(true);
        for (int i = 0; i < winTypes.Length; i++)
        {
            if (winTypes[i].winType == winType) 
            {
                winTypeImage.sprite = winTypes[i].sprite;
                winTypeImage.SetNativeSize();
                this.winningPlayer.text = winningPlayer;
                break;
            }
        }
    }
}
