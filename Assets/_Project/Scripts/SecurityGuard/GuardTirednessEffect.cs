using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Photon.Pun;
using System.Collections;

/// <summary>
/// Applies a tiredness effect (lens distortion + desaturation) to the guard.
/// Call TriggerTiredness() to start the effect.
/// Call StopTiredness() to stop it.
/// </summary>
public class GuardTirednessEffect : MonoBehaviourPun
{
    [Header("Lens Distortion")]
    [SerializeField] private float distortionIntensity = -0.4f;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseRange = 0.15f;
    
    [Header("Color Adjustments")]
    [SerializeField] private float saturationReduction = -40f;

    [Header("Panting Audio")]
    [SerializeField] private AudioClip pantingLoop;
    [SerializeField] private float pantingVolume = 0.3f;

    private AudioSource pantingSource;

    [Header("References")]
    [SerializeField] private Volume postProcessVolume;
    
    private LensDistortion lensDistortion;
    private ColorAdjustments colorAdjustments;
    
    private float originalDistortion = 0f;
    private float originalSaturation = 0f;
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
            postProcessVolume.profile.TryGet(out colorAdjustments);
            
            if (lensDistortion != null) originalDistortion = lensDistortion.intensity.value;
            if (colorAdjustments != null) originalSaturation = colorAdjustments.saturation.value;
        }
        pantingSource = gameObject.AddComponent<AudioSource>();
        pantingSource.clip = pantingLoop;
        pantingSource.loop = true;
        pantingSource.playOnAwake = false;
        pantingSource.volume = pantingVolume;
        pantingSource.spatialBlend = 0f; 
    }
    
    void Update()
    {
        if (!photonView.IsMine) return;
        
        if (isTired && lensDistortion != null)
        {
            float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseRange;
            lensDistortion.intensity.value = distortionIntensity + pulse;
        }
    }
    
    public void TriggerTiredness()
    {
        if (!photonView.IsMine) return;
        
        isTired = true;
        
        if (lensDistortion != null)
        {
            lensDistortion.intensity.overrideState = true;
            lensDistortion.intensity.value = distortionIntensity;
        }
        
        if (colorAdjustments != null)
        {
            colorAdjustments.saturation.overrideState = true;
            colorAdjustments.saturation.value = saturationReduction;
        }

        if (pantingSource != null && pantingLoop != null && !pantingSource.isPlaying)
        {
            pantingSource.Play();
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
        
        float startDistortion = lensDistortion != null ? lensDistortion.intensity.value : 0f;
        float startSaturation = colorAdjustments != null ? colorAdjustments.saturation.value : 0f;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            
            if (lensDistortion != null)
                lensDistortion.intensity.value = Mathf.Lerp(startDistortion, originalDistortion, t);
            
            if (colorAdjustments != null)
                colorAdjustments.saturation.value = Mathf.Lerp(startSaturation, originalSaturation, t);
            
            yield return null;
        }
        
        if (lensDistortion != null) lensDistortion.intensity.value = originalDistortion;
        if (colorAdjustments != null) colorAdjustments.saturation.value = originalSaturation;

        if (pantingSource != null && pantingSource.isPlaying)
        {
            pantingSource.Stop();
        }


        isTired = false;
    }
    
    public bool IsTired()
    {
        return isTired;
    }
}
