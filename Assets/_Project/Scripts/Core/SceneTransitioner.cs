using UnityEngine.SceneManagement;

public static class SceneTransitioner
{
    public static void GoTo(string sceneName)
    {
        // 목표 씬으로 바로 전환하지 않고 Loading 씬을 항상 거침 -
        // LoadingScreen 이 비동기 로드 + 진행바를 담당(무건운 전환용). 가벼운 Base<->ZWD 전환은 SceneFader 사용.
        SceneTransitionRequest.Set(sceneName);
        SceneManager.LoadScene("Loading");
    }
}
