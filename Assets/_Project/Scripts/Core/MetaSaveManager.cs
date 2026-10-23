using System.IO;
using UnityEngine;

public class MetaSaveManager : MonoBehaviour
{
    public static MetaSaveManager Instance { get; private set; }

#if UNITY_EDITOR
    // SaveManager와 같은 이유로 에디터에서는 Assets 하위에 저장
    private string SavePath => Path.Combine(Application.dataPath, "_Project", "meta_save.json");
#else
    private string SavePath => Path.Combine(Application.persistentDataPath, "meta_save.json");
#endif

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
