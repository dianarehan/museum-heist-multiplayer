using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class Launcher : MonoBehaviourPunCallbacks
{
    private string gameVersion = "1";

    [Header("UI Panels")]
    [Tooltip("The panel holding your InputField, Create, and Join buttons")]
    [SerializeField] private GameObject controlPanel;

    [Tooltip("The panel shown after joining a room, with the player list and ready button")]
    [SerializeField] private GameObject lobbyPanel;
    [Tooltip("The text object used to show connection status (e.g., 'Connecting...')")]
    [SerializeField] private TMP_Text statusText;

    [Tooltip("The Text object that lists all players in the room")]
    [SerializeField] private TMP_Text playerListText;
    [Header("UI Elements")]
    [Tooltip("The InputField where players type the room code")]
    [SerializeField] private TMP_InputField roomCodeInput;

    [Header("Game Scene")]
    [Tooltip("The *exact* name of your Game Scene to load")]
    [SerializeField] private string gameSceneName = "Game";
    
    [Tooltip("The Button players click to ready up")]
    [SerializeField] private Button readyButton;
    private const string READY_PROPERTY_KEY = "isReady";
    private const string ROLE_PROPERTY_KEY = "Role";
    void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    void Start()
    {
        if (controlPanel != null) controlPanel.SetActive(false);
        if (statusText != null) statusText.text = "Connecting to server...";
        if (lobbyPanel != null) lobbyPanel.SetActive(false);
        Connect();
    }

    public void Connect()
    {
        // Check if we are already connected
        if (!PhotonNetwork.IsConnected)
        {
            // Not connected, so connect using settings
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.GameVersion = gameVersion;
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master Server!");
        
        if (controlPanel != null) controlPanel.SetActive(true);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarningFormat("Disconnected from server with reason: {0}", cause);

        if (controlPanel != null) controlPanel.SetActive(false);
        if (lobbyPanel != null) lobbyPanel.SetActive(false);
        if (statusText != null) statusText.text = "Status: Disconnected. Please restart.";
    }

    public void CreateRoom()
    {
        string roomCode = roomCodeInput.text;
        if (string.IsNullOrEmpty(roomCode))
        {
            if (statusText != null) statusText.text = "Please enter a room name.";
            return;
        }

        if (statusText != null) statusText.text = $"Creating room '{roomCode}'...";
        
        RoomOptions roomOps = new RoomOptions() 
        { 
            IsVisible = true, 
            IsOpen = true, 
            MaxPlayers = 3
        };
        
        PhotonNetwork.CreateRoom(roomCode, roomOps);
    }

    public void JoinRoom()
    {
        string roomCode = roomCodeInput.text;
        if (string.IsNullOrEmpty(roomCode))
        {
            if (statusText != null) statusText.text = "Please enter a room name.";
            return;
        }

        if (statusText != null) statusText.text = $"Joining room '{roomCode}'...";
        
        PhotonNetwork.JoinRoom(roomCode);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"Create Room Failed: {message} (Code: {returnCode})");
        if (statusText != null) statusText.text = $"Error: Room '{roomCodeInput.text}' already exists.";
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"Join Room Failed: {message} (Code: {returnCode})");
        if (statusText != null) statusText.text = $"Error: Room '{roomCodeInput.text}' doesn't exist or is full.";
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"Successfully joined room: {PhotonNetwork.CurrentRoom.Name}");
        
        if (controlPanel != null) controlPanel.SetActive(false);
        if (lobbyPanel != null) lobbyPanel.SetActive(true);
        if (statusText != null) statusText.text = $"Joined Room: {PhotonNetwork.CurrentRoom.Name}";

        PhotonNetwork.NickName = PlayerData.PlayerName;

        string playerRole;
        if (PhotonNetwork.IsMasterClient)
        {
            playerRole = "Guard";
        }
        else
        {
            playerRole = "Thief";
        }

        Hashtable initialProps = new Hashtable
        {
            { READY_PROPERTY_KEY, false },
            { ROLE_PROPERTY_KEY, playerRole }
        };        

        PhotonNetwork.LocalPlayer.SetCustomProperties(initialProps);
        UpdatePlayerListUI();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdatePlayerListUI();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdatePlayerListUI();
        CheckIfAllReady(); 
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        UpdatePlayerListUI();
        CheckIfAllReady();
    }

    public void OnClick_Ready()
    {
        bool isReady = (bool)PhotonNetwork.LocalPlayer.CustomProperties[READY_PROPERTY_KEY];

        Hashtable newProps = new Hashtable() { { READY_PROPERTY_KEY, !isReady } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(newProps);

        if (readyButton != null)
        {
            readyButton.GetComponentInChildren<TMP_Text>().text = !isReady ? "Ready! (Waiting)" : "Ready?";
        }
    }

    private void UpdatePlayerListUI()
    {
        if (playerListText == null) return;

        string playerList = "Players:\n";
        foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
        {
            object isReady;
            bool readyState = false;
            if (player.CustomProperties.TryGetValue(READY_PROPERTY_KEY, out isReady))
            {
                readyState = (bool)isReady;
            }

            object role;
            string roleName = "Joining...";
            if (player.CustomProperties.TryGetValue(ROLE_PROPERTY_KEY, out role))
            {
                roleName = (string)role;
            }

            playerList += $"{player.NickName} - [<color=yellow>{roleName}</color>] - {(readyState ? "<color=green>Ready</color>" : "<color=red>Waiting</color>")}\n";
        }

        playerListText.text = playerList;
    }

    private void CheckIfAllReady()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            Debug.Log($"Checking player {player.NickName} readiness.");
            object isReady;
            if (!player.CustomProperties.TryGetValue(READY_PROPERTY_KEY, out isReady) || (bool)isReady == false)
            {
                return; 
            }
        }

        Debug.Log("All players are ready! Loading game scene...");
        
        // Optional: Close the room so no one else can join
        // PhotonNetwork.CurrentRoom.IsOpen = false;
        
        PhotonNetwork.LoadLevel(gameSceneName);
    }
}