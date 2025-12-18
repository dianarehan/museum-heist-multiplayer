using UnityEngine;
using Photon.Pun;

public class PressurePlate : MonoBehaviourPun
{
    [SerializeField] private LaserGrid mainLaserGrid;
    [SerializeField] private Material activeMaterial;
    
    private Material defaultMaterial;
    private Renderer plateRenderer;

    void Start()
    {
        plateRenderer = GetComponent<Renderer>();
        defaultMaterial = plateRenderer.material;
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
        if (isActivated)
        {
            plateRenderer.material = activeMaterial;
            
            if (mainLaserGrid != null)
            {
                mainLaserGrid.PlateActivated(true, this);
            }
        }
        else
        {
            plateRenderer.material = defaultMaterial;
            
            if (mainLaserGrid != null)
            {
                mainLaserGrid.PlateActivated(false, this);
            }
        }
    }
}