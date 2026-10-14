using UnityEngine;

[CreateAssetMenu(menuName = "Data/Tutorial Data")]
public class TutorialData : ScriptableObject
{
    [System.Serializable]
    public class Page
    {
        public string title;
        [TextArea(3, 8)] public string body;
    }

    [SerializeField] private Page[] pages;

    public int Count => pages.Length;
    public Page Get(int index) => pages[index];
}
