using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] private Slider progressBar;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private float minimumDisplayTime = 1.2f;

    private void Start()
    {
        StartCoroutine(LoadTargetScene());
    }

    private IEnumerator LoadTargetScene()
    {
        string targetScene = SceneTransitionRequest.TargetSceneName;

        AsyncOperation operation = SceneManager.LoadSceneAsync(targetScene);
        operation.allowSceneActivation = false;

        float elapsed = 0f;

        while(!operation.isDone)
        {
            elapsed += Time.deltaTime;

            // operation.progress 는 0~0.9 범위로만 올라가므로, 0.9 로 나눠서 0~1 범위로 재정규화
            float loadProgress = Mathf.Clamp01(operation.progress / 0.9f);
            float timeProgress = Mathf.Clamp01(elapsed / minimumDisplayTime);
            // 둘 중 더 느린 쪽(작은 값)을 표시 - 로드가 순식간에 끝나도 최소 시간만큼은 진행바가 천천히 차오르게
            float displayProgress = Mathf.Min(loadProgress, timeProgress);

            progressBar.value = displayProgress;
            progressText.text = $"{Mathf.RoundToInt(displayProgress * 100)}%";

            if(operation.progress >= 0.9f && elapsed >= minimumDisplayTime)
            {
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
