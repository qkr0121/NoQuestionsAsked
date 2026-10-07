using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Data/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("계열 고유 특성(강화 대상 아님, 무기 계열마다 다름)")]
    public float fireCooldown = 0.2f;
    public int magazineSize = 12;
    public float reloadTime = 1.5f;
    public int startingReserveAmmo = 60;
    public int maxReserveAmmo = 100;

    [Header("탄")]
    public float bulletSpeed = 300f;    // 거의 즉시 맞는 느낌. 판정이 이동 구간 Raycast라 빨라도 좀비를 건너뛰지 않음
    public float range = 50f;           // 사거리(월드 단위). 지금까지는 lifeTime x speed로 우연히 정해졌음

    [Header("반동")]
    public AnimationCurve verticalKickByShot = AnimationCurve.Linear(0f, 0.3f, 10f, 0.6f); // x: 연속발사 순번, y: 조준점이 앞으로 밀리는 거리(월드 단위) 탑다운이라 탄 방향은 안 바뀌는 연출용
    public float horizontalKick = 4f;   // 좌우 흔들림 최대 각도(도). 실제 명중률에 영향을 주는 반동
    [FormerlySerializedAs("maxRecoil")]
    public float maxRecoilPush = 1.5f;  // 앞쪽 밀림 누적 상한
    public float maxRecoilAngle = 8f;   // 좌우 각도 누적 상한
    public float recoveryRate = 3f;     // 지수 감쇠 속도

    [Header("이동 감속")]
    public float fireSlowMultiplier = 0.8f;
    public float fireSlowDuration = 0.25f;  // 마지막 발사 후 감속 유지 시간 - fireCooldown과 분리해야 저격총처럼 발사 간격이 긴 무기가 계속 느려지지 않음
    public float reloadSlowMultiplier = 0.7f;

    [Header("레벨별 데미지(인덱스 0 =1 레벨, 강화 대상")]
    public int[] damageByLevel = new int[5];
    public int[] upgradeCostByLevel = new int[5];

    [Header("탄약 구매")]
    public int ammoBundleCost = 50;   // 매거진 크기(1묶음) 당 가격

    public int MaxLevel => damageByLevel.Length;
}
