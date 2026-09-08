using UnityEngine;

[RequireComponent(typeof(InteractableStructure))]
public class BedInteraction : MonoBehaviour
{
    public void OnInteract()
    {
        ConfirmationDialog.Instance.Show("수면하시겠습니까?", Sleep);
    }

    private void Sleep()
    {
        ScreenFader.Instance.FadeAction(() =>
        {
            PlayerManager.Instance.FillHealth();
            RunState.Instance.AdvanceSlot();
        });
    }
}
