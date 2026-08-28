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
        return !IsSubmitted && currentDay > DueDay;
    }
}
