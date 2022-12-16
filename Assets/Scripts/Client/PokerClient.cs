using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PokerClient : MonoBehaviour
{
    public static PokerClient ins;
    void Awake() { ins = this; }


    public PokerControls lpControls;

    public CardsHolder communityCards;
    public CardsHolder tableCommunityCards;
    public CardsHolder communityCards3D;


    public PotUI mainPot;
    public PotUI[] sidePots;

    public GameObject restartBtn;

    [HideInInspector] public float smallBlind;

    [HideInInspector]
    public List<PokerSeat> seats;

    void Start()
    {
        for (int i = 0; i < NetworkRoomClient.ins.seats.Count; i++) { seats.Add(NetworkRoomClient.ins.seats[i].GetComponent<PokerSeat>()); }
        ServerClientBridge.ins.onServerMsgRecieved += OnMsgRecieved;
    }

    public void OnMsgRecieved(ExitGames.Client.Photon.Hashtable hashtable)
    {
        if (hashtable.ContainsKey("smallBlindSeat")) 
        {
            smallBlind = (float)hashtable["smallBlindAmount"];
            TurnGameClient.ins.seats[(int)hashtable["smallBlindSeat"]].MakeMove("S-BLIND", smallBlind);
        }

        if (hashtable.ContainsKey("bigBlindSeat"))   { TurnGameClient.ins.seats[(int)hashtable["bigBlindSeat"]].MakeMove("B-BLIND", (float)hashtable["bigBlindAmount"]); }

        if (hashtable.ContainsKey("evaluatePokerControls"))
        {
            //Debug.Log("evaluatePokerControls");
            float balance = (float)ph.GetPlayerData(NetworkRoomClient.ins.seats[TurnGameClient.ins.turn].player, "balance");
            lpControls.EvaluateCallBtn((float)hashtable["currentBet"], (float)hashtable["playerBet"], balance);
            lpControls.EvaluateRaiseBtn((float)hashtable["currentBet"], balance);
        }

       
        if (hashtable.ContainsKey("endRound"))
        {
            for (int i = 0; i < NetworkGameClient.ins.seats.Count; i++) { seats[i].DeactivateMoveInfo(); }
        }

        if (hashtable.ContainsKey("submitRoundBet"))
        {
            for (int i = 0; i < seats.Count; i++) { seats[i].SubmitRoundBet(); }
        }

        if (hashtable.ContainsKey("mainPotAmount"))
        {
            mainPot.SetPot((float)hashtable["mainPotAmount"], (int[])hashtable["mainPotSeats"]); 
        }

        if (hashtable.ContainsKey("sidePotAmount"))
        {
            mainPot.gameObject.SetActive(false);
            for (int i = 0; i < sidePots.Length; i++)
            {
                if (!sidePots[i].gameObject.activeInHierarchy) { sidePots[i].SetPot((float)hashtable["sidePotAmount"], (int[])hashtable["sidePotSeats"]); break; }
            }
        }

        if (hashtable.ContainsKey("newCommunityCards"))
        {
            Debug.Log("newCommunityCards");
            bool forShowdown = (bool)hashtable["forShowdown"];
            int[] newCommunityCards = (int[])hashtable["newCommunityCards"];

            for (int i = 0; i < newCommunityCards.Length; i++)
            {
                //Debug.Log(newCommunityCards[i]);
                Deck.ins.CreateNewCard(newCommunityCards[i], communityCards3D);
            }
            if (!forShowdown)
            {
                Utils.InvokeDelayedAction(1, () => { communityCards.CopyCards(communityCards3D, true); });
            }
            else { PokerWinManager.ins.winInfoPanel.SetActive(false); }
        }


        if (hashtable.ContainsKey("takeMainPotAmount"))  {  PokerWinManager.ins.TakeMainPotAmount((bool)hashtable["mainPotFoldout"]); }
        if (hashtable.ContainsKey("startShowDowns")) {  PokerWinManager.ins.StartShowDowns(); }

        if (hashtable.ContainsKey("showDownCommunityCards")) { PokerWinManager.ins.winInfoPanel.SetActive(false); }

        if (hashtable.ContainsKey("startShowDown"))
        {
            PotUI showDownPot = mainPot;
            int potIndex = (int)hashtable["potIndex"];
            if (potIndex > -1) { showDownPot = sidePots[potIndex]; }
            int[] winningSeats = (int[])hashtable["winningSeats"];
            string winType = (string)hashtable["winType"];
            int[] winningCards = (int[])hashtable["winningCards"];

            PokerWinManager.ins.StartShowDown(showDownPot, winningSeats, winType, winningCards);
        }

        if (hashtable.ContainsKey("gameComplete")) { restartBtn.SetActive(true); }
    }

    public void RestartBtn()
    {
        NetworkRoomClient.ins.LeaveGameRoom(() => { UnityEngine.SceneManagement.SceneManager.LoadScene("PokerRoom_2"); });
    }
}
