using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class ClientSend : MonoBehaviour
{
    private static void SendReliableData(Packet _packet)
    {
        _packet.WriteLength();
        _packet.InsertByte(steamNetworkClient.myID);
        SteamManager.connection.Connection.SendMessage(_packet.ToIntPtr(), _packet.Length(), Steamworks.Data.SendType.Reliable);
    }
    private static void SendUnreliableData(Packet _packet)
    {
        _packet.WriteLength();
        _packet.InsertByte(steamNetworkClient.myID);
        SteamManager.connection.Connection.SendMessage(_packet.ToIntPtr(), _packet.Length(), Steamworks.Data.SendType.Unreliable);
    }

    static byte floatToByte(float min, float max, float v)
    {
        return (byte)Mathf.RoundToInt(min + (v - min) * (255) / (max - min));
    }

    #region Packets
    public static void acceptConnection()
    {
        using (Packet _packet = new Packet((ushort)ClientPackets.connectionAccepted))
        {
            _packet.Write(steamNetworkClient.myID);
            _packet.Write(SteamManager.instance.steamID);
            _packet.Write(SteamClient.Name);

            SendReliableData(_packet);
        }
    }
    public static void pong(float time)
    {
        using (Packet _packet = new Packet((ushort)ClientPackets.pong))
        {
            _packet.Write(time);

            SendReliableData(_packet);
        }
    }
    public static void sendInputToServer(uint tick, bool[] inputs, float rotation, float yRot)
    {
        using (Packet _packet = new Packet((ushort)ClientPackets.inputs))
        {
            _packet.Write(tick);
            _packet.Write(inputs);
            _packet.Write(rotation);
            _packet.Write(yRot);

            SendUnreliableData(_packet);
        }
    }
    public static void sendVoice(byte[] data)
    {
        using (Packet _packet = new Packet((ushort)ClientPackets.voiceChat))
        {
            _packet.Write(data);

            SendUnreliableData(_packet);
        }
    }
    #endregion
}
