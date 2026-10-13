using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class InteractableStructure : MonoBehaviour
{
    public enum Availability { Always, NotInEscapePhase, OnlyInEscapePhase }

    [SerializeField] private UnityEvent onInteract; // 인스펙터에서 코드 수정 없이 상호작용 대상을 바꿀 수 있게 하기 위함
    [SerializeField] private GameObject interactPrompt;
    [SerializeField] private Availability availability;   // 기본값 Always - 탈출 단계에 막을 건물(상점·침대 등)만 지정

    private bool IsUsable
    {
        get
        {
            bool escape = RunState.Instance.IsEscapeReady;
            return availability switch
            {
                Availability.NotInEscapePhase => !escape,
                Availability.OnlyInEscapePhase => escape,
                _ => true
            };
        }
    }

    // 건물 위에 선 채로 침대에서 자면 날짜가 바뀌어도 프롬프트가 남으므로 매 프레임 확인
    private void Update()
    {
        if (interactPrompt != null && interactPrompt.activeSelf && !IsUsable) interactPrompt.SetActive(false);
    }

    public void SetPromptVisible(bool visible)
    {
        if (interactPrompt != null) interactPrompt.SetActive(visible && IsUsable);
    }

    public void Interact()
    {
        if (!IsUsable) return;
        onInteract.Invoke();
    }
}
