using Photon.Pun;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using _Project.Scripts.Interaction;

/// <summary>
/// Attach to a pickupable item in the scene (like a bat on the floor).
/// When thief presses E while looking at it, it disappears and equips the player.
/// </summary>
public class PickupableItem : MonoBehaviourPun, IInteractableEnhanced
{
    [Header("Item Settings")]
    [SerializeField] private string itemId = "sword";
    [SerializeField] private float hideDelay = 0.5f;
    
    [Header("Interaction")]
    [SerializeField] private Transform promptPosition;
    
    private bool isPickedUp = false;
    [SerializeField] private Outline outline;
    
    // Static registry to find items by ID
    private static Dictionary<string, PickupableItem> itemRegistry = new Dictionary<string, PickupableItem>();
    
    public string ItemId => itemId;
    public bool IsPickedUp => isPickedUp;
    
    void Awake()
    {
        if (outline != null)
        {
            outline.OutlineMode = Outline.Mode.OutlineVisible;
            outline.enabled = false;
        }
        
        if (promptPosition == null)
        {
            promptPosition = transform;
        }
        
        // Register this item
        if (!string.IsNullOrEmpty(itemId))
        {
            itemRegistry[itemId] = this;
        }
    }
    
    void OnDestroy()
    {
        // Unregister on destroy
        if (!string.IsNullOrEmpty(itemId) && itemRegistry.ContainsKey(itemId) && itemRegistry[itemId] == this)
        {
            itemRegistry.Remove(itemId);
        }
    }
    
    void Start()
    {
        // Ensure outline is disabled after OnEnable runs
        if (outline != null)
        {
            outline.enabled = false;
        }
    }
    
    /// <summary>
    /// Find a pickupable item by its ID
    /// </summary>
    public static PickupableItem GetByItemId(string id)
    {
        if (itemRegistry.TryGetValue(id, out PickupableItem item))
        {
            return item;
        }
        return null;
    }
    
    // IInteractable Implementation
    public bool CanInteract(string playerTag)
    {
        // Only Thief can pick up items
        return playerTag == "Thief" && !isPickedUp;
    }
    
    public void Interact(GameObject interactingPlayer)
    {
        Pickup();
        
        // Find the local player's equipment and equip the item
        if (itemId == "bat" || itemId == "sword" || itemId == "s,word")
        {
            // Find local player's PlayerEquipment
            foreach (var equipment in FindObjectsOfType<PlayerEquipment>())
            {
                if (equipment.photonView.IsMine)
                {
                    equipment.EquipItem(itemId);
                    break;
                }
            }
        }
    }
    
    public string GetInteractionPrompt()
    {
        return $"Pick Up {itemId}";
    }
    
    public Transform GetPromptPosition()
    {
        return promptPosition;
    }
    
    public void ShowHighlight()
    {
        if (outline != null) outline.enabled = true;
    }
    
    public void HideHighlight()
    {
        if (outline != null) outline.enabled = false;
    }
    
    [PunRPC]
    private void RPC_OnPickedUp()
    {
        if (isPickedUp) return;
        isPickedUp = true;
        StartCoroutine(HideAfterDelay());
    }
    
    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(hideDelay);
        gameObject.SetActive(false);
    }
    
    public void Pickup()
    {
        if (isPickedUp) return;
        
        if (photonView != null)
        {
            photonView.RPC(nameof(RPC_OnPickedUp), RpcTarget.AllBuffered);
        }
        else
        {
            RPC_OnPickedUp();
        }
    }
    
    /// <summary>
    /// Called when the thief holding this item is caught.
    /// Respawns the item at its original location.
    /// </summary>
    public void Respawn()
    {
        if (photonView != null)
        {
            photonView.RPC(nameof(RPC_Respawn), RpcTarget.AllBuffered);
        }
        else
        {
            RPC_Respawn();
        }
    }
    
    [PunRPC]
    private void RPC_Respawn()
    {
        isPickedUp = false;
        gameObject.SetActive(true);
        Debug.Log($"[PickupableItem] {itemId} has respawned!");
        
        // Notify all players
        NotificationHUD.Show($"The {itemId} has returned!");
    }
}

