using UnityEngine;

// Bu scriptin çalışması için objede kesinlikle CharacterController olmalı diyoruz.
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InputReader _inputReader; // Input kaynağımız
    [SerializeField] private Transform _cameraTarget;  // Kamera (Kafamız)

    [Header("Settings")]
    [SerializeField] private float _moveSpeed = 6f;
    [SerializeField] private float _mouseSensitivity = 15f;
    [SerializeField] private float _gravity = -9.81f;

    private CharacterController _characterController;
    private Vector2 _currentMovementInput;
    private Vector2 _currentLookInput;
    private float _cameraPitch = 0f; // Yukarı/Aşağı bakış açısı
    private float _verticalVelocity; // Düşme hızı

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        
        // Mouse imlecini gizle ve kilitle
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnEnable()
    {
        // 1. ÖNCE Input sistemini uyandır (Burası yeni)
        _inputReader.EnableInput();

        // 2. Sonra eventlere abone ol
        _inputReader.MoveEvent += OnMove;
        _inputReader.LookEvent += OnLook;
    }

    private void OnDisable()
    {
        // Event aboneliğini bırak
        _inputReader.MoveEvent -= OnMove;
        _inputReader.LookEvent -= OnLook;

        // Input sistemini kapat (Temiz bırakalım)
        _inputReader.DisableInput();
    }

    private void Update()
    {
        HandleMovement();
        HandleGravity();
    }

    private void LateUpdate()
    {
        // Kamera hareketleri titrememesi için LateUpdate'de yapılır
        HandleLook();
    }

    // --- Logic ---

    private void HandleMovement()
    {
        // Karakterin baktığı yöne göre hareket vektörünü hesapla
        // transform.right = Sağ/Sol, transform.forward = İleri/Geri
        Vector3 moveDirection = transform.right * _currentMovementInput.x + transform.forward * _currentMovementInput.y;
        
        // CharacterController ile hareket ettir (Time.deltaTime ile kare hızından bağımsız)
        _characterController.Move(moveDirection * _moveSpeed * Time.deltaTime);
    }

    private void HandleLook()
    {
        // 1. Sağa/Sola Dönüş (Gövde döner)
        float mouseX = _currentLookInput.x * _mouseSensitivity * Time.deltaTime;
        transform.Rotate(Vector3.up * mouseX);

        // 2. Yukarı/Aşağı Bakış (Sadece kamera döner)
        float mouseY = _currentLookInput.y * _mouseSensitivity * Time.deltaTime;
        
        _cameraPitch -= mouseY;
        _cameraPitch = Mathf.Clamp(_cameraPitch, -90f, 90f); // Boyun kırma sınırı :)

        _cameraTarget.localRotation = Quaternion.Euler(_cameraPitch, 0f, 0f);
    }

    private void HandleGravity()
    {
        if (_characterController.isGrounded && _verticalVelocity < 0)
        {
            _verticalVelocity = -2f; // Yere yapışık kalması için küçük bir kuvvet
        }

        _verticalVelocity += _gravity * Time.deltaTime;
        _characterController.Move(Vector3.up * _verticalVelocity * Time.deltaTime);
    }

    // --- Event Listeners ---
    // InputReader'dan gelen verileri değişkenlere kaydediyoruz
    private void OnMove(Vector2 movement)
    {
        _currentMovementInput = movement;
    }

    private void OnLook(Vector2 lookDelta)
    {
        _currentLookInput = lookDelta;
    }
}