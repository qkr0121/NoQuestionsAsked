using UnityEngine;

[RequireComponent(typeof(InteractableStructure))]
public class BedInteraction : MonoBehaviour
{
    public void OnInteract()
    {
        ConfirmationDialog.Instance.Show("휴식하시겠습니까? (체력이 회복됩니다)", Rest);
    }

    private void Rest()
    {
        ScreenFader.Instance.FadeAction(() =>
        {
            // 시간은 흐르지 않고 체력만 회복함 - 날짜는 전투 복귀(CombatExitPoint)로만 진행됨
            PlayerManager.Instance.FillHealth();
        });
    }
}
