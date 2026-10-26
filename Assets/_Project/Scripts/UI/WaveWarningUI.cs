using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WaveWarningUI : MonoBehaviour
{
    [SerializeField] private WaveSpawner waveSpawner;
    [SerializeField] private GameObject root;         // 처음에 꺼진 자식 - 이 스크립트를 붙인 오브젝트의 자식이어야 함(자기 자신을 끄면 구독이 끊김)
    [SerializeField] private Image redOverlay;        // 전체 화면 빨간 이미지 (Raycast Target 끄기)
    [SerializeField] private float maxAlpha = 0.35f;
    [SerializeField] private float pulseSpeed = 3f;   // 깜빡임 속도
    [SerializeField] private float sirenLoopSeconds = 2.598f;   // 반복용 사이렌(Siren_Loop.wav, 삑 4번)의 길이 - 이 간격으로 다시 시작해야 박자가 끊기지 않음

    private Coroutine routine;

    private void Awake()
    {
        root.SetActive(false);
    }

    private void OnEnable()
    {
        if (waveSpawner != null) waveSpawner.OnWarning += Show;
    }

    private void OnDisable()
    {
        if (waveSpawner != null) waveSpawner.OnWarning -= Show;
    }

    private void Show(float duration)
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(ShowRoutine(duration));
    }

    private IEnumerator ShowRoutine(float duration)
    {
        root.SetActive(true);
        Color color = redOverlay.color;
        float elapsed = 0f;

        AudioManager.PlayWarning();
        float nextSoundTime = sirenLoopSeconds;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            // 남은 시간이 사이렌의 절반보다 짧으면 반복하지 않음 - 경고가 끝났는데 소리만 남는 것을 막음
            if (elapsed >= nextSoundTime && duration - elapsed > sirenLoopSeconds * 0.5f)
            {
                AudioManager.PlayWarning();
                nextSoundTime += sirenLoopSeconds;
            }

            // 사인파로 깜빡이며 경고임을 강조
            float alpha = maxAlpha * (0.5f + 0.5f * Mathf.Sin(elapsed * pulseSpeed * Mathf.PI));
            redOverlay.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        root.SetActive(false);
        routine = null;
    }
}
