using UnityEngine;
using TMPro;

public class InteractLabel : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private GameObject promptRoot;   // 가까이 갔을 때만 보이는 안내 줄
    [SerializeField] private Color normalNameColor = Color.white;
    [SerializeField] private Color dimmedNameColor = new Color(0.5f, 0.5f, 0.5f, 1f);

    private Camera cam;

    public void SetName(string text) => nameText.text = text;
    public void SetPrompt(string text) => promptText.text = text;
    public void SetPromptVisible(bool visible) => promptRoot.SetActive(visible);
    public void SetDimmed(bool dimmed) => nameText.color = dimmed ? dimmedNameColor : normalNameColor;

    private void LateUpdate()
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        // 탑다운 카메라는 각도가 고정이라 카메라와 같은 회전을 주면 항상 정면으로 읽힘
        transform.rotation = cam.transform.rotation;
    }
}
