using Photon.Pun;
using UnityEngine;

public class LootItem : MonoBehaviourPun
{
    [SerializeField] private int value = 100;   // How much money this gives
    [SerializeField] private string interactKeyText = "E";

    private void OnTriggerStay(Collider other)
    {
        // Only thieves can interact
        if (!other.CompareTag("Thief"))
            return;

        // Ensure we only handle input for the local player
        PhotonView playerPV = other.GetComponent<PhotonView>();
        if (playerPV == null || !playerPV.IsMine)
            return;

        // Press E to steal
        if (Input.GetKeyDown(KeyCode.E))
        {
            ThiefWallet wallet = other.GetComponent<ThiefWallet>();
            if (wallet != null)
            {
                wallet.AddMoney(value);
            }

            // Destroy item for all players
            if (photonView != null && photonView.IsMine)
            {
                PhotonNetwork.Destroy(gameObject);
            }
            else
            {
                // Fallback if not using PhotonView on loot
                Destroy(gameObject);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize trigger area a bit (optional)
        Gizmos.color = Color.yellow;
        var col = GetComponent<Collider>() as SphereCollider;
        if (col != null)
        {
            Gizmos.DrawWireSphere(transform.position + col.center, col.radius);
        }
    }
}
