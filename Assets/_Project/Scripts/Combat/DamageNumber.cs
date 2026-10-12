using UnityEngine;
using UnityEngine.Pool;
using TMPro;

[RequireComponent(typeof(TextMeshPro))]
public class DamageNumber : MonoBehaviour
{
    [SerializeField] private float lifetime = 0.8f;
    [SerializeField] private float riseSpeed = 1.5f;   // 기준 크기에서 초당 상승 거리(월드 단위)

    private TextMeshPro text;
    private IObjectPool<DamageNumber> pool;
    private float timer;
    private float scale;

    private void Awake()
    {
        text = GetComponent<TextMeshPro>();
    }

    public void SetPool(IObjectPool<DamageNumber> pool)
    {
        this.pool = pool;
    }

    public void Show(int amount, Vector3 position, Quaternion rotation, float scale)
    {
        this.scale = scale;
        text.text = amount.ToString();
        text.alpha = 1f;
        transform.SetPositionAndRotation(position, rotation);
        transform.localScale = Vector3.one * scale;
        timer = 0f;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        // 상승 거리도 크기 보정과 같은 비율로 키워서, 화면에서 보이는 속도가 줌과 무관하게 일정함
        transform.position += Vector3.up * riseSpeed * scale * Time.deltaTime;
        text.alpha = Mathf.Clamp01(1f - timer / lifetime);

        if (timer >= lifetime) pool.Release(this);
    }
}
