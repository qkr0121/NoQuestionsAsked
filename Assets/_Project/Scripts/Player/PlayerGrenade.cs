using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGrenade : MonoBehaviour
{
    [SerializeField] private PlayerAim playerAim;
    [SerializeField] private PlayerShoot playerShoot;
    [SerializeField] private SpriteRenderer indicator;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask throwBlockLayer;
    [SerializeField] private float throwCooldown = 1f;
    [SerializeField] private Color readyColor = Color.white;
    [SerializeField] private Color emptyColor = Color.gray;

    private int selectedIndex;
    private float cooldownTimer;

    public bool IsHolding { get; private set; }
    
    private void Awake()
    {
        indicator.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (PlayerInputLock.IsLocked)
        {
            indicator.gameObject.SetActive(false);
            return;
        }

        cooldownTimer -= Time.deltaTime;

        if(!IsHolding || !TryGetLandPoint(out Vector3 landPoint))
        {
            indicator.gameObject.SetActive(false);
            return;
        }

        GrenadeData data = PlayerConsumables.Instance.GetGrenadeData(selectedIndex);
        bool hasGrenade = HasGrenade();
        UpdateIndicator(landPoint, data, hasGrenade);

        Mouse mouse = Mouse.current;
        if(mouse != null && mouse.leftButton.wasPressedThisFrame && cooldownTimer <= 0f)
        {
            Throw(landPoint, data);
        }
    }

    public bool HasGrenade() => InfiniteAmmoZone.IsActive || PlayerConsumables.Instance.GetGrenadeCount(selectedIndex) > 0;

    // 입력은 PlayerHotbar가 받아서 호출함
    public void OnGrenadeKey()
    {
        if(IsHolding)
        {
            // 들고 있을 때 다시 누르면 다음 종류로 - MVP는 1종이라 그대로 유지됨
            selectedIndex = (selectedIndex + 1) % PlayerConsumables.Instance.GrenadeTypeCount;
            return;
        }

        // 총을 든 상태에서 전 종류가 0개면 아예 들지 않음 (던져서 0개가 된 경우는 빈 손으로 계속 들고 있음)
        bool hasType = TrySelectAvailableType();
        if (!hasType && !InfiniteAmmoZone.IsActive) return;

        IsHolding = true;
        playerShoot.CancelReload();
    }

    public void PutDown()
    {
        IsHolding = false;
    }

    // 현재 선택 종류부터 순환하며 개수가 남은 첫 종류를 고름 - 선택 종류가 0개여도 다른 종류를 들 수 있게
    private bool TrySelectAvailableType()
    {
        int typeCount = PlayerConsumables.Instance.GrenadeTypeCount;
        for(int i = 0; i < typeCount; i++)
        {
            int index = (selectedIndex + i) % typeCount;
            if(PlayerConsumables.Instance.GetGrenadeCount(index) > 0)
            {
                selectedIndex = index;
                return true;
            }
        }

        return false;
    }

    private bool TryGetLandPoint(out Vector3 landPoint)
    {
        landPoint = Vector3.zero;

        // 플레이어 pivot이 몸 중간이라 실제 바닥 높이를 따로 구해야 착탄 원이 바닥에 붙음
        if (!Physics.Raycast(transform.position, Vector3.down, out RaycastHit groundHit, 10f, groundLayer)) return false;
        float groundY = groundHit.point.y;

        Vector3? mousePoint = playerAim.GetMousePointOnPlane(groundY);
        if (mousePoint == null) return false;

        Vector3 origin = new Vector3(transform.position.x, groundY, transform.position.z);
        GrenadeData data = PlayerConsumables.Instance.GetGrenadeData(selectedIndex);
        Vector3 offset = Vector3.ClampMagnitude(mousePoint.Value - origin, data.maxThrowRange);
        landPoint = origin + offset;

        // 맵 끝 벽 너머로는 못던지게 벽 바로 앞에서 멈춤
        if(offset.sqrMagnitude > 0.0001f && Physics.Raycast(origin + Vector3.up, offset.normalized, out RaycastHit wallhit,
            offset.magnitude, throwBlockLayer))
        {
            landPoint = origin + offset.normalized * Mathf.Max(0f, wallhit.distance - 0.1f);
        }

        // 마우스가 사거리/벽을 벗어났으면 착탄 지점으로 되돌림 - 포인터를 숨겨서, 안 그러면 돌아올 때 원이 안 움직이는 구간이 생김
        // 0.01f 여유는 경계에서 매 프레임 미세하게 되돌려지는 떨림 방지
        Vector3 wanted = mousePoint.Value - origin;
        if(wanted.magnitude > (landPoint - origin).magnitude + 0.05f)
        {
            Mouse.current.WarpCursorPosition(playerAim.WorldToScreen(landPoint));
        }

        return true;
    }

    private void UpdateIndicator(Vector3 landPoint, GrenadeData data, bool hasGrenade)
    {
        indicator.gameObject.SetActive(true);
        indicator.transform.position = landPoint + Vector3.up * 0.05f; // 바닥과 겹쳐 깜빡이지 않게 살짝 띄움

        float size = data.explosionRadius * 2f;
        indicator.transform.localScale = new Vector3(size, size, 1f);
        indicator.color = hasGrenade ? readyColor : emptyColor;
    }

    private void Throw(Vector3 landPoint, GrenadeData data)
    {
        if (!InfiniteAmmoZone.IsActive && !PlayerConsumables.Instance.TryConsumeGrenade(selectedIndex)) return;

        cooldownTimer = throwCooldown;
        Grenade grenade = Instantiate(data.prefab);
        grenade.Launch(playerAim.FirePoint.position, landPoint, data);
        AudioManager.PlaySfx(data.throwSound);
    }
}
