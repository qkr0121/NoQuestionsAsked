using UnityEngine;

public class BarricadeSlot : MonoBehaviour
{
    [SerializeField] private BarricadeDirection direction;
    [SerializeField] private GameObject barricadeModel;
    [SerializeField] private BarricadeDurabilityBar durabilityBar;   // 방어벽 모델과 별개의 자식 - 모델이 꺼져도 바는 남아야 하기 때문, 비워도 동작
    [SerializeField] private Renderer[] visualRenderers;             // 방어벽 조각들의 렌더러 - 비워도 동작
    [SerializeField] private Material[] levelMaterials;              // 인덱스 0 = 1레벨 - 레벨마다 다르게 보이게 함

    public BarricadeDirection Direction => direction;

    private void OnEnable()
    {
        BarricadeManager.Instance.OnSlotChanged += HandleSlotChanged;
        BarricadeManager.Instance.OnSlotDamaged += HandleSlotDamaged;
        Refresh();
    }

    private void OnDisable()
    {
        BarricadeManager.Instance.OnSlotChanged -= HandleSlotChanged;
        BarricadeManager.Instance.OnSlotDamaged -= HandleSlotDamaged;
    }

    public void TakeDamage(int amount)
    {
        BarricadeManager.Instance.TakeDamage(direction, amount);
    }

    private void HandleSlotChanged(BarricadeDirection changedDirection)
    {
        if (changedDirection != direction) return;
        Refresh();
    }

    private void HandleSlotDamaged(BarricadeDirection changedDirection)
    {
        if (changedDirection != direction || durabilityBar == null) return;

        BarricadeSlotInfo info = BarricadeManager.Instance.GetSlotInfo(direction);
        if (info.State == BarricadeState.Alive) durabilityBar.Show(info.CurrentDurability, info.MaxDurability);
        else durabilityBar.Hide();   // 파괴됨
    }

    private void ApplyLevelMaterial(BarricadeSlotInfo info)
    {
        if (info.State != BarricadeState.Alive) return;
        if (visualRenderers == null || levelMaterials == null) return;
        if (info.CurrentLevel < 1 || info.CurrentLevel > levelMaterials.Length) return;

        Material material = levelMaterials[info.CurrentLevel - 1];
        foreach (Renderer r in visualRenderers) r.sharedMaterial = material;
    }

    private void Refresh()
    {
        BarricadeSlotInfo info = BarricadeManager.Instance.GetSlotInfo(direction);
        barricadeModel.SetActive(info.State == BarricadeState.Alive);
        ApplyLevelMaterial(info);
        if (durabilityBar != null) durabilityBar.Hide();   // 설치·수리·로드로 바뀌면 이전 바를 지움
    }
}
