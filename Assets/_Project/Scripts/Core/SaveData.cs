using System;

/* Mission과 필드가 겹치지만, 세이브 파일 포맷(디스크에 저장되는 형태)과
 * 게임 로직 클래스는 독립적으로 유지하기 위해 별도로 둠 - Mission 클래스 구조가 바뀌어도
 * 기존 세이브 파일과의 호환성을 이 DTO에서 흡수할 수 있음*/

[Serializable]
public class MissionSaveEntry
{
    public string missionName;
    public int requiredCrystal;
    public int issuedDay;
    public int dueDay;
    public bool isSubmitted;
}

[Serializable]
public class WeaponSaveEntry
{
    public int level;
    public int magazineAmmo;
    public int reserveAmmo;
    public bool isUnlocked;
}

[Serializable]
public class BarricadeSaveEntry
{
    public int state;        // BarricadeState 값
    public int level;
    public int durability;
}

[Serializable]
public class PlayerSaveData
{
    public int currentHealth;
    public int maxHealth;
}

[Serializable]
public class SaveData
{
    public int currentDay;
    public int currentSlot;
    public int crystal;
    public int rareCrystal; // TODO : CurrencyManager 에 희귀 크리스탈 재화 추가 시 Save/Load 배관 연결 필요(현재 미사용)
    public WeaponSaveEntry[] weapons;   // 슬롯 순서 = PlayerWeapon.weaponSlots 순서
    public int equippedIndex;
    public int motiveIndex = -1;   // 초기값 -1: 이 필드가 생기기 전의 옛 세이브는 0이 아니라 미선택으로 읽힘
    public BarricadeSaveEntry[] barricades;   // BarricadeDirection 순서 (North, East, South, West)
    public int[] grenadeCounts;   // 소모품 개수 - 종류 순서는 PlayerConsumables의 배열 순서
    public int[] potionCounts;
    public int clearedWaveNumber;   // 가장 높이 클리어한 웨이브 번호 - 옛 세이브는 0(없음)으로 읽힘

    public MissionSaveEntry[] missions;
    public PlayerSaveData player;
}
