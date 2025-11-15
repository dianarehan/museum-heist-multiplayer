using UnityEngine;
using Photon.Pun;

public class ThiefCameraSetup : MonoBehaviourPun
{
    [SerializeField] private Unity.Cinemachine.CinemachineCamera fpCamera;
    [SerializeField] private AudioListener audioListener;

    void Start()
    {
        if (!photonView.IsMine)
        {
            // Disable remote cameras
            if (fpCamera != null) fpCamera.enabled = false;
            if (audioListener != null) audioListener.enabled = false;
        }
        else
        {
            // Enable only my camera
            if (fpCamera != null) fpCamera.enabled = true;

            // Audio should ONLY be for local player
            if (audioListener != null) audioListener.enabled = true;
        }
    }
}
