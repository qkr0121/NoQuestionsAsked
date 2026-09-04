using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class InteractableStructure : MonoBehaviour
{
    [SerializeField] private UnityEvent onInteract; // 인스펙터에서 코드 수정 없이 상호작용 대상을 바꿀 수 있게 하기 위함
    [SerializeField] private GameObject interactPrompt;

    public void SetPromptVisible(bool visible)
    {
        if (interactPrompt != null) interactPrompt.SetActive(visible);
    }

    public void Interact()
    {
        onInteract.Invoke();
    }
}
