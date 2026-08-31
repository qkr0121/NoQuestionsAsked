using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    [SerializeField] private int crystalPerKill = 10;

    public int Crystal { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void AddKillReward()
    {
        Crystal += crystalPerKill;
    }

    public bool TrySpend(int amount)
    {
        if (Crystal < amount) return false;

        Crystal -= amount;
        return true;
    }

    public void SetCrystal(int amount)
    {
        Crystal = amount;
    }

    public void ResetCrystal()
    {
        Crystal = 0;
    }
}
