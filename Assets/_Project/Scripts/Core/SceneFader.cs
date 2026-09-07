using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneFader : MonoBehaviour
{
    public static SceneFader Instance { get; private set; }

    [Header("페이드 연출")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 0.4f;

    private bool isTransitioning;

    private void Awake()
    {
        Instance = this;
    }

    public void FadeToScene(string sceneName)
    {
        if (isTransitioning) return;
        isTransitioning = true;

        PlayerInputLock.Lock();
        StartCoroutine(FadeRoutine(sceneName));
    }

    private IEnumerator FadeRoutine(string sceneName)
    {
        yield return Fade(0f, 1f);
        SceneManager.LoadScene(sceneName);
        // 동기 로드 - Base/ZWD 는 가벼운 씬이라 로딩 지연이 거의 없음(로딩 화면 없이 처리)
        // 무거워지면 LoadingScreen 처럼 비동기 로드+진행바로 바꿀 필요 있음
        yield return Fade(1f, 0f);

        PlayerInputLock.Unlock();
        isTransitioning = false;
    }

    private IEnumerator Fade(float from, float to)
    {
        float elapsed = 0f;
        Color color = fadeImage.color;

        while(elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
            fadeImage.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        // 루프가 끝난 뒤 정확히 목표값으로 딱 맞춤(Time.deltaTime 누적 오차로 완전히 안 맞을 수 있어서 마지막에 확정)
        fadeImage.color = new Color(color.r, color.g, color.b, to);
    }
}
