using TMPro;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class MoneyUI : MonoBehaviour
{
    public static MoneyUI Instance;

    [SerializeField] private TextMeshProUGUI moneyText;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Check local player's role
        object roleObj;
        if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("Role", out roleObj))
        {
            string role = roleObj as string;

            // If I'm NOT a thief disable this whole UI
            if (role != "Thief")
            {
                gameObject.SetActive(false);
                return;
            }
        }
        else
        {
            // No role set? safest: hide
            gameObject.SetActive(false);
            return;
        }

        // If we reach here, we ARE a thief
        UpdateMoney(ThiefWallet.Local != null ? ThiefWallet.Local.GetMoney() : 0);
    }

    public void UpdateMoney(int newValue)
    {
        if (moneyText != null)
        {
            moneyText.text = $"Money: {newValue}";
        }
    }
}
