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
            
            if (playerCamera == null)
            {
                Debug.LogError("Camera not found on Guard!");
            }
        }

        private void Update()
        {
            if (photonView != null && !photonView.IsMine)
                return;
            
            if (Input.GetMouseButtonDown(0))
            {
                PerformRaycast();
            }
        }

        private void PerformRaycast()
        {
            if (playerCamera == null)
                return;
            StartCoroutine(PlayTaseSound());

            Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, raycastRange))
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
