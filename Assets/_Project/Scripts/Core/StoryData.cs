using UnityEngine;

[CreateAssetMenu(menuName = "Data/Story Data")]
public class StoryData : ScriptableObject
{
    [System.Serializable]
    public class Page
    {
        public string title;
        [TextArea(2, 6)] public string body;
        public Sprite image;   // 비워 두면 배경이 검게 남고 글만 나옴 - 이미지가 안 나온 페이지를 텍스트로 대체하려고
    }

    [SerializeField] private Page[] pages;

    public int Count => pages.Length;
    public Page Get(int index) => pages[index];
}
