using UnityEngine;
using UnityEngine.SceneManagement;

public class BaseExitPoint : MonoBehaviour
{
    public void Exit()
    {
        SceneManager.LoadScene("ZWD");
    }
}
