using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour {
    public Transform playerCamera = null;
    float mouseSensitivity = 1.0f;
    float camerPitch = 0f;
    bool lockCurser;
    Animator anim;
    public float playerSpeed;
    Rigidbody rb;
    Vector3 velocity;
    bool walking;
    [SerializeField] private Pause pause;
    [SerializeField] private Sensitivity sensitivity;
    // Use this for initialization
    private void Awake()
    {
        anim = GetComponent<Animator>();
        if (pause == null)
        {
            var hud = GameObject.Find("HUDCanvasd");
            if (hud != null) pause = hud.GetComponent<Pause>();
        }
        if (sensitivity == null)
        {
            var sens = GameObject.Find("Sensitivity");
            if (sens != null) sensitivity = sens.GetComponent<Sensitivity>();
        }
    }
    void Start () {
        Cursor.lockState = CursorLockMode.Locked;
        rb = GetComponent<Rigidbody>();
        mouseSensitivity = 1.0f;
	}
	
	// Update is called once per frame
	void Update () {
        MouseLook();
    if(Input.GetButtonDown("Fire1") && pause != null && !pause.isPaused)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    if (pause != null && pause.isPaused)
        {
            mouseSensitivity = Sensitivity.sensitivityValue / 100;
        }
    }
    void MouseLook()
    {
    if (pause == null || !pause.isPaused)
        {
            Vector2 mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")); // x rotates y direction, y rotates x direction
            camerPitch -= mouseDelta.y * mouseSensitivity;
            camerPitch = Mathf.Clamp(camerPitch, -90f, 90f);
            playerCamera.localEulerAngles = Vector3.right * camerPitch; // rotates player left & right
            transform.Rotate(Vector3.up * mouseDelta.x * mouseSensitivity);
        }
    }
    private void FixedUpdate()
    {
        Movement();
    }
    void Movement()
    {
        Vector2 movementInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        movementInput.Normalize();
        velocity = (transform.forward * movementInput.y + transform.right * movementInput.x) * playerSpeed * Time.deltaTime;
        rb.MovePosition(transform.position + velocity);
        walking = movementInput.x != 0f || movementInput.y != 0f;
        anim.SetBool("IsWalking", walking);
    }
 
}
