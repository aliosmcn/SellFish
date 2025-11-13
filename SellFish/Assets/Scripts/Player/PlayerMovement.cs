using UnityEngine;
using UnityEngine.InputSystem;

// Player objesinde CharacterController olmasını zorunlu kılar
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;
    private Vector2 moveInput;
    
    public float speed = 8f;
    
    // Yerçekimi simülasyonu için
    private float gravity = -9.81f; 
    private Vector3 velocity;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Girdiyi (moveInput) 3D bir yöne çevir
        // transform.right ve transform.forward kullanarak yerel koordinatlara göre hareket
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        
        // Hızla çarp
        controller.Move(move * speed * Time.deltaTime);

        // Yerçekimi uygula
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Yerdeyken hafif bir kuvvet (daha stabil)
        }
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    // BU FONKSİYON Player Input komponenti tarafından OTOMATİK ÇAĞRILACAK (Adım 6'da bağlayacağız)
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }
}