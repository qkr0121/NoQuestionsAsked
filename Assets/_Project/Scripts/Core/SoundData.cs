using UnityEngine;

// 소리 하나의 재생 설정 - 코드는 이름이 아니라 이 에셋 참조로 소리를 부름
[CreateAssetMenu(menuName = "Data/Sound Data")]
public class SoundData : ScriptableObject
{
    public enum SoundBus { World, UI, Ambient }

    [SerializeField] private AudioClip[] clips;           // 재생할 때마다 무작위로 하나 - 같은 소리가 반복돼 들리지 않게
    [SerializeField] private SoundBus bus = SoundBus.World;
    [SerializeField, Range(0f, 1f)] private float volume = 1f;
    [SerializeField] private Vector2 pitchRange = new Vector2(0.95f, 1.05f);
    [SerializeField] private int maxConcurrent = 4;       // 같은 소리가 동시에 울릴 수 있는 최대 수 (좀비가 많을 때 폭주 방지)
    [SerializeField] private float minInterval = 0.04f;   // 같은 소리의 최소 재생 간격

    public SoundBus Bus => bus;
    public float Volume => volume;
    public Vector2 PitchRange => pitchRange;
    public int MaxConcurrent => maxConcurrent;
    public float MinInterval => minInterval;

    public AudioClip PickClip()
    {
        if (clips == null || clips.Length == 0) return null;
        return clips[Random.Range(0, clips.Length)];
    }
}
