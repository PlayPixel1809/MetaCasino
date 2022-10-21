using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnGame1 : MonoBehaviour
{
    public static TurnGame1 ins;
    void Awake() { ins = this; }

    public Transform cam;
    public Transform camLookAtPoint;

    public int startGameTimer = 10;
    public int turnTime = 10;

    public Pot tablePot;
    public GameObject tableChips;
    public Text counter;

    public List<TurnGameSeat> turnGameSeats;

    public GameObject lpControls;
    public PlayerInfoUi lpInfoUi;
    public Timer lpTimer;

    public Action onGameStart;
    public Action onGameRestart;

    [Header("Assigned During Game -")]
    public int[] seats;
    public int dealer = -1;
    public bool[] turnEligiblePlayers;
    public bool[] turnRecievedPlayers;
    public float[] playersBets;
    public float currentBet;
    public float pot;

    private Coroutine waitForTurnCoroutine;

    public Action<int> onDealerSet;

    void Start()
    {
        dealer = -1;

        NetworkGame.ins.onGameStart += () =>
        {
            turnEligiblePlayers = new bool[NetworkGame.ins.seats.Length];
            for (int i = 0; i < NetworkGame.ins.seats.Length; i++) { if (NetworkGame.ins.seats[i] != 0) { turnEligiblePlayers[i] = true; } }
            NetworkGame.ins.SyncData("turnEligiblePlayers", turnEligiblePlayers);

            dealer = GetNextTurnIndex((int)ph.GetRoomData("dealer"));
            NetworkGame.ins.SyncData("dealer", dealer);
            onDealerSet?.Invoke(dealer);
        };




        tablePot.SetPotAmount(0);

        if (User.localUser != null)
        { ph.SetLocalPlayerData("balance", User.localUser.balance); }
        else
        { User.onCreateLocalUser += () => { ph.SetLocalPlayerData("balance", User.localUser.balance); }; }

        playersBets = new float[turnGameSeats.Count];
        Room.ins.startProperties.Add("playersBets", playersBets);

        turnRecievedPlayers = new bool[turnGameSeats.Count];
        Room.ins.startProperties.Add("turnRecievedPlayers", turnRecievedPlayers);

        Room.ins.startProperties.Add("minBet", (float)250);
        Room.ins.startProperties.Add("minBalance", (float)10000);
        Room.ins.startProperties.Add("seats", new int[turnGameSeats.Count]);
        Room.ins.startProperties.Add("dealer", -1);
        Room.ins.startProperties.Add("turn", 0);
        Room.ins.startProperties.Add("pot", 0);
        Room.ins.startProperties.Add("startCounter", startGameTimer);
        Room.ins.startProperties.Add("currentBet", (float)0);

        BotManager.ins.onBotInitiated += (p) => { p.Add(new KeyValue() { key = "balance", obj = (float)100000 }); };
        BotManager.ins.onBotCreated += (actorNo) => { AssignSeatToPlayer(actorNo); };

        Room.ins.onRoomJoined += (p) => { AssignSeatToPlayer(p.ActorNumber); };
        Room.ins.onPlayerEnteredRoom += (p) => { AssignSeatToPlayer(p.ActorNumber); };
        Room.ins.onPlayerLeftRoom += OnPlayerLeftRoom;
        Room.ins.onRoomPropertiesChanged += OnRoomPropertiesChanged;
    }

    

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.B)) { StopCoroutine(waitForTurnCoroutine); }
    }

    

    void OnRoomPropertiesChanged(ExitGames.Client.Photon.Hashtable hashtable)
    {
        //if (hashtable.ContainsKey("data")) { Utils.InvokeDelayedAction(0, () => { Debug.Log((string)ph.GetRoomData("data")); }); }
        if (hashtable.ContainsKey("seats"))
        {
            seats = (int[])hashtable["seats"];
            for (int i = 0; i < seats.Length; i++)
            {
                //if (seats[i] != 0 && turnGameSeats[i].player == null) { PlayerEnteredTable(seats[i], i); }
                //if (seats[i] == 0 && turnGameSeats[i].player != null) { PlayerLeftTable(i); }
            }
        }

        if (hashtable.ContainsKey("startCounter"))
        {
            int count = (int)hashtable["startCounter"];
            counter.gameObject.SetActive(true);
            counter.text = count.ToString();
            if (count == 0) { counter.gameObject.SetActive(false); }
        }

        if (hashtable.ContainsKey("moveMade") && ph.IsMasterClient()) 
        { 
            StopCoroutine(waitForTurnCoroutine);
            if (hashtable.ContainsKey("moveAmount"))
            {
                float bet = (float)ph.GetRoomData("moveAmount");
                int turn = (int)ph.GetRoomData("turn");
                float playerTotalBetAmount = playersBets[turn] + bet;
                playersBets[turn] = playerTotalBetAmount;
                ph.SetRoomData("playersBets", playersBets);

                currentBet = (float)ph.GetRoomData("currentBet");
                if (currentBet < playerTotalBetAmount)
                {
                    currentBet = playerTotalBetAmount;
                    ph.SetRoomData("currentBet", currentBet);
                }
            }

            Utils.FrameDelayedAction(()=> { ph.SetRoomData("canStartNextTurn", true); }); 
        }

    }


    public void AssignSeatToPlayer(int playerRoomIndex)
    {
        if (!ph.IsMasterClient()) { return; }

        seats = (int[])ph.GetRoomData("seats");

        List<int> availableSeats = new List<int>();
        for (int i = 0; i < seats.Length; i++) { if (seats[i] == 0) { availableSeats.Add(i); } }
        int randSeat = availableSeats[UnityEngine.Random.Range(0, availableSeats.Count)];
        availableSeats.Remove(randSeat);
        seats[randSeat] = playerRoomIndex;

        ph.SetRoomData("seats", seats);

        int filledSeats = seats.Length - availableSeats.Count;
        if (filledSeats == 2) { StartCoroutine("StartGameCounter", startGameTimer); }
    }

    void OnPlayerLeftRoom(Player player)
    {
        if (!ph.IsMasterClient()) { return; }
        seats = (int[])ph.GetRoomData("seats");
        seats[turnGameSeats.IndexOf(GetTurnGameSeat(player))] = 0;
        ph.SetRoomData("seats", seats);
    }

    //void PlayerEnteredTable(int actorNo, int seatIndex) { turnGameSeats[seatIndex].OccupySeat(ph.GetPlayer(actorNo)); }
    //void PlayerLeftTable(int seatIndex) { turnGameSeats[seatIndex].VaccateSeat(); }

    IEnumerator StartGameCounter(int startFrom)
    {
        for (int counter = startFrom; counter > -1; counter--)
        {
            ph.SetRoomData("startCounter", counter);
            yield return new WaitForSeconds(1);
        }
        StartGame();
    }

    public void StartGame()
    {
        turnEligiblePlayers = new bool[turnGameSeats.Count];
        for (int i = 0; i < seats.Length; i++) { if (seats[i] != 0) { turnEligiblePlayers[i] = true; } }
        ph.SetRoomData("turnEligiblePlayers", turnEligiblePlayers);

        ph.SetRoomData("dealer", GetNextTurnIndex((int)ph.GetRoomData("dealer")));
    }

    public void StartTurn(int turnIndex, Action onPlayerDidntRespond)
    {
        turnRecievedPlayers = (bool[])ph.GetRoomData("turnRecievedPlayers");
        turnRecievedPlayers[turnIndex] = true;
        ph.SetRoomData("turn", turnIndex);
        ph.SetRoomData("turnRecievedPlayers", turnRecievedPlayers);
        waitForTurnCoroutine = StartCoroutine(WaitForTurnResponse(turnTime, onPlayerDidntRespond));
    }

    public IEnumerator WaitForTurnResponse(int waitTime, Action onPlayerDidntRespond)
    {
        for (int counter = waitTime; counter > -1; counter--)
        {
            ph.SetRoomData("turnResponseTimer", counter);
            yield return new WaitForSeconds(1);
        }
        onPlayerDidntRespond?.Invoke();
    }

    public void RestartGame()
    {
        tableChips.SetActive(false);
        tablePot.gameObject.SetActive(false);

        playersBets = new float[turnGameSeats.Count];
        ph.SetRoomData("playersBets", playersBets);

        turnRecievedPlayers = new bool[turnGameSeats.Count];
        ph.SetRoomData("turnRecievedPlayers", turnRecievedPlayers);
    }

    public TurnGameSeat GetTurnGameSeat(Player player)
    {
        //for (int i = 0; i < turnGameSeats.Count; i++) { if (turnGameSeats[i].player == player) { return turnGameSeats[i]; } }
        return null;
    }
    
    public int GetNextTurnIndex(int currentTurnIndex)
    {
        int iterations = turnEligiblePlayers.Length - 1;
        int nextTurnIndex = currentTurnIndex;
        while (iterations > 0)
        {
            iterations -= 1;
            nextTurnIndex += 1;
            if (nextTurnIndex == turnGameSeats.Count) { nextTurnIndex = 0; }
            if (turnEligiblePlayers[nextTurnIndex]) { return nextTurnIndex; }
        }
        return -1;
    }

    public int GetTurnEligiblePlayersCount()
    {
        int count = 0;
        for (int i = 0; i < turnGameSeats.Count; i++)
        {
            //if (turnGameSeats[i].player != null)
            //{
           //     if ((bool)ph.GetPlayerData(turnGameSeats[i].player, "turnEligible")) { count += 1; }
           // }
        }
        return count;
    }
}
