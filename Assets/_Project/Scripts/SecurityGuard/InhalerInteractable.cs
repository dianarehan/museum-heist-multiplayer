using UnityEngine;
using Photon.Pun;
using _Project.Scripts.Interaction;

public class InhalerInteractable : MonoBehaviourPun, IInteractableEnhanced
{
    [Header("Settings")]
    [SerializeField] private float useCooldown = 10f;
    private bool used = false;

    [Header("Audio")]
    [SerializeField] private AudioClip inhaleSound;
    private AudioSource audioSource;
    
    [Header("Interaction")]
    [SerializeField] private string interactionPrompt = "Use Inhaler";
    [SerializeField] private Transform promptPosition;
    [SerializeField] private Outline outline;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        
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
        // Only Guard can use, and only when not used/on cooldown
        return playerTag == "Guard" && !used;
    }
    
    public void Interact(GameObject interactingPlayer)
    {
        var tiredness = interactingPlayer.GetComponent<GuardTirednessEffect>();
        if (tiredness != null)
        {
            Use(tiredness);
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

    public void Use(GuardTirednessEffect tiredness)
    {
        if (used) return;

        used = true;
        tiredness.StopTiredness();

        Debug.Log("Inhaler used - tiredness stopped");

        if (audioSource != null && inhaleSound != null)
        {
            audioSource.PlayOneShot(inhaleSound);
            Debug.Log("Inhaler sound played");
        }
        
        // Hide inhaler
        gameObject.SetActive(false);

        // Respawn later
        Invoke(nameof(ResetInhaler), useCooldown);
    }

    private void ResetInhaler()
    {
        used = false;
        gameObject.SetActive(true);
    }
}
