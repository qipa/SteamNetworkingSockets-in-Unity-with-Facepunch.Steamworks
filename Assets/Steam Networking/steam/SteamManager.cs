using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using Steamworks.Data;
using System.Threading.Tasks;
using UnityEngine.UI;
using System;
using Image = Steamworks.Data.Image;
using System.Threading;
using TMPro;

public class SteamManager : MonoBehaviour
{
    public static SteamManager instance;
    public bool _steamServer;

    public SteamId steamID;

    public static Dictionary<SteamId, int> players = new Dictionary<SteamId, int>();

    public static int updateDelay = 1;

    public Lobby lobby;
    bool lobbyLeft;
    public ulong hostId;

    public static steamServer server;
    public static steamNetworkClient connection;

    public bool p2pClient;

    public static bool lobbyServerAvailable;

    bool createLobby;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            if (_steamServer)
            {
                try
                {
                    SteamClient.Init(480);
                    SteamNetworkingUtils.InitRelayNetworkAccess();
                    steamID = SteamClient.SteamId;
                }
                catch (System.Exception e)
                {
                    Debug.Log("something went wrong loading steam");
                    _steamServer = false;
                }
            }
        }
        else if (instance != this)
        {
            Destroy(this);
        }
        DontDestroyOnLoad(gameObject);
    }
    private void OnApplicationQuit()
    {
        if (SteamClient.IsValid)
        {
            hardReset();
            SteamClient.Shutdown();
        }
    }
    public static void hardReset()
    {
        if (NetworkTest.clients != null)
        {
            NetworkTest.clients.Clear();
        }
        if (connection != null)
        {
            connection.Close();
        }
        if (SteamManager.server != null)
        {
            server.Close();
        }
    }
    private async void Start()
    {
        if (!SteamClient.IsValid)
        {
            return;
        }
        SteamFriends.ListenForFriendsMessages = true;

        SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequestedAsync;
        SteamMatchmaking.OnLobbyCreated += onLobbyCreatedAsync;
        SteamMatchmaking.OnLobbyMemberJoined += onLobbyMemberJoined;
        SteamMatchmaking.OnLobbyEntered += onJoinedLobby;
        SteamMatchmaking.OnLobbyMemberLeave += playerLeft;
        SteamMatchmaking.OnChatMessage += onChaMessage;
        SteamMatchmaking.OnLobbyDataChanged += OnLobbyDataChanged;
        SteamMatchmaking.OnLobbyMemberDataChanged += onLobbyMemberDataChanged;
        SteamMatchmaking.OnLobbyGameCreated += OnLobbyGameCreated;
    }
    private async void Update()
    {
        if (!SteamClient.IsValid)
        {
            return;
        }
        SteamClient.RunCallbacks();

        //Debug.Log(lobby.MemberCount);
        //Debug.Log(lobby.IsOwnedBy(steamID));
        if (createLobby && (lobby.MemberCount == 0 || lobbyLeft))
        {
            createLobby = false;
            lobbyLeft = false;
            await createLobbyAsync();
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            //packetLoss();
            networkStatus();
        }
    }
    void networkStatus()
    {
        Debug.Log(connection.Connection.DetailedStatus());
    }
    public bool isMyLobby()
    {
        if (lobby.IsOwnedBy(steamID))
        {
            return true;
        }
        return false;
    }
    #region matchmaking
    public TMP_InputField gameModeField;
    public TMP_InputField maxPingField;
    List<Lobby> compatibleLobbies;
    int lobbyType = 0;
    int _gameMode = 0;
    public async void searchForMatch()
    {
        lobbyType = 0;
        NetworkTest.instance.searchMenu.SetActive(false);
        Debug.Log("search started");
        int searchQuery = 1;
        int.TryParse(gameModeField.text, out _gameMode);
        int.TryParse(maxPingField.text, out searchQuery);
        await listLobbys(_gameMode, searchQuery);

        if (compatibleLobbies.Count == 0)   //no compatible lobbies found, create one.
        {
            await createLobbyAsync();
        }
        else
        {
            await joinlobby(compatibleLobbies[0].Id); //join the most compatible lobby
            //await Task.Delay(3000);
            //ConnectToServer();
            waitingToJoin = true;
        }
    }
    bool waitingToJoin = false;
    public async void createPrivateLobby()
    {
        lobbyType = 1;
        NetworkTest.instance.searchMenu.SetActive(false);
        
        int.TryParse(gameModeField.text, out _gameMode);
        await createLobbyAsync();
    }
    public async void createFriendsOnlyLobby()
    {
        lobbyType = 2;
        NetworkTest.instance.searchMenu.SetActive(false);
        int.TryParse(gameModeField.text, out _gameMode);
        await createLobbyAsync();
    }
    public async void listPlayersInLobby()
    {
        foreach(Friend f in lobby.Members)
        {
            Debug.Log(f.Name);
        }
    }
    async Task listLobbys(int matchType, int searchQuery)
    {
        int minOpenSlots = 1; //you can configure this if for example you wanted to join a lobby with a friend
        compatibleLobbies = new List<Lobby>();
        Lobby[] lobbies;
        //Debug.Log(searchQuery);
        if (searchQuery == 0)       // FINDS LOBBIES ONLY IN THE PLAYERS REGION
        {
            lobbies = await SteamMatchmaking.LobbyList.WithSlotsAvailable(minOpenSlots).FilterDistanceClose().WithKeyValue("gameMode", matchType.ToString()).RequestAsync();
        }
        else if (searchQuery == 2)  //FILTERS LOBBIES ABOUT HALFWAY ACROSS THE GLOBE
        {
            lobbies = await SteamMatchmaking.LobbyList.WithSlotsAvailable(minOpenSlots).FilterDistanceFar().WithKeyValue("gameMode", matchType.ToString()).RequestAsync();
        }
        else if (searchQuery == 3)  // FINDS WORLD WIDE LOBBIES
        {
            lobbies = await SteamMatchmaking.LobbyList.WithSlotsAvailable(minOpenSlots).FilterDistanceWorldwide().WithKeyValue("gameMode", matchType.ToString()).RequestAsync();
        }
        else                        // FINDS LOBBIES IN PLAYERS REGION AND NEIGHBORING REGIONS
        {
            lobbies = await SteamMatchmaking.LobbyList.WithSlotsAvailable(minOpenSlots).WithKeyValue("gameMode", matchType.ToString()).RequestAsync();
        }

        if (lobbies == null)
        {
            return;
        }
        //LOOP THROUGH ALL FOUND LOBBIES AND ADD THEM TO THE COMPATIBLE LOBBY LIST. 
        // this isnt really neccesary anymore since we are now filtering with the search function, but i will leave it here in case you want to do custom filter types
        for (int i = 0; i < lobbies.Length; i++)
        {
            //Debug.Log(lobbies[i].GetData("gameMode"));
            if (lobbies[i].GetData("gameMode") == matchType.ToString())
            {
                compatibleLobbies.Add(lobbies[i]);
            }
        }
    }

    #endregion
    #region lobby
    public void invitePlayersToLobby()
    {
        SteamFriends.OpenGameInviteOverlay(lobby.Id);
    }
    public async void OnGameLobbyJoinRequestedAsync(Lobby L, SteamId sID)
    {
        await joinlobby(L.Id);
    }
    static async Task joinlobby(SteamId lobbyID)
    {
        Lobby lobby = (Lobby)await SteamMatchmaking.JoinLobbyAsync(lobbyID);
        //Debug.Log(lobby.Id);
    }
    async void onJoinedLobby(Lobby l)
    {
        lobby = l;
        hostId = lobby.Owner.Id;
        if (waitingToJoin)
        {
            waitingToJoin = false;
            ConnectToServer();
        }
    }
    async void onLobbyMemberJoined(Lobby l, Friend f)
    {
        Debug.Log(f.Name + " Joined the lobby");
    }

    static async Task createLobbyAsync()
    {
        //Debug.Log("creating lobbu");
        await SteamMatchmaking.CreateLobbyAsync();
    }
    async void onLobbyCreatedAsync(Result a, Lobby _lobby)
    {
        lobby = _lobby;
        //Debug.Log(lobby.Owner.Id);
        Debug.Log("lobby " + lobby.Id + " was created");

        lobby.SetData("gameMode", _gameMode.ToString());
        lobby.SetJoinable(true);
        lobby.MaxMembers = 16;

        if (lobbyType == 0)         //public
        {
            lobby.SetPublic();
        }
        else if (lobbyType == 1)    //private
        {
            lobby.SetPrivate(); //INVITE ONLY
        }
        else if (lobbyType == 2)    //friends only
        {
            lobby.SetFriendsOnly(); //FRIENDS ONLY
        }
        
        await StartServerAsync();

        //await Task.Delay(1000);   //IF YOU RUN INTO ISSUES TRY UNCOMMENTING THIS
        
        ConnectToServer();
    }
    async void playerLeft(Lobby l, Friend f)
    {
        Debug.Log(f.Name + " Left the lobby");

        checkHostMigrate();
    }
    void checkHostMigrate()
    {
        //Debug.Log(hostId);
        //Debug.Log(lobby.Owner.Id);
        if (hostId != lobby.Owner.Id)
        {
            hostMigrate();
        }
    }
    async void hostMigrate()
    {
        NetworkTest.instance.hostMigrate();
        Debug.Log("host migrating");


        if (lobby.Owner.Id == steamID)
        {
            lobby.SetData("started", "0");
            Debug.Log("im the new host");
            while (connection != null)
            {
                await Task.Delay(25);
            }
            Debug.Log("done waiting");
            await StartServerAsync();
            await Task.Delay(100);
            ConnectToServer();
            lobby.SetJoinable(true);
            lobby.SetPublic();
        }
        else
        {
            waitingToJoin = true;
        }
    }
    public void acceptNewConnections()
    {
        if (lobby.Owner.Id == steamID)
        {
            lobby.SetData("started", "1");
        }
    }


    public void leaveLobby()
    {
        Debug.Log("leaving lobby");
        lobbyServerAvailable = false;
        lobby.Leave();
        lobbyLeft = true;
    }
    public void OnLobbyDataChanged(Lobby l)
    {
        if (_steamServer)
        {
            string s = lobby.GetData("started");
            int n = -1;
            int.TryParse(s, out n);
            if (n != -1)
            {
                if (n == 0)
                {
                    Debug.Log("server is not started");
                    lobbyServerAvailable = false;
                }
                else if (n == 1)
                {
                    Debug.Log("server is started");
                    lobbyServerAvailable = true;
                    if (waitingToJoin)
                    {
                        ConnectToServer();
                        waitingToJoin = false;
                    }
                }
            }
        }
    }
    void onLobbyMemberDataChanged(Lobby l, Friend f)
    {
        Debug.Log("clock");
    }

    public void onChaMessage(Lobby l, Friend f, string s)
    {

    }
    void OnLobbyGameCreated(Lobby l, uint n, ushort s, SteamId serverID)
    {
        Debug.Log(lobby.Id);
        Debug.Log(l.Id);
        Debug.Log(n);
        Debug.Log(s);
        Debug.Log(serverID);
    }
    #endregion

    public async Task StartServerAsync()
    {
        if (SteamClient.IsValid)
        {
            SteamNetworkManager.instance.startServer();

            //await Task.Delay(2000);

            Debug.Log($"----- Creating Socket Relay Socket..");
            steamServer socket = SteamNetworkingSockets.CreateRelaySocket<steamServer>(0);
            hostId = lobby.Owner.Id;

            server = socket;

            SteamNetworkingUtils.Timeout = 2000;

            lobbyServerAvailable = true;
        }
    }
    public void ConnectToServer()
    {
        p2pClient = true;
        Debug.Log($"----- Connecting To Socket via SteamId ({lobby.Owner.Id})");
        steamNetworkClient _connection = SteamNetworkingSockets.ConnectRelay<steamNetworkClient>(lobby.Owner.Id, 0);
        _connection.initialize();
        connection = _connection;
    }
}
