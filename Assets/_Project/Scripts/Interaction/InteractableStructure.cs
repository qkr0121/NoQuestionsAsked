using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class InteractableStructure : MonoBehaviour
{
    [SerializeField] private UnityEvent onInteract;
    [SerializeField] private GameObject interactPrompt;

    public void SetPromptVisible(bool visible)
    {
        if (interactPrompt != null) interactPrompt.SetActive(visible);
    }

    public void Interact()
    {
        Debug.Log("Interact");
        onInteract.Invoke();
    }
}
