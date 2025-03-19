using UnityEngine;

/// <summary>
/// Handles player movement and camera following in a top-down view
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;           // Base movement speed
    [SerializeField] private float acceleration = 2f;       // How quickly the player reaches max speed
    [SerializeField] private float deceleration = 2f;       // How quickly the player slows down
    
    [Header("Camera Settings")]
    [SerializeField] private float cameraSmoothSpeed = 0.125f;  // How smoothly the camera follows the player
    [SerializeField] private Vector3 cameraOffset = new Vector3(0, 0, -10f);  // Camera offset from player

    // Private variables
    private Vector2 moveInput;                              // Raw input from player
    private Vector2 currentVelocity;                        // Current movement velocity
    private Camera mainCamera;                             // Reference to the main camera

    private void Start()
    {
        // Get reference to main camera
        mainCamera = Camera.main;
        
        // Ensure the player has the correct tag for terrain generation
        if (!gameObject.CompareTag("Player"))
        {
            Debug.LogWarning("Player object is not tagged as 'Player'. Adding tag...");
            gameObject.tag = "Player";
        }
    }

    private void Update()
    {
        // Get input from player
        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
    }

    private void FixedUpdate()
    {
        // Calculate target velocity based on input
        Vector2 targetVelocity = moveInput * moveSpeed;

        // Smoothly interpolate current velocity towards target velocity
        if (moveInput != Vector2.zero)
        {
            // Accelerate
            currentVelocity = Vector2.Lerp(currentVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
        }
        else
        {
            // Decelerate
            currentVelocity = Vector2.Lerp(currentVelocity, Vector2.zero, deceleration * Time.fixedDeltaTime);
        }

        // Apply movement
        Vector2 movement = currentVelocity * Time.fixedDeltaTime;
        transform.Translate(movement, Space.World);

        // Update camera position
        UpdateCamera();
    }

    /// <summary>
    /// Smoothly moves the camera to follow the player
    /// </summary>
    private void UpdateCamera()
    {
        if (mainCamera != null)
        {
            Vector3 desiredPosition = transform.position + cameraOffset;
            Vector3 smoothedPosition = Vector3.Lerp(mainCamera.transform.position, desiredPosition, cameraSmoothSpeed);
            mainCamera.transform.position = smoothedPosition;
        }
    }
} 