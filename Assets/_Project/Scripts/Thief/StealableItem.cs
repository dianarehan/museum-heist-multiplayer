using Photon.Pun;
using UnityEngine;

public class StealableItem : MonoBehaviourPun
{
    [SerializeField] private int value = 100;

    private bool isStolen = false;

    public int Value => value;

    [PunRPC]
    private void RPC_OnStolen()
    {
        if (isStolen) return;
        isStolen = true;
        // You can play VFX / sound here instead of just disabling
        gameObject.SetActive(false);
    }

    public void Steal()
    {
        if (isStolen) return;

        if (photonView != null)
        {
            // Make item disappear for all players
            photonView.RPC(nameof(RPC_OnStolen), RpcTarget.AllBuffered);
        }
        else
        {
            // Non-networked fallback
            RPC_OnStolen();
        }
    }
}
