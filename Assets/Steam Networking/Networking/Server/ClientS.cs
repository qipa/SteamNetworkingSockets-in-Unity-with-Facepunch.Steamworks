using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using Steamworks.Data;
using System.Threading;

public class ClientS
{
    public static int dataBufferSize = 4096;

    public byte id;
    public bool slotTaken;
    public bool connectionFinalized;
    public Connection connection;
    public ConnectionInfo info;
    public ulong steamId = 0;
    public string userName = "default";
    public ushort ping = 0;

    public ClientS(byte _clientId)
    {
        id = _clientId;
    }

    public void steamConnect(Connection c, ConnectionInfo i)
    {
        slotTaken = true;
        connection = c;
        info = i;
        Debug.Log("steam connection accepted, id: " + id);
        Thread newThread = new Thread(waitForConfirm);
        newThread.Start();
    }
    void waitForConfirm()
    {
        Thread.Sleep(1500);
        ServerSend.ConnectionAccepted(id);
    }
    public void ConnectionConfirmed(string _userName, ulong _steamID)
    {
        Debug.Log("confirmed new connection");
        steamId = _steamID;
        userName = _userName;
        connectionFinalized = true;
        for (byte i = 1; i <= Server.clients.Count; i++)
        {
            if (Server.clients[i].connectionFinalized)
            {
                if (i != id)
                {
                    //Debug.Log(i);
                    //Debug.Log(Server.clients[i].userName);
                    ServerSend.ConnectionConfirmed(id, i, Server.clients[i].steamId, Server.clients[i].userName);
                }
                ServerSend.ConnectionConfirmed(i, id, _steamID, _userName);
            }
        }    
    }
    public void Disconnect()
    {
        ServerSend.ClientDisconnected(id);

        slotTaken = false;
        connectionFinalized = false;
        Debug.Log("steam player disconnected");
    }
}
