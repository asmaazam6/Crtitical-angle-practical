using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class LabExperimentManager : MonoBehaviour
{
    public static LabExperimentManager Instance { get; private set; }

    public enum LabStep { PlacePrism, TracePrism, PlaceIncidentPins, LookThroughPrism, Completed }

    [Header("Current Progress")]
    public LabStep CurrentStep = LabStep.PlacePrism;

    [Header("UI Components")]
    public TMPro.TextMeshProUGUI instructionText;
    public Button nextStepButton;

    [Header("Camera Positions")]
    public Transform mainCameraTransform;
    public Transform camPosOverview;      // Positioned at an angle showing the whole board
    public Transform camPosTopDown;       // Positioned directly above looking down (for tracing)
    public Transform camPosEyeLevel;      // Positioned low on the side looking *through* the glass

    [Header("Visual Guides")]
    public GameObject prismGhostOutline;   // A semi-transparent 3D model/line showing where to put it

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        nextStepButton.onClick.AddListener(AdvanceStep);
        UpdateCurrentStepState();
    }

    public void AdvanceStep()
    {
        if (CurrentStep != LabStep.Completed)
        {
            CurrentStep++;
            UpdateCurrentStepState();
        }
    }

    private void UpdateCurrentStepState()
    {
        switch (CurrentStep)
        {
            case LabStep.PlacePrism:
                instructionText.text = "<b>Step 1:</b> Drag and place the glass prism onto the designated outline on the drawing board.";
                prismGhostOutline.SetActive(true);
                nextStepButton.gameObject.SetActive(false); // Hide button; trigger this automatically when dropped perfectly
                StartCoroutine(MoveCameraToPosition(camPosOverview, 1.5f));
                break;

            case LabStep.TracePrism:
                instructionText.text = "<b>Step 2: Tracing</b>\nUse your pencil to trace the outer triangular boundary of the prism.";
                prismGhostOutline.SetActive(false);
                nextStepButton.gameObject.SetActive(true);
                StartCoroutine(MoveCameraToPosition(camPosTopDown, 1.2f));
                break;

            case LabStep.PlaceIncidentPins:
                instructionText.text = "<b>Step 3: Incident Ray & Normal</b>\nDraw a straight line connecting Pin 3 and Pin 4, extending it forward until it hits the angle of line refracted by Pin1.";
                nextStepButton.gameObject.SetActive(true);
                StartCoroutine(MoveCameraToPosition(camPosTopDown, 1.2f));
                break;

            case LabStep.LookThroughPrism:
                instructionText.text = "<b>Step 4: Refracted & Emergent Rays</b>\nDraw a straight line from Pin 1 directly to the left edge of the prism, then extend it straight outward using equal distance.\n2. Join that entry point to the Pin 3 surface point inside the prism.";
                nextStepButton.gameObject.SetActive(true);
                StartCoroutine(MoveCameraToPosition(camPosTopDown, 1.2f));
                break;

            case LabStep.Completed:
                instructionText.text = "<b>Experiment Finished!</b> You have mapped out the refraction matrix through a glass prism successfully.";
                nextStepButton.gameObject.SetActive(false);
                StartCoroutine(MoveCameraToPosition(camPosOverview, 1.5f));
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayCelebration();
                }
                break;
        }
    }

    // Call this from your PrismDragger script when it snaps perfectly into the ghost boundary box
    public void OnPrismPlacedSuccessfully()
    {
        if (CurrentStep == LabStep.PlacePrism)
        {
            AdvanceStep();
        }
    }

    // Smooth Camera interpolation coroutine
    private IEnumerator MoveCameraToPosition(Transform targetTransform, float duration)
    {
        if (mainCameraTransform == null || targetTransform == null) yield break;

        Vector3 startPos = mainCameraTransform.position;
        Quaternion startRot = mainCameraTransform.rotation;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration); // Clean ease-in-out curve

            mainCameraTransform.position = Vector3.Lerp(startPos, targetTransform.position, t);
            mainCameraTransform.rotation = Quaternion.Slerp(startRot, targetTransform.rotation, t);
            yield return null;
        }

        mainCameraTransform.position = targetTransform.position;
        mainCameraTransform.rotation = targetTransform.rotation;
    }
}
