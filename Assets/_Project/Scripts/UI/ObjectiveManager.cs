using Photon.Pun;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages objective items for thieves. Only items in this list can be stolen.
/// Place this on a persistent Manager object in the scene.
/// </summary>
public class ObjectiveManager : MonoBehaviourPun
{
    public static ObjectiveManager Instance { get; private set; }
    
    [Header("Objectives")]
    [SerializeField] private List<StealableItem> objectiveItems = new List<StealableItem>();
    
    private Dictionary<StealableItem, bool> acquiredState = new Dictionary<StealableItem, bool>();
    
    public List<StealableItem> ObjectiveItems => objectiveItems;
    
    public event System.Action<StealableItem> OnItemAcquired;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        // Initialize state dictionary
        foreach (var item in objectiveItems)
        {
            if (item != null)
            {
                acquiredState[item] = false;
            }
        }
    }
    
    /// <summary>
    /// Check if an item is a valid objective that can be stolen
    /// </summary>
    public bool IsObjective(StealableItem item)
    {
        return objectiveItems.Contains(item);
    }
    
    /// <summary>
    /// Check if an objective has been acquired
    /// </summary>
    public bool IsAcquired(StealableItem item)
    {
        return acquiredState.ContainsKey(item) && acquiredState[item];
    }
    
    /// <summary>
    /// Mark an objective as acquired - called when item is stolen
    /// </summary>
    public void MarkAcquired(StealableItem item)
    {
        if (!IsObjective(item)) return;
        if (IsAcquired(item)) return;
        
        // Sync across network
        int itemIndex = objectiveItems.IndexOf(item);
        photonView.RPC(nameof(RPC_MarkAcquired), RpcTarget.AllBuffered, itemIndex);
    }
    
    [PunRPC]
    private void RPC_MarkAcquired(int itemIndex)
    {
        if (itemIndex < 0 || itemIndex >= objectiveItems.Count) return;
        
        var item = objectiveItems[itemIndex];
        if (item != null)
        {
            acquiredState[item] = true;
            OnItemAcquired?.Invoke(item);
        }
    }
    
    /// <summary>
    /// Get count of acquired objectives
    /// </summary>
    public int GetAcquiredCount()
    {
        int count = 0;
        foreach (var kvp in acquiredState)
        {
            if (kvp.Value) count++;
        }
        return count;
    }
    
    /// <summary>
    /// Get total number of objectives
    /// </summary>
    public int GetTotalCount()
    {
        return objectiveItems.Count;
    }
}
