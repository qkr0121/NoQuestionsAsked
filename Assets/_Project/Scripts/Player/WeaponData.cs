using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Data/Weapon Data")]
public class WeaponData : ScriptableObject
{
    // 강화로 바뀌는 스탯을 레벨 한 줄에 모아서, 인스펙터의 한 줄이 설계표의 한 행과 같게 보이도록 함
    [System.Serializable]
    public class LevelStats
    {
        public int damage = 20;
        public float fireInterval = 0.5f;           // 발사 간격(초)
        public float recoilScale = 1f;              // 밀림(recoilPush)에 곱하는 배율 - 강화로 줄어듦
        public float spreadScale = 1f;              // 탄퍼짐 각도에 곱하는 배율 - 강화로 줄어듦 (ADS 퍼짐 구현 후 사용)
        public float reloadTime = 1.5f;
        public int magazineSize = 12;
        public int maxReserveAmmo = 100;
        public float fireSlowMultiplier = 0.5f;     // 발사 직후 이동속도 배율
        public float reloadSlowMultiplier = 0.7f;    // 재장전 중 이동속도 배율
        public float hitSlowMultiplier = 1f;         // 맞은 좀비의 이동속도 배율 (1 = 슬로우 없음) - 이후 무기/좀비 추가 때 쓸 수 있게 모든 무기가 보유
        public float hitSlowDuration = 0f;           // 맞은 좀비의 슬로우 지속 시간(초), 0이면 효과 없음
        public int upgradeCost;                     // 이 레벨로 올리는 비용 (1레벨 줄은 사용하지 않음)
    }

    [Header("레벨별 스탯 (인덱스 0 = 1레벨, 강화 대상)")]
    public LevelStats[] levels = { new LevelStats(), new LevelStats(), new LevelStats(), new LevelStats(), new LevelStats() };

    [Header("표시 / 해금")]
    public string displayName = "무기";
    public Sprite icon;     // 핫바 슬롯에 표시할 아이콘
    public bool useRifleAnimation;  // 소총 자세 애니메이션 사용 (저격총). 권총은 기본값 false
    public GameObject heldModel;    // 손에 쥘 모델 프리팹 - 위치/회전/크기 보정은 이 프리팹 안에서 맞춤
    public MuzzleFlashEffect muzzleFlashPrefab;   // 발사 때 터지는 총구 화염 - 비우면 PlayerShoot 의 기본 화염을 씀
    public int unlockCost; // 0이면 처음부터 해금(권총). 상점에서 사서 여는 무기만 값을 넣음

    [Header("계열 고유 특성(강화 대상 아님)")]
    public int startingReserveAmmo = 60;
    public int ammoCostPerRound = 4; // 묶음 가격이 아니라 한 발 가격 - 탄창이 커져도 발당 가격이 그대로이도록

    [Header("탄")]
    public float bulletSpeed = 300f;    // 거의 즉시 맞는 느낌. 판정이 이동 구간 Raycast라 빨라도 좀비를 건너뛰지 않음
    public float range = 50f;           // 사거리(월드 단위). 지금까지는 lifeTime x speed로 우연히 정해졌음

    [Header("반동")]
    public AnimationCurve verticalKickByShot = AnimationCurve.Linear(0f, 0.3f, 10f, 0.6f); // x: 연속발사 순번, y: 조준점이 앞으로 밀리는 거리(월드 단위) 탑다운이라 탄 방향은 안 바뀌는 연출용
    public float horizontalKick = 4f;   // 좌우 흔들림 최대 각도(도). 실제 명중률에 영향을 주는 반동
    [FormerlySerializedAs("maxRecoil")]
    public float maxRecoilPush = 1.5f;  // 앞쪽 밀림 누적 상한
    public float maxRecoilAngle = 8f;   // 좌우 각도 누적 상한
    [FormerlySerializedAs("recoveryRate")]
    public float pushRecoveryRate = 1f;     // 밀림 회복 속도 - 느리게 해서 연사 중 쌓였다가 서서히 돌아오게
    public float angleRecoveryRate = 4f;    // 좌우 흔들림 회복 속도 - 빠르게 해서 피드백으로만 남게

    [Header("이동 감속")]
    public float fireSlowDuration = 0.25f;  // 마지막 발사 후 감속 유지 시간 - 발사간격과 분리해야 저격총처럼 발사 간격이 긴 무기가 계속 느려지지 않음

    public int MaxLevel => levels.Length;
    public LevelStats GetStats(int level) => levels[level - 1];
}
