using UnityEngine;

public class ScenePlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private CameraFollow cameraFollow;
    [SerializeField] private WaveSpawner waveSpawner;

    private void Start()
    {
        GameObject playerObj = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);

        PlayerAim playerAim = playerObj.GetComponent<PlayerAim>();
        playerAim.SetCamera(Camera.main);

        cameraFollow.SetTarget(playerObj.transform);

        if(waveSpawner != null)
        {
            waveSpawner.SetPlayer(playerObj.transform);
        }
    }
}
