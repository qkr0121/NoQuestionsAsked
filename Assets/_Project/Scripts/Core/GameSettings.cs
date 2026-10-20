using UnityEngine;

public static class GameSettings
{
    private const string ResolutionWidthKey = "Settings.ResolutionWidth";
    private const string ResolutionHeightKey = "Settings.ResolutionHeight";
    private const string FullscreenKey = "Settings.Fullscreen";
    private const string QualityKey = "Settings.Quality";

    // 게임 시작 때 한 번 - 저장된 값이 있을 때만 적용해서 첫 실행은 기본 설정을 건드리지 않음
    public static void ApplySaved()
    {
        if (PlayerPrefs.HasKey(QualityKey))
            QualitySettings.SetQualityLevel(Mathf.Clamp(PlayerPrefs.GetInt(QualityKey), 0, QualitySettings.names.Length - 1), true);

        FullScreenMode mode = PlayerPrefs.GetInt(FullscreenKey, Screen.fullScreen ? 1 : 0) == 1 ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        if (PlayerPrefs.HasKey(ResolutionWidthKey))
            Screen.SetResolution(PlayerPrefs.GetInt(ResolutionWidthKey), PlayerPrefs.GetInt(ResolutionHeightKey), mode);
        else
            Screen.fullScreenMode = mode;
    }

    public static void SetResolution(int width, int height)
    {
        Screen.SetResolution(width, height, Screen.fullScreenMode);
        PlayerPrefs.SetInt(ResolutionWidthKey, width);
        PlayerPrefs.SetInt(ResolutionHeightKey, height);
        PlayerPrefs.Save();
    }

    public static void SetFullscreen(bool on)
    {
        // 창 모드로 돌아올 때는 저장해 둔 해상도로 복원 - 전체화면 동안 바뀐 창 크기가 남지 않게
        if (!on && PlayerPrefs.HasKey(ResolutionWidthKey))
            Screen.SetResolution(PlayerPrefs.GetInt(ResolutionWidthKey), PlayerPrefs.GetInt(ResolutionHeightKey), FullScreenMode.Windowed);
        else
            Screen.fullScreenMode = on ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        PlayerPrefs.SetInt(FullscreenKey, on ? 1 : 0);
        PlayerPrefs.Save();
    }

    public static void SetQuality(int level)
    {
        QualitySettings.SetQualityLevel(level, true);
        PlayerPrefs.SetInt(QualityKey, level);
        PlayerPrefs.Save();
    }
}
