using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NetworkTest : MonoBehaviour
{
    public static NetworkTest instance;
    public GameObject testPrefab;
    public static Dictionary<byte, Character> clients = new Dictionary<byte, Character>();
    public Transform clientList;
    public GameObject characterPrefab;
    float pingTime = 0;
    public bool rayCastHitFake;
    public GameObject leaveGameButton;
    public GameObject searchMenu;
    public Camera lobbyCam;

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
    public void playerConnected(byte id, ulong steamId, string userName)
    {
        lobbyCam.gameObject.SetActive(false);
        Debug.Log("spawning player");
        Character character = Instantiate(characterPrefab, new Vector3(0, 10, 0), Quaternion.identity).GetComponent<Character>();
        clients.Add(id, character);
        character.id = id;
        character.steamId = steamId;
        character.username = userName;
        if (id != steamNetworkClient.myID)
        {
            Destroy(character.transform.GetChild(0).GetChild(0).gameObject);
        }
        else
        {
            leaveGameButton.SetActive(true);
            //textureBreaker.sendTexture(steamNetworkClient.myID, 0, textureBreaker.textureToByteArrayList(character.textureToSend), character.textureToSend.width, character.textureToSend.height, (int)character.textureToSend.format);
        }
        NetworkTestPrefab client = Instantiate(testPrefab).GetComponent<NetworkTestPrefab>();
        client.id = id;
        client.steamId = steamId;
        client.username = userName;
        client.transform.SetParent(clientList);
        client.transform.localScale = Vector3.one;
        client.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = id.ToString();
        client.transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>().text = userName;
        client.transform.GetChild(2).GetChild(0).GetComponent<TextMeshProUGUI>().text = steamId.ToString();
        character.ui = client;
    }
    public void playerDisconnected(byte id)
    {
        if (clients.ContainsKey(id))
        {
            Destroy(clients[id].ui.gameObject);
            Destroy(clients[id].gameObject);
            clients.Remove(id);
        }
    }

    public void test(byte id, bool[] inputs, ushort ping)
    {
        if (!clients.ContainsKey(id))
        {
            return;
        }
        clients[id].ui.transform.GetChild(3).GetComponent<UnityEngine.UI.Image>().color = Color.white;
        clients[id].ui.transform.GetChild(4).GetComponent<UnityEngine.UI.Image>().color = Color.white;
        clients[id].ui.transform.GetChild(5).GetComponent<UnityEngine.UI.Image>().color = Color.white;
        clients[id].ui.transform.GetChild(6).GetComponent<UnityEngine.UI.Image>().color = Color.white;
        if (inputs[0])
        {
            clients[id].ui.transform.GetChild(3).GetComponent<UnityEngine.UI.Image>().color = Color.green;
        }
        if (inputs[1])
        {
            clients[id].ui.transform.GetChild(4).GetComponent<UnityEngine.UI.Image>().color = Color.green;
        }
        if (inputs[2])
        {
            clients[id].ui.transform.GetChild(5).GetComponent<UnityEngine.UI.Image>().color = Color.green;
        }
        if (inputs[3])
        {
            clients[id].ui.transform.GetChild(6).GetComponent<UnityEngine.UI.Image>().color = Color.green;
        }
        clients[id].ui.transform.GetChild(7).GetChild(0).GetComponent<TextMeshProUGUI>().text = ping.ToString();
    }

    public void leaveGame()
    {
        if (SteamManager.server != null)
        {
            closeServer();
        }
        if (SteamManager.connection != null)
        {
            leaveServer();
        }
        clients.Clear();
        SteamManager.instance.leaveLobby();
        leaveGameButton.SetActive(false);
        searchMenu.SetActive(true);
    }
    public void hostMigrate()
    {
        if (SteamManager.server != null)
        {
            closeServer();
        }
        if (SteamManager.connection != null)
        {
            leaveServer();
        }
        clients.Clear();
        leaveGameButton.SetActive(false);
        searchMenu.SetActive(true);
    }
    void leaveServer()
    {
        foreach(KeyValuePair<byte, Character> c in clients)
        {
            Destroy(c.Value.ui.gameObject);
            Destroy(c.Value.gameObject);
        }
        clients.Clear();
        if (SteamManager.connection != null)
        {
            SteamManager.connection.Close();
        }
        lobbyCam.gameObject.SetActive(true);
    }
    void closeServer()
    {
        if (SteamManager.server != null)
        {
            SteamManager.server.Close();
            SteamManager.server = null;
            SteamManager.instance.lobby.SetData("started", "0");
        }
        leaveServer();
    }
    private void Update()
    {
        //leave server if it doesnt exist
        if (SteamManager.server == null && (SteamManager.connection != null && !SteamManager.lobbyServerAvailable))
        {
            leaveServer();
        }
    }
    private void FixedUpdate()
    {
        //return if you are not the host of the server
        if (SteamManager.server == null)
        {
            return;
        }
        pingTime += Time.fixedDeltaTime;
        if (pingTime > 3)
        {
            pingTime = 0;
            for (byte i = 1; i <= Server.clients.Count; i++)
            {
                if (clients.ContainsKey(i))
                {
                    ServerSend.ping(i);
                }
            }
        }
    }
}
