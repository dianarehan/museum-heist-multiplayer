using Photon.Pun;
using UnityEngine;
using System.Collections;
using _Project.Scripts.Interaction;

public class StealableItem : MonoBehaviourPun, IInteractableEnhanced
{
    [SerializeField] private int value = 100;
    
    [Header("Protection")]
    [Tooltip("If true, item cannot be stolen until protection is removed (e.g., glass is shattered)")]
    [SerializeField] private bool isProtected = false;
    
    [Header("Animation")]
    [SerializeField] private float hideDelay = 0.5f;
    
    [Header("Interaction")]
    [SerializeField] private Transform promptPosition;

    private bool isStolen = false;
    [SerializeField] private Outline outline;

    public int Value => value;
    
    /// <summary>
    /// Returns true if item can be stolen:
    /// - Must be an objective (in ObjectiveManager list)
    /// - Not protected
    /// - Not already stolen
    /// </summary>
    public bool CanSteal
    {
        get
        {
            // Must be an objective to be stealable
            if (ObjectiveManager.Instance != null && !ObjectiveManager.Instance.IsObjective(this))
            {
                return false;
            }
            return !isProtected && !isStolen;
        }
    }
    
    /// <summary>
    /// Returns true if this item is a valid objective
    /// </summary>
    public bool IsObjective
    {
        get
        {
            return ObjectiveManager.Instance != null && ObjectiveManager.Instance.IsObjective(this);
        }
    }
    
    public bool IsProtected => isProtected;
    public bool IsStolen => isStolen;
    
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
        // Only Thief can steal, and only if stealable
        return playerTag == "Thief" && CanSteal;
    }
    
    public void Interact(GameObject interactingPlayer)
    {
        Steal();
    }
    
    public string GetInteractionPrompt()
    {
        if (isProtected) return "Protected";
        if (isStolen) return "Already Stolen";
        return $"Steal (${value})";
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
    private void RPC_OnStolen()
    {
        if (isStolen) return;
        isStolen = true;
        
        // Notify ObjectiveManager
        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.MarkAcquired(this);
        }
        
        StartCoroutine(HideAfterDelay());
    }
    
    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(hideDelay);
        gameObject.SetActive(false);
    }
    
    [PunRPC]
    private void RPC_UnlockProtection()
    {
        isProtected = false;
    }

    public void Steal()
    {
        if (isStolen || isProtected) return;
        
        // Check if this is an objective
        if (ObjectiveManager.Instance != null && !ObjectiveManager.Instance.IsObjective(this))
        {
            Debug.Log("Cannot steal - not an objective item");
            return;
        }

        if (photonView != null)
        {
            photonView.RPC(nameof(RPC_OnStolen), RpcTarget.AllBuffered);
        }
        else
        {
            RPC_OnStolen();
        }
    }
    
    public void UnlockProtection()
    {
        if (photonView != null)
        {
            photonView.RPC(nameof(RPC_UnlockProtection), RpcTarget.AllBuffered);
        }
        else
        {
            RPC_UnlockProtection();
        }
    }
}
