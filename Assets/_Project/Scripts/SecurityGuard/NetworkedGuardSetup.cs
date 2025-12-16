using UnityEngine;
using Photon.Pun;

public class NetworkedGuardSetup : MonoBehaviourPun
{
    [Header("Local-only components")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private AudioListener audioListener;
    [SerializeField] private MonoBehaviour[] inputScripts;
    // e.g. FirstPersonMovement, FirstPersonCamera, maybe UpperBodyIK if it's local-only

    private void Awake()
    {
        if (photonView.IsMine)
        {
            // This is MY player
            if (playerCamera != null)
            {
                playerCamera.gameObject.SetActive(true);
                // Optionally disable the old scene camera if still present
                var main = Camera.main;
                if (main != null && main != playerCamera)
                    main.gameObject.SetActive(false);
            }

            if (audioListener != null)
                audioListener.enabled = true;

            foreach (var script in inputScripts)
                if (script != null) script.enabled = true;
        }
        else
        {
            // This is someone ELSE's player
            if (playerCamera != null)
                playerCamera.enabled = false;

            if (audioListener != null)
                audioListener.enabled = false;

            foreach (var script in inputScripts)
                if (script != null) script.enabled = false;
        }
    }
}
