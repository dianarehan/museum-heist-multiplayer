using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// HUD that displays objective items in a grid.
/// Attach to a Canvas with a Grid Layout Group.
/// </summary>
public class ObjectiveHUD : MonoBehaviourPun
{
    [Header("UI References")]
    [SerializeField] private Transform slotContainer; // Parent with GridLayoutGroup
    [SerializeField] private ObjectiveSlot slotPrefab; // Prefab for each slot
    
    [Header("Display Settings")]
    [SerializeField] private bool showOnlyForThief = true;
    
    private Dictionary<StealableItem, ObjectiveSlot> itemSlots = new Dictionary<StealableItem, ObjectiveSlot>();
    
    private void Start()
    {
        // Only show for thief players
        if (showOnlyForThief)
        {
            bool isThief = false;
            foreach (var pv in FindObjectsOfType<PhotonView>())
            {
                if (pv.IsMine && pv.CompareTag("Thief"))
                {
                    isThief = true;
                    break;
                }
            }
            
            if (!isThief && PhotonNetwork.IsConnected)
            {
                gameObject.SetActive(false);
                return;
            }
        }
        
        // Wait a frame for ObjectiveManager to initialize
        Invoke(nameof(BuildHUD), 0.1f);
    }
    
    private void BuildHUD()
    {
        if (ObjectiveManager.Instance == null)
        {
            Debug.LogWarning("ObjectiveManager not found!");
            return;
        }
        
        // Subscribe to events
        ObjectiveManager.Instance.OnItemAcquired += OnObjectiveAcquired;
        
        // Create slots for each objective
        foreach (var item in ObjectiveManager.Instance.ObjectiveItems)
        {
            if (item == null) continue;
            
            CreateSlot(item);
        }
    }
    
    private void CreateSlot(StealableItem item)
    {
        if (slotPrefab == null || slotContainer == null) return;
        
        ObjectiveSlot slot = Instantiate(slotPrefab, slotContainer);
        
        // Get item data
        ObjectiveItemData data = item.GetComponent<ObjectiveItemData>();
        Sprite icon = data != null ? data.Icon : null;
        string itemName = data != null ? data.ItemName : item.gameObject.name;
        
        slot.Setup(item, icon, itemName);
        itemSlots[item] = slot;
        
        // Check if already acquired
        if (ObjectiveManager.Instance.IsAcquired(item))
        {
            slot.MarkAcquired();
        }
    }
    
    private void OnObjectiveAcquired(StealableItem item)
    {
        if (itemSlots.TryGetValue(item, out ObjectiveSlot slot))
        {
            slot.MarkAcquired();
        }
    }
    
    private void OnDestroy()
    {
        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.OnItemAcquired -= OnObjectiveAcquired;
        }
    }
}
