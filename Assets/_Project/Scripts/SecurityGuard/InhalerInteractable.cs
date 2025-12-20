using UnityEngine;
using Photon.Pun;

public class InhalerInteractable : MonoBehaviourPun
{
    [SerializeField] private float useCooldown = 10f;
    private bool used = false;

    public void Use(GuardTirednessEffect tiredness)
    {
        if (used) return;

        used = true;
        tiredness.StopTiredness();

        Debug.Log("Inhaler used – tiredness stopped");

        // Optional: hide inhaler
        gameObject.SetActive(false);

        // Optional: respawn later
        Invoke(nameof(ResetInhaler), useCooldown);
    }

    private void ResetInhaler()
    {
        used = false;
        gameObject.SetActive(true);
    }
}
