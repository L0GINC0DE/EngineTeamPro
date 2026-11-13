# 2D 플랫포머 스크립트 설계서

입력·생명주기·시간 등 공통 관리는 단일 모듈에서 처리하고, 신규 콘텐츠는 데이터(ScriptableObject) 설정이나 파생 행동 클래스 확장만으로 연결한다. abstract는 무기·투사체·설치 장치처럼 실행 방식이 분기되는 경계에만 도입한다.

설계 기준  2026년 11월 9일 · Unity 6000.3.20f1 · Tae 브랜치 80bc23e
반영일  2026년 11월 10일

본 문서에 정의된 신규 클래스와 메서드는 향후 구현할 목표 아키텍처다. 현재 코드베이스에 존재하는 API와 명확히 구분하며, 이번 설계 단계에서는 기존 게임 스크립트와 씬을 직접 수정하지 않는다.

진행 원칙  이 설계는 목표 구조를 설명한다. 실제 구현 결과와 플레이 검증에 따라 순서와 세부 구조를 바꾼다. 다음 작업은 프로토타입의 해당 부분과 완료 확인 기준을 함께 제시하며, 한 단계씩 진행한다.

# 1 전체 연결 구조 및 소유권 원칙

## 아키텍처 흐름도

```text
PlayerInputReader ──► PlatformPlayer
                       ├─ AgentMovement (이동·점프·발사·넉백 물리 제어)
                       ├─ CombatController ──► WeaponBehavior ──► AttackExecution
                       ├─ EquipmentController (몸·무기 슬롯 관리 및 스왑)
                       └─ Interactor ──► Interactable (옷장 등 상호작용)

PlayerMenuController ──┬─► EnvironmentManager ──► EnvironmentDevice
                       └─► GameTimeController ◄── 히트스톱(HitStop) 요청

AttackExecution ──► HitReceiver ──► HealthModule ──► 피격 연출 및 사망 이벤트
```

## 공통 규칙의 단일 소유자

| 규칙과 담당 | 관리 책임 |
| --- | --- |
| 공격 주기<br>CombatController | 다음 공격 가능 시각(쿨타임)을 단독으로 보관·판정한다. |
| 중복 타격<br>AttackExecution | 공격 실행 단위마다 생성되어 피격 대상 집합(HashSet<HitReceiver>)을 관리한다. |
| 장비 슬롯<br>EquipmentController | 몸(Body)·무기(Weapon) 슬롯의 장착 상태와 스왑 규칙을 독점 변경한다. |
| 장치 수명<br>EnvironmentManager | 월드 내 단일 활성 장치와 14초 수명 타이머를 관리한다. |
| 시간 배율<br>GameTimeController | 모든 정지·히트스톱 요청을 핸들로 취합해 Time.timeScale을 유일하게 변경한다. |
| 체력과 이동<br>HealthModule / AgentMovement | HealthModule은 수치 증감만, AgentMovement는 Rigidbody2D 제어만 담당한다. |

## 정적 데이터와 런타임 상태의 분리

### 정적 데이터 (UIDataSO)

식별자, 표시 이름, 아이콘, 테마 색상, 몸 기본 능력치, 무기 및 장치 프리팹 참조만 보관한다. 에셋을 런타임 상태 저장소로 절대 사용하지 않는다.

### 런타임 상태

현재 체력, 장착 슬롯, 남은 쿨타임, 설치된 장치 인스턴스는 각 씬의 실행 컴포넌트가 소유한다.

### 파일 및 에셋 관리

기존 스크립트는 .meta 파일(GUID)과 공개 API 계약을 유지한다. 신규 스크립트는 Assets/01.Scripts/Platformer 경로에 추가하며, 유실된 UIData 에셋은 신규 UIDataSO 3종으로 재생성해 참조를 교체한다.

# 2 입력과 플레이어 이동

기존 PlayerInputReader, PlatformPlayer, AgentMovement의 파일과 .meta를 유지한 채 기능을 확장하며, 플레이어 본체를 새로 작성하는 방식은 지양한다.

| 스크립트와 구분 | 보관 상태와 책임 |
| --- | --- |
| PlayerInputReader<br>기존 확장 | MoveInput, AttackHeld<br>이동 벡터와 공격 버튼 유지 상태를 수집하고, JumpPressed, InteractPerformed, PlacementPressed 이벤트를 발행한다. |
| PlatformPlayer<br>기존 확장 | 하위 컴포넌트 참조, 시선 방향<br>입력을 이동·전투·상호작용 모듈에 중계하고 사망 시 조작을 차단한다. 장비 수치나 타격 판정은 직접 계산하지 않는다. |
| AgentMovement<br>기존 확장 | 이동 입력, 점프 버퍼, 마지막 접지 시각, 현재 이동 모드<br>SetMoveDir(), Jump(), SetSpeed(), Launch(), ApplyKnockback(), Stop()을 통해 물리 이동을 일괄 제어한다. |
| PlayerMenuController<br>신규 | 현재 열린 메뉴 종류, 활성 정지 핸들<br>TryOpenWardrobe(), TryOpenPlacement(), CloseCurrent()로 메뉴 전환과 플레이어 조작 제한을 조율한다. |

## AgentMovement 실행 계약

### 점프 판정

Jump() 호출 시 입력 시각을 버퍼에 기록하고, FixedUpdate에서 접지 상태·코요테 타임·점프 버퍼 조건을 검사해 1회 소비한다 (제안 기본값: 버퍼 0.15초, 코요테 타임 0.15초).

### 발사(Launch(LaunchRequest))

초기 속도와 적용 축 정보를 전달받는다. 수평 발사기는 X·Y 속도를 모두 덮어쓰고, 수직 점프대는 Y 속도만 지정하며 기존 X 속도는 유지한다.

### 발사 잠금

발사 직후 0.2초 동안 일반 이동 및 점프 실행을 잠근다. 잠금 중에도 메뉴 열기 입력은 허용하되, 일반 이동 로직이 발사 속도를 덮어쓰지 않도록 보호한다.

### 이동 모드 우선순위

조작 종료(Dead/Disabled) → 넉백(Knockback) → 발사(Launch) → 일반 이동(Normal) 순으로 적용한다. 발사 도중 피격되어 넉백이 발생하면 즉시 발사 모드를 종료한다.

### 물리 및 타이머 기준

모든 물리 속도 갱신과 추가 중력 적용은 FixedUpdate로 일원화한다. 발사 잠금과 점프 관련 타이머는 게임 시간(Time.deltaTime)을 기준으로 삼아 메뉴 정지 중에는 흐르지 않게 한다.

## 입력 바인딩 및 생명주기 규칙

Input Action 에셋에서 Attack은 Z 키, Environment는 C 키로 바인딩하고 C# 래퍼 클래스를 재생성한다.

메뉴 진입 시 UI 조작 입력은 열어두되 인게임 행동 입력은 차단한다. 메뉴 진입 즉시 점프 버퍼를 비우며, 메뉴 종료 후 공격은 Z 키를 한 번 떼었다가 다시 눌러야 시작되도록 처리한다(제안 기본값).

Awake에서 컴포넌트 참조를 확보·검증하고(transform.root 탐색 대신 인스펙터에 명시적으로 할당된 Rigidbody2D 사용), OnEnable/OnDisable에서 이벤트 구독과 해제를 대칭으로 관리한다. PlayerInputReader는 OnDestroy에서 생성한 입력 에셋 인스턴스를 반드시 Dispose한다.

# 3 무기와 공통 피해 처리

플레이어와 적이 동일한 전투 파이프라인을 공유할 수 있도록 기존 PlayerCombatController 구상을 범용 CombatController로 일반화한다. 차크람은 발사기(ProjectileWeapon)와 귀환 탄환(ReturningProjectile)으로 역할을 분리한다.

```text
WeaponBehavior (abstract)
 ├─ MeleeWeapon          : 부채꼴·직사각형 판정 데이터로 검과 봉을 구분 (단일 클래스)
 └─ ProjectileWeapon     : 지정된 투사체 프리팹을 월드에 생성 및 발사

Projectile (abstract)
 ├─ ReturningProjectile  : 차크람의 최대 사거리 전진 및 소유자 방향 귀환
 └─ LinearProjectile     : 원거리 적의 직선 비행 탄환
```

| 구성 요소와 분류 | 핵심 계약과 상태 |
| --- | --- |
| CombatController<br>컴포넌트 | TryAttack()에서 생존 여부·메뉴 상태·쿨타임을 검사한다. 조건 충족 시 AttackExecution을 생성하고, 허공에 휘두른 경우를 포함해 공격 시작 즉시 다음 공격 가능 시각을 갱신한다. |
| WeaponBehavior<br>abstract | Initialize(context)와 Unequip()으로 초기화·해제 흐름을 통일한다. TryBeginAttack(execution)에서 공통 유효성을 검사한 뒤 하위 클래스의 BeginAttackCore(execution)을 호출한다. |
| MeleeWeapon<br>파생 클래스 | 별도 파생 클래스 없이 설정값(부채꼴/직사각형 범위)만으로 검과 봉을 구분한다. 범위 내 대상을 검출해 execution.TryHit(receiver)를 호출한다. |
| ProjectileWeapon / Projectile<br>파생 및 abstract | 발사·충돌·소멸 흐름은 부모에서 공통 처리하고 궤적 계산만 자식이 구현한다. 귀환형 투사체는 최대 사거리 도달 후 소유자에게 돌아오며, 지형(Ground) 충돌 시 즉시 소멸한다. |
| AttackExecution<br>일반 C# 객체 | 공격 1회당 새로 인스턴스화된다. 발사 시점의 공격력 스냅샷·소유자·진영·방향과 피격 집합(HashSet<HitReceiver>)을 보관하며, Finish()로 판정을 최종 종료한다. |
| HitReceiver<br>신규 컴포넌트 | TryReceive(hit)에서 사망·진영·무적 시간을 검사한다. 몸 장비의 피해 배율을 적용한 뒤 HealthModule.TakeDamage()와 이동 모듈의 넉백을 순차 요청한다. |

## 공격 실행의 수명과 중복 타격 판정

### 루트 단위 식별

피격 대상은 개별 Collider2D가 아니라 적 루트 객체의 HitReceiver를 기준으로 식별한다. 한 몬스터에 여러 콜라이더가 붙어 있어도 단일 대상으로 묶어 중복 피해를 막는다.

### 무적 판정 기록

유효 접촉이 발생한 대상은 즉시 이번 공격의 피격 집합에 등록한다. 피격자의 무적 상태 때문에 피해가 거부되었더라도 동일 공격 실행 중에는 재시도하지 않는다.

### 판정 종료(Finish) 시점

근접 무기: 무기 컴포넌트가 실행 객체를 소유하며, 공격 모션 종료 또는 장비 해제 시점에 Finish()를 호출한다.투사체: 적을 관통·타격해도 실행 객체를 유지하며, 귀환 완료·지형 충돌·강제 소멸 시점에 Finish()를 호출한다. 동시에 날아가는 투사체들은 각자 독립된 피격 집합을 가지며, 차크람은 왕복 전체 주기를 통틀어 대상당 1회 타격을 기본값으로 둔다.

### 투사체 생명주기 분리

투사체는 무기 소켓의 자식이 아닌 월드 공간에 생성하여, 공격 도중 무기를 교체해도 파괴되지 않고 기존 공격력 스냅샷으로 끝까지 비행하게 한다. 단, 소유자 사망이나 스테이지 종료 시 일괄 회수할 수 있도록 소유자별 활성 투사체 목록을 유지한다.

# 4 장비 교체와 옷장 상호작용

슬롯 상태와 교환 규칙은 EquipmentController가 독점 소유하며, 게임 시작 시 초기 장비 장착도 동일한 경로를 거친다. UI View는 사용자 입력 전달과 화면 갱신만 수행하고, 실제 무기 프리팹 생성·파괴는 CombatController 한 곳에서 처리한다.

| 스크립트와 데이터 | 주요 필드와 API |
| --- | --- |
| UIDataSO<br>신규 에셋 | Id, DisplayName, Icon, ThemeColor, BodyStats, BodySprite, WeaponPrefab, DevicePrefab. (무기 공격력·쿨타임·판정 크기는 데이터 중복을 막기 위해 무기 프리팹에만 저장한다.) |
| EquipmentController<br>신규 컴포넌트 | CurrentBody, CurrentWeapon, TryEquip(slot, data), EquipmentChanged 이벤트. 슬롯 참조를 보관하며 몸 능력치 반영과 무기 교체를 조율한다. |
| HealthModule<br>기존 확장 | 기존 TakeDamage(), ResetHealth(), CurrentHealth 및 이벤트를 유지한 채, SetMaxHealth(newMax, preserveRatio)와 최대 체력 변경 이벤트를 추가한다. |
| Interactable / Wardrobe<br>abstract 및 파생 | TryInteract(player)에서 상호작용 가능 여부를 공통 검사한 뒤 OnInteractCore(player)를 호출한다. Wardrobe는 옷장 메뉴 열기를 요청한다. |
| Interactor / WardrobeView<br>신규 컴포넌트 | Interactor는 주변의 유효 상호작용 대상을 탐색한다. WardrobeView는 슬롯 선택·닫기 입력과 UI 표시만 담당하며 체력·시간·프리팹을 직접 건드리지 않는다. |

## 장비 교체 실행 순서

- 플레이어의 Enter 키 홀드 완료 → Interactor가 현재 옷장의 유효성 확인

- PlayerMenuController가 옷장 메뉴를 활성화하고 GameTimeController로부터 일시정지 핸들 확보

- WardrobeView에서 장착할 슬롯과 UI 선택 → EquipmentController.TryEquip(slot, data) 호출

- 두 슬롯의 변경 결과(단순 변경 또는 스왑) 계산 및 새 무기 프리팹 준비 상태 사전 검증

- 검증 통과 시 몸 능력치와 새 무기를 적용하고 슬롯 상태를 원자적(Atomic)으로 확정

- EquipmentChanged 이벤트를 발행해 화면 갱신 → 메뉴 닫기 시 정지 핸들 해제

## 세부 교체 규칙

### 중복 장착 금지 및 자동 스왑

동일한 UI를 몸과 무기 두 슬롯에 동시에 장착할 수 없다. 반대편 슬롯에 이미 장착된 UI를 선택하면 두 슬롯의 장비를 맞교환(Swap)하며, 현재 슬롯과 동일한 UI를 다시 선택하면 아무 작업도 하지 않는다.

### 체력 비율 보존 및 정밀도 유지

몸 장비 교체 시 현재 HP 비율을 유지한다. 반복 교체로 인한 반올림 체력 증식 버그를 막기 위해 내부 연산용 체력은 부동소수점(float) 정밀도로 보관하고, 화면 표시용 CurrentHealth만 정수로 변환한다. 사망 상태에서는 장비 교체로 부활할 수 없다.

### 실패 복구 및 쿨타임 유지

사전 검증이나 새 무기 초기화에 실패하면 기존 슬롯과 무기를 그대로 유지한다. 무기를 교체하더라도 CombatController의 남은 공격 쿨타임은 초기화하지 않는다.

# 5 환경 장치 설치와 공통 수명 관리

EnvironmentManager가 설치 프로세스와 월드 내 단일 활성 장치를 관리한다. 최상위 EnvironmentDevice는 초기화와 제거 수명주기만 공통화하고, 플레이어와의 물리 접촉 판정이 필요한 장치들은 ContactDevice를 상속받아 재진입 제한 로직을 공유한다.

```text
EnvironmentDevice (abstract)
 ├─ TemporaryPlatform        : 4타일 폭의 단방향(One-way) 물리 발판
 └─ ContactDevice (abstract) : 플레이어별 접촉 콜라이더 집합 및 초기 겹침 발동 제한 관리
     └─ LaunchDevice         : 설정된 방향과 축으로 대상을 발사 (점프대·수평 발사기 겸용)
```

| 담당 모듈 | 주요 필드와 공개 계약 |
| --- | --- |
| EnvironmentManager | CurrentDevice, availableUIs, lifetimeSeconds = 14f. TryBeginPlacement(), TryConfirmPlacement(request), CancelPlacement() 제공. 수명 만료를 추적하며 설치 확정 시 Installed 이벤트를 발행한다. |
| EnvironmentDevice | Initialize(DeviceContext) 호출 시 내부 OnPlacedCore() 실행. Remove(reason)는 중복 호출을 차단하고 콜라이더와 동작을 먼저 끈 뒤 OnRemoving()으로 하위 클래스의 개별 정리를 호출한다. |
| ContactDevice | 플레이어별로 현재 겹쳐 있는 Collider2D 집합과 초기 발동 금지 대상을 추적한다. 초기 겹침이 완전히 해소된 후의 유효 재진입 시에만 OnContactCore(player)를 호출한다. |
| LaunchDevice | LaunchRequest(속도·적용 축·잠금 시간) 데이터를 구성해 AgentMovement.Launch()를 호출한다. 플레이어의 Rigidbody2D를 직접 조작하지 않는다. |
| TemporaryPlatform / PlacementPoint | 발판은 4타일 폭과 One-way 충돌을 프리팹 단에서 설정한다. PlacementPoint는 설치 좌표·허용 방향·현재 사용 가능 여부를 제공한다. |

## 설치 프로세스와 예외 처리

### 설치 및 교체 순서

C 키 입력 → 메뉴 열기 및 카메라 추적 정지 → 화면 내 유효 PlacementPoint 선택 → 장착할 UI 및 방향 선택 → 새 장치 인스턴스 준비 및 배치 검증 → 기존 활성 장치 제거 → 새 장치를 CurrentDevice로 확정 → 메뉴 닫기 순으로 진행한다. 도중 취소하거나 배치 검증에 실패하면 기존 장치를 파괴하지 않고 유지한다.

### 수명 타이머 기준

14초 수명은 인게임 플레이 시간(Time.deltaTime)을 기준으로 차감하여, 메뉴 열람이나 히트스톱 정지 중에는 수명이 줄어들지 않게 한다.

### 초기 겹침 발동 제한 (ContactDevice)

설치 순간 이미 장치와 겹쳐 있는 플레이어는 즉시 '발동 금지 대상'으로 등록한다. 해당 플레이어의 모든 콜라이더가 장치 영역에서 완전히 빠져나온 뒤에야 제한이 해제되며, 다중 콜라이더 중 일부만 OnTriggerExit2D된 상태에서는 제한을 풀지 않는다.

### 발판 배치 제한 (TemporaryPlatform)

물리 발판은 플레이어와 겹치는 위치에 설치할 수 없도록 사전 검증 단계에서 거부한다(접촉 장치의 재진입 제한 규칙을 발판에 억지로 적용하지 않는다).

### 안전한 참조 정리

장치 교체·시간 만료·씬 종료는 모두 Remove(reason) 경로로 수렴한다. 파괴되는 이전 장치가 제거 콜백을 보내더라도, EnvironmentManager는 해당 장치가 현재 CurrentDevice와 일치할 때만 참조를 비워 새 장치 참조가 덮어써지는 현상을 막는다.

### 선택 목록 기본값

설치 메뉴에는 기본적으로 UI 3종을 모두 표시하며, 장치 설치가 플레이어의 장비 슬롯을 소모하지 않도록 한다. 추후 해금·보유 제한이 추가될 경우 availableUIs 필터링 단계에서 처리한다.

# 6 시간 정지와 메뉴 제어

개별 메뉴나 피격 연출 스크립트가 Time.timeScale을 직접 건드리거나 임의로 1f로 복원하지 못하도록 차단한다. 모든 정지 요청에 고유 핸들을 발급하고, 활성 요청이 단 하나라도 남아 있으면 정지 상태를 유지한다.

| 스크립트와 분류 | 보유 상태와 핵심 역할 |
| --- | --- |
| GameTimeController<br>신규 컴포넌트 | AcquirePause(reason) 호출 시 PauseHandle을 발급한다. RequestHitStop(seconds)는 실제 시간(unscaledTime) 기준 만료 타이머를 등록한다. 활성 요청 목록이 비어 있지 않으면 IsPaused = true 및 Time.timeScale = 0f을 유지한다. |
| PauseHandle<br>일반 C# 객체 | 고유 요청 ID와 해제 여부를 보관한다. Dispose() 호출 시 전체 목록을 초기화하지 않고 자신의 요청 ID만 정확히 1회 해제한다. |
| PlayerMenuController<br>신규 컴포넌트 | 현재 활성 메뉴 상태(None / Wardrobe / Placement / Pause)를 보관하며, 메뉴 UI 표시·카메라 정지·점프 버퍼 초기화·정지 핸들의 수명을 통합 관리한다. |
| 메뉴 View와 카메라<br>UI 및 연출 | 각 View는 선택·닫기 이벤트만 발행한다. 메뉴 활성화 시 PlayerMenuController가 Cinemachine 카메라의 추적·평가를 일시 중단해 화면을 고정하고, 메뉴 종료 시 복원한다. |

## 다중 정지 요청 중첩 시나리오

```text
1 히트스톱 H 발생       활성 요청 {H}      timeScale 0  정지
2 설치 메뉴 M 발생     활성 요청 {H, M}   timeScale 0  정지 유지
3 히트스톱 H 만료      활성 요청 {M}      timeScale 0  메뉴 정지 유지
4 설치 메뉴 M 닫힘     활성 요청 {}       timeScale 1  게임 재개
```

## 시스템별 참조 시간 기준

| 대상 | 시간 기준과 정지 중 동작 |
| --- | --- |
| 무기 쿨타임과 장치 14초 | 게임 시간 (Time.deltaTime)<br>진행을 멈추고 남은 시간을 보존한다. |
| 발사 잠금과 점프 버퍼 | 게임 시간 (Time.deltaTime)<br>발사 잠금 타이머는 멈추며, 점프 버퍼는 메뉴 진입 즉시 비운다. |
| 히트스톱 지속 시간 | 실제 시간 (Time.unscaledDeltaTime)<br>timeScale이 0인 상태에서도 정상적으로 만료 시각을 계산한다. |
| UI 입력과 메뉴 애니메이션 | 실제 시간 (Unscaled Time)<br>정지 상태와 무관하게 슬롯 선택·닫기·연출을 정상 처리한다. |

## 레거시 히트스톱 연결과 누수 방지

기존 KTH_HitStop은 직전 timeScale을 지역 변수로 저장했다가 직접 덮어쓰는 구조이므로, 내부 구현을 GameTimeController에 핸들을 요청·해제하는 어댑터 구조로 교체한다. 외부 전투 스크립트의 호출 시그니처는 그대로 유지하면서 실제 시간 배율 제어권만 단일화한다.

메뉴 닫기, 컴포넌트 OnDisable/OnDestroy, 씬 전환 시점에 보유 중인 모든 PauseHandle과 카메라 고정 상태를 강제 해제해 정지 상태가 다음 판으로 누수되지 않도록 보장한다.

# 7 적 행동(AI)과 스테이지 진행

적 캐릭터도 플레이어와 동일한 무기(WeaponBehavior), 체력(HealthModule), 물리 이동(AgentMovement) 모듈을 재사용하며, 오직 의사결정(EnemyBrain)만 교체한다. 개발 초기에는 근접형 적 하나로 전체 루프를 먼저 완성한 뒤 나머지 파생형을 확장한다.

| 스크립트와 분류 | 공통 또는 개별 책임 |
| --- | --- |
| EnemyBrain<br>abstract | 이동·전투·체력 컴포넌트 참조를 확보한다. 매 프레임 생존 여부를 먼저 확인한 뒤 TickBrain(deltaTime)을 호출하며, 사망 시 즉시 모든 행동과 공격을 중단한다. |
| MeleeEnemyBrain<br>파생 클래스 | 순찰(Patrol) → 감지(Detect) → 추적(Chase) → 공격 예고(Telegraph) → CombatController 공격 요청 상태 흐름을 수행하며 MeleeWeapon을 사용한다. |
| JumpEnemyBrain<br>파생 클래스 | 주기적인 점프와 이동 방향을 결정한다. 몸통 박치기(접촉 공격) 역시 예외 경로를 두지 않고 CombatController의 쿨타임 및 공통 피해 파이프라인을 경유한다. |
| RangedEnemyBrain<br>파생 클래스 | 플레이어와의 적정 사거리를 유지하며 발사 타이밍을 결정하고, ProjectileWeapon과 LinearProjectile로 원거리 공격을 수행한다. |
| StageController<br>신규 컴포넌트 | 스테이지 진행 상태(Running / Cleared / Failed)와 결과 통계를 보관한다. RegisterEnemy(), ClearStage(), FailStage(), RestartStage()를 제공하며, 적의 OnDead와 장치의 Installed 이벤트를 구독해 통계를 집계한다. |
| StageExit / ResultView<br>신규 컴포넌트 | StageExit은 살아 있는 플레이어의 도달 여부를 감지해 클리어를 요청한다(적 전멸은 클리어 조건에 포함하지 않는다). ResultView는 집계된 기록을 표시하고 재시작 요청을 중계한다. |

## 피격과 사망의 단일 처리 경로

```text
무기 또는 투사체 판정
  → AttackExecution.TryHit(HitReceiver)
  → HitReceiver: 무적 시간 및 몸 장비 피해 배율 검사
  → HealthModule.TakeDamage() 및 AgentMovement 넉백 요청
  ├ 생존 시 피격 이벤트 발행
  │  → BlinkFeedback 깜빡임 및 ImpulseFeedback 카메라 흔들림
  └ HP <= 0: OnDead 이벤트 발행
     ├ 플레이어: 조작 차단 및 열린 메뉴 강제 종료
     │  → StageController.FailStage()
     └ 적: EnemyBrain 및 공격 즉시 종료
        → StageController 처치 수 +1 집계
```

## 피격과 넉백 세부 규칙

### 역할 분리

플레이어의 피격 후 1초 무적과 피해 배율 계산은 HitReceiver 설정에서 처리한다. 공용 HealthModule 내부에 플레이어 조작 차단이나 적 AI 중지 코드를 직접 작성하지 않으며, 장비 교체로 인한 최대 체력 변경(SetMaxHealth) 시에는 피격 연출이 재생되지 않도록 구분한다.

### 넉백 충돌 방지

플랫포머의 모든 넉백은 AgentMovement의 Rigidbody2D 제어로 일원화한다. 기존 KTH_Knockback(Transform 직접 이동 방식)이 물리 연산과 충돌하지 않도록 플랫포머 플레이어의 HealthModule.hitEffect 자동 실행 슬롯은 비워 두고, BlinkFeedback·ImpulseFeedback만 이벤트로 선별 연결한다.

## 플레이어 프리팹 최소 구조

### Root 객체

PlayerInputReader, PlatformPlayer, HealthModule, HitReceiver, EquipmentController, CombatController, Rigidbody2D 배치.

### 자식 구성

AgentMovement는 기존 자식 계층을 유지하되 루트의 Rigidbody2D를 인스펙터에서 명시적으로 참조한다. 하위 WeaponSocket은 CombatController에, Visual 객체는 외형 스프라이트 교체 및 피격 피드백 대상에 각각 연결한다.

현재 코드와 대조할 사항  위 흐름은 목표 설계다. 현재 HealthModule은 치명적인 피해에서도 OnHitEvent를 먼저 발행한 뒤 OnDead를 발행한다. 이 순서를 유지할지 변경할지는 해당 기능을 구현할 때 확인한다.

# 8 작업 분류 및 단계별 구현 순서

## 대상 파일 분류

| 작업 분류 | 대상 스크립트와 세부 내역 |
| --- | --- |
| 기존 스크립트 확장 | PlayerInputReader, PlatformPlayer, AgentMovement, HealthModule (기존 GUID와 외부 호출 계약을 보존하며 기능 확장) |
| 레거시 연동 조정 | KTH_HitStop 시간 제어 어댑터화 및 피격 연출 분리 (AbstractFeedback 계열은 재사용하되 기존 턴제 전투 씬의 회귀 여부를 별도 점검) |
| 신규 코어 모듈 | CombatController, AttackExecution, HitReceiver, EquipmentController, GameTimeController, PlayerMenuController, UIDataSO |
| 신규 행동 및 UI | WeaponBehavior·Projectile 파생 계열, EnvironmentDevice 파생 계열, Interactor·Interactable·Wardrobe, WardrobeView, PlacementView, PlacementPoint |
| 스테이지 및 AI | StageController, StageExit, ResultView는 1차 루프부터 즉시 연결하며, EnemyBrain 파생 클래스들은 적 종류 확장 단계에서 순차 추가 |

## 콘텐츠 확장 가이드

### 검·봉의 변형 무기 추가

기존 전투 코드를 수정하지 않고, MeleeWeapon 프리팹의 공격력·쿨타임·판정 범위 수치만 조정해 새 UIDataSO에 연결한다.

### 새로운 궤적의 투사체 추가

Projectile 추상 클래스를 상속받아 이동 계산만 구현한 뒤 ProjectileWeapon 프리팹에 할당한다(피해 및 중복 타격 판정은 자동 공유).

### 새로운 환경 장치 추가

물리 구조물은 EnvironmentDevice를, 접촉 작동형 장치는 ContactDevice를 상속받아 고유 동작만 구현한다(메뉴 선택·단일 유지·14초 만료는 자동 공유).

### 새로운 UI 테마 추가

새 동작이 없다면 클래스 추가 없이 UIDataSO 에셋만 생성해 외형·몸 능력치·기존 무기 및 장치 프리팹을 조합한다.

## 참고 진행 순서와 검증 기준

다음 네 단계는 우선순위 참고안이다. 한 단계를 시작하기 전 현재 코드와 플레이 결과를 확인하며, 필요하면 단계를 나누거나 순서를 바꾼다. 아래 항목을 모두 미리 구현할 필요는 없다.

### 1단계 기반 이동 및 시간 제어 구축

누락된 에셋 참조를 복구하고 이동·발사 잠금·GameTimeController를 연결한다.

확인 기준   발사 직후(0.2초 내) 반대 방향 키 입력 시 속도가 덮어써지지 않는지, 히트스톱과 메뉴 열기가 겹쳤을 때 시간 배율이 안전하게 유지·복원되는지 확인한다.

### 2단계 코어 전투 및 스테이지 루프 완성

MeleeWeapon, HitReceiver, 근접 적 1종, StageExit을 연결해 시작부터 클리어/실패까지 이어지는 한 판을 완성한다.

확인 기준   다중 콜라이더를 가진 적을 공격했을 때 공격 1회당 피해가 정확히 한 번만 들어가는지 확인한다.

### 3단계 옷장 장비 교체 및 환경 장치 연결

WardrobeView와 PlacementView를 연결하고 슬롯 장착 및 장치 설치를 활성화한다.

확인 기준   반대 슬롯 선택 시 자동 스왑, 몸 교체 시 HP 비율 보존, 무기 교체 시 쿨타임 유지, 설치 취소 시 기존 장치 보존, 접촉 장치 초기 겹침 시 재진입 전까지 발동 보류, 14초 수명 만료를 확인한다.

### 4단계 차크람 귀환 투사체 및 적 AI 확장

ReturningProjectile과 점프형·원거리형 적 AI를 추가한다.

확인 기준   다중 투사체의 피격 집합 독립성, 공격 중 무기 교체 시 기존 차크람 유지, 소유자 사망 및 씬 재진입 시 잔여 투사체·정지 핸들 누수가 없는지 최종 확인한다

# 9 한 단계씩 진행하는 방법

설계서를 고정된 정답으로 두지 않는다. 지금 만들 기능을 프로토타입의 실제 장면과 연결해 설명하고, 작은 작업 하나를 수행한 뒤 결과를 확인한다. 변경이 필요하면 이유와 영향을 정리하고 다음 작업을 다시 고른다.

## 작업 안내 형식

| 항목 | 매번 안내할 내용 |
| --- | --- |
| 프로토타입 위치 | 플레이어 조작·전투·옷장·장치 설치·출구 중 해당 부분을 설명한다. |
| 지금 할 일 | 이번에 수행할 작은 작업 하나와 목적을 안내한다. |
| 작업 위치 | 실제 씬, Hierarchy 오브젝트, 스크립트 또는 Inspector 항목을 짚는다. |
| 확인 기준 | 어떤 입력과 상태로 확인하며 무엇이 보이면 다음 단계로 갈 수 있는지 적는다. |
| 다음 단계 결정 | 현재 파일과 실행 결과를 확인한 뒤 다음 작업을 고른다. |

## 프로토타입 기능과 스크립트의 대응

| 플레이에서 보이는 부분 | 주로 연결되는 스크립트 |
| --- | --- |
| 캐릭터가 발판에서 움직이고 점프함 | PlayerInputReader, PlatformPlayer, AgentMovement |
| Z로 검을 휘둘러 적에게 피해를 줌 | CombatController, MeleeWeapon, AttackExecution, HitReceiver |
| 옷장에서 몸과 무기를 바꿈 | Interactor, Wardrobe, EquipmentController, WardrobeView |
| C로 장치를 놓고 도약하거나 발사됨 | PlayerMenuController, EnvironmentManager, LaunchDevice |
| 출구에 도착하고 결과를 확인함 | StageController, StageExit, ResultView |

## 현재 첫 작업은 기본 테스트 씬 정리

프로토타입 위치  캐릭터가 발판 위에서 이동하고 점프하는 기본 조작 테스트 공간이다. 현재 작업 대상은 Unity의 TestMove 씬이며, 별도 ChainArm HTML 프로토타입과는 구분한다.

작업 위치  Assets/00.Scenes/TAE/TestMove.unity

- Canvas의 UIManager 참조는 이미 제거됐다. StageManager, Square (1), Weapon, Player_Platformer에 남은 Missing Script를 확인한다.

- 남아 있는 StageManager·InteractTest·Weapon·Interactor에 대응하는 Missing Script 컴포넌트만 정리한다. 이미 삭제한 오브젝트를 다시 만들지 않는다.

- 저장한 뒤 Play Mode에서 A/D 이동과 Space 점프를 확인한다. 다른 Console 오류가 발생하면 새 기능 추가보다 해당 원인부터 확인한다.

완료 기준  현재 남은 누락 스크립트가 정리되고, 이동·접지 점프의 실행 결과와 남은 오류를 확인할 수 있다. 실제 실행을 확인하기 전에는 완료로 판단하지 않는다.
