using Steamworks;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class ClientHandle : MonoBehaviour
{
    static float byteToFloat(float min, float max, byte t)
    {
        return Mathf.Lerp(min, max, (float)(t / 255f));
    }
    public static void ConnectionAccepted(Packet _packet)
    {
        byte id = _packet.ReadByte();
        int tickRate = _packet.ReadInt();
        Constants.TICKS_PER_SEC = tickRate;
        Constants.MS_PER_TICK = 1000f / Constants.TICKS_PER_SEC;
        Time.fixedDeltaTime = (float)(1f / (float)Constants.TICKS_PER_SEC);
        steamNetworkClient.myID = id;
        ClientSend.acceptConnection();
    }
    public static void ConnectionConfirmed(Packet _packet)
    {
        byte id = _packet.ReadByte();
        ulong steamID = _packet.ReaduLong();
        string userName = _packet.ReadString();
        NetworkTest.instance.playerConnected(id, steamID, userName);
    }
    public static void ClientDisconnected(Packet _packet)
    {
        byte id = _packet.ReadByte();
        NetworkTest.instance.playerDisconnected(id);
    }
    public static void displayInputs(Packet _packet)
    {
        byte id = _packet.ReadByte();
        bool[] inputs = _packet.ReadBoolArray();
        ushort ping = _packet.ReadUShort();
        NetworkTest.instance.test(id, inputs, ping);
    }
    public static void Ping(Packet _packet)
    {
        float t = _packet.ReadFloat();
        ClientSend.pong(t);
    }
    public static void MovePlayer(Packet _packet)
    {
        if (SteamManager.server != null)
        {
            return;
        }
        byte _id = _packet.ReadByte();
        Vector3 position = _packet.ReadVector3();
        uint tick = _packet.ReadUInt();

        if (NetworkTest.clients.ContainsKey(_id))
        {
            NetworkTest.clients[_id].setPosition(tick, position);
        }
    }
    public static void RotatePlayer(Packet _packet)
    {
        if (SteamManager.server != null)
        {
            return;
        }
        byte _id = _packet.ReadByte();
        float rotation = _packet.ReadFloat();
        float _yRot = _packet.ReadFloat();

        if (NetworkTest.clients.ContainsKey(_id))
        {
            NetworkTest.clients[_id].setRotation(rotation, _yRot);
        }
    }
    public static void VoiceChat(Packet _packet)
    {
        byte p = _packet.ReadByte();
        byte[] data = _packet.ReadBytes(_packet.UnreadLength());
        NetworkTest.clients[p].GetComponent<Character>().voiceChat(data);
    }
}
