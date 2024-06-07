using UnityEngine;
using Google.Protobuf;
using ServerCore;
using Protocol;
using System;

class PacketHandler
{
    public static void SEnterGameHandler(PacketSession session, IMessage packet)
    {
        S_ENTER_GAME enterGamePacket = packet as S_ENTER_GAME;
        ServerSession serverSession = session as ServerSession;

        Debug.Log("S_EnterGameHandler");
        Debug.Log(enterGamePacket.Player);
    }

    public static void SLeaveGameHandler(PacketSession session, IMessage packet)
    {
        S_LEAVE_GAME leaveGamePacket = packet as S_LEAVE_GAME;
        ServerSession serverSession = session as ServerSession;

        Debug.Log("S_LeaveGameHandler");
        Debug.Log(leaveGamePacket);
    }

    public static void SSpawnHandler(PacketSession session, IMessage packet)
    {
        S_SPAWN spawnPacket = packet as S_SPAWN;
        ServerSession serverSession = session as ServerSession;

        Debug.Log("S_SpawnHandler");
        Debug.Log(spawnPacket.Players);
    }

    public static void SDespawnHandler(PacketSession session, IMessage packet)
    {
        S_DESPAWN spawnPacket = packet as S_DESPAWN;
        ServerSession serverSession = session as ServerSession;

        Debug.Log("S_DespawnHandler");
        Debug.Log(spawnPacket);
    }

    public static void SMoveHandler(PacketSession session, IMessage packet)
    {
        S_MOVE movePacket = packet as S_MOVE;
        ServerSession serverSession = session as ServerSession;
    }

    public static void SChatHandler(PacketSession session, IMessage packet)
    {
        S_CHAT chatPacket = packet as S_CHAT;
        ServerSession serverSession = session as ServerSession;

        Debug.Log(chatPacket.Msg);
    }
}