using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PokerSeat : MonoBehaviour
{
    
    public Pocket roundBet;

    [HideInInspector] public NetworkRoomSeat networkRoomSeat;
    [HideInInspector] public NetworkGameSeat networkGameSeat;
    [HideInInspector] public TurnGameSeat turnGameSeat;
    [HideInInspector] public CardGameSeat cardGameSeat;

    void Start()
    {
        networkRoomSeat = GetComponent<NetworkRoomSeat>();
        networkGameSeat = GetComponent<NetworkGameSeat>();
        turnGameSeat = GetComponent<TurnGameSeat>();
        cardGameSeat = GetComponent<CardGameSeat>();

        networkRoomSeat.onSeatOccupied += () =>
        {
            roundBet.Reset();
        };

        networkRoomSeat.onSeatVaccated += () =>
        {
            roundBet.Reset();
        };
    }

    public void StartTurn(float currentBet, float playerBet)
    {
        float balance = (float)ph.GetPlayerData(networkRoomSeat.player, "balance");

        if (networkRoomSeat.actorNo > 0)
        {
            if (networkRoomSeat.player.IsLocal) { PokerClient.ins.lpControls.ActivateControls(currentBet, playerBet, balance); }
        }

        if (networkRoomSeat.actorNo < 0 && ph.IsMasterClient())
        {
            if (BotManager.ins.playForBots) { PokerClient.ins.lpControls.ActivateControls(currentBet, playerBet, balance); }
            else
            { 
                CreateMoveForBot(currentBet, playerBet, balance); 
            }
        }
    }


    public void DeactivateMoveInfo()
    {
        if (turnGameSeat.moveMade.GetLabel().ToLower() != "all in" && turnGameSeat.moveMade.GetLabel().ToLower() != "fold")
        {
            turnGameSeat.ResetMoveMade(); 
        }
    }

    public void MakeBet(float amount)
    {
        roundBet.AddAmount(amount);
    }

    public void MakePlayerFoldInAllPots()
    {
        MakePlayerFoldInPot(NetworkGameClient.ins.gamePot);
        for (int i = 0; i < PokerClient.ins.sidePots.Length; i++) { MakePlayerFoldInPot(PokerClient.ins.sidePots[i]); }
    }

    void MakePlayerFoldInPot(Pot pot)
    {
        if (pot.gameObject.activeInHierarchy)
        {
            for (int i = 0; i < pot.depositors.childCount; i++)
            {
                if (pot.depositors.GetChild(i).GetComponent<Text>().text == ph.GetPlayerNickname(networkRoomSeat.player)) 
                { pot.depositors.GetChild(i).GetComponent<Text>().text = pot.depositors.GetChild(i).GetComponent<Text>().text + " (fold)"; }
            }
        }
    }

    public void SubmitRoundBet()
    {
        if (roundBet.GetAmount() > 0)
        {
            Vector3 from = roundBet.amountGraphic.transform.position;
            AnimUtils.Transform(roundBet.amountGraphic.transform, from, NetworkGameClient.ins.gamePot.amountGraphic.transform.position, 1, Space.World, GameUtils.ins.easeIn, () =>
            {
                roundBet.Reset();
            });
        }
    }

    void CreateMoveForBot(float currentBet, float playerBet, float playerBalance)
    {
        Utils.InvokeDelayedAction(Random.Range(1.5f,TurnGame.ins.turnTime*.6f), () =>
        {
            int rand = Random.Range(1, 11);

            string moveName = string.Empty;
            float moveAmount = 0;

            if (rand < 2) { moveName = "FOLD"; }
            if (rand > 1 && rand < 7)
            {
                if (playerBet == currentBet) { moveName = "CHECK"; }
                if (playerBet < currentBet) { moveName = "CALL"; moveAmount = currentBet - playerBet; }
            }

            if (rand > 6)
            {
                moveAmount = (currentBet - playerBet) + 1000f * Random.Range(1, 6);
                if (currentBet == 0) { moveName = "BET"; } else { moveName = "RAISE"; }
            }

            if (playerBalance < moveAmount) { moveAmount = playerBalance; moveName = "ALL IN"; }

            ExitGames.Client.Photon.Hashtable data = new ExitGames.Client.Photon.Hashtable();
            data.Add("moveMade", moveName);
            if (moveAmount > 0) { data.Add("moveAmount", moveAmount); }
            ServerClientBridge.ins.NotifyServer("ExecuteTurn" + TurnGameClient.ins.turn, data);
        });
        
    }
}
