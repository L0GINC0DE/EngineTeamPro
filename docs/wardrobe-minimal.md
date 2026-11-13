# 옷장 최소 구현

새 런타임 클래스 없이 기존 InteractTest(옷장), UIManager(표시), PlatformPlayer(현재 장비)를 연결한다.
한 옷장에 UIData 하나를 지정하고, 몸 또는 무기 버튼으로 적용한다.
같은 UIData를 몸과 무기에 동시에 장착할 수 없다. 해당 버튼도 비활성화한다.
선택에 성공하면 즉시 적용하고 화면을 닫는다. Enter/E를 길게 눌러 열고 Esc 또는 닫기 버튼으로 닫는다.
Interact는 원래의 Hold 설정을 사용한다. 도중에 키를 떼면 취소되고, 설정된 유지 시간이 지나야 완료된다.
선택 상태는 패널의 activeInHierarchy로 판단한다. infoUI.SetActive(false)만 호출해도
즉시 이동·점프·공격이 다시 허용되고 장비 교체는 차단된다. EndWardrobe나 옷장 참조 초기화는 필요 없다.
옷장 이용 중에는 이동·점프·공격을 막고, 이용 종료·범위 이탈·비활성화 후 변경은 거절한다.
사거리는 무기 데이터에서 읽는다. 실제 공격 동작 추가는 이번 범위에 포함하지 않는다.

## 구현 순서

- [x] 개별 교체·범위 제한·버튼 연결 회귀 테스트를 추가한다.
- [x] 남은 TestPlayerData 참조와 Interact 인자 불일치를 정리한다.
- [x] 현재 슬롯 하나만 교체하고 기존 SetUp에서 수치를 적용한다.
- [x] 기존 버튼의 선택·닫기와 Enter 입력을 연결한다.
- [x] 별도 Unity 검증 프로젝트에서 Play Mode 테스트를 실행한다.

PlayerData에는 별도 생성자를 두지 않고 현재 몸·무기 참조만 저장한다.
ChangeBody() / ChangeWeapon()이 옷장의 Data를 직접 사용하므로 별도의 데이터 인자는 없다.
Enter는 키보드 상태를 주입해 실제 InputAction → PlayerInputReader → Interactor → 옷장 화면 경로를 확인한다.
기존 씬·프리팹의 Inspector 참조는 자동으로 변경하지 않았다.

## Inspector 연결

1. 플레이어의 Starting Body / Starting Weapon에 UIData 에셋을 지정한다.
2. Interactor의 Range와 What Is Interactable을 설정한다.
3. 옷장에는 Collider2D와 InteractTest를 붙이고 Data를 지정한다.
4. 씬의 UIManager에 Body Img / Weapon Img / Body Btn / Weapon Btn / Info UI를 연결한다.
5. Info UI는 UIManager 자체가 아닌 자식 패널로 지정한다. 처음에는 비활성화한다.
6. UI 버튼을 사용할 Canvas, GraphicRaycaster, EventSystem/InputSystemUIInputModule은 기존 씬 구성을 사용한다.
7. 닫기 버튼은 UIManager.HideUI에 연결한다. Esc도 같은 동작을 한다.

목록 UI, 새 매니저, 저장, 공격 동작 구현, 씬·프리팹 자동 생성은 추가하지 않는다.
