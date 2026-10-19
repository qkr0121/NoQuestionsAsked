using UnityEngine;

public class EnemyHitFeedback : MonoBehaviour
{
    [SerializeField] private Renderer[] renderers;      // 비워두면 자식의 렌더러를 자동으로 찾음
    [SerializeField] private Transform punchTarget;     // 맞을 때 잠깐 커지는 모델 루트, 비우면 펀치 없음 (헤비는 비움)
    [SerializeField] private Color flashColor = new Color(2.5f, 2.5f, 2.5f, 1f);   // 1보다 커서 텍스처 색이 하얗게 밝아짐
    [SerializeField] private float flashDuration = 0.08f;
    [SerializeField] private float punchScale = 0.08f;  // 순간 커지는 비율
    [SerializeField] private float punchDuration = 0.12f;

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    private MaterialPropertyBlock block;
    private Vector3 baseScale;
    private float flashTimer;
    private float punchTimer;

    private void Awake()
    {
        block = new MaterialPropertyBlock();
        if (renderers == null || renderers.Length == 0) renderers = GetComponentsInChildren<Renderer>(true);
        if (punchTarget != null) baseScale = punchTarget.localScale;
    }

    private void OnEnable()
    {
        // 풀에서 재사용될 때 이전 번쩍임과 크기 변화가 남지 않게 함
        flashTimer = 0f;
        punchTimer = 0f;
        ApplyFlash(false);
        if (punchTarget != null) punchTarget.localScale = baseScale;
    }

    public void Play()
    {
        flashTimer = flashDuration;
        punchTimer = punchDuration;
        ApplyFlash(true);
    }

    private void Update()
    {
        if (flashTimer > 0f)
        {
            flashTimer -= Time.deltaTime;
            if (flashTimer <= 0f) ApplyFlash(false);
        }

        if (punchTarget != null && punchTimer > 0f)
        {
            punchTimer -= Time.deltaTime;
            float t = Mathf.Clamp01(punchTimer / punchDuration);   // 1 -> 0
            punchTarget.localScale = baseScale * (1f + punchScale * t);
        }
    }

    private void ApplyFlash(bool on)
    {
        foreach (Renderer r in renderers)
        {
            if (on)
            {
                block.SetColor(BaseColorId, flashColor);
                r.SetPropertyBlock(block);
            }
            else
            {
                r.SetPropertyBlock(null);   // 원래 머티리얼 색으로 되돌림
            }
        }
    }
}
