using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
using System.Collections;

[RequireComponent(typeof(PhotonView))]
public class LaserGrid : MonoBehaviour
{
    [Tooltip("Drag all pressure plates here in order. First plates are enabled first based on player count.")]
    [SerializeField] private List<PressurePlate> requiredPlates;

    [SerializeField] private GameObject[] laserLinesObject;

    [Tooltip("How long (in seconds) it takes for the lasers to scale down")]
    [SerializeField] private float disableDuration = 1.0f;
    
    [Header("Audio")]
    [SerializeField] private AudioClip successSound;
    [SerializeField] private AudioSource audioSource;
    
    [Header("Player Scaling")]
    [Tooltip("Minimum number of plates to enable (even with 1 player)")]
    [SerializeField] private int minActivePlates = 1;
    [Tooltip("Enable one plate per X thieves")]
    [SerializeField] private int platesPerPlayer = 1;
    
    private HashSet<PressurePlate> activePlates = new HashSet<PressurePlate>();
    private PhotonView photonView;
    private bool isPuzzleSolved = false;
    private int enabledPlateCount = 0;

    void Awake()
    {
        photonView = GetComponent<PhotonView>();
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }
    
    void Start()
    {
        // Configure plates based on player count
        ConfigurePlatesForPlayerCount();
    }
    
    /// <summary>
    /// Enable/disable pressure plates based on number of thieves in session
    /// </summary>
    private void ConfigurePlatesForPlayerCount()
    {
        // Count thieves in the room
        int thiefCount = CountThievesInRoom();
        
        // Calculate how many plates to enable
        int platesToEnable = Mathf.Max(minActivePlates, thiefCount * platesPerPlayer);
        platesToEnable = Mathf.Min(platesToEnable, requiredPlates.Count); // Don't exceed available plates
        
        enabledPlateCount = platesToEnable;
        
        Debug.Log($"[LaserGrid] {thiefCount} thieves in session. Enabling {platesToEnable}/{requiredPlates.Count} plates.");
        
        // Enable/disable plates
        for (int i = 0; i < requiredPlates.Count; i++)
        {
            if (requiredPlates[i] != null)
            {
                bool shouldEnable = i < platesToEnable;
                requiredPlates[i].gameObject.SetActive(shouldEnable);
                
                if (shouldEnable)
                {
                    Debug.Log($"[LaserGrid] Plate {i + 1} ENABLED");
                }
                else
                {
                    Debug.Log($"[LaserGrid] Plate {i + 1} DISABLED");
                }
            }
        }
    }
    
    private int CountThievesInRoom()
    {
        int count = 0;
        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (player.CustomProperties.TryGetValue("Role", out object role))
            {
                if ((string)role == "Thief")
                {
                    count++;
                }
            }
        }
        
        // Fallback: if no roles set yet, count players minus 1 (assume 1 guard)
        if (count == 0 && PhotonNetwork.PlayerList.Length > 1)
        {
            count = PhotonNetwork.PlayerList.Length - 1;
        }
        else if (count == 0)
        {
            count = 1; // At least 1 for testing
        }
        
        return count;
    }

    public void PlateActivated(bool isSteppedOn, PressurePlate plate)
    {
        if (isPuzzleSolved) return;
        
        // Only count plates that are enabled
        if (!plate.gameObject.activeInHierarchy) return;

        if (isSteppedOn)
        {
            activePlates.Add(plate);
        }
        else
        {
            activePlates.Remove(plate);
        }

        CheckForSolution();
    }

    private void CheckForSolution()
    {
        // Only need to activate the enabled plates
        if (activePlates.Count >= enabledPlateCount && enabledPlateCount > 0)
        {
            Debug.Log("Puzzle Solved! Disabling lasers.");
            isPuzzleSolved = true;

            photonView.RPC("RPC_DisableLasers", RpcTarget.All);
        }
    }

    [PunRPC]
    public void RPC_DisableLasers()
    {
        // Play success sound
        PlaySuccessSound();
        
        if (laserLinesObject != null && laserLinesObject.Length > 0)
        {
            foreach (GameObject laserLine in laserLinesObject)
            {
                if (laserLine.activeInHierarchy)
                {
                    StartCoroutine(ScaleDownAndDisable(laserLine));
                }
            }
        }
    }
    
    private void PlaySuccessSound()
    {
        if (successSound == null) return;
        
        if (audioSource != null)
        {
            audioSource.PlayOneShot(successSound);
        }
        else
        {
            AudioSource.PlayClipAtPoint(successSound, transform.position);
        }
    }
    
    private IEnumerator ScaleDownAndDisable(GameObject laser)
    {
        LineRenderer lineRenderer = laser.gameObject.transform.GetChild(0).GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            laser.SetActive(false);
            yield break;
        }

        float startWidth = lineRenderer.startWidth;
        float endWidth = lineRenderer.endWidth;
        float elapsedTime = 0f;

        while (elapsedTime < disableDuration)
        {
            float newWidth = Mathf.Lerp(startWidth, 0f, elapsedTime / disableDuration);
        
            lineRenderer.startWidth = newWidth;
            lineRenderer.endWidth = newWidth;

            elapsedTime += Time.deltaTime;
            yield return null; 
        }

        lineRenderer.startWidth = 0f;
        lineRenderer.endWidth = 0f;
        laser.SetActive(false);

        lineRenderer.startWidth = startWidth;
        lineRenderer.endWidth = endWidth;
    }
}

