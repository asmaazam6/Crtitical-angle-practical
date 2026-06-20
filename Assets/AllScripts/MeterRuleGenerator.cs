using UnityEngine;

public class MeterRuleGenerator : MonoBehaviour
{
    [Header("Ruler Dimensions")]
    public float rulerLengthInMeters = 1.0f;
    public float rulerWidth = 0.05f;
    public float rulerThickness = 0.01f;

    [Header("Tick Mark Dimensions")]
    public float cmTickLength = 0.01f;
    public float dmTickLength = 0.02f;
    public float tickThickness = 0.002f;

    [Header("Materials")]
    public Material rulerMaterial;
    public Material tickMaterial;
    public Material decimeterTickMaterial; // Optional: Distinct color for major blocks (e.g., Red or Bold Black)

    void Start()
    {
        GenerateRuler();
    }

    public float MeasureDistance(Vector3 pointA, Vector3 pointB)
    {
        return Vector3.Distance(pointA, pointB);
    }

    void GenerateRuler()
    {
        // 1. Create the Base Ruler Body
        GameObject rulerBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rulerBody.name = "RulerBody";
        rulerBody.transform.SetParent(this.transform);

        // Remove colliders so it doesn't block rays or pins
        Destroy(rulerBody.GetComponent<BoxCollider>());

        rulerBody.transform.localScale = new Vector3(rulerWidth, rulerThickness, rulerLengthInMeters);
        rulerBody.transform.localPosition = new Vector3(0, -rulerThickness / 2, rulerLengthInMeters / 2);

        if (rulerMaterial != null)
            rulerBody.GetComponent<Renderer>().material = rulerMaterial;

        // 2. Generate Ticks
        int totalCentimeters = Mathf.RoundToInt(rulerLengthInMeters * 100);

        for (int i = 0; i <= totalCentimeters; i++)
        {
            float zPosition = i * 0.01f;
            bool isDecimeter = (i % 10 == 0);
            bool isHalfDecimeter = (i % 5 == 0);

            // Determine tick size based on significance
            float currentTickLength = cmTickLength;
            float heightMultiplier = 1.2f;

            if (isDecimeter)
            {
                currentTickLength = dmTickLength;
                heightMultiplier = 2.0f; // Make 10cm markers physically taller for easy visual grouping
            }
            else if (isHalfDecimeter)
            {
                currentTickLength = dmTickLength * 0.75f;
                heightMultiplier = 1.5f; // 5cm marks are intermediate height
            }

            // Create Tick GameObject
            GameObject tick = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tick.name = $"Tick_{i}cm";
            tick.transform.SetParent(this.transform);

            // Clean up colliders so this ruler is completely invisible to physics and optics raycasts
            Destroy(tick.GetComponent<BoxCollider>());

            // Scale and position the tick mark
            tick.transform.localScale = new Vector3(currentTickLength, rulerThickness * heightMultiplier, tickThickness);
            tick.transform.localPosition = new Vector3((-rulerWidth / 2) + (currentTickLength / 2), 0, zPosition);

            // Assign Materials based on block type
            if (isDecimeter && decimeterTickMaterial != null)
            {
                tick.GetComponent<Renderer>().material = decimeterTickMaterial;
            }
            else if (tickMaterial != null)
            {
                tick.GetComponent<Renderer>().material = tickMaterial;
            }
        }
    }
}