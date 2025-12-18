using Photon.Pun;
using UnityEngine;

public class ThiefStealRaycast : MonoBehaviourPun
{
    [Header("Raycast Settings")]
    [SerializeField] private Camera thiefCamera;
    [SerializeField] private float stealRange = 3f;
    [SerializeField] private LayerMask stealableLayers;

    [Header("Refs")]
    [SerializeField] private ThiefWallet wallet;

    [Header("UI / Feedback")]
    [SerializeField] private GameObject stealPrompt;  // e.g. "Press E to steal"

    private StealableItem currentTarget;

    private void Start()
    {
        if (thiefCamera == null)
        {
            thiefCamera = Camera.main;
        }

        // Only local thief should run this
        if (!photonView.IsMine)
        {
            enabled = false;
            if (stealPrompt != null) stealPrompt.SetActive(false);
            return;
        }

        if (thiefCamera == null)
        {
            thiefCamera = Camera.main;
        }
    }

    private void Update()
    {

        if (!photonView.IsMine) return;
        

        
        Debug.DrawRay(
            thiefCamera.transform.position,
            thiefCamera.transform.forward * stealRange,
            Color.green
        );
        if (thiefCamera == null)
        {
            thiefCamera = Camera.main;
        }
        if (thiefCamera == null) return;

        UpdateTarget();
        HandleInput();
    }

    private void UpdateTarget()
    {
        currentTarget = null;

        Ray ray = new Ray(thiefCamera.transform.position, thiefCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, stealRange, stealableLayers))
        {
            StealableItem item = hit.collider.GetComponent<StealableItem>();

            // Only set as target if item exists and can be stolen (not protected)
            if (item != null && item.CanSteal)
            {
                currentTarget = item;
                if (stealPrompt != null) stealPrompt.SetActive(true);
                return;
            }
        }

        if (stealPrompt != null) stealPrompt.SetActive(false);
    }

    private void HandleInput()
    {
        if (currentTarget == null) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            int value = currentTarget.Value;

            // Update local wallet
            if (wallet != null)
            {
                wallet.AddMoney(value);
            }

            // Make item disappear for everyone
            currentTarget.Steal();
            Debug.Log("Stole item worth: " + value);
            GameState.Instance.ThiefCollectedLoot(value);


            // Clear prompt
            if (stealPrompt != null) stealPrompt.SetActive(false);
            currentTarget = null;
        }
    }
}
