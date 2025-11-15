using Photon.Pun;
using UnityEngine;

public class ThiefWallet : MonoBehaviourPun
{
    public static ThiefWallet Local;   // Reference to the local thief's wallet

    [SerializeField] private int money = 0;

    private void Awake()
    {
        // Only the local player sets this
        if (photonView.IsMine)
        {
            Local = this;
        }
    }

    public int GetMoney()
    {
        return money;
    }

    public void AddMoney(int amount)
    {
        if (!photonView.IsMine) return;  // Only local player changes their own money

        money += amount;

        // Update UI if it exists
        if (MoneyUI.Instance != null)
        {
            MoneyUI.Instance.UpdateMoney(money);
        }
    }
}
