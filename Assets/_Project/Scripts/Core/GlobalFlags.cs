public static class PlayerInputLock
{ 
    public static bool IsLocked { get; private set; }

    public static void Lock()
    {
        IsLocked = true;
    }

    public static void Unlock()
    {
        IsLocked = false;
    }
}

public static class PendingLoad
{
    public static bool ShouldLoad;
}

public static class SceneTransitionRequest
{
    public static string TargetSceneName;
}