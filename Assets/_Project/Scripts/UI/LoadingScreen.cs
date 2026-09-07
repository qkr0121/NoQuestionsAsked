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
        PlayerInputLock.Lock();
        StartCoroutine(LoadTargetScene());
    }

    private IEnumerator LoadTargetScene()
    {
        if(PendingRunReset.ShouldReset)
        {
            RunState.Instance.ResetEntireRun();
            PendingRunReset.Reset();
        }

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
                /* Unlock 을 여기(씬 전환 직전)에서 호출 - while 루프 끝(코루틴 마지막)에 두면
                 * allowSceneActivation=true 로 씬이 전환되는 순간 이 오브젝트 자체가 파괴되어
                 * 코루틴이 중간에 강제 종료되고 Unlock()이 아예 실행되지 않는 버그가 있었음(입력 영구 잠김)
                 * 지금은 Loading 씬에 별도 페이드 연출이 없어서 "잠금 해제-실제 전환" 사이 짧은 시차가 무해하지만,
                 * 나중에 Loading 쪽에도 페이드 등 연출이 추가되면 이 타이밍을 다시 검토해야 함*/
                PlayerInputLock.Unlock();
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
