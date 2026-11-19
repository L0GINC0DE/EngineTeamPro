WhiteWaterLeft_DLJ — 왼쪽으로 발사되는 하얀 물줄기

오른쪽 발사 지점에서 왼쪽으로 직진해. 중심 물줄기는 좁게 유지되고,
날아갈수록 가장자리의 거품과 물방울이 조금 퍼졌다가 사라져.
이동 방향은 프레임에 이미 반영돼 있어.

크기와 재생
- PNG 시트: 1024 x 784 px / 가로 4칸 x 세로 7칸
- 프레임: 256 x 112 px, 총 28프레임, 30 fps
- 재생: 약 0.93초, 1회, 마지막 프레임은 완전 투명
- 읽는 순서: 왼쪽 위부터 오른쪽, 이어서 다음 줄
- 피벗: 왼쪽 위 기준 (224, 56), Unity 정규화 좌표 (0.875, 0.5)
- Pixels Per Unit=100, Point 필터, 압축 없음, 밉맵 없음

Unity
Assets/02.Sprites/DLJ/Effects/WhiteWaterLeft_DLJ에 분할 설정과 .anim을 넣어 뒀어.
SpriteRenderer와 Animator를 같은 오브젝트에 놓고 WhiteWaterLeft_DLJ.anim을 재생하면 돼.
SpriteRenderer.flipX=false일 때 왼쪽으로 발사돼. flipX=true면 오른쪽으로 바뀌어.
오브젝트 위치는 발사 지점에 맞추면 돼. 왼쪽 이동은 애니메이션 안에 포함돼 있어.
다른 프로젝트로 옮길 땐 PNG, PNG.meta, anim, anim.meta를 함께 복사해야 참조가 유지돼.

추가 파일
- WhiteWaterLeft_DLJ.json: Aseprite 스타일 프레임 좌표와 타이밍
- WhiteWaterLeft_DLJ.aseprite: 28프레임 투명 배경 편집용 원본
- WhiteWaterLeft_DLJ_preview.gif: 2배 확대 미리보기. 어두운 배경은 실제 시트에 포함되지 않아
- dlj-white-water-left.js: 이 효과의 pixelfx 소스. pixelfx/src/fx/water에 넣고 tools/render.mjs로 렌더링할 수 있어
- DirectionVerification_DLJ.json: 프레임별 중심이 오른쪽에서 왼쪽으로 이동하는지 확인한 기록

검증
pixelfx 검사: 1 pass, 0 fail, 0 warnings.
같은 시드의 결과 일치, 왼쪽 이동, 마지막 투명 프레임을 확인했어.
Unity 6000.3.20f1에서 스프라이트 28개, 피벗과 크기, 비반복 클립의 28프레임 재생을 확인했어.
캐릭터의 스킬 발사나 공격 판정에는 아직 연결하지 않았어.

출처
pixelfx의 water/water-jet을 바탕으로 DLJ 왼쪽 발사 펄스, 직선 경로, 흰색 팔레트와 물방울을 조정했어.
https://github.com/patch-gg/pixelfx
원래 빔 레시피: Hugo Duprez / Sprite Fusion
https://www.spritefusion.com/pixel-art-vfx/energy
Copyright (c) 2026 Patch
Copyright (c) 2026 Hugo Duprez
MIT 라이선스 전문은 LICENSE_pixelfx.txt에 있어.