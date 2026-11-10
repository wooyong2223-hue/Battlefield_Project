# 점프 몸통을 따르는 조준·카메라·손 (2026-10-29)

사용자 요청: 점프할 때 손과 에임이 몸통을 따라가고, 조준점·카메라 시선도 몸통 움직임을 따라갈 것. 마우스 입력은 계속 동작한다.

## 작업 전 비교와 구조

직전 손 동작 작업의 applied.json 6개 파일 모두 동일. Camera/FirstPersonArmsView/Scene/AnimatorController도 직전 검증 프로젝트와 바이트 일치. Library/CodexJumpBodyAim/baseline.json에 관련 해시 저장 후, 원본 반영 직전 재비교해 사용자/Editor 변경 없음 확인. Scene/프리팹/모델 배치/카메라 위치·FOV/물리 점프높이1.2/애니메이션 참조/오디오 설정 변경 없음.

점프 몸통 기준 자세·혼합·변환은 독립 plain C# 상태 객체 PlayerCharacterJumpPoseState가 담당한다. Camera가 상태를 소유하고 기존 WeaponAimView가 Animator의 기본 레이어 IK 콜백에서 원본 몸통을 샘플링해 제공한다. 카메라가 무기 컴포넌트를 검색하는 역방향 의존이나 새 Scene 컴포넌트는 추가하지 않았다. 카메라/손/조준 레이는 같은 몸통 변환을 소비한다.

## 변경 파일과 정확한 교체점

1. `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterJumpPoseState.cs` 및 `.meta` 신규.
   - 마지막 지상 몸통 위치·회전을 캐릭터 루트 좌표로 저장한다. Jump/InAir/Land 및 공중 동안 원본 몸통과 기준 자세 차이를 계산한다.
   - Blend는 `MoveTowards(Blend, jumping ? 1 : 0, deltaTime * 10)`으로 진입/복귀하며, 새 내부 전환 상수이고 기존 Inspector 값을 덮어쓰지 않는다.
   - Rotation은 `rootRotation * Slerp(identity, currentLocalBodyRotation * inverse(referenceLocalBodyRotation), Blend) * inverse(rootRotation)`.
   - 위치는 `Pivot + Offset + Rotation * (worldPoint - Pivot)`. 루트 이동 자체는 차분에서 제외해 물리 점프 높이를 다시 더하지 않는다.
   - 비활성/미샘플 프레임/컴포넌트 중지 시 Reset. 읽기 전용 값과 Sample/TransformPoint/Reset 계약으로 상태 소유를 제한한다.

2. `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterCamera.cs` 수정.
   - `_jumpPose`와 `_jumpSampleFrame`, SampleJumpBody, JumpBodyRotation/Blend, ResolvedAimRotation, ApplyJumpBody 추가.
   - 이전 `rotation = _view.LookRotation` → `JumpBodyRotation * _view.LookRotation`.
   - 이전 VisualEyePosition 및 TP pivot → 기존 계산 결과에 ApplyJumpBody 적용. 기존 충돌 검사/복귀 거리 보간/카메라 오프셋/FOV는 그대로 사용한다.
   - 이전 GetAimRay의 `_view.AimRotation` → `ResolvedAimRotation = JumpBodyRotation * _view.AimRotation`; 시작 위치에도 같은 몸통 변환을 적용한다. 자유 시점은 기존대로 LookRotation과 AimRotation을 분리한다.
   - 이전 OnEnable/OnDisable의 모션 초기화에 점프 상태 초기화를 추가하고, 해당 프레임 몸통 샘플이 없으면 누적 변환을 제거한다.
   - 기존 모션 프로필의 CameraPitch/CameraRoll/JumpOffset/WeaponEuler 추가 표현은 종전대로 유지한다. 모든 기존 카메라 표현 회전을 발사 방향에 통합하는 변경은 아니며 새 몸통 변환만 공통 적용한다.

3. `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterWeaponAimView.cs` 수정.
   - `_movement` 캐시 추가. 기존 `_aimRotation = _view.AimRotation` 전에, 기본 레이어 OnAnimatorIK에서 몸통 위치/회전을 카메라 상태에 제공한다.
   - 샘플은 기존 ApplyUpperBodyAim/LookAt 처리 전이므로 마우스 입력에 따른 몸통 IK가 다시 입력에 누적되는 순환을 방지한다.
   - 기존 `ApplyGrip(rightPosition, rightRotation, reachScale)` → `ApplyGrip(_camera.ApplyJumpBody(rightPosition), _camera.JumpBodyRotation * rightRotation, reachScale)`.
   - 카메라 기준으로만 고정되던 TP 목표를 점프 몸통 움직임에 함께 변환한다. 기존 도달 범위/그립 좌표/장전·근접 해제/FP 팔 IK는 유지한다.

4. `Assets/Features/PlayerCharacter/Scripts/FirstPersonArmsView.cs` 수정.
   - 기존 `aim = _view.AimRotation` → 모션 소스가 있으면 `ResolvedAimRotation` 사용, 없으면 기존 입력 유지.
   - 기존 `pivot = _eye.position + aim * offset` → 몸통 변환을 적용한 eye 위치 + aim * offset.
   - 기존 torso 기준 팔 복사 정규화는 보존한다. 그 위에 점프 변환을 공통 적용해 원래 복사 과정에서 상쇄되던 몸통 움직임을 시선과 함께 전달한다. 새 애니메이션 제작/키프레임 편집 없음.

5. `Assets/Tests/PlayMode/PlayerCharacterJumpBodyAimTests.cs` 및 `.meta` 신규.
   - 기준 몸통에서 위치·회전 차분, 루트 점프 이동 중복 미반영, 착지/비활성 초기화 검사.
   - 실제 물리 점프를 양 시점 × 마우스 입력 변경 유무4조건에서 실행. 공중 진입/혼합1/몸통 변화 반영/카메라-레이 차이3도 미만(기존 추가 카메라 표현 포함)/몸통 변환을 공유하는 총 방향2도 미만/착지 복귀 검사.
   - 조준 레이와 ResolvedAimRotation의 벡터 차이 < .00001, 입력 누적 없이 몸통 회전90도 미만 확인. 기존 손 그립 오차 검사도 회귀로 실행한다.
   - 고정 .02초 테스트 시간을 복원하고 테스트 바닥을 제거한다.

문서 `Docs/AI/JumpBodyAimChanges.md` 신규, `Docs/AI/UnityProjectContext.md` 기록 추가. 삭제한 메서드·필드·컴포넌트·참조·에셋 없음. 교체한 것은 위 위치/회전 계산식이며 이전 손 동작·장전·점프 클립은 유지한다.

## 검증과 한계

격리 Unity6000.4.8f1 `-batchmode -noaudio` 최종56/56 통과, 실패/건너뜀0. 구성은 제품 검사55 + Library 전용 영상 캡처1. 기존 손 동작/권총 포즈/전투 표현/장전 어깨/카메라/모션/애니메이션 및 재질·팀 더미 사격장 포함. 최초 새 검사는 같은 방향 벡터의 Vector3.Angle 반올림값 .028도 때문에 .01도 비교가 실패해, 동등 방향의 벡터 거리 비교로 수정했다(실제 회전 허용폭을 늘린 수정 아님).

양 시점110프레임씩50fps 렌더링 및 화면 확인, 점프/착지 중 총·손·시선 동조 확인. 현재 원본 모션 몸통 회전 최대 약43.8~48.1도가 전달된다. 사용자가 원한 몸통 전체 움직임을 반영하며 별도 각도 축소/클램프는 넣지 않았다. 점프 높이/루트 이동/시야각은 변경하지 않았다. 캡처는 Camera.Render라 Overlay Canvas 조준점 자체는 포함하지 않는다. 기존 중앙 조준점 UI 배치는 변경하지 않고 카메라 시선이 향하는 월드 방향이 이동한다.

원본 반영 파일과 검증 파일 SHA256 일치. 증거 Library/CodexJumpBodyAim/final.xml/log, TP·FP PNG 각110장, jump-body-follow.mp4(TP 왼쪽/FP 오른쪽). 원본 Editor 조작/지속 오디오 설정 변경 없음. 사용자 Editor의 실제 체감·오브젝트 근처 시야와 배포 플랫폼 빌드 확인은 남음. 전체 프로젝트 테스트를 실행한 것은 아님.

## 설계 검토

차분/혼합 상태를 독립 객체로 분리하고 카메라는 시점 적용, 기존 AimView는 애니메이션 샘플/손 목표, FP View는 팔 복사 책임을 유지한다. 상속·광범위 인터페이스·별도 매니저를 추가하지 않았다. 기존 명시 의존 Camera를 통해 변환을 공유하며 상태 외부 직접 변경을 허용하지 않는다. SOLID/OOP 의도적 예외 없음.
