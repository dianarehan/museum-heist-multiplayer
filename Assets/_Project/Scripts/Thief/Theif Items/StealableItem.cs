using Photon.Pun;
using UnityEngine;

public class StealableItem : MonoBehaviourPun
{
    [SerializeField] private int value = 100;
    
    [Header("Protection")]
    [Tooltip("If true, item cannot be stolen until protection is removed (e.g., glass is shattered)")]
    [SerializeField] private bool isProtected = false;

    private bool isStolen = false;

    public int Value => value;
    
    /// <summary>
    /// Returns true if the item can be stolen (not protected and not already stolen)
    /// </summary>
    public bool CanSteal => !isProtected && !isStolen;
    
    /// <summary>
    /// Returns true if this item is currently protected
    /// </summary>
    public bool IsProtected => isProtected;

    [PunRPC]
    private void RPC_OnStolen()
    {
        if (isStolen) return;
        isStolen = true;
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
    
    /// <summary>
    /// Call this when the protection is removed (e.g., glass shattered)
    /// </summary>
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