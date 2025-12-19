using Photon.Pun;
using Photon.Voice.Unity;
using UnityEngine;

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
    
    [Header("Voice Control")]
    [SerializeField] private Recorder voiceRecorder; // Photon Voice Recorder
    
    private Rigidbody rb;
    private CharacterController cc;
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
        lastPosition = transform.position;
        
        // Find Recorder if not assigned
        if (voiceRecorder == null)
        {
            voiceRecorder = GetComponent<Recorder>();
        }
        
        // Start with mic muted
        if (voiceRecorder != null)
        {
            voiceRecorder.TransmitEnabled = false;
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
        
        // Debug: show current transmit state
        if (voiceRecorder != null && Time.frameCount % 60 == 0) // Every ~1 second
        {
            Debug.Log($"[GuardHeadpiece] TransmitEnabled: {voiceRecorder.TransmitEnabled}, IsListening: {IsListening}");
        }
        
        wasListening = IsListening;
        
        lastPosition = transform.position;
    }
    
    private bool IsStationary()
    {
        float speed = 0f;
        
        if (rb != null)
        {
            speed = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude;
        }
        else if (cc != null)
        {
            speed = cc.velocity.magnitude;
        }
        else
        {
            speed = Vector3.Distance(transform.position, lastPosition) / Time.deltaTime;
        }
        
        bool stationary = speed < movementThreshold;
        // Debug.Log($"Speed: {speed:F2}, Threshold: {movementThreshold}, Stationary: {stationary}");
        return stationary;
    }
    
    private void OnStartListening()
    {
        Debug.Log("Guard started listening with headpiece");
        
        // Enable microphone
        if (voiceRecorder != null)
        {
            voiceRecorder.TransmitEnabled = true;
        }
        
        if (headpieceIndicator != null)
        {
            headpieceIndicator.SetActive(true);
        }
        
        if (activateSound != null)
        {
            AudioSource.PlayClipAtPoint(activateSound, transform.position);
        }
    }
    
    private void OnStopListening()
    {
        Debug.Log("Guard stopped listening");
        
        // Disable microphone
        if (voiceRecorder != null)
        {
            voiceRecorder.TransmitEnabled = false;
        }
        
        if (headpieceIndicator != null)
        {
            headpieceIndicator.SetActive(false);
        }
        
        if (deactivateSound != null)
        {
            AudioSource.PlayClipAtPoint(deactivateSound, transform.position);
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
            style.fontSize = 16;
            
            GUI.Label(new Rect(10, 50, 300, 30), "Headpiece: " + status, style);
        }
    }
}
