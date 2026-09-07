/* 씬 전환 시 일반 변수로는 값을 전달할 수 없어(씬이 바뀌면 초기화됨),
 * static 필드로 "다음 씬이 읽을 값"을 임시 저장하는 용도의 클래스들을 모아둔 파일.*/

// UI 패널이 열려있는 동안 플레이어 이동/조준/발사 입력을 막기 위한 전역 잠금 스위치
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

// MainMenu에서 "이어하기"를 눌렀는지 여부를 Base 씬까지 전달해, RunState가 세이브 로드 여부를 판단하는 데 사용
public static class PendingLoad
{
    public static bool ShouldLoad { get; private set; }

    public static void MarkForLoad()
    {
        ShouldLoad = true;
    }

    public static void Reset()
    {
        ShouldLoad = false;
    }
}

// GameOverUI 에서 메인메뉴로 돌아갈 때, 화면이 로딩 UI(LoadingScreen)로 가려진 뒤에 런을 리셋하기 위한 플래그
// GameOverUI 가 직접 리셋하면 씬 전환 전 잠깐 HUD에 리셋된 값(0,1일차 등)이 보이는 문제가 있어 이렇게 분리함
public static class PendingRunReset
{
    public static bool ShouldReset { get; private set; }

    public static void MarkForReset()
    {
        ShouldReset = true;
    }

    public static void Reset()
    {
        ShouldReset = false;
    }
}

// SceneTransitioner 가 Loading 씬으로 넘어가기 직전, 실제로 로드할 목표 씬 이름을 적어두는 용도
public static class SceneTransitionRequest
{
    public static string TargetSceneName { get; private set; }
    public static void Set(string targetSceneName) => TargetSceneName = targetSceneName;
}