using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks.Data;
using System.IO;
using System;

public class ServerSend
{
    private static void SendReliableData(byte _toClient, Packet _packet)
    {
        _packet.WriteLength();
        IntPtr pack = _packet.ToIntPtr();
        Server.clients[_toClient].connection.SendMessage(pack, _packet.Length(), SendType.Reliable);
    }
    private static void SendUnreliableData(byte _toClient, Packet _packet)
    {
        _packet.WriteLength();
        IntPtr pack = _packet.ToIntPtr();
        Server.clients[_toClient].connection.SendMessage(pack, _packet.Length(), SendType.Unreliable);
    }
    private static void SendReliableDataToAll(Packet _packet)
    {
        _packet.WriteLength();
        IntPtr pack = _packet.ToIntPtr();
        for (byte i = 1; i <= Server.MaxPlayers; i++)
        {
            Server.clients[i].connection.SendMessage(pack, _packet.Length(), SendType.Reliable);
        }
    }
    private static void SendUnreliableDataToAll(Packet _packet)
    {
        _packet.WriteLength();
        IntPtr pack = _packet.ToIntPtr();
        for (byte i = 1; i <= Server.MaxPlayers; i++)
        {
            Server.clients[i].connection.SendMessage(pack, _packet.Length(), SendType.Unreliable);
        }
    }
    private static void SendReliableDataToAll (int _exceptClient, Packet _packet)
    {
        _packet.WriteLength();
        IntPtr pack = _packet.ToIntPtr();
        for (byte i = 1; i <= Server.MaxPlayers; i++)
        {
            if (i != _exceptClient)
            {
                Server.clients[i].connection.SendMessage(pack, _packet.Length(), SendType.Reliable);
            }
        }
    }
    private static void SendUnreliableDataToAll(int _exceptClient, Packet _packet)
    {
        _packet.WriteLength();
        IntPtr pack = _packet.ToIntPtr();
        for (byte i = 1; i <= Server.MaxPlayers; i++)
        {
            if (i != _exceptClient)
            {
                Server.clients[i].connection.SendMessage(pack, _packet.Length(), SendType.Unreliable);
            }
        }
    }
    static byte floatToByte(float min, float max, float v)
    {
        return (byte)Mathf.RoundToInt(min + (v - min) * (255) / (max - min));
    }

    #region Packets
    public static void ConnectionAccepted(byte _toClient)
    {
        using (Packet _packet = new Packet((ushort)ServerPackets.acceptConnection))
        {
            _packet.Write(_toClient);
            _packet.Write(SteamNetworkManager.instance.tickRate);

            SendReliableData(_toClient, _packet);
        }
    }
    public static void ConnectionConfirmed(byte toSend, byte id, ulong steamID, string userName)
    {
        using (Packet _packet = new Packet((ushort)ServerPackets.connectionConfirmed))
        {
            _packet.Write(id);
            _packet.Write(steamID);
            _packet.Write(userName);

            SendReliableData(toSend, _packet);
        }
    }
    public static void ClientDisconnected(byte client)
    {
        using (Packet _packet = new Packet((ushort)ServerPackets.clientDisconnected))
        {
            _packet.Write(client);

            SendReliableDataToAll(_packet);
        }
    }
    public static void returnInputs(byte client, bool[] inputs)
    {
        using (Packet _packet = new Packet((ushort)ServerPackets.returnInputs))
        {
            _packet.Write(client);
            _packet.Write(inputs);
            _packet.Write(Server.clients[client].ping);

            SendUnreliableDataToAll(_packet);
        }
    }
    public static void ping(byte client)
    {
        using (Packet _packet = new Packet((ushort)ServerPackets.ping))
        {
            _packet.Write(Time.time);

            SendReliableData(client, _packet);
        }
    }
    public static void sendPosition(byte client, Vector3 position, uint tick)
    {
        using (Packet _packet = new Packet((ushort)ServerPackets.playerPos))
        {
            _packet.Write(client);
            _packet.Write(position);
            _packet.Write(tick);

            SendUnreliableDataToAll(_packet);
        }
    }
    public static void sendRotation(byte client, float rotation, float yrot)
    {
        using (Packet _packet = new Packet((ushort)ServerPackets.playerRot))
        {
            _packet.Write(client);
            _packet.Write(rotation);
            _packet.Write(yrot);

            SendUnreliableDataToAll(client, _packet);
        }
    }
    public static void voiceChat(byte p, byte[] data)
    {
        using (Packet _packet = new Packet((ushort)ServerPackets.voiceChat))
        {
            _packet.Write(p);
            _packet.Write(data);

            SendUnreliableDataToAll(p, _packet);
        }
    }
    #endregion
}
