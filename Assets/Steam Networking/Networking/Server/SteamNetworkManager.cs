using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SteamNetworkManager : MonoBehaviour
{
    public static SteamNetworkManager instance;
    public byte maxPlayers;
    public int tickRate;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(this);
        }
        DontDestroyOnLoad(instance.gameObject);
    }
    private void Start()
    {
        if (SystemInfo.graphicsDeviceName == null)
        {
            if (!Application.isEditor)
            {
                Debug.Log("THIS IS A UNITY SERVER BUILD");
            }
            QualitySettings.vSyncCount = 0;
            Time.fixedDeltaTime = (float)(1f / (float)Constants.TICKS_PER_SEC);
            Application.targetFrameRate = Constants.TICKS_PER_SEC;
        }
    }
    private void Update()
    {
        
    }
    public void startServer()
    {
        steamNetworkClient.myID = 1;
        Constants.TICKS_PER_SEC = tickRate;


        Constants.MS_PER_TICK = 1000f / Constants.TICKS_PER_SEC;
        Time.fixedDeltaTime = (float)(1f / (float)Constants.TICKS_PER_SEC);
        Server.Start(maxPlayers);
    }
}
