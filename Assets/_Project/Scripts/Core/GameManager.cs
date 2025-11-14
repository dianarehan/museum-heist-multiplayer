using UnityEngine;
using Photon.Pun;

public class GameManager : MonoBehaviour
{
    [SerializeField] private string guardPrefabName = "GuardPrefab";
    [SerializeField] private string thiefPrefabName = "ThiefPrefab";
    [SerializeField] private Transform guardSpawnPoint;
    [SerializeField] private Transform thiefSpawnPoint;

    void Start()
    {
        string myRole = (string)PhotonNetwork.LocalPlayer.CustomProperties["Role"];

        if (myRole == "Guard")
        {
            PhotonNetwork.Instantiate(guardPrefabName, guardSpawnPoint.position, guardSpawnPoint.rotation);
        }
        else
        {
            PhotonNetwork.Instantiate(thiefPrefabName, thiefSpawnPoint.position, thiefSpawnPoint.rotation);
        }
    }
}