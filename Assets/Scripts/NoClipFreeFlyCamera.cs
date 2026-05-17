using UnityEngine;

public class NoClipFreeFlyCamera : MonoBehaviour
{
    [Header("Hareket Hýzlarý")]
    public float movementSpeed = 10f;
    public float verticalMovementSpeed = 8f; // Q/E tuþlarý için
    public float sprintMultiplier = 3f;      // Shift tuþu basýlýyken hýzlanma

    [Header("Bakýþ (Mouse) Hassasiyeti")]
    public float mouseSensitivity = 2f;
    public float pitchLimit = 85f;          // Tepeden aþaðý ve yukarý bakma sýnýrý

    private float _yaw = 0f;
    private float _pitch = 0f;

    void Start()
    {
        // Mouse imlecini kilitle ve gizle
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // 1. Bakýþ Kontrolü (Mouse)
        _yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        _pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        _pitch = Mathf.Clamp(_pitch, -pitchLimit, pitchLimit);

        transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);

        // 2. Hareket Kontrolü (Keyboard WASD)
        float currentMovementSpeed = movementSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentMovementSpeed *= sprintMultiplier;
        }

        Vector3 moveInput = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) moveInput += transform.forward;
        if (Input.GetKey(KeyCode.S)) moveInput -= transform.forward;
        if (Input.GetKey(KeyCode.A)) moveInput -= transform.right;
        if (Input.GetKey(KeyCode.D)) moveInput += transform.right;

        // Dikey Hareket (Q/E)
        if (Input.GetKey(KeyCode.Q)) moveInput -= transform.up;
        if (Input.GetKey(KeyCode.E)) moveInput += transform.up;

        // Hareketi uygula
        transform.position += moveInput.normalized * currentMovementSpeed * Time.deltaTime;
    }

    void OnDisable()
    {
        // Script devre dýþý kalýrsa imleci serbest býrak
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}