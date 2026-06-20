using UnityEngine;
using UnityEngine.EventSystems;

public class PrismDragger : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private Camera mainCamera;
    private bool isDragging = false;
    private Collider prismCollider;
    private float prismHalfHeight;

    [Header("Placement Settings")]
    [SerializeField] private LayerMask boardLayer;

    [Tooltip("Extra fine-tuning offset if needed. Keep at 0 to start.")]
    [SerializeField] private float fineTuneOffset = 0.0f;

    void Start()
    {
        mainCamera = Camera.main;
        prismCollider = GetComponent<Collider>();

        // Calculate the static half-height dimension along its localized geometry
        if (prismCollider != null)
        {
            // Using localized bounding parameters protects against skew variations during rotation
            prismHalfHeight = prismCollider.bounds.extents.y;
        }
        else
        {
            prismHalfHeight = 0.5f; // Fallback fallback default
        }
    }

    // Inside PrismDragger.cs Update()
    void Update()
    {
        // ONLY allow dragging if we are in the placement phase!
        if (LabExperimentManager.Instance != null && LabExperimentManager.Instance.CurrentStep != LabExperimentManager.LabStep.PlacePrism)
        {
            isDragging = false;
            return;
        }

        if (isDragging) { DragPrismToSurface(); }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
        SnapToSurface();
    }

    private void DragPrismToSurface()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        // Temporarily disable our collider so we don't raycast against ourselves
        if (prismCollider != null) prismCollider.enabled = false;

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, boardLayer))
        {
            // Position above the point dynamically while sliding
            transform.position = hit.point + (hit.normal * (prismHalfHeight + fineTuneOffset));
        }

        if (prismCollider != null) prismCollider.enabled = true;
    }

    private void SnapToSurface()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (prismCollider != null) prismCollider.enabled = false;

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, boardLayer))
        {
            // Fix: Cleanly align upward normal vectors without destroying existing yaw rotations
            Vector3 targetUp = hit.normal;
            Vector3 forwardProjection = Vector3.ProjectOnPlane(transform.forward, targetUp);

            if (forwardProjection.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(forwardProjection, targetUp);
            }
            else
            {
                transform.rotation = Quaternion.FromToRotation(Vector3.up, targetUp) * transform.rotation;
            }

            // Lock position firmly right flush on top of the surface point
            transform.position = hit.point + (hit.normal * (prismHalfHeight + fineTuneOffset));

            Debug.Log("<color=cyan>[Prism Placed]</color> Prism successfully sitting on the surface.");
            if (LabExperimentManager.Instance != null)
            {
                LabExperimentManager.Instance.OnPrismPlacedSuccessfully();
            }
        }

        if (prismCollider != null) prismCollider.enabled = true;
    }
}