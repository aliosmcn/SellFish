using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "InputReader", menuName = "Game/Input Reader")]
public class InputReader : ScriptableObject, GameInput.IGameplayActions
{
    // --- Olaylar (Events) ---
    public event UnityAction<Vector2> MoveEvent;
    public event UnityAction<Vector2> LookEvent;
    
    // E Tuşu (Bağlamsal Etkileşim)
    public event UnityAction InteractEvent; 
    
    // Sol Tık (Tutma / Bırakma / Kullanma)
    public event UnityAction PrimaryActionEvent; 
    public event UnityAction PrimaryActionCancelledEvent; // Tuşu bırakınca (gerekirse)

    // Sağ Tık (Fırlatma / İptal)
    public event UnityAction SecondaryActionEvent;

    // Scroll (Döndürme - Float değeri döner: +120 veya -120 gibi)
    public event UnityAction<float> RotateEvent;

    public event UnityAction<bool> RunEvent; 

    private GameInput _gameInput;

    private void OnEnable()
    {
        if (_gameInput == null)
        {
            _gameInput = new GameInput();
            _gameInput.Gameplay.SetCallbacks(this);
            _gameInput.Gameplay.Enable();
        }
    }

    private void OnDisable()
    {
        _gameInput?.Gameplay.Disable();
    }

    // --- Interface Uygulamaları ---

    public void OnMove(InputAction.CallbackContext context)
    {
        MoveEvent?.Invoke(context.ReadValue<Vector2>());
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        LookEvent?.Invoke(context.ReadValue<Vector2>());
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        // E tuşuna basıldığı an
        if (context.phase == InputActionPhase.Performed)
            InteractEvent?.Invoke();
    }

    public void OnPrimaryAction(InputAction.CallbackContext context)
    {
        // Sol Tık basıldı
        if (context.phase == InputActionPhase.Performed)
            PrimaryActionEvent?.Invoke();
        
        // Sol Tık bırakıldı (Eşya tutma mekaniği için gerekebilir)
        if (context.phase == InputActionPhase.Canceled)
            PrimaryActionCancelledEvent?.Invoke();
    }

    public void OnSecondaryAction(InputAction.CallbackContext context)
    {
        // Sağ Tık basıldı
        if (context.phase == InputActionPhase.Performed)
            SecondaryActionEvent?.Invoke();
    }

    public void OnRotateItem(InputAction.CallbackContext context)
    {
        // Scroll çevrildiğinde çalışır.
        // Değer 0 değilse (yani bir hareket varsa) eventi tetikle.
        if (context.performed)
        {
            float scrollValue = context.ReadValue<float>();
            if (scrollValue != 0)
            {
                RotateEvent?.Invoke(scrollValue);
            }
        }
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
            RunEvent?.Invoke(true);
        else if (context.phase == InputActionPhase.Canceled)
            RunEvent?.Invoke(false);
    }
}