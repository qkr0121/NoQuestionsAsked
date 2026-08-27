using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    private static bool initialized;

    [SerializeField] private GameObject persistentManagersPrefab;

    private void Awake()
    {
        if(initialized)
        {
            Destroy(gameObject);
            return;
        }

        initialized = true;
        DontDestroyOnLoad(Instantiate(persistentManagersPrefab));
    }
}
