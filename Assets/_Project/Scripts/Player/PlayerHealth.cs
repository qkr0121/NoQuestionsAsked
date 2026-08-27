using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public void TakeDamage(int amount)
    {
        PlayerManager.Instance.TakeDamage(amount);
    }
}
