using UnityEngine;
using UnityEngine.EventSystems;

public class ThumbPinDragger : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private Camera mainCamera;
    private bool isDragging = false;

    [Header("Placement Settings")]
    [SerializeField] private LayerMask boardLayer;

    [Tooltip("The height the pin floats above the board surface while you are actively dragging it.")]
    [SerializeField] private float dragHeight = 0.3f;

    [Tooltip("How far the pin sinks into the board when dropped.")]
    [SerializeField] private float insertionDepth = 0.02f;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (isDragging)
        {
            DragPinOnSurface();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log($"[Pin Clicked] Grabbed: {gameObject.name}", this);
        isDragging = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log("[Pin Released] Dropping pin.", this);
        isDragging = false;
        TryInsertPin();
    }

    private void DragPinOnSurface()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        // Disable collider temporarily so the pin doesn't block its own path to the board
        Collider myCollider = GetComponent<Collider>();
        if (myCollider != null) myCollider.enabled = false;

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, boardLayer))
        {
            transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            transform.position = hit.point + (hit.normal * dragHeight);
        }

        if (myCollider != null) myCollider.enabled = true;
    }

    private void TryInsertPin()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        // Cast widely to capture any geometry intersecting the placement zone
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
        {
            // Fix: Check object naming variations safely without throwing missing tag exceptions
            if (hit.collider.gameObject.name.Contains("Prism"))
            {
                Debug.LogWarning("[Insertion Blocked] Cannot place a pin directly through the Prism.", this);
                return; // Safety exit
            }

            // Check if the pin landed squarely on the board components
            if (hit.collider.CompareTag("Paper") || hit.collider.name.Contains("DrawingBoard"))
            {
                Debug.Log("<color=green>[Success]</color> Pin conditions met! Inserting pin.", this);

                // Match orientation to surface plane normal
                transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

                // Sink slightly below face level
                Vector3 targetPosition = hit.point;
                targetPosition -= hit.normal * insertionDepth;

                transform.position = targetPosition;
            }
        }
        else
        {
            Debug.LogWarning("[Insertion Failed] Raycast missed entirely upon release.", this);
        }
    }
}