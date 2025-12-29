using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Ayarlar")]
    [SerializeField] private InputReader _inputReader; 
    [SerializeField] private Transform _cameraTarget;  
    [SerializeField] private float _moveSpeed = 6f;    
    [SerializeField] private float _mouseSensitivity = 0.2f; 

    private float _gravity = -9.81f; 

    
    private CharacterController _characterController; 
    private Vector2 _movementInput; 
    private Vector2 _lookInput;     
    private float _cameraPitch = 0f; 
    private float _verticalVelocity; 

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Start()
    {
        _cameraTarget.localRotation = Quaternion.identity;
    }

    private void OnEnable()
    {
        _inputReader.EnableInput();

        _inputReader.MoveEvent += OnMove;
        _inputReader.LookEvent += OnLook;
    }

    private void OnDisable()
    {
        _inputReader.MoveEvent -= OnMove;
        _inputReader.LookEvent -= OnLook;

        _inputReader.DisableInput();
    }

    private void Update()
    {
        MoveCharacter();
        ApplyGravity();
    }

    private void LateUpdate()
    {
        RotateCamera();
    }


    private void MoveCharacter()
    {
        var movement = Vector3.zero;

        if (_movementInput != Vector2.zero)
        {
            movement = (transform.right * _movementInput.x + transform.forward * _movementInput.y).normalized;
        }

        _characterController.Move(movement * _moveSpeed * Time.deltaTime);
    }

    private void RotateCamera()
    {
        float mouseX = _lookInput.x * _mouseSensitivity;
        transform.Rotate(Vector3.up * mouseX);

        float mouseY = _lookInput.y * _mouseSensitivity;
        
        _cameraPitch -= mouseY;
        _cameraPitch = Mathf.Clamp(_cameraPitch, -90f, 90f);

        _cameraTarget.localRotation = Quaternion.Euler(_cameraPitch, 0f, 0f);
    }

    private void ApplyGravity()
    {
        if (_characterController.isGrounded && _verticalVelocity < 0)
        {
            _verticalVelocity = -2f; 
        }

        _verticalVelocity += _gravity * Time.deltaTime;

        _characterController.Move(Vector3.up * _verticalVelocity * Time.deltaTime);
    }


    private void OnMove(Vector2 input)
    {
        _movementInput = input;
    }

    private void OnLook(Vector2 input)
    {
        _lookInput = input;
    }
}