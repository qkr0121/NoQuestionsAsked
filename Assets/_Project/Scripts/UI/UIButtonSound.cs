using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 버튼에 붙이면 클릭·호버 소리가 남 - 소리를 지정하지 않으면 AudioManager의 기본 소리
[RequireComponent(typeof(Button))]
public class UIButtonSound : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] private SoundData clickOverride;
    [SerializeField] private bool playHover = true;   // 서류의 큰 투명 줄 버튼처럼 호버 소리가 시끄러운 곳은 끔

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(PlayClick);
    }

    private void PlayClick()
    {
        if (AudioManager.Instance == null) return;
        AudioManager.Instance.Play(clickOverride != null ? clickOverride : AudioManager.Instance.DefaultClick);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!playHover || AudioManager.Instance == null || !button.interactable) return;
        AudioManager.Instance.Play(AudioManager.Instance.DefaultHover);
    }
}
