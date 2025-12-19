using UnityEngine;

/// <summary>
/// Attach to StealableItem to provide display data for the objective HUD.
/// </summary>
public class ObjectiveItemData : MonoBehaviour
{
    [Header("Display Info")]
    [SerializeField] private string itemName = "Item";
    [SerializeField] private Sprite icon;
    
    public string ItemName => itemName;
    public Sprite Icon => icon;
}
