using System;
using System.Collections;
using UnityEngine;

public class PaperStamp : MonoBehaviour
{
    [SerializeField] private RectTransform stamp;   // 도장 이미지(+글자 자식)의 루트
    [SerializeField] private CanvasGroup group;
    [SerializeField] private float startScale = 2.2f;
    [SerializeField] private float slamDuration = 0.12f;
    [SerializeField] private float holdTime = 0.45f;   // 찍힌 도장을 잠깐 보여 주는 시간
    [SerializeField] private SoundData stampSound;     // 도장이 종이에 닿는 순간 나는 소리, 비워도 동작

    private Coroutine routine;

    private void Awake()
    {
        Hide();
    }

    public void Hide()
    {
        if (routine != null) StopCoroutine(routine);
        routine = null;
        group.alpha = 0f;
        stamp.localScale = Vector3.one;
    }

    public void Play(Action onDone)
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(Slam(onDone));
    }

    private IEnumerator Slam(Action onDone)
    {
        stamp.localRotation = Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(-10f, 10f));

        // 크게 내려와 찍힘 (가속)
        float elapsed = 0f;
        while (elapsed < slamDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(elapsed / slamDuration);
            stamp.localScale = Vector3.one * Mathf.Lerp(startScale, 1f, k * k);
            group.alpha = Mathf.Clamp01(k * 3f);
            yield return null;
        }
        stamp.localScale = Vector3.one;
        group.alpha = 1f;
        AudioManager.PlaySfx(stampSound);

        // 찍힌 직후 살짝 눌렸다 돌아오는 느낌
        elapsed = 0f;
        while (elapsed < 0.08f)
        {
            elapsed += Time.unscaledDeltaTime;
            stamp.localScale = Vector3.one * (1f + 0.07f * Mathf.Sin(Mathf.Clamp01(elapsed / 0.08f) * Mathf.PI));
            yield return null;
        }
        stamp.localScale = Vector3.one;

        yield return new WaitForSecondsRealtime(holdTime);
        routine = null;
        onDone?.Invoke();
    }
}
