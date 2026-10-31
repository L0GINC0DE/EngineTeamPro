# 턴 전투 구조 변경

## 목표와 설계

제공된 BattleUnit, Attacker, ICommand, AttackCommand 구조를 현재 프로젝트에 적용한다.
체력은 기존 Agents.HealthModule을 사용하며 초기화 책임은 HealthModule에 둔다.
Player는 기존 MonoBehaviour 그대로 사용한다. BattleUnit도 독립된 MonoBehaviour 컴포넌트다.
플레이어와 적 오브젝트에 BattleUnit, Attacker, HealthModule을 붙이고 타입을 Inspector에서 지정한다.
TurnManager의 Player 슬롯에는 플레이어 오브젝트의 BattleUnit을 직접 지정한다. 자동 검색하지 않는다.
기존 턴 전용 상속 클래스는 제거하고 관련 프리팹/씬의 컴포넌트와 참조를 BattleUnit으로 교체했다.
턴 상태는 TurnManager.CurrentUnit 한 곳에서 관리한다. 전투 순서는 플레이어 → 등록된 적 순서다.
턴 진행은 재귀 호출과 이벤트 재구독 대신 매니저의 반복문 한 곳에서 처리한다.

## 구현 및 검증 순서

- 실제 Unity에서 체력 회복 시 피격 이벤트가 발생하는 회귀 테스트 실패 확인 후 수정.
- BattleUnit/Attacker/Command를 추가하고 HealthModule의 초기화와 피격 이벤트 분리.
- TurnManager의 생존 검사, 재진입 차단, 전투 종료 및 새 전투 등록 구현.
- 플레이어 공격 입력 및 공격 UI를 매니저에 연결.
- Unity Play Mode에서 공격 성공/실패, 턴 순서, 사망, 재등록, 기존 씬/프리팹 검증.
- 불필요한 Turn 클래스, 종료 이벤트, 중복 상태/API 제거. MonoSingleton 상속 유지.

## 동작 계약

TestScene에서는 Game 뷰에 포커스를 두고 Space 키를 누르면 첫 번째 생존 적을 공격한다.
한 번 누를 때 한 번 공격하며, 키를 누르고 있어도 반복 공격하지 않는다.
공격 실패는 턴을 소비하지 않는다. 플레이어 자신과 미등록 대상은 공격할 수 없다.
적/보스는 각자 한 번 공격하며 죽거나 비활성화된 대상은 건너뛴다.
적 목록은 전달된 항목을 그대로 등록한다. 같은 적을 여러 번 등록하면 그 횟수만큼 행동한다.
적 행동과 공격 콜백 도중의 추가 공격/턴 종료 요청은 무시한다.
전투 종료 후 StartTurn/EndTurn/공격은 무시한다. AddTurn으로 새 전투를 등록하면 종료 상태가 풀린다.
새 전투 등록은 체력을 자동 회복하지 않는다. 부활이 필요하면 ResetHealth를 명시적으로 호출한다.
일반 BattleUnit은 Player/Enemy/Boss 타입을 Inspector에서 지정한다.
