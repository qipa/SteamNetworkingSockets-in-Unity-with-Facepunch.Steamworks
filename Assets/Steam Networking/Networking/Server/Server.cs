using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Server
{
    public static byte MaxPlayers { get; private set; }
    public static Dictionary<byte, ClientS> clients = new Dictionary<byte, ClientS>();
    public delegate void PacketHandler(byte _fromClient, Packet _packet);
    public static Dictionary<ushort, PacketHandler> packetHandlers;


    public static void Start(byte _maxPlayers)
    {
        MaxPlayers = _maxPlayers;

        Debug.Log("Starting server...");
        InitializeServerData();
    }

    public static void steamConnect(Steamworks.Data.Connection c, Steamworks.Data.ConnectionInfo info)
    {
        for (byte i = 1; i <= MaxPlayers; i++)
        {
            if (!clients[i].slotTaken)
            {
                clients[i].steamConnect(c, info);
                return;
            }
        }
    }
    public delegate void InitAction();
    public static event InitAction OnInit;
    private static void InitializeServerData()
    {
        clients = new Dictionary<byte, ClientS>();
        for (byte i = 1; i <= MaxPlayers; i++)
        {
            clients.Add(i, new ClientS(i));
        }
        packetHandlers = new Dictionary<ushort, PacketHandler>()
        {
            { (ushort)ClientPackets.connectionAccepted, ServerHandle.ConnectionAccepted },
            { (ushort)ClientPackets.inputs, ServerHandle.ManageInputs },
            { (ushort)ClientPackets.pong, ServerHandle.Pong },
            { (ushort)ClientPackets.voiceChat, ServerHandle.VoiceChat },
        };
        if (OnInit != null) OnInit();
        Debug.Log("Initialized packets.");

    }
    private void OnApplicationQuit()
    {
        Stop(); // Disconnect when the game is closed
    }
    public static void Stop()
    {
        Debug.Log("server closed");
        foreach (KeyValuePair<byte, ClientS> c in clients)
        {
            c.Value.Disconnect();
        }
        clients.Clear();
    }
}
