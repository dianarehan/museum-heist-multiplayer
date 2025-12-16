using UnityEngine;
using Photon.Pun;

public class LookIK : MonoBehaviourPunCallbacks
{
    public Animator animator;
    public Transform lookTarget; // child of Camera Holder, a bit forward
    [Range(0, 1)] public float weight = 1f;   // overall
    [Range(0, 1)] public float bodyWeight = 0.5f;
    [Range(0, 1)] public float headWeight = 0.8f;
    [Range(0, 1)] public float eyesWeight = 0.0f;
    [Range(0, 1)] public float clampWeight = 0.5f;

    void OnAnimatorIK(int layerIndex)
    {
        if (photonView.IsMine == false && PhotonNetwork.IsConnected == true) return;
        if (!animator || !lookTarget) return;
        animator.SetLookAtWeight(weight, bodyWeight, headWeight, eyesWeight, clampWeight);
        animator.SetLookAtPosition(lookTarget.position);
    }
}