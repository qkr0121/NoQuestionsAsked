using UnityEngine;

public class LocksInputWhileAcitve : MonoBehaviour
{
    private void OnEnable()
    {
        PlayerInputLock.Lock();
    }

    private void OnDisable()
    {
        PlayerInputLock.Unlock();
    }
}
