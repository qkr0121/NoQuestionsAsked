using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShoot : MonoBehaviour
{
    [SerializeField] private Transform firePoint;
    [SerializeField] private PlayerAim playerAim;
    [SerializeField] private float fireCooldown = 0.2f;

    private float cooldownTimer;

    private void Update()
    {
        cooldownTimer -= Time.deltaTime;

        Mouse mouse = Mouse.current;
        if (mouse == null) return;

        if(mouse.leftButton.isPressed && cooldownTimer <= 0f)
        {
            Fire();
            cooldownTimer = fireCooldown;
        }
    }

    private void Fire()
    {
        Vector3 direction = playerAim.AimPoint - firePoint.position;
        direction.y = 0f;

        Quaternion fireRotation = direction.sqrMagnitude > 0.0001f
            ? Quaternion.LookRotation(direction)
            : firePoint.rotation;

        ProjectilePool.Instance.Get(firePoint.position, fireRotation);
    }

}
