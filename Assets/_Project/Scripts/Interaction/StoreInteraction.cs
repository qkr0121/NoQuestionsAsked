using UnityEngine;

[RequireComponent(typeof(InteractableStructure))]
public class StoreInteraction : MonoBehaviour
{
    public void OnInteract()
    {
        StoreUI.Instance.Open();
    }
}
