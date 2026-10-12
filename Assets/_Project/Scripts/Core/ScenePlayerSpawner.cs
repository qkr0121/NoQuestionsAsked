using UnityEngine;

public class ScenePlayerSpawner : MonoBehaviour
{
    [Header("플레이어 생성")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform spawnPoint;

    [Header("연동 대상")]
    [SerializeField] private CameraFollow cameraFollow;
    [SerializeField] private WaveSpawner waveSpawner;
    [SerializeField] private HotbarUI hotbarUI;   // HUD 인스턴스의 Hotbar

    private void Start()
    {
        GameObject playerObj = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);

        PlayerAim playerAim = playerObj.GetComponent<PlayerAim>();
        playerAim.SetCamera(Camera.main);
        // 생성 직후라 인스펙터로 미리 연결할 수 없어서 코드로 주입

        cameraFollow.SetTarget(playerObj.transform);

        if(waveSpawner != null) // Base 씬에는 WaveSpawner 가 없어서 인스펙터에 연결 안됨 - ZWD 에서만 실행됨
        {
            waveSpawner.SetPlayer(playerObj.transform);
        }

        hotbarUI.SetPlayer(playerObj.GetComponent<PlayerGrenade>(), playerObj.GetComponent<PlayerPotion>());
    }
}
