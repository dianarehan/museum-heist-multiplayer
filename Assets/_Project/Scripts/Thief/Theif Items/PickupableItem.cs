using Photon.Pun;
using UnityEngine;
using System.Collections;

/// <summary>
/// Attach to a pickupable item in the scene (like a bat on the floor).
/// When player presses E while looking at it, it disappears and equips the player.
/// </summary>
public class PickupableItem : MonoBehaviourPun
{
    [Header("Item Settings")]
    [SerializeField] private string itemId = "bat";
    [SerializeField] private float hideDelay = 0.5f; // Delay before hiding to allow animation
    
    private bool isPickedUp = false;
    
    public string ItemId => itemId;
    public bool IsPickedUp => isPickedUp;
    
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
