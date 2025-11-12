using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

[RequireComponent(typeof(PhotonView))]
public class LaserGrid : MonoBehaviour
{
    [Tooltip("Drag all pressure plates here in the Inspector")]
    [SerializeField] private List<PressurePlate> requiredPlates;

    [SerializeField] private GameObject [] laserLinesObject;

    private HashSet<PressurePlate> activePlates = new HashSet<PressurePlate>();

    private PhotonView photonView;
    private bool isPuzzleSolved = false;

    void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }

    public void PlateActivated(bool isSteppedOn, PressurePlate plate)
    {
        if (isPuzzleSolved) return;

        if (isSteppedOn)
        {
            activePlates.Add(plate);
        }
        else
        {
            activePlates.Remove(plate);
        }

        CheckForSolution();
    }

    private void CheckForSolution()
    {
        if (activePlates.Count == requiredPlates.Count)
        {
            Debug.Log("Puzzle Solved! Disabling lasers.");
            isPuzzleSolved = true;

            photonView.RPC("RPC_DisableLasers", RpcTarget.All);
        }
    }

    [PunRPC]
    public void RPC_DisableLasers()
    {
        if (laserLinesObject != null)
        {
            foreach (GameObject laserLine in laserLinesObject)
            {
                laserLine.SetActive(false);
            }
        }
    }
}