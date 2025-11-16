using UnityEngine;
using Photon.Pun;

namespace _Project.Scripts.Player
{
    [RequireComponent(typeof(PhotonView))]
    public class ThiefController : MonoBehaviourPunCallbacks
    {
        [Header("Components")]
        private CharacterController characterController;
        private Animator animator;
        private PhotonView pv;
        
        [Header("Ragdoll")]
        [SerializeField] private bool isKnockedOut = false;
        private Rigidbody[] ragdollRigidbodies;
        private Collider[] ragdollColliders;
        
        // Reference to your player movement script
        private MonoBehaviour playerMovementScript;

        private void Start()
        {
            pv = GetComponent<PhotonView>();
            characterController = GetComponent<CharacterController>();
            animator = GetComponent<Animator>();
            
            // Get your movement script - replace with your actual script name
            // Example: playerMovementScript = GetComponent<FirstPersonController>();
            
            // Setup ragdoll
            SetupRagdoll();
            DisableRagdoll();
        }

        private void SetupRagdoll()
        {
            // Get all rigidbodies in CHILDREN only (not on this GameObject)
            Rigidbody[] allRigidbodies = GetComponentsInChildren<Rigidbody>();
            ragdollRigidbodies = new Rigidbody[allRigidbodies.Length];
            
            int index = 0;
            foreach (var rb in allRigidbodies)
            {
                // Only add rigidbodies that are NOT on the main player object
                if (rb.gameObject != gameObject)
                {
                    ragdollRigidbodies[index] = rb;
                    index++;
                }
            }
            
            // Resize array to actual count
            System.Array.Resize(ref ragdollRigidbodies, index);
            
            // Get all colliders in CHILDREN only
            Collider[] allColliders = GetComponentsInChildren<Collider>();
            ragdollColliders = new Collider[allColliders.Length];
            
            index = 0;
            foreach (var col in allColliders)
            {
                // Only add colliders that are NOT the CharacterController
                if (col.gameObject != gameObject)
                {
                    ragdollColliders[index] = col;
                    index++;
                }
            }
            
            // Resize array to actual count
            System.Array.Resize(ref ragdollColliders, index);
        }

        private void DisableRagdoll()
        {
            // Make all ragdoll rigidbodies kinematic (physics disabled)
            foreach (var rb in ragdollRigidbodies)
            {
                if (rb != null)
                    rb.isKinematic = true;
            }

            // Disable all ragdoll colliders
            foreach (var col in ragdollColliders)
            {
                if (col != null)
                    col.enabled = false;
            }

            // Enable animator for normal animations
            if (animator != null)
                animator.enabled = true;

            // Enable character controller for movement
            if (characterController != null)
                characterController.enabled = true;
        }

        private void EnableRagdoll()
        {
            // Enable physics on all ragdoll parts
            foreach (var rb in ragdollRigidbodies)
            {
                if (rb != null)
                {
                    rb.isKinematic = false;
                    rb.useGravity = true;
                }
            }

            // Enable all ragdoll colliders
            foreach (var col in ragdollColliders)
            {
                if (col != null)
                    col.enabled = true;
            }

            // Disable animator so ragdoll can move freely
            if (animator != null)
                animator.enabled = false;

            // Disable character controller so it doesn't interfere with ragdoll
            if (characterController != null)
                characterController.enabled = false;
        }

        [PunRPC]
        public void RPC_KnockoutThief()
        {
            if (isKnockedOut) return;
            
            isKnockedOut = true;
            
            Debug.Log($"Thief {gameObject.name} has been knocked out!");
            
            // Disable player controls FIRST
            DisablePlayerControls();
            
            // Then enable ragdoll physics
            EnableRagdoll();
            
            // Optional: Add a small force to make them fall
            if (ragdollRigidbodies.Length > 0)
            {
                // Find the spine or chest rigidbody for applying force
                Rigidbody spine = ragdollRigidbodies[0];
                if (spine != null)
                {
                    spine.AddForce(Vector3.back * 100f, ForceMode.Impulse); // Adjust force as needed
                }
            }
        }

        private void DisablePlayerControls()
        {
            // Disable your movement script
            if (playerMovementScript != null)
                playerMovementScript.enabled = false;
            
            // If you have a camera script, disable it
            // var cameraScript = GetComponentInChildren<CameraController>();
            // if (cameraScript != null) cameraScript.enabled = false;
            
            // Unlock cursor only for local player
            if (pv.IsMine)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        public bool IsKnockedOut()
        {
            return isKnockedOut;
        }
    }
}