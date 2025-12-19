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
    [SerializeField] private float hideDelay = 0.5f;

    private bool isStolen = false;

    public int Value => value;
    
    /// <summary>
    /// Returns true if item can be stolen:
    /// - Must be an objective (in ObjectiveManager list)
    /// - Not protected
    /// - Not already stolen
    /// </summary>
    public bool CanSteal
    {
        get
        {
            // Must be an objective to be stealable
            if (ObjectiveManager.Instance != null && !ObjectiveManager.Instance.IsObjective(this))
            {
                return false;
            }
            return !isProtected && !isStolen;
        }
    }
    
    /// <summary>
    /// Returns true if this item is a valid objective
    /// </summary>
    public bool IsObjective
    {
        get
        {
            return ObjectiveManager.Instance != null && ObjectiveManager.Instance.IsObjective(this);
        }
    }
    
    public bool IsProtected => isProtected;
    public bool IsStolen => isStolen;

    [PunRPC]
    private void RPC_OnStolen()
    {
        if (isStolen) return;
        isStolen = true;
        
        // Notify ObjectiveManager
        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.MarkAcquired(this);
        }
        
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
        
        // Check if this is an objective
        if (ObjectiveManager.Instance != null && !ObjectiveManager.Instance.IsObjective(this))
        {
            Debug.Log("Cannot steal - not an objective item");
            return;
        }

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
