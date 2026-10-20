using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;              // 처음에 꺼 둠, 전체 화면 + 배경 Image
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private TMP_Dropdown qualityDropdown;
    [SerializeField] private Button backButton;

    private readonly List<Vector2Int> resolutions = new List<Vector2Int>();
    private Action onBack;

    public bool IsOpen => panel.activeSelf;

    private void Awake()
    {
        BuildResolutionOptions();
        BuildQualityOptions();

        resolutionDropdown.onValueChanged.AddListener(i => GameSettings.SetResolution(resolutions[i].x, resolutions[i].y));
        fullscreenToggle.onValueChanged.AddListener(on =>
        {
            GameSettings.SetFullscreen(on);
            resolutionDropdown.interactable = !on;   // 테두리 없는 전체화면은 바탕화면 해상도에 맞춰져 해상도 선택이 적용되지 않음
        });
        qualityDropdown.onValueChanged.AddListener(GameSettings.SetQuality);
        backButton.onClick.AddListener(() => onBack?.Invoke());
        panel.SetActive(false);
    }

    public void Open(Action back)
    {
        onBack = back;
        panel.SetActive(true);
        Refresh();
    }

    public void Close()
    {
        panel.SetActive(false);
    }

    private void BuildResolutionOptions()
    {
        // 주사율만 다른 항목은 하나로 합침
        foreach (Resolution r in Screen.resolutions)
        {
            Vector2Int size = new Vector2Int(r.width, r.height);
            if (!resolutions.Contains(size)) resolutions.Add(size);
        }

        List<string> labels = new List<string>();
        foreach (Vector2Int size in resolutions) labels.Add($"{size.x} x {size.y}");
        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(labels);
    }

    private void BuildQualityOptions()
    {
        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(new List<string>(QualitySettings.names));
    }

    // 값을 바꿀 때 저장 함수가 다시 불리지 않도록 알림 없이 표시만 맞춤
    private void Refresh()
    {
        int current = resolutions.IndexOf(new Vector2Int(Screen.width, Screen.height));
        resolutionDropdown.SetValueWithoutNotify(Mathf.Max(current, 0));
        fullscreenToggle.SetIsOnWithoutNotify(Screen.fullScreen);
        resolutionDropdown.interactable = !Screen.fullScreen;
        qualityDropdown.SetValueWithoutNotify(QualitySettings.GetQualityLevel());
    }
}
