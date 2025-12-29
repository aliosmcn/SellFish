using UnityEngine;
using UnityEngine.Events;

public class Interactor : MonoBehaviour
{
    [Header("Ayarlar")]
    [SerializeField] private Transform cameraPoint;
    [SerializeField] private float range = 3f;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private InputReader inputReader;

    public event UnityAction<IInteractable> OnInteractableChanged;

    private IInteractable _currentTarget; 

    private void OnEnable() => inputReader.InteractEvent += OnInteract;
    private void OnDisable() => inputReader.InteractEvent -= OnInteract;

    private void Update()
    {
        CheckForInteractable();
    }

    private void CheckForInteractable()
    {
        RaycastHit hit;
        var isHit = Physics.Raycast(cameraPoint.position, cameraPoint.forward, out hit, range, layerMask);

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