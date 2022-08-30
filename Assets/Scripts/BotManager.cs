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
        Utils.FrameDelayedAction(() =>
        {
            Room.ins.onRoomCreated += CreateInitialBots;
            
        },1);


       /* Room.onRoomCreated += CreateInitialBots;
        CardGame.onGameStart += CreateGameplayBots;
        CardGame.onGameRestart += CreateGameRestartBots;*/
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
        List<KeyValue> botProperties = new List<KeyValue>() { new KeyValue() { key = "username", obj = "Bot"} };
        onBotInitiated?.Invoke(botProperties);

        int botRoomIndex = 0;
        for (int i = -1; i > -100; i--)
        {
            if (!Room.ins.GetRoomProperties().ContainsKey("bot" + i + "username")) 
            { 
                botRoomIndex = i; 
                break; 
            }
        }
        for (int i = 0; i < botProperties.Count; i++) { ph.SetRoomData("bot" + botRoomIndex + botProperties[i].key, botProperties[i].GetVal()); }

        onBotCreated?.Invoke(botRoomIndex);
    }

}
