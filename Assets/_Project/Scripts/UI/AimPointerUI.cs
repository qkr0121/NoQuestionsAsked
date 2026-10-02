using UnityEngine;

public class AimPointerUI : MonoBehaviour
{
    [SerializeField] private PlayerAim playerAim;
    [SerializeField] private RectTransform pointer;

    private void LateUpdate()
    {
        bool locked = PlayerInputLock.IsLocked;

        // UI 가 열려 있으면 버튼을 클릭해야 하므로 OS 커서로 전환
        Cursor.visible = locked;
        pointer.gameObject.SetActive(!locked);
        if (locked) return;

        // Overlay 캔버스는 position이 화면 픽셀 좌표와 같아서 그대로 넣으면 됨
        pointer.position = playerAim.AimScreenPoint;
    }

    private void OnDisable()
    {
        // 씬 이동 등으로 플레이어가 사라질 때 커서가 숨겨진 채로 남지 않게 복구
        Cursor.visible = true;
    }
}
