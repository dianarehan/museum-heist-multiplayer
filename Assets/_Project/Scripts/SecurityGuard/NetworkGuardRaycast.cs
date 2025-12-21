using UnityEngine;
using Photon.Pun;
using System.Collections;

namespace _Project.Scripts.Player
{
    public class NetworkGuardRaycast : MonoBehaviourPunCallbacks
    {
        [Header("Raycast Settings")]
        [SerializeField] private float raycastRange = 10f;
        [SerializeField] private LayerMask targetLayers;
        
        [Header("Tase Audio")]
        [SerializeField] private AudioClip taseSound;
        [SerializeField] [Range(0f, 1f)] private float taseVolume = 1f;
        [SerializeField] private float taseDuration = 1f;
        private AudioSource taseAudioSource;

        [Header("Pistol Charge")]
        [SerializeField] private int maxCharges = 6;
        [SerializeField] private int currentCharges;
        [SerializeField] private float rechargeTime = 5f;

        [SerializeField] private GameObject pistolVisual; // mesh in hand

        [SerializeField] private float interactionRange = 10f;
        [SerializeField] private LayerMask chargerLayer;

        public float RechargeTime => rechargeTime;


        [Header("Inhaler Interaction")]
        [SerializeField] private float interactRange = 10f;
        [SerializeField] private LayerMask inhalerLayer;
        

        private GuardTirednessEffect tirednessEffect;




        private Camera playerCamera;
        private PhotonView photonView;

        private void Start()
        {
            playerCamera = GetComponentInChildren<Camera>();
            photonView = GetComponent<PhotonView>();
            
            // Create dedicated AudioSource for tase sound
            taseAudioSource = gameObject.AddComponent<AudioSource>();
            taseAudioSource.playOnAwake = false;
            taseAudioSource.spatialBlend = 1f; // 3D sound - distance affects volume
            taseAudioSource.rolloffMode = AudioRolloffMode.Linear;
            taseAudioSource.minDistance = 20f;
            taseAudioSource.maxDistance = 100f;

            currentCharges = maxCharges;
            pistolVisual.SetActive(true);

            tirednessEffect = GetComponent<GuardTirednessEffect>();
            
            // Note: tiredness now starts after a delay, not immediately



            if (playerCamera == null)
            {
                Debug.LogError("Camera not found on Guard!");
            }
        }

        private void Update()
        {
            if (photonView != null && !photonView.IsMine)
                return;

            if (Input.GetMouseButtonDown(0) && currentCharges > 0)
            {
                PerformRaycast();
                currentCharges--;
                Debug.Log($"Charges left: {currentCharges}");

            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                TryPlacePistolOnCharger();
                CheckInhalerInteraction();
            }
        }

        private void CheckInhalerInteraction()
        {
            if (!tirednessEffect || !tirednessEffect.IsTired())
                return;

            Ray ray = playerCamera.ScreenPointToRay(
                new Vector3(Screen.width / 2, Screen.height / 2)
            );

            if (Physics.Raycast(ray, out RaycastHit hit, interactRange, inhalerLayer))
            {
                Debug.Log("Inhaler in range");

                
               InhalerInteractable inhaler = hit.collider.GetComponent<InhalerInteractable>();

               if (inhaler != null)
               {
                    inhaler.Use(tirednessEffect);
               }
                
            }
        }


        private void TryPlacePistolOnCharger()
        {
            if (currentCharges == maxCharges) return;

            Ray ray = playerCamera.ScreenPointToRay(
                new Vector3(Screen.width / 2, Screen.height / 2, 0));

            if (Physics.Raycast(ray, out RaycastHit hit, interactionRange, chargerLayer))
            {
                var charger = hit.collider.GetComponent<PistolChargingStation>();
                if (charger != null)
                {
                    charger.StartCharging(this);
                }
            }
        }

        public void OnPistolPlacedOnCharger()
        {
            pistolVisual.SetActive(false);
        }

        public void OnPistolFullyCharged()
        {
            currentCharges = maxCharges;
            pistolVisual.SetActive(true);
        }



        private void PerformRaycast()
        {
            if (playerCamera == null)
                return;
            StartCoroutine(PlayTaseSound());

            Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, raycastRange, targetLayers))
            {
                Debug.Log($"Hit object: {hit.collider.gameObject.name}");

                if (hit.collider.CompareTag("Thief"))
                {
                    Debug.Log("THIEF DETECTED! Action triggered!");
                    OnThiefDetected(hit.collider.gameObject);
                }
            }
            else
            {
                Debug.Log("Raycast hit nothing");
            }
        }

        private void OnThiefDetected(GameObject thief)
        {
            Debug.Log($"Caught thief: {thief.name}");
            
            PhotonView thiefPhotonView = thief.GetComponent<PhotonView>();
            if (thiefPhotonView != null)
            {
                thiefPhotonView.RPC("RPC_KnockoutThief", RpcTarget.AllBuffered);
                
                if (PhotonNetwork.IsMasterClient)
                {
                    GameState.Instance.ThiefDied();
                }
                else
                {
                    PhotonView gameStateView = GameState.Instance.GetComponent<PhotonView>();
                    if (gameStateView != null)
                    {
                        gameStateView.RPC("RPC_ThiefCaughtRequest", RpcTarget.MasterClient);
                    }
                }
            }
        }
        
        private IEnumerator PlayTaseSound()
        {
            if (taseSound == null || taseAudioSource == null) yield break;
            
            taseAudioSource.clip = taseSound;
            taseAudioSource.volume = taseVolume;
            taseAudioSource.loop = true;
            taseAudioSource.Play();
            
            yield return new WaitForSeconds(taseDuration);
            
            taseAudioSource.Stop();
            taseAudioSource.loop = false;
        }
    }
}
