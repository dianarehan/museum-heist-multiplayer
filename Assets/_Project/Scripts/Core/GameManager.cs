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
        string myRole = (string)PhotonNetwork.LocalPlayer.CustomProperties["Role"];

        if (myRole == "Guard")
        {
            PhotonNetwork.Instantiate(guardPrefabName, guardSpawnPoint.position, guardSpawnPoint.rotation);
        }
        else
        {
            int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;

            int spawnIndex = actorNumber - 2;
            int wrappedIndex = spawnIndex % thiefSpawnPoints.Count;

            Transform spawnPoint = thiefSpawnPoints[wrappedIndex];

            PhotonNetwork.Instantiate(thiefPrefabName, spawnPoint.position, spawnPoint.rotation);
        }
    }
}