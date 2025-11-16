using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class GameState : MonoBehaviourPunCallbacks
{
    public static GameState Instance;

    [Header("Game State")]
    public int ThievesAlive;

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

        // Optionally broadcast to all clients
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

    // Called by thief when he dies
    public void ThiefDied()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        ThievesAlive--;
        photonView.RPC("RPC_UpdateThievesAlive", RpcTarget.All, ThievesAlive);

        if (ThievesAlive <= 0)
            GuardWins();
    }

    // Called by thief when collecting loot
    public void ThiefCollectedLoot(int amount)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        LootCollected += amount;

        if (LootCollected >= TotalLootAmount)
            ThievesWin();
    }

    [PunRPC]
    void RPC_UpdateThievesAlive(int newValue)
    {
        ThievesAlive = newValue;
        Debug.Log("Thieves Alive updated: " + ThievesAlive);
    }

    void GuardWins()
    {
        Debug.Log("GUARD WINS!");
        GameOverManager.WinnerMessage = "Guard Wins!";
        PhotonNetwork.LoadLevel("Game Over");
        
    }

    void ThievesWin()
    {
        Debug.Log("THIEVES WIN!");
        // Load end screen, show UI, etc.
        GameOverManager.WinnerMessage = "Thieves Win!";
        PhotonNetwork.LoadLevel("Game Over");
    }
}
