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


    public Pot mainPot;
    public Pot[] sidePots;

    public GameObject restartBtn;

    [HideInInspector] public float smallBlind;

    //[HideInInspector]
    public List<PokerSeat> seats;

    void Start()
    {
        StartCoroutine("AssignSeats");
        ServerClientBridge.ins.onServerMsgRecieved += OnMsgRecieved;
    }

    IEnumerator AssignSeats()
    {
        yield return new WaitForEndOfFrame();
        seats = new List<PokerSeat>();
        for (int i = 0; i < NetworkRoomClient.ins.seats.Count; i++) { seats.Add(NetworkRoomClient.ins.seats[i].GetComponent<PokerSeat>()); }
    }

    public void OnMsgRecieved(ExitGames.Client.Photon.Hashtable hashtable)
    {
        if (hashtable.ContainsKey("gameStartCounter"))
        {
            for (int i = 0; i < seats.Count; i++) { seats[i].roundBet.ResetAmount(); }
        }

        if (hashtable.ContainsKey("smallBlindSeat")) 
        {
            smallBlind = (float)hashtable["smallBlindAmount"];
            TurnGameClient.ins.seats[(int)hashtable["smallBlindSeat"]].MakeMove("S-BLIND", smallBlind);
            seats[(int)hashtable["smallBlindSeat"]].MakeBet(smallBlind);
        }

        if (hashtable.ContainsKey("bigBlindSeat"))   
        { 
            TurnGameClient.ins.seats[(int)hashtable["bigBlindSeat"]].MakeMove("B-BLIND", (float)hashtable["bigBlindAmount"]);
            seats[(int)hashtable["bigBlindSeat"]].MakeBet((float)hashtable["bigBlindAmount"]);
        }

        if (hashtable.ContainsKey("getPokerMove"))
        {
            seats[TurnGameClient.ins.turn].GetPokerMove((float)hashtable["currentBet"], (float)hashtable["playerBet"]);
        }

        if (hashtable.ContainsKey("gameStartCounter"))
        {
            for (int i = 0; i < seats.Count; i++) { seats[i].roundBet.ResetAmount(); }
        }

        if (hashtable["moveMade"] != null && (string)hashtable["moveMade"] == "FOLD") { seats[(int)hashtable["moveMadeBy"]].MakePlayerFoldInAllPots(); }

        if (hashtable["moveAmount"] != null) { seats[(int)hashtable["moveMadeBy"]].MakeBet((float)hashtable["moveAmount"]); }

        if (hashtable.ContainsKey("endRound"))
        {
            for (int i = 0; i < seats.Count; i++) { seats[i].DeactivateMoveInfo(); }
        }

        if (hashtable.ContainsKey("submitRoundBet"))
        {
            for (int i = 0; i < seats.Count; i++) { seats[i].SubmitRoundBet(); }
        }

        if (hashtable.ContainsKey("mainPotAmount"))
        {
            mainPot.AddAmount((float)hashtable["mainPotAmount"], (int[])hashtable["mainPotSeats"]); 
        }

        if (hashtable.ContainsKey("sidePotAmount"))
        {
            mainPot.gameObject.SetActive(false);
            for (int i = 0; i < sidePots.Length; i++)
            {
                if (!sidePots[i].gameObject.activeInHierarchy) { sidePots[i].AddAmount((float)hashtable["sidePotAmount"], (int[])hashtable["sidePotSeats"]); break; }
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
            Pot showDownPot = mainPot;
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

    public void QuitBtn()
    {
        NetworkRoomClient.ins.LeaveGameRoom(() => { UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby"); });
    }
}
