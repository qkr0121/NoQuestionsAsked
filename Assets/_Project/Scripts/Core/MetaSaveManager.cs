using System.IO;
using UnityEngine;

public class MetaSaveManager : MonoBehaviour
{
    public static MetaSaveManager Instance { get; private set; }

    private string SavePath => Path.Combine(Application.dataPath, "_Project", "meta_save.json");
    // SaveManager 와 동일한 임시 디버그 경로 - 빌드 전 반드시 persistentDataPath 로 되돌릴 것

    private void Awake()
    {
        Instance = this;
    }

    public void Save()
    {
        MetaSaveData data = new MetaSaveData
        {
            unlockedBlueprints = BlueprintManager.Instance.GetUnlockedNames()
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
    }

    public bool HasSave() => File.Exists(SavePath);

    public void LoadAndApply()
    {
        if (!HasSave()) return;

        string json = File.ReadAllText(SavePath);
        MetaSaveData data = JsonUtility.FromJson<MetaSaveData>(json);
        BlueprintManager.Instance.LoadFrom(data.unlockedBlueprints);
    }
}
