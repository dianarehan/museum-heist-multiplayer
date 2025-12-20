using UnityEngine;
using Photon.Pun;

public class InhalerInteractable : MonoBehaviourPun
{
    [SerializeField] private float useCooldown = 10f;
    private bool used = false;

    [SerializeField] private AudioClip inhaleSound;
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        
    }

    public void Use(GuardTirednessEffect tiredness)
    {
        if (used) return;

        used = true;
        tiredness.StopTiredness();

        Debug.Log("Inhaler used – tiredness stopped");

        if (audioSource != null && inhaleSound != null)
        {
            audioSource.PlayOneShot(inhaleSound);
            Debug.Log("Inhaler sound played");

        }
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
