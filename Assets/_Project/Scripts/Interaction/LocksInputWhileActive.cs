using UnityEngine;

// UI 패널 오브젝트에 이 컴포넌트를 붙이면, 패널이 열릴 때(OnEnable) 자동으로 플레이어 입력이 잠기고
// 닫힐 때(OnDisable) 자동으로 풀림. 각 UI 스크립트에서 직접 Lock/Unlock 호출할 필요 없음
public class LocksInputWhileActive : MonoBehaviour
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
