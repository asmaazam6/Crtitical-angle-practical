using UnityEngine;
using System.Collections.Generic;

public class OpticsManager : MonoBehaviour
{
    [Header("Optical Properties")]
    [Tooltip("Refractive index of the glass prism (e.g., Crown Glass = 1.52)")]
    [SerializeField] private float indexOfRefractionGlass = 1.52f;
    private float indexOfRefractionAir = 1.0f;

    [Header("Simulation Setup")]
    [SerializeField] private LayerMask prismLayer;
    public Material rayMaterial; // Assign a glowing material here (unlit color)
    public float rayWidth = 0.02f;

    private ThumbPinDragger[] activePins;
    private List<LineRenderer> lineRenderers = new List<LineRenderer>();

    void Update()
    {
        // Clear old visual paths from previous frame
        ClearRayLines();

        // Use Unity 6 clean syntax to locate all pins
        activePins = Object.FindObjectsByType<ThumbPinDragger>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        foreach (var pin in activePins)
        {
            // 1. Locate the child mesh named "Body" inside this specific pin
            Transform visualBody = pin.transform.Find("Body");

            Vector3 calculationPosition;

            if (visualBody != null)
            {
                // If found, use the actual world position of the visual pin cylinder base
                calculationPosition = visualBody.position;
            }
            else
            {
                // Fallback to parent position if child is missing
                calculationPosition = pin.transform.position;
            }

            // 2. Fire the ray using the real visual contact position
            SimulateNeedleRayVisual(calculationPosition, pin.transform.forward);
        }
    }

    private void SimulateNeedleRayVisual(Vector3 pinPosition, Vector3 pinForward)
    {
        GameObject prism = GameObject.Find("Prism");
        if (prism == null) return;
        Vector3 rayOrigin = new Vector3(pinPosition.x, pinPosition.y + 0.01f, pinPosition.z);
        // Use the Pin's forward direction or aim directly at the prism center
        Vector3 directionToPrism = (prism.transform.position - rayOrigin).normalized;
        directionToPrism.y = 0;
        directionToPrism.Normalize();

        // Slightly lift ray origin off the paper surface to prevent rendering clipping
        // This forces the laser path lines to stay perfectly flat on top of the paper surface

        Ray ray = new Ray(rayOrigin, directionToPrism);
        List<Vector3> visualPoints = new List<Vector3> { rayOrigin };

        // 1. Ray hits Entering Surface
        if (Physics.Raycast(ray, out RaycastHit hit1, Mathf.Infinity, prismLayer))
        {
            visualPoints.Add(hit1.point);

            Vector3 incidentRay = directionToPrism;
            Vector3 normal1 = hit1.normal;

            // Refract into glass
            Vector3 refractedRay1 = Refract(incidentRay, normal1, indexOfRefractionAir, indexOfRefractionGlass);

            // Trick to hit the interior back-face: enable Queries Hit Backfaces in physics
            // Or manually calculate the exit point using a reverse raycast step
            Ray internalRay = new Ray(hit1.point + refractedRay1 * 0.01f, refractedRay1);

            // To reliably hit inside ProBuilder geometry, invert the ray check distance or clear the layer block
            if (Physics.Raycast(internalRay, out RaycastHit hit2, Mathf.Infinity, prismLayer))
            {
                visualPoints.Add(hit2.point);
                Vector3 normal2 = hit2.normal;

                float angleOfIncidence = Vector3.Angle(-refractedRay1, normal2);
                float criticalAngle = Mathf.Asin(indexOfRefractionAir / indexOfRefractionGlass) * Mathf.Rad2Deg;

                Vector3 finalRayDirection;

                if (angleOfIncidence >= criticalAngle)
                {
                    // Total Internal Reflection
                    finalRayDirection = Vector3.Reflect(refractedRay1, normal2);
                }
                else
                {
                    // Exit Refraction out to air
                    finalRayDirection = Refract(refractedRay1, -normal2, indexOfRefractionGlass, indexOfRefractionAir);
                }

                visualPoints.Add(hit2.point + finalRayDirection * 5.0f);
            }
            else
            {
                // Fallback if interior hit fails to register backfaces
                visualPoints.Add(hit1.point + refractedRay1 * 2.0f);
            }
        }
        else
        {
            // Ray missed the prism completely, draw straight forward
            visualPoints.Add(rayOrigin + directionToPrism * 5.0f);
        }

        CreateVisualLine(visualPoints);
    }

    private Vector3 Refract(Vector3 incident, Vector3 normal, float n1, float n2)
    {
        float eta = n1 / n2;
        float cosTheta = Mathf.Clamp(Vector3.Dot(-incident, normal), -1f, 1f);
        float k = 1.0f - eta * eta * (1.0f - cosTheta * cosTheta);

        if (k < 0.0f) return Vector3.Reflect(incident, normal);
        return eta * incident + (eta * cosTheta - Mathf.Sqrt(k)) * normal;
    }

    private void CreateVisualLine(List<Vector3> points)
    {
        GameObject lineObj = new GameObject("VisualRayPath");
        lineObj.transform.SetParent(transform);

        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.material = rayMaterial != null ? rayMaterial : new Material(Shader.Find("Sprites/Default"));
        lr.startWidth = rayWidth;
        lr.endWidth = rayWidth;
        lr.startColor = Color.cyan;
        lr.endColor = Color.cyan;
        lr.positionCount = points.Count;
        lr.SetPositions(points.ToArray());

        lineRenderers.Add(lr);
    }

    private void ClearRayLines()
    {
        foreach (var lr in lineRenderers)
        {
            if (lr != null) Destroy(lr.gameObject);
        }
        lineRenderers.Clear();
    }
}