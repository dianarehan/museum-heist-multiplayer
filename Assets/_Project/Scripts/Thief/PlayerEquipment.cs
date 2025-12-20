using Photon.Pun;
using UnityEngine;
using System.Collections;

/// <summary>
/// Manages player equipment - pickup items, show equipped items, and use them.
/// Attach to the Thief player.
/// </summary>
public class PlayerEquipment : MonoBehaviourPun
{
    [Header("Raycast Settings")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float pickupRange = 3f;
    [SerializeField] private float pickupRadius = 0.3f; // SphereCast radius for easier detection
    [SerializeField] private LayerMask pickupLayers;
    
    [Header("Equipped Items")]
    [SerializeField] private GameObject handBat;
    [SerializeField] private float equipDelay = 0.5f; // Delay before showing hand bat (matches animation)
    
    [Header("Input")]
    [SerializeField] private KeyCode pickupKey = KeyCode.E;
    [SerializeField] private KeyCode swingKey = KeyCode.Mouse0; // Left click to swing
    
    [Header("Swing Settings")]
    [SerializeField] private float swingCooldown = 0.5f;
    [SerializeField] private float swingRange = 2f;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip swingSound;
    [SerializeField] private AudioClip pickupSound;
    
    private Animator anim;
    private bool hasBat = false;
    private float lastSwingTime = 0f;
    
    public bool HasBat => hasBat;
    
    private void Start()
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
        {
            enabled = false;
            return;
        }
        
        anim = GetComponent<Animator>();
        
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
        
        // Make sure hand bat starts hidden
        if (handBat != null)
        {
            handBat.SetActive(false);
        }
    }
    
    private void Update()
    {
        if (!photonView.IsMine) return;
        
        // Debug ray visualization in Scene view
        Debug.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * pickupRange, Color.yellow);
        
        HandlePickup();
        HandleSwing();
    }
    
    private void HandlePickup()
    {
        if (!Input.GetKeyDown(pickupKey)) return;
        
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;
        
        // Use SphereCast for wider detection area
        if (Physics.SphereCast(ray, pickupRadius, out hit, pickupRange, pickupLayers))
        {
            PickupableItem item = hit.collider.GetComponent<PickupableItem>();
            
            // Also check parent if not found on collider
            if (item == null)
            {
                item = hit.collider.GetComponentInParent<PickupableItem>();
            }
            
            if (item != null && !item.IsPickedUp)
            {
                Debug.Log($"Found pickupable item: {item.ItemId}");
                
                // Check what type of item it is
                if (item.ItemId == "bat")
                {
                    item.Pickup();
                    EquipBat();
                }
            }
        }
        else
        {
            Debug.Log("Pickup raycast hit nothing on pickup layers");
        }
    }
    
    /// <summary>
    /// Called by PickupableItem when item is picked up via IInteractable
    /// </summary>
    public void EquipItem(string itemId)
    {
        Debug.Log($"[PlayerEquipment] EquipItem called with: {itemId}");
        if (itemId == "bat" || itemId == "sword" || itemId == "s,word")
        {
            EquipBat();
        }
    }
    
    private void EquipBat()
    {
        hasBat = true;
        equippedItemId = "sword"; // or "bat" - track what was equipped
        
        // Show the hand bat for all players
        photonView.RPC(nameof(RPC_ShowHandBat), RpcTarget.All, true);
    }
    
    private string equippedItemId = "";
    
    /// <summary>
    /// Drop the bat when thief is caught. Respawns the floor item.
    /// </summary>
    public void DropBat()
    {
        if (!hasBat) return;
        
        Debug.Log("[PlayerEquipment] Dropping bat!");
        
        // Hide hand bat
        photonView.RPC(nameof(RPC_ShowHandBat), RpcTarget.All, false);
        
        // Respawn floor item
        var floorItem = PickupableItem.GetByItemId(equippedItemId);
        if (floorItem != null)
        {
            floorItem.Respawn();
        }
        
        hasBat = false;
        equippedItemId = "";
    }
    
    private Coroutine showBatCoroutine;
    
    [PunRPC]
    private void RPC_ShowHandBat(bool show)
    {
        Debug.Log($"[PlayerEquipment] RPC_ShowHandBat called: show={show}, handBat={handBat != null}");
        
        // Cancel any existing coroutine
        if (showBatCoroutine != null)
        {
            StopCoroutine(showBatCoroutine);
            showBatCoroutine = null;
        }
        
        // Play pickup animation first
        if (show && anim != null)
        {
            anim.SetTrigger("pickUp");
        }
        
        // Play pickup sound
        if (show)
        {
            PlayPickupSound();
        }
        
        hasBat = show;
        
        if (show)
        {
            // Delay showing hand bat until after animation
            showBatCoroutine = StartCoroutine(ShowHandBatDelayed());
        }
        else if (handBat != null)
        {
            handBat.SetActive(false);
        }
    }
    
    private void PlayPickupSound()
    {
        if (pickupSound == null)
        {
            Debug.LogWarning("Pickup sound not assigned!");
            return;
        }
        Debug.Log("Playing pickup sound");
        
        // Use existing audioSource if available, otherwise create temp one
        if (audioSource != null)
        {
            audioSource.PlayOneShot(pickupSound, 1f);
        }
        else
        {
            // Create temp AudioSource on camera
            GameObject tempAudio = new GameObject("TempPickupSound");
            tempAudio.transform.position = playerCamera != null ? playerCamera.transform.position : transform.position;
            AudioSource tempSource = tempAudio.AddComponent<AudioSource>();
            tempSource.clip = pickupSound;
            tempSource.volume = 1f;
            tempSource.Play();
            Destroy(tempAudio, pickupSound.length + 0.1f);
        }
    }
    
    private IEnumerator ShowHandBatDelayed()
    {
        yield return new WaitForSeconds(equipDelay);
        if (handBat != null)
        {
            handBat.SetActive(true);
            // In ShowHandBatDelayed()
Debug.Log($"[PlayerEquipment] Setting handBat active: {handBat != null}");
        }
    }
    
    private void HandleSwing()
    {
        if (!hasBat) return;
        if (Time.time - lastSwingTime < swingCooldown) return;
        
        if (Input.GetKeyDown(swingKey))
        {
            lastSwingTime = Time.time;
            
            // Trigger swing animation
            if (anim != null)
            {
                anim.SetTrigger("Hit");
            }
            
            // Play swing sound
            PlaySwingSound();
            
            // Sync swing to all players
            photonView.RPC(nameof(RPC_Swing), RpcTarget.All);
            
            // Check for breakable glass in front
            CheckForBreakables();
        }
    }
    
    private void PlaySwingSound()
    {
        if (swingSound == null)
        {
            Debug.LogWarning("Swing sound clip not assigned!");
            return;
        }
        
        // Use PlayClipAtPoint to avoid conflicts with other audio sources (like footsteps)
        AudioSource.PlayClipAtPoint(swingSound, transform.position);
        Debug.Log("Swing sound played");
    }
    
    [PunRPC]
    private void RPC_Swing()
    {
        // Play swing animation on remote players
        if (anim != null && !photonView.IsMine)
        {
            anim.SetTrigger("Hit");
        }
    }
    
    private void CheckForBreakables()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, swingRange))
        {
            // Check for MeshDemolisherExample (glass)
            var glass = hit.collider.GetComponentInChildren<Hanzzz.MeshDemolisher.MeshDemolisherExample>();
            if (glass != null)
            {
                glass.BreakGlass();
            }
        }
    }
}
