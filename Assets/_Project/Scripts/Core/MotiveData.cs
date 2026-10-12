using UnityEngine;

[CreateAssetMenu(menuName = "Data/Motive Data")]
public class MotiveData : ScriptableObject
{
    [System.Serializable]
    public class Motive
    {
        public string displayName;
        [TextArea(2, 4)] public string description;   // 선택 화면에 보여줄 설명
        [TextArea(2, 6)] public string endingText;    // 28일 엔딩(Issue 11)에서 보여줄 결과 텍스트
    }

    [SerializeField] private Motive[] motives;
    [SerializeField] private int offerCount = 4;      // 새 게임마다 무작위로 보여줄 개수
    [SerializeField, TextArea(2, 6)] private string noAnswerEndingText;   // 지원 동기에 답하지 않았을 때(-1)의 엔딩 텍스트

    public string NoAnswerEndingText => noAnswerEndingText;
    public int Count => motives.Length;
    public int OfferCount => Mathf.Min(offerCount, motives.Length);
    public Motive Get(int index) => motives[index];
}
