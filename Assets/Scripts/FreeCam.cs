using UnityEngine;

public class FreeCam : MonoBehaviour {
    /*
        const int MOUSE_LEFT_BUTTON = 0;
    */
    /*
        const int MOUSE_RIGHT_BUTTON = 1;
    */

    /// <summary>
    /// Mouse sensitivity.
    /// Default 0.3.
    /// </summary>
    public float MouseSensitivity = 0.3f;

    /// <summary>
    /// Keyboard sensitivity.
    /// Default 5.0.
    /// </summary>
    public float KeyboardSensitivity = 5.0f;

    /// <summary>
    /// Last position of the mouse.
    /// </summary>
    private Vector3 _lastMousePosition;

    private void Awake() {
        _lastMousePosition = new Vector3(0, 0, 0);
    }

    public float mouseSensitivity = 100.0f;
    public float clampAngle = 80.0f;

    private float rotY = 0.0f; // rotation around the up/y axis
    private float rotX = 0.0f; // rotation around the right/x axis

    void Start() {
        Vector3 _rot = transform.localRotation.eulerAngles;
        rotY = _rot.y;
        rotX = _rot.x;
    }

    void Update() {
        Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible = true;
        if (GameManager.IsPuzzleMode()) return;
        float _mouseX = Input.GetAxis("Mouse X");
        float _mouseY = -Input.GetAxis("Mouse Y");

        rotY += _mouseX * mouseSensitivity * Time.deltaTime;
        rotX += _mouseY * mouseSensitivity * Time.deltaTime;

        rotX = Mathf.Clamp(rotX, -clampAngle, clampAngle);

        Quaternion _localRotation = Quaternion.Euler(rotX, rotY, 0.0f);
        transform.rotation = _localRotation;

        HandleKeyboard();
    }

    private void HandleKeyboard() {
        Vector3 _position = new Vector3();

        // if (Input.GetKey(KeyCode.A)) _position += Vector3.down;
        // if (Input.GetKey(KeyCode.E)) _position += Vector3.up;
        if (Input.GetKey(KeyCode.Z)) _position += Vector3.forward;
        if (Input.GetKey(KeyCode.Q)) _position += Vector3.left;
        if (Input.GetKey(KeyCode.S)) _position += Vector3.back;
        if (Input.GetKey(KeyCode.D)) _position += Vector3.right;

        _position = _position * Time.deltaTime * KeyboardSensitivity;

        transform.Translate(_position);
    }
}