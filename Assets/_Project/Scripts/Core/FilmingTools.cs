using UnityEngine;
using UnityEngine.InputSystem;

// 홍보영상 촬영용 단축키 - 씬 배치 없이 실행 시 자동 생성됨. 촬영이 끝나면 파일째 삭제할 것
public class FilmingTools : MonoBehaviour
{
    private const int LateGameDay = 22;       // 4주차 - 웨이브 10까지 열리는 주차
    private const int LateGameClearedWave = 9;
    private const int CrystalBonus = 99999;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Create()
    {
        var go = new GameObject("FilmingTools");
        DontDestroyOnLoad(go);
        go.AddComponent<FilmingTools>();
    }

    private void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        if (kb.f5Key.wasPressedThisFrame) ToggleHud();
        else if (kb.f6Key.wasPressedThisFrame) PlayerManager.Instance.FillHealth();
        else if (kb.f7Key.wasPressedThisFrame) FullLoadout();
        else if (kb.f8Key.wasPressedThisFrame) RunState.Instance.DebugJump(LateGameDay, LateGameClearedWave);
        else if (kb.f9Key.wasPressedThisFrame) JumpToEnding();
    }

    // 패널이 같은 캔버스에 있어도 깨끗한 전투 컷이 우선이라 캔버스째 끔
    private void ToggleHud()
    {
        GameUI ui = FindFirstObjectByType<GameUI>();
        if (ui == null) return;

        Canvas canvas = ui.GetComponentInParent<Canvas>().rootCanvas;
        canvas.enabled = !canvas.enabled;
    }

    private void FullLoadout()
    {
        CurrencyManager.Instance.AddKillReward(CrystalBonus);
        PlayerManager.Instance.FillHealth();

        PlayerWeapon weapon = PlayerWeapon.Instance;
        for (int i = 0; i < weapon.SlotCount; i++)
        {
            PlayerWeapon.WeaponSlot slot = weapon.GetSlot(i);
            slot.IsUnlocked = true;
            slot.CurrentLevel = slot.data.MaxLevel;
            slot.MagazineAmmo = slot.Stats.magazineSize;
            slot.ReserveAmmo = slot.Stats.maxReserveAmmo;
        }

        // LoadCounts 로 채우면 구매 소리가 녹음에 섞이지 않음
        PlayerConsumables items = PlayerConsumables.Instance;
        int[] grenades = new int[items.GrenadeTypeCount];
        for (int i = 0; i < grenades.Length; i++) grenades[i] = items.GetMaxGrenadeCount(i);
        int[] potions = new int[items.GetPotionCounts().Length];
        for (int i = 0; i < potions.Length; i++) potions[i] = items.GetMaxPotionCount(i);
        items.LoadCounts(grenades, potions);
    }

    private void JumpToEnding()
    {
        RunState run = RunState.Instance;
        run.DebugJump(run.TotalDays + 1, 10);

        // 로켓은 씬 로드 때 탈출 단계가 아니면 꺼져 있어서 직접 켬
        foreach (RocketInteraction rocket in FindObjectsByType<RocketInteraction>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            rocket.gameObject.SetActive(true);
    }
}
