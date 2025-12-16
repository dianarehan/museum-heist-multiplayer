using UnityEngine;
using Photon.Pun;
using Unity.Cinemachine;

public class ThiefCameraSetup : MonoBehaviourPun
{
    [SerializeField] private CinemachineCamera fpCamera;
    [SerializeField] private CinemachineCamera tpCamera;   // optional

    void Start()
    {
        bool mine = photonView.IsMine;

        if (fpCamera != null) fpCamera.enabled = mine;
        if (tpCamera != null) tpCamera.enabled = mine;
    }
}
