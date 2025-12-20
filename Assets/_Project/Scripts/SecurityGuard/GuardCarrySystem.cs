using UnityEngine;

public class GuardCarrySystem : MonoBehaviour
{
    [SerializeField] private Transform carryPosition;
    
    void Start()
    {
        // Auto-find carry position if not set
        if (carryPosition == null)
        {
            Transform found = transform.Find("CarryPosition");
            if (found != null)
            {
                carryPosition = found;
            }
            else
            {
                // Create carry position if it doesn't exist
                GameObject carryPosObj = new GameObject("CarryPosition");
                carryPosObj.transform.SetParent(transform);
                carryPosObj.transform.localPosition = new Vector3(0.7f, 0f, 0.5f);
                carryPosition = carryPosObj.transform;
            }
        }
    }
    
    public Transform GetCarryPosition()
    {
        return carryPosition;
    }
}