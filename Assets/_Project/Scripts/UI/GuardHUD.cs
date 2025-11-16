using Photon.Pun;
using UnityEngine;

namespace _Project.Scripts.UI
{
    public class GuardHUD : MonoBehaviour
    {
        public static GuardHUD Instance;
        private Canvas canvas;
        
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
    }
}