using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    public const int SlotCount = 3;

    public int ActiveSlot { get; private set; } = -1;   // 지금 런이 쓰는 칸 - 메인 메뉴에서 고를 때 정해지고, -1이면 저장하지 않음

#if UNITY_EDITOR
    // 에디터에서는 세이브 파일을 바로 열어 확인할 수 있게 Assets 하위에 저장
    private string GetSlotPath(int slot) => Path.Combine(Application.dataPath, "_Project", $"save_slot_{slot}.json");
#else
    private string GetSlotPath(int slot) => Path.Combine(Application.persistentDataPath, $"save_slot_{slot}.json");
#endif

    private void Awake()
    {
        Instance = this;
    }

    public void SetActiveSlot(int slot)
    {
        ActiveSlot = slot;
    }

    public void Save()
    {
        if (ActiveSlot < 0) return;   // 메인 메뉴를 거치지 않고 씬을 바로 실행한 경우 등 - 칸이 없으면 저장하지 않음

        SaveData data = new SaveData
        {
            currentDay = RunState.Instance.CurrentDay,
            crystal = CurrencyManager.Instance.Crystal,
            weapons = BuildWeaponEntries(),
            equippedIndex = PlayerWeapon.Instance.EquippedIndex,
            motiveIndex = RunState.Instance.MotiveIndex,
            barricades = BuildBarricadeEntries(),
            grenadeCounts = PlayerConsumables.Instance.GetGrenadeCounts(),
            potionCounts = PlayerConsumables.Instance.GetPotionCounts(),
            clearedWaveNumber = RunState.Instance.ClearedWaveNumber,
            missions = BuildMissionEntries(),
            player = new PlayerSaveData
            {
                currentHealth = PlayerManager.Instance.CurrentHealth,
                maxHealth = PlayerManager.Instance.MaxHealth
            },
            difficulty = (int)RunState.Instance.Difficulty,
            savedAtTicks = System.DateTime.Now.Ticks,
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetSlotPath(ActiveSlot), json);
    }

    private WeaponSaveEntry[] BuildWeaponEntries()
    {
        PlayerWeapon weapon = PlayerWeapon.Instance;
        WeaponSaveEntry[] entries = new WeaponSaveEntry[weapon.SlotCount];
        for (int i = 0; i < entries.Length; i++)
        {
            PlayerWeapon.WeaponSlot slot = weapon.GetSlot(i);
            entries[i] = new WeaponSaveEntry
            {
                level = slot.CurrentLevel,
                magazineAmmo = slot.MagazineAmmo,
                reserveAmmo = slot.ReserveAmmo,
                isUnlocked = slot.IsUnlocked
            };
        }
        return entries;
    }

    private BarricadeSaveEntry[] BuildBarricadeEntries()
    {
        BarricadeDirection[] directions = (BarricadeDirection[])System.Enum.GetValues(typeof(BarricadeDirection));
        BarricadeSaveEntry[] entries = new BarricadeSaveEntry[directions.Length];
        for (int i = 0; i < directions.Length; i++)
        {
            BarricadeSlotInfo info = BarricadeManager.Instance.GetSlotInfo(directions[i]);
            entries[i] = new BarricadeSaveEntry
            {
                state = (int)info.State,
                level = info.CurrentLevel,
                durability = info.CurrentDurability
            };
        }
        return entries;
    }

    private MissionSaveEntry[] BuildMissionEntries()
    {
        Mission mission = RunState.Instance.CurrentMission;
        if(mission == null)
        {
            return new MissionSaveEntry[0];
        }

        return new MissionSaveEntry[]
        {
            new MissionSaveEntry
            {
                missionName = mission.MissionName,
                requiredCrystal = mission.RequiredCrystal,
                issuedDay = mission.IssuedDay,
                dueDay = mission.DueDay,
                isSubmitted = mission.IsSubmitted
            }
        };
    }

    public bool HasSave(int slot)
    {
        return slot >= 0 && slot < SlotCount && File.Exists(GetSlotPath(slot));
    }

    public bool HasAnySave()
    {
        for (int i = 0; i < SlotCount; i++)
        {
            if (HasSave(i)) return true;
        }
        return false;
    }

    public SaveData Load(int slot)
    {
        if (!HasSave(slot)) return null;

        string json = File.ReadAllText(GetSlotPath(slot));
        return JsonUtility.FromJson<SaveData>(json);
    }

    // 칸 목록에 보여줄 요약 - 빈 칸이면 null
    public SaveSlotSummary GetSummary(int slot)
    {
        SaveData data = Load(slot);
        if (data == null) return null;

        MotiveData motives = RunState.Instance.Motives;
        bool hasMotive = data.motiveIndex >= 0 && data.motiveIndex < motives.Count;
        return new SaveSlotSummary
        {
            day = data.currentDay,
            difficulty = (GameDifficulty)data.difficulty,
            motiveName = hasMotive ? motives.Get(data.motiveIndex).displayName : "답하지 않음",
            crystal = data.crystal,
            savedAt = new System.DateTime(data.savedAtTicks)
        };
    }

    public bool TryLoadAndApply()
    {
        SaveData data = Load(ActiveSlot);
        if (data == null) return false;

        RunState.Instance.LoadFrom(data);
        CurrencyManager.Instance.LoadCrystal(data.crystal);
        PlayerWeapon.Instance.LoadFrom(data.weapons, data.equippedIndex);
        BarricadeManager.Instance.LoadFrom(data.barricades);
        PlayerConsumables.Instance.LoadCounts(data.grenadeCounts, data.potionCounts);
        if(data.player != null)
        {
            PlayerManager.Instance.LoadHealth(data.player.currentHealth, data.player.maxHealth);
        }
        return true;
    }

    // 로켓 엔딩, 서바이벌 사망처럼 "지금 런의 칸"을 지울 때
    public void DeleteSave()
    {
        DeleteSlot(ActiveSlot);
    }

    public void DeleteSlot(int slot)
    {
        if (HasSave(slot))
        {
            File.Delete(GetSlotPath(slot));
        }
    }
}
