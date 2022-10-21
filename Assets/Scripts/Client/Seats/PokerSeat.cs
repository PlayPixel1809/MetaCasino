using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PokerSeat : MonoBehaviour
{
    public NetworkGameSeat networkGameSeat;
    public TurnGameSeat turnGameSeat;
    public CardGameSeat cardGameSeat;

    void Start()
    {
        networkGameSeat.onSeatOccupied += () =>
        {
            ServerClientBridge.ins.onServerMsgRecieved += OnServerMsgRecieved;
        };

        networkGameSeat.onSeatVaccated += () =>
        {
            ServerClientBridge.ins.onServerMsgRecieved -= OnServerMsgRecieved;
        };
    }

    

    public void OnServerMsgRecieved(ExitGames.Client.Photon.Hashtable properties)
    {
        if (properties.ContainsKey("cardsDistributed"))
        {
            int dealerIndex = (int)ph.GetRoomData("dealer");
            if (dealerIndex == networkGameSeat.GetSeatIndex())
            { MakeMove("S-BLIND", (float)ph.GetRoomData("minBet")); }

            if (TurnGame.ins.GetNextTurnIndex(dealerIndex) == networkGameSeat.GetSeatIndex())
            { MakeMove("B-BLIND", (float)ph.GetRoomData("minBet") * 2); }
        }

        if (properties["turn"] != null)
        {
            if ((int)properties["turn"] == networkGameSeat.GetSeatIndex() && networkGameSeat.player.ActorNumber < 0 && ph.IsMasterClient()) { CreateMoveForBot(); }
        }

        if (properties["roundEnd"] != null) 
        {
            Vector3 from = networkGameSeat.chips.transform.position;
            networkGameSeat.moveAndBetInfo.gameObject.SetActive(false);
            AnimUtils.Transform(networkGameSeat.chips.transform, from, NetworkGameClient.ins.tableChips.transform.position, 1, Space.World, GameUtils.ins.easeIn, ()=> 
            {
                NetworkGameClient.ins.tablePot.TakeAmount(networkGameSeat.moveAndBetInfo.bet, networkGameSeat.moveAndBetInfo.bet.GetPotAmount());
                NetworkGameClient.ins.tableChips.SetActive(true);
                networkGameSeat.chips.transform.position = from;
                networkGameSeat.chips.SetActive(false);
                //Utils.InvokeDelayedAction(.25f,()=> {  });
            });
        }
    }

    public void MakeMove(string moveType, float amount = 0)
    {
        networkGameSeat.moveAndBetInfo.MakeMove(moveType);
        networkGameSeat.moveAndBetInfo.MakeBet(networkGameSeat.playerInfo.balance, amount);
        networkGameSeat.chips.SetActive(true);
    }

    void CreateMoveForBot()
    {
        Utils.InvokeDelayedAction(Random.Range(1.5f,TurnGame.ins.turnTime*.8f), () =>
        {
            /*float currentBet = (float)ph.GetRoomData("currentBet");
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
            ph.SetRoomData(data);*/

        });
        
    }
}
