using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(InteractableStructure))]
public class BaseExitPoint : MonoBehaviour
{
    public void Exit()
    {
        ScreenFader.Instance.FadeAction(() => SceneManager.LoadScene("ZWD"));
        
    }
}
