using UnityEngine;
using TMPro;
using Photon.Pun;
using System.Collections.Generic;

[RequireComponent(typeof(PhotonView))]
public class PlayerSetup : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject playerModel;
    [SerializeField] private TMP_Text nameDisplay;
    [SerializeField] private bool isCustomizable = true;
    [SerializeField] private List<Material> allPossibleMaterials;

    private PhotonView photonView;

    void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }

    void Start()
    {
        if (photonView.IsMine)
        {
            string myName = PlayerData.PlayerName;
            string myMaterialName = "";

            if (isCustomizable && PlayerData.PlayerMaterial != null)
            {
                myMaterialName = PlayerData.PlayerMaterial.name;
            }

            photonView.RPC("RPC_SetPlayerVisuals", RpcTarget.AllBuffered, myName, myMaterialName);
        }
    }

    [PunRPC]
    void RPC_SetPlayerVisuals(string playerName, string materialName)
    {
        this.gameObject.name = playerName;
        if (nameDisplay != null)
        {
            nameDisplay.text = playerName;
        }

        if (isCustomizable && !string.IsNullOrEmpty(materialName))
        {
            Material mat = allPossibleMaterials.Find(m => m.name == materialName);

            if (mat != null && playerModel != null)
            {
                Renderer[] renderers = playerModel.GetComponentsInChildren<Renderer>();
                foreach (Renderer renderer in renderers)
                {
                    renderer.material = mat;
                }
            }
        }
    }
}