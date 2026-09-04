using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 6f;

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
        if (PlayerInputLock.IsLocked) return;

        Vector2 input = ReadMoveInput();
        Vector3 moveDir = new Vector3(input.x, 0f, input.y);

        controller.Move(moveDir * moveSpeed * Time.deltaTime);
        
        if(playerCharacterAnimator != null)
        {
            playerCharacterAnimator.SetBool("IsMoving", input.magnitude > 0f);
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
}