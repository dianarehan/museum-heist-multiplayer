using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Photon.Pun;
using System.Collections;

/// <summary>
/// Applies a tiredness effect (pulsating lens distortion) to the guard.
/// Call TriggerTiredness() to start the effect.
/// Call StopTiredness() to stop it.
/// </summary>
public class GuardTirednessEffect : MonoBehaviourPun
{
    [Header("Effect Settings")]
    [SerializeField] private float distortionIntensity = -0.4f; // Negative = barrel, Positive = pincushion
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseRange = 0.15f;
    
    [Header("References")]
    [SerializeField] private Volume postProcessVolume;
    
    private LensDistortion lensDistortion;
    private float originalIntensity = 0f;
    private bool isTired = false;
    
    void Start()
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
        {
            enabled = false;
            return;
        }
        
        if (postProcessVolume == null)
        {
            postProcessVolume = FindObjectOfType<Volume>();
        }
        
        if (postProcessVolume != null && postProcessVolume.profile != null)
        {
            postProcessVolume.profile.TryGet(out lensDistortion);
            
            if (lensDistortion != null)
            {
                originalIntensity = lensDistortion.intensity.value;
            }
        }
        Invoke("TriggerTiredness", 3f); 
    }
    
    void Update()
    {
        if (!photonView.IsMine) return;
        
        if (isTired && lensDistortion != null)
        {
            // Pulsating distortion - wobbles between values
            float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseRange;
            lensDistortion.intensity.value = distortionIntensity + pulse;
        }
    }
    
    public void TriggerTiredness()
    {
        if (!photonView.IsMine) return;
        
        isTired = true;
        Debug.Log("Guard is tired");
        if (lensDistortion != null)
        {
            lensDistortion.intensity.overrideState = true;
            lensDistortion.intensity.value = distortionIntensity;
        }
    }
    
    public void StopTiredness()
    {
        if (!photonView.IsMine) return;
        
        StartCoroutine(FadeOutEffect());
    }
    
    private IEnumerator FadeOutEffect()
    {
        float fadeDuration = 1f;
        float elapsed = 0f;
        float startIntensity = lensDistortion != null ? lensDistortion.intensity.value : 0f;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            
            if (lensDistortion != null)
            {
                lensDistortion.intensity.value = Mathf.Lerp(startIntensity, originalIntensity, t);
            }
            
            yield return null;
        }
        
        if (lensDistortion != null)
        {
            lensDistortion.intensity.value = originalIntensity;
        }
        
        isTired = false;
    }
    
    public bool IsTired()
    {
        return isTired;
    }
}

