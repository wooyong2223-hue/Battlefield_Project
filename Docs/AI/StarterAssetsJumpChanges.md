# Starter Assets 점프 모션 적용

2026-10-29. 사용자 요청: 캐릭터 모델을 가져온 Unity Starter Assets의 점프 모션 사용. 앞선 Quaternius 우선 정책 중 점프3종만 이번 명시 요청에 따라 변경.

## 작업 전 비교

컨트롤러/Scene/이동코드/카메라모션프로필/Starter 점프FBX 메타는 직전검증본과동일. baseline.json에 해시기록. 기존값/계층/원본이벤트설정을보존한다. 새기능스크립트없이 기존Animator상태의Motion데이터만교체하는구조를선택했다. OOP/SOLID 의도적예외없음.

## 컨트롤러에서 교체한 이전 참조

| 상태 | 이전 | 변경 후 | 재생속도 |
|---|---|---|---|
| Jump | Quaternius JumpStart,1.333333s | StarterJumpStart,.4s | 기존1 유지 |
| InAir | Quaternius JumpAir,2.5s | StarterInAir,2.666667s,원래루프설정유지 | 기존1 유지 |
| Land | Quaternius JumpLand,1.266667s | StarterJumpLand,.6333333s | 2.0000002→1 |

Land는 이전1.266667/2≈.633333초와새.633333/1을맞춰실효재생시간을유지했다. 기존상태fileID/전환조건/exitTime/transitionDuration/기타상태/기존장전레이어등컨트롤러의다른문서는그대로다. 정적비교상기존문서수동일,Jump/InAir/Land3문서만다르다. 기존Quaternius클립에셋은삭제하지않았다.

## 가져온 모션과 이벤트 처리

모델과같은제품196526(Unity Companion License)의 기존원본:
- Assets/ThirdParty/UnityStarterAssets/Animations/Jump--Jump.anim.fbx: JumpStart 및 JumpLand
- Assets/ThirdParty/UnityStarterAssets/Animations/Jump--InAir.anim.fbx: InAir

원본 FBX/메타/Avatar와 이벤트를 수정하지 않고 importedAnimationClip을Instantiate해 Features/PlayerCharacter/Animation/StarterAssetsJump 아래 연결용.anim3개로저장했다. 키프레임/리타깃/길이/루프/루트설정은그대로복사. JumpLand 원본의OnLand는미포함된상류Starter컨트롤러사운드함수를요청하므로 새복사본에서만 해당이벤트를제외했다(기존OnLand수신기/사운드신규구현없음). 원본OnLand유지는자동검사로확인. 이벤트외모션커브직접편집/새모션제작없음. 기존라이선스 Assets/ThirdParty/UnityStarterAssets/license.txt 및 SOURCE.md 유지.

## 자동검사 변경

PlayerCharacterAnimationTests.cs에 JumpStatesUseStarterAssetsMotionsAndKeepLandingDuration 추가. Editor/AnimatorController namespace import,3상태의실제클립명·HumanMotion·원본길이·루프일치·새클립OnLand없음/원본OnLand존재·상태속도1·착지실효.633333초확인. 기존 실제점프상태전환/모델보존등검사는변경하지않았다.

## 보존과 검증

점프높이1.2/물리이동·속도·중력/RootMotionfalse/모델Scale·배치/카메라위치·관성·착지반응/Scene/일반이동·전투·장전설정은유지. runtimeC#수정없음. 격리Unity6000.4.8f1 -noaudio 집중22/22통과(PlayerCharacterAnimation/Motion/Movement) + 실점프 전후캡처1/1통과. 물리1.2m 상승/착지,Jump→InAir→Land 전환과루트모델고정,1/3인칭카메라관성검사포함. 두실행의전체회귀/타겟빌드검사는재실행하지않았다.

기존원본컨트롤러의격리복사본을이전조건으로사용해같은캐릭터와물리에서110프레임×2(50fps)렌더링. 비교영상 jump-comparison.mp4 왼쪽Quaternius/오른쪽StarterAssets,직접화면확인. 사용자Editor실제조작감/일반이동으로돌아오는착지체감은Play재시작후확인필요. 원본사용자Editor조작없음. 임시Editor생성기/이전컨트롤러복사/캡처코드는Library에만존재.

## 전체 변경/추가 파일 (meta 포함)

- `Assets/Features/PlayerCharacter/Animation/PlayerCharacter.controller`
- `Assets/Tests/PlayMode/PlayerCharacterAnimationTests.cs`
- `Assets/Features/PlayerCharacter/Animation/StarterAssetsJump/StarterInAir.anim`
- `Assets/Features/PlayerCharacter/Animation/StarterAssetsJump/StarterInAir.anim.meta`
- `Assets/Features/PlayerCharacter/Animation/StarterAssetsJump/StarterJumpLand.anim`
- `Assets/Features/PlayerCharacter/Animation/StarterAssetsJump/StarterJumpLand.anim.meta`
- `Assets/Features/PlayerCharacter/Animation/StarterAssetsJump/StarterJumpStart.anim`
- `Assets/Features/PlayerCharacter/Animation/StarterAssetsJump/StarterJumpStart.anim.meta`
- `Assets/Features/PlayerCharacter/Animation/StarterAssetsJump.meta`

문서: Docs/AI/StarterAssetsJumpChanges.md 신규, Docs/AI/UnityProjectContext.md 기록추가. 로그 Library/CodexStarterJump/create.log,focused.xml/log,capture.xml/log.
