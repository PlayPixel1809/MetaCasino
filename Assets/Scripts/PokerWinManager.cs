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
    private Pot activePot;

    [System.Serializable]
    public class WinType
    {
        public string winType;
        public Sprite sprite;
    }

    public void WinMainPotWithoutShowdown(bool foldout)
    {
        StartCoroutine("WinMainPotWithoutShowdownCoroutine", foldout);
    }

    IEnumerator WinMainPotWithoutShowdownCoroutine(bool foldout)
    {
        PokerSeat winningSeat = PokerClient.ins.seats[PokerClient.ins.mainPot.seatIndexes[0]];
        CardGameClient.ins.lpCards.RemoveCards();
        PokerClient.ins.communityCards.RemoveCards();
        if (foldout)
        {
            PokerClient.ins.mainPot.Highlight();

            yield return new WaitForSeconds(2);

            SetWinningPlayerAndWinType(ph.GetPlayerNickname(winningSeat.networkRoomSeat.player), "Foldout");
            winningSeat.networkGameSeat.AddBalance(PokerClient.ins.mainPot.GetPotAmount());
            PokerClient.ins.mainPot.amountTxt.text = "";

            yield return new WaitForSeconds(2);

            PokerClient.ins.mainPot.RemoveHighlight();
        }
        else 
        {
            yield return new WaitForSeconds(1);
            winningSeat.networkGameSeat.AddBalance(PokerClient.ins.mainPot.GetPotAmount());
        }
        
        PokerClient.ins.mainPot.gameObject.SetActive(false);
        ServerClientBridge.ins.NotifyServerIfMasterClient("WinMainPotWithoutShowdown");
    }

    public void PrepareForShowDowns()
    {
        winInfoPanel.SetActive(false);
        PokerClient.ins.tableCommunityCards.CopyCards(PokerClient.ins.communityCards3D, true);
        //PokerClient.ins.communityCards.RemoveCards();
        PokerClient.ins.communityCards3D.RemoveCards();
        //CardGameClient.ins.lpCards.RemoveCards();

        showDownSeats = new List<CardGameSeat>();
        for (int i = 0; i < CardGameClient.ins.seats.Count; i++)
        {
            if (CardGameClient.ins.seats[i].cards3D.cards.Count > 0 && TurnGameClient.ins.seats[i].moveMade.GetLabel().ToLower() != "fold")
            {
                TurnGameClient.ins.seats[i].ResetMoveMade();

                CardGameClient.ins.seats[i].cards.CopyCards(CardGameClient.ins.seats[i].cards3D);
                CardGameClient.ins.seats[i].cards3D.RemoveCards();
                PokerClient.ins.seats[i].roundBet.Reset();

                showDownSeats.Add(CardGameClient.ins.seats[i]);
            }
        }

        Utils.InvokeDelayedAction(2, () => { ServerClientBridge.ins.NotifyServerIfMasterClient("PrepareForShowDowns"); });
    }


    public void HighlightShowdownPot(int potIndex)
    {
        StartCoroutine(HighlightShowdownPotCoroutine(potIndex));
    }

    IEnumerator HighlightShowdownPotCoroutine(int potIndex)
    {
        Pot pot = PokerClient.ins.mainPot;
        if (potIndex > -1) { pot = PokerClient.ins.sidePots[potIndex]; }

        winInfoPanel.SetActive(false);
        if (activePot != null)
        {
            activePot.RemoveHighlight();
            activePot.gameObject.SetActive(false);
        }
        activePot = pot;
        activePot.Highlight();

        PokerClient.ins.tableCommunityCards.RemoveCardsHighlight();
        for (int i = 0; i < showDownSeats.Count; i++) { showDownSeats[i].cards.HideCards(); }
        for (int i = 0; i < pot.seatIndexes.Length; i++) { CardGameClient.ins.seats[pot.seatIndexes[i]].cards.RevealCards(); }

        yield return new WaitForSeconds(2);
        
        ServerClientBridge.ins.NotifyServerIfMasterClient("HighlightShowdownPot", "potIndex", potIndex);
    }

    public void DeclarePotWinners(int potIndex, int[] winningSeats, string winType, int[] winningCards)
    {
        StartCoroutine(DeclarePotWinnersCoroutine(potIndex,winningSeats, winType, winningCards));
    }

    IEnumerator DeclarePotWinnersCoroutine(int potIndex, int[] winningSeats, string winType, int[] winningCards)
    {
        Pot pot = PokerClient.ins.mainPot;
        if (potIndex > -1) { pot = PokerClient.ins.sidePots[potIndex]; }

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

            winningSeat.networkGameSeat.AddBalance(pot.GetPotAmount() / winningSeats.Length);
        }
        SetWinningPlayerAndWinType(winningPlayers, winType);

        List<int> communityCards = PokerClient.ins.tableCommunityCards.GetCardsIndexes();
        for (int i = 0; i < communityCards.Count; i++)
        {
            if (winningCardsList.Contains(communityCards[i])) { PokerClient.ins.tableCommunityCards.cards[i].HighlightCard(); }
        }
        yield return new WaitForSeconds(4);
        ServerClientBridge.ins.NotifyServerIfMasterClient("DeclarePotWinners", "potIndex", potIndex);
    }

    void SetWinningPlayerAndWinType(string winningPlayer, string winType)
    {
        winInfoPanel.SetActive(true);
        winTypeImage.sprite = GetWinTypeSprite(winType);
        winTypeImage.SetNativeSize();
        this.winningPlayer.text = winningPlayer;
    }

    public Sprite GetWinTypeSprite(string winType)
    {
        for (int i = 0; i < winTypes.Length; i++)
        {
            if (winTypes[i].winType == winType) { return winTypes[i].sprite; }
        }
        return null;
    }
}
