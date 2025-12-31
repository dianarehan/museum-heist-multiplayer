using UnityEngine;
using Photon.Pun;
using System.Collections;
using _Project.Scripts.Player;
using _Project.Scripts.Interaction;

public class PistolChargingStation : MonoBehaviourPun, IInteractableEnhanced
{
    [Header("Charging Settings")]
    private bool isOccupied = false;
    [SerializeField] private GameObject pistolVisual;

    [Header("Audio")]
    [SerializeField] private AudioClip placeOnChargerSfx;
    [SerializeField] private AudioClip pickupFromChargerSfx;
    private AudioSource audioSource;
    
    [Header("Interaction")]
    [SerializeField] private string interactionPrompt = "Charge Pistol";
    [SerializeField] private Transform promptPosition;
    [SerializeField] private Outline outline;
    
    // Store reference to the guard currently using this station
    private NetworkGuardRaycast currentGuard;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        
        if (outline == null) outline = GetComponent<Outline>();
        if (outline != null)
        {
            outline.OutlineMode = Outline.Mode.OutlineVisible;
            outline.enabled = false;
        }
        
        if (promptPosition == null) promptPosition = transform;
    }

    // IInteractableEnhanced Implementation
    public bool CanInteract(string playerTag)
    {
        // Only Guard can use, and only when not occupied
        return playerTag == "Guard" && !isOccupied;
    }
    
    public void Interact(GameObject interactingPlayer)
    {
        var guard = interactingPlayer.GetComponent<NetworkGuardRaycast>();
        if (guard != null && guard.CurrentCharges < guard.MaxCharges)
        {
            StartCharging(guard);
        }
    }
    
    public string GetInteractionPrompt()
    {
        return interactionPrompt;
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

    public void StartCharging(NetworkGuardRaycast guard)
    {
        if (isOccupied) return;

        isOccupied = true;
        currentGuard = guard;
        
        if (audioSource != null && placeOnChargerSfx != null)
            audioSource.PlayOneShot(placeOnChargerSfx);
            
        StartCoroutine(ChargeRoutine(guard));
    }

    private IEnumerator ChargeRoutine(NetworkGuardRaycast guard)
    {
        guard.OnPistolPlacedOnCharger();
        OnPistolPlacedOnCharger();

        yield return new WaitForSeconds(guard.RechargeTime);

        guard.OnPistolFullyCharged();
        OnPistolFullyCharged();
        
        if (audioSource != null && pickupFromChargerSfx != null)
            audioSource.PlayOneShot(pickupFromChargerSfx);

        isOccupied = false;
        currentGuard = null;
    }
    
    public void OnPistolPlacedOnCharger()
    {
        pistolVisual.SetActive(true);
    }
    
    public void OnPistolFullyCharged()
    {
        pistolVisual.SetActive(false);
    }
}

