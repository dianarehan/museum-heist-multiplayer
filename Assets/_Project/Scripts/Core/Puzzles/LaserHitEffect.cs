using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Photon.Pun;

public class LaserHitEffect : MonoBehaviour
{
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
    private bool playerIsInside = false;
    private Coroutine fadeCoroutine;
    
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
    
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Thief")) return;
        
        PhotonView pv = other.GetComponent<PhotonView>();
        if (pv == null || !pv.IsMine) return;
        
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;
        
        // Stop any fade out in progress
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }
        
        affectedPlayer = player;
        playerIsInside = true;
        
        // Play laser hit sound
        PlayHitSound();
        
        ApplyFullEffect();
    }
    
    private void OnTriggerStay(Collider other)
    {
        // Keep effect active while player is inside
        if (playerIsInside && affectedPlayer != null && other.CompareTag("Thief"))
        {
            PhotonView pv = other.GetComponent<PhotonView>();
            if (pv != null && pv.IsMine)
            {
                ApplyFullEffect();
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Thief")) return;
        
        PhotonView pv = other.GetComponent<PhotonView>();
        if (pv == null || !pv.IsMine) return;
        
        playerIsInside = false;
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
            // Stop fading if player re-entered
            if (playerIsInside)
            {
                yield break;
            }
            
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
