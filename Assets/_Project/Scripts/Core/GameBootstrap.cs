using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    // 각 씬 (MainMenu, Base, ZWD 등)에 이 컴포넌트가 하나씩 배치되어 있어,
    // 씬을 새로 로드할 때마다 Awake가 다시 호출됨.
    // PersistentManagers(DontDestoryOnLoad)는 최초 1회만 생성되어야 하므로,
    // 이미 생성된 적 있으면 이 GameBootstrap 자신만 파괴하고 넘어감.
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
        MetaSaveManager.Instance.LoadAndApply();
    }
}
