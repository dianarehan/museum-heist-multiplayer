using UnityEngine;
using UnityEngine.UI; 
using TMPro;
using System.Linq;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using System.Collections;

public class MenuCustomizer : MonoBehaviour
{
    [SerializeField] private TMP_InputField nameInputField;
    
    [SerializeField] private Button saveButton;

    [SerializeField] private GameObject previewModel;

    [SerializeField]
    private SerializedDictionary<Button, Material> materialButtons;
    [SerializeField] private GameObject customPanel;
    [SerializeField] private GameObject mainMenuPanel;
    private Material selectedMaterial;
    private string selectedName;

    void Start()
    {        
        if (PlayerData.PlayerMaterial != null)
        {
            selectedMaterial = PlayerData.PlayerMaterial;
        }
        else if (materialButtons != null && materialButtons.Count > 0)
        {
            selectedMaterial = materialButtons.Values.FirstOrDefault();
        }
        ApplyMaterialToPreview(selectedMaterial);

        selectedName = PlayerData.PlayerName;
        if (nameInputField != null)
        {
            nameInputField.text = selectedName;
        }

        if (saveButton != null)
        {
            saveButton.onClick.AddListener(SaveChanges);
        }

        if (nameInputField != null)
        {
            nameInputField.onValueChanged.AddListener(OnNameChanged);
        }

        if (materialButtons != null)
        {
            foreach (KeyValuePair<Button, Material> pair in materialButtons)
            {
                Button button = pair.Key;
                Material material = pair.Value;

                if (button != null && material != null)
                {
                    button.onClick.AddListener(() => SelectMaterial(material));
                }
            }
        }
    }

    private void OnNameChanged(string newName)
    {
        selectedName = newName;
    }

    private void SelectMaterial(Material mat)
    {
        selectedMaterial = mat;
        ApplyMaterialToPreview(selectedMaterial);
    }

    public void SaveChanges()
    {
        PlayerData.PlayerName = selectedName;
        PlayerData.PlayerMaterial = selectedMaterial;

        Debug.Log($"Changes Saved: Name = {PlayerData.PlayerName}, Material = {PlayerData.PlayerMaterial.name}");

        StartCoroutine(SaveAndBackToMenu());
    }

    private void ApplyMaterialToPreview(Material mat)
    {
        if (previewModel == null || mat == null) return;

        Renderer[] renderers = previewModel.GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            renderer.material = mat;
        }
    }

    private IEnumerator SaveAndBackToMenu()
    {
        saveButton.GetComponentInChildren<TMP_Text>().text = "Loading...";
        yield return new WaitForSeconds(0.8f);
        customPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        saveButton.GetComponentInChildren<TMP_Text>().text = "Save Changes";
    }
}