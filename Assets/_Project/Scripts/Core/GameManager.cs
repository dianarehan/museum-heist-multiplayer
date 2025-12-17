using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [SerializeField] private string guardPrefabName = "NetworkedGuard";
    [SerializeField] private string thiefPrefabName = "NetworkedThief";

    [Header("Spawn Points")]
    [SerializeField] private Transform guardSpawnPoint;
    [SerializeField] private List<Transform> thiefSpawnPoints;

    void Start()
    {
        DebugNetworkBootstrapper debugBootstrapper = FindObjectOfType<DebugNetworkBootstrapper>();
        if (debugBootstrapper != null && debugBootstrapper.enabled)
        {
            Debug.Log("[GameManager] Debug bootstrapper detected, skipping standard spawn.");
            return;
        }

        string myRole = "Guard";
        object roleValue;
        if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("Role", out roleValue))
        {
            myRole = (string)roleValue;
        }
        else
        {
            Debug.LogWarning("[GameManager] No Role set in player properties, defaulting to Guard");
        }

        if (myRole == "Guard")
        {
            if (guardSpawnPoint != null)
            {
                PhotonNetwork.Instantiate(guardPrefabName, guardSpawnPoint.position, guardSpawnPoint.rotation);
            }
            else
            {
                PhotonNetwork.Instantiate(guardPrefabName, Vector3.zero, Quaternion.identity);
                Debug.LogWarning("[GameManager] No guard spawn point set, spawning at origin");
            }
        }
        else
        {
            int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;

            int spawnIndex = actorNumber - 2;
            int wrappedIndex = spawnIndex % Mathf.Max(1, thiefSpawnPoints.Count);

            Transform spawnPoint = wrappedIndex < thiefSpawnPoints.Count ? thiefSpawnPoints[wrappedIndex] : null;

            if (spawnPoint != null)
            {
                PhotonNetwork.Instantiate(thiefPrefabName, spawnPoint.position, spawnPoint.rotation);
            }
            else
            {
                PhotonNetwork.Instantiate(thiefPrefabName, Vector3.zero, Quaternion.identity);
                Debug.LogWarning("[GameManager] No thief spawn point available, spawning at origin");
            }
        }
    }
}