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
    }


    

    public void DeactivateMoveInfo()
    {
        if (turnGameSeat.moveMade.GetLabel() != "ALL IN" && turnGameSeat.moveMade.GetLabel() != "FOLD")
        {
            turnGameSeat.ResetMoveMade(); 
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
