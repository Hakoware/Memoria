using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FlyCamera : MonoBehaviour
{
    public float acceleration = 50; // how fast you accelerate
    public float accSprintMultiplier = 4; // how much faster you go when "sprinting"
    public float lookSensitivity = 1; // mouse look sensitivity
    public float dampingCoefficient = 5; // how quickly you break to a halt after you stop your input
    public KeyCode lockCameraKey = KeyCode.C; // key to lock the camera

    private Vector3 velocity; // Actual velocity
    private bool cameraLocked = false; // Camera locking status
    private bool isRotating = false; // Check rotationg (mouse wheel)

    void Update()
    {
        // Key input to lock the camera
        if (Input.GetKeyDown(lockCameraKey))
        {
            cameraLocked = !cameraLocked; // Camera status
        }
        // Camera unlock --> movement
        if (!cameraLocked)
        {
            UpdateInput();

            // Camera physics (motion smoothing)
            velocity = Vector3.Lerp(velocity, Vector3.zero, dampingCoefficient * Time.deltaTime);
            transform.position += velocity * Time.deltaTime;
        }

        // Handle rotation
        if (Input.GetMouseButtonDown(2))
        {
            isRotating = true;
            Cursor.visible = false; // Hide cursor while ratating
            Cursor.lockState = CursorLockMode.Locked; // Block the cursor
        }

        if (Input.GetMouseButtonUp(2))
        {
            isRotating = false;
            Cursor.visible = true; // Displays cursor when the mouse wheel is released
            Cursor.lockState = CursorLockMode.None; // Unlock the cursor
        }
    }

    void UpdateInput()
    {
        // Accelerate based on keyboard input direction
        velocity += GetAccelerationVector() * Time.deltaTime;

        // Only allow rotation if the mouse wheel is held down.
        if (isRotating)
        {
            Vector2 mouseDelta = lookSensitivity * new Vector2(Input.GetAxis("Mouse X"), -Input.GetAxis("Mouse Y"));
            Quaternion rotation = transform.rotation;
            Quaternion horiz = Quaternion.AngleAxis(mouseDelta.x, Vector3.up);
            Quaternion vert = Quaternion.AngleAxis(mouseDelta.y, Vector3.right);
            transform.rotation = horiz * rotation * vert;
        }
    }

    Vector3 GetAccelerationVector()
    {
        Vector3 moveInput = default;

        void AddMovement(KeyCode key, Vector3 dir)
        {
            if (Input.GetKey(key))
                moveInput += dir;
        }

        AddMovement(KeyCode.W, Vector3.forward);
        AddMovement(KeyCode.S, Vector3.back);
        AddMovement(KeyCode.D, Vector3.right);
        AddMovement(KeyCode.A, Vector3.left);
        AddMovement(KeyCode.Space, Vector3.up);
        AddMovement(KeyCode.LeftControl, Vector3.down);
        Vector3 direction = transform.TransformVector(moveInput.normalized);

        if (Input.GetKey(KeyCode.LeftShift))
            return direction * (acceleration * accSprintMultiplier); // Sprint
        return direction * acceleration; // normal walking
    }
}
