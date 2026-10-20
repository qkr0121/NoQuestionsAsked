using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PaperCheckBox : MonoBehaviour
{
    [SerializeField] private Image checkImage;          // 칸 안의 붉은 V (평소 꺼 둠), Image Type은 Filled/Horizontal/Left
    [SerializeField] private Sprite[] checkSprites;     // 손으로 하나씩 그은 느낌이 나도록 번갈아 사용
    [SerializeField] private float drawDuration = 0.18f;

    private Coroutine routine;

    public bool IsChecked { get; private set; }

    private void Awake()
    {
        checkImage.enabled = false;
    }

    public void SetChecked(bool on, bool animate = true)
    {
        if (routine != null) StopCoroutine(routine);
        routine = null;
        IsChecked = on;

        if (!on)
        {
            checkImage.enabled = false;
            return;
        }

        // 칸마다 모양과 각도를 달리해 반복돼 보이지 않게 함
        if (checkSprites != null && checkSprites.Length > 0)
            checkImage.sprite = checkSprites[Random.Range(0, checkSprites.Length)];
        checkImage.rectTransform.localRotation = Quaternion.Euler(0f, 0f, Random.Range(-6f, 6f));
        checkImage.enabled = true;

        if (!animate)
        {
            checkImage.fillAmount = 1f;
            return;
        }
        routine = StartCoroutine(Draw());
    }

    private IEnumerator Draw()
    {
        checkImage.fillAmount = 0f;
        float elapsed = 0f;
        while (elapsed < drawDuration)
        {
            elapsed += Time.unscaledDeltaTime;   // 시간이 멈춘 화면(일시정지 등)에서도 그려지도록
            checkImage.fillAmount = Mathf.Clamp01(elapsed / drawDuration);
            yield return null;
        }
        checkImage.fillAmount = 1f;
        routine = null;
    }
}
