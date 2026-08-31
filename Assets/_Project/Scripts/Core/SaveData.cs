using System;

[Serializable]
public class MissionSaveEntry
{
    public string missionName;
    public int requiredCrystal;
    public int issuedDay;
    public int dueDay;
    public bool isSubmitted;
}

[Serializable]
public class PlayerSaveData
{
    public int currentHealth;
    public int maxHealth;
}

[Serializable]
public class SaveData
{
    public int currentDay;
    public int currentSlot;
    public int crystal;
    public int rareCrystal;

    public MissionSaveEntry[] missions;
    public PlayerSaveData player;
    
    public bool HasSaveData;
}
