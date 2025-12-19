using Photon.Pun;
using Photon.Voice.Unity;
using UnityEngine;

/// <summary>
/// Sets up thief voice to transmit on Interest Group 1.
/// Guard must subscribe to Group 1 to hear thieves.
/// </summary>
public class ThiefVoiceSetup : MonoBehaviourPun
{
    [Header("Voice Group")]
    [SerializeField] private byte voiceGroup = 1;
    
    private void Start()
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
        {
            enabled = false;
            return;
        }
        
        // Set recorder to transmit on Group 1
        Recorder recorder = GetComponent<Recorder>();
        if (recorder != null)
        {
            recorder.InterestGroup = voiceGroup;
            Debug.Log($"[ThiefVoiceSetup] Transmitting on voice group {voiceGroup}");
        }
        else
        {
            Debug.LogWarning("[ThiefVoiceSetup] No Recorder component found!");
        }
    }
}
