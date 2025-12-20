using UnityEngine;
using Photon.Pun;
using System;

public class GuardPistolState : MonoBehaviourPun
{
    public int MaxCharges => maxCharges;
    public int CurrentCharges => currentCharges;
    public bool IsCharging => isCharging;
    public float ChargeProgress => chargeProgress;

    [SerializeField] private int maxCharges = 6;

    private int currentCharges;
    private bool isCharging;
    private float chargeProgress;

    // Events for HUD
    public event Action OnStateChanged;

    void Start()
    {
        if (!photonView.IsMine) return;
        currentCharges = maxCharges;
        Notify();
    }

    public void ConsumeCharge()
    {
        if (currentCharges <= 0) return;
        currentCharges--;
        Notify();
    }

    public void StartCharging()
    {
        isCharging = true;
        chargeProgress = 0f;
        Notify();
    }

    public void UpdateCharging(float progress)
    {
        chargeProgress = progress;
        Notify();
    }

    public void FinishCharging()
    {
        currentCharges = maxCharges;
        isCharging = false;
        chargeProgress = 1f;
        Notify();
    }

    private void Notify()
    {
        OnStateChanged?.Invoke();
    }
}
