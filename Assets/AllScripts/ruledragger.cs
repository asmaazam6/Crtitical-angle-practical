using UnityEngine;
using UnityEngine.EventSystems;

public class RulerDragger : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private Camera mainCamera;
    private bool isDragging = false;
    [SerializeField] private LayerMask boardLayer;
    [SerializeField] private float dragHeight = 0.02f; // Keeps it flat right above paper

    void Start() => mainCamera = Camera.main;

    void Update()
    {
        if (isDragging)
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, boardLayer))
            {
                // Keeps it perfectly flat parallel to the board while sliding
                transform.position = hit.point + (hit.normal * dragHeight);
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData) => isDragging = true;
    public void OnPointerUp(PointerEventData eventData) => isDragging = false;
}