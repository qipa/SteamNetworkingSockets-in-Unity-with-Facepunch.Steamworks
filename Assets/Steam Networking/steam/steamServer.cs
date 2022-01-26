using Steamworks;
using Steamworks.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class steamServer : SocketManager
{
    public bool HasFinished = false;
    //static Thread t;

    public override void OnConnectionChanged(Connection connection, ConnectionInfo data)
    {
        Debug.Log($"[Socket{Socket}][connection:{connection}][data.Identity:{data.Identity}] [data.State:{data.State}]");

        base.OnConnectionChanged(connection, data);
    }

    public override void OnConnecting(Connection connection, ConnectionInfo data)
    {
        Debug.Log($" - OnConnecting");
        base.OnConnecting(connection, data);
    }

    /// <summary>
    /// Client is connected. They move from connecting to Connections
    /// </summary>
    public override void OnConnected(Connection connection, ConnectionInfo data)
    {
        Debug.Log("connected to steam server");
        base.OnConnected(connection, data);
        Server.steamConnect(connection, data);
        SteamManager.instance.acceptNewConnections();
    }

    /// <summary>
    /// The connection has been closed remotely or disconnected locally. Check data.State for details.
    /// </summary>
    public override void OnDisconnected(Connection connection, ConnectionInfo data)
    {
        Debug.Log($" - OnDisconnected");
        foreach(KeyValuePair<byte, ClientS> c in Server.clients)
        {
            if (c.Value.connection == connection)
            {
                c.Value.Disconnect();
            }
        }
        
        base.OnDisconnected(connection, data);
        connection.Close();
    }
    public override unsafe void OnMessage(Connection connection, NetIdentity identity, IntPtr _data, int size, long messageNum, long recvTime, int channel)
    {
        //Debug.Log(recvTime);
        //Debug.Log("server: data received");
        byte[] data = new byte[size];
        byte* b = (byte*)_data;
        for (int i = 0; i < size; ++i)
        {
            data[i] = *b;
            b++;
        }
        //Debug.Log(BitConverter.ToString(data));
        Packet _packet = new Packet(data);
        byte clientId = _packet.ReadByte();
        ushort length = _packet.ReadUShort();
        ushort handleID = _packet.ReadUShort();

        Server.packetHandlers[handleID](clientId, _packet);
    }
}
