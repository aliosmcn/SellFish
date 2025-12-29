using UnityEngine;
using UnityEngine.Events;

public class Interactor : MonoBehaviour
{
    [Header("Ayarlar")]
    [SerializeField] private Transform _cameraPoint;
    [SerializeField] private float _range = 3f;
    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private InputReader _inputReader;

    public event UnityAction<IInteractable> OnInteractableChanged;

    private IInteractable _currentTarget; 

    private void OnEnable() => _inputReader.InteractEvent += OnInteract;
    private void OnDisable() => _inputReader.InteractEvent -= OnInteract;

    private void Update()
    {
        CheckForInteractable();
    }

    private void CheckForInteractable()
    {
        RaycastHit hit;
        var isHit = Physics.Raycast(_cameraPoint.position, _cameraPoint.forward, out hit, _range, _layerMask);

        IInteractable newTarget = null;

        if (isHit)
        {
            hit.collider.TryGetComponent(out newTarget);
        }

        if (newTarget != _currentTarget)
        {
            _currentTarget = newTarget;
            OnInteractableChanged?.Invoke(_currentTarget);
        }
    }

    private void OnInteract()
    {
        if (_currentTarget != null)
        {
            _currentTarget.Interact();
        }
    }
}