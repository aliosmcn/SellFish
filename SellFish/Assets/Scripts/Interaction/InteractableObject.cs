using UnityEngine;
using UnityEngine.Events;

public class InteractableObject : MonoBehaviour, IInteractable
{
    [SerializeField] private string promptText = "example";
    [SerializeField] private UnityEvent onInteract;

    public string GetInteractText()
    {
        return promptText;
    }

    public void Interact()
    {
        onInteract?.Invoke();
    }
}