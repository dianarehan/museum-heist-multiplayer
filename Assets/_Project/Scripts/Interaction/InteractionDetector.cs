using UnityEngine;
using TMPro;
using Photon.Pun;

/// <summary>
/// Attach to player prefabs (Guard/Thief) to detect and highlight interactables.
/// Shows floating [E] prompt above interactable objects.
/// </summary>
public class InteractionDetector : MonoBehaviourPun
{
    [Header("Raycast Settings")]
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private LayerMask interactableLayers = -1;
    
    [Header("Prompt UI (World Space)")]
    [SerializeField] private GameObject promptPrefab;
    [SerializeField] private Vector3 promptOffset = new Vector3(0f, 1.5f, 0f);
    
    private Camera playerCamera;
    private IInteractable currentTarget;
    private GameObject currentTargetObject;
    private Outline currentOutline;
    private GameObject promptInstance;
    private TextMeshPro promptText;
    
    private void Start()
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
        {
            enabled = false;
            return;
        }
        
        playerCamera = GetComponentInChildren<Camera>();
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
        
        // Create prompt instance if prefab is assigned
        if (promptPrefab != null)
        {
            promptInstance = Instantiate(promptPrefab);
            promptInstance.SetActive(false);
            promptText = promptInstance.GetComponentInChildren<TextMeshPro>();
        }
    }
    
    private void Update()
    {
        if (!photonView.IsMine) return;
        if (playerCamera == null) return;
        
        UpdateTarget();
        HandleInput();
    }
    
    private void UpdateTarget()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;
        
        IInteractable newTarget = null;
        GameObject newTargetObject = null;
        Outline newOutline = null;
        
        if (Physics.Raycast(ray, out hit, interactionRange, interactableLayers))
        {
            // Try to get IInteractable from hit object or parents
            newTarget = hit.collider.GetComponent<IInteractable>();
            if (newTarget == null)
            {
                newTarget = hit.collider.GetComponentInParent<IInteractable>();
            }
            
            // Get the GameObject and Outline for highlighting
            if (newTarget != null)
            {
                newTargetObject = hit.collider.gameObject;
                
                // Try to get Outline from hit object or parents
                newOutline = hit.collider.GetComponent<Outline>();
                if (newOutline == null)
                {
                    newOutline = hit.collider.GetComponentInParent<Outline>();
                }
                
                // Check if player can interact
                if (!newTarget.CanInteract(gameObject.tag))
                {
                    newTarget = null;
                    newTargetObject = null;
                    newOutline = null;
                }
            }
        }
        
        // Handle target change
        if (newTarget != currentTarget)
        {
            // Hide previous highlight
            if (currentOutline != null)
            {
                currentOutline.enabled = false;
            }
            
            currentTarget = newTarget;
            currentTargetObject = newTargetObject;
            currentOutline = newOutline;
            
            // Show new highlight
            if (currentOutline != null)
            {
                currentOutline.enabled = true;
            }
            
            UpdatePrompt();
        }
        
        // Update prompt position if target exists
        if (currentTarget != null && promptInstance != null)
        {
            Transform promptPos = currentTarget.GetPromptPosition();
            promptInstance.transform.position = promptPos.position + promptOffset;
            
            // Billboard - face camera
            promptInstance.transform.LookAt(playerCamera.transform);
            promptInstance.transform.Rotate(0, 180, 0);
        }
    }
    
    private void UpdatePrompt()
    {
        if (promptInstance == null) return;
        
        if (currentTarget != null)
        {
            promptInstance.SetActive(true);
            if (promptText != null)
            {
                promptText.text = $"[E] {currentTarget.GetInteractionPrompt()}";
            }
        }
        else
        {
            promptInstance.SetActive(false);
        }
    }
    
    private void HandleInput()
    {
        if (currentTarget == null) return;
        
        if (Input.GetKeyDown(KeyCode.E))
        {
            currentTarget.Interact();
        }
    }
    
    private void OnDestroy()
    {
        if (promptInstance != null)
        {
            Destroy(promptInstance);
        }
    }
}
