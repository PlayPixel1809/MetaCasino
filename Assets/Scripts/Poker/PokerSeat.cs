using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PokerSeat : MonoBehaviour
{
    private TurnGameSeat turnGameSeat;
    private CardGameSeat cardGameSeat;

    void Start()
    {
        turnGameSeat = GetComponent<TurnGameSeat>();
        cardGameSeat = GetComponent<CardGameSeat>();
        turnGameSeat.onSeatOccupy += SeatOccupied;
    }

    void SeatOccupied()
    {
        Room.ins.onRoomPropertiesChanged += OnRoomPropertiesChanged;
    }

    public void OnRoomPropertiesChanged(ExitGames.Client.Photon.Hashtable properties)
    {
        if (properties.ContainsKey("cardsDistributed"))
        {
            int dealerIndex = (int)ph.GetRoomData("dealer");
            if (dealerIndex == turnGameSeat.GetSeatIndex())
            { MakeMove("S-BLIND", (float)ph.GetRoomData("minBet")); }

            if (TurnGame.ins.GetNextTurnIndex(dealerIndex) == turnGameSeat.GetSeatIndex())
            { MakeMove("B-BLIND", (float)ph.GetRoomData("minBet") * 2); }
        }

        if (properties["turn"] != null)
        {
            if ((int)properties["turn"] == turnGameSeat.GetSeatIndex() && turnGameSeat.player.ActorNumber < 0 && ph.IsMasterClient()) { CreateMoveForBot(); }
        }

        if (properties["roundEnd"] != null) 
        {
            Vector3 from = turnGameSeat.chips.transform.position;
            turnGameSeat.moveAndBetInfo.gameObject.SetActive(false);
            AnimUtils.Transform(turnGameSeat.chips.transform, from, TurnGame.ins.tableChips.transform.position, 1, Space.World, turnGameSeat.betAnimCurve, ()=> 
            {
                TurnGame.ins.tablePot.TakeAmount(turnGameSeat.moveAndBetInfo.bet, turnGameSeat.moveAndBetInfo.bet.GetPotAmount());
                TurnGame.ins.tableChips.SetActive(true);
                turnGameSeat.chips.transform.position = from;
                turnGameSeat.chips.SetActive(false);
                //Utils.InvokeDelayedAction(.25f,()=> {  });
            });
        }
    }

    void MakeMove(string moveType, float amount = 0)
    {
        turnGameSeat.moveAndBetInfo.MakeMove(moveType);
        turnGameSeat.moveAndBetInfo.MakeBet(turnGameSeat.playerInfo.balance, amount);
        turnGameSeat.chips.SetActive(true);
    }

    void CreateMoveForBot()
    {
        Utils.InvokeDelayedAction(Random.Range(1.5f,TurnGame.ins.turnTime*.8f), () =>
        {
            float currentBet = (float)ph.GetRoomData("currentBet");
            float playerBet = TurnGame.ins.playersBets[(int)ph.GetRoomData("turn")];

            int rand = Random.Range(1, 11);

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
                moveAmount = (currentBet - playerBet) + 500.0f * Random.Range(1, 6);
                if (currentBet == 0) { moveName = "BET"; } else { moveName = "RAISE"; }
            }

            float playerBalance = (float)ph.GetPlayerData(turnGameSeat.player, "balance");
            if (playerBalance < moveAmount) { moveAmount = playerBalance; moveName = "ALL-IN"; }

            ExitGames.Client.Photon.Hashtable data = new ExitGames.Client.Photon.Hashtable();
            data.Add("moveMade", moveName);
            if (moveAmount > 0) { data.Add("moveAmount", moveAmount); }
            ph.SetRoomData(data);

        });
        
    }
}
