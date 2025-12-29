using UnityEngine;
using TMPro; 

public class InteractionUI : MonoBehaviour
{
    [Header("Ayarlar")]
    [SerializeField] private TextMeshProUGUI promptText; 
    [SerializeField] private Interactor interactor;     

    private void Awake()
    {
        promptText.text = "";
        promptText.gameObject.SetActive(false); 
    }

    private void OnEnable()
    {
        interactor.OnInteractableChanged += UpdateDisplay;
    }

    private void OnDisable()
    {
        interactor.OnInteractableChanged -= UpdateDisplay;
    }

    private void UpdateDisplay(IInteractable interactable)
    {
        if (interactable != null)
        {
            promptText.gameObject.SetActive(true);
            promptText.text = interactable.GetInteractText() + " [E]"; 
        }
        else
        {
            promptText.gameObject.SetActive(false); 
            promptText.text = "";
        }
    }
}