using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class GameState : MonoBehaviourPunCallbacks
{
    public static GameState Instance;

    [Header("Game State")]
    public int ThievesAlive;
    public int ThievesKnockedOut = 0;

    public int TotalLootAmount = 3200;
    public int LootCollected = 0;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
            InitializeGameState();
    }

    void InitializeGameState()
    {
        // Count thieves in the room
        ThievesAlive = CountThievesInRoom();

        // Broadcast to all clients
        photonView.RPC("RPC_UpdateThievesAlive", RpcTarget.All, ThievesAlive);
    }

    int CountThievesInRoom()
    {
        int count = 0;
        foreach (var p in PhotonNetwork.PlayerList)
        {
            if (p.CustomProperties.ContainsKey("Role") &&
                (string)p.CustomProperties["Role"] == "Thief")
            {
                count++;
            }
        }
        return count;
    }

    // Called when a thief is caught/knocked out
    public void ThiefDied()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        ThievesAlive--;
        ThievesKnockedOut++;
        
        photonView.RPC("RPC_UpdateThievesAlive", RpcTarget.All, ThievesAlive);
        photonView.RPC("RPC_UpdateThievesKnockedOut", RpcTarget.All, ThievesKnockedOut);

        Debug.Log($"Thief knocked out! Remaining: {ThievesAlive}");

        if (ThievesAlive <= 0)
            GuardWins();
    }

    // Called by thief when collecting loot
    public void ThiefCollectedLoot(int amount)
    {
        // if (!PhotonNetwork.IsMasterClient) return;

        LootCollected += amount;
        
        photonView.RPC("RPC_UpdateLootCollected", RpcTarget.All, LootCollected);

        if (LootCollected >= TotalLootAmount)
            ThievesWin();
    }

    // NEW: RPC method for non-master clients to request thief caught
    [PunRPC]
    void RPC_ThiefCaughtRequest()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            ThiefDied();
        }
    }

    [PunRPC]
    void RPC_UpdateThievesAlive(int newValue)
    {
        ThievesAlive = newValue;
        Debug.Log("Thieves Alive updated: " + ThievesAlive);
    }

    [PunRPC]
    void RPC_UpdateThievesKnockedOut(int newValue)
    {
        ThievesKnockedOut = newValue;
        Debug.Log("Thieves Knocked Out: " + ThievesKnockedOut);
    }

    [PunRPC]
    void RPC_UpdateLootCollected(int newValue)
    {
        LootCollected = newValue;
        Debug.Log("Loot Collected: " + LootCollected + "/" + TotalLootAmount);
    }

    void GuardWins()
    {
        photonView.RPC("RPC_GuardWins", RpcTarget.All);
    }

    void ThievesWin()
    {
        photonView.RPC("RPC_ThievesWin", RpcTarget.All);
    }

    [PunRPC]
    void RPC_GuardWins()
    {
        Debug.Log("GUARD WINS! All thieves have been caught!");
        PhotonNetwork.LoadLevel("Guard Win");
        // Load end screen, lock input, show UI, etc.
    }

    [PunRPC]
    void RPC_ThievesWin()
    {
        Debug.Log("THIEVES WIN! They collected enough loot!");
        PhotonNetwork.LoadLevel("Game Over");
        // Load end screen, show UI, etc.
    }
}