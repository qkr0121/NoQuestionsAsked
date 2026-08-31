using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    //private string SavePath => Path.Combine(Application.persistentDataPath, "save.json");
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
            currentSlot = (int)RunState.Instance.CurrentSlot,
            crystal = CurrencyManager.Instance.Crystal,
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

    public void DeleteSave()
    {
        if(HasSave())
        {
            File.Delete(SavePath);
        }
    }
}
