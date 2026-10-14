using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    //private string SavePath => Path.Combine(Application.persistentDataPath, "save.json");
    // 임시 : 에디터에서 세이브 파일을 직접 열어 확인하기 위해 Assets 폴더 하위에 저장 중
    // Application.dataPath 는 빌드에서는 다른 경로를 가리키므로, 빌드 전 반드시 위 줄(persistentDataPath)로 되돌릴 것
    private string SavePath => Path.Combine(Application.dataPath, "_Project", "save.json");

    private void Awake()
    {
        Instance = this;
    }

    public void Save()
    {
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
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
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

    public bool HasSave()
    {
        return File.Exists(SavePath);
    }

    public SaveData Load()
    {
        if (!HasSave()) return null;

        string json = File.ReadAllText(SavePath);
        return JsonUtility.FromJson<SaveData>(json);
    }

    public bool TryLoadAndApply()
    {
        SaveData data = Load();
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

    public void DeleteSave()
    {
        if(HasSave())
        {
            File.Delete(SavePath);
        }
    }
}
