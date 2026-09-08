using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BlueprintManager : MonoBehaviour
{
    public static BlueprintManager Instance { get; private set; }

    private HashSet<BlueprintId> unlocked = new HashSet<BlueprintId>();

    private void Awake()
    {
        Instance = this;
    }

    public bool IsUnlocked(BlueprintId id) => unlocked.Contains(id);

    public void Unlock(BlueprintId id)
    {
        unlocked.Add(id);
        MetaSaveManager.Instance.Save();
    }

    public string[] GetUnlockedNames()
    {
        return unlocked.Select(id => id.ToString()).ToArray();
    }

    public void LoadFrom(string[] names)
    {
        unlocked.Clear();
        if (names == null) return;

        foreach(string name in names)
        {
            if(System.Enum.TryParse(name, out BlueprintId id))
            {
                unlocked.Add(id);
            }
        }
    }
}
