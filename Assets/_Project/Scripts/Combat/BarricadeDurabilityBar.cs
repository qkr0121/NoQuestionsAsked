using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BarricadeDurabilityBar : MonoBehaviour
{
    [SerializeField] private Image fillImage;            // Image Type = Filled (Horizontal)
    [SerializeField] private TMP_Text valueText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float showDuration = 3f;    // 마지막 피해 뒤 유지 시간
    [SerializeField] private float fadeDuration = 0.4f;
    [SerializeField] private Color fullColor = new Color(0.35f, 0.9f, 0.4f, 1f);
    [SerializeField] private Color emptyColor = new Color(1f, 0.25f, 0.2f, 1f);

    private float hideTimer;
    private Camera cam;

    private void Awake()
    {
        canvasGroup.alpha = 0f;
    }

    public void Show(int current, int max)
    {
        float ratio = max > 0 ? Mathf.Clamp01((float)current / max) : 0f;
        fillImage.fillAmount = ratio;
        fillImage.color = Color.Lerp(emptyColor, fullColor, ratio);
        valueText.text = $"{Mathf.Max(current, 0)}/{max}";

        hideTimer = showDuration + fadeDuration;
        canvasGroup.alpha = 1f;
    }

    public void Hide()
    {
        hideTimer = 0f;
        canvasGroup.alpha = 0f;
    }

    private void Update()
    {
        if (hideTimer <= 0f) return;

        hideTimer -= Time.deltaTime;
        canvasGroup.alpha = Mathf.Clamp01(hideTimer / fadeDuration);   // 유지 시간이 끝나면 서서히 사라짐
    }

    private void LateUpdate()
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        // 탑다운 카메라는 각도가 고정이라 카메라와 같은 회전을 주면 항상 정면으로 읽힘 (InteractLabel 과 동일)
        transform.rotation = cam.transform.rotation;
    }
}
