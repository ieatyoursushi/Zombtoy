using UnityEngine;

/// <summary>
/// Player Input Manager - Centralized input handling
/// Easily configurable for different control schemes and multiplayer
/// </summary>
public class PlayerInputManager : MonoBehaviour
{
    [Header("Input Configuration")]
    [SerializeField] private bool inputEnabled = true;
    [SerializeField] private bool allowPauseInput = true;
    
    [Header("Movement Input")]
    [SerializeField] private string horizontalAxis = "Horizontal";
    [SerializeField] private string verticalAxis = "Vertical";
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    
    [Header("Action Input")]
    [SerializeField] private string fireButton = "Fire1";
    [SerializeField] private string alternateFireButton = "Fire2";
    [SerializeField] private KeyCode reloadKey = KeyCode.R;
    [SerializeField] private KeyCode useKey = KeyCode.E;
    
    [Header("System Input")]
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
    [SerializeField] private KeyCode alternativePauseKey = KeyCode.Tab;
    
    // Input state
    private Vector2 movementInput;
    private Vector2 mouseInput;
    private bool fireInput;
    private bool fireInputDown;
    private bool alternateFireInput;
    private bool alternateFireInputDown;
    private bool sprintInput;
    private bool reloadInputDown;
    private bool useInputDown;
    private bool pauseInputDown;
    
    // Properties
    public Vector2 MovementInput => movementInput;
    public Vector2 MouseInput => mouseInput;
    public bool FireInput => fireInput;
    public bool FireInputDown => fireInputDown;
    public bool AlternateFireInput => alternateFireInput;
    public bool AlternateFireInputDown => alternateFireInputDown;
    public bool SprintInput => sprintInput;
    public bool ReloadInputDown => reloadInputDown;
    public bool UseInputDown => useInputDown;
    public bool PauseInputDown => pauseInputDown;
    public bool InputEnabled => inputEnabled;
    
    // Events
    public System.Action<Vector2> OnMovementInput;
    public System.Action OnFirePressed;
    public System.Action OnFireReleased;
    public System.Action OnAlternateFirePressed;
    public System.Action OnReloadPressed;
    public System.Action OnUsePressed;
    public System.Action OnPausePressed;
    public System.Action<bool> OnSprintChanged;
    
    void Update()
    {
        if (!inputEnabled)
        {
            ClearAllInput();
            return;
        }
        
        HandleMovementInput();
        HandleMouseInput();
        HandleActionInput();
        HandleSystemInput();
    }
    
    private void HandleMovementInput()
    {
        // Get movement input
        float horizontal = Input.GetAxisRaw(horizontalAxis);
        float vertical = Input.GetAxisRaw(verticalAxis);
        Vector2 newMovement = new Vector2(horizontal, vertical);
        
        if (newMovement != movementInput)
        {
            movementInput = newMovement;
            OnMovementInput?.Invoke(movementInput);
        }
        
        // Handle sprint input
        bool newSprint = Input.GetKey(sprintKey);
        if (newSprint != sprintInput)
        {
            sprintInput = newSprint;
            OnSprintChanged?.Invoke(sprintInput);
        }
    }
    
    private void HandleMouseInput()
    {
        mouseInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
    }
    
    private void HandleActionInput()
    {
        // Fire input
        bool newFire = Input.GetButton(fireButton);
        bool newFireDown = Input.GetButtonDown(fireButton);
        
        if (newFire != fireInput)
        {
            fireInput = newFire;
            if (fireInput)
                OnFirePressed?.Invoke();
            else
                OnFireReleased?.Invoke();
        }
        fireInputDown = newFireDown;
        
        // Alternate fire input
        alternateFireInput = Input.GetButton(alternateFireButton);
        alternateFireInputDown = Input.GetButtonDown(alternateFireButton);
        if (alternateFireInputDown)
        {
            OnAlternateFirePressed?.Invoke();
        }
        
        // Reload input
        reloadInputDown = Input.GetKeyDown(reloadKey);
        if (reloadInputDown)
        {
            OnReloadPressed?.Invoke();
        }
        
        // Use input
        useInputDown = Input.GetKeyDown(useKey);
        if (useInputDown)
        {
            OnUsePressed?.Invoke();
        }
    }
    
    private void HandleSystemInput()
    {
        if (!allowPauseInput)
            return;
            
        pauseInputDown = Input.GetKeyDown(pauseKey) || Input.GetKeyDown(alternativePauseKey);
        if (pauseInputDown)
        {
            OnPausePressed?.Invoke();
        }
    }
    
    private void ClearAllInput()
    {
        movementInput = Vector2.zero;
        mouseInput = Vector2.zero;
        fireInput = false;
        fireInputDown = false;
        alternateFireInput = false;
        alternateFireInputDown = false;
        sprintInput = false;
        reloadInputDown = false;
        useInputDown = false;
        pauseInputDown = false;
    }
    
    public void EnableInput()
    {
        inputEnabled = true;
    }
    
    public void DisableInput()
    {
        inputEnabled = false;
        ClearAllInput();
    }
    
    public void SetInputEnabled(bool enabled)
    {
        inputEnabled = enabled;
        if (!enabled)
        {
            ClearAllInput();
        }
    }
    
    // Configuration methods for different control schemes
    public void ConfigureForKeyboardMouse()
    {
        sprintKey = KeyCode.LeftShift;
        reloadKey = KeyCode.R;
        useKey = KeyCode.E;
        pauseKey = KeyCode.Escape;
    }
    
    public void ConfigureForController()
    {
        // This would be expanded for gamepad support
        fireButton = "joystick button 0";
        alternateFireButton = "joystick button 1";
    }
    
    // Multiplayer support - can be used to apply remote input
    public void SetRemoteInput(Vector2 movement, bool fire, bool altFire, bool sprint)
    {
        movementInput = movement;
        fireInput = fire;
        alternateFireInput = altFire;
        sprintInput = sprint;
    }
    
    // Debugging
    void OnGUI()
    {
        if (!Debug.isDebugBuild)
            return;
            
        if (GUI.Button(new Rect(10, 10, 100, 30), "Toggle Input"))
        {
            SetInputEnabled(!inputEnabled);
        }
    }
}
