using UnityEngine;
using UnityEngine.SceneManagement;

public class BaseExitPoint : MonoBehaviour
{
    public void Exit()
    {
        SceneFader.Instance.FadeToScene("ZWD");
    }
}
