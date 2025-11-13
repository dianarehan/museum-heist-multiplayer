using UnityEngine;
using Photon.Pun;

public class PressurePlate : MonoBehaviour
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
        if (other.CompareTag("Player"))
        {
            plateRenderer.material = activeMaterial;
            
            if (mainLaserGrid != null)
            {
                mainLaserGrid.PlateActivated(true, this);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            plateRenderer.material = defaultMaterial;
            
            if (mainLaserGrid != null)
            {
                mainLaserGrid.PlateActivated(false, this);
            }
        }
    }
}