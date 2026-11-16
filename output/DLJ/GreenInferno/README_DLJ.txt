GreenInferno_DLJ — 다가와서 거대하게 폭발하는 초록 불덩이

초록색 원형 불덩이가 오른쪽에서 왼쪽으로 다가오면서 커져.
짧게 압축된 뒤 밝은 섬광과 함께 거대한 초록 화염, 충격파와 잔불로 폭발해.

재생 구간
- 0.00~0.82초: 원형 불덩이가 왼쪽으로 접근하며 커짐
- 0.82~0.90초: 압축과 밝아지는 핵
- 0.90초: 폭발 시작, 공격 판정 타이밍으로 사용할 수 있는 지점
- 0.90~1.70초: 거대한 화염과 확장 충격파
- 이후: 잔불과 연기가 사라짐

파일 사양
- 프레임: 384 x 320 px, 97프레임, 30 fps, 약 3.23초
- PNG 시트: 3456 x 3520 px, 가로 9칸 x 세로 11칸. 마지막 2칸은 미사용
- 읽는 순서: 왼쪽 위에서 오른쪽으로, 이어서 다음 줄
- 피벗: 폭발 중심 (176, 160), Unity 정규화 좌표 (0.4583333, 0.5)
- Pixels Per Unit=100, Point 필터, 압축 없음, 밉맵 없음
- Max Texture Size=4096. 2048로 줄이면 픽셀이 축소될 수 있어
- 마지막 프레임은 완전 투명, 비반복 재생

Unity
Assets/02.Sprites/DLJ/Effects/GreenInferno_DLJ에 PNG, 분할 설정과 .anim을 넣어 뒀어.
같은 오브젝트의 SpriteRenderer와 Animator로 GreenInferno_DLJ.anim을 재생하면 돼.
오브젝트 위치는 폭발할 지점에 맞추면 돼. 접근과 확대, 폭발이 프레임 안에 포함돼 있어.
flipX=false면 오른쪽에서 다가오고, flipX=true면 왼쪽에서 다가와.
다른 프로젝트로 옮길 때는 PNG, PNG.meta, anim, anim.meta를 함께 복사해.
실제 피해 판정과 스킬 발동 코드는 이 파일에 연결하지 않았어.

추가 파일
- GreenInferno_DLJ.json: Aseprite 스타일 프레임 좌표와 메타데이터
- GreenInferno_DLJ.aseprite: 투명 배경 97프레임 편집용 원본
- GreenInferno_DLJ_preview.gif: 어두운 배경 위 2배 확대 미리보기. PNG에는 배경이 포함되지 않아
- dlj-green-inferno.js: pixelfx/src/fx/arcane에 넣어 렌더링할 수 있는 효과 소스
- InfernoVerification_DLJ.json: 접근 방향, 확대, 초록색과 폭발 크기 검증 기록

검증
pixelfx 검사: 1 pass, 0 fail, 0 warnings.
같은 시드의 결과 일치, 접근하면서 커지는 불덩이, 초록색 폭발과 마지막 투명 프레임을 확인했어.
0.8초 불덩이 대비 폭발 최고점의 불투명 면적은 약 3.5배였어.
Unity 6000.3.20f1에서 97개 스프라이트와 애니메이션 97프레임 샘플을 검증했어.

출처
pixelfx의 화염 그리기 도구를 사용해 DLJ 원형 불덩이와 폭발 연출을 만들었어.
https://github.com/patch-gg/pixelfx
화염 도구의 원래 레시피: Hugo Duprez / Sprite Fusion
https://www.spritefusion.com/pixel-art-vfx/fire
https://www.spritefusion.com/pixel-art-vfx/explosion
Copyright (c) 2026 Patch
Copyright (c) 2026 Hugo Duprez
MIT 라이선스 전문은 LICENSE_pixelfx.txt에 있어.