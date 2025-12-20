using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Photon.Pun;

public class LaserHitEffect : MonoBehaviour
{
    [Header("Laser Detection (Linecast)")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private LayerMask detectionLayers = -1;
    [SerializeField] private float triggerCooldown = 1f;
    
    [Header("Effect Settings")]
    [SerializeField] private float fadeOutDuration = 2f;
    [SerializeField] private float slowdownMultiplier = 0.5f;
    [SerializeField] private float chromaticAberrationIntensity = 1f;
    
    [Header("Audio")]
    [SerializeField] private AudioClip laserHitSound;
    [SerializeField] private AudioSource audioSource;
    
    [Header("References")]
    [SerializeField] private Volume postProcessVolume;
    
    private ChromaticAberration chromaticAberration;
    private float originalChromaticIntensity = 0f;
    private PlayerController affectedPlayer;
    private Coroutine fadeCoroutine;
    private float lastTriggerTime = -999f;
    
    void Start()
    {
        if (postProcessVolume == null)
        {
            postProcessVolume = FindObjectOfType<Volume>();
        }
        
        if (postProcessVolume != null && postProcessVolume.profile != null)
        {
            postProcessVolume.profile.TryGet(out chromaticAberration);
            if (chromaticAberration != null)
            {
                originalChromaticIntensity = chromaticAberration.intensity.value;
            }
        }
        
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }
    
    void Update()
    {
        if (pointA == null || pointB == null) return;
        if (Time.time - lastTriggerTime < triggerCooldown) return;
        
        CheckLaserCrossing();
    }
    
    private void CheckLaserCrossing()
    {
        RaycastHit hit;
        Vector3 direction = pointB.position - pointA.position;
        float distance = direction.magnitude;
        
        if (Physics.Raycast(pointA.position, direction.normalized, out hit, distance, detectionLayers))
        {
            if (hit.collider.CompareTag("Thief"))
            {
                PhotonView pv = hit.collider.GetComponent<PhotonView>();
                if (pv == null || !pv.IsMine) return;
                
                PlayerController player = hit.collider.GetComponent<PlayerController>();
                if (player == null) return;
                
                lastTriggerTime = Time.time;
                TriggerEffect(player);
            }
        }
    }
    
    private void TriggerEffect(PlayerController player)
    {
        Debug.Log("[LaserHitEffect] Thief crossed the laser!");
        
        // Stop any fade out in progress
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }
        
        affectedPlayer = player;
        
        // Play laser hit sound
        PlayHitSound();
        
        // Apply effect
        ApplyFullEffect();
        
        // Start fade out
        fadeCoroutine = StartCoroutine(FadeOutEffect());
    }
    
    private void ApplyFullEffect()
    {
        if (affectedPlayer != null)
        {
            affectedPlayer.speedMultiplier = slowdownMultiplier;
        }
        
        if (chromaticAberration != null)
        {
            chromaticAberration.intensity.value = chromaticAberrationIntensity;
        }
    }
    
    private void PlayHitSound()
    {
        if (laserHitSound == null) return;
        
        if (audioSource != null)
        {
            audioSource.PlayOneShot(laserHitSound);
        }
        else
        {
            AudioSource.PlayClipAtPoint(laserHitSound, transform.position);
        }
    }
    
    private System.Collections.IEnumerator FadeOutEffect()
    {
        float elapsed = 0f;
        
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeOutDuration;
            
            if (affectedPlayer != null)
            {
                affectedPlayer.speedMultiplier = Mathf.Lerp(slowdownMultiplier, 1f, t);
            }
            
            if (chromaticAberration != null)
            {
                chromaticAberration.intensity.value = Mathf.Lerp(chromaticAberrationIntensity, originalChromaticIntensity, t);
            }
            
            yield return null;
        }
        
        // Fully reset
        if (affectedPlayer != null)
        {
            affectedPlayer.speedMultiplier = 1f;
        }
        if (chromaticAberration != null)
        {
            chromaticAberration.intensity.value = originalChromaticIntensity;
        }
        
        affectedPlayer = null;
        fadeCoroutine = null;
    }
}

