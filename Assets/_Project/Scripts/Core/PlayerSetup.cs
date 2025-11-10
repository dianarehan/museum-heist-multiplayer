using UnityEngine;
using TMPro;

public class PlayerSetup : MonoBehaviour
{

    [SerializeField] private GameObject playerModel;
    
    [SerializeField] private TMP_Text nameDisplay;

    void Start()
    {
        if (PlayerData.PlayerMaterial != null && playerModel != null)
        {
            Renderer[] renderers = playerModel.GetComponentsInChildren<Renderer>();
            
            foreach (Renderer renderer in renderers)
            {
                renderer.material = PlayerData.PlayerMaterial;
            }
        }
        else
        {
            Debug.LogWarning("Player material or model not set from PlayerData.");
        }

        this.gameObject.name = PlayerData.PlayerName;

        if (nameDisplay != null)
        {
            nameDisplay.text = PlayerData.PlayerName;
        }

        Debug.Log($"Player '{PlayerData.PlayerName}' initialized with material '{PlayerData.PlayerMaterial.name}'.");
    }
}