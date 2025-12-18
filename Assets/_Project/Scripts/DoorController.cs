using UnityEngine;
using Photon.Pun;

public class DoorController : MonoBehaviourPun
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactionRange = 3f;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;
    
    private bool isOpen = false;
    private Animator anim;
    
    void Start()
    {
        anim = GetComponent<Animator>();
        if(anim == null)
        {
            Debug.LogError("Animator not found on door controller");
        }
        
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }
    
    void Update()
    {
        // Check for E key press
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }
    }
    
    private void TryInteract()
    {
        // Find local player
        GameObject localPlayer = GetLocalPlayer();
        if (localPlayer == null) return;
        
        // Check distance
        float distance = Vector3.Distance(localPlayer.transform.position, transform.position);
        if (distance > interactionRange) return;
        
        // Check if player is looking at this door (raycast from player's camera)
        Camera playerCam = localPlayer.GetComponentInChildren<Camera>();
        if (playerCam == null) playerCam = Camera.main;
        if (playerCam == null) return;
        
        Ray ray = new Ray(playerCam.transform.position, playerCam.transform.forward);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, interactionRange))
        {
            // Check if raycast hit this door or any of its children/parents
            if (hit.transform == transform || 
                hit.transform.IsChildOf(transform) || 
                transform.IsChildOf(hit.transform))
            {
                photonView.RPC("RPC_ToggleDoor", RpcTarget.All);
            }
        }
    }
    
    private GameObject GetLocalPlayer()
    {
        foreach (var player in FindObjectsOfType<PhotonView>())
        {
            if (player.IsMine && (player.CompareTag("Thief") || player.CompareTag("Guard")))
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
        
        PlaySound(openSound);
    }

    private void CloseDoor()
    {
        anim.SetBool("closing", true);
        anim.SetBool("opening", false);
        anim.SetBool("open", false);
        anim.SetBool("closed", true);
        isOpen = false;
        
        PlaySound(closeSound);
    }
    
    private void PlaySound(AudioClip clip)
    {
        if (clip == null) return;
        
        if (audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
        else
        {
            AudioSource.PlayClipAtPoint(clip, transform.position);
        }
    }
}



