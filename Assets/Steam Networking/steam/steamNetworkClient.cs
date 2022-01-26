using Steamworks;
using Steamworks.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class steamNetworkClient : ConnectionManager
{
    public delegate void PacketHandler(Packet _packet);
    public static Dictionary<ushort, PacketHandler> packetHandlers;
    public static byte myID;

    public override void OnConnectionChanged(ConnectionInfo data)
    {
        Debug.Log($"[Connection][{Connection}] [{data.State}]");

        base.OnConnectionChanged(data);
    }

    public override void OnConnecting(ConnectionInfo data)
    {
        Debug.Log($" - OnConnecting");
        base.OnConnecting(data);
    }

    /// <summary>
    /// Client is connected. They move from connecting to Connections
    /// </summary>
    public override void OnConnected(ConnectionInfo data)
    {
        Debug.Log($" - OnConnected");
        base.OnConnected(data);
    }

    /// <summary>
    /// The connection has been closed remotely or disconnected locally. Check data.State for details.
    /// </summary>
    public override void OnDisconnected(ConnectionInfo data)
    {
        Debug.Log($" - OnDisconnected");
        SteamManager.connection = null;
        if (NetworkTest.instance != null)
        {
            NetworkTest.instance.hostMigrate();
        }
        base.OnDisconnected(data);
    }

    public override unsafe void OnMessage(IntPtr _data, int size, long messageNum, long recvTime, int channel)
    {
        //Debug.Log("client: data received");
        byte[] data = new byte[size];
        //Marshal.Copy(_data, data, 0, size);
        byte* b = (byte*)_data;
        for (int i = 0; i < size; ++i)
        {
            data[i] = *b;
            b++;
        }
        Packet _packet = new Packet(data);
        ushort length = _packet.ReadUShort();
        ushort handleID = _packet.ReadUShort();
        if (Connected)
        {
            packetHandlers[handleID](_packet);
        }
    }
    public void initialize()
    {
        InitializeClientData();
    }
    public delegate void InitAction();
    public static event InitAction OnInit;
    private void InitializeClientData()
    {
        packetHandlers = new Dictionary<ushort, PacketHandler>()
        {
            { (ushort)ServerPackets.acceptConnection, ClientHandle.ConnectionAccepted },
            { (ushort)ServerPackets.connectionConfirmed, ClientHandle.ConnectionConfirmed },
            { (ushort)ServerPackets.clientDisconnected, ClientHandle.ClientDisconnected },
            { (ushort)ServerPackets.returnInputs, ClientHandle.displayInputs },
            { (ushort)ServerPackets.ping, ClientHandle.Ping },
            { (ushort)ServerPackets.playerPos, ClientHandle.MovePlayer },
            { (ushort)ServerPackets.playerRot, ClientHandle.RotatePlayer },
            { (ushort)ServerPackets.voiceChat, ClientHandle.VoiceChat },
        };
        if (OnInit != null) OnInit();
        Debug.Log("Initialized packets.");
    }
}
