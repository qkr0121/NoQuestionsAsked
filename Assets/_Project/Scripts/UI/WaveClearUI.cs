using UnityEngine;

public class WaveClearUI : MonoBehaviour
{
    [SerializeField] private GameObject clearPanel;
    [SerializeField] private WaveSpawner waveSpawner;

    private bool shown;

    private void Awake()
    {
        clearPanel.SetActive(false);
    }

    private void Update()
    {
        if (shown || !waveSpawner.IsCleared) return;

        shown = true;
        clearPanel.SetActive(true);
    }
}
