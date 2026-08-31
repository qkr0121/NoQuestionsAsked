# SessionLog — No Questions Asked (ZombieWaveDefence)

마지막 업데이트: 2026-08-31

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
- **미해결 항목**: `valueEnemyCount` 표시값에 `TimeOfDayModifier.SpawnCountMultiplier`를 반영하는 수정(`Mathf.Max(1, Mathf.RoundToInt(wave.TotalEnemyCount * multiplier))`, `WaveSpawner.BeginWave()`와 동일 계산식 사용)을 안내했으나 **사용자가 아직 반영 안 했다고 명시적으로 말함** — 다음 세션에서 확인/적용 필요.
- 시간대 아이콘(`TimeOfDayIcon` Image + Morning/Day/Night 스프라이트 3개)도 설계는 했으나 실제 반영 여부 미확인.

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

남은 목록 (다음 목표: **3D 모델 적용 → 웨이브 추가 → 프로토타입 v1**):

1. **3D 모델 적용** — VARCO 3D AI로 카툰풍 인간형 캐릭터/좀비 제작 및 적용 (다음 작업).
2. **다른 웨이브 추가** — 지금은 `WD_Wave01` 하나뿐, 프로토타입 v1을 위해 몇 개 더 필요.
3. 슬롯 소비 지점 확장 — 지금은 전투 복귀시(`CombatExitPoint`)에만 `AdvanceSlot()` 호출됨. 건설/수면 임시 상호작용도 만들어서 슬롯 소비 흐름 전체 검증.
4. 미션 실패/성공 시 보드판에 시각적 피드백(느낌표 아이콘 등) — 사용자가 "이건 이거(미션 시스템) 다 작성한 다음 진행할 예정"이라고 명시.
5. "웨이브 미선택 상태로 지상 다녀오면 시간 흐르는 문제" 기획 결정 (3옵션 중 선택 필요).
6. **전투 피드백 구현** — 설계는 위 6번 섹션에 완료, 실제 구현 착수. (총구화염/킥백/카메라셰이크 → 좀비 색상플래시/넉백/스턴 → 피 튀김 파티클 → 사운드 순서 추천)
7. **빌드 전 필수**: `SaveManager.SavePath`를 `Application.persistentDataPath`로 원복, `Assets/_Project/save.json` 정리.
8. 이후: 여러 미션이 이어지는 구조에서 실패 시 페널티(경고 누적/이월 등 GDD 원안) 재도입 여부 검토, 사망 처리 로그라이트화, 지원 동기/유산 기록 시스템, 파괴 가능 장애물, 적 종류 다양화.

---

## 7. 사용자 작업 스타일 (다음 세션에서도 유지할 것)

- **코드는 전부 사용자가 직접 작성/타이핑함.** Claude는 코드 스니펫을 제시하고 설명하되, Edit/Write 도구로 스크립트 파일을 직접 고치지 않음(초기엔 몇 번 직접 고쳤다가 사용자가 명시적으로 "고치지 말고 설명만 하라"고 정정함).
- Unity MCP를 통한 씬/에셋 조작(GameObject 생성, 컴포넌트 부착, 프리팹화, 머티리얼 생성, 텍스처 생성 등)은 허용됨 — 이건 "코드"가 아니라 에디터 설정이라는 구분.
- 질문이 오면 먼저 정확한 원인 진단(왜 이런 증상이 나는지)을 설명한 뒤 수정 방향을 코드로 제시하는 순서를 선호함.
- 오류/버그가 나오면 코드를 실제로 Read해서 정확한 원인을 짚어주길 원함(추측성 답변 지양).
- 설계 관련 질문에서는 "왜 이 방식이 좋은지"와 "실제로 언제 이 방식이 손해인지"를 균형 있게 설명하는 걸 선호 — 무조건 정석/베스트 프랙티스를 밀어붙이기보다, 지금 규모에 맞는 실용적 판단을 요구함 (예: 상속 vs 직접 작성 논쟁에서 사용자가 오버엔지니어링을 지적하면 그 지적이 맞으면 인정하고 더 간단한 대안 제시).
- 메모리 파일(`C:\Users\KGA1\.claude\projects\C--Unity-ZombieWaveDefence\memory\project_zombie_wave_defence.md`)에 프로젝트 결정사항이 계속 누적되고 있음 — 새 세션에서도 이 메모리가 로드되므로 이 SessionLog와 함께 참고하면 됨.
