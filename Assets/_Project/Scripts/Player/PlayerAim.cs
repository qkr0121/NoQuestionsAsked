using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAim : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform firePoint;

    public Vector3 AimPoint { get; private set; }

    public void SetCamera(Camera camera)
    {
        mainCamera = camera;
    }

    private void Awake()
    {
        if(mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    private void Update()
    {
        Vector3? aimPoint = GetMouseWorldPoint();
        if (aimPoint == null) return;

        AimPoint = aimPoint.Value;

        Vector3 direction = AimPoint - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.0001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = targetRotation;
    }

    private Vector3? GetMouseWorldPoint()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null) return null;

        Vector2 mousePos = mouse.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePos);

        Plane aimPlane = new Plane(Vector3.up, firePoint.position);

        if (aimPlane.Raycast(ray, out float enter))
        {
            return ray.GetPoint(enter);
        }

        return null;
    }
}
