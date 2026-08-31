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

            float loadProgress = Mathf.Clamp01(operation.progress / 0.9f);
            float timeProgress = Mathf.Clamp01(elapsed / minimumDisplayTime);
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
