using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerHandle
{
    static float byteToFloat(float min, float max, byte t)
    {
        return Mathf.Lerp(min, max, (float)(t / 255f));
    }

    public static void ConnectionAccepted(byte _fromClient, Packet _packet)
    {
        byte _clientIdCheck = _packet.ReadByte();
        ulong _steamID = _packet.ReaduLong();
        string _username = _packet.ReadString();

        Debug.Log($"Steam player connected successfully and is now player {_fromClient}. their username is {_username} and their steam id is {_steamID}");
        if (_fromClient != _clientIdCheck)
        {
            Debug.Log($"Player \"{_username}\" (ID: {_fromClient}) has assumed the wrong client ID ({_clientIdCheck})!");
        }
        Server.clients[_fromClient].ConnectionConfirmed(_username, _steamID);
    }
    public static void ManageInputs(byte _fromClient, Packet _packet)
    {
        uint tick = _packet.ReadUInt();
        bool[] inputs = _packet.ReadBoolArray();
        float rotation = _packet.ReadFloat();
        float yRot = _packet.ReadFloat();
        if (!NetworkTest.clients.ContainsKey(_fromClient))
        {
            return;
        }
        NetworkTest.clients[_fromClient].setRotation(rotation, yRot);
        NetworkTest.clients[_fromClient].setInput(tick, inputs);

        ServerSend.returnInputs(_fromClient, inputs);
    }
    public static void Pong(byte _fromClient, Packet _packet)
    {
        float t = _packet.ReadFloat();
        ushort p = (ushort)((Time.time - t) * 1000);
        Server.clients[_fromClient].ping = p;
    }
    public static void VoiceChat(byte _fromClient, Packet _packet)
    {
        byte[] data = _packet.ReadBytes(_packet.UnreadLength());

        ServerSend.voiceChat(_fromClient, data);
    }
}
