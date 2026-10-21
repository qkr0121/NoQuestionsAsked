using UnityEngine;

// 씬마다 하나 - 씬이 열리면 그 씬의 음악·앰비언스로 크로스페이드
public class SceneMusic : MonoBehaviour
{
    [SerializeField] private AudioClip music;      // 비우면 음악을 끔
    [SerializeField] private AudioClip ambience;   // 비우면 앰비언스를 끔

    // AudioManager는 GameBootstrap이 Awake에서 만들기 때문에 Awake가 아니라 Start에서 부름
    private void Start()
    {
        if (AudioManager.Instance == null) return;
        AudioManager.Instance.PlayMusic(music);
        AudioManager.Instance.PlayAmbience(ambience);
    }
}
