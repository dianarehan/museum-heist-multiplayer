using Photon.Pun;
using UnityEngine;
using System.Collections;
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
    }
    
    void Start()
    {
        // Ensure outline is disabled after OnEnable runs
        if (outline != null)
        {
            outline.enabled = false;
        }
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
}
