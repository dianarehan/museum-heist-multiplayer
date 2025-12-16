using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

/// <summary>
/// Add this component to your scene to enable direct play testing without going through the lobby.
/// It will automatically connect to Photon and set up networking for any players in the scene.
/// 
/// Usage:
/// 1. Add this script to an empty GameObject in your test scene
/// 2. Place your Guard/Thief prefabs directly in the scene (NOT the networked versions)
/// 3. Assign the prefabs to this script's inspector fields
/// 4. Hit Play - the script will handle networking automatically
/// </summary>
public class DebugNetworkBootstrapper : MonoBehaviourPunCallbacks
{
    [Header("Debug Settings")]
    [Tooltip("Enable this script for debug mode. Disable in production.")]
    [SerializeField] private bool enableDebugMode = true;
    
    [Tooltip("Use offline mode (single player, no internet needed) or connect online")]
    [SerializeField] private bool useOfflineMode = true;
    
    [Tooltip("Role to assign to the local player")]
    [SerializeField] private PlayerRole debugRole = PlayerRole.Guard;
    
    [Tooltip("Debug player name")]
    [SerializeField] private string debugPlayerName = "DebugPlayer";
    
    [Header("Scene Players (Optional)")]
    [Tooltip("If you have a Guard already placed in scene, assign it here for local control")]
    [SerializeField] private GameObject sceneGuard;
    
    [Tooltip("If you have Thieves already placed in scene, assign them here")]
    [SerializeField] private GameObject[] sceneThieves;
    
    [Header("Prefab Spawning (Alternative)")]
    [Tooltip("If true, spawn prefabs instead of using scene-placed objects")]
    [SerializeField] private bool spawnPrefabsInstead = false;
    
    [SerializeField] private string guardPrefabName = "NetworkedGuard";
    [SerializeField] private string thiefPrefabName = "NetworkedThief";
    [SerializeField] private Transform spawnPoint;

    public enum PlayerRole { Guard, Thief }
    
    private bool hasInitialized = false;

    void Awake()
    {
        if (!enableDebugMode)
        {
            if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
            {
                // if normal game flow -> disable this bootstrapper
                gameObject.SetActive(false);
                return;
            }
        }
        
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            Debug.Log("[DebugBootstrapper] Already in room, skipping debug setup.");
            gameObject.SetActive(false);
            return;
        }
    }

    void Start()
    {
        if (!enableDebugMode) return;
        if (hasInitialized) return;
        
        PlayerData.PlayerName = debugPlayerName;
        
        if (useOfflineMode)
        {
            StartOfflineMode();
        }
        else
        {
            StartOnlineDebugMode();
        }
    }

    private void StartOfflineMode()
    {
        Debug.Log("[DebugBootstrapper] Starting OFFLINE debug mode...");
        
        PhotonNetwork.OfflineMode = true;
        
        PhotonNetwork.CreateRoom("DebugRoom_Offline");
    }

    private void StartOnlineDebugMode()
    {
        Debug.Log("[DebugBootstrapper] Starting ONLINE debug mode...");
        
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
        else if (PhotonNetwork.IsConnectedAndReady)
        {
            CreateOrJoinDebugRoom();
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("[DebugBootstrapper] Connected to Master Server");
        CreateOrJoinDebugRoom();
    }

    private void CreateOrJoinDebugRoom()
    {
        RoomOptions roomOptions = new RoomOptions
        {
            IsVisible = false,
            IsOpen = true,
            MaxPlayers = 4
        };
        
        PhotonNetwork.JoinOrCreateRoom("DebugRoom_" + System.DateTime.Now.Ticks, roomOptions, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"[DebugBootstrapper] Joined room: {PhotonNetwork.CurrentRoom.Name}");
        InitializeDebugPlayer();
    }

    public override void OnCreatedRoom()
    {
        Debug.Log($"[DebugBootstrapper] Created room: {PhotonNetwork.CurrentRoom.Name}");
    }

    private void InitializeDebugPlayer()
    {
        if (hasInitialized) return;
        hasInitialized = true;

        string roleString = debugRole == PlayerRole.Guard ? "Guard" : "Thief";
        
        Hashtable playerProps = new Hashtable
        {
            { "Role", roleString },
            { "isReady", true }
        };
        
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProps);
        PhotonNetwork.NickName = debugPlayerName;
        
        Debug.Log($"[DebugBootstrapper] Player initialized as {roleString}");

        if (spawnPrefabsInstead)
        {
            SpawnNetworkedPrefab(roleString);
        }
        else
        {
            SetupScenePlacedPlayers(roleString);
        }
    }

    private void SpawnNetworkedPrefab(string role)
    {
        Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        Quaternion spawnRot = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;
        
        string prefabName = role == "Guard" ? guardPrefabName : thiefPrefabName;
        
        GameObject player = PhotonNetwork.Instantiate(prefabName, spawnPos, spawnRot);
        Debug.Log($"[DebugBootstrapper] Spawned {prefabName} at {spawnPos}");
    }

    private void SetupScenePlacedPlayers(string localRole)
    {
        if (localRole == "Guard" && sceneGuard != null)
        {
            SetupLocalPlayer(sceneGuard);
            
            foreach (var thief in sceneThieves)
            {
                if (thief != null)
                {
                    DisableRemotePlayer(thief);
                }
            }
        }
        else if (localRole == "Thief")
        {
            if (sceneThieves != null && sceneThieves.Length > 0 && sceneThieves[0] != null)
            {
                SetupLocalPlayer(sceneThieves[0]);
                
                for (int i = 1; i < sceneThieves.Length; i++)
                {
                    if (sceneThieves[i] != null)
                    {
                        DisableRemotePlayer(sceneThieves[i]);
                    }
                }
            }
            
            if (sceneGuard != null)
            {
                DisableRemotePlayer(sceneGuard);
            }
        }
    }

    private void SetupLocalPlayer(GameObject player)
    {
        PhotonView pv = player.GetComponent<PhotonView>();
        
        if (pv != null)
        {
            if (pv.Owner == null || pv.Owner == PhotonNetwork.LocalPlayer)
            {
                // For scene objects, we need to request ownership or the object must be set to "Takeover"
                if (pv.OwnershipTransfer == OwnershipOption.Takeover || 
                    pv.OwnershipTransfer == OwnershipOption.Request)
                {
                    pv.TransferOwnership(PhotonNetwork.LocalPlayer);
                }
            }
            
            Debug.Log($"[DebugBootstrapper] Set up local control for: {player.name}");
        }
        else
        {
            Debug.LogWarning($"[DebugBootstrapper] {player.name} has no PhotonView - adding one");
            pv = player.AddComponent<PhotonView>();
        }
        
        // Enable all components that check IsMine
        EnableLocalComponents(player);
    }

    private void EnableLocalComponents(GameObject player)
    {
        // Enable camera and audio listener
        Camera cam = player.GetComponentInChildren<Camera>(true);
        if (cam != null) cam.enabled = true;
        
        AudioListener listener = player.GetComponentInChildren<AudioListener>(true);
        if (listener != null) listener.enabled = true;
    }

    private void DisableRemotePlayer(GameObject player)
    {
        // Disable camera and audio listener for remote players
        Camera cam = player.GetComponentInChildren<Camera>(true);
        if (cam != null) cam.enabled = false;
        
        AudioListener listener = player.GetComponentInChildren<AudioListener>(true);
        if (listener != null) listener.enabled = false;
        
        Debug.Log($"[DebugBootstrapper] Disabled remote player: {player.name}");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        if (cause != DisconnectCause.DisconnectByClientLogic)
        {
            Debug.LogWarning($"[DebugBootstrapper] Disconnected: {cause}");
        }
    }

    void OnDestroy()
    {
        // Clean up if we enabled offline mode
        if (useOfflineMode && PhotonNetwork.OfflineMode)
        {
            // Don't disable offline mode here as it might disrupt gameplay
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Test Guard Role")]
    private void TestGuardRole()
    {
        debugRole = PlayerRole.Guard;
    }

    [ContextMenu("Test Thief Role")]
    private void TestThiefRole()
    {
        debugRole = PlayerRole.Thief;
    }
#endif
}
