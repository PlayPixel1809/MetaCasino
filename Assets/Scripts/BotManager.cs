using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BotManager : MonoBehaviourPunCallbacks
{
    public static BotManager ins;
    public void Awake() { ins = this; }

    public int botsCount = 2;


    public List<Player> bots;
    public Action<List<KeyValue>> onBotInitiated;
    public Action<int> onBotCreated;

    void Start()
    {
        NetworkRoom.ins.onRoomCreated += CreateInitialBots;

        TurnGame.ins.onTurn += (s) =>
        {
            //if (NetworkRoom.ins.seats[s] < 0) { CreateMoveForBot(s); }
        };
    }

    public Player GetBot(int botIndex)
    {
        Player bot = new Player();
        bot.NickName = (string)ph.GetRoomData("bot" + botIndex + "Name");
        bot.CustomProperties.Add("balance", (float)ph.GetRoomData("bot" + botIndex + "Balance"));

        return bot;
    }


    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        
    }

    void CreateInitialBots()
    {
        for (int i = 0; i < botsCount; i++) { CreateBot(); }
    }

    void CreateGameplayBots()
    {
        ph.SetRoomData("BotsCreationStatus", "CreatingGameplayBots");
        StartCoroutine("ManageBots");
    }

    void CreateGameRestartBots()
    {
        ph.SetRoomData("BotsCreationStatus", "CreatingRestartBots");
        StartCoroutine("ManageBots");
    }

    void CreateBots(float randDelay)
    {
        StopCoroutine("CreateBotsCoroutine");
        StartCoroutine("CreateBotsCoroutine", randDelay);
    }


    IEnumerator CreateBotsCoroutine(float randDelay)
    {
        while (true)
        {
            //yield return new WaitForSeconds(Random.Range(1.5f, randDelay));
        }
    }

    void CreateBot()
    {
        List<int> seatsList = new List<int>();
        for (int i = 0; i < NetworkRoom.ins.seats.Length; i++) { seatsList.Add(NetworkRoom.ins.seats[i]); }

        int botRoomIndex = 0;
        for (int i = -1; i > -100; i--)
        {
            if (!seatsList.Contains(i)) 
            { 
                botRoomIndex = i; 
                break; 
            }
        }

        ph.SetRoomData("bot" + botRoomIndex + "username", "Bot " + MathF.Abs(botRoomIndex));
        ph.SetRoomData("bot" + botRoomIndex + "balance", (float)(-botRoomIndex * 40000));

        NetworkRoom.ins.OnPlayerEnteredRoom(new Player() { ActorNumber = botRoomIndex });
    }


    void CreateMoveForBot(int seatIndex)
    {
        Utils.InvokeDelayedAction(UnityEngine.Random.Range(1.5f, TurnGame.ins.turnTime * .8f), () =>
        {
            float currentBet = Poker.ins.GetHighestBet();
            float playerBet = Poker.ins.playersBetsForRound[seatIndex]; 

            int rand = UnityEngine.Random.Range(1, 11);

            string moveName = string.Empty;
            float moveAmount = 0;

            if (rand < 2) { moveName = "FOLD"; }
            if (rand > 0 && rand < 7)
            {
                if (playerBet == currentBet) { moveName = "CHECK"; }
                if (playerBet < currentBet) { moveName = "CALL"; moveAmount = currentBet - playerBet; }
            }

            if (rand > 6)
            {
                moveAmount = (currentBet - playerBet) + 500.0f * UnityEngine.Random.Range(1, 6);
                if (currentBet == 0) { moveName = "BET"; } else { moveName = "RAISE"; }
            }

            float playerBalance = (float)ph.GetPlayerData(NetworkRoom.ins.seats[seatIndex], "balance");
            if (playerBalance < moveAmount) { moveAmount = playerBalance; moveName = "ALL-IN"; }

            ExitGames.Client.Photon.Hashtable data = new ExitGames.Client.Photon.Hashtable();
            data.Add("moveMade", moveName);
            if (moveAmount > 0) { data.Add("moveAmount", moveAmount); }

            ServerClientBridge.ins.onClientMsgRecieved.Invoke(NetworkRoom.ins.seats[seatIndex], data);
        });

    }

}
