using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
using System.Collections;
[RequireComponent(typeof(PhotonView))]
public class LaserGrid : MonoBehaviour
{
    [Tooltip("Drag all pressure plates here in the Inspector")]
    [SerializeField] private List<PressurePlate> requiredPlates;

    [SerializeField] private GameObject[] laserLinesObject;

    [Tooltip("How long (in seconds) it takes for the lasers to scale down")]
    [SerializeField] private float disableDuration = 1.0f;
    
    [Header("Audio")]
    [SerializeField] private AudioClip successSound;
    [SerializeField] private AudioSource audioSource;
    
    private HashSet<PressurePlate> activePlates = new HashSet<PressurePlate>();
    private PhotonView photonView;
    private bool isPuzzleSolved = false;

    void Awake()
    {
        photonView = GetComponent<PhotonView>();
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    public void PlateActivated(bool isSteppedOn, PressurePlate plate)
    {
        if (isPuzzleSolved) return;

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
        if (activePlates.Count == requiredPlates.Count)
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
