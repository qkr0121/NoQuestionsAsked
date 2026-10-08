using UnityEngine;
using TMPro;

public class StatRowUI : MonoBehaviour
{
    [SerializeField] private TMP_Text labelText;
    [SerializeField] private TMP_Text currentText;
    [SerializeField] private TMP_Text nextText;   // "→ 34" 다음 레벨에서 값이 바뀔 때만 표시

    public void Set(string label, string current, string next)
    {
        labelText.text = label;
        currentText.text = current;

        // 값이 같은 줄은 비워서, 바뀌는 줄만 눈에 띄게 함
        bool changed = next != null && next != current;
        nextText.gameObject.SetActive(changed);
        if (changed) nextText.text = $"→ {next}";
    }
}
