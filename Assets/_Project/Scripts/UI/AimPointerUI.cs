using UnityEngine;

public class AimPointerUI : MonoBehaviour
{
    [SerializeField] private PlayerAim playerAim;
    [SerializeField] private RectTransform pointer;
    [SerializeField] private PlayerGrenade playerGrenade;

    private void LateUpdate()
    {
        bool locked = PlayerInputLock.IsLocked;

        // UI 가 열려 있으면 버튼을 클릭해야 하므로 OS 커서로 전환
        Cursor.visible = locked;
        // 수류탄을 든 동안에는 반동이 없으므로 포인터 대신 착탄 원으로 조준
        bool showPointer = !locked && !playerGrenade.IsHolding;
        pointer.gameObject.SetActive(showPointer);
        if (!showPointer) return;

        // Overlay 캔버스는 position이 화면 픽셀 좌표와 같아서 그대로 넣으면 됨
        pointer.position = playerAim.AimScreenPoint;
    }

    private void OnDisable()
    {
        // 씬 이동 등으로 플레이어가 사라질 때 커서가 숨겨진 채로 남지 않게 복구
        Cursor.visible = true;
    }
}
