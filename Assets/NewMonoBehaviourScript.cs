using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float sprintSpeed = 8f;
    public float jumpHeight = 2f;
    public float gravity = -20f;
    public int maxAirJumps = 1;

    [Header("Dash")]
    public float dashDistance = 6f;
    public float dashDuration = 0.25f;
    public float dashCooldown = 1f;

    [Header("Field of View")]
    public float sprintFovIncrease = 13f;
    public float dashFovIncrease = 20f;
    public float fovChangeSpeed = 8f;

    [Header("References")]
    public Transform cameraTransform;

    private CharacterController controller;
    private Camera fovCamera;
    private float startingFov;
    private Vector3 velocity;
    private Vector3 dashDirection;
    private int jumpCount;
    private float dashTimer;
    private float dashCooldownTimer;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        ResolveFovCamera();

        if (controller == null)
        {
            Debug.LogError("CharacterController not found on " + name, this);
        }
    }

    private void Update()
    {
        if (controller == null)
            return;

        if (fovCamera == null)
            ResolveFovCamera();

        bool wasGrounded = controller.isGrounded;
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 forward = (cameraTransform != null) ? cameraTransform.forward : transform.forward;
        Vector3 right = (cameraTransform != null) ? cameraTransform.right : transform.right;

        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = (forward * vertical + right * horizontal).normalized;
        Keyboard keyboard = Keyboard.current;
        bool sprintPressed = keyboard != null &&
            (keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed);
        bool isSprinting = sprintPressed && moveDirection.sqrMagnitude > 0f;
        float currentSpeed = sprintPressed
            ? sprintSpeed
            : moveSpeed;

        if (wasGrounded && velocity.y < 0f)
        {
            velocity.y = -2f;
            jumpCount = 0;
        }

        if (Input.GetButtonDown("Jump"))
        {
            if (wasGrounded)
            {
                PerformJump();
                jumpCount = 1;
            }
            else if (jumpCount < maxAirJumps + 1)
            {
                PerformJump();
                jumpCount++;
            }
        }

        dashCooldownTimer = Mathf.Max(0f, dashCooldownTimer - Time.deltaTime);
        if (keyboard != null && keyboard.qKey.wasPressedThisFrame && dashCooldownTimer <= 0f && dashDuration > 0f)
        {
            dashDirection = moveDirection.sqrMagnitude > 0f ? moveDirection : forward;
            dashTimer = dashDuration;
            dashCooldownTimer = dashCooldown;
        }

        velocity.y += gravity * Time.deltaTime;
        Vector3 movement = moveDirection * currentSpeed + Vector3.up * velocity.y;
        float dashStep = Mathf.Min(Time.deltaTime, dashTimer);
        bool isDashing = dashTimer > 0f;
        if (dashStep > 0f)
        {
            movement += dashDirection * (dashDistance / dashDuration) * (dashStep / Time.deltaTime);
            dashTimer -= dashStep;
        }

        controller.Move(movement * Time.deltaTime);

        if (fovCamera != null)
        {
            float fovIncrease = isDashing
                ? dashFovIncrease
                : isSprinting ? sprintFovIncrease : 0f;
            float targetFov = startingFov + fovIncrease;
            fovCamera.fieldOfView = Mathf.Lerp(
                fovCamera.fieldOfView,
                targetFov,
                fovChangeSpeed * Time.deltaTime);
        }
    }

    private void PerformJump()
    {
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }

    private void ResolveFovCamera()
    {
        fovCamera = cameraTransform != null
            ? cameraTransform.GetComponent<Camera>()
            : null;

        if (fovCamera == null && cameraTransform != null)
            fovCamera = cameraTransform.GetComponentInChildren<Camera>();

        if (fovCamera == null)
            fovCamera = Camera.main;

        if (fovCamera != null)
        {
            cameraTransform = fovCamera.transform;
            startingFov = fovCamera.fieldOfView;
        }
    }
}
