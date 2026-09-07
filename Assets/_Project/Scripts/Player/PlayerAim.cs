using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAim : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float rotationSpeed = 15f;

    public Vector3 AimPoint { get; private set; }
    public Transform FirePoint => firePoint;

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

        Vector3? aimPoint = GetMouseWorldPoint();
        if (aimPoint == null) return;

        AimPoint = aimPoint.Value;

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

    private Vector3? GetMouseWorldPoint()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null) return null;

        Vector2 mousePos = mouse.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePos);

        // 바닥(Ground)에 직접 Raycast 하지 않고 firePoint 높이의 가상 평면을 쓰는 이유 :
        // 총구 높이와 바닥 높이가 다르면 화면상 마우스 위치와 실제 탄착점이 어긋나는 버그가 있었음
        Plane aimPlane = new Plane(Vector3.up, firePoint.position);

        if (aimPlane.Raycast(ray, out float enter))
        {
            return ray.GetPoint(enter);
        }

        return null;
    }
}
