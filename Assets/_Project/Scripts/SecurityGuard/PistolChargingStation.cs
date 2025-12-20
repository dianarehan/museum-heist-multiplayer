using UnityEngine;
using Photon.Pun;
using System.Collections;
using _Project.Scripts.Player;

public class PistolChargingStation : MonoBehaviourPun
{
    private bool isOccupied = false;
    [SerializeField] private GameObject pistolVisual;

    [SerializeField] private AudioClip placeOnChargerSfx;
    [SerializeField] private AudioClip pickupFromChargerSfx;

    private AudioSource audioSource;


    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void StartCharging(NetworkGuardRaycast guard)
    {
        if (isOccupied) return;

        isOccupied = true;
        if (audioSource != null && placeOnChargerSfx != null)
            audioSource.PlayOneShot(placeOnChargerSfx);
        StartCoroutine(ChargeRoutine(guard));
    }

    private IEnumerator ChargeRoutine(NetworkGuardRaycast guard)
    {
        guard.OnPistolPlacedOnCharger();
        OnPistolPlacedOnCharger();

        yield return new WaitForSeconds(guard.RechargeTime);

        guard.OnPistolFullyCharged();
        OnPistolFullyCharged();
        if (audioSource != null && pickupFromChargerSfx != null)
            audioSource.PlayOneShot(pickupFromChargerSfx);

        isOccupied = false;
    }
    public void OnPistolPlacedOnCharger()
    {
        pistolVisual.SetActive(true);
    }
    public void OnPistolFullyCharged()
    {
        pistolVisual.SetActive(false);
    }

}
