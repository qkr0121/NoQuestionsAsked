using UnityEngine;

[RequireComponent(typeof(InteractableStructure))]
public class BaseExitPoint : MonoBehaviour
{
    public void Exit()
    {
        SceneFader.Instance.FadeToScene("ZWD");
    }
}
