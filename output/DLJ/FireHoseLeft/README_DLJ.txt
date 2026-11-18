FireHoseLeft_DLJ — 소방호스 느낌의 강한 흰색 물줄기

굵고 조밀한 물기둥이 오른쪽에서 왼쪽으로 빠르게 뻗고,
분사 지점과 연결된 상태로 수압을 유지하다가 끝에서 거친 물보라로 찢어져.
기존 WhiteWaterLeft보다 중심이 굵고 발사 전개가 빠른 강화 버전이야.

- 프레임: 320 x 144 px, 37프레임, 30 fps, 약 1.23초
- PNG 시트: 1600 x 1152 px, 가로 5칸 x 세로 8칸. 마지막 3칸은 미사용
- 프레임 순서: 왼쪽 위부터 오른쪽으로, 이어서 다음 줄
- 발사 지점 피벗: (284, 72), 정규화 (0.8875, 0.5)
- Point 필터, 압축 없음, 밉맵 없음, Pixels Per Unit=100
- 마지막 프레임 완전 투명, 비반복 재생

Unity 사용
Assets/02.Sprites/DLJ/Effects/FireHoseLeft_DLJ에 분할된 PNG와 .anim을 넣어 뒀어.
SpriteRenderer와 Animator가 같은 오브젝트에 있는 상태에서 FireHoseLeft_DLJ.anim을 재생하면 돼.
flipX=false일 때 왼쪽으로 분사돼. 위치는 물이 나오는 지점에 맞추면 돼.
왼쪽으로 전개되는 움직임은 이미지에 이미 포함돼 있어.
다른 프로젝트에 옮길 때는 PNG, PNG.meta, anim, anim.meta를 함께 복사해.

추가 파일
- FireHoseLeft_DLJ.json: 프레임 좌표와 타이밍
- FireHoseLeft_DLJ.aseprite: 투명 배경 37프레임 편집용 원본
- FireHoseLeft_DLJ_preview.gif: 어두운 배경 위 2배 확대 미리보기. 실제 시트는 투명 배경이야
- dlj-fire-hose-left.js: pixelfx/src/fx/water에 넣어 렌더링할 수 있는 소스
- PressureVerification_DLJ.json: 기존 효과와의 중심 두께, 초기 전개 속도, 분사 유지 비교 기록

검증
pixelfx 검사: 1 pass, 0 fail, 0 warnings.
같은 시드의 결과 일치, 왼쪽 전개, 분사 유지, 마지막 투명 프레임을 확인했어.
같은 시간과 발사 지점 근처 구간에서 중심 불투명 두께는 기존의 약 2.3배,
첫 0.1초 전개 거리는 약 2.7배로 측정됐어. 물리 시뮬레이션 수압 수치가 아닌 시각적 비교야.
Unity 6000.3.20f1에서 37개 스프라이트와 애니메이션 37프레임 샘플을 검증했어.

출처
pixelfx water/water-jet을 바탕으로 DLJ 분사 효과를 강화했어.
https://github.com/patch-gg/pixelfx
원래 빔 레시피: Hugo Duprez / Sprite Fusion
https://www.spritefusion.com/pixel-art-vfx/energy
Copyright (c) 2026 Patch
Copyright (c) 2026 Hugo Duprez
MIT 라이선스 전문은 LICENSE_pixelfx.txt에 있어.