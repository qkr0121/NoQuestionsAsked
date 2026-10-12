using UnityEngine;
using UnityEngine.Pool;

public class DamageNumberPool : MonoBehaviour
{
    public static DamageNumberPool Instance { get; private set; }

    [SerializeField] private DamageNumber prefab;
    [SerializeField] private int maxActive = 30;               // 대량 피격 시 화면이 숫자로 덮이지 않게 하는 상한
    [SerializeField] private float baseScale = 1f;             // 기준 깊이에서의 크기
    [SerializeField] private float referenceDepth = 14.4f;     // 기본 줌에서 카메라와 대상 사이 깊이 (offset 크기)
    [SerializeField] private float spread = 0.4f;              // 가로 흩뿌림 (기준 크기 기준)

    private ObjectPool<DamageNumber> pool;
    private int activeCount;

    private void Awake()
    {
        Instance = this;

        pool = new ObjectPool<DamageNumber>(
            createFunc: () =>
            {
                DamageNumber number = Instantiate(prefab, transform);
                number.SetPool(pool);
                return number;
            },
            actionOnGet: number => { number.gameObject.SetActive(true); activeCount++; },
            actionOnRelease: number => { number.gameObject.SetActive(false); activeCount--; },
            actionOnDestroy: number => Destroy(number.gameObject),
            defaultCapacity: 20,
            maxSize: 50);
    }

    public void Show(int amount, Vector3 worldPosition)
    {
        if (activeCount >= maxActive) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        // 원근 카메라라 멀수록 작아 보이므로, 카메라 앞쪽 깊이에 비례해 키워 화면 크기를 일정하게 유지함
        float depth = Mathf.Max(Vector3.Dot(worldPosition - cam.transform.position, cam.transform.forward), 0.1f);
        float scale = baseScale * depth / referenceDepth;

        // 같은 좀비를 연속으로 맞혀도 겹치지 않게 화면 가로 방향으로 흩뿌림
        Vector3 jitter = cam.transform.right * Random.Range(-spread, spread) * scale;

        DamageNumber number = pool.Get();
        number.Show(amount, worldPosition + jitter, cam.transform.rotation, scale);
    }
}
