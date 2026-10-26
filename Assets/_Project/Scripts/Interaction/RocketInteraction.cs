using System.Collections;
using UnityEngine;

[RequireComponent(typeof(InteractableStructure))]
public class RocketInteraction : MonoBehaviour
{
    [SerializeField] private Transform rocketModel;   // 날아오를 비주얼 - 루트를 움직이면 상호작용 콜라이더도 같이 떠서 분리
    [SerializeField] private EndingUI endingUI;
    [SerializeField] private float launchSeconds = 3.2f;   // 상승에 걸리는 시간 - 길수록 천천히 오름 (로켓 소리의 절정 약 3초와 맞춤)
    [SerializeField] private float launchHeight = 40f;

    private GameObject player;
    private bool launching;

    private void Start()
    {
        // 로켓은 귀환 수단이라 탈출 단계(29일 이후)에만 존재 - 그 전에는 모델과 콜라이더를 통째로 숨김
        gameObject.SetActive(RunState.Instance.IsEscapeReady);
    }

    // 플레이어가 런타임 생성이라 ScenePlayerSpawner 가 주입함
    public void SetPlayer(GameObject playerObj)
    {
        player = playerObj;
    }

    public void OnInteract()
    {
        if (launching || !RunState.Instance.IsEscapeReady) return;
        ConfirmationDialog.Instance.Show("지구로 돌아가시겠습니까?", Launch);
    }

    private void Launch()
    {
        launching = true;
        StartCoroutine(LaunchRoutine());
    }

    private IEnumerator LaunchRoutine()
    {
        PlayerInputLock.Lock();
        AudioManager.PlayRocketLaunch();
        AudioManager.StopMusic();
        if (player != null) player.SetActive(false);   // 탑승한 것으로 연출

        Vector3 start = rocketModel.position;
        float elapsed = 0f;
        while (elapsed < launchSeconds)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / launchSeconds;
            rocketModel.position = start + Vector3.up * (launchHeight * t * t);   // 가속하며 상승
            yield return null;
        }

        // 암전 중에 세이브를 지움 - 엔딩 화면에서 게임을 꺼도 이어하기가 남지 않게 함
        ScreenFader.Instance.FadeAction(() =>
        {
            SaveManager.Instance.DeleteSave();
            endingUI.Show();
        });
        PlayerInputLock.Unlock();
    }
}
