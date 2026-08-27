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
        if (structure == null || structure != currentTarget) return;

        structure.SetPromptVisible(false);
        currentTarget = null;
    }

    private bool IsInteractableLayer(GameObject obj)
    {
        return (interactableLayer.value & (1 << obj.layer)) != 0;
    }

    private void Update()
    {
        if (currentTarget == null) return;

        Keyboard kb = Keyboard.current;
        if(kb != null && kb.eKey.wasPressedThisFrame)
        {
            currentTarget.Interact();
        }
    }
}
