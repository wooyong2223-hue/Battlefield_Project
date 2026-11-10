# Starter Assets 원본 방식의 점프 카메라 (2026-10-30)

사용자 요청: 머리 추적을 사용하지 말고 캐릭터 에셋을 가져온 곳의 점프 카메라 기능을 가져올 것.

## 원본 확인

현재 설치된 Unity Starter Assets 2.0.2의 ThirdPersonController/FirstPersonController CameraRotation과 JumpAndGravity, PlayerFollowCamera 및 PlayerCameraRoot 프리팹을 직접 읽었다. 별도 점프 카메라 클립/본 추적 코드는 없다. PlayerCameraRoot는 캐릭터 루트 아래 고정 위치에 있으며 카메라는 CinemachineThirdPersonFollow로 추적한다. 3인칭 프리팹 Damping=(.1,.25,.3), 1인칭=(0,0,0), 시선은 마우스 입력으로 제어한다. 원본에 들어 있는 별도 Perlin 잡음은 상시 노이즈이며 점프 기능이 아니므로 이번 변경에서 가져오지 않는다.

적용 범위는 원본의 루트 기반 수직 추적과 수직 감쇠값이다. 원본의 거리4/FOV40/고정 타깃높이1.375/좌우·전후 감쇠/충돌/전체 프리팹을 그대로 대체하지 않으며, 사용자 현재 구도·FOV·충돌·자유 시점·이동·점프 물리는 유지한다. Cinemachine3.1.7 Runtime/Core/Predictor.cs의 공개 Damper.Damp를 직접 호출한다. ThirdPersonFollow의 이전 타깃 위치 차분 누적 후 감쇠를 빼는 방식과 동일한 수직 처리를 사용한다. 새 임의 점프 파형/키프레임 제작 없음.

## 변경 전 비교

직전 AssetJumpCamera의 applied.json5개 파일 동일. Camera/AimView/JumpPoseState/Features asmdef/FPArms/MotionState/관련 테스트/Scene/controller도 직전 검증본과 바이트 동일. baseline.json에10개 파일 해시 기록 후 원본 반영 직전 다시 확인했고 동일했다. 원본 프로젝트 Cinemachine3.1.7은 이미 설치돼 있으며 Packages 파일 변경 없음.

## 모든 제품 파일 변경

1. Assets/Features/PlayerCharacter/Scripts/PlayerCharacterJumpPoseState.cs
   - 이전 머리 포즈 Sample/TransformPoint 구현을 루트 추적 Tick(rootPosition,firstPerson,active,deltaTime)으로 교체.
   - 이전 _referencePosition/_referenceRotation/_hasReference 및 Blend/Pivot/Rotation 제거. 새 _previousRoot/_correction/_initialized/_firstPerson으로 루트의 수직 추적 상태만 소유한다.
   - `_correction += previousRoot.y - rootPosition.y; _correction -= Damper.Damp(_correction, firstPerson ? 0 : .25, deltaTime)`.
   - Offset은 Vector3.up * correction. 1인칭/3인칭 전환과 비활성 시 Reset, 최초 위치에서 오프셋0으로 초기화해 이전 위치가 섞이지 않게 한다. 기존 파일·meta GUID는 유지.
2. Assets/Features/PlayerCharacter/Scripts/PlayerCharacterCamera.cs
   - _jumpSampleFrame, JumpBodyRotation, JumpBodyBlend, JumpAnimationOffset, ApplyJumpBody, SampleJumpBody, JumpOffset 제거. 애니메이션 샘플/본 변환을 카메라에서 제거하기 위해 더 이상 필요하지 않다.
   - JumpFollowOffset 읽기 전용 속성과 LateUpdate의 root-follow Tick 추가. 카메라가 원래 보유한 _target/_movement/_actions로 계산하므로 Animator/무기에서 샘플을 받을 필요가 없다.
   - VisualEyePosition/TP pivot/GetAimRay pivot은 기존 고정 루트 눈높이 위치+JumpFollowOffset을 사용한다.
   - LocomotionOffset의 `CameraOffset - LandingCameraOffset * JumpBodyBlend`를 `CameraOffset - LandingCameraOffset`으로 교체. 기존 계산식 착지 반동은 카메라에 더하지 않는다. CameraPitch 적용과 속도 기반 JumpOffset도 제거한다.
   - 시선/AimRotation/자유 시점 분리/기존 카메라 위치·FOV·거리·충돌/일반 보행 bob·roll 유지. 원본 루트 추적은 점프에만 트리거되는 이벤트가 아니라 루트의 수직 이동을 따라가는 방식이므로 계단/경사에서도 동일한 수직 감쇠가 적용된다.
3. Assets/Features/PlayerCharacter/Scripts/PlayerCharacterWeaponAimView.cs
   - 이전 점프 샘플용 _movement 필드와 Awake 캐시, 기본 레이어의 Jump/InAir/Land 감지 및 SampleJumpBody 호출 블록 제거.
   - `ApplyGrip(ApplyJumpBody(rightPosition), JumpBodyRotation * rightRotation,reachScale)` → `ApplyGrip(rightPosition,rightRotation,reachScale)`.
   - 카메라용 본 추적 책임이 제거됐다. 기존 실제 몸통·머리 조준/양손 그립/1인칭 팔 IK/장전·근접 예외는 유지.
4. Assets/Features/Battlefield.Features.asmdef
   - references에 Unity.Cinemachine 하나 추가. 기존 참조/플랫폼/define 설정 유지. 원본 라이브러리 함수를 호출하기 위한 명시적 런타임 의존이며 새 패키지 설치/원본 패키지 변경은 없다.
5. Assets/Tests/PlayMode/PlayerCharacterJumpBodyAimTests.cs
   - BodyDeltaFollowsAnimationWithoutDuplicatingRootMovementAndResets를 StarterRootFollowUsesCinemachineDampingAndResetsAtPerspectiveChanges로 교체. .25초에1% 잔여, 반복 감쇠, FPS 지연0, 시점 전환/비활성 초기화 검사.
   - ActualJumpMovesCameraAimAndHandsTogetherInBothViews를 ActualJumpUsesRootTrackingWithoutAnimationBoneMotionInBothViews로 교체. 머리 변위/혼합/몸통 회전 검사 대신 루트 추적 보정값과 시선 안정성을 검사.
   - 실제 점프 양 시점×공중 마우스 입력 유무4조건. TPS 보정> .01m/FPS 보정0, 착지 후<.001m, 카메라 마우스 방향 차이<.1도, FPS 눈높이에서 본 추적에 의한 큰 이동이 없는지<.2m, 기존 레이·총 방향 유지 확인. 파일/meta GUID는 유지.
6. Assets/Tests/PlayMode/PlayerCharacterMotionTests.cs
   - JumpCameraRespondsInBothViewsWithoutChangingAimOrMovementSettings에서 이전 `maxPitch > .1` 기대를 `maxPitch < .1`로 교체. 원본 방식에는 점프 카메라 피치가 없기 때문이다. 나머지 이동속도/FOV/공중진입/복귀 검사 유지.

문서: Docs/AI/StarterRootJumpCameraChanges.md 신규, Docs/AI/UnityProjectContext.md 기록 추가. 제거·교체한 구현은 위 목록이며 기존 Scene 컴포넌트/에셋/직렬화 필드·값/애니메이션/meta 삭제 없음. FirstPersonArmsView의 이전 중복 적용 수정은 보존한다. MotionState/프로필의 기존 점프 값과 수학은 데이터 보존을 위해 유지하지만 새 카메라는 그 점프·착지 출력을 소비하지 않는다.

## 구조와 원본 근거

작은 plain 상태 객체가 루트 추적 이력/감쇠 상태를 소유하고, Camera가 위치에 적용한다. 무기와 애니메이션 컴포넌트가 카메라 추적을 제어하던 의존을 제거했다. 새 매니저/컴포넌트/인터페이스 없이 기존 책임에 맞게 교체했으며 OOP/SOLID 의도적 예외 없음. Cinemachine 함수에 대한 명시적 라이브러리 의존은 원본 기능을 실제 사용하는 목적에 필요하다.

원본 파일(수정 없음):
- Assets/Starter Assets/Runtime/ThirdPersonController/Scripts/ThirdPersonController.cs
- Assets/Starter Assets/Runtime/FirstPersonController/Scripts/FirstPersonController.cs
- 각 Runtime/.../Prefabs/PlayerFollowCamera.prefab와 PlayerCapsule/PlayerArmature.prefab
- Library/PackageCache/com.unity.cinemachine@f3f96bcb59af/Runtime/Components/CinemachineThirdPersonFollow.cs (수직 추적 차분/감쇠)
- 동일 패키지 Runtime/Core/Predictor.cs (Damper)

## 검증

격리 Unity6000.4.8f1 -batchmode -noaudio 집중55/55 통과(focused2.xml/log), 실패/건너뜀0. 그립/권총 포즈/전투 표현/장전 어깨/카메라/모션/점프 애니메이션 및 allied dummy 팀·재질 사격장 회귀 포함. 최초 격리 환경에는 Cinemachine이 빠져 컴파일 실패(focused.log); 원본에 이미 설치된 Cinemachine3.1.7/Splines/Mathematics/Settings/Burst를 해당 원본 캐시의 file 참조로 격리 Packages에만 연결해 해결했다. 원본 Packages는 변경하지 않았다. 격리 패키지의 HDRP 샘플 asmref 경고는 본 프로젝트에 HDRP가 없는 패키지 샘플 경고이며 제품 스크립트 컴파일 오류 없이 검사 완료.

최종 캡처1/1 통과(capture.xml/log). 양 시점110프레임50fps 직접 렌더링 및 화면 확인, TPS 최대 추적 보정.185m/FPS0m. 비교영상 starter-root-comparison.mp4는 왼쪽 이전 머리 추적/오른쪽 Starter Assets 루트 수직 추적. Overlay Canvas는 캡처에 포함하지 않는다. 원본 반영6개 파일과 검증본 SHA256 일치 확인. 전체 프로젝트 테스트/배포 플랫폼 빌드는 실행하지 않으며 사용자 Editor의 실제 조작감·가까운 장애물 시야 확인은 남는다. 임시 이전 본 추적 캡처 코드2개는 새 API와 호환되지 않아 격리 Assets에서 Library/CodexStarterRootCamera/Archived로 보관 이동했다. 제품 파일 삭제가 아니며 새 캡처 코드도 Library 전용이다. 영구 음소거/AudioSource 변경 없음.
