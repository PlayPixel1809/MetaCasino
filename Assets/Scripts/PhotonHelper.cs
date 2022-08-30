using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public static class ph //photon helper
{
    public static bool IsMasterClient()
    {
        if (PhotonNetwork.CurrentRoom == null) { return true; }
        return PhotonNetwork.LocalPlayer.IsMasterClient;
    }

    public static Player GetPlayer(int actorNo)
    {
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            if (PhotonNetwork.PlayerList[i].ActorNumber == actorNo) { return PhotonNetwork.PlayerList[i]; }
        }
        return new Player() { ActorNumber = actorNo };
    }

    public static bool IsPlayerOnline(string nickname)
    {
        for (int i = 0; i < PhotonNetwork.PlayerListOthers.Length; i++)
        {
            if (PhotonNetwork.PlayerListOthers[i].NickName == nickname) { return true; }
        }
        return false;
    }

    public static string GetUsername(int actorNo)
    {
        for (int i = 0; i < PhotonNetwork.PlayerListOthers.Length; i++)
        {
            if (PhotonNetwork.PlayerListOthers[i].ActorNumber == actorNo) { return PhotonNetwork.PlayerListOthers[i].NickName; }
        }
        return string.Empty;
    }

    public static int GetActorNo(string nickname)
    {
        for (int i = 0; i < PhotonNetwork.PlayerListOthers.Length; i++)
        {
            if (PhotonNetwork.PlayerListOthers[i].NickName == nickname) { return PhotonNetwork.PlayerListOthers[i].ActorNumber; }
        }
        return 0;
    }

    public static void SetRoomData(string key, object val)
    {
        PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { key, val } });
    }

    public static void SetRoomData(ExitGames.Client.Photon.Hashtable data)
    {
        PhotonNetwork.CurrentRoom.SetCustomProperties(data);
    }

    public static object GetRoomData(string key)
    {
        return PhotonNetwork.CurrentRoom.CustomProperties[key];
    }

    public static void ChangePlayerData(Player player, string key, float amount)
    {
        float newVal = (float)player.CustomProperties[key] + amount;
        SetPlayerData(player, key, newVal);
    }


    public static void ChangeLocalPlayerData(string key, float amount)
    {
        ChangePlayerData(PhotonNetwork.LocalPlayer, key, amount);
    }


    public static object GetPlayerData(Player player, string key)
    {
        if (player.ActorNumber > 0)
        {
            return player.CustomProperties[key];
        }
        if (player.ActorNumber < 0)
        {
            return GetRoomData("bot" + player.ActorNumber + key);
        }
        return null;
    }
    public static object GetPlayerData(int actorNo, string key) { return GetPlayerData(GetPlayer(actorNo), key); }


    public static string GetPlayerNickname(Player player)
    {
        if (!string.IsNullOrEmpty(player.NickName))
        {
            return player.NickName;
        }
        else
        {
            return (string)GetRoomData("bot" + player.ActorNumber + "username");
        }
    }

    public static void SetPlayerData(Player player, string key, object val)
    {
        if (player.IsLocal)
        {
            player.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { key, val } });
        }
        else
        {
            SetRoomData("bot" + player.ActorNumber + key, val);
        }
    }

    public static void RemovePlayerData(Player player)
    {
        player.CustomProperties = new ExitGames.Client.Photon.Hashtable();
    }

    public static void SetLocalPlayerData(string key, object val)
    {
        SetPlayerData(PhotonNetwork.LocalPlayer, key, val);
    }

    public static object GetLocalPlayerData(string key)
    {
        return GetPlayerData(PhotonNetwork.LocalPlayer, key);
    }

    

    
}
