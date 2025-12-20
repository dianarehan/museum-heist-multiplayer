using _Project.Scripts.Interaction;
using UnityEngine;
using TMPro;

/// <summary>
/// Base class for interactable objects. Handles outline and floating prompt.
/// Inherit from this or implement IInteractable directly.
/// </summary>
[RequireComponent(typeof(Outline))]
public abstract class InteractableBase : MonoBehaviour, IInteractableEnhanced
{
    [Header("Interaction Settings")]
    [SerializeField] protected string interactionPrompt = "Interact";
    [SerializeField] protected bool guardCanInteract = true;
    [SerializeField] protected bool thiefCanInteract = true;
    
    [Header("Prompt Settings")]
    [SerializeField] protected Transform promptPosition;
    [SerializeField] protected GameObject promptPrefab;
    
    [SerializeField] protected Outline outline;
    protected GameObject promptInstance;
    protected TextMeshPro promptText;
    protected bool isHighlighted = false;
    
    protected virtual void Awake()
    {
        if (outline != null)
        {
            outline.OutlineMode = Outline.Mode.OutlineVisible;
            outline.enabled = false;
            Debug.Log("Outline enabled");   
        }
        
        // Default prompt position to this object's top
        if (promptPosition == null)
        {
            promptPosition = transform;
        }
    }
    
    public virtual bool CanInteract(string playerTag)
    {
        if (playerTag == "Guard") return guardCanInteract;
        if (playerTag == "Thief") return thiefCanInteract;
        return false;
    }
    
    public abstract void Interact(GameObject interactingPlayer);
    
    public virtual string GetInteractionPrompt()
    {
        return interactionPrompt;
    }
    
    public virtual Transform GetPromptPosition()
    {
        return promptPosition;
    }
    
    public virtual void ShowHighlight()
    {
        if (isHighlighted) return;
        isHighlighted = true;
        
        if (outline != null)
        {
            outline.enabled = true;
        }
    }
    
    public virtual void HideHighlight()
    {
        if (!isHighlighted) return;
        isHighlighted = false;
        
        if (outline != null)
        {
            outline.enabled = false;
        }
    }
}
