using Photon.Pun;
using UnityEngine;
using TMPro;

public class ThiefStealRaycast : MonoBehaviourPun
{
    [Header("Raycast Settings")]
    [SerializeField] private Camera thiefCamera;
    [SerializeField] private float stealRange = 3f;
    [SerializeField] private LayerMask stealableLayers;

    [Header("Refs")]
    [SerializeField] private ThiefWallet wallet;

    [Header("UI / Feedback")]
    [SerializeField] private GameObject stealPrompt;
    [SerializeField] private float displayDuration = 2f;
    
    [Header("Prompt Messages")]
    [SerializeField] private string stealMessage = "Press E to Steal";
    [SerializeField] private string protectedMessage = "Protected";

    private StealableItem currentTarget;
    private TMP_Text promptText;
    private float hideTimer = 0f;

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
        
        // Auto-find TMP_Text in the prompt object
        if (stealPrompt != null)
        {
            promptText = stealPrompt.GetComponentInChildren<TMP_Text>();
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
        
        // Auto-hide timer
        if (hideTimer > 0)
        {
            hideTimer -= Time.deltaTime;
            if (hideTimer <= 0 && stealPrompt != null)
            {
                stealPrompt.SetActive(false);
            }
        }
    }

    private void UpdateTarget()
    {
        currentTarget = null;

        Ray ray = new Ray(thiefCamera.transform.position, thiefCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, stealRange, stealableLayers))
        {
            StealableItem item = hit.collider.GetComponent<StealableItem>();

            if (item != null)
            {
                ShowPrompt();
                
                if (item.CanSteal)
                {
                    currentTarget = item;
                    SetPromptText($"{stealMessage} (${item.Value})");
                }
                else if (item.IsProtected)
                {
                    SetPromptText(protectedMessage);
                }
                return;
            }
        }
    }
    
    private void ShowPrompt()
    {
        if (stealPrompt != null)
        {
            stealPrompt.SetActive(true);
            hideTimer = displayDuration;
        }
    }
    
    private void SetPromptText(string text)
    {
        if (promptText != null)
        {
            promptText.text = text;
        }
    }

    private void HandleInput()
    {
        if (currentTarget == null) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            int value = currentTarget.Value;

            if (wallet != null)
            {
                wallet.AddMoney(value);
            }

            currentTarget.Steal();
            Debug.Log("Stole item worth: " + value);
            GameState.Instance.ThiefCollectedLoot(value);

            if (stealPrompt != null) stealPrompt.SetActive(false);
            currentTarget = null;
            hideTimer = 0f;
        }
    }
}

