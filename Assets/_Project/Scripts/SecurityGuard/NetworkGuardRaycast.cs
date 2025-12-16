using UnityEngine;
using Photon.Pun;

namespace _Project.Scripts.Player
{
    public class NetworkGuardRaycast : MonoBehaviourPunCallbacks // Changed to MonoBehaviourPunCallbacks
    {
        [Header("Raycast Settings")]
        [SerializeField] private float raycastRange = 10f;
        [SerializeField] private LayerMask targetLayers; // Optional: set specific layers
        
        private Camera playerCamera;
        private PhotonView photonView;

        private void Start()
        {
            // Get the camera (assuming it's a child of the Guard)
            playerCamera = GetComponentInChildren<Camera>();
            photonView = GetComponent<PhotonView>();
            
            if (playerCamera == null)
            {
                Debug.LogError("Camera not found on Guard!");
            }
        }

        private void Update()
        {
            // Only allow the local player to raycast
            if (photonView != null && !photonView.IsMine)
                return;
            
            // Check for left mouse button click
            if (Input.GetMouseButtonDown(0))
            {
                PerformRaycast();
            }
        }

        private void PerformRaycast()
        {
            if (playerCamera == null)
                return;

            Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            RaycastHit hit;

            // Perform the raycast
            if (Physics.Raycast(ray, out hit, raycastRange))
            {
                Debug.Log($"Hit object: {hit.collider.gameObject.name}");
                
                // Check if the hit object has the "Thief" tag
                if (hit.collider.CompareTag("Thief"))
                {
                    Debug.Log("THIEF DETECTED! Action triggered!");
                    OnThiefDetected(hit.collider.gameObject);
                }
            }
            else
            {
                Debug.Log("Raycast hit nothing");
            }
        }

        private void OnThiefDetected(GameObject thief)
        {
            Debug.Log($"Caught thief: {thief.name}");
            
            // Get the thief's PhotonView
            PhotonView thiefPhotonView = thief.GetComponent<PhotonView>();
            if (thiefPhotonView != null)
            {
                // Call the knockout function on the thief's network object
                thiefPhotonView.RPC("RPC_KnockoutThief", RpcTarget.AllBuffered);
                
                // Notify game state to update
                if (PhotonNetwork.IsMasterClient)
                {
                    GameState.Instance.ThiefDied();
                }
                else
                {
                    // Request master client to update via GameState's PhotonView
                    PhotonView gameStateView = GameState.Instance.GetComponent<PhotonView>();
                    if (gameStateView != null)
                    {
                        gameStateView.RPC("RPC_ThiefCaughtRequest", RpcTarget.MasterClient);
                    }
                }
            }
        }
        
    }
}