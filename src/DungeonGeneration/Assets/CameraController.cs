using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float fastMultiplier = 4f;
    [SerializeField] private float slowMultiplier = 0.25f;
    [SerializeField] private float verticalSpeed = 8f;
    [SerializeField] private bool useWorldUpForVertical = true;

    [Header("Look")]
    [SerializeField] private float mouseSensitivity = 2.0f;
    [SerializeField] private bool invertY = false;
    [SerializeField] private float pitchMin = -89f;
    [SerializeField] private float pitchMax = 89f;

    [Header("Behavior")]
    [SerializeField] private bool requireRightMouseButtonToLook = true;
    [SerializeField] private bool lockCursorWhileLooking = true;

    private float _yaw;
    private float _pitch;

    private void Start()
    {
        var euler = transform.eulerAngles;
        _yaw = euler.y;
        _pitch = euler.x;

        if (!requireRightMouseButtonToLook && lockCursorWhileLooking)
            SetCursorLocked(true);
    }

    private void OnDisable()
    {
        SetCursorLocked(false);
    }

    private void Update()
    {
        HandleLook();
        HandleMove();
    }

    private void HandleLook()
    {
        bool looking = !requireRightMouseButtonToLook || Input.GetMouseButton(1);

        if (lockCursorWhileLooking)
            SetCursorLocked(looking);

        if (!looking)
            return;

        float mouseX = Input.GetAxisRaw("Mouse X");
        float mouseY = Input.GetAxisRaw("Mouse Y");

        float ySign = invertY ? 1f : -1f;

        _yaw += mouseX * mouseSensitivity;
        _pitch += mouseY * mouseSensitivity * ySign;
        _pitch = Mathf.Clamp(_pitch, pitchMin, pitchMax);

        transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
    }

    private void HandleMove()
    {
        float multiplier = 1f;
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) multiplier *= fastMultiplier;
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) multiplier *= slowMultiplier;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 move = (transform.right * h + transform.forward * v);

        float up = 0f;
        if (Input.GetKey(KeyCode.E)) up += 1f;
        if (Input.GetKey(KeyCode.Q)) up -= 1f;

        Vector3 vertical = useWorldUpForVertical ? (Vector3.up * up) : (transform.up * up);

        Vector3 velocity = (moveSpeed * multiplier) * move + (verticalSpeed * multiplier) * vertical;
        transform.position += velocity * Time.unscaledDeltaTime;
    }

    private static void SetCursorLocked(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}