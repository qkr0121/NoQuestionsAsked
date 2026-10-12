using UnityEngine;

public class ReloadGaugeUI : MonoBehaviour
{
    [SerializeField] private PlayerShoot playerShoot;
    [SerializeField] private PlayerAim playerAim;
    [SerializeField] private RectTransform root;       // 바 전체(배경 포함) - 재장전 중에만 켬
    [SerializeField] private RectTransform fill;       // 가로 스케일로 채우는 자식 (피벗 왼쪽)
    [SerializeField] private float headHeight = 1.6f;  // 플레이어 위치(몸 중간)에서 머리 위까지 월드 높이

    private void LateUpdate()
    {
        bool show = playerShoot.IsReloading && !PlayerInputLock.IsLocked;
        root.gameObject.SetActive(show);
        if (!show) return;

        // Overlay 캔버스는 position이 화면 픽셀 좌표와 같음 - 화면에서 크기가 일정함
        Vector3 head = playerShoot.transform.position + Vector3.up * headHeight;
        root.position = playerAim.WorldToScreen(head);
        fill.localScale = new Vector3(playerShoot.ReloadProgress, 1f, 1f);
    }
}
