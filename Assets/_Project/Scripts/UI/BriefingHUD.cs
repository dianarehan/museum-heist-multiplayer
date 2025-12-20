using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using Photon.Pun;

/// <summary>
/// Shows a briefing at game start with typewriter text effect.
/// Press Space to dismiss once text is complete.
/// Add to any player prefab and set the briefing message in Inspector.
/// </summary>
public class BriefingHUD : MonoBehaviourPun
{
    [Header("UI References")]
    [SerializeField] private GameObject briefingPanel;
    [SerializeField] private TMP_Text briefingText;
    [SerializeField] private TMP_Text spaceToContinueText;
    
    [Header("Typewriter Settings")]
    [SerializeField] private float typingSpeed = 0.03f;
    [SerializeField] private float spacePromptFadeSpeed = 1.5f;
    
    [Header("Briefing Content")]
    [TextArea(5, 15)]
    [SerializeField] private string briefingMessage = @"Your briefing message here...";
    
    private bool typingComplete = false;
    private bool briefingDismissed = false;
    private Coroutine fadeCoroutine;
    
    void Start()
    {
        // Only show for local player
        if (photonView != null && !photonView.IsMine && PhotonNetwork.IsConnected)
        {
            if (briefingPanel != null) briefingPanel.SetActive(false);
            enabled = false;
            return;
        }
        
        ShowBriefing();
    }
    
    public void ShowBriefing()
    {
        if (briefingPanel != null)
        {
            briefingPanel.SetActive(true);
        }
        
        if (briefingText != null)
        {
            briefingText.text = "";
        }
        
        if (spaceToContinueText != null)
        {
            spaceToContinueText.alpha = 0f;
        }
        
        // Pause game during briefing
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        StartCoroutine(TypeBriefing());
    }
    
    private IEnumerator TypeBriefing()
    {
        typingComplete = false;
        
        foreach (char c in briefingMessage)
        {
            if (briefingText != null)
            {
                briefingText.text += c;
            }
            
            // Use unscaled time since game is paused
            yield return new WaitForSecondsRealtime(typingSpeed);
        }
        
        typingComplete = true;
        
        // Start fading in the space prompt
        if (spaceToContinueText != null)
        {
            fadeCoroutine = StartCoroutine(FadeSpacePrompt());
        }
    }
    
    private IEnumerator FadeSpacePrompt()
    {
        while (!briefingDismissed)
        {
            // Fade in and out using sin wave
            float alpha = (Mathf.Sin(Time.unscaledTime * spacePromptFadeSpeed) + 1f) / 2f;
            
            if (spaceToContinueText != null)
            {
                spaceToContinueText.alpha = alpha;
            }
            
            yield return null;
        }
    }
    
    void Update()
    {
        if (briefingDismissed) return;
        
        // Allow skipping to end of typing with space
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (typingComplete)
            {
                DismissBriefing();
            }
            else
            {
                // Skip to end of typing
                StopAllCoroutines();
                if (briefingText != null)
                {
                    briefingText.text = briefingMessage;
                }
                typingComplete = true;
                fadeCoroutine = StartCoroutine(FadeSpacePrompt());
            }
        }
    }
    
    private void DismissBriefing()
    {
        briefingDismissed = true;
        
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        
        if (briefingPanel != null)
        {
            briefingPanel.SetActive(false);
        }
        
        // Resume game
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    void OnDestroy()
    {
        // Make sure time scale is reset if object is destroyed
        Time.timeScale = 1f;
    }
}
