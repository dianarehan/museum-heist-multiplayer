using _Project.Scripts.Interaction;
using UnityEngine;
using Photon.Pun;

public class KnockedOut : MonoBehaviourPun, IInteractableEnhanced
{
    private bool isCarried = false;
    [SerializeField] private Transform promptPosition;
    [SerializeField] private Transform carryPosition; // Position on guard where thief will be attached
    
    private GameObject currentGuard; // Reference to the guard carrying this thief
    private Animator thiefAnimator;
    private CharacterController thiefController;
    private Rigidbody thiefRigidbody;
    
    // Animation parameter names
    private const string THIEF_CARRIED_ANIM = "IsCarried";
    
    void Start()
    {
        if (promptPosition == null)
        {
            promptPosition = transform;
        }
        
        thiefAnimator = GetComponent<Animator>();
        thiefController = GetComponent<CharacterController>();
        thiefRigidbody = GetComponent<Rigidbody>();
    }
    
    public bool CanInteract(string playerTag)
    {
        // Only Guard can carry the thief
        return playerTag == "Guard";
    }

    public void Interact()
    {
        // Find the guard who is interacting
        GameObject[] guards = GameObject.FindGameObjectsWithTag("Guard");
        GameObject nearestGuard = null;
        float minDistance = float.MaxValue;
        
        foreach (GameObject guard in guards)
        {
            float distance = Vector3.Distance(transform.position, guard.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestGuard = guard;
            }
        }
        
        if (nearestGuard != null)
        {
            PhotonView guardPhotonView = nearestGuard.GetComponent<PhotonView>();
            if (guardPhotonView != null)
            {
                photonView.RPC("RPC_CarryThief", RpcTarget.All, guardPhotonView.ViewID);
            }
        }
    }
    
    public void Interact(GameObject interactor)
    {
        // Use the interactor directly instead of searching for guards
        if (interactor == null)
        {
            Debug.LogWarning("Interactor is null!");
            return;
        }
    
        // Verify the interactor is actually a guard
        if (interactor.CompareTag("Guard"))
        {
            PhotonView guardPhotonView = interactor.GetComponent<PhotonView>();
            if (guardPhotonView != null)
            {
                photonView.RPC("RPC_CarryThief", RpcTarget.All, guardPhotonView.ViewID);
            }
            else
            {
                Debug.LogWarning("Guard does not have a PhotonView component!");
            }
        }
        else
        {
            Debug.LogWarning("Interactor is not a Guard!");
        }
    }
    
    public string GetInteractionPrompt()
    {
        return isCarried ? "Put Down Thief" : "Carry Thief";
    }
    
    public Transform GetPromptPosition()
    {
        return promptPosition;
    }
    
    [PunRPC]
    private void RPC_CarryThief(int guardViewID)
    {
        PhotonView guardPhotonView = PhotonView.Find(guardViewID);
        if (guardPhotonView == null) return;
        
        GameObject guard = guardPhotonView.gameObject;
        
        if (!isCarried)
        {
            // Start carrying
            StartCarrying(guard);
        }
        else
        {
            // Put down
            StopCarrying(guard);
        }
        
        isCarried = !isCarried;
    }
    
    private void StartCarrying(GameObject guard)
    {
        currentGuard = guard;
        Debug.Log(guard);
        
        // Get guard's animator and trigger carrying animation
        Animator guardAnimator = guard.GetComponent<Animator>();
        if (guardAnimator != null)
        {
            guardAnimator.SetBool("IsCarrying", true);
        }
        
        // Get guard's carry position (should be a child transform on the guard)
        GuardCarrySystem carrySystem = guard.GetComponent<GuardCarrySystem>();
        if (carrySystem != null)
        {
            carryPosition = carrySystem.GetCarryPosition();
        }
        
        // Trigger thief carried animation
        if (thiefAnimator != null)
        {
            thiefAnimator.SetBool(THIEF_CARRIED_ANIM, true);
        }
        
        // Disable thief's physics/movement
        if (thiefController != null)
        {
            thiefController.enabled = false;
        }
        
        if (thiefRigidbody != null)
        {
            thiefRigidbody.isKinematic = true;
            thiefRigidbody.useGravity = false;
        }
        
        // Parent thief to guard's carry position
        if (carryPosition != null)
        {
            transform.SetParent(carryPosition);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.Euler(0, 0, 0); // Adjust rotation as needed
        }
        else
        {
            // Fallback: parent to guard directly with offset
            transform.SetParent(guard.transform);
            transform.localPosition = new Vector3(0.7f, 0f, 0.5f); // Offset to side
            transform.localRotation = Quaternion.Euler(0, -90, 0);
        }
    }
    
    private void StopCarrying(GameObject guard)
    {
        // Stop guard's carrying animation
        Animator guardAnimator = guard.GetComponent<Animator>();
        if (guardAnimator != null)
        {
            guardAnimator.SetBool("IsCarrying", false);
        }
        
        // Stop thief's carried animation
        if (thiefAnimator != null)
        {
            thiefAnimator.SetBool(THIEF_CARRIED_ANIM, false);
        }
        
        // Unparent thief
        transform.SetParent(null);
        
        // Re-enable thief's physics/movement
        if (thiefController != null)
        {
            thiefController.enabled = true;
        }
        
        if (thiefRigidbody != null)
        {
            thiefRigidbody.isKinematic = false;
            thiefRigidbody.useGravity = true;
        }
        
        
        // Position thief slightly in front of guard
        transform.position = guard.transform.position + guard.transform.forward * 1.5f;
        transform.rotation = guard.transform.rotation;
        
        currentGuard = null;
    }
    
    // Optional: Update to smoothly follow guard if not using parenting
    void LateUpdate()
    {
        if (isCarried && currentGuard != null && carryPosition == null)
        {
            // Fallback smooth follow if no carry position
            transform.position = Vector3.Lerp(
                transform.position, 
                currentGuard.transform.position + currentGuard.transform.right * 0.7f + Vector3.up * 0.5f,
                Time.deltaTime * 10f
            );
        }
    }
}