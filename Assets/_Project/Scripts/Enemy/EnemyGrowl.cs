using UnityEngine;

// 좀비가 가끔 내는 그르렁 소리 - 매 프레임 판정하지 않고 다음 재생 시각만 들고 있다가 비교함
public class EnemyGrowl : MonoBehaviour
{
    [SerializeField] private SoundData growlSound;                          // 비워 두면 조용히 넘어감
    [SerializeField] private Vector2 intervalRange = new Vector2(5f, 12f);   // 다음 소리까지 걸리는 시간
    [SerializeField] private float firstDelayMax = 4f;                      // 한꺼번에 스폰된 좀비가 동시에 울지 않도록 첫 소리를 흩어 놓음

    private float nextTime;

    // 풀에서 다시 꺼낼 때마다 새로 정함 (죽어서 풀로 돌아가면 비활성이라 소리도 멈춤)
    private void OnEnable()
    {
        nextTime = Time.time + Random.Range(0.5f, firstDelayMax);
    }

    // Time.time은 일시정지(timeScale 0) 중에 멈추므로 메뉴를 열면 소리도 같이 멈춤
    private void Update()
    {
        if (Time.time < nextTime) return;

        AudioManager.PlaySfx(growlSound);
        nextTime = Time.time + Random.Range(intervalRange.x, intervalRange.y);
    }
}
