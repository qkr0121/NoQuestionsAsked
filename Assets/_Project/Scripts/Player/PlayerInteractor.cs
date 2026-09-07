using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SphereCollider), typeof(Rigidbody))]
public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private LayerMask interactableLayer;

    private InteractableStructure currentTarget;

    private void OnTriggerEnter(Collider other)
    {
        if (!IsInteractableLayer(other.gameObject)) return;

        InteractableStructure structure = other.GetComponent<InteractableStructure>();
        if (structure == null) return;

        currentTarget = structure;
        structure.SetPromptVisible(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsInteractableLayer(other.gameObject)) return;

        InteractableStructure structure = other.GetComponent<InteractableStructure>();
        if (structure == null || structure != currentTarget) return; // 나가는 대상이 지금 currentTarget 과 같을 때만 처리)

        structure.SetPromptVisible(false);
        currentTarget = null;
    }

    private void Update()
    {
        if (PlayerInputLock.IsLocked) return;
        if (currentTarget == null) return;

        Keyboard kb = Keyboard.current;
        if(kb != null && kb.eKey.wasPressedThisFrame)
        {
            currentTarget.Interact();
        }
    }

    private bool IsInteractableLayer(GameObject obj)
    {
        return (interactableLayer.value & (1 << obj.layer)) != 0;
        // 비트마스크 연산 - LayerMask 는 각 레이어를 비트 하나로 표현하는데,
        // obj.layer(레이어 번호)를 그 위치의 비트로 바꿔서 interactableLayer 에 포함되는지 AND 연산으로 확인
    }
}
