using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShoot : MonoBehaviour
{
    [SerializeField] private PlayerAim playerAim;
    [SerializeField] private float fireCooldown = 0.2f;

    private float cooldownTimer;

    private void Update()
    {
        if (PlayerInputLock.IsLocked) return;

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
        Transform fp = playerAim.FirePoint;

        ProjectilePool.Instance.Get(fp.position, fp.rotation);
    }
}
