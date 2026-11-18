PurpleSlash_DLJ — 보라색 검격

- 투명 PNG 시트: 680 x 520 px, 가로 5칸 x 세로 5칸, 마지막 칸은 미사용
- 프레임: 136 x 104 px, 총 24프레임, 왼쪽 위에서 오른쪽으로 읽고 다음 줄로 이동
- 재생: 30 fps, 약 0.8초, 1회 재생
- 피벗: 프레임 왼쪽 위 기준 (44, 92), Unity 아래쪽 기준 (44, 12)
- Unity 정규화 피벗: (0.3235294, 0.1153846)
- Pixels Per Unit: 100
- Point 필터, 압축 없음, 밉맵 없음
- 마지막 프레임은 완전히 투명해서 재생이 끝나면 검격이 사라져

Unity 사용
이 프로젝트에는 Assets/02.Sprites/DLJ/Effects/PurpleSlash_DLJ에 PNG와 .anim을 넣어 뒀어.
PNG는 이미 24개 스프라이트로 분할돼 있어.
SpriteRenderer와 Animator가 같은 오브젝트에 있는 상태에서 PurpleSlash_DLJ.anim을 재생하면 돼.
클립은 SpriteRenderer.m_Sprite를 제어하며 UI Image용 클립은 아니야.
다른 프로젝트에서는 PNG, PNG.meta, anim, anim.meta를 함께 복사해야 클립 참조가 유지돼.
직접 분할할 경우 Sprite Mode=Multiple, Grid By Cell Size=136 x 104로 설정하고 마지막 빈 칸은 제외해.

추가 파일
- PurpleSlash_DLJ.json: 프레임 좌표와 pixelfx 메타데이터
- PurpleSlash_DLJ.aseprite: 24프레임 편집용 원본
- PurpleSlash_DLJ_preview.gif: 어두운 배경 위 3배 확대 미리보기, 배경은 실제 PNG에 포함되지 않아
- PurpleSlash_DLJ_settings.json: 재현용 보라색 팔레트와 설정

검증
Unity 6000.3.20f1에서 스프라이트 24개 가져오기, 피벗, 30 fps, 비반복 설정과
Animator가 있는 오브젝트의 애니메이션 24프레임 샘플을 확인했어.
실제 게임의 공격 판정이나 캐릭터 동작에는 아직 연결하지 않았어.

출처
pixelfx의 melee/sword-slash를 보라색으로 조정했어.
https://github.com/patch-gg/pixelfx
원래 검격 레시피: Hugo Duprez / Sprite Fusion
https://www.spritefusion.com/pixel-art-vfx/slash
Copyright (c) 2026 Patch
Copyright (c) 2026 Hugo Duprez
MIT 라이선스 전문은 LICENSE_pixelfx.txt에 있어.