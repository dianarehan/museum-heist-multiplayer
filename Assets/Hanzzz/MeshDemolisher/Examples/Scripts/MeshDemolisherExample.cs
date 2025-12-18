using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Hanzzz.MeshDemolisher
{

public class MeshDemolisherExample : MonoBehaviour
{
    [Header("Hint: right click on the script to call Demolish and Reset\nin the editor mode.")]
    [Space]
    [SerializeField] private GameObject targetGameObject;
    [SerializeField] private Transform breakPointsParent;
    [SerializeField] private Material interiorMaterial;

    [SerializeField] private KeyCode demolishKey;

    [SerializeField] private Transform resultParent;

    [SerializeField] private TMP_Text logText;
    
    [Header("Glass Physics")]
    [SerializeField] private float pieceMass = 0.1f;
    [SerializeField] private float destroyDelay = 5f;

    private static MeshDemolisher meshDemolisher = new MeshDemolisher();
    
    private void Start()
    {
        // Hide break points (they're just markers, not visible)
        // Skip the target object if it's in the list
        if (breakPointsParent != null)
        {
            foreach (Transform child in breakPointsParent)
            {
                // Don't hide the target object
                if (child.gameObject == targetGameObject) continue;
                
                Renderer rend = child.GetComponent<Renderer>();
                if (rend != null) rend.enabled = false;
            }
        }
    }

    private void Update()
    {
        if(!Input.GetKeyDown(demolishKey))
        {
            return;
        }

        if(targetGameObject.activeSelf)
        {
            Demolish();
        }
        else
        {
            Reset();
        }
    }

    [ContextMenu("Verify Demolish Input")]
    public void VerifyDemolishInput()
    {
        List<Transform> breakPoints = Enumerable.Range(0,breakPointsParent.childCount).Select(x=>breakPointsParent.GetChild(x)).ToList();

        // Passing this verification does not mean the input is valid.
        // Refer to the documentation to see all input requirements.
        bool res = meshDemolisher.VerifyDemolishInput(targetGameObject, breakPoints);
        if(res)
        {
            Debug.Log("Demolish input looks good.");
        }
    }

    [ContextMenu("Demolish")]
    public void Demolish()
    {
        Enumerable.Range(0,resultParent.childCount).Select(i=>resultParent.GetChild(i)).ToList().ForEach(x=>DestroyImmediate(x.gameObject));
        List<Transform> breakPoints = Enumerable.Range(0,breakPointsParent.childCount).Select(x=>breakPointsParent.GetChild(x)).ToList();

        var watch = System.Diagnostics.Stopwatch.StartNew();
        List<GameObject> res = meshDemolisher.Demolish(targetGameObject, breakPoints, interiorMaterial);
        watch.Stop();
        // logText?.text = $"Demolish time: {watch.ElapsedMilliseconds}ms.";

        res.ForEach(x=>x.transform.SetParent(resultParent, true));
        
        // Keep natural scale (don't resize)
        res.ForEach(x=>x.transform.localScale = Vector3.one);
        
        // Add physics to each piece so they fall
        foreach (var piece in res)
        {
            AddPhysicsToPiece(piece);
        }

        targetGameObject.SetActive(false);
    }

    [ContextMenu("Demolish Async")]
    public async void DemolishAsync()
    {
        Enumerable.Range(0,resultParent.childCount).Select(i=>resultParent.GetChild(i)).ToList().ForEach(x=>DestroyImmediate(x.gameObject));
        List<Transform> breakPoints = Enumerable.Range(0,breakPointsParent.childCount).Select(x=>breakPointsParent.GetChild(x)).ToList();

        var watch = System.Diagnostics.Stopwatch.StartNew();
        List<GameObject> res = await meshDemolisher.DemolishAsync(targetGameObject, breakPoints, interiorMaterial);
        watch.Stop();
        // logText?.text = $"Demolish time: {watch.ElapsedMilliseconds}ms.";

        res.ForEach(x=>x.transform.SetParent(resultParent, true));
        
        // Keep natural scale (don't resize)
        res.ForEach(x=>x.transform.localScale = Vector3.one);
        
        // Add physics to each piece so they fall
        foreach (var piece in res)
        {
            AddPhysicsToPiece(piece);
        }

        targetGameObject.SetActive(false);
    }

    [ContextMenu("Reset")]
    public void Reset()
    {
        Enumerable.Range(0,resultParent.childCount).Select(i=>resultParent.GetChild(i)).ToList().ForEach(x=>DestroyImmediate(x.gameObject));

        targetGameObject.SetActive(true);
    }
    
    private void AddPhysicsToPiece(GameObject piece)
    {
        // Add MeshCollider for collision
        MeshCollider meshCollider = piece.AddComponent<MeshCollider>();
        meshCollider.convex = true;
        
        // Add Rigidbody - just let it fall naturally with gravity
        Rigidbody rb = piece.AddComponent<Rigidbody>();
        rb.mass = pieceMass;
        
        // Destroy piece after delay to clean up
        if (destroyDelay > 0)
        {
            Destroy(piece, destroyDelay);
        }
    }
}

}
