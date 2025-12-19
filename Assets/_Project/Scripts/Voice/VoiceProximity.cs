using Photon.Pun;
using Photon.Voice.Unity;
using UnityEngine;

public class VoiceProximity : MonoBehaviourPun
{
    [Header("Proximity Settings")]
    [SerializeField] private float maxDistance = 30f; 
    [SerializeField] private float minDistance = 2f;  
    
    [Header("Testing")]
    [SerializeField] private bool disableEarpieceLogic = true;
    
    [Header("References")]
    [SerializeField] private Speaker speaker; 
    
    private AudioSource speakerAudioSource;
    private Transform localPlayerTransform;
    private bool isGuard = false;
    private GuardHeadpiece guardHeadpiece;
    
    private void Start()
    {
        // Get the Speaker's AudioSource
        if (speaker == null)
        {
            speaker = GetComponent<Speaker>();
        }
        
        if (speaker != null)
        {
            speakerAudioSource = speaker.GetComponent<AudioSource>();
        }
        
        FindLocalPlayer();
    }
    
    private void FindLocalPlayer()
    {
        foreach (var pv in FindObjectsOfType<PhotonView>())
        {
            if (pv.IsMine)
            {
                if (pv.CompareTag("Thief") || pv.CompareTag("Guard"))
                {
                    localPlayerTransform = pv.transform;
                    isGuard = pv.CompareTag("Guard");
                    
                    if (isGuard)
                    {
                        guardHeadpiece = pv.GetComponent<GuardHeadpiece>();
                    }
                    break;
                }
            }
        }
    }
    
    private void Update()
    {
        if (speakerAudioSource == null)
        {
            Debug.LogWarning($"[VoiceProximity] {gameObject.name}: No speaker AudioSource found!");
            return;
        }
        
        if (localPlayerTransform == null)
        {
            FindLocalPlayer(); 
            if (localPlayerTransform == null)
            {
                Debug.LogWarning("[VoiceProximity] No local player found!");
                return;
            }
        }
        
        // Don't adjust volume for our own speaker
        if (photonView.IsMine) return;
        
        // Calculate distance to local player
        float distance = Vector3.Distance(transform.position, localPlayerTransform.position);
        
        // Check if local player can hear this voice
        bool canHear = CanLocalPlayerHear();
        
        if (!canHear)
        {
            // Mute if can't hear
            speakerAudioSource.volume = 0f;
            return;
        }
        
        // Calculate volume based on distance (inverse linear falloff)
        float volume = 0f;
        
        if (distance <= minDistance)
        {
            volume = 1f;
        }
        else if (distance <= maxDistance)
        {
            // Linear falloff from 1 to 0
            float t = (distance - minDistance) / (maxDistance - minDistance);
            volume = 1f - t;
        }
        // else volume stays 0 (out of range)
        
        speakerAudioSource.volume = volume;
    }
    
    private bool CanLocalPlayerHear()
    {
        // Testing mode: everyone can hear everyone
        if (disableEarpieceLogic)
        {
            return true;
        }
        
        // If this voice belongs to a Thief
        bool speakerIsThief = CompareTag("Thief");
        
        // If local player is Thief - can always hear other Thieves in range
        if (!isGuard && speakerIsThief)
        {
            return true;
        }
        
        // If local player is Guard - can only hear if using headpiece
        if (isGuard && speakerIsThief)
        {
            if (guardHeadpiece != null)
            {
                Debug.Log($"[VoiceProximity] Guard headpiece IsListening: {guardHeadpiece.IsListening}");
                return guardHeadpiece.IsListening;
            }
            else
            {
                Debug.LogWarning("[VoiceProximity] Guard has no GuardHeadpiece component!");
            }
            return false;
        }
        
        // Guard-to-Guard or other cases - allow normal hearing
        return true;
    }
}
