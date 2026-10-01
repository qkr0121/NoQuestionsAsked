using UnityEngine;

[CreateAssetMenu(menuName = "Data/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("계열 고유 특성(강화 대상 아님, 무기 계열마다 다름)")]
    public float fireCooldown = 0.2f;
    public int magazineSize = 12;
    public float reloadTime = 1.5f;
    public int startingReserveAmmo = 60;
    public int maxReserveAmmo = 100;
    // 사거리는 아직 코드에 사거리 개념 자체가 없음(Projectile 이 lifeTime 으로만 판정) - 필요해지면 여기 추가

    [Header("레벨별 데미지(인덱스 0 =1 레벨, 강화 대상")]
    public int[] damageByLevel = new int[5];
    public int[] upgradeCostByLevel = new int[5];

    [Header("탄약 구매")]
    public int ammoBundleCost = 50;   // 마거진 크기(1묶음) 당 가격

    public int MaxLevel => damageByLevel.Length;
}
