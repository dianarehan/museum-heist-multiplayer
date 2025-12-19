using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Individual slot in the objective HUD grid.
/// Shows item icon, name, and acquired state.
/// </summary>
public class ObjectiveSlot : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private GameObject acquiredOverlay; // Checkmark or dim overlay
    [SerializeField] private Image backgroundImage;
    
    [Header("Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color acquiredColor = new Color(0.5f, 1f, 0.5f, 1f); // Green tint
    
    private StealableItem linkedItem;
    private bool isAcquired = false;
    
    public StealableItem LinkedItem => linkedItem;
    
    public void Setup(StealableItem item, Sprite icon, string itemName)
    {
        linkedItem = item;
        
        if (iconImage != null && icon != null)
        {
            iconImage.sprite = icon;
            iconImage.enabled = true;
        }
        
        if (nameText != null)
        {
            nameText.text = itemName;
        }
        
        if (acquiredOverlay != null)
        {
            acquiredOverlay.SetActive(false);
        }
        
        UpdateVisuals();
    }
    
    public void MarkAcquired()
    {
        isAcquired = true;
        UpdateVisuals();
    }
    
    private void UpdateVisuals()
    {
        if (acquiredOverlay != null)
        {
            acquiredOverlay.SetActive(isAcquired);
        }
        
        if (backgroundImage != null)
        {
            backgroundImage.color = isAcquired ? acquiredColor : normalColor;
        }
        
        // Dim the icon when acquired
        if (iconImage != null)
        {
            iconImage.color = isAcquired ? new Color(1f, 1f, 1f, 0.5f) : Color.white;
        }
    }
}
