using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class OrbitCameraController : MonoBehaviour
{
    [Header("Orbit Pivot")]
    [SerializeField] private Transform pivotTarget;
    [SerializeField] private Vector3 fallbackPivot = new Vector3(0f, 0f, -8f);

    [Header("Orbit")]
    [SerializeField] private float yaw = 0f;
    [SerializeField] private float pitch = 55f;
    [SerializeField] private float minPitch = 35f;
    [SerializeField] private float maxPitch = 72f;
    [SerializeField] private float orbitSensitivity = 0.22f;

    [Header("Zoom")]
    [SerializeField] private float distance = 42f;
    [SerializeField] private float minDistance = 24f;
    [SerializeField] private float maxDistance = 62f;
    [SerializeField] private float zoomStep = 6f;

    [Header("Smoothing")]
    [SerializeField] private bool useSmoothing = true;
    [SerializeField] private float orbitSmoothTime = 0.045f;
    [SerializeField] private float zoomSmoothTime = 0.055f;

    [Header("Input")]
    [SerializeField] private bool requireRightMouseForOrbit = true;

    private Vector3 pivot;

    private float targetYaw;
    private float targetPitch;
    private float targetDistance;

    private float currentYaw;
    private float currentPitch;
    private float currentDistance;

    private float yawVelocity;
    private float pitchVelocity;
    private float distanceVelocity;

    private void Start()
    {
        AutoFindPivotTargetIfMissing();

        pivot = pivotTarget != null
            ? pivotTarget.position
            : fallbackPivot;

        targetYaw = yaw;
        targetPitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        targetDistance = Mathf.Clamp(distance, minDistance, maxDistance);

        currentYaw = targetYaw;
        currentPitch = targetPitch;
        currentDistance = targetDistance;

        ApplyCameraPosition();
    }

    private void LateUpdate()
    {
        if (Mouse.current == null)
            return;

        HandleOrbitInput();
        HandleZoomInput();
        UpdateSmoothedValues();
        ApplyCameraPosition();
    }

    private void AutoFindPivotTargetIfMissing()
    {
        if (pivotTarget != null)
            return;

        GameObject capturePoint = GameObject.Find("CapturePoint_Center");

        if (capturePoint != null)
            pivotTarget = capturePoint.transform;
    }

    private void HandleOrbitInput()
    {
        bool canOrbit = !requireRightMouseForOrbit || Mouse.current.rightButton.isPressed;

        if (!canOrbit)
            return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        targetYaw += mouseDelta.x * orbitSensitivity;
        targetPitch -= mouseDelta.y * orbitSensitivity;
        targetPitch = Mathf.Clamp(targetPitch, minPitch, maxPitch);
    }

    private void HandleZoomInput()
    {
        Vector2 scroll = Mouse.current.scroll.ReadValue();

        if (Mathf.Abs(scroll.y) < 0.01f)
            return;

        float scrollSteps = scroll.y / 120f;

        targetDistance -= scrollSteps * zoomStep;
        targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
    }

    private void UpdateSmoothedValues()
    {
        if (!useSmoothing)
        {
            currentYaw = targetYaw;
            currentPitch = targetPitch;
            currentDistance = targetDistance;
            return;
        }

        currentYaw = Mathf.SmoothDampAngle(
            currentYaw,
            targetYaw,
            ref yawVelocity,
            orbitSmoothTime
        );

        currentPitch = Mathf.SmoothDamp(
            currentPitch,
            targetPitch,
            ref pitchVelocity,
            orbitSmoothTime
        );

        currentDistance = Mathf.SmoothDamp(
            currentDistance,
            targetDistance,
            ref distanceVelocity,
            zoomSmoothTime
        );
    }

    private void ApplyCameraPosition()
    {
        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0f);
        Vector3 offset = rotation * new Vector3(0f, 0f, -currentDistance);

        transform.position = pivot + offset;
        transform.LookAt(pivot);
    }
}