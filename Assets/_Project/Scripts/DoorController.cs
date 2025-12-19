using UnityEngine;
using Photon.Pun;

public class DoorController : MonoBehaviourPun, IInteractable
{
    [Header("Interaction Settings")]
    [SerializeField] private string interactionPrompt = "Open Door";
    [SerializeField] private Transform promptPosition;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;
    
    [Header("Outline")]
    [SerializeField] private Outline outline;
    
    private bool isOpen = false;
    private Animator anim;
    
    void Awake()
    {
        outline = GetComponent<Outline>();
        if (outline != null)
        {
            outline.OutlineMode = Outline.Mode.OutlineVisible;
            outline.enabled = false;
        }
    }
    
    void Start()
    {
        anim = GetComponent<Animator>();
        if (anim == null)
        {
            Debug.LogError("Animator not found on door controller");
        }
        
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
        
        if (promptPosition == null)
        {
            promptPosition = transform;
        }
        
        // Ensure outline is disabled on start
        if (outline != null)
        {
            outline.enabled = false;
        }
    }
    
    // IInteractable Implementation
    public bool CanInteract(string playerTag)
    {
        // Both Guard and Thief can use doors
        return playerTag == "Guard" || playerTag == "Thief";
    }
    
    public void Interact()
    {
        photonView.RPC("RPC_ToggleDoor", RpcTarget.All);
    }
    
    public string GetInteractionPrompt()
    {
        return isOpen ? "Close Door" : "Open Door";
    }
    
    public Transform GetPromptPosition()
    {
        return promptPosition;
    }
    
    // Highlight methods for outline
    public void ShowHighlight()
    {
        if (outline != null) outline.enabled = true;
    }
    
    public void HideHighlight()
    {
        if (outline != null) outline.enabled = false;
    }

    [PunRPC]
    private void RPC_ToggleDoor()
    {
        if (isOpen)
        {
            CloseDoor();
        }
        else
        {
            OpenDoor();
        }
    }

    private void OpenDoor()
    {
        anim.SetBool("opening", true);
        anim.SetBool("closing", false);
        anim.SetBool("closed", false);
        anim.SetBool("open", true);
        isOpen = true;
        
        PlaySound(openSound);
    }

    private void CloseDoor()
    {
        anim.SetBool("closing", true);
        anim.SetBool("opening", false);
        anim.SetBool("open", false);
        anim.SetBool("closed", true);
        isOpen = false;
        
        PlaySound(closeSound);
    }
    
    private void PlaySound(AudioClip clip)
    {
        if (clip == null) return;
        
        if (audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
        else
        {
            AudioSource.PlayClipAtPoint(clip, transform.position);
        }
    }
}
