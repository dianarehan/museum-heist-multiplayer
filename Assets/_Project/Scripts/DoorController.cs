using UnityEngine;
using Photon.Pun;

public class DoorController : MonoBehaviourPun
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactionRange = 32f;
    
    private bool isOpen = false;
    private Animator anim;
    
    void Start()
    {
        anim = GetComponent<Animator>();
        if(anim == null)
        {
            Debug.LogError("Animator not found on door controller");
        }
    }

    private void OnMouseDown()
    {
        // Find the local player
        GameObject localPlayer = GetLocalPlayer();
        if (localPlayer == null)
        {
            Debug.LogWarning("Local player not found");
            return;
        }
        
        // Check if player is within interaction range
        float distance = Vector3.Distance(localPlayer.transform.position, transform.position);
        if (distance > interactionRange)
        {
            Debug.Log($"Too far from door. Distance: {distance:F1}, Required: {interactionRange}");
            return;
        }
        
        // Call RPC to toggle door state on all clients
        photonView.RPC("RPC_ToggleDoor", RpcTarget.All);
    }
    
    private GameObject GetLocalPlayer()
    {
        // Find the local player by checking PhotonView ownership
        foreach (var player in FindObjectsOfType<PhotonView>())
        {
            if (player.IsMine && player.CompareTag("Thief"))
            {
                return player.gameObject;
            }
        }
        return null;
    }

    [PunRPC]
    private void RPC_ToggleDoor()
    {
        if (isOpen)
        {
            CloseDoor();
        }
        else
        {
            OpenDoor();
        }
    }

    private void OpenDoor()
    {
        anim.SetBool("opening", true);
        anim.SetBool("closing", false);
        anim.SetBool("closed", false);
        anim.SetBool("open", true);
        isOpen = true;
    }

    private void CloseDoor()
    {
        anim.SetBool("closing", true);
        anim.SetBool("opening", false);
        anim.SetBool("open", false);
        anim.SetBool("closed", true);
        isOpen = false;
    }
}
