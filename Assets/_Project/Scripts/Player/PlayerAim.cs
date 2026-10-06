using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAim : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float rotationSpeed = 15f;

    private int consecutiveShots;
    private float lastShotTime;
    private float noiseSeed;
    private float recoilAngle;  // 좌우 회전 각도(도) - 조준 거리와 무관하게 같은 각도로 빗나가게 하려고 거리 대신 각도로 누적
    private float recoilPush;   // 조준 방향 앞쪽 밀림(월드 단위) - 연출용

    public Vector3 AimPoint { get; private set; }
    public Transform FirePoint => firePoint;
    public Vector2 AimScreenPoint => mainCamera.WorldToScreenPoint(AimPoint);

    private void Awake()
    {
        if(mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    private void Update()
    {
        if (PlayerInputLock.IsLocked) return;

        Vector3? rawAim = GetMousePointOnPlane(firePoint.position.y);
        if (rawAim == null) return;

        // 연사 중에도 계속 회복 - 튀는 양과 회복량이 맞는 지점에서 자연스럽게 수렴
        float decay = Mathf.Exp(-PlayerWeapon.Instance.Data.recoveryRate * Time.deltaTime);
        recoilAngle *= decay;
        recoilPush *= decay;

        // 회전 중심 높이를 조준 평면에 맞춰야 반동 0일 때 포인터가 OS 커서와 정확히 겹침
        Vector3 pivot = transform.position;
        pivot.y = rawAim.Value.y;

        Vector3 toAim = rawAim.Value - pivot;
        if (toAim.sqrMagnitude < 0.0001f) return;

        Vector3 rotated = Quaternion.AngleAxis(recoilAngle, Vector3.up) * toAim;
        AimPoint = pivot + rotated + rotated.normalized * recoilPush;

        Vector3 direction = AimPoint - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.0001f) return;
        
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 1 - Mathf.Exp(-rotationSpeed * Time.deltaTime));
    }

    public void SetCamera(Camera camera)
    {
        mainCamera = camera;
    }

    public void ApplyRecoil(WeaponData data)
    {
        // 발사 간격보다 충분히 오래 쉬었으면 새 연사로 보고 커브를 처음부터 다시 탐
        if(Time.time - lastShotTime > data.fireCooldown * 2f)
        {
            consecutiveShots = 0;
            noiseSeed = Random.value * 100f; // 연사마다 좌우 흔들림 패턴이 달라지도록
        }
        lastShotTime = Time.time;

        float vertical = data.verticalKickByShot.Evaluate(consecutiveShots);
        // 발사 순번으로 샘플링해서 연속된 발끼리 좌우가 부드럽게 이어지게 함(-1~1)
        float noise = Mathf.PerlinNoise(noiseSeed, consecutiveShots * 0.35f) * 2f - 1f;

        recoilPush = Mathf.Min(recoilPush + vertical, data.maxRecoilPush);
        recoilAngle = Mathf.Clamp(recoilAngle + noise * data.horizontalKick, -data.maxRecoilAngle, data.maxRecoilAngle);
        consecutiveShots++;
    }

    public Vector3? GetMousePointOnPlane(float height)
    {
        Mouse mouse = Mouse.current;
        if (mouse == null) return null;

        Vector2 mousePos = mouse.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePos);

        // 바닥(Ground)에 직접 Raycast 하지 않고 firePoint 높이의 가상 평면을 쓰는 이유 :
        // 총구 높이와 바닥 높이가 다르면 화면상 마우스 위치와 실제 탄착점이 어긋나는 버그가 있었음
        // 사격은 총구 높이, 수류탄 착탄은 바닥 높이를 넘겨서 같은 함수를 씀
        Plane plane = new Plane(Vector3.up, new Vector3(0f, height, 0f));

        if (plane.Raycast(ray, out float enter))
        {
            return ray.GetPoint(enter);
        }

        return null;
    }

    public Vector2 WorldToScreen(Vector3 worldPoint) => mainCamera.WorldToScreenPoint(worldPoint);
}
