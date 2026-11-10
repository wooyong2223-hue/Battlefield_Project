# 캐릭터 1인칭·3인칭 시점 구현

2026-10-27 작업. 기존 캐릭터 이동과 Armature 애니메이션에 배그식 시점 조작을 연결했다. 기존 위치·입력·카메라 설정 교체와 시작 위치 이동은 사용자 승인을 받은 뒤 적용했다.

## 조작과 동작

| 입력 | 동작 |
|---|---|
| V | 선호 시점을 3인칭 ↔ 1인칭으로 전환. ADS 상태에서는 ADS를 해제하고 전환한다. |
| 마우스 | 우클릭 없이 몸의 좌우 방향과 카메라의 상하 시선을 조작한다. |
| Alt + 마우스 | 몸·이동 기준·발사 방향을 유지하며 주변을 본다. 3인칭 좌우 180도, 1인칭 좌우 90도. 놓으면 원래 시선으로 복귀한다. |
| 우클릭 유지 | 견착 조준. 3인칭은 더 가까운 어깨 카메라, 시야각 50. |
| 우클릭 짧게 누르기 | 0.2초 이내 클릭으로 ADS 전환/해제. 눈 위치와 시야각 40을 사용하고, 해제하면 선호 시점으로 복귀한다. |
| Esc | 마우스 잠금과 게임 입력을 해제한다. |
| 좌클릭 | 잠금 해제 상태에서는 조작 복귀. 이 클릭은 발사하지 않고, 버튼을 놓은 뒤 다음 클릭부터 발사한다. 잠금 상태에서는 기존 발사 입력이다. |
| WASD / Shift / Space | 기존 이동 / 달리기 / 점프를 유지한다. |

배그의 PC 기본 조작을 참고했다: https://support.pubg.com/hc/en-us/articles/360002074913-What-are-the-basic-game-commands

## 변경 전 비교와 보존

이전 시점 상태 점검 이후 관련 스크립트, 입력 액션과 CombatTestScene은 변경되지 않았음을 기존 검증본 및 해시와 비교했다. 이전 작업의 기체 회전·사운드·피격 효과 변경, Armature 프리팹·Animator·원본 모델/클립, 폰트 아틀라스 변경은 유지했다. 이번 작업은 패키지 목록이나 ProjectSettings를 변경하지 않았다.

걷기 8, 달리기 12, 점프 높이 1.2, 캡슐 높이 4·반지름 0.5, 모델 배율 약 2.270328과 모델의 로컬 위치는 유지했다. 기존 Visual 큐브는 보관 및 렌더러 OFF 상태를 유지한다.

## 코드 변경과 제거·교체

### PlayerCharacterInput.cs / IPlayerCharacterInput.cs

이동 계약의 Look·RotateHeld를 제거하고 이동·달리기·점프만 남겼다. 시점 입력은 앞서 추가한 IPlayerCharacterViewInput을 구현해 제공한다. _rotate와 RotateHeld 대신 _aim과 AimHeld·AimPressed·AimReleased를 사용한다. 기존 액션 인스턴스 복제·정리 방식은 유지했다.

OnEnable의 CursorLockMode.None·visible=true를 조작 잠금으로 교체했다. Esc, 포커스 상실, 클릭 재포착, 비활성화 시 이전 커서 상태 복원을 처리한다. 잠금이 해제되면 이동·시선·발사 입력을 내보내지 않는다. 재포착 프레임의 마우스 이동과 재포착 클릭 발사를 억제한다. DefaultExecutionOrder=-100으로 커서/입력 상태가 시점 처리보다 먼저 갱신된다.

### PlayerCharacterMovement.cs

```csharp
// 이전: 이동 컴포넌트가 우클릭 회전까지 처리
Tick(move, look, rotate, sprint, jump, deltaTime);
if (rotate)
    transform.Rotate(0f, look.x * _rotationSensitivity, 0f, Space.World);

// 변경: 몸 방향은 시점 컴포넌트가 결정하고 이동은 현재 몸 방향을 사용
Tick(move, sprint, jump, deltaTime);
```

_rotationSensitivity 필드와 회전 분기를 제거했다. 기존 값 3은 PlayerCharacterView의 _lookSensitivity=3으로 옮겼다. CharacterController 이동·중력·충돌·점프·속도 설정은 변경하지 않았다. 테스트의 Tick 호출과 가짜 이동 입력 구현도 새 계약으로 갱신했다.

### PlayerCharacterView.cs

앞서 추가한 상태 컴포넌트를 실제 입력에 연결했다. 선호 시점, ADS·견착, 수직 시선과 자유 시점 상태를 소유한다. DefaultExecutionOrder=-50으로 몸 방향을 이동보다 먼저 처리한다. 마우스 델타는 기존 입력 배율 0.1과 감도 3을 사용한다. 몸에는 yaw만, 카메라에는 pitch와 yaw를 적용한다.

기존 단일 자유 시점 yaw 제한 120을 3인칭 180·1인칭 90으로 구분했다. 일반/자유 시점 pitch는 ±80도로 제한하며, 자유 시점 복귀와 새 마우스 입력이 겹쳐도 합산 pitch가 범위를 넘지 않게 했다. 자유 시점 복귀 속도 12, ADS 클릭 구분 0.2초다. 조작 상실이나 비활성화 시 조준·자유 시점의 임시 상태를 정리한다.

### PlayerCharacterCamera.cs

클래스·GUID·기존 카메라 오브젝트·_target 참조는 유지했다. 기존의 위치 Vector3.Lerp와 앞 지점을 바라보는 Quaternion.LookRotation 추적 구현을 눈높이 기반 카메라 위치와 시점 회전으로 교체했다.

| 직렬화 설정 | 이전 | 변경 |
|---|---|---|
| _offset | (0,18,-14) | (0.8,0.25,-5), 눈높이 피벗 기준 3인칭 오프셋 |
| _lookOffset | (0,0,5), 바라볼 앞 지점 | (0,1.6,0), 3인칭 피벗 |
| _firstPersonOffset | 없음 | (0,1.6,0), 캐릭터 루트 기준 눈 위치 |
| _aimOffset | 없음 | (0.65,0.05,-3), 견착 카메라 |
| _followSpeed | 12 | 12 유지. 거리 복귀와 FOV 보간에 사용 |
| _normalFieldOfView | 없음 | 60 |
| _aimFieldOfView / _adsFieldOfView | 없음 | 50 / 40 |
| _collisionRadius / _collisionPadding | 없음 | 0.2 / 0.05 |
| _collisionMask | 없음 | 모든 레이어, Trigger와 자신의 콜라이더 제외 |
| Camera.nearClipPlane | 0.3 | 0.05 |

눈 위치는 애니메이션 본 대신 캐릭터 루트에 고정해 머리 애니메이션의 흔들림이 직접 카메라로 전해지지 않게 했다. SphereCastNonAlloc의 고정 버퍼 64개로 벽과의 거리를 계산한다. 장애물이 가까워지면 즉시 거리를 줄이고 멀어지면 부드럽게 복귀한다. 카메라의 회전과 눈높이는 현재 프레임의 시점/캐릭터 위치를 사용한다.

IPlayerCharacterAimSource.GetAimRay를 구현한다. 새 입력에 대한 현재 조준 회전과 위치를 계산하므로 이전 LateUpdate의 카메라 Transform에 의존하지 않으며, Alt 자유 시점은 조준 광선을 변경하지 않는다.

검증 중 같은 카메라의 비활성 JetCamera도 Awake에서 FOV를 80으로 바꾸는 것을 확인했다. 캐릭터 카메라가 기본 FOV를 런타임 값에서 복사하지 않고 직렬화된 60으로 직접 관리하도록 했다. JetCamera 코드는 이번 작업에서 수정하지 않았다.

### PlayerCharacterVisibility.cs

현재 시점과 카메라 거리에 따라 Armature의 SkinnedMeshRenderer를 ShadowsOnly로 전환한다. 1인칭·ADS 또는 카메라가 피벗에서 0.8 미만으로 접근할 때 몸이 화면을 막지 않으면서 그림자를 유지한다. 3인칭/거리 복귀와 비활성화 시 원래 그림자 모드를 복구한다. DefaultExecutionOrder=50으로 카메라의 LateUpdate 뒤에 표시를 갱신한다. 기존 Visual 큐브의 렌더러나 모델 프리팹을 수정하지 않는다.

### PlayerCharacterController.cs

기존 transform.forward 고정 발사에 선택적 IPlayerCharacterAimSource를 연결했다. 조준 광선의 가장 가까운 표면을 구한 뒤 실제 캐릭터 발사 위치에서 그 지점을 향하도록 한다. 조준 소스가 없는 기존 구성은 몸 정면 발사를 유지한다. 직렬화 참조 _aimSourceBehaviour는 Camera 컴포넌트로 연결했다. 잘못된 타입을 연결하면 오류를 알리고 비활성화한다.

_aimDistance=1000, _aimMask=모든 레이어를 추가했다. RaycastNonAlloc 버퍼 64개를 재사용하고, 소유자와 발사체 콜라이더는 조준 표면 조회에서 제외한다. 발사 위치를 몸 바깥으로 옮길 때 사이에 엄폐물이 있으면 그 직전으로 제한해 벽 너머에 총알이 생성되지 않게 했다. 조준점이 발사 위치보다 뒤에 있을 때 뒤로 발사하지 않도록 한다.

발사 주기 10, 피해 10, 총구 오프셋 0.6과 BulletPool 참조, ProjectileData의 소유자 및 팀 판정은 유지한다. 새 발사를 원본 씬에 연결하기 전에 아군 더미 소유자로 적·중립·아군 판정과 엄폐물 차단을 검증했다.

## 입력 에셋·씬·테스트 구성

InputSystem_Actions.inputactions의 PlayerCharacter 맵에서 Rotate 이름만 Aim으로 바꾸고 액션 ID, 바인딩 ID와 우클릭 경로를 유지했다. TogglePerspective(V), FreeLook(Left Alt), ReleaseCursor(Esc) 액션·바인딩을 추가했다. 기존 다른 액션·맵·키·제어 스킴은 보존했다.

CombatTestScene의 PlayerCharacter 루트에 PlayerCharacterView(fileID 950000004), PlayerCharacterVisibility(950000003)를 추가했다. 기존 카메라(920000002)에 시점 참조를, 발사 컴포넌트(920000001)에 조준 소스를 연결했다. 기존 Armature SkinnedMeshRenderer를 참조하는 stripped 참조(1601236111)를 추가했으며 모델 컴포넌트 자체를 새로 만들거나 교체하지 않았다. Camera(330585545) 근거리 한계와 위 설정을 적용하고 이동 컴포넌트의 옛 _rotationSensitivity 값을 제거했다.

시작 위치가 Dummy_Metal 콜라이더와 겹치는 문제를 실제 렌더링과 물리 쿼리로 확인했다. 사용자 추가 승인 후 PlayerCharacter 루트 Transform(757094942)의 로컬 위치만 (0,2,0) → (0,2,10)으로 변경했다. 다른 오브젝트 위치·계층·크기는 유지했다. 원본 씬에서 바뀐 기존 블록은 위 6개이며 새 참조/컴포넌트 블록 3개만 추가했다.

PlayMode 테스트 asmdef에 기존 설치된 Unity.InputSystem과 Unity.InputSystem.TestFramework 참조를 추가했다. 새 패키지는 설치하지 않았다. 가상 키 입력은 공식 InputTestFixture로 실제 장치와 Editor 포커스의 영향을 격리하고 수동 프레임 펌프로 검증한다. 테스트 종료 시 장치와 설정을 복원한다.

## 설계 점검

이동·입력·시점 상태·카메라 배치·표시·발사를 각 책임에 맞게 분리했다. 상태는 담당 컴포넌트 안에 보관하고 소비자에는 읽기 전용 속성/집중된 입력 계약으로 노출했다. 발사 코드는 카메라 구체 타입 대신 IPlayerCharacterAimSource에 의존한다. 같은 로컬 캐릭터를 구성하는 시점·카메라·표시 사이에는 명시적 직렬화 참조를 사용했다. 각 클래스용 추가 인터페이스나 상속 계층은 만들지 않았다. DefaultExecutionOrder 세 곳은 입력 → 몸 방향 → 이동 → 카메라 → 표시 순서를 보장하기 위한 의도적 사용이며 프로젝트 전역 실행 순서 설정을 변경하지 않았다.

## 검증과 남은 확인

- Unity 6000.4.8f1 별도 검증 프로젝트에서 컴파일 및 PlayMode 27/27 통과.
- 시점 상태 5개, 카메라·표시·시작 위치 6개, 실제 입력 바인딩·커서 생명주기 2개, 새 조준/팀/엄폐물 2개와 기존 이동·발사·피격·탄흔·애니메이션 회귀 12개를 포함한다.
- 아군 더미 소유자의 상향 조준으로 적/중립 피해와 아군 무피해를 먼저 확인했다. 카메라만 벽 너머를 볼 때 총알이 벽을 건너뛰지 않는 점도 확인했다.
- 원본과 검증본의 관련 변경 파일 19개가 동일하다. 원래 입력 ID/바인딩, 다른 맵, 제어 스킴, 씬 내부 참조·fileID 유일성, 원본 씬의 다른 블록 보존을 확인했다. git diff --check 통과.
- 최종 시작 위치에서 1인칭·3인칭을 실제 URP 카메라로 렌더링해 모델 표시·숨김과 눈높이 구도를 확인했다.
- 원본 Unity Editor에서 CombatTestScene을 다시 불러와 Play하고 V·마우스·Alt·우클릭·Esc·복귀 클릭을 직접 조작해 감도, 벽 근처의 구도와 FOV 전환 느낌을 확인해야 한다.
- 현재 총/손 모델과 조준경은 없으므로 ADS는 눈 위치와 FOV 전환이다. 새 무기 모델·조준 애니메이션·자세/기울이기 기능을 포함한 배그 전체 캐릭터 시스템을 구현한 것은 아니다.

## 이번 턴에서 수정·추가한 모든 파일

기존 Git 미커밋 변경 전체와 이번 변경을 구분하기 위해 직전 작업 파일 해시와 비교한 목록이다. 새 테스트의 메타데이터와 이 보고서도 포함한다.

- [Assets/Features/PlayerCharacter/Scripts/IPlayerCharacterInput.cs](E:/GitHub/Battlefield_Project/Assets/Features/PlayerCharacter/Scripts/IPlayerCharacterInput.cs)
- [Assets/Features/PlayerCharacter/Scripts/PlayerCharacterCamera.cs](E:/GitHub/Battlefield_Project/Assets/Features/PlayerCharacter/Scripts/PlayerCharacterCamera.cs)
- [Assets/Features/PlayerCharacter/Scripts/PlayerCharacterController.cs](E:/GitHub/Battlefield_Project/Assets/Features/PlayerCharacter/Scripts/PlayerCharacterController.cs)
- [Assets/Features/PlayerCharacter/Scripts/PlayerCharacterInput.cs](E:/GitHub/Battlefield_Project/Assets/Features/PlayerCharacter/Scripts/PlayerCharacterInput.cs)
- [Assets/Features/PlayerCharacter/Scripts/PlayerCharacterMovement.cs](E:/GitHub/Battlefield_Project/Assets/Features/PlayerCharacter/Scripts/PlayerCharacterMovement.cs)
- [Assets/Features/PlayerCharacter/Scripts/PlayerCharacterView.cs](E:/GitHub/Battlefield_Project/Assets/Features/PlayerCharacter/Scripts/PlayerCharacterView.cs)
- [Assets/Features/PlayerCharacter/Scripts/PlayerCharacterVisibility.cs](E:/GitHub/Battlefield_Project/Assets/Features/PlayerCharacter/Scripts/PlayerCharacterVisibility.cs)
- [Assets/InputSystem_Actions.inputactions](E:/GitHub/Battlefield_Project/Assets/InputSystem_Actions.inputactions)
- [Assets/Scenes/CombatTestScene.unity](E:/GitHub/Battlefield_Project/Assets/Scenes/CombatTestScene.unity)
- [Assets/Tests/PlayMode/Battlefield.Features.PlayModeTests.asmdef](E:/GitHub/Battlefield_Project/Assets/Tests/PlayMode/Battlefield.Features.PlayModeTests.asmdef)
- [Assets/Tests/PlayMode/PlayerCharacterAimTests.cs](E:/GitHub/Battlefield_Project/Assets/Tests/PlayMode/PlayerCharacterAimTests.cs)
- [Assets/Tests/PlayMode/PlayerCharacterAimTests.cs.meta](E:/GitHub/Battlefield_Project/Assets/Tests/PlayMode/PlayerCharacterAimTests.cs.meta)
- [Assets/Tests/PlayMode/PlayerCharacterAnimationTests.cs](E:/GitHub/Battlefield_Project/Assets/Tests/PlayMode/PlayerCharacterAnimationTests.cs)
- [Assets/Tests/PlayMode/PlayerCharacterCameraTests.cs](E:/GitHub/Battlefield_Project/Assets/Tests/PlayMode/PlayerCharacterCameraTests.cs)
- [Assets/Tests/PlayMode/PlayerCharacterCameraTests.cs.meta](E:/GitHub/Battlefield_Project/Assets/Tests/PlayMode/PlayerCharacterCameraTests.cs.meta)
- [Assets/Tests/PlayMode/PlayerCharacterInputTests.cs](E:/GitHub/Battlefield_Project/Assets/Tests/PlayMode/PlayerCharacterInputTests.cs)
- [Assets/Tests/PlayMode/PlayerCharacterInputTests.cs.meta](E:/GitHub/Battlefield_Project/Assets/Tests/PlayMode/PlayerCharacterInputTests.cs.meta)
- [Assets/Tests/PlayMode/PlayerCharacterMovementTests.cs](E:/GitHub/Battlefield_Project/Assets/Tests/PlayMode/PlayerCharacterMovementTests.cs)
- [Assets/Tests/PlayMode/PlayerCharacterViewTests.cs](E:/GitHub/Battlefield_Project/Assets/Tests/PlayMode/PlayerCharacterViewTests.cs)
- [Docs/AI/PlayerCharacterViewChanges.md](E:/GitHub/Battlefield_Project/Docs/AI/PlayerCharacterViewChanges.md)

## 앞선 턴에서 추가해 이번에 연결한 계약

다음 파일은 이번 턴에서 내용이 바뀌지 않았다. 이전 시점 상태 준비 작업에서 생성한 계약이며 기존 .meta를 유지했다.

- [Assets/Features/PlayerCharacter/Scripts/IPlayerCharacterViewInput.cs](E:/GitHub/Battlefield_Project/Assets/Features/PlayerCharacter/Scripts/IPlayerCharacterViewInput.cs) 및 같은 이름의 `.meta`
- [Assets/Features/PlayerCharacter/Scripts/IPlayerCharacterAimSource.cs](E:/GitHub/Battlefield_Project/Assets/Features/PlayerCharacter/Scripts/IPlayerCharacterAimSource.cs) 및 같은 이름의 `.meta`

## 검증 산출물

아래 Temp 산출물과 검증용 Editor 스크립트는 Git에 포함하지 않는다. 원본 Assets에 검증용 Editor 스크립트를 추가하지 않았다.

- [Temp/codex-character-aim-dummies.xml](E:/GitHub/Battlefield_Project/Temp/codex-character-aim-dummies.xml)
- [Temp/codex-character-perspective-final.xml](E:/GitHub/Battlefield_Project/Temp/codex-character-perspective-final.xml)
- [Temp/codex-character-perspective-final.log](E:/GitHub/Battlefield_Project/Temp/codex-character-perspective-final.log)
- [Temp/CodexStructureValidation/Assets/Editor/CodexCharacterCameraSetup.cs](E:/GitHub/Battlefield_Project/Temp/CodexStructureValidation/Assets/Editor/CodexCharacterCameraSetup.cs)

3인칭:

![3인칭](E:/GitHub/Battlefield_Project/Temp/codex-character-third-person.png)

1인칭:

![1인칭](E:/GitHub/Battlefield_Project/Temp/codex-character-first-person.png)
