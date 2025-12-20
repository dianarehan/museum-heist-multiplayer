using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Shows temporary notifications on screen.
/// Assign your existing TMP_Text in Inspector.
/// </summary>
public class NotificationHUD : MonoBehaviour
{
    public static NotificationHUD Instance { get; private set; }
    
    [SerializeField] private TMP_Text notificationText;
    [SerializeField] private float displayDuration = 3f;
    
    void Awake()
    {
        Instance = this;
        
        // Start hidden
        if (notificationText != null)
        {
            notificationText.text = "";
        }
    }
    
    /// <summary>
    /// Show a notification message
    /// </summary>
    public static void Show(string message)
    {
        if (Instance != null && Instance.notificationText != null)
        {
            Instance.StopAllCoroutines();
            Instance.StartCoroutine(Instance.DisplayNotification(message));
        }
        else
        {
            Debug.Log($"[Notification] {message}");
        }
    }
    
    private IEnumerator DisplayNotification(string message)
    {
        notificationText.text = message;
        yield return new WaitForSeconds(displayDuration);
        notificationText.text = "";
    }
}

