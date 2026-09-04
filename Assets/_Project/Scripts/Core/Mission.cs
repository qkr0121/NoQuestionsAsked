using System;

[Serializable]
public class Mission
{
    public string MissionName;
    public int RequiredCrystal;
    public int IssuedDay;
    public int DueDay;
    public bool IsSubmitted;

    public int DaysRemaining(int currentDay)
    {
        return Math.Max(0, DueDay - currentDay);
    }

    public bool IsOverdue(int currentDay)
    {
        // 마감일 당일(currentDay == DueDay)까지는 제출 가능하도록 '>' 사용 (>= 아님, 의도적)
        return !IsSubmitted && currentDay > DueDay;
    }
}
