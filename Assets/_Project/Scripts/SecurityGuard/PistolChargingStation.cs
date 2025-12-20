using UnityEngine;
using Photon.Pun;
using System.Collections;
using _Project.Scripts.Player;

public class PistolChargingStation : MonoBehaviourPun
{
    private bool isOccupied = false;
    [SerializeField] private GameObject pistolVisual;

    public void StartCharging(NetworkGuardRaycast guard)
    {
        if (isOccupied) return;

        isOccupied = true;
        StartCoroutine(ChargeRoutine(guard));
    }

    private IEnumerator ChargeRoutine(NetworkGuardRaycast guard)
    {
        guard.OnPistolPlacedOnCharger();
        OnPistolPlacedOnCharger();

        yield return new WaitForSeconds(guard.RechargeTime);

        guard.OnPistolFullyCharged();
        OnPistolFullyCharged();
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
