using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private PlayerShoot playerShoot;

    private Animator playerCharacterAnimator;
    private CharacterController controller;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        // Animator 는 fbx 자식 오브젝트에 있어서 GetComponentInChildren 필요
        playerCharacterAnimator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        if (PlayerInputLock.IsLocked)
        {
            if(playerCharacterAnimator != null)
            {
                playerCharacterAnimator.SetFloat("MoveSpeed", 0f);
            }
            return;
        }

        Vector2 input = ReadMoveInput();
        Vector3 moveDir = new Vector3(input.x, 0f, input.y);

        controller.Move(moveDir * moveSpeed * GetSpeedMultiplier() * Time.deltaTime);
        
        if(playerCharacterAnimator != null)
        {
            playerCharacterAnimator.SetFloat("MoveSpeed", input.magnitude);
        }
        
    }

    private Vector2 ReadMoveInput()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null) return Vector2.zero;

        Vector2 input = Vector2.zero;
        if (kb.wKey.isPressed) input.y += 1f;
        if (kb.sKey.isPressed) input.y -= 1f;
        if (kb.aKey.isPressed) input.x -= 1f;
        if (kb.dKey.isPressed) input.x += 1f;

        // 대각선 이동 시 더 빠르게 이동하는 버그 방지
        return input.normalized;
    }

    private float GetSpeedMultiplier()
    {
        PlayerWeapon weapon = PlayerWeapon.Instance;
        float multiplier = 1f;

        // 발사와 재장전 감속이 겹치면 곱하지 않고 더 느린 쪽만 적용 - 곱하면 의도보다 과하게 느려짐
        if (playerShoot.IsReloading)
            multiplier = Mathf.Min(multiplier, weapon.ReloadSlowMultiplier);
        if (Time.time - playerShoot.LastFireTime < weapon.Data.fireSlowDuration)
            multiplier = Mathf.Min(multiplier, weapon.FireSlowMultiplier);

        return multiplier;
    }
}