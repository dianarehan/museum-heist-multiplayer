using Photon.Pun;
using UnityEngine;
using System.Collections;

public class StealableItem : MonoBehaviourPun
{
    [SerializeField] private int value = 100;
    
    [Header("Protection")]
    [Tooltip("If true, item cannot be stolen until protection is removed (e.g., glass is shattered)")]
    [SerializeField] private bool isProtected = false;
    
    [Header("Animation")]
    [SerializeField] private float hideDelay = 0.5f; // Delay before hiding to allow animation

    private bool isStolen = false;

    public int Value => value;
    
    public bool CanSteal => !isProtected && !isStolen;
    public bool IsProtected => isProtected;

    [PunRPC]
    private void RPC_OnStolen()
    {
        if (isStolen) return;
        isStolen = true;
        StartCoroutine(HideAfterDelay());
    }
    
    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(hideDelay);
        gameObject.SetActive(false);
    }
    
    [PunRPC]
    private void RPC_UnlockProtection()
    {
        isProtected = false;
    }

    public void Steal()
    {
        if (isStolen || isProtected) return;

        if (photonView != null)
        {
            photonView.RPC(nameof(RPC_OnStolen), RpcTarget.AllBuffered);
        }
        else
        {
            RPC_OnStolen();
        }
    }
    
    public void UnlockProtection()
    {
        if (photonView != null)
        {
            photonView.RPC(nameof(RPC_UnlockProtection), RpcTarget.AllBuffered);
        }
        else
        {
            RPC_UnlockProtection();
        }
    }
}