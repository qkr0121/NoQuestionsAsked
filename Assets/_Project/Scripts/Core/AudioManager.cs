using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

// PersistentManagers 프리팹에 붙는 오디오 재생 담당 - 효과음 풀 + 음악·앰비언스 크로스페이드
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("믹서")]
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private AudioMixerGroup musicGroup;
    [SerializeField] private AudioMixerGroup worldGroup;
    [SerializeField] private AudioMixerGroup uiGroup;
    [SerializeField] private AudioMixerGroup ambientGroup;

    [Header("효과음")]
    [SerializeField] private int poolSize = 24;

    [Header("기본 UI 소리 (버튼에 따로 지정하지 않았을 때)")]
    [SerializeField] private SoundData defaultClick;
    [SerializeField] private SoundData defaultHover;

    [Header("패널")]
    [SerializeField] private SoundData panelSound;   // 패널을 열고 닫을 때

    [Header("웨이브")]
    [SerializeField] private SoundData warningSound;   // 웨이브 경고 사이렌

    [Header("연출")]
    [SerializeField] private SoundData gameOverSound;
    [SerializeField] private SoundData rocketLaunchSound;

    [Header("음악·앰비언스")]
    [SerializeField] private float fadeTime = 1f;

    private AudioSource[] pool;
    private SoundData[] playing;   // 풀의 각 소스가 지금 재생 중인 소리 (동시 재생 수 계산용)
    private float[] startTime;
    private readonly Dictionary<SoundData, float> lastPlayed = new Dictionary<SoundData, float>();

    private LoopChannel music;
    private LoopChannel ambience;

    public SoundData DefaultClick => defaultClick;
    public SoundData DefaultHover => defaultHover;

    private void Awake()
    {
        Instance = this;

        pool = new AudioSource[poolSize];
        playing = new SoundData[poolSize];
        startTime = new float[poolSize];
        for (int i = 0; i < poolSize; i++)
        {
            var go = new GameObject("Sfx_" + i);
            go.transform.SetParent(transform, false);
            pool[i] = go.AddComponent<AudioSource>();
            pool[i].playOnAwake = false;
            pool[i].spatialBlend = 0f;   // 탑다운이라 전부 2D - 거리 감쇠는 동시 재생 제한으로 대신함
        }

        music = new LoopChannel(transform, musicGroup, "Music");
        ambience = new LoopChannel(transform, ambientGroup, "Ambience");
    }

    // 믹서 값은 Awake에서 설정하면 첫 프레임에 안 먹는 경우가 있어 Start에서 적용
    private void Start()
    {
        foreach (VolumeChannel channel in System.Enum.GetValues(typeof(VolumeChannel)))
            ApplyVolume(channel, GameSettings.GetVolume(channel));
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // 호출하는 쪽마다 Instance null 검사를 반복하지 않도록 - 오디오가 없는 씬에서도 안전
    public static void PlaySfx(SoundData sound)
    {
        if (Instance != null) Instance.Play(sound);
    }

    public static void PlayPanel()
    {
        if (Instance != null) Instance.Play(Instance.panelSound);
    }

    public static void PlayClick()
    {
        if (Instance != null) Instance.Play(Instance.defaultClick);
    }

    public static void PlayWarning()
    {
        if (Instance != null) Instance.Play(Instance.warningSound);
    }

    public static void PlayGameOver()
    {
        if (Instance != null) Instance.Play(Instance.gameOverSound);
    }

    public static void PlayRocketLaunch()
    {
        if (Instance != null) Instance.Play(Instance.rocketLaunchSound);
    }

    // 현재 BGM을 페이드아웃해서 끔 - 엔딩·게임오버 연출 소리가 주인공이 되게
    public static void StopMusic()
    {
        if (Instance != null) Instance.PlayMusic(null);
    }

    public void Play(SoundData sound)
    {
        if (sound == null) return;   // 소리를 아직 연결하지 않은 곳은 조용히 넘어감
        AudioClip clip = sound.PickClip();
        if (clip == null) return;

        float now = Time.unscaledTime;   // 일시정지(timeScale 0) 중의 UI 소리도 간격 계산이 되도록
        if (lastPlayed.TryGetValue(sound, out float last) && now - last < sound.MinInterval) return;
        if (CountPlaying(sound) >= sound.MaxConcurrent) return;

        int index = FindSlot();
        AudioSource source = pool[index];
        source.outputAudioMixerGroup = GetGroup(sound.Bus);
        source.clip = clip;
        source.volume = sound.Volume;
        source.pitch = Random.Range(sound.PitchRange.x, sound.PitchRange.y);
        source.Play();

        playing[index] = sound;
        startTime[index] = now;
        lastPlayed[sound] = now;
    }

    // clip이 null이면 현재 곡을 페이드아웃해서 끔
    public void PlayMusic(AudioClip clip)
    {
        music.Switch(this, clip, fadeTime);
    }

    public void PlayAmbience(AudioClip clip)
    {
        ambience.Switch(this, clip, fadeTime);
    }

    // 슬라이더 0~1을 믹서의 데시벨로 바꿔 적용 (0은 -80dB로 사실상 무음)
    public void ApplyVolume(VolumeChannel channel, float linear)
    {
        float db = linear > 0.0001f ? Mathf.Log10(linear) * 20f : -80f;
        mixer.SetFloat(ParamName(channel), db);
    }

    private static string ParamName(VolumeChannel channel)
    {
        switch (channel)
        {
            case VolumeChannel.Music: return "MusicVolume";
            case VolumeChannel.Sfx: return "SfxVolume";
            default: return "MasterVolume";
        }
    }

    private AudioMixerGroup GetGroup(SoundData.SoundBus bus)
    {
        switch (bus)
        {
            case SoundData.SoundBus.UI: return uiGroup;
            case SoundData.SoundBus.Ambient: return ambientGroup;
            default: return worldGroup;
        }
    }

    private int CountPlaying(SoundData sound)
    {
        int count = 0;
        for (int i = 0; i < pool.Length; i++)
            if (playing[i] == sound && pool[i].isPlaying) count++;
        return count;
    }

    // 빈 소스를 쓰고, 모두 사용 중이면 가장 오래된 소스를 가로챔
    private int FindSlot()
    {
        int oldest = 0;
        for (int i = 0; i < pool.Length; i++)
        {
            if (!pool[i].isPlaying) return i;
            if (startTime[i] < startTime[oldest]) oldest = i;
        }
        return oldest;
    }

    // 음악·앰비언스용 반복 채널 - 소스 두 개를 번갈아 쓰며 크로스페이드
    private class LoopChannel
    {
        private readonly AudioSource[] sources = new AudioSource[2];
        private int active;
        private Coroutine routine;
        private AudioClip current;

        public LoopChannel(Transform parent, AudioMixerGroup group, string name)
        {
            for (int i = 0; i < 2; i++)
            {
                var go = new GameObject(name + "_" + i);
                go.transform.SetParent(parent, false);
                sources[i] = go.AddComponent<AudioSource>();
                sources[i].outputAudioMixerGroup = group;
                sources[i].loop = true;
                sources[i].playOnAwake = false;
                sources[i].spatialBlend = 0f;
                sources[i].volume = 0f;
            }
        }

        public void Switch(MonoBehaviour host, AudioClip clip, float fadeTime)
        {
            if (clip == current) return;   // 같은 곡을 또 부르면(씬을 다시 열 때 등) 끊기지 않게 무시
            current = clip;
            if (routine != null) host.StopCoroutine(routine);
            routine = host.StartCoroutine(Crossfade(clip, fadeTime));
        }

        private IEnumerator Crossfade(AudioClip clip, float fadeTime)
        {
            AudioSource from = sources[active];
            AudioSource to = sources[1 - active];
            active = 1 - active;

            to.Stop();   // 이전 전환 도중 페이드아웃 중이던 소스가 남지 않게
            if (clip != null)
            {
                to.clip = clip;
                to.volume = 0f;
                to.Play();
            }

            float fromStart = from.volume;
            float t = 0f;
            while (t < fadeTime)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / fadeTime);
                if (clip != null) to.volume = k;
                from.volume = fromStart * (1f - k);
                yield return null;
            }

            from.Stop();
            from.volume = 0f;
            if (clip != null) to.volume = 1f;
            routine = null;
        }
    }
}
