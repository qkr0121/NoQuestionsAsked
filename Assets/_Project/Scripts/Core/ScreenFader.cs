using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance { get; private set; }

    [Header("페이드 연출")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 0.4f;

    private bool isFading;

    private void Awake()
    {
        Instance = this;
    }

    public void FadeAction(Action action)
    {
        if (isFading) return;
        isFading = true;

        StartCoroutine(FadeActionRoutine(action));
    }

    private IEnumerator FadeActionRoutine(Action action)
    {
        PlayerInputLock.Lock();

        yield return Fade(0f, 1f);
        action?.Invoke();
        yield return Fade(1f, 0f);

        PlayerInputLock.Unlock();
        isFading = false;
    }

    private IEnumerator Fade(float from, float to)
    {
        float elapsed = 0f;
        Color color = fadeImage.color;

        while(elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;   // 게임오버로 timeScale이 0이 되어도 페이드가 끝나도록
            float alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
            fadeImage.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        fadeImage.color = new Color(color.r, color.g, color.b, to);
    }
}
