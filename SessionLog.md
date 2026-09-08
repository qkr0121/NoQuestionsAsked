# SessionLog — No Questions Asked (ZombieWaveDefence)

마지막 업데이트: 2026-09-08

다른 PC / 새 세션에서 이어갈 때, 이 문서 하나만 읽으면 지금까지의 결정과 현재 상태를 파악할 수 있도록 정리했습니다. Unity/에디터 작업은 전부 사용자가 직접 진행하고, Claude는 설명·코드 안내만 담당(직접 코드 수정 금지 — Unity MCP로 씬/에셋 조작만 허용)하는 방식으로 작업해왔습니다.

---

## 0. 프로젝트 기본 정보

- 경로: `C:\Unity\ZombieWaveDefence`
- Unity 6000.3.21f1, URP
- New Input System만 사용 (`Keyboard.current` / `Mouse.current`), 레거시 Input Manager 없음
- Git 저장소 초기화 완료, GitHub Desktop으로 게시 완료 (`Initial commit` 이후 지금까지의 변경은 아직 커밋 안 됨 — 위 git status 참고)
- Unity MCP (MCP for Unity) 연동되어 있음 — 씬 조작은 이걸로 가능하나 종종 연결이 끊김, 그때는 Unity 에디터 포커스를 주면 재연결됨

---

## 1. 지금까지의 진행 단계 (큰 흐름)

1. **필수 과제 11단계 완료** (탑다운 웨이브 디펜스 슈팅 기본 골격) — 이동/조준/발사/적AI/웨이브스폰/충돌/체력/UI/게임오버/재시작 전부 구현 및 테스트 완료.
2. **One-page GDD 작성** — 게임을 정식 타이틀로 발전시키기로 하고 기획서 작성. Artifact로 게시: **https://claude.ai/code/artifact/5688964a-2d15-44d1-a894-eeb74df19cde** (제목: "No Questions Asked"). 같은 URL로 계속 갱신 중이므로 새 세션에서도 이 링크를 다시 읽으면 최신 기획 내용을 알 수 있음.
3. **Notion 포트폴리오** — "No Questions Asked" 페이지 안 "진행과정" 데이터베이스에 5개 하위 페이지 작성됨 (GDD 요약, 기술 스택, 필수 11단계, 로그라이트 전환 설계, 배운 점/버그 기록). Notion 링크는 사용자 워크스페이스에 있음 (검색 가능: "No Questions Asked").
4. **아키텍처 전환**: 단일 씬 → `Base`(지하실, 관리) / `ZWD`(전투) 2씬 구조 + `DontDestroyOnLoad` 영속 매니저.
5. **웨이브 선택 UI, HUD 공용화, 미션 보드 시스템**까지 구현 완료.
6. **세이브/로드 + MainMenu 씬 + 로딩/페이드 연출**까지 구현 완료 (아래 8번 섹션 참고). 이제 씬이 4개(`MainMenu`, `Loading`, `Base`, `ZWD`)로 늘어남.

---

## 2. 확정된 게임 기획 (GDD 핵심 — Artifact가 최신 소스지만 요약)

### 장르/구조
- **로그라이트**. 맵/적 랜덤화는 없음 — 대신 런 시작 시 무작위 요소(지원 동기, 나중엔 설계도 후보)로 매 런을 다르게 느끼게 함.
- **시대 배경**: 먼 미래(항성간 이주, 만능 제작기) + 장비는 근미래 현용 화기(총, 수류탄, 지뢰). "회사가 소모품 취급하는 지원자에게 최신 장비를 안 준다"는 설정으로 냉소적 톤 뒷받침.
- **아트 디렉션**: 스토리/UI는 어둡고 냉소적인 톤 유지, 3D 비주얼(캐릭터·좀비 전부 인간형)은 반대로 톰과 제리/디즈니/소닉풍 밝고 둥근 카툰 스타일 — 이 부조화 자체가 게임 정체성. VARCO 3D AI로 제작 예정. **UI를 밝게 맞추려는 시도는 의도를 해치므로 지양.**
- **스토리**: 크리스탈 좀비 행성 발견 → 크리스탈로 만능 제작기 가동 → 채굴 인력 전멸 → "이유는 묻지 않는다" 정책으로 지원자 모집 → 대원이 죽으면 다음 지원자가 그 자리에 섬(로그라이트 리셋의 서사적 근거).

### 런 구조
- 한 런 = 최대 100일. 지구 송출은 항상 100일째에만 발생(조기 클리어 없음 — 상세는 아래 "미션 시스템"으로 재설계됨, 원래 GDD의 "10일 단위 구간+경고 3회" 설계는 미션 시스템으로 대체 진행 중).
- 사망 시 즉시 런 종료.

### 재화
- **일반 크리스탈**: 흔함, 즉시 소비(방벽/무기 강화/미션 제출), 런 종료 시 보유량이 최종 점수. 런 끝나면 초기화.
- **희귀 크리스탈**: 드묾(밤·정예 처치), 기록엔 안 잡히지만 런 내 특수 장비/설계도 해금에 필요. 런 끝나면 초기화.

### 하루 구조
- 하루 = 아침/낮/밤 3슬롯. 슬롯마다 전투(소환기)/건설/수면 중 선택.
- 시간대 특성: 아침=적음/약함("정찰조"), 낮=물량 최대("채굴대"), 밤=적음/강함+희귀크리스탈↑("정예").

### 지원 동기 시스템 (미착수, GDD에 설계만 있음)
- 9종 동기(돈/사면/가족치료/기억상실/복수/갈곳없음/명예/위장잠입/호기심) 중 런 시작 시 4개 무작위 제시 → 플레이어 선택 → 세부 대상도 무작위.
- 유산 기록: 런 종료(클리어/실패) + 동기 조합으로 짧은 문구 자동 생성, 기지에 남아 다음 대원이 발견. **10일/슬롯 시스템 완성 후 착수하기로 명시적으로 미룸.**

### 보류 중인 결정 (아직 정하지 않음, 다시 논의 필요)
- 소환기가 자원을 소모하는지 여부
- 체력/수면 시스템 세부 규칙
- 설계도 후보 풀 크기
- 희귀 크리스탈 획득 방식 (드롭 vs 채굴)
- **웨이브 미선택 상태로 지상에 올라갔다 돌아올 때 시간(슬롯)이 흐르는 문제** — 3가지 옵션 중 미정:
  1. 항상 흐르게 하되 경고 메시지로 방지
  2. 특정 행동(건설 등)을 했을 때만 흐르게
  3. 아예 안 흐르게
- 사망 처리(`GameOverUI.Restart()`)를 로그라이트 규칙(런 리셋)에 맞게 재설계하는 것 — 지금은 그냥 씬 재시작만 함, 미착수.

---

## 3. 코드 아키텍처 현황

### 폴더 구조 (`Assets/_Project/Scripts/`)
```
Camera/   CameraFollow.cs
Combat/   Projectile.cs, ProjectilePool.cs
Core/     CurrencyManager.cs, GameBootstrap.cs, GameManager.cs, Mission.cs,
          PlayerManager.cs, RunState.cs, ScenePlayerSpawner.cs, UIManager.cs
Enemy/    EnemyAttack.cs, EnemyHealth.cs, EnemyMovement.cs, EnemyPool.cs
Interaction/ BaseExitPoint.cs, CombatExitPoint.cs, InteractableStructure.cs,
             LocksInputWhileAcitve.cs (오타 있음, 아래 참고), MissionBoardUI.cs
Player/   PlayerAim.cs, PlayerHealth.cs, PlayerInputLock.cs, PlayerInteractor.cs,
          PlayerMovement.cs, PlayerShoot.cs
UI/       DayCycleUI.cs, GameOverUI.cs, GameUI.cs, WaveClearUI.cs, WaveSelectionUI.cs
Wave/     CombatSceneEntry.cs, TimeOfDayModifier.cs, WaveDefinition.cs, WaveSpawner.cs
```
**참고**: `LocksInputWhileAcitve.cs`는 파일명에 오타(`Acitve` → `Active`)가 있음 — 기능엔 문제없으나 인지해둘 것. 정리하고 싶으면 사용자가 리네임하면 됨(Unity가 GUID로 참조를 유지하므로 안전).

### 씬 구조
- `Base.unity`: 지하실. 소환기(웨이브 선택 트리거), 미션보드, 지상行 출구(`BaseExitPoint`), HUD, 플레이어는 `ScenePlayerSpawner`가 동적 생성.
- `ZWD.unity`: 전투. `WaveSpawner`, `EnemyPool`, 스폰포인트(N/E/S/W 4방향만 존재, 8방향은 아직 미생성), 지하行 출구(`CombatExitPoint`), HUD, `CombatSceneEntry`(웨이브 시작 트리거), 플레이어는 마찬가지로 동적 생성.
- 두 씬 모두에 `Bootstrap`(GameBootstrap) 오브젝트 존재, `PersistentManagers.prefab`을 최초 1회 `DontDestroyOnLoad`로 인스턴스화.

### PersistentManagers 프리팹 안에 있는 것
`CurrencyManager`, `GameManager`, `RunState`, `ProjectilePool`, `PlayerManager`, `UIManager` — 전부 `static Instance` 싱글톤 패턴, `Awake()`에서 `Instance = this`.

### 핵심 시스템 상태

**웨이브/전투**
- `WaveSpawner`: 전멸전 구조 (`BeginWave(WaveDefinition, TimeOfDayModifier)` 명시적 시작, `spawnedCount`/`aliveCount` 분리 추적, `IsCleared`/`CanExit` 프로퍼티). 이전의 "시간 지날수록 강해지는 무한 웨이브" 구조는 완전히 제거됨.
- `WaveDefinition`(ScriptableObject): 적 수, 스폰 간격, `SpawnDirectionPattern`(Four/Eight enum, 8방향은 로직만 있고 실제 스폰포인트는 미생성), 희귀 크리스탈 확률.
- `TimeOfDayModifier`(ScriptableObject): 스폰수/강도/희귀드롭률 배율 3종 + `GetStrengthTier()`(Weak/Normal/Strong 등급 변환, UI에 원시 숫자 노출 금지).
- 에셋: `Assets/_Project/Data/WD_Wave01.asset`, `TOD_Morning/Day/Night.asset`.
- `EnemyHealth.Initialize(strengthMultiplier, onDeathCallback)`을 `SpawnEnemy()`에서 `Get()` 직후 명시 호출 — `OnEnable`은 최소 리셋만 담당(풀링 오브젝트는 `OnEnable`에서 진짜 초기화하면 안 됨, 호출자가 넘긴 데이터가 아직 없는 시점이라).

**런/미션 시스템 (진행 중, 최근 재설계됨)**
- **주의**: "10일마다 미션 발급, 100일 런" 같은 구체적 수치는 확정된 최종 값이 아니라 지금 잡아둔 큰 틀. 사용자가 명시적으로 "언제든 바뀔 수 있다(예: 5일마다 미션 제공, 50일 런 완료 등)"고 밝힘 — 코드에 하드코딩된 수치를 GDD 원문처럼 고정값으로 취급하지 말고, 나중에 사용자가 다른 값으로 바꿔도 자연스러운 일이라고 여길 것.
- 원래 계획했던 "10일마다 자동 판정 + 경고3회+이월" 구조를 버리고, **미션 보드판 방식**으로 교체 완료.
- `Mission.cs`: 순수 데이터 클래스(`[Serializable]`). `MissionName`, `RequiredCrystal`, `IssuedDay`, `DueDay`, `IsSubmitted`, `DaysRemaining()`, `IsOverdue()`(정확히 `currentDay > DueDay`일 때만 true — 마감일 당일까지는 제출 가능하도록 의도적으로 설계됨).
- `RunState.cs`: `currentDay`, `TimeSlot`(Morning/Day/Night) 3슬롯 진행, `CurrentMission`, `OnMissionIssued`/`OnMissionResolved` 이벤트(event Action). `AdvanceSlot()`이 Night→다음날 전환 시 `AdvanceDay()` 호출 → 그 안에서 (1) 기존 미션 기한 초과 체크 → 실패 처리, (2) 오늘이 발급일(`(currentDay-1) % missionInterval == 0`, 즉 1/11/21...)이면 새 미션 발급. **중요: 미션 제출 즉시 다음 미션을 주지 않음** — 오직 정해진 발급일에만 새 미션이 뜸(91일에 조기 제출해도 92~100일은 미션 없는 자유 구간이 되도록 하는 핵심 설계).
- `MissionBoardUI.cs`: 미션보드 상호작용 시 열리는 패널. `Start()`에서 `RunState.Instance.OnMissionIssued` 구독(← `OnEnable`에서 하면 `RunState.Instance`가 아직 null일 수 있어 NullReferenceException 발생했던 걸 고침, 실행순서 문제). `Open()`은 `panel.SetActive(true)` + `RefreshDisplay()` 강제 호출(이벤트만 믿으면 씬 재진입 시 상태가 낡은 채로 남는 버그가 있어서 추가함). `Awake()`에서 `panel.SetActive(false)`로 시작 시 숨김(최근 추가함, 이게 없어서 씬 시작/재진입 시 패널이 계속 떠 있던 버그 있었음). `RefreshDisplay()`는 아직 "미션 없음(제출완료 후 대기 중)" 상태를 명확히 구분해서 보여주도록 다듬는 중 — 마지막으로 안내한 방향은 `hasMission` bool로 미션 관련 텍스트/버튼 그룹과 "대기중" 텍스트를 SetActive로 전환하는 것. **이 부분이 실제로 코드에 반영됐는지는 다음 세션에서 확인 필요.**
- `CombatExitPoint.cs`: 예전에 있던 `AddSegmentProgress()` 호출은 삭제 대상으로 안내함(미션 시스템에서는 크리스탈 보유량 자체가 곧 진행도라 별도 누적이 불필요) — **반영 여부 미확인, 다음 세션에서 확인.**

**입력 잠금 시스템**
- `PlayerInputLock`(static class, MonoBehaviour 아님): `IsLocked` bool. `Lock()`/`Unlock()`.
- `PlayerMovement`/`PlayerAim`/`PlayerShoot`의 `Update()` 맨 위에 `if (PlayerInputLock.IsLocked) return;` 가드 추가됨.
- `LocksInputWhileAcitve.cs`(오타 파일명): `OnEnable`→`Lock()`, `OnDisable`→`Unlock()`. UI 패널 오브젝트(`WaveSelectionPanel`, `MissionBoardPanel`)에 부착해서 패널 활성화 상태와 입력 잠금을 자동 동기화. 이 방식(컴포넌트 분리, Unity 생명주기로 자동 처리)을 상속 기반 방식보다 명시적으로 선택함 — UI 스크립트마다 `Lock()`/`Unlock()`을 직접 호출하는 것도, 상속으로 묶는 것도 기각하고 이 방식으로 확정.

**웨이브 선택 UI**
- `WaveSelectionUI.cs`: 소환기 상호작용 시 열림. 웨이브 버튼 동적 생성(`Instantiate` 반복, `WaveButton.prefab`). 정보 표시는 `Label`/`Value` 두 컬럼(각각 `Vertical Layout Group` 개별 부착 필요 — 하나만 붙이면 정렬 안 맞음, 실제로 겪은 버그). 시간대는 `RunState.CurrentSlot` 인덱스로 `timeOfDayOptions` 배열에서 선택(배열 순서를 Morning/Day/Night로 맞춰 Inspector 등록 필요). `Confirm()`에서 `RunState.SelectWave()` 호출 후 `Close()`.
- **완료 확인(2026-09-07, 코드 리팩토링 중 재확인)**: `valueEnemyCount`에 `TimeOfDayModifier.SpawnCountMultiplier` 반영 — `WaveSelectionUI.ShowWaveInfo()`에 `WaveSpawner.BeginWave()`와 동일한 계산식(`Mathf.Max(1, Mathf.RoundToInt(wave.TotalEnemyCount * spawnMultiplier))`)으로 이미 구현되어 있음을 확인.
- 시간대 아이콘: **코드 로직(`UpdateTimeOfDayIcon()`)은 완성되어 있음.** 다만 이건 "코드 구현 여부"와 "실제 아이콘 에셋 여부"가 별개 — `morningIcon`/`dayIcon`/`nightIcon`에 연결된 실제 스프라이트가 아직 `Icon_Sky.png` 등 단색 임시 이미지 상태(아래 스프라이트 섹션 참고). **실제 시간대 아이콘 이미지 제작이 남은 작업.**

**HUD**
- `GameUI.cs`: 크리스탈/체력/날짜/슬롯 텍스트를 한 곳에서 표시. `RunState.CurrentDay`, `RunState.CurrentSlot` 참조.
- `Canvas`를 `HUD.prefab`으로 프리팹화해서 Base/ZWD 양쪽 씬에 배치 완료(사용자가 직접 함).
- `DayCycleUI.cs`도 존재하는데 이건 초기에 별도로 제안했던 것 — GameUI에 흡수하는 방향으로 안내했으나, 파일 자체는 아직 프로젝트에 남아있음(사용법 확인 필요, 중복/미사용일 수 있음).

**폰트**
- 에스코어드림(본문/UI용, S-Core 무료 라이선스, 임베딩 허용 확인됨) + 검은고딕(제목/헤딩용, SIL OFL 1.1, 임베딩 허용 확인됨). 둘 다 눈누(noonnu.cc)에서 다운로드, `Assets/_Project/Fonts/`에 위치.
- 겪은 버그: 한글이 □(tofu box)로 표시 → Custom Character List에 글자 누락이 원인, Font Asset Creator에서 재생성 필요. 텍스트가 흐림 → Atlas Resolution 부족(512→1024로 해결, Sampling Point Size는 이미 Auto라 무관했음).

**스프라이트**
- `Assets/_Project/Sprites/Icon_Sky.png`, `Icon_Red.png`, `Icon_Navy.png` — Claude가 `execute_code`로 생성한 64x64 단색 PNG 3장 (Sprite (2D and UI) 타입으로 임포트 완료). 시간대 아이콘 임시용으로 만듦. 사용자가 나중에 `2D Sprite` 패키지(Package Manager에서 설치 필요, 현재 미설치)로 직접 도형 스프라이트를 만드는 방법도 안내함.

---

## 4. 최근 발견된 버그 패턴 (다시 반복될 수 있으니 참고)

1. **부등호/논리연산자 반전 실수** — `PlayerAim.cs`(`<`를 `>`로), `PlayerInteractor.cs`(`!=`를 `==`로 등)에서 여러 차례 발생. 코드 리뷰 시 조건문을 문장으로 다시 말해보는 습관으로 사용자가 스스로 잡아내는 경우가 많아짐.
2. **오브젝트 풀링 이중 반납** — 타이머 만료 + 물리 충돌이 겹쳐 `pool.Release()`가 중복 호출되는 문제. `isReleased`/`isDead` 플래그로 "이미 처리됐으면 재실행 안 함" 가드 전체 적용.
3. **실행 순서(Execution Order) 문제** — `Awake()`/`Start()`/`OnEnable()` 순서가 스크립트 간 보장 안 되는 데서 오는 버그 반복 발생 (씬 동적 스폰 시 카메라/웨이브스포너 참조가 비는 문제, `RunState.Instance`가 아직 null인 상태에서 `OnEnable`이 구독을 시도하는 문제 등). 일반적 해법: 씬 진입 시점에 명시적으로 참조를 주입하는 `~SceneEntry`/`~Spawner` 스크립트를 두거나, 구독 타이밍을 `Awake`보다 늦은 `Start`로 옮김.
4. **Unity Layout Group 오해** — `Control Child Size`를 켜두면 자식 크기가 강제로 재계산됨(버튼이 납작해지는 원인이었음). `Vertical/Horizontal Layout Group`은 두 개의 병렬 컬럼(Label/Value)에 각각 독립적으로 붙여야 정렬이 맞음.

---

## 5. Deferred cleanup (사용자가 나중에 하겠다고 명시함, 지금 하지 말 것)

- 스크립트 전반에 `[Header("...")]` 어트리뷰트 추가 (Inspector 필드 그룹화)
- WHY가 명확하지 않은 곳에 코드 주석 추가 (프로젝트 정책: 코드가 스스로 설명 못 하는 비자명한 이유만 주석으로 — what이 아니라 why)
- 일반 코드 리팩토링 패스 (네이밍/구조 정리) — 시스템이 좀 더 안정된 뒤에.
- **에디터 Hierarchy 정리** (Base, ZWD 두 씬 모두) — 기능 하나씩 만들면서 쌓인 GameObject들 정리(빈 부모 폴더 오브젝트, 이름 일관성, 남은 테스트용 오브젝트 제거 등).
- `EnemyHealth`의 단일 `Action` 콜백을 `event Action<int> OnDamaged` / `event Action OnDied`로 전환 — 단, 실제로 히트 피드백(애니메이션/VFX/쉐이더) 시스템을 만들 때만.
- 적 종류 다양화 시, `EnemyMovement`가 지금 겸하고 있는 "풀 참조/정체성" 역할을 별도 `Enemy.cs`(또는 `EnemyIdentity.cs`)로 분리 — 컴포지션 기반으로 여러 적 종류를 조합 가능하게.
- 파괴 가능한 장애물 시스템 (좀비가 시야 막히면 부수는 AI) — 원래 필수 11단계 이후 추가 기능으로 계획했던 것.
- **전투 피드백(타격감) 구현** — 아래 6번 섹션에 설계 상세 있음. 아직 미착수.

---

## 6. 전투 피드백 설계 (논의 완료, 미착수)

사용자가 "전투가 심심하다"고 지적, 타격감이 핵심인 게임은 아니지만 최소한의 피드백은 원함. 좀비 쪽 제약 조건을 먼저 못박음:

- 좀비 애니메이션은 **걷기 + 공격 2종**뿐. 별도 피격 리액션 애니메이션 없음.
- 무기에 따라 **짧은 스턴**은 허용(좀비 행동을 중단시키는 유일한 예외).
- 구조물 공격 중이 아니면 좀비는 **무조건 플레이어를 향해 걷는다** — 새 AI 상태/애니메이션 없이 덧씌우는 방식의 피드백만 가능.

**확정된 범위** (탄피 배출은 제외, 나머지는 전부 채택):

- **플레이어/총**: FirePoint에 총구 화염 파티클, 총 모델 킥백(트랜스폼 오프셋을 코드로 흔들고 스프링처럼 복귀 — 애니메이션 클립 아님), 발사 시 짧은 카메라 셰이크, 발사/피격 사운드.
- **좀비**: 피격 시 머티리얼/쉐이더 색상 플래시(흰색 또는 빨간색, walk/attack 애니메이터 상태와 독립적으로 동시 재생 가능), 트랜스폼 레벨 넉백(기존 이동 로직 위에 임시 위치 오프셋만 얹음 — AI 판단/경로는 안 바뀜), 스턴은 애니메이터 speed 파라미터를 0으로(새 클립 불필요), 피격/그로울/사망 사운드.
- **피 튀김**: Particle System 방식. Shape=Cone, Gravity Modifier로 포물선 낙하, Burst emission(약 10~20개), Render Mode=Billboard(탑다운에서도 잘 보임), Stop Action=Destroy로 자동 정리. 색상은 카툰 좀비 톤에 맞춰 **밝고 선명한 빨강**(어둡고 그로테스크하지 않게).
- **미루기로 한 것**: 바닥에 남는 피 얼룩(Decal Projector 또는 pooled quad+texture 방식) — 나중에 진행, 지금은 공중 튀김 파티클만.

구현 시작 전에 이 설계를 그대로 참고해서 진행하면 됨.

---

## 8. 세이브/로드 · MainMenu · 로딩/페이드 시스템 (구현 완료)

새 씬 `MainMenu.unity`(빌드 인덱스 0)와 `Loading.unity`을 추가해 씬이 총 4개(`MainMenu`, `Loading`, `Base`, `ZWD`)가 됨.

**세이브/로드**
- `SaveData.cs` 한 파일에 독립 top-level 클래스 3개(중첩 아님, 그냥 이름으로 바로 참조): `MissionSaveEntry`(미션을 배열로 저장 — 지금은 0~1개뿐이지만 나중에 여러 미션 동시 진행 확장 대비), `PlayerSaveData`(지금은 체력만, 무기/웨이브해금트리는 나중에 필드 추가 예정), `SaveData`(day/slot/crystal/rareCrystal + missions 배열 + player + HasSaveData).
- `SaveManager.cs` (`PersistentManagers`, static Instance 패턴): JSON 직렬화(`JsonUtility`)로 저장/로드/삭제. **`HasSaveData` 필드는 현재 Save()에서 세팅 안 됨(죽은 필드) — HasSave()는 파일 존재 여부로만 판단하니 실害 없지만 나중에 정리 대상.**
- **임시 디버그 설정**: `SaveManager.SavePath`를 `Application.persistentDataPath` 대신 `Application.dataPath/_Project/save.json`으로 변경해 `Assets/_Project/save.json`에서 직접 확인 가능하게 함. **반드시 실제 빌드 전에 `Application.persistentDataPath`로 되돌려야 함** (`Application.dataPath`는 에디터에서만 Assets 폴더를 가리키고 빌드에서는 다른 경로를 가리킴). 지금은 의도적으로 이 save.json을 **git에 커밋**해서 여러 PC 간 동일한 세이브로 이어서 테스트할 목적 — `.gitignore`에 넣지 않음(넣으면 다른 PC에 세이브가 안 딸려옴). 실제 빌드 준비 시점에 경로 원복 + 이 파일 정리 필요.
- 자동저장은 `RunState.AdvanceSlot()` 안에서만 트리거(슬롯/날짜 넘어갈 때). **의도적으로** `GameUI.OnReturnToMainMenuClicked()`에는 저장 호출을 넣지 않음 — 슬롯 중간에 메뉴로 나가면 그 진행은 저장 안 되는 게 설계 의도(일부러 손실 나게 함), 나중에 "왜 저장 안 하지"라고 헷갈리지 말 것.

**실행 순서 버그와 해결 (기록해둘 가치 있음)**
- `RunState`가 `PersistentManagers`(`DontDestroyOnLoad`)에 있고, `MainMenu` 씬에도 `Bootstrap`이 있어서 `RunState.Awake()`가 `MainMenu` 로드 시점에 이미 실행됨. 원래 로드-체크 로직을 `RunState.Start()`에 뒀더니, Base 씬 진입 시점엔 이미 `Start()`가 지나간 뒤라(같은 인스턴스가 살아남아서) 전혀 실행되지 않는 버그가 있었음. **해결**: `Start()`를 없애고 `Awake()`에서 `SceneManager.sceneLoaded`를 구독, `scene.name == "Base"`일 때만 반응 + `if (CurrentMission != null) return;`로 이미 진행 중인 런에서 Base↔ZWD를 오갈 때 재트리거되지 않게 가드. **교훈**: DontDestroyOnLoad 오브젝트에서 "씬 X가 로드될 때 뭔가 하기" 로직은 `Start()`가 아니라 `SceneManager.sceneLoaded`를 써야 함 — `Start()`는 살아남은 인스턴스에서 다시 안 불림.
- "새로 시작"이 세이브 파일만 지우고 메모리 상의 `RunState`/`CurrencyManager`/`PlayerManager` 값은 그대로 남아 이전 런 이어서 시작되는 버그도 같은 원인 계열. **해결**: `RunState.ResetForNewRun()`, `CurrencyManager.ResetCrystal()`을 추가해 `MainMenuUI.OnNewGameClicked()`에서 명시 호출 + `PlayerManager.SetHealth(max,max)`로 체력도 리셋. **교훈**: DontDestroyOnLoad 싱글톤은 "새로 시작" 시나리오를 위한 명시적 리셋 메서드가 필요함 — 세이브 파일 삭제는 디스크만 지울 뿐 살아있는 인스턴스의 메모리 상태는 안 건드림.

**MainMenu 씬**
- `MainMenuUI.cs`: 이어하기(`SaveManager.HasSave()`로 표시 여부 결정 후 `PendingLoad.ShouldLoad=true` 설정 → 씬 이동), 새로시작(세이브 삭제 + 위 리셋들 호출), 설정(미구현), 종료(`Application.Quit()`, 에디터에서는 no-op, 빌드에서만 동작).
- 버튼 4개를 `ButtonContainer`(`Vertical Layout Group`, Control Child Size는 Width만 체크/Height는 체크 해제, Child Force Expand Width) 아래 자식으로 배치 — 이어하기 버튼이 `SetActive(false)`되면 Layout Group이 자동으로 나머지 3개를 재정렬하므로 별도 정렬 코드 불필요.
- 배경은 임시로 단색 Panel(`#1C1A15`, GDD에서 쓰던 표면색) — 화면 전체를 덮으면 사실상 거의 검정으로 보임(육안 차이 미미), 어차피 나중에 실제 배경 이미지로 교체 예정이라 지금은 무시하고 넘어감.

**Loading 씬 (MainMenu↔Base 전용)**
- `GlobalFlags.cs`에 `SceneTransitionRequest`(static, `TargetSceneName` 문자열 하나) 추가 — `PlayerInputLock`, `PendingLoad`와 함께 한 파일에 모음(cs 파일 개수가 39개까지 늘어나 파악하기 어려워져서, 이후 이런 순수 static 플래그/데이터 클래스들은 이 파일에 모으기로 함. MonoBehaviour + Instance 싱글톤류는 계속 개별 파일 유지).
- `SceneTransitioner.cs`(static, `GoTo(sceneName)`): 목표 씬 이름을 `SceneTransitionRequest`에 적어두고 `"Loading"` 씬으로 이동.
- `LoadingScreen.cs`: `SceneManager.LoadSceneAsync` + `operation.allowSceneActivation = false`로 90%에서 대기시켜두고, 최소 표시 시간(1.2초)과 실제 로드 진행률 중 더 느린 쪽을 프로그레스바에 반영, 로드 완료 + 최소시간 경과 둘 다 만족해야 `allowSceneActivation = true`로 실제 전환. 씬이 가벼워서 로딩이 순식간에 끝나도 로딩 화면이 최소 시간만큼은 항상 보이도록 하기 위한 장치.
- `MainMenuUI`의 씬 이동 호출을 전부 `SceneTransitioner.GoTo(...)`로 교체.

**Base↔ZWD 페이드 인/아웃 (Loading 씬 대신, 더 빈번한 전환이라 가벼운 연출 선택)**
- `SceneFader.cs`(`PersistentManagers`, static Instance): `FadeToScene(name)` → 화면을 검게 페이드아웃 → `SceneManager.LoadScene`(동기, Base/ZWD는 가벼워서 문제없음) → 페이드인.
- `FadeCanvas`(Sort Order 높게) + `FadeImage`(화면 전체 덮는 검은 Image, 초기 Alpha 0)를 `PersistentManagers`에 추가. **겪은 버그**: Alpha 0이라도 `Image`의 `Raycast Target`이 켜져 있으면 투명해도 클릭을 가로채서 뒤의 버튼이 안 눌림 — `Raycast Target` 체크 해제로 해결. 렌더링 자체는 계속 되고 있지만(완전 투명 풀스크린 사각형 1장) 성능에 영향 없는 수준이라 그냥 두기로 함(SetActive로 껐다 켜는 최적화는 불필요하다고 판단).
- `BaseExitPoint.Exit()`, `CombatExitPoint.TryExit()`의 `SceneManager.LoadScene(...)`를 `SceneFader.Instance.FadeToScene(...)`로 교체.

---

## 7. 다음에 할 일 (우선순위 순, 사용자가 마지막에 확인한 흐름 기준)

**완료됨**: 미션 보드 UI hasMission 분기 / 웨이브 선택 UI 시간대 배율 반영 / CombatExitPoint AddSegmentProgress 삭제 / 세이브·로드 시스템 / MainMenu 씬 / 인게임→메인메뉴 복귀 버튼 / Loading 씬(MainMenu↔Base) / 페이드 인아웃(Base↔ZWD). 상세는 위 8번 섹션.

**3D 모델 적용 완료** (2026-09-03): VARCO 3D로 제작한 카툰풍 모델을 플레이어/HeavyZombie/Summoner(소환기)/MissionBoard/ExitPoint(Base·ZWD 공용 엘리베이터)까지, Base·ZWD 씬의 바닥/벽/천장까지 전부 적용 완료. **지금 존재하는 기능 범위 내에서는 3D 모델 적용 단계 끝** — CrystalRock/Store/Bed는 해당 기능(채굴/상점/수면) 자체가 아직 없어서 모델만 대기 중, 새 기능 만들 때마다 그때그때 모델 적용이 필요함(예: 새 웨이브의 적 종류 추가 시 등).
- **Base 바닥/벽/천장**: fbx 모델이 아니라 Plane/Cube 프리미티브 + 이미지 생성(Nano Banana Pro)으로 뽑은 seamless 타일링 텍스처로 구현. 이유: 바닥/벽처럼 반복 배치가 전제되는 평면 구조물은 통짜 3D 모델보다 텍스처 타일링 방식이 맵 크기 변경에 유연하고 이음새 문제도 적음. 스타일은 소환기/미션보드(카툰이지만 리벳·금속 하이라이트가 있는 "살짝 사실적인 스타일라이즈드" 톤)에 맞춰 "카툰 SF" 절충으로 결정, 색상은 회색 콘크리트 톤이 아니라 철/스틸 톤으로. 세계관상 벽/바닥은 크리스탈 없는 순수 인간 건축(소환기 등 장비만 외계 크리스탈 기술)이라는 설정 반영해 프롬프트에 명시적으로 크리스탈 배제.
- **콜라이더-바닥 정렬 팁**: Plane과 Cube는 피벗 위치가 다름(Plane은 표면 중앙, Cube는 정육면체 중앙) — 벽(Cube) Position.y를 `벽 높이/2`로 맞춰야 바닥(Plane, Y=0)과 밑면이 딱 맞음. 정확히 맞추려면 Scene 뷰에서 `V` 키를 누른 채 벽의 모서리 버텍스를 바닥 모서리 버텍스로 드래그하는 버텍스 스냅이 가장 정확함.
- 플레이어: `PlayerCharacterPistolIdle.fbx`(Idle) + `Pistol Walk.fbx`(Mixamo 소스 Walk 애니메이션)를 Humanoid Animator로 연결. `PlayerMovement.cs`에 `Animator` 참조 추가, 입력 있으면 `IsMoving` bool true/false로 Idle↔Walk 전환.
- **겪은 버그와 원인 (기록해둘 가치 있음)**:
  - Walk 애니메이션 재생 시 캐릭터가 맵 중앙으로 이동해버리는 문제 → fbx Animation 클립의 `Root Transform Position (XZ)`가 `Bake Into Pose`(keepOriginalPositionXZ) 꺼진 상태라, 원본 캡처의 실제 이동 궤적이 그대로 재생된 것. XZ Bake Into Pose 체크로 해결.
  - 이동은 멈췄지만 방향이 살짝 틀어짐 → 같은 원리로 `Root Transform Rotation`의 Bake Into Pose(lockRootRotation)가 꺼져 있던 게 원인. 체크로 해결.
  - Y축으로 약 1 정도 뜬 채로 걷는 문제 → 처음엔 피벗 위치 차이로 추정했으나 실제 원인은 **스케일 불일치**: Idle 모델은 Blender에서 3배 확대 + 피벗 중앙 이동 작업을 거쳤지만, Walk 애니메이션(Mixamo 원본)은 그 작업을 안 거쳐 리그 크기가 3배 차이 났음(Hips~발 높이 비교로 확인). Walk fbx도 Blender에서 동일하게 3배 확대 후 재출력하여 해결. **교훈**: 같은 캐�릭터에 물릴 여러 fbx(모델/애니메이션 별도 파일)는 반드시 동일한 스케일 작업을 거쳐야 함 — 하나만 스케일 조정하면 Humanoid 리타겟팅의 루트 높이 계산이 어긋나 위아래로 뜨거나 붕 뜨는 것처럼 보임.
- 좀비/소환기/미션보드는 기존 프리미티브(캡슐/큐브) 자리에 fbx를 자식으로 넣고 시각 컴포넌트(MeshFilter/MeshRenderer)만 제거하는 방식으로 교체.
- **Summoner(소환기) 콜라이더 변경**: 기존 BoxCollider가 소환기의 좁은 실제 형태보다 넓게 감싸서 빈 공간까지 막는 문제가 있어 MeshCollider(Non-convex, Is Trigger 꺼짐)로 교체. 플레이어의 `PlayerInteractor`가 이미 `SphereCollider`+`Rigidbody`로 트리거 이벤트를 발생시키므로(트리거는 둘 중 하나만 있어도 발생), Summoner 쪽 콜라이더는 Solid로 두면서 막힘+상호작용 감지를 콜라이더 하나로 겸함. 정적 오브젝트라 MeshCollider 성능 비용도 무시할 수준.
- MissionBoard도 동일한 방식으로 모델 교체 완료(비균등 스케일 이슈는 실제로 안 걸림, 그대로 진행).
- **CrystalRock(Purple/Emerald 1~3), Store, Bed는 모델은 임포트 완료되었으나 기능(채굴, 상점, 수면) 자체가 아직 미구현이라 배치/연결은 보류** — 해당 기능 만들 때 같이 진행하기로 함.

**3D 모델 적용 단계 상세 기록**:
- **ExitPoint 디자인 결정**: 문/포탈 대신 원기둥 엘리베이터로 확정 — 세계관(순수 인간 건축, 크리스탈 없음)과 지하/지상 수직 이동 공간감에 가장 잘 맞음. Base·ZWD 양쪽에 완전히 동일한 모델(`ExitElevator.fbx`) 사용(하나의 승강 장치가 오르내리는 설정). 난간까지 있는 모델이라 실제로 막는 콜라이더를 만들면 문 여는 연출 등이 필요해져서, **의도적으로 난간도 그냥 통과되도록 둠**(나중에 여유 생기면 난간 없는 버전으로 재생성 검토).
- **배치 방식**: 계단 등 실제 Y축 이동 구현은 지금 `PlayerMovement.cs`(순수 수평 이동만 처리)를 크게 건드려야 해서 범위 밖 — 대신 엘리베이터 모델을 Ground 아래로 파묻어 발판 상단만 보이게 배치, 콜라이더는 `SphereCollider`(Is Trigger, 발판 중앙 근처 좁은 범위) 하나만 사용. `BaseExitPoint`/`CombatExitPoint` 오브젝트에 `InteractableStructure`+ExitPoint 스크립트+`SphereCollider`가 함께 있고, 모델(`ExitElevator`)은 자식으로 MeshFilter/MeshRenderer만 가진 순수 시각 오브젝트 — 소환기 때와 같은 패턴. Base/ZWD 양쪽 다 동일한 로컬 Transform 값으로 배치해서 시각적으로 통일.
- **`interactPrompt` 필드는 모든 InteractableStructure(소환기/미션보드/엘리베이터 전부)에서 아직 비어 있음(null)** — `SetPromptVisible`이 null 체크로 방어되어 있어 에러는 안 나지만, 상호작용 시 화면에 안내 UI 자체가 안 뜸. 프롬프트 UI(예: "F: 상호작용" 텍스트)는 아직 미구현 상태로, 별도 작업 필요.
- **나중에 할 일(폴리싱, 지금은 보류)**: 엘리베이터 몸통이 안 보여서 "이게 엘리베이터"라는 게 헷갈릴 수 있는 문제 — (a) interactPrompt 구현 시 "엘리베이터 탑승/지상으로 이동" 등 명확한 문구로 채우기, (b) 상판 가장자리에 발광 라인이나 스포트라이트 조명으로 강조. 몸통 일부 노출은 구현 난이도상 보류 결정.

**카메라 줌 인/아웃 기능 완료** (2026-09-03): Base 지하 기지가 고정 카메라 거리로는 너무 멀어 보이는 문제로 착수, 마우스 휠로 카메라 거리를 조절하는 기능 추가.
- `CameraFollow.cs`에 `zoomLevel`(배율, 기본 1)을 추가하고, 기존 `offset` 벡터에 이 배율을 곱해서 카메라-플레이어 거리만 조절되도록 함(방향/각도는 고정). 마우스 휠 입력은 New Input System의 `Mouse.current.scroll`을 직접 읽는 방식(프로젝트 전체가 이미 `Keyboard.current`/`Mouse.current` 직접 조회 패턴이라 Input Actions 에셋 등록 없이 바로 사용 가능했음).
- **겪은 버그**: 처음 구현 시 `Update()`에서 `zoomLevel` 값은 마우스 휠에 따라 정상적으로 계산되고 있었지만, 실제 카메라 위치를 계산하는 `LateUpdate()`의 `desirePosition = target.position + offset` 식에는 이 값이 곱해지지 않아 화면상 아무 변화가 없었음. `offset * zoomLevel`로 수정해 해결 — New Input System 입력 인식 문제가 아니라, 계산된 값을 실제로 사용하지 않은 단순 로직 누락이었음.
- **씬별 기본 줌 차이**: Base(지하실)와 ZWD(전투)는 원하는 기본 카메라 거리가 다름. `zoomLevel`을 코드에서 고정값(`= 1f`)으로 초기화하면 Play 모드마다 항상 그 값으로 리셋되어, 씬마다 다른 기본값을 주기 어려웠음. 씬 이름으로 분기하는 조건문 대신, `initialZoom`이라는 별도 `[SerializeField]` 필드를 두고 `Awake()`에서 `zoomLevel = initialZoom`으로 초기화하는 방식을 택함 — Base/ZWD 각 씬의 카메라 오브젝트 인스펙터에서 값만 다르게 넣으면 되므로, 코드를 씬 이름에 의존시키지 않고 순수하게 데이터(인스펙터 값) 차이로 처리.

남은 목록 (다음 목표: **상점 구현 → 좀비 종류 추가 → 웨이브 추가 → 프로토타입 v1**):

1. **사망 처리 로그라이트화 — 1차 마무리 완료 (2026-09-07)** (`GameOverUI.cs`가 필수 11단계 시절의 낡은 `Restart()` 로직을 그대로 갖고 있어 로그라이트 구조와 안 맞는 상태를 발견하고 재설계, 실제 구현까지 완료):
   - **UI**: 사망 시 뜨는 패널에 생존 일수(`RunState.CurrentDay`)와 최종 크리스탈(`CurrencyManager.Crystal`) 표시. 버튼은 "메인메뉴로" 하나만 — `survivedDaysText` 필드 추가, `Restart()` 삭제하고 `OnReturnToMainMenuClicked()`로 교체 완료.
   - **`RunState.ResetEntireRun()` 신설**: `ResetForNewRun()` + `CurrencyManager.ResetCrystal()` + `PlayerManager.ResetHealth()` + `GameManager.ResetGameOver()`를 한 번에 호출하는 조율 메서드. `MainMenuUI.OnNewGameClicked()`와 `GameOverUI.OnReturnToMainMenuClicked()` 양쪽이 이 메서드 하나로 통일됨(코드 중복 제거).
   - **겪은 버그 1 — `GameManager.IsGameOver`가 두 번째 런에서 리셋 안 됨**: `GameManager`도 영속 매니저라 `IsGameOver = true`가 씬을 재로드해도 안 풀림 → 두 번째 런에서 체력이 0이 돼도 `GameOver()`의 `if (IsGameOver) return;` 가드에 막혀 게임오버가 아예 트리거 안 됨. `GameManager.ResetGameOver()` 추가 후 `ResetEntireRun()`에 포함시켜 해결.
   - **겪은 버그 2 — 메인메뉴 복귀 시 HUD에 리셋값이 잠깐 노출**: `GameOverUI`가 직접 `ResetEntireRun()`을 호출하면, 씬 전환이 실제로 일어나기 전 몇 프레임 동안 ZWD의 HUD가 리셋된 값(크리스탈 0, 1일차 등)을 잠깐 보여주는 깜빡임이 있었음. **해결**: `PendingRunReset`(GlobalFlags.cs, `PendingLoad`와 동일 패턴) static 플래그를 신설해, `GameOverUI`는 리셋을 요청만 하고 실제 `ResetEntireRun()` 호출은 `LoadingScreen.LoadTargetScene()` 코루틴 맨 앞(화면이 이미 로딩 UI로 가려진 시점)으로 옮김 — 화면 전환 중 상태 변경은 "가려진 뒤에" 처리한다는 패턴으로 일반화 가능.
   - **씬 전환 방식**: `SceneFader`(페이드)가 아니라 `SceneTransitioner.GoTo("MainMenu")`(Loading 씬 경유) — 기존 원칙(Loading은 무거운/저빈도 전환용, SceneFader는 가벼운/고빈도 전환용)을 그대로 적용.
   - **"1차" 마무리인 이유(남은 것 2가지, 아래 항목으로 분리)**: ① 런 결과(생존 일수/크리스탈) 기록 시스템 미착수 — 지금은 리셋 시 그냥 사라짐. ② 씬 전환 중 입력 잠금 미착수 — ExitPoint 연타 시 페이드 코루틴이 중복 시작되어 화면이 깜빡이는 문제 확인함(에러는 안 남).
2. **다른 웨이브 추가** — 지금은 `WD_Wave01` 하나뿐, 프로토타입 v1을 위해 몇 개 더 필요.
3. 상호작용 프롬프트 UI 구현 — 위에서 언급한 대로 모든 InteractableStructure의 `interactPrompt`가 비어 있어 상호작용 가능 여부가 화면에 안 보임. 별도 UI 오브젝트(예: "F" 아이콘 + 안내 텍스트) 제작 필요.
4. 슬롯 소비 지점 확장 — 지금은 전투 복귀시(`CombatExitPoint`)에만 `AdvanceSlot()` 호출됨. 건설(Store)/수면(Bed) 상호작용을 실제 기능으로 구현하며 슬롯 소비 흐름 전체 검증 — Store/Bed 모델은 이미 임포트되어 있으니 기능 구현 시 바로 배치 가능.
5. **미션 시각적 피드백 (구체 계획 확정됨)**:
   - 새 미션 발급 시 미션보드 오브젝트 위에 느낌표 표시(월드스페이스 UI 또는 3D 오브젝트) — `RunState.OnMissionIssued` 이벤트를 구독해서 구현 예정.
   - 미션을 처음 확인(패널 진입) 또는 수락 버튼 클릭 중 하나의 시점에 물음표로 전환 — 이건 이벤트가 아니라 `MissionBoardUI.Open()`/`OnSubmitClicked()`에서 표시기 오브젝트를 직접 호출하는 방식으로 구현 예정.
   - 실패 피드백은 별도로 만들 예정(성공/실패 공용 피드백 아님).
   - **현재 상태**: `RunState.OnMissionIssued`/`OnMissionResolved` 둘 다 구독자가 없는 상태(`MissionBoardUI`가 구독하던 걸 죽은 코드로 판단해 제거함 — `Open()`이 이미 강제 갱신하고 있어서 이벤트 구독이 중복이었음). 다만 이 이벤트들 자체는 위 느낌표/물음표 기능을 위해 의도적으로 남겨둠, 삭제 아님.
6. **웨이브 클리어 결과창 (설계 방향 확정, 구현은 나중)**: 지금 `WaveClearUI.cs`는 `WaveSpawner.IsCleared`가 true가 되는 순간 그냥 빈 패널 하나를 띄우기만 하는 최소 골격 상태. 실제 목표는:
   1. **1단계 — CLEAR! 이펙트**: 짧은 연출(텍스트/이펙트), 자동 재생 후 사라짐.
   2. **2단계 — 결과창**: 이번 웨이브에서 획득한 크리스탈(일반/희귀), 획득한 설계도(나중에 추가) 등을 표시. 확인 버튼 클릭 시 패널이 닫힘.
   3. **나갈 수 있는 조건 변경 필요**: 지금 `WaveSpawner.CanExit => !spawningStarted || IsCleared;`는 클리어 즉시 나갈 수 있게 하는데, 결과창을 반드시 확인해야 나갈 수 있도록 `ResultsConfirmed` bool을 추가해 `CanExit => !spawningStarted || (IsCleared && ResultsConfirmed);`로 변경 예정. 확인 버튼 클릭 시 `ResultsConfirmed = true` 처리.
   4. **"이번 웨이브 획득량" 추적 필요**: `CurrencyManager.Crystal`은 누적 총량이라, 결과창에 표시할 "이번 웨이브에서만 얻은 양"은 웨이브 시작 시점 크리스탈량을 별도로 기록해뒀다가 클리어 시점과의 차이로 계산해야 함(`WaveSpawner.BeginWave()`에서 시작 시점 스냅샷 저장).
   5. **선행 조건**: 희귀 크리스탈 재화 자체가 아직 `CurrencyManager`에 없음(SaveData에만 자리만 있음, 8번 섹션 참고), 설계도 시스템도 미착수 — 이 결과창을 온전히 구현하려면 두 시스템이 먼저 또는 함께 진행되어야 함.
7. "웨이브 미선택 상태로 지상 다녀오면 시간 흐르는 문제" 기획 결정 (3옵션 중 선택 필요).
8. **전투 피드백 구현** — 설계는 위 6번 섹션에 완료, 실제 구현 착수. (총구화염/킥백/카메라셰이크 → 좀비 색상플래시/넉백/스턴 → 피 튀김 파티클 → 사운드 순서 추천)
9. CrystalRock(채굴 기능) 구현 — 모델(Purple/Emerald Crystal 1~3)은 준비됨, 채굴 로직/재화 연결 미착수.
10. **빌드 전 필수**: `SaveManager.SavePath`를 `Application.persistentDataPath`로 원복, `Assets/_Project/save.json` 정리.
11. **런 결과 기록 시스템 (사망 처리 로그라이트화에서 분리, 별도 작업으로 미룸)**: 지금 `RunState.ResetEntireRun()`은 이번 런의 결과(생존 일수, 최종 크리스탈)를 어디에도 남기지 않고 그냥 리셋해버림 — GDD의 "런 종료 시 남은 크리스탈이 순위표에 오른다"는 기록 시스템 자체가 아직 없음. `GameOverUI.Show()`가 화면에 잠깐 보여주는 게 전부. 순위표/최고기록을 영속 저장할지, 어떤 형태로 할지 결정 필요.
12. **HUD/UI 텍스트 → 이미지 전환**: 지금 `GameUI`, `GameOverUI`, `WaveSelectionUI` 등 대부분의 정보 표시가 TMP_Text 텍스트임. 체력, 크리스탈, 슬롯(아침/낮/밤) 등 대부분을 이미지(게이지 바, 아이콘 등)로 교체 예정 — "Day 1"처럼 가변 숫자라 텍스트가 필수인 것들은 제외.
13. **씬 전환 중 입력 잠금 — 완료 (2026-09-07)**: `ExitPoint`에서 E키를 연타하면 `SceneFader.FadeToScene()`(코루틴)이 중복으로 시작되어 화면이 깜빡이던 문제(에러는 안 남) 해결.
    - `SceneFader.cs`: `isTransitioning` bool 필드 추가 — `FadeToScene()` 시작 시 이미 전환 중이면 즉시 return(중복 코루틴 시작 원천 차단), 아니면 `PlayerInputLock.Lock()` 호출 후 코루틴 시작. `FadeRoutine()` 끝(페이드 인 완료 후)에서 `PlayerInputLock.Unlock()` + `isTransitioning = false`.
    - `PlayerInteractor.cs`: `Update()` 맨 위에 `if (PlayerInputLock.IsLocked) return;` 추가 — 다른 입력 스크립트(PlayerMovement/PlayerAim/PlayerShoot)와 동일한 가드를 상호작용에도 적용, E키 연타 자체가 `Interact()` 호출로 이어지지 않게 원천 차단.
    - `LoadingScreen.cs`: 처음엔 예방적으로 `Start()`에서 `PlayerInputLock.Lock()`, 코루틴(`while` 루프) 끝나고 `Unlock()`하는 패턴을 적용했으나, **실제 버그로 이어짐(아래 참고)**.
    - 정리하면 `SceneFader`(Base↔ZWD)는 이중 방어(코루틴 중복 차단 + 입력 잠금), `LoadingScreen`(MainMenu↔Base 등)은 입력 잠금만으로 충분 — 단 Unlock 호출 위치는 아래 버그 수정 반영.
    - **겪은 버그 — 입력 잠금이 영구히 안 풀림 (2026-09-07, 플레이테스트로 발견)**: Base 씬에서 메인메뉴로 갔다가 다시 Base로 돌아오면 `PlayerInputLock.IsLocked`가 계속 `true`로 남아 이동/조준/발사/상호작용이 전부 막힘. `Debug.Log`로 추적한 결과 `LoadingScreen.LoadTargetScene()` 코루틴 끝의 `PlayerInputLock.Unlock()`이 아예 실행되지 않는 것으로 확인됨.
      - **원인**: `operation.allowSceneActivation = true`로 설정하는 순간 Loading 씬이 실제로 언로드되면서, 그 코루틴을 실행 중이던 `LoadingScreen` 오브젝트 자체가 파괴됨 — `while` 루프 밖(코루틴 맨 끝)에 있던 `Unlock()`까지 도달하기 전에 코루틴이 강제 중단되는 것이었음. `GameBootstrap.Awake()`에서 씬 진입 시 무조건 `Unlock()`을 부르는 방식도 고려했으나, 이러면 `SceneFader`가 페이드 인이 끝나기 전에 입력을 막아두는 보호가 무력화되는 부작용이 있어 기각.
      - **해결**: `Unlock()` 호출 위치를 코루틴 끝이 아니라, **`allowSceneActivation = true`로 설정하기 직전 줄**로 이동 — 이 시점엔 `LoadingScreen` 오브젝트가 아직 파괴되기 전이라 확실히 실행됨. `SceneFader.cs`는 건드리지 않아 그쪽 보호 로직은 그대로 유지.
      - **남은 이론적 허점(무해함, 주석으로 남겨둠)**: `Unlock()` 실행 시점과 실제 씬 전환 사이에 짧은 시차(보통 한 프레임)가 있어 그 사이엔 "Loading 화면이 아직 보이는데 입력은 이미 풀린" 상태가 되지만, Loading 씬엔 플레이어 오브젝트 자체가 없어 실질적 영향 없음. 나중에 Loading 씬에도 페이드 등 별도 연출이 추가되면 이 타이밍을 재검토 필요.
14. **플레이어 조준 회전 부드럽게 — 완료 (2026-09-07)**: `PlayerAim.cs`의 `transform.rotation = targetRotation;`(즉시 스냅)이 마우스가 반대 방향으로 휙 움직일 때 180도 회전이 순간이동처럼 부자연스러웠던 문제. `EnemyMovement.cs`가 이미 쓰던 것과 동일한 지수 감쇠 `Quaternion.Slerp` 패턴(`1 - Mathf.Exp(-rotationSpeed * Time.deltaTime)`)으로 교체, `rotationSpeed`(기본값 15) 필드를 인스펙터에 노출해 조절 가능하게 함 — 좀비(8)보다 플레이어는 훨씬 즉각적으로 반응해야 하므로 더 큰 기본값 사용.
15. **플레이어 Idle/Walk 애니메이션 방향 불일치 — 얼추 해결 (2026-09-07)**: Idle 상태에서 사격 방향이 어긋나 모델을 y=30 회전으로 보정했더니, Walk로 전환되면 그 보정이 다시 어긋나 보이는 문제.
    - **원인 조사**: 두 fbx(`PlayerCharacterPistolIdle.fbx`, `PlayerCharacterPistolWalk.fbx`)의 정적 바인드 포즈(Armature localRotation)는 거의 동일(270도, 오차 범위)했으나, 각각 `Avatar Definition = Create From This Model`로 별도의 Avatar(`...IdleAvatar`, `...WalkAvatar`)를 갖고 있어 리타겟팅 기준이 미세하게 다를 가능성을 의심 — Walk의 Avatar를 Idle 것으로 공유(`Copy From Other Avatar`)하도록 시도했으나 그래도 방향 차이(5~10도)가 남아있어, 결국 **원본 캡처 자체(모션 캡처/AI 생성 소스)의 미세한 회전 오차**로 결론.
    - **해결 방법**: 모델 오브젝트 전체를 통째로 회전시키는 방식(Idle 기준 y=30) 대신, 각 fbx 클립의 **Root Transform Rotation → Offset** 값(임포트 설정, `ModelImporterClipAnimation.rotationOffset`)을 클립별로 독립적으로 조정 — 원본 fbx나 리그를 건드리지 않고 Unity 임포트 메타데이터에만 저장되는 값이라 안전하고 되돌리기 쉬움. Idle과 Walk 각각 다른 Offset 값으로 얼추 맞춤.
    - **참고**: 사격 애니메이션은 별도로 만들지 않고 Idle/Walk 두 상태 모두에서 사격 가능하도록 유지 — 사격 피드백은 다른 방식(총구화염/킥백 등, 6번 섹션 전투 피드백 설계)으로 커버할 예정.
16. 이후: 여러 미션이 이어지는 구조에서 실패 시 페널티(경고 누적/이월 등 GDD 원안) 재도입 여부 검토, 지원 동기/유산 기록 시스템, 파괴 가능 장애물, 적 종류 다양화.
17. **좀비 종류 추가 시 같이 처리 (설계 방향 확정, 구현은 실제 종류 늘어날 때)**:
    - `CurrencyManager.crystalPerKill`(고정값)을 `EnemyHealth`(또는 향후 좀비 데이터)로 이동하고, `AddKillReward()`에 `int amount` 매개변수를 추가해 좀비 종류별로 다른 크리스탈 보상을 줄 수 있게 변경.
    - **웨이브 내 좀비 등장 순서 제어**: 지금 `WaveSpawner`는 "총 마릿수"만 세는 카운터 방식(`totalToSpawn`/`spawnedCount`)이고 `EnemyPool`도 프리팹 하나만 관리하는 싱글톤이라, 여러 종류를 정해진 순서로 등장시킬 수 없음. 확장 방향:
      1. `WaveDefinition`에 `EnemySpawnEntry[] spawnSequence`(각 항목 = `{enemyPrefab, count}`) 배열 추가 — 배열 순서 자체가 등장 순서(예: [0]=정찰 5마리, [1]=중형 3마리, [2]=정예 1마리).
      2. `WaveSpawner`의 카운터 로직을 "총 마릿수" 방식에서 "시퀀스 인덱스"(`sequenceIndex`, `countInCurrentEntry`) 방식으로 변경 — 현재 시퀀스 항목의 count를 다 채우면 다음 인덱스로 진행.
      3. `EnemyPool`을 프리팹 하나만 관리하는 싱글톤에서, **프리팹별로 풀을 나눠 관리하는 매니저**(`Dictionary<GameObject, ObjectPool<EnemyMovement>>` 등)로 확장 — 가장 큰 변경 지점.
    - 지금은 좀비가 HeavyZombie 하나뿐이라 전부 보류 중, 새 좀비 종류(3D 모델 포함) 추가하는 시점에 위 설계대로 함께 구현 예정.
16. **여러 미션 동시 진행(로그라이트 랜덤 미션) 도입 시 `RunState.cs`에서 같이 처리할 것**:
    - `CurrentMission`(단일 `Mission`)을 `Mission[]`/`List<Mission>`으로 변경, 발급·제출·마감판정 로직(`IssueNewMission`, `TrySubmitMission`, `AdvanceDay`의 마감 체크)을 별도 `MissionManager` 클래스로 분리 검토.
    - **잠재 버그(지금은 발현 안 함, 반드시 같이 고칠 것)**: `RunState.AdvanceDay()`에서 미션 마감 실패 시(`OnMissionResolved?.Invoke(false)`) `CurrentMission`을 `null`로 비우지 않음. 지금은 `DueDay = 발급일 + missionInterval - 1` 공식 덕분에 "마감 실패 판정 나는 날 = 항상 다음 발급일"이 우연히 성립해서 `IssueNewMission()`이 실패한 미션을 바로 덮어써 문제가 안 보임. 랜덤 미션 도입으로 발급 간격/마감일이 불규칙해지면 이 전제가 깨져 실패한 미션이 `CurrentMission`에 계속 남을 수 있음 — 실패 판정 직후 명시적으로 `CurrentMission = null` 처리 추가 필요.

## 9. 다음 구현 순서 및 무기/좀비 상성 설계 (2026-09-07 논의, 기록만 — 구현 시 세부사항 다시 논의)

**구현 순서 확정**: 1) 침대 구현(하면서 슬롯 소비 지점 결정) → 2) 상점 구현(하면서 방어구조물(벽) + 무기 트리 중 권총 계열만 우선) → 3) 좀비 한 종류 추가(하면서 좀비의 구조물 공격 구현) → 4) 웨이브 추가. 세부사항(침대 상호작용 흐름 등)은 각 구현 시점에 다시 논의하기로 함.

### 1) 침대 구현 — 완료 (2026-09-07)

**슬롯 소비 원칙 확정**: 이 게임은 슬롯 소비가 계획적으로 이루어져야 하는 게임이라, **모든 슬롯 소비 지점은 반드시 확인 UI를 거쳐야 함**(원클릭 즉시 소비 금지) — 이 원칙은 앞으로 만들 상점/다른 슬롯 소비 지점에도 동일하게 적용.

**공용 `ConfirmationDialog` 신설**: 지금까지 이런 예/아니오 확인 팝업이 프로젝트에 없었어서 침대를 계기로 처음 만듦. `PersistentManagers` 프리팹 하위에 `ConfirmationCanvas`(Canvas + `ConfirmationDialog` 컴포넌트) → `ConfirmationPanel`(`LocksInputWhileActive` 부착, 열려있는 동안 자동 입력 잠금) → `MessageText`/`YesButton`/`NoButton` 구조로 배치. `Show(string message, Action onConfirmAction)` 하나만 호출하면 메시지와 확인 시 실행할 콜백을 넘길 수 있어, 앞으로 상점 구매 확인·ExitPoint 확인 등에도 그대로 재사용 가능한 공용 컴포넌트로 설계.

**체력 회복 기준 확정**: 침대(슬롯 1개 소비) = 완전 회복(풀피)을 기준점으로 잡음. 나중에 상점에서 크리스탈로 사는 회복 아이템(물약 등)은 이 기준보다 저렴/불완전한 회복으로 설계될 예정 — 침대가 "가장 확실하지만 슬롯(시간)을 쓰는 회복", 물약이 "즉시지만 크리스탈을 쓰는 회복"으로 위계를 나눔.
- `PlayerManager.cs`에 `FillHealth()` 신설 — 기존 `ResetHealth()`(새 런 시작 전용, 런 생애주기 이벤트)와 개념이 다름을 인지하고 분리. `FillHealth()`는 게임플레이 중 체력을 채우는 일반 행동이라 저수준 API `SetCurrentHealth(maxHealth)`를 통해 처리(직접 대입 아님) — 나중에 "최대 체력 초과 회복 규칙"이 바뀌어도 한 곳만 고치면 되도록.
- **이미 풀피 상태에서 침대 이용 가능 여부**: 막지 않기로 결정. 침대의 본질은 "체력 회복"이 아니라 "슬롯을 소비하며 안전하게 시간을 넘기는 행동"이고 회복은 부수 효과일 뿐 — 이미 풀피여도 "이번 슬롯은 안전하게 쉬고 싶다"는 선택은 여전히 유효하므로 상호작용 자체를 막을 이유가 없음.
- `BedInteraction.cs`(신설): `InteractableStructure.onInteract`에 연결되는 `OnInteract()` → `ConfirmationDialog.Show("수면하시겠습니까?", Sleep)` → 확인 시 `PlayerManager.FillHealth()` + `RunState.AdvanceSlot()`.

**중요 발견 — 소환기/미션보드/침대 콜라이더가 전부 `Is Trigger = false`(Solid)인데도 상호작용 정상 작동**: `PlayerInteractor`가 트리거 이벤트로 감지하는데, 상호작용 구조물 쪽 콜라이더는 트리거가 아니어도 문제없음 — 플레이어의 `SphereCollider`(트리거) + `Rigidbody` 조합이 이미 트리거 이벤트 발생 조건을 충족시키므로, 상대방 콜라이더는 Solid여도 무방(이전에 Summoner 리팩토링 때 확인했던 "트리거는 둘 중 하나만 있어도 발생" 원리가 여기서도 그대로 적용됨). 즉 **상호작용 구조물의 콜라이더는 굳이 트리거로 바꿀 필요 없음** — 오히려 Solid로 두면 플레이어를 물리적으로도 막아주는 효과를 겸함.

**주의사항 — 새 상호작용 구조물 만들 때마다 챙길 것**: 상호작용 구조물은 **Layer를 `Interactable`로 설정**해야 `PlayerInteractor`가 감지함(`interactableLayer` 마스크 체크). 지금까지는 만들 때마다 개별적으로 설정해왔는데, 앞으로 새 구조물(상점, 방어구조물 등) 만들 때 이 설정을 빠뜨리지 않도록 매번 같이 확인할 것.

**남은 것(다음 단계에서 처리)**:
1. `Bed` 오브젝트에 아직 실제 3D 모델(fbx)이 안 들어가 있음(지금은 기본 Cube 형태) — **다음 단계(상점 구현) 시작하자마자 가장 먼저 Bed에 fbx 모델 배치할 것**.
2. 침대 확인 → 수면 처리 시 화면 페이드 인/아웃 연출 추가 예정(같은 씬 내 암전, `SceneFader`와는 별개의 새 컴포넌트 필요 — 씬 전환이 없는 페이드).

### ⚠️ 상점 구현 시작 전 먼저 처리할 5가지 — 전부 완료(2026-09-08)

**다른 PC에서 이어서 작업 시: 아래 5개는 모두 완료됨. 다음은 "2) 상점 구현"(권총 계열 + 방어구조물(벽) 판매) 단계로 바로 진행하면 됨.**

1. ~~`Bed`에 아직 3D 모델(fbx)이 없음~~ — **완료(2026-09-08)**: `Bed.fbx`를 자식으로 배치, 부모(콜라이더/InteractableStructure/BedInteraction 보유)의 MeshFilter/MeshRenderer는 제거. 소환기/미션보드와 동일한 패턴.
2. ~~침대 확인 → 수면 처리 시 화면 페이드 인/아웃 연출 없음~~ — **완료(2026-09-08), 겸사겸사 SceneFader/ScreenFader 아키텍처 통합**:
   - 처음엔 `SceneFader`(씬 전환용)와 별개로 `ScreenFader`(같은 씬 내 암전용)를 새로 만들려 했으나, 두 클래스의 핵심 로직(페이드 계산)이 완전히 중복되는 걸 발견하고 설계를 재검토.
   - **최종 구조**: `ScreenFader` 하나가 페이드 연출 + `PlayerInputLock` 잠금/해제 + "그 사이 원하는 동작 실행"까지 전부 캡슐화하는 단일 공용 유틸리티로 확정 — `public void FadeAction(Action action)` 메서드 하나로, 호출부는 "무엇을 할지"만 델리게이트로 넘기면 됨(`yield return Fade(0,1) → action?.Invoke() → yield return Fade(1,0)`, 그 앞뒤로 `PlayerInputLock.Lock()/Unlock()`).
   - **`SceneFader.cs`는 완전히 삭제**: 씬 전환 로직(`SceneManager.LoadScene`)까지도 그냥 `ScreenFader.Instance.FadeAction(() => SceneManager.LoadScene(sceneName))`처럼 액션으로 넘기면 충분해서, 별도 클래스로 존재할 이유가 없었음. `BaseExitPoint.cs`/`CombatExitPoint.cs`도 이 방식으로 수정, `BedInteraction.cs`도 동일하게 `ScreenFader.FadeAction()` 사용.
   - **이 구조의 핵심 이점**: `PlayerInputLock.Lock()/Unlock()`을 호출하는 코드가 프로젝트 전체에서 정확히 `ScreenFader.FadeActionRoutine` 한 곳(두 줄)에만 존재하게 됨 — 지난 세션에 겪었던 "여러 곳에서 같은 잠금 스위치를 건드리다 순서가 꼬여 영구 잠김" 버그 유형이 구조적으로 재발할 수 없음. 호출부(ExitPoint, BedInteraction)는 각자 고유 로직(씬 전환, 회복+슬롯소비)만 신경 쓰면 됨.
   - **겪은 문제 — Missing Script로 프리팹 저장 불가**: `SceneFader.cs` 파일을 삭제하자, 그 컴포넌트가 붙어있던 `PersistentManagers` 프리팹의 `FadeCanvas` 오브젝트에 "Missing Script"(빈 컴포넌트 껍데기)가 남아 `Error while saving Prefab... missing script`로 저장 자체가 막힘. Unity는 스크립트 파일이 사라져도 인스펙터에 저장된 값을 보존하려고 컴포넌트 껍데기를 자동 삭제하지 않고 남겨두기 때문 — **해결**: Missing Script 컴포넌트 영역 우클릭 → Remove Component(⋮ 메뉴가 없는 경우 컴포넌트 본문 우클릭으로 대체 가능)로 제거 후, `ScreenFader` 컴포넌트를 새로 추가하고 `Fade Image` 필드 연결. **교훈**: 프리팹/씬에 이미 부착된 컴포넌트의 스크립트 파일을 삭제/개명할 때는, 반드시 그 컴포넌트가 붙어있던 모든 오브젝트에서 Missing Script를 수동 정리해야 함.
   - **부수 작업 — `ConfirmationDialog` 팝업 크기 수정**: 처음에 `ConfirmationPanel`의 RectTransform이 anchorMin(0,0)~anchorMax(1,1)(화면 전체를 채우는 설정)로 되어 있어 확인창이 화면 전체를 뒤덮던 문제 발견. **"게임 월드가 계속 흘러가는 상황(좀비도 계속 움직임)에서 뜨는 확인창이라, 설정창처럼 화면 전체를 딤(배경을 어둡게 죽이는 것) 처리하면 안 된다"**는 원칙을 정리 — 딤 처리는 게임이 완전히 멈추는 메뉴류(일시정지 등)에만 어울리고, 지금처럼 캐릭터 입력만 잠그고 월드는 계속 흘러가는 상황에는 안 어울림. `ConfirmationPanel`의 anchor를 Middle-Center(0.5,0.5 고정점)로 바꾸고 `sizeDelta`를 고정 크기(600×360)로 지정해 화면 중앙의 작은 팝업창으로 수정, 배경 이미지 알파도 0.392(딤용으로 흐릿했던 값)에서 0.902(팝업 배경으로 적절)로 조정.
3. ~~걷는 도중 상호작용하면 Walk 애니메이션이 멈추지 않고 계속 재생됨~~ — **완료(2026-09-08)**: 원인은 `PlayerMovement.Update()`가 `PlayerInputLock.IsLocked` 가드로 통째로 멈추면서, 그 직전 프레임에 세팅된 애니메이터 값이 그대로 남는 것. `IsMoving`(bool) 파라미터를 `MoveSpeed`(float)로 교체 — 잠금 시 `SetFloat("MoveSpeed", 0f)`를 명시적으로 호출한 뒤 return, 평소엔 `input.magnitude`(0 또는 1)를 그대로 넘김. Animator Controller의 Idle↔Walk 전환 조건도 `IsMoving == true/false`에서 `MoveSpeed > 0.1` / `MoveSpeed < 0.1`로 변경. float 파라미터로 바꿔둬서 나중에 걷기/뛰기 등 속도 단계가 추가돼도 코드 수정 없이 확장 가능.
4. ~~좀비가 플레이어와 완전히 겹치면 제자리에서 빙글빙글 회전~~ — **완료(2026-09-08)**: 원인은 `EnemyMovement.Update()`가 "목표까지의 거리"를 전혀 고려하지 않고, 방향이 0에 아주 가깝지만 않으면 항상 최고 속도로 전진하던 것 — 콜라이더가 겹쳐서 물리적으로 밀려나는 것과 계속 전진하려는 힘이 충돌해 빙글빙글 도는 것처럼 보임. `stoppingDistance`(기본값 0.4) 필드를 추가해 `distance > stoppingDistance`일 때만 전진하도록 변경 — 이 거리 안으로 들어오면 회전(플레이어 바라보기)은 계속하되 더 이상 다가가지 않음, `EnemyAttack`의 근접 공격 판정 범위와 자연스럽게 맞물리는 "다가와서 멈추고 공격" 동작 완성. 좀비 여러 마리가 한 지점에 몰릴 때 서로 밀려 겹치는 경우까지 완전히 막아주진 않을 수 있음(별도의 물리 충돌 해석 이슈) — 필요시 추가 확인.
5. ~~Base 씬 카메라가 벽을 뚫고 나감~~ — **완료(2026-09-08)**: `CameraFollow.cs`의 `LateUpdate()`에서 `desirePosition`(추적+줌 반영된 목표 위치) 계산 직후, x/z를 `Mathf.Clamp`로 제한하는 방식 채택. `useBounds`(bool) + `minBounds`/`maxBounds`(`Vector2`, x=월드 x, y=월드 z) 필드 추가 — 씬 이름으로 분기하지 않고 인스펙터 값 차이로 처리(기존 `initialZoom` 패턴과 동일한 방향). Base 씬은 `useBounds` 켜고 실제 방 크기에 맞는 경계값 입력, ZWD처럼 트인 맵은 꺼두면 됨.
   - **Cinemachine Confiner 대안 검토**: Unity 2D의 `CompositeCollider2D`+`CinemachineConfiner2D`(3D는 `CinemachineConfiner3D`)가 정확히 이 문제(카메라를 경계 안에 가두기)를 위한 공식 기능이지만, Cinemachine 패키지 설치 + 기존 커스텀 `CameraFollow.cs` 체계를 통째로 교체해야 하는 비용 대비, 지금처럼 사각형 벽으로 막힌 단순한 방에는 오버엔지니어링이라 판단해 기각. 나중에 맵이 비정형/복잡해지거나 카메라 연출(블렌딩 등)이 정교해져야 할 때 재고려 가능.

**⚠️ 상점 구현 시작 전 처리할 5가지 전부 완료 — 다음은 "2) 상점 구현" 단계로 진행.**

### 좀비 특색 설계 (가위바위보 상성)
플레이어가 스스로 체감하는 암묵적 상성으로 시작하기로 확정 — 명시적 속성/저항 시스템은 지금 만들지 않음(좀비 3종·무기 계열 1개뿐인 지금 단계에 과함).
- **헤비좀비**(구현됨): 고체력·저속, 구조물에 고데미지 → 단일 대상 고데미지 무기(저격/RPG류)로 상대하는 게 유리하도록 설계 의도.
- **일반좀비**(2번째로 만들 좀비): 모든 면에서 평범 — 범용 무기(권총/소총류)로 무난하게 상대.
- **러너좀비**(3번째로 만들 좀비): 작고 빠르지만 저체력 → 슬로우 또는 범위 공격으로 상대하는 게 유리하도록 설계 의도.
- 나중에(시점 미정) 날씨/속성 시스템을 좀비와 무기 양쪽에 도입해 상성을 명시적으로 만들 가능성은 열어둠 — 지금은 아님.

### 무기 트리 설계
- **계열(무기 종류) × 등급(강화 단계)의 2차원 구조**로 이해 정리. 원래 계획(권총/소총/저격/RPG 등 계열마다 다른 모델 제작)은 제작 시간이 너무 많이 들어 축소 — **계열당 모델 하나만 만들고, 등급은 같은 모델에 색 오라 + 스탯 배율만 다르게 적용하는 강화판 방식으로 확정**. 디아블로류의 "색으로 희귀도 표현" 관습과 같은 방향이라 플레이어에게도 익숙함. 나중에 특정 등급 하나만 골라 별도 모델로 승격시키는 것도 가능한 유연한 구조.
- 지금은 계열 1개(권총)만 우선 구현.

### 설계도(무기 계열 해금) 시스템 설계 — 계열이 2개 이상이 되면 본격 도입, 지금은 기록만
- **해금형으로 확정**: 설계도를 얻어야만 그 무기 계열이 상점에 뜸(안 얻으면 상점에 아예 안 보임). "설계도 있으면 할인, 없어도 구매 가능" 안은 기각 — 계열이 하나뿐인 지금 단계에서 체감이 안 되고, "이번 런은 이 무기로 가야 한다"는 방향성도 해금형이 더 강하게 줌. 밤 웨이브(GDD상 이미 위험-보상 구조)에서 설계도 획득 기회를 주는 것과도 자연스럽게 결합.
- **영구 해금(메타 진행) 개념 도입**: 설계도 획득은 특정 런에서만 유효한 게 아니라 **런을 넘나드는 영구 기록**. `SaveData`(런 진행 상태, 런 종료 시 삭제됨)와는 완전히 별도로 "메타 진행 데이터"를 저장해야 함 — `SaveManager.DeleteSave()`가 지금은 파일 하나를 통째로 지우는데, 앞으로는 이 메타 데이터는 보존하고 런 진행 데이터만 지우도록 구조 변경 필요.
- **런 시작 시 지급**: 권총(기본 해금, 항상 보유)에 더해, 그동안 영구 해금해온 설계도 중 하나를 무작위로 지급받아 이번 런의 시작 방향을 잡음.
- **런 진행 중 추가 획득(드랍 테이블)**: 이미 해금된 설계도(확률 높음) + 아직 한 번도 못 본 미해금 설계도(확률 낮음) 중에서 뽑힘. 발견 즉시 그 자리에서 영구 해금되고, 동시에 이번 런에서도 바로 사용 가능. 미해금 설계도를 특정 웨이브/기믹으로 별도 획득하는 방식은 더 복잡해서 지금은 보류, 확률 기반 드랍만 우선 적용.
- 첫 런(해금된 설계도가 하나도 없을 때)은 자연히 권총만 갖고 시작 — 튜토리얼성으로 자연스러움.

### 유물 시스템 — 구현 시점 미정, 상점과 완전히 별개
- 체력/속도/연사력/사거리/데미지 증가 등 패시브 능력치 보너스. 상점(장비)과는 다른 카테고리라 별도 시스템으로 취급.
- 목적: 유물에 따른 무기 선택과 웨이브 선택(미래의 날씨/속성 시스템과도 연결됨)의 다양성을 주기 위함. 지금은 구현 안 함.

## 8. 코드 리팩토링 진행 상황 (폴더별로 하나씩 검토 중)

방식: 폴더 순서(Core → Player → Enemy → Combat → Interaction → Wave → UI → Camera)로, 스크립트 하나씩 6가지 관점(①한 줄씩 설명 ②주석 필요 여부 ③더 효율적인 방법 ④확장 계획 ⑤멤버/메서드 순서 ⑥`[SerializeField]`인데 `[Header]` 없는 필드 분류)으로 검토. 코드 수정은 사용자가 직접 진행.

**진행률 (2026-09-07 기준): 전체 완료.** Core·Player·Enemy·Combat·Interaction·Camera·Wave·UI 폴더 전부 검토 완료. `DayCycleUI.cs`는 검토 중 완전한 미사용 파일(GameUI에 이미 흡수됨, 어떤 씬/프리팹에도 안 붙어있음)로 확인되어 삭제함 — 최종 39개 스크립트.

### Core 폴더 — 완료
- `GlobalFlags.cs` — `PendingLoad`/`SceneTransitionRequest`를 `PlayerInputLock`과 같은 `{ get; private set; }` + 전용 메서드 패턴으로 통일(캡슐화), 각 클래스 역할 설명 주석 추가.
- `GameBootstrap.cs` — 구조 이미 견고, 주석만 보강 권장(수정 안 함).
- `CurrencyManager.cs` — `SetCrystal` → `LoadCrystal`로 이름 변경(세이브 로드 전용임을 명확히). `crystalPerKill`/`AddKillReward()`는 좀비 종류 추가 시 `EnemyHealth`로 이동 + 매개변수화 예정(7번 목록 10번 항목).
- `GameManager.cs` — 구조 견고, 수정 없음(주석 보강만 권장).
- `Mission.cs` — 순수 데이터 클래스. `IsOverdue()`의 `>` 부등호(마감일 당일까지 제출 가능) 의도 확인. 여러 미션 확장 시 `MissionManager` 분리 계획 확정(7번 목록 11번 항목).
- `PlayerManager.cs` — `TakeDamage`/`Heal`을 `PlayerManager`에서 `PlayerHealth`(Player 폴더)로 이동, `PlayerManager`는 순수 데이터 계층(`CurrentHealth`, `MaxHealth`, `LoadHealth`, `ResetHealth`, `SetCurrentHealth`)으로 축소 — 상세는 아래 Player 폴더 항목 참고.
- `RunState.cs` — 메서드 순서 정리(public/private 그룹화), `TrySubmitMission()`의 크리스탈 체크 중복 제거(`TrySpend` 반환값만 사용), `LoadFrom()`을 자기 필드만 복원하도록 축소(아래 SaveManager 항목과 연계), `[Header]` 2그룹 분리. `OnMissionIssued`/`OnMissionResolved`는 현재 구독자 없지만 미션 시각 피드백(느낌표/물음표) 구현 예정이라 의도적으로 유지 — 삭제 아님.
- `SaveData.cs` — `HasSaveData` 필드는 완전히 죽은 코드(삭제 가능, 미실행). `rareCrystal`은 살아있지만 아직 저장/로드 배관 없음(희귀 크리스탈 시스템 도입 시 연결 예정).
- `SaveManager.cs` — **Save/Load 비대칭 구조 리팩토링**: `Save()`는 이미 여러 매니저→SaveData 조립하는 조율자였는데 `Load()`는 파일→객체 변환만 하던 것을, `TryLoadAndApply()` 신설로 대칭 맞춤(SaveData→여러 매니저 분배까지 SaveManager가 전담). `RunState.OnSceneLoaded()`가 이제 `SaveManager.Instance.TryLoadAndApply()`만 호출하도록 단순화됨.
- `SceneFader.cs` — 구조 견고, 동기 로드 이유/alpha 보정 이유 주석만 권장(수정 안 함).
- `ScenePlayerSpawner.cs` — 구조 견고, `[Header]` 2그룹(생성/연동) 제안(수정 안 함).
- `SceneTransitioner.cs` — 완결된 2줄짜리 클래스, 수정 없음.
- `UIManager.cs` — **`gameUI` 필드 삭제**(등록만 되고 실제로 안 쓰이던 죽은 배선 확인 후 제거). `GameUI.Start()`의 `RegisterGameUI` 호출도 함께 제거. `GameUI`가 코드로 활성/비활성화되는 게 아니라 `HUD.prefab` 자체가 Base/ZWD 씬에만 배치되어 있어 MainMenu에는 안 보인다는 것 확인.

### Player 폴더 — 완료
- `PlayerMovement.cs` — 구조 견고(이전 세션에서 null 체크 버그 이미 수정).
- `PlayerAim.cs` — `FirePoint` 프로퍼티 추가(외부 노출), `Plane` 기반 조준 계산 이유 주석 추가.
- `PlayerHealth.cs` — **PlayerManager와 책임 재분리**: `TakeDamage`/`Heal` 로직을 `PlayerManager`에서 이 클래스로 이동. `PlayerManager`는 순수 데이터 저장소, `PlayerHealth`는 게임 로직(사망 판정, GameManager 연동) 전담. 자체 체력 필드는 두지 않고 항상 `PlayerManager.Instance.CurrentHealth`를 통해 읽고 씀(값이 두 곳에 따로 존재하며 어긋나는 버그 방지). `PlayerManager`에 저수준 API `SetCurrentHealth(int value)` 추가.
- `PlayerInteractor.cs` — 구조 견고, `OnTriggerEnter`/`OnTriggerExit` 중복 코드 헬퍼 추출은 선택 사항(수정 안 함).
- `PlayerShoot.cs` — **PlayerAim과의 중복 제거**: `firePoint` 필드 삭제하고 `playerAim.FirePoint` 재사용, 자체 방향 계산 없이 `firePoint.rotation` 그대로 사용. 참고: `PlayerAim.Update()`와 `PlayerShoot.Update()`의 스크립트 실행 순서가 보장되지 않아 이론상 한 프레임 오차 가능성 있음(지금은 체감 안 됨, 필요시 Script Execution Order로 명시 가능).

### Enemy 폴더 — 완료
- `EnemyHealth.cs` — 구조 견고. `CurrencyManager.AddKillReward()` 관련해 `crystalPerKill`을 여기로 옮기고 매개변수화하는 계획 재확인(좀비 종류 추가 시 진행, 7번 목록 10번 항목).
- `EnemyMovement.cs` — 구조 견고, 지수 감쇠 회전/멱등 가드 등 기존 설계 그대로 유지.
- `EnemyAttack.cs` — 구조 견고. `attackTimer = attackInterval`이 if 블록 밖에 있는 이유(playerHealth가 null인 예외 상황에서도 매 프레임 GetComponent 반복 호출되는 것을 막는 안전장치) 확인 — 지금 구조가 맞음, 수정 안 함.
- `EnemyPool.cs` — Unity 표준 ObjectPool<T> 정확한 활용, 수정 없음.

### Combat 폴더 — 완료
- `Projectile.cs` — 구조 견고, `isReleased` 가드가 실제 경쟁 상황(시간 만료+충돌 동시 발생)을 막는 것 확인.
- `ProjectilePool.cs` — **`collectionCheck: false` 옵션 제거**(EnemyPool과 통일). Projectile의 `isReleased` 가드가 이미 중복 Release를 원천 차단하고 있어 이 옵션이 불필요했음— 끄면 오히려 향후 실수 시 에디터 경고를 못 받는 안전망 손실이었음.

### Interaction 폴더 — 완료
- `InteractableStructure.cs` — **`Debug.Log("Interact")` 삭제**(디버그용 임시 로그였음). `onInteract`(UnityEvent) 설계 이유 주석 추가.
- `LocksInputWhileAcitve.cs` → **`LocksInputWhileActive.cs`로 파일명/클래스명 오타 수정** (SessionLog에 기존 기록된 오타). 여러 UI가 동시에 겹쳐 열리는 경우 `PlayerInputLock`이 단순 bool이라 먼저 닫힌 패널이 늦게 열린 패널의 잠금까지 풀어버릴 수 있는 잠재 이슈 확인(카운터 방식 전환 후보) — 지금은 UI가 동시에 안 겹쳐서 미발현, 필요시 재검토.
- `MissionBoardUI.cs` — **이벤트 구독 제거**: `RunState.OnMissionIssued` 구독(`Start`/`OnDisable`)이 `Open()`의 강제 갱신과 완전히 중복되는 죽은 코드였음을 확인 후 삭제. `RunState.OnMissionIssued`/`OnMissionResolved` 이벤트 자체는 삭제하지 않음 — 미션 시각 피드백(느낌표/물음표) 구현 계획이 확정되어 그때 다시 쓰일 예정(7번 목록 4번 항목에 상세).
- `BaseExitPoint.cs` — `[RequireComponent(typeof(InteractableStructure))]` 추가(CombatExitPoint와 일관성 맞춤), 안 쓰이는 `using UnityEngine.SceneManagement;` 제거.
- `CombatExitPoint.cs` — 구조 견고, BaseExitPoint와 합치지 않기로 결정(각자 다른 로직을 가진 별개 행동이라 분리 유지가 맞음). 확인 UI(나가기 전 확인 팝업 + 웨이브 미완료 시 안내)는 공용 컴포넌트(예: `ConfirmationDialog`) 하나를 만들어 양쪽이 호출만 하는 방식으로 구현 예정 — 중복 구현 불필요.

### Camera 폴더 — 완료
- `CameraFollow.cs` — `[Header]` 2그룹(추적/줌) 분리. **줌 계산에서 `Time.deltaTime` 제거**: 마우스 스크롤 델타값이 이산적 입력(`scroll` ±1)이라 `Time.deltaTime`을 곱하면 프레임레이트에 따라 감도가 달라지는 문제가 있어, 고정 배율(`* 0.01f`)로 변경. `zoomSpeed`가 이제 정수 단위(1~25 등)로 직관적 조정 가능.

### Wave 폴더 — 완료
- `CombatSceneEntry.cs` — 구조 견고. `SelectedWave == null`(웨이브 미선택 상태) 분기가 GDD 미정 사항("웨이브 미선택 시 시간 흐름 여부")과 연관된 지점이라는 주석 추가. `ScenePlayerSpawner`와 `Start()` 실행 순서가 보장 안 되는 잠재 이슈는 `WaveSpawner`쪽 가드로 해결(아래 참고).
- `TimeOfDayModifier.cs` — `[Header]` 2그룹(식별/배율) 분리, `GetStrengthTier()` 임계값(0.9/1.1)이 확정값 아님을 주석으로 명시, `[CreateAssetMenu]` 메뉴명 오탈자(`Time Of DayModifier` → `Time Of Day Modifier`) 수정.
- `WaveDefinition.cs` — `[Header]` 3그룹(식별/스폰 구성/보상) 분리, `SpawnDirectionPattern.Eight`가 아직 로직만 있고 실제 스폰포인트 미생성 상태임을 주석으로 명시.
- `WaveSpawner.cs` — 안 쓰이는 `using System.Threading;` 제거. **실행 순서 방어 추가**: `player`가 아직 주입되기 전(첫 프레임 스폰 타이밍과 `ScenePlayerSpawner.Start()`가 경쟁할 수 있는 이론적 가능성)에도 안전하도록 `Update()`에 `if (player == null) return;` 가드 추가 — 지금까지 증상은 없었으나 실행 순서에 의존하지 않는 예방적 조치로 확정.

### UI 폴더 — 완료
- `GameOverUI.cs` — **로그라이트 사망 처리 재설계 확정, 구현은 리팩토링 완료 후 1순위**. 기존 `Restart()`(필수 11단계 시절 로직, 씬만 재로드하고 영속 매니저는 리셋 안 함)를 발견 후 폐기 결정. 새 설계: 사망 시 생존 일수+최종 크리스탈 표시, "메인메뉴로" 버튼 하나만, 클릭 시 런 전체 리셋(`RunState`/`CurrencyManager`/`PlayerManager`) + `SaveManager.DeleteSave()` + `SceneTransitioner.GoTo("MainMenu")`(Loading 경유) + 그 시점에 `Time.timeScale = 1f` 복구. 상세는 7번 목록 1번 항목.
- `GameUI.cs` — 이미 죽은 코드(UIManager 등록) 제거된 상태라 추가 수정 없음. 매 프레임 폴링 방식(이벤트 대신) 유지가 지금 규모에 적합함을 확인.
- `LoadingScreen.cs` — 구조 견고, `operation.progress`가 `allowSceneActivation=false`일 때 0.9(90%)에서 멈추는 Unity 사양 때문에 `/0.9f` 재정규화가 필요하다는 것과 `Mathf.Min(loadProgress, timeProgress)`로 최소 표시 시간을 보장하는 이유를 주석으로 남기는 것 권장(수정 안 함).
- `MainMenuUI.cs` — 구조 견고. `GameOverUI`가 필요로 할 리셋 로직(`RunState.ResetForNewRun`+`CurrencyManager.ResetCrystal`+`PlayerManager.ResetHealth`)이 `OnNewGameClicked()`와 완전히 중복되는 걸 확인 — `RunState.cs`에 `ResetEntireRun()` 조율 메서드를 신설해 양쪽이 재사용하도록 구현 예정(7번 목록 1번 항목과 함께 진행).
- `WaveClearUI.cs` — 지금은 클리어 시 빈 패널만 띄우는 최소 골격. **실제 목표(설계 확정, 구현은 나중)**: CLEAR! 이펙트 → 결과창(이번 웨이브 획득 크리스탈 일반/희귀, 설계도 등) → 확인 버튼 클릭 시 닫힘 + 그제서야 나갈 수 있는 상태로 전환. `WaveSpawner.CanExit`에 `ResultsConfirmed` 조건 추가 필요, "이번 웨이브 획득량"은 `BeginWave()` 시점 크리스탈 스냅샷과의 차이로 계산 필요. 상세는 7번 목록 6번 항목.
- `WaveSelectionUI.cs` — `[Header]` 5그룹(패널/데이터/웨이브 버튼 목록/웨이브 정보 표시/시간대 아이콘) 분리. **SessionLog 미해결 항목 재확인 결과**: `valueEnemyCount`에 `SpawnCountMultiplier` 반영은 이미 완료 상태였음(코드 확인됨). 시간대 아이콘은 코드 로직(`UpdateTimeOfDayIcon()`)은 완성되어 있으나 실제 아이콘 스프라이트가 아직 단색 임시 이미지 상태 — 코드가 아니라 에셋 제작이 남은 작업. `foreach` 클로저로 버튼마다 웨이브를 캡처하는 부분은 C# 5.0+ 사양상 안전(반복마다 변수 새로 생성)하다는 것 확인.
- `DayCycleUI.cs` — **완전한 미사용 파일로 확인 후 삭제**. `GameUI.cs`에 기능이 이미 흡수되어 있었고(dayText/slotText/GetSlotLabel 동일), 코드 어디서도 참조 안 되고 어떤 씬/프리팹에도 컴포넌트로 붙어있지 않음을 검증 후 제거.

---

## 7. 사용자 작업 스타일 (다음 세션에서도 유지할 것)

- **코드는 전부 사용자가 직접 작성/타이핑함.** Claude는 코드 스니펫을 제시하고 설명하되, Edit/Write 도구로 스크립트 파일을 직접 고치지 않음(초기엔 몇 번 직접 고쳤다가 사용자가 명시적으로 "고치지 말고 설명만 하라"고 정정함).
- Unity MCP를 통한 씬/에셋 조작(GameObject 생성, 컴포넌트 부착, 프리팹화, 머티리얼 생성, 텍스처 생성 등)은 허용됨 — 이건 "코드"가 아니라 에디터 설정이라는 구분.
- 질문이 오면 먼저 정확한 원인 진단(왜 이런 증상이 나는지)을 설명한 뒤 수정 방향을 코드로 제시하는 순서를 선호함.
- 오류/버그가 나오면 코드를 실제로 Read해서 정확한 원인을 짚어주길 원함(추측성 답변 지양).
- **커밋 타이밍**: 기능 구현 중 Claude가 판단하기에 적당한 지점(예: 기능 하나 완료)에 도달하면, 사용자에게 GitHub(Desktop)에 커밋하라고 먼저 알려줄 것. Claude가 직접 git commit을 실행하지 않고(코드는 사용자가 직접 관리), 타이밍만 알려주는 역할.
- 설계 관련 질문에서는 "왜 이 방식이 좋은지"와 "실제로 언제 이 방식이 손해인지"를 균형 있게 설명하는 걸 선호 — 무조건 정석/베스트 프랙티스를 밀어붙이기보다, 지금 규모에 맞는 실용적 판단을 요구함 (예: 상속 vs 직접 작성 논쟁에서 사용자가 오버엔지니어링을 지적하면 그 지적이 맞으면 인정하고 더 간단한 대안 제시).
- 메모리 파일(`C:\Users\KGA1\.claude\projects\C--Unity-ZombieWaveDefence\memory\project_zombie_wave_defence.md`)에 프로젝트 결정사항이 계속 누적되고 있음 — 새 세션에서도 이 메모리가 로드되므로 이 SessionLog와 함께 참고하면 됨.
