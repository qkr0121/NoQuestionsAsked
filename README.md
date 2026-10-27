# Regardless of Reason (사유 불문)

> 지원 동기는 적게 하지만, 그 이유가 무엇이든 중요하지 않다.

크리스탈 좀비 행성에서 28일 동안 방어벽 뒤에서 버티며 마감 미션을 채우는 **탑다운 웨이브 디펜스**입니다.
Unity 6 · URP · New Input System · C# (스크립트 109개, 약 6,800줄)

## 게임 한눈에 보기
- **28일의 런**: 7일마다 크리스탈 상납 마감, 넘기면 즉시 게임오버, 28일을 채우면 로켓으로 귀환
- **웨이브 10개**: 이전 웨이브를 깨야 다음이 열리고, 주차마다 상한(3 · 6 · 9 · 10)이 있음
- **방어벽 방어**: 경고가 뜨면 방어벽 뒤로 물러나 막는 구조. 방어벽 3등급
- **서류 UI**: 난이도와 지원 동기를 근무계약서에 체크하고 도장을 찍어 선택
- 세이브 3칸, 노말 / 서바이벌 난이도

조작: `WASD` 이동 · 마우스 조준/좌클릭 사격 · `R` 재장전 · `1·2·3` 무기 · `4` 수류탄 · `Q` 회복약 · `E` 상호작용 · `ESC` 메뉴

---

## 기술적으로 신경 쓴 부분

### 1. 오브젝트 풀링과 이중 반납 방지
좀비·투사체·이펙트·데미지 숫자를 풀에서 재사용합니다 (`EnemyPool`, `ProjectilePool`, `EffectPool`, `DamageNumberPool`).
- 좀비는 `UnityEngine.Pool.ObjectPool<T>`을 타입별로 두고 `Dictionary<EnemyType, ObjectPool<EnemyMovement>>`로 관리합니다.
- 사망, 타이머, 충돌 등 **여러 경로에서 반납이 중복 호출될 수 있어** `isReleased` 플래그로 두 번째 반납을 막습니다 ([`EnemyMovement.cs`](Assets/_Project/Scripts/Enemy/EnemyMovement.cs)).
- 풀에서 다시 꺼낼 때 `OnEnable`에서 슬로우·밀림 상태를 초기화해 이전 상태가 남지 않게 했습니다.
- 투사체는 명중 후 잔상(Trail)이 끊기지 않도록 **반납을 잔상 시간만큼 미룹니다** ([`Projectile.cs`](Assets/_Project/Scripts/Combat/Projectile.cs)).

### 2. NavMesh 없는 좀비 AI
맵이 단순해서 NavMesh 대신 `Transform` 직선 추격으로 구현했습니다 ([`EnemyMovement.cs`](Assets/_Project/Scripts/Enemy/EnemyMovement.cs)).
- 회전은 프레임레이트에 무관하게 일정한 속도가 나오도록 **지수 감쇠 Slerp**(`1 - exp(-k·dt)`)를 씁니다.
- **분리 조향(separation steering)**: 활성 좀비 목록을 `static List`로 관리하고, 몸집 반경이 겹치면 서로 밀어냅니다. 질량 비율로 헤비가 덜 밀리게 하고, 데드존과 `Lerp` 스무딩으로 떨림을 막았습니다. 그 결과 좀비가 한 점에 뭉치지 않고 **플레이어를 고리 모양으로 둘러쌉니다.**
- 완전히 같은 위치에 스폰되면 밀 방향을 정할 수 없으므로 `GetInstanceID()` 기반의 고정 방향으로 처리합니다.

### 3. 방어벽 공격: SphereCast + 애니메이션 이벤트
[`EnemyAttack.cs`](Assets/_Project/Scripts/Enemy/EnemyAttack.cs)
- 벽 감지는 `Raycast`가 아니라 **`SphereCast`**(좀비 몸통 반경)를 써서 대각선으로 접근해도 벽을 잡습니다.
- 막히기 전에는 매 프레임, 막힌 뒤에는 0.25초 주기로만 확인해 비용을 줄였습니다.
- **피해는 공격 모션의 내려찍는 프레임에 Animation Event로** 적용합니다. 이벤트 순간에 벽이 이미 부서졌으면 피해를 주지 않도록 한 번 더 확인합니다. 모션 배속은 피해 주기에 맞춰 조절합니다.
- 방어벽에 막힌 좀비도 플레이어 쪽으로 몸을 돌려, 플레이어가 반대편으로 가면 막힘이 풀리고 다시 추격합니다.

### 4. 탄속이 빨라도 뚫리지 않는 투사체
[`Projectile.cs`](Assets/_Project/Scripts/Combat/Projectile.cs)
- 이번 프레임에 이동할 구간을 **먼저 Raycast로 검사**하고 이동하므로, 탄속이 아무리 빨라도 좀비를 건너뛰지 않습니다.
- 총구가 이미 좀비 콜라이더 안이면 Raycast가 시작점 내부를 무시하는 문제가 있어, 발사 직후 `OverlapSphere`로 따로 명중 처리합니다.

### 5. 데이터 주도 설계 (ScriptableObject)
웨이브 · 무기 · 방어벽 · 수류탄 · 회복약 · 사운드 · 지원 동기 · 튜토리얼 문구를 모두 ScriptableObject로 둬서 **코드를 건드리지 않고 조정**합니다.
- 웨이브는 `WaveDefinition`의 구간(`EnemySpawnSegment`) 배열로 정의합니다. 구간마다 종류, 마릿수, 방향, 동시 스폰 수, 대기 시간, 경고 표시, "필드를 비울 때까지 대기" 여부를 설정합니다 ([`WaveDefinition.cs`](Assets/_Project/Scripts/Wave/WaveDefinition.cs)).
- [`WaveSpawner`](Assets/_Project/Scripts/Wave/WaveSpawner.cs)는 이를 `Queue<SpawnBatch>`로 풀어 놓고 타이머로 소비합니다. 돌격 전 경고와 숨 돌리는 시간은 **데이터만 바꿔서** 연출합니다.

### 6. 씬 구조와 실행 순서
`MainMenu · Loading · Base · ZWD` 네 씬을 오갑니다.
- 각 씬에 [`GameBootstrap`](Assets/_Project/Scripts/Core/GameBootstrap.cs)이 하나씩 있고, 정적 플래그로 **`PersistentManagers`를 최초 한 번만 `DontDestroyOnLoad`로 생성**합니다. 어느 씬에서 시작해도 동작합니다.
- 씬이 로드될 때마다 호출되는 `SceneManager.sceneLoaded` 콜백에서는 **"이미 런이 진행 중이면 다시 실행하지 않는" 가드**로 세이브 로드와 미션 발급이 반복되지 않게 했습니다 ([`RunState.cs`](Assets/_Project/Scripts/Core/RunState.cs)).

### 7. 세이브 시스템
[`SaveManager.cs`](Assets/_Project/Scripts/Core/SaveManager.cs)
- JSON 3칸 (`JsonUtility`), 전투에서 복귀할 때 자동 저장합니다.
- **에디터와 빌드의 저장 경로를 `#if UNITY_EDITOR`로 분리**했습니다. 에디터에서는 세이브 파일을 바로 열어 보고, 빌드에서는 `persistentDataPath`를 씁니다.
- 마감 실패로 끝난 런은 저장하지 않고, 서바이벌 난이도는 사망 시 칸을 삭제합니다.

### 8. 오디오 시스템
[`AudioManager.cs`](Assets/_Project/Scripts/Core/AudioManager.cs)
- 효과음 `AudioSource` 풀(40개)과 믹서 그룹 4개(Music · World · UI · Ambient)로 구성하고, 노출 파라미터(Master · Music · Sfx)로 볼륨을 제어합니다.
- 슬라이더 값(0~1)을 `Mathf.Log10(v) * 20`으로 **dB로 변환**해 사람 귀에 맞는 볼륨 곡선을 만듭니다.
- 음악은 `LoopChannel`이 **두 `AudioSource`를 번갈아 코루틴으로 크로스페이드**합니다. 씬이 바뀌어도 끊기지 않습니다.
- 소리는 `SoundData`에 담고 `AudioManager.PlaySfx(SoundData)`로 재생하므로, 호출하는 쪽은 클립이나 믹서를 알 필요가 없습니다.

### 9. UI 입력 잠금을 컴포넌트로 분리
[`LocksInputWhileActive`](Assets/_Project/Scripts/Interaction/LocksInputWhileActive.cs)를 UI 패널에 붙이면 **패널이 켜질 때 플레이어 입력이 잠기고 꺼질 때 풀립니다** (`OnEnable` / `OnDisable`). 각 UI 스크립트가 잠금·해제를 직접 호출하지 않아도 되고, 해제 누락으로 인한 입력 막힘이 구조적으로 사라집니다.

---

## 폴더 구조
```
Assets/_Project/
├─ Scripts/   Camera · Combat · Core · Enemy · Interaction · Player · UI · Wave
├─ Datas/     ScriptableObject 데이터 (웨이브 · 무기 · 사운드 · 튜토리얼 등)
├─ Prefabs/ · Models/ · Sprites/ · Materials/ · Audios/ · Scenes/
```

## 사용한 에셋과 도구
| 분류 | 출처 |
|---|---|
| 3D 모델 · UI 이미지 | AI 생성 (VARCO 3D, VARCO Image, GPT Image) |
| BGM | AI 생성 (VARCO 3D Sound) |
| 효과음 | Pixabay, Freesound |
| 애니메이션 | Mixamo |
| 이펙트 | WarFX by Jean Moreno (Unity Asset Store, 무료) |
| 폰트 | S-Core Dream |

### 라이선스 표기
- "Flesh Bite.wav" by jgriffie919 — https://freesound.org/s/400174/
  Licensed under [CC BY 3.0](https://creativecommons.org/licenses/by/3.0/). 게임에 맞게 잘라서 사용했습니다.
- "m4a1 or m16 reload sound.mp3", "pistol reload sound.mp3" by GFL7 — CC0
