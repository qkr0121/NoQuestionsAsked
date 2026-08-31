using UnityEngine.SceneManagement;

public static class SceneTransitioner
{
    public static void GoTo(string sceneName)
    {
        SceneTransitionRequest.TargetSceneName = sceneName;
        SceneManager.LoadScene("Loading");
    }
}
