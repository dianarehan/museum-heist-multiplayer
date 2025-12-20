using Photon.Pun;
using Photon.Voice.Unity;
using Photon.Voice.PUN;
using UnityEngine;
using prefabs.SecurityGuard.scripts;

public class GuardHeadpiece : MonoBehaviourPun
{
    [Header("Input")]
    [SerializeField] private KeyCode listenKey = KeyCode.H;
    
    [Header("Movement Detection")]
    [SerializeField] private float movementThreshold = 0.5f;
    
    [Header("Visual Feedback")]
    [SerializeField] private GameObject headpieceIndicator;
    
    [Header("Audio Feedback")]
    [SerializeField] private AudioClip activateSound;
    [SerializeField] private AudioClip deactivateSound;
    
    [Header("Interest Group (Thieves transmit on this group)")]
    [SerializeField] private byte thiefVoiceGroup = 1;
    
    private Rigidbody rb;
    private CharacterController cc;
    private AudioSource audioSource;
    private FirstPersonMovement movement;
    private Vector3 lastPosition;
    private bool wasListening = false;
    
    public bool IsListening { get; private set; }
    
    private void Start()
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
        {
            enabled = false;
            return;
        }
        
        rb = GetComponent<Rigidbody>();
        cc = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();
        movement = GetComponent<FirstPersonMovement>();
        lastPosition = transform.position;
        
        if (PunVoiceClient.Instance != null)
        {
            PunVoiceClient.Instance.Client.OpChangeGroups(new byte[] { thiefVoiceGroup }, null);
            Debug.Log("[GuardHeadpiece] Removed from thief voice group - cannot hear thieves");
        }
        
        if (headpieceIndicator != null)
        {
            headpieceIndicator.SetActive(false);
        }
    }
    
    private void Update()
    {
        if (!photonView.IsMine) return;
        
        bool holdingKey = Input.GetKey(listenKey);
        bool isStationary = IsStationary();
        
        IsListening = holdingKey && isStationary;
        
        if (IsListening && !wasListening)
        {
            OnStartListening();
        }
        else if (!IsListening && wasListening)
        {
            OnStopListening();
        }
        
        wasListening = IsListening;
        lastPosition = transform.position;
    }
    
    private bool IsStationary()
    {
        // Use FirstPersonMovement if available
        if (movement != null)
        {
            return !movement.IsMoving();
        }
        
        // Fallback: check position delta
        float speed = Vector3.Distance(transform.position, lastPosition) / Time.deltaTime;
        return speed < movementThreshold;
    }
    
    private void OnStartListening()
    {
        Debug.Log("[GuardHeadpiece] Started listening - subscribing to thief voice group");
        
        // Subscribe to thief voice group
        if (PunVoiceClient.Instance != null)
        {
            PunVoiceClient.Instance.Client.OpChangeGroups(null, new byte[] { thiefVoiceGroup });
        }
        
        if (headpieceIndicator != null)
        {
            headpieceIndicator.SetActive(true);
        }
        
        if (activateSound != null)
        {
            PlaySound(activateSound);
        }
    }
    
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null)
        {
            audioSource.PlayOneShot(clip, 1f);
        }
        else
        {
            AudioSource.PlayClipAtPoint(clip, transform.position, 1f);
        }
    }
    
    private void OnStopListening()
    {
        Debug.Log("[GuardHeadpiece] Stopped listening - unsubscribing from thief voice group");
        
        // Unsubscribe from thief voice group
        if (PunVoiceClient.Instance != null)
        {
            PunVoiceClient.Instance.Client.OpChangeGroups(new byte[] { thiefVoiceGroup }, null);
        }
        
        if (headpieceIndicator != null)
        {
            headpieceIndicator.SetActive(false);
        }
        
        if (deactivateSound != null)
        {
            PlaySound(deactivateSound);
        }
    }
    
    private void OnGUI()
    {
        if (photonView.IsMine)
        {
            string status = IsListening ? "<color=green>LISTENING</color>" : 
                           (Input.GetKey(listenKey) ? "<color=yellow>HOLD STILL TO LISTEN</color>" : 
                           "<color=red>NOT LISTENING (Hold H)</color>");
            
            GUIStyle style = new GUIStyle();
            style.richText = true;
            style.fontSize = 30;
            
            GUI.Label(new Rect(10, 50, 300, 30), "Headpiece: " + status, style);
        }
    }
}
