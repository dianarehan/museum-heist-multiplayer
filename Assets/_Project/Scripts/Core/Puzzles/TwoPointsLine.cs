using UnityEngine;

[ExecuteInEditMode]
public class TwoPointsLine : MonoBehaviour
{
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;

    private LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer = gameObject.GetComponent<LineRenderer>();
    }

    void Update()
    {
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, pointA.position);
        lineRenderer.SetPosition(1, pointB.position);
    }
}