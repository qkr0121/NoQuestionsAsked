using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    [SerializeField] private int maxHealth = 100;

    public int MaxHealth => maxHealth;
    public int CurrentHealth { get; private set; }

    private void Awake()
    {
        Instance = this;
        CurrentHealth = maxHealth;
    }

    // max 는 세이브에 저장된 당시의 최대 체력을 복원(향후 최대 체력 증가 유물 도입 시 이 값이 개인별로 달라질 수 있음)
    public void LoadHealth(int current, int max) // 세이브 로드 전용
    {
        maxHealth = max;
        CurrentHealth = Mathf.Clamp(current, 0, maxHealth);
    }

    public void ResetHealth() // 새 게임 시작 전용
    {
        CurrentHealth = maxHealth;
    }

    public void SetCurrentHealth(int value) => CurrentHealth = Mathf.Clamp(value, 0, maxHealth);
}
