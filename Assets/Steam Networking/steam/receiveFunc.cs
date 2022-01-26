using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class receiveFunc : MonoBehaviour
{
    private void FixedUpdate()
    {
        if (SteamManager.server != null)
        {
            SteamManager.server.Receive();
        }
    }
    private void Update()
    {
        if (SteamManager.connection != null)
        {
            SteamManager.connection.Receive();
        }
    }
}
