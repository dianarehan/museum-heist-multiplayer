using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime; 

public class Launcher : MonoBehaviourPunCallbacks
{
    private string gameVersion = "1";

    [Header("UI Panels")]
    [Tooltip("The panel holding your InputField, Create, and Join buttons")]
    [SerializeField] private GameObject controlPanel;
    
    [Tooltip("The text object used to show connection status (e.g., 'Connecting...')")]
    [SerializeField] private TMP_Text statusText;

    [Header("UI Elements")]
    [Tooltip("The InputField where players type the room code")]
    [SerializeField] private TMP_InputField roomCodeInput;
    
    [Header("Game Scene")]
    [Tooltip("The *exact* name of your Game Scene to load")]
    [SerializeField] private string gameSceneName = "Game";


    void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    void Start()
    {
        if (controlPanel != null) controlPanel.SetActive(false);
        if (statusText != null) statusText.text = "Connecting to server...";

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
        // If we *are* connected, OnConnectedToMaster() will have already run, 
        // and our UI will be visible, so we don't need to do anything.
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
        if (statusText != null) statusText.text = "Joined room! Loading game...";

        PhotonNetwork.NickName = PlayerData.PlayerName;
        Debug.Log($"Player nickname set to: {PhotonNetwork.NickName}");
        PhotonNetwork.LoadLevel(gameSceneName);
    }
}