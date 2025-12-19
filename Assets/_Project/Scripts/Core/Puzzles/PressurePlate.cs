using UnityEngine;
using Photon.Pun;

public class PressurePlate : MonoBehaviourPun
{
    [SerializeField] private LaserGrid mainLaserGrid;
    [SerializeField] private Material activeMaterial;
    
    [Header("Button Animation")]
    [SerializeField] private Transform buttonPart;
    [SerializeField] private float pressDepth = 0.05f;
    [SerializeField] private float pressSpeed = 5f;
    
    [Header("Audio")]
    [SerializeField] private AudioClip pressSound;
    [SerializeField] private AudioClip releaseSound;
    
    private Material defaultMaterial;
    private Renderer plateRenderer;
    private AudioSource audioSource;
    private Vector3 buttonOriginalPosition;
    private Vector3 buttonPressedPosition;
    private bool isPressed = false;

    void Start()
    {
        plateRenderer = GetComponent<Renderer>();
        defaultMaterial = plateRenderer.material;
        
        if (buttonPart != null)
        {
            buttonOriginalPosition = buttonPart.localPosition;
            buttonPressedPosition = buttonOriginalPosition - new Vector3(0, pressDepth, 0);
        }
    }
    
    void Update()
    {
        if (buttonPart != null)
        {
            Vector3 targetPos = isPressed ? buttonPressedPosition : buttonOriginalPosition;
            buttonPart.localPosition = Vector3.Lerp(buttonPart.localPosition, targetPos, Time.deltaTime * pressSpeed);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Thief"))
        {
            photonView.RPC("RPC_ActivatePlate", RpcTarget.All, true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Thief"))
        {
            photonView.RPC("RPC_ActivatePlate", RpcTarget.All, false);
        }
    }

    [PunRPC]
    private void RPC_ActivatePlate(bool isActivated)
    {
        isPressed = isActivated;
        
        if (isActivated)
        {
            plateRenderer.material = activeMaterial;
            PlaySound(pressSound);
            
            if (mainLaserGrid != null)
            {
                mainLaserGrid.PlateActivated(true, this);
            }
        }
        else
        {
            plateRenderer.material = defaultMaterial;
            PlaySound(releaseSound);
            
            if (mainLaserGrid != null)
            {
                mainLaserGrid.PlateActivated(false, this);
            }
        }
    }
    
    private void PlaySound(AudioClip clip)
    {
        if (clip == null) return;
        
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
        
        if (audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}
