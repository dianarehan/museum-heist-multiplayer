using Photon.Pun;
using UnityEngine;
using TMPro;
using _Project.Scripts.Player;

namespace _Project.Scripts.UI
{
    public class GuardHUD : MonoBehaviour
    {
        public static GuardHUD Instance;
        private Canvas canvas;
        
        [Header("Charges Display")]
        [SerializeField] private TMP_Text chargesText;
        [SerializeField] private string chargesFormat = "CHARGES: {0}/{1}";
        
        private NetworkGuardRaycast guardRaycast;
        
        private void Awake()
        {
            Instance = this;
            canvas = GetComponent<Canvas>();
        }
    
        private void Start()
        {
            // Check local player's role
            object roleObj;
            if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("Role", out roleObj))
            {
                string role = roleObj as string;
                Debug.Log(role);
                
                // If I'm NOT a guard disable this whole UI
                if (role != "Guard")
                {
                    gameObject.SetActive(false);
                    return;
                }
                
                // If I AM a guard, find the guard player and link the canvas to their camera
                if (role == "Guard")
                {
                    // Find the Guard player GameObject
                    GameObject guardPlayer = GameObject.FindGameObjectWithTag("Guard");
                    
                    if (guardPlayer != null)
                    {
                        // Find the camera in the guard player (adjust path as needed)
                        Camera guardCamera = guardPlayer.GetComponentInChildren<Camera>();
                        
                        if (guardCamera != null)
                        {
                            // Link canvas to the guard's camera
                            canvas.renderMode = RenderMode.ScreenSpaceCamera;
                            canvas.worldCamera = guardCamera;
                            canvas.planeDistance = 0.3f;
                        }
                        else
                        {
                            Debug.LogError("Guard camera not found!");
                        }
                        
                        // Get NetworkGuardRaycast for charges
                        guardRaycast = guardPlayer.GetComponent<NetworkGuardRaycast>();
                    }
                    else
                    {
                        Debug.LogError("Guard player GameObject not found!");
                    }
                }
            }
            else
            {
                // No role set? safest: hide
                gameObject.SetActive(false);
                return;
            }
        }
        
        private void Update()
        {
            UpdateChargesDisplay();
        }
        
        private void UpdateChargesDisplay()
        {
            if (chargesText == null || guardRaycast == null) return;
            
            chargesText.text = string.Format(chargesFormat, 
                guardRaycast.CurrentCharges, 
                guardRaycast.MaxCharges);
        }
    }
}
