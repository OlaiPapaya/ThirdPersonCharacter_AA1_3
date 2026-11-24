using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset;
    [SerializeField] private float camDistance;
    [SerializeField] private float sensitivity;
    [SerializeField] private float verticalLowerRangeAngle;
    [SerializeField] private float verticalUpperRangeAngle;
    [SerializeField] private LayerMask ignoredLayers;
    [SerializeField] private float castRadius = 0.3f;
    [SerializeField] private float cameraDistanceChangeSpeed = 20f;

    private Vector2 currentRotation;
    private float currentDistance;
    private float smoothCurrentDistance;

    private InputSystem_Actions input;

    void Start()
    {
        input = new();
        input.Player.Look.Enable();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        currentDistance = camDistance;
        smoothCurrentDistance = camDistance;
    }

    void Update()
    {
        // If the camera distance changes, we adjust the offset si it's smaller when the cam is closer.
        float distanceMultiplier = Mathf.Clamp01(smoothCurrentDistance / camDistance);
        Vector3 rawCenterPos = target.position + transform.TransformDirection(offset);
        Vector3 centerPos = target.position + transform.TransformDirection(offset) * distanceMultiplier;

        AdjustDistance(centerPos, rawCenterPos);
        RotateCamera(centerPos);
    }

    void AdjustDistance(Vector3 centerPos, Vector3 rawCenterPos)
    {
        Vector3 originalCamPos = rawCenterPos - transform.forward * camDistance;
        Vector3 castDir = (originalCamPos - target.position).normalized * camDistance;
        // Cast a sphere from the player to the camera to detect objects in the middle
        if (Physics.SphereCast(target.position, castRadius, castDir, out RaycastHit hit, camDistance, ~ignoredLayers)
            && hit.distance < camDistance)
        {
            // If the camera has to get closer, the correction is instant.
            if (hit.distance < currentDistance) smoothCurrentDistance = hit.distance;
            // If not, we set currentDistance and let the smoothing take care.
            currentDistance = hit.distance;
        }
        else
        {
            currentDistance = camDistance;
        }

        // If the smoothed distance is already correct, do nothing
        if (Mathf.Approximately(currentDistance, smoothCurrentDistance)) return;

        // Smooth camera distance change.
        smoothCurrentDistance = Mathf.Lerp(smoothCurrentDistance, currentDistance, cameraDistanceChangeSpeed * Time.deltaTime);
    }

    void RotateCamera(Vector3 centerPos)
    {
        Vector2 mouseDelta = input.Player.Look.ReadValue<Vector2>() * sensitivity;

        // Calculating the next turret's rotation using player input
        currentRotation = new(
            currentRotation.x - mouseDelta.y,
            currentRotation.y + mouseDelta.x
        );

        // Clamp current rotation to the specified limit.
        if (verticalLowerRangeAngle != 0 && verticalUpperRangeAngle != 0)
            currentRotation.x = Mathf.Clamp(currentRotation.x, verticalLowerRangeAngle, verticalUpperRangeAngle);

        // Apply the processed rotation
        transform.localEulerAngles = new(
            currentRotation.x,
            currentRotation.y,
            transform.localEulerAngles.z
            );

        transform.position = centerPos - transform.forward * smoothCurrentDistance;
    }

    private void OnDestroy()
    {
        input.Player.Look.Disable();
    }
}
