using UnityEngine;

/// <summary>
/// Modular Player Movement System (Refactored)
/// Supports different movement types and is multiplayer-ready
/// </summary>
public class PlayerMovementRefactored : MonoBehaviour
{
    [Header("Movement Configuration")]
    [SerializeField] private float baseSpeed = 6f;
    [SerializeField] private float currentSpeed;
    [SerializeField] private MovementType movementType = MovementType.TopDown;
    
    [Header("Rotation Settings")]
    [SerializeField] private bool useMouseRotation = true;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private LayerMask floorMask = -1;
    [SerializeField] private float raycastDistance = 100f;
    
    [Header("Physics")]
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 10f;
    [SerializeField] private bool usePhysics = true;
    
    public enum MovementType
    {
        TopDown,
        FirstPerson,
        ThirdPerson
    }
    
    // Components
    private ComponentCache componentCache;
    private Rigidbody playerRigidbody;
    private Animator animator;
    private Camera playerCamera;
    
    // Input
    private Vector2 inputVector;
    private Vector3 movementVector;
    private bool isMoving;
    
    // State
    private Vector3 lastMovementDirection;
    private float currentVelocityMagnitude;
    
    // Properties
    public float CurrentSpeed => currentSpeed;
    public bool IsMoving => isMoving;
    public Vector3 MovementDirection => movementVector.normalized;
    public float VelocityMagnitude => currentVelocityMagnitude;
    public MovementType CurrentMovementType => movementType;
    
    // Events
    public System.Action<Vector3> OnMovementStarted;
    public System.Action OnMovementStopped;
    public System.Action<float> OnSpeedChanged;
    
    void Awake()
    {
        InitializeComponents();
        InitializeMovement();
    }
    
    void Start()
    {
        SetupMovementType();
        SubscribeToEvents();
    }
    
    void Update()
    {
        HandleInput();
        HandleRotation();
    }
    
    void FixedUpdate()
    {
        HandleMovement();
        UpdateAnimator();
    }
    
    void OnDestroy()
    {
        UnsubscribeFromEvents();
    }
    
    private void InitializeComponents()
    {
        componentCache = GetComponent<ComponentCache>();
        if (componentCache == null)
            componentCache = gameObject.AddComponent<ComponentCache>();
            
        playerRigidbody = componentCache.GetCachedComponent<Rigidbody>();
        animator = componentCache.GetCachedComponent<Animator>();
        
        // Get main camera if no specific camera assigned
        if (playerCamera == null)
            playerCamera = Camera.main;
    }
    
    private void InitializeMovement()
    {
        currentSpeed = baseSpeed;
        
        if (floorMask == -1)
            floorMask = LayerMask.GetMask("Floor");
    }
    
    private void SetupMovementType()
    {
        switch (movementType)
        {
            case MovementType.TopDown:
                useMouseRotation = true;
                break;
            case MovementType.FirstPerson:
                useMouseRotation = false;
                break;
            case MovementType.ThirdPerson:
                useMouseRotation = false;
                break;
        }
    }
    
    private void SubscribeToEvents()
    {
        // Subscribe to player health events for movement modifications
        var playerHealth = GetComponent<PlayerHealthRefactored>();
        if (playerHealth != null)
        {
            playerHealth.OnSprintStateChanged += HandleSprintStateChanged;
        }
    }
    
    private void UnsubscribeFromEvents()
    {
        var playerHealth = GetComponent<PlayerHealthRefactored>();
        if (playerHealth != null)
        {
            playerHealth.OnSprintStateChanged -= HandleSprintStateChanged;
        }
    }
    
    private void HandleSprintStateChanged(bool isSprinting)
    {
        // Speed is now controlled by PlayerHealth system
        // This method can be used for sprint-specific movement effects
    }
    
    private void HandleInput()
    {
        // Get raw input for immediate response
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        
        inputVector = new Vector2(horizontal, vertical);
        
        // Normalize diagonal movement
        if (inputVector.magnitude > 1f)
            inputVector = inputVector.normalized;
        
        // Check if we started or stopped moving
        bool wasMoving = isMoving;
        isMoving = inputVector.magnitude > 0.1f;
        
        if (isMoving && !wasMoving)
        {
            OnMovementStarted?.Invoke(movementVector);
        }
        else if (!isMoving && wasMoving)
        {
            OnMovementStopped?.Invoke();
        }
    }
    
    private void HandleRotation()
    {
        if (!useMouseRotation || movementType != MovementType.TopDown)
            return;
            
        // Raycast from camera to floor
        Ray cameraRay = playerCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit floorHit;
        
        if (Physics.Raycast(cameraRay, out floorHit, raycastDistance, floorMask))
        {
            Vector3 playerToMouse = floorHit.point - transform.position;
            playerToMouse.y = 0f;
            
            if (playerToMouse.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(playerToMouse);
                
                if (usePhysics && playerRigidbody != null)
                {
                    playerRigidbody.MoveRotation(Quaternion.Slerp(playerRigidbody.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));
                }
                else
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                }
            }
        }
    }
    
    private void HandleMovement()
    {
        // Calculate movement based on input
        if (movementType == MovementType.TopDown)
        {
            movementVector = new Vector3(inputVector.x, 0f, inputVector.y);
        }
        else
        {
            // For first/third person, move relative to camera or player forward
            Vector3 forward = transform.forward;
            Vector3 right = transform.right;
            
            movementVector = (forward * inputVector.y + right * inputVector.x);
        }
        
        // Apply speed
        movementVector = movementVector.normalized * currentSpeed;
        
        // Apply movement
        if (usePhysics && playerRigidbody != null)
        {
            Vector3 targetPosition = transform.position + movementVector * Time.fixedDeltaTime;
            playerRigidbody.MovePosition(targetPosition);
            currentVelocityMagnitude = playerRigidbody.velocity.magnitude;
        }
        else
        {
            transform.position += movementVector * Time.fixedDeltaTime;
            currentVelocityMagnitude = movementVector.magnitude;
        }
        
        // Store movement direction
        if (movementVector.magnitude > 0.1f)
        {
            lastMovementDirection = movementVector.normalized;
        }
    }
    
    private void UpdateAnimator()
    {
        if (animator == null)
            return;
            
        // Update walking animation
        animator.SetBool("IsWalking", isMoving);
        
        // Update movement speed for blend trees
        animator.SetFloat("Speed", currentVelocityMagnitude);
        
        // Update movement direction for directional animations
        if (movementType != MovementType.TopDown)
        {
            animator.SetFloat("Horizontal", inputVector.x);
            animator.SetFloat("Vertical", inputVector.y);
        }
    }
    
    public void SetMoveSpeed(float newSpeed)
    {
        if (currentSpeed != newSpeed)
        {
            currentSpeed = newSpeed;
            OnSpeedChanged?.Invoke(currentSpeed);
        }
    }
    
    public void SetMovementType(MovementType newType)
    {
        movementType = newType;
        SetupMovementType();
    }
    
    public void ModifySpeed(float multiplier)
    {
        SetMoveSpeed(baseSpeed * multiplier);
    }
    
    public void ResetSpeed()
    {
        SetMoveSpeed(baseSpeed);
    }
    
    public void Stop()
    {
        inputVector = Vector2.zero;
        movementVector = Vector3.zero;
        isMoving = false;
        
        if (usePhysics && playerRigidbody != null)
        {
            playerRigidbody.velocity = Vector3.zero;
        }
    }
    
    public void Teleport(Vector3 position)
    {
        if (usePhysics && playerRigidbody != null)
        {
            playerRigidbody.position = position;
        }
        else
        {
            transform.position = position;
        }
    }
    
    // Multiplayer support methods
    public Vector3 GetPredictedPosition(float deltaTime)
    {
        return transform.position + movementVector * deltaTime;
    }
    
    public void SetPositionAndRotation(Vector3 position, Quaternion rotation)
    {
        if (usePhysics && playerRigidbody != null)
        {
            playerRigidbody.position = position;
            playerRigidbody.rotation = rotation;
        }
        else
        {
            transform.position = position;
            transform.rotation = rotation;
        }
    }
    
    // Debug visualization
    void OnDrawGizmosSelected()
    {
        if (isMoving)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, lastMovementDirection * 2f);
        }
        
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, currentSpeed * 0.1f);
    }
}
