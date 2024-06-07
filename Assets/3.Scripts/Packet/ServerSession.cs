using Google.Protobuf;
using Protocol;
using ServerCore;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class ServerSession : PacketSession
{
    public override void OnConnected(EndPoint endPoint)
    {
        if (endPoint == null)
            return;

        Debug.Log($"OnConnected : {endPoint}");
    }

    public override void OnDisconnected(EndPoint endPoint)
    {
        Debug.Log($"OnDisConnected : {endPoint}");
    }

    public override void OnRecvPacket(ArraySegment<byte> buffer)
    {
        Session session = this;

        var protocolId = BitConverter.ToUInt16(session._recvArgs.Buffer, 2);
        if (protocolId == 0)
            return;

        session.SetPacketId((Protocol.PacketId)protocolId);
        PacketId id = session.GetPacketId();

        PktHandler(buffer, id);
    }

    public override void OnSend(int numOfBytes)
    {
        Debug.Log($"SendPacket : {numOfBytes}");
    }

    #region PktHandler
    private void PktHandler(ArraySegment<byte> buffer, PacketId id)
    {
        switch (id)
        {
            case PacketId.PktCEnterGame:
                C_ENTER_GAME cEnterPkt = new C_ENTER_GAME();
                cEnterPkt.MergeFrom(buffer.Array, buffer.Offset + 4, buffer.Count - 4);
                PacketHandler.SEnterGameHandler(this, cEnterPkt);
                break;
            case PacketId.PktSEnterGame:
                S_ENTER_GAME sEnterPkt = new S_ENTER_GAME();
                sEnterPkt.MergeFrom(buffer.Array, buffer.Offset + 4, buffer.Count - 4);
                PacketHandler.SEnterGameHandler(this, sEnterPkt);
                break;
            case PacketId.PktCLeaveGame:
                C_LEAVE_GAME cLeavePkt = new C_LEAVE_GAME();
                cLeavePkt.MergeFrom(buffer.Array, buffer.Offset + 4, buffer.Count - 4);
                PacketHandler.SChatHandler(this, cLeavePkt);
                break;
            case PacketId.PktSLeaveGame:
                S_LEAVE_GAME sLeavePkt = new S_LEAVE_GAME();
                sLeavePkt.MergeFrom(buffer.Array, buffer.Offset + 4, buffer.Count - 4);
                PacketHandler.SLeaveGameHandler(this, sLeavePkt);
                break;
            case PacketId.PktSSpawn:
                S_SPAWN sSpawnPkt = new S_SPAWN();
                sSpawnPkt.MergeFrom(buffer.Array, buffer.Offset + 4, buffer.Count - 4);
                PacketHandler.SSpawnHandler(this, sSpawnPkt);
                break;
            case PacketId.PktSDespawn:
                S_DESPAWN sDespawnPkt = new S_DESPAWN();
                sDespawnPkt.MergeFrom(buffer.Array, buffer.Offset + 4, buffer.Count - 4);
                PacketHandler.SDespawnHandler(this, sDespawnPkt);
                break;
            case PacketId.PktCMove:
                C_MOVE cMovePkt = new C_MOVE();
                cMovePkt.MergeFrom(buffer.Array, buffer.Offset + 4, buffer.Count - 4);
                PacketHandler.SChatHandler(this, cMovePkt);
                break;
            case PacketId.PktSMove:
                S_MOVE sMovePkt = new S_MOVE();
                sMovePkt.MergeFrom(buffer.Array, buffer.Offset + 4, buffer.Count - 4);
                PacketHandler.SMoveHandler(this, sMovePkt);
                break;
            case PacketId.PktCChat:
                C_CHAT cChatPkt = new C_CHAT();
                cChatPkt.MergeFrom(buffer.Array, buffer.Offset + 4, buffer.Count - 4);
                PacketHandler.SChatHandler(this, cChatPkt);
                break;
            case PacketId.PktSChat:
                S_CHAT sChatPkt = new S_CHAT();
                sChatPkt.MergeFrom(buffer.Array, buffer.Offset + 4, buffer.Count - 4);
                PacketHandler.SChatHandler(this, sChatPkt);
                break;
            default:
                break;
        }
    }
    #endregion

}