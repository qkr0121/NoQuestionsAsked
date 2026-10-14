using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class InteractableStructure : MonoBehaviour
{
    public enum Availability { Always, NotInEscapePhase, OnlyInEscapePhase }

    [SerializeField] private UnityEvent onInteract; // 인스펙터에서 코드 수정 없이 상호작용 대상을 바꿀 수 있게 하기 위함
    [SerializeField] private Availability availability;   // 기본값 Always - 탈출 단계에 막을 건물(상점·침대 등)만 지정

    [Header("월드 라벨")]
    [SerializeField] private InteractLabel labelPrefab;
    [SerializeField] private string displayName;
    [SerializeField] private string promptText = "상호작용";
    [SerializeField] private Vector3 labelOffset = new Vector3(0f, 3f, 0f);   // 월드 단위 - 부모 스케일과 무관하게 적용됨

    private InteractLabel label;
    private bool playerNear;

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

    public void SetPromptText(string text) => promptText = text;   // 상태에 따라 안내 문구가 바뀌는 건축물용

    private void Awake()
    {
        if (labelPrefab == null) return;

        label = Instantiate(labelPrefab, transform);
        // 부모 스케일(로켓 2배 등)에 라벨 크기와 높이가 영향받지 않도록 보정
        Vector3 s = transform.lossyScale;
        label.transform.localPosition = new Vector3(labelOffset.x / s.x, labelOffset.y / s.y, labelOffset.z / s.z);
        Vector3 prefabScale = labelPrefab.transform.localScale;
        label.transform.localScale = new Vector3(prefabScale.x / s.x, prefabScale.y / s.y, prefabScale.z / s.z);
        label.SetName(displayName);
    }

    private void Update()
    {
        if (label == null) return;

        bool usable = IsUsable;
        label.SetDimmed(!usable);
        label.SetPromptVisible(playerNear);
        // 날짜가 바뀌어 사용 가능 여부가 달라져도 바로 반영됨
        label.SetPrompt(usable ? $"[E] {promptText}" : "지금은 사용할 수 없다");
    }

    public void SetPromptVisible(bool visible)
    {
        playerNear = visible;
    }

    public void Interact()
    {
        if (!IsUsable) return;
        onInteract.Invoke();
    }
}
