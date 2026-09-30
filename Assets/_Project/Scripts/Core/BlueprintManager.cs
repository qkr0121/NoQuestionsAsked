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

        // MVP 임시 처리: 드랍/해금 로직을 만들기 전까지 전체 기본 해금 상태로 시작.
        // 발표 이후 실제 드랍 로직으로 교체 예정 (SessionLog "이번 개발 범위에서 제외 확정" 참고).
        foreach (BlueprintId id in System.Enum.GetValues(typeof(BlueprintId)))
        {
            unlocked.Add(id);
        }
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
