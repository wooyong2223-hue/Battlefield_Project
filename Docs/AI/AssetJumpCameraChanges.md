# 에셋 기준 점프 카메라 수정 (2026-10-30)

사용자 증상: 점프 카메라 무빙이 이상함. 요청: 점프 카메라 이동을 캐릭터 에셋 움직임 기준으로 변경.

## 비교와 원인

직전 JumpBodyAim applied.json의7개 파일 및 격리본 Camera/AimView/MotionState/FPArms/JumpPoseState/테스트/Scene/controller가 동일했다. 이전 작업 후 사용자/Editor 변경 없음. 기준 해시는 Library/CodexAssetJumpCamera/baseline.json에 기록, 원본 반영 직전 재검사했고 동일했다.

변경 전 실제 점프 검사에서 추가 회전48.087도로 실패(before.xml). 기존 코드는 UpperChest 원본 회전을 `JumpBodyRotation * LookRotation`으로 카메라에 그대로 적용해 착지 숙임이 화면 회전이 됐다. 속도 기반 JumpCameraOffset/CameraPitch/landing spring도 에셋 몸통 변화와 중첩됐다.

캡처에서 발견한 두 번째 원인: Scene의 FirstPersonArmsView._eye가 실제 카메라 Transform(fileID330585546)을 가리킨다. 기존 `ApplyJumpBody(_eye.position)`은 카메라가 이미 받은 점프 이동을 팔에 다시 더해 착지 때 총이 화면 아래로 내려갔다. 참조/계층은 바꾸지 않고 이 중복 계산을 제거한다.

## 모든 변경 파일과 교체 내용

1. Assets/Features/PlayerCharacter/Scripts/PlayerCharacterWeaponAimView.cs
   - 이전 SampleJumpBody 입력 `_meleeChest.position, _meleeChest.rotation` → `_locomotionHead.position, _combat.transform.rotation`.
   - 점프 클립의 머리 위치 차분은 사용하되, 시선에는 애니메이션 몸통·머리 회전을 더하지 않는다. 마우스 입력의 중복을 막는 기존 look-at 이전 샘플 시점은 유지한다. 사용자 손 좌표, 3인칭 그립/1인칭 팔 IK 및 장전·근접 예외는 유지한다.
2. Assets/Features/PlayerCharacter/Scripts/PlayerCharacterCamera.cs
   - `ResolvedAimRotation = JumpBodyRotation * _view.AimRotation` → `_view.AimRotation`.
   - `rotation = JumpBodyRotation * _view.LookRotation` → `_view.LookRotation`.
   - JumpAnimationOffset 읽기 전용 getter 추가(테스트/진단용 실제 에셋 이동량).
   - JumpOffset과 CameraPitch의 이전 전체 적용 → 기존 계산에 `(1 - JumpBodyBlend)`를 곱해 에셋 점프가 담당하는 구간에는 계산식 점프 반동을 제거. 기존 spring 계산/프로필/직렬화 값은 유지한다.
   - 새 LocomotionOffset은 기존 CameraOffset에서 `LandingCameraOffset * JumpBodyBlend`만 빼서 에셋 착지와 landing spring의 이중 적용을 막는다. 일반 보행 bob/roll은 유지한다.
   - VisualEyePosition의 기존 inline CameraOffset 대신 LocomotionOffset 사용. 기존 ApplyJumpBody API/JumpPoseState는 재사용하며, 새 입력에서는 회전이 identity라 실제로 에셋 머리 이동만 전달된다. 3인칭 pivot과 FP 팔에도 일관된 위치 변화가 적용된다.
3. Assets/Features/PlayerCharacter/Scripts/PlayerCharacterMotionState.cs
   - 읽기 전용 LandingCameraOffset 추가; 기존 `_landing * stabilization`을 별도 값으로 제공하고 Reset 시 zero.
   - 기존 `CameraOffset = (side * amplitude.x, down * amplitude.y + _landing * stabilization, 0)` → `(side * amplitude.x, down * amplitude.y, 0) + LandingCameraOffset`.
   - 기존 최종 CameraOffset과 spring/속도/프로필 동작은 동일하며 카메라 소비자가 landing 성분만 대체할 수 있게 했다.
4. Assets/Features/PlayerCharacter/Scripts/FirstPersonArmsView.cs
   - 이전 `pivot = (_motionSource != null ? _motionSource.ApplyJumpBody(_eye.position) : _eye.position) + aim * offset` → `_eye.position + aim * offset`.
   - 실제 카메라를 참조하는 eye 위치에는 점프 이동이 이미 포함돼 있으므로 한 번만 사용한다. 기존 팔 복사/그립/마우스 조준·반동/장전·근접 모션 유지.
5. Assets/Tests/PlayMode/PlayerCharacterJumpBodyAimTests.cs
   - 이전 실제 점프에서 몸통 회전>1도를 요구하던 검사를 추가 회전<.05도로 교체.
   - 실제 에셋 머리 이동>.02m 확인, 에셋 점프 Blend1일 때 카메라 시선과 마우스 LookRotation 차이<.1도 확인.
   - 양 시점×점프 중 마우스 변경 유무4조건, 물리 공중 진입/레이 방향/총 방향/착지 복귀 검사는 유지. 로그는 에셋 머리 이동량과 추가 회전으로 변경.

문서: Docs/AI/AssetJumpCameraChanges.md 신규, Docs/AI/UnityProjectContext.md 기록 추가. 삭제한 메서드·필드·컴포넌트·에셋 없음. 위 계산식과 테스트 기대값을 교체했다. JumpPoseState/Scene/controller/애니메이션/meta/카메라 Inspector 위치·FOV·거리/모델 크기·배치/점프높이1.2/이동/프로필/오디오 값 변경 없음. 새 애니메이션 제작·수정·다운로드 없음; 기존 Starter Assets 점프 클립의 실제 리타깃 머리 이동을 샘플링한다.

## 구조와 검증

새 매니저/컴포넌트 없이 기존 점프 상태 객체를 재사용한다. AimView는 애니메이션 샘플 제공, Camera는 시점 적용, MotionState는 기존 모션 성분 계산, FPArms는 카메라 기준 팔 복사를 맡는다. LandingCameraOffset은 읽기 전용이며 내부 spring state를 외부에서 변경하지 못한다. OOP/SOLID 의도적 예외 없음.

격리 Unity6000.4.8f1 -batchmode -noaudio 사용. 중간본55/55 통과(after.xml). 최종56/56 통과(final.xml/log): 제품 검사55개+실제 화면 캡처1개, 실패/건너뜀0. 기존 조준·손·장전·모션·카메라·애니메이션 및 더미 팀/재질 사격장 회귀 포함. 양 시점110프레임50fps 캡처 및 직접 화면 확인; 최종 FP 착지 화면에서 총이 화면에 유지되는 것도 확인. 검증본과 원본 반영5개 파일 바이트 일치. jump-camera-comparison.mp4는 이전48도 몸통 추적(왼쪽)/수정된 에셋 머리 이동(오른쪽)의1인칭 비교. 카메라 렌더 캡처이므로 Overlay Canvas는 포함하지 않는다. 변경 전48.087도 → 수정 후 추가 회전0도, 현재 높이4m 캐릭터에서 에셋 로컬 머리 변위 최대 약2.03m(물리 루트 점프 이동은 차분에 포함하지 않음). 머리 회전을 따라 시선이 꺾이던 문제를 제거한 것이며 에셋 자체의 숙임 이동은 그대로 사용한다.

범위는 관련 Play Mode 회귀이며 전체 프로젝트/플랫폼 빌드 검증은 아니다. 원본 Editor의 실제 조작감과 손가락/총 배치의 주관적 품질 확인은 남는다. 임시 캡처 코드·PNG·영상은 Library에만 존재한다. 테스트용 영구 음소거 설정/AudioSource 변경 없음.
