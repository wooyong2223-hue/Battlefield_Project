# 캐릭터 앉기·앉아서 이동 및 외부 애니메이션 연결

2026-10-27. 첫 캐릭터 확장 범위로 Ctrl 앉기 전환, 앉아서 이동, 캡슐/시점 높이, 천장 아래 일어서기 제한, 실제 앉기 애니메이션을 구현했다. 커밋은 만들지 않았다.

## 사용 방법과 동작

- SampleScene에서 Play하고 왼쪽 Ctrl로 앉기/서기를 전환한다. 새로 앉는 입력은 지상에서만 처리한다.
- 앉아서 이동하는 속도는 4이며 달리기와 점프 입력은 제한한다. 기존 서기 이동 8, 달리기 12, 점프 높이 1.2는 유지한다.
- 캡슐 높이는 서기 4 → 앉기 2.5, 중심 Y는 0 → -0.75다. 바닥 위치는 그대로 유지하고 캡슐 윗부분만 낮춘다. 반지름 0.5와 캐릭터/모델의 위치·크기 설정은 보존한다.
- 서기 공간에 장애물이 있으면 Ctrl을 눌러도 앉기 상태를 유지한다. 자기 충돌체와 트리거는 무시하고, 전체 서기 캡슐로 검사한다. 충돌 조회 버퍼가 가득 차면 보수적으로 일어서기를 거부한다.
- 1인칭·ADS 눈높이와 3인칭 카메라 기준점은 1.5 낮아진다. 기존 카메라 오프셋 값을 바꾸지 않고 현재 자세의 높이 차이를 적용하며 기존 추적 속도 12로 부드럽게 전환한다. 조준 Ray에도 같은 시점 높이를 적용한다.
- Esc로 커서를 해제하거나 차량에 탑승한 동안에는 앉기 입력을 받지 않는다. 앉은 상태에서 전투기에 타면 자세 컴포넌트를 중지하고 서기 캡슐로 복원한 뒤 기존 실제 착석 기능을 적용한다. 하차하면 서기 상태로 돌아온다.

## 변경 전 비교와 구조 선택

작업 전 SampleScene, Fighter.prefab, PlayerCharacterMovement, PlayerCharacterCamera가 직전 검증본과 같음을 확인했다. 승인받아 적용한 전투기 배율 2.270328·좌석/카메라 보정과 사용자가 저장한 씬 배치는 보존했다. 기존 입력/카메라/이동 설정과 애니메이션 상태를 기준으로 확장했다.

앉기 상태와 충돌 캡슐 변경은 독립 책임이므로 PlayerCharacterStance를 분리했다. 원시 입력은 PlayerCharacterInput, 물리 이동은 PlayerCharacterMovement, 시점은 PlayerCharacterCamera, 애니메이션은 PlayerCharacterAnimationView가 맡는다. 자세 입력만 요구하는 IPlayerCharacterStanceInput을 별도로 두어 기존 IPlayerCharacterInput 구현과 테스트를 불필요하게 변경하지 않았다. 새 매니저나 상속 계층은 추가하지 않았다.

## 외부 애니메이션 출처

- 제작자: Quaternius.
- 공식 다운로드: https://quaternius.itch.io/universal-animation-library
- 무료 Standard판, CC0 1.0 Universal. 동봉 License.txt를 보관했다.
- 원본 Unity/UAL1_Standard.fbx는 루트 이동이 없는 버전이며, 원본 클립 이름은 Armature|Crouch_Idle_Loop 및 Armature|Crouch_Fwd_Loop이다.
- Unity 6000.4.8f1에서 Humanoid로 변환한 뒤 필요한 두 클립만 .anim으로 추출했다. 원본 FBX 약 24MB, 모델, 재질, 벤더 코드와 다른 동작은 프로젝트에 넣지 않았다. 추출본만으로 실행되는지 검증하기 위해 검증 프로젝트에서도 FBX를 Assets 밖으로 옮긴 뒤 전체 테스트를 실행했다.
- 기존 Armature와 추출 클립의 전방 축이 180도 달라 실제 캐릭터가 뒤를 보는 문제가 재현됐다. 파생 클립의 Humanoid RootQ/RootT 곡선을 Y축 180도 회전해 보정했다. Quaternion (x,y,z,w) → (z,w,-x,-y), RootT의 X/Z 값과 접선 부호 반전이다. 근육 자세와 원래 모델 Transform은 유지한다. 모델 오브젝트를 돌려서 기존 서기/걷기 방향을 바꾸지 않았다.
- SOURCE.md에 다운로드 날짜, ZIP SHA256, 추출/방향 보정 내용을 기록했다. Root Motion은 계속 OFF이며 CharacterController가 위치 이동의 유일한 권한이다.

## 변경·추가한 파일 전체

| 파일 | 구체적인 변경 |
|---|---|
| Assets/Features/PlayerCharacter/Scripts/IPlayerCharacterStanceInput.cs 및 .meta | 신규. CrouchPressed 읽기 속성만 제공하는 자세 입력 계약. |
| Assets/Features/PlayerCharacter/Scripts/PlayerCharacterStance.cs 및 .meta | 신규 컴포넌트. `_crouchHeight=2.5`, `_crouchMoveSpeed=4`, `_standingObstacleMask=AllLayers`. Awake에서 현재 캡슐의 서기 높이/중심을 기억한다. TrySetCrouched(bool), IsCrouched, HeightReduction, MoveSpeed 제공. 실행 순서 -60으로 입력(-100)/탑승 상호작용(-75) 후, 시점(-50)/이동(0) 전에 갱신한다. OnDisable은 원래 캡슐과 서기 상태를 복원한다. |
| Assets/Features/PlayerCharacter/Scripts/PlayerCharacterInput.cs | IPlayerCharacterStanceInput 구현, `_crouch` 액션 참조 및 `HasControl`로 제한한 CrouchPressed 추가. 기존 입력과 커서 수명은 유지한다. |
| Assets/Features/PlayerCharacter/Scripts/PlayerCharacterMovement.cs | 직렬화 `_stance` 참조 추가. 앉기 중 MoveSpeed=4, IsSprinting=false, 점프 요청 미적용. 기존 서기 분기는 유지한다. |
| Assets/Features/PlayerCharacter/Scripts/PlayerCharacterCamera.cs | 직렬화 `_stance`, 내부 `_stanceHeightOffset`, GetEyeOffset() 추가. LateUpdate에서 높이 차이를 보간하고 1인칭 위치/3인칭 기준점/GetAimRay에 같이 적용한다. OnEnable에서 현재 자세의 높이 차이로 초기화한다. 기존 카메라 직렬화 오프셋/FOV/충돌 값은 유지한다. |
| Assets/Features/PlayerCharacter/Scripts/PlayerCharacterAnimationView.cs | 직렬화 `_stance` 참조와 Crouched Animator bool 갱신 추가. 앉기 속도 정규화 기준은 4이며, 실제 이동 속도를 읽는 기존 방식을 유지한다. 입력·충돌·이동을 변경하지 않는다. |
| Assets/InputSystem_Actions.inputactions | PlayerCharacter 맵에 Crouch Button 및 `<Keyboard>/leftCtrl` 바인딩 추가. 이전 맵/액션/바인딩 ID와 Control Scheme은 보존한다. |
| Assets/Features/PlayerCharacter/Animation/PlayerCharacter.controller | Crouched(bool, 기본 false), CrouchLocomotion 상태/1D Blend Tree 추가. Speed=0에 CrouchIdle, Speed=1에 CrouchWalk. Locomotion↔CrouchLocomotion 전환 0.12초, Land→CrouchLocomotion 0.08초. 기존 Idle/Walk/Run/Jump/InAir/Land 상태·클립·전환은 삭제하지 않았다. |
| Assets/Scenes/SampleScene.unity | PlayerCharacter에 PlayerCharacterStance(fileID 950000018) 추가. 이동/카메라/애니메이션의 `_stance`를 연결하고 PlayerVehicleOccupant의 `_onFootComponents` 첫 항목에 추가했다. 기존 캡슐 높이 4/중심 0/반지름 0.5, 모델 배율·로컬 위치, 씬 배치는 유지한다. |
| Assets/ThirdParty/Quaternius/Animations/CrouchIdle.anim 및 .meta | CC0 앉기 대기 Humanoid 클립 추출, 반복 설정, 전방 축 보정. |
| Assets/ThirdParty/Quaternius/Animations/CrouchWalk.anim 및 .meta | CC0 앉아서 앞으로 걷기 Humanoid 클립 추출, 반복 설정, 전방 축 보정. 현재는 이동 방향별 별도 앉기 클립을 추가하지 않았다. |
| Assets/ThirdParty/Quaternius/License.txt 및 .meta | 배포 ZIP에 동봉된 CC0 라이선스 원문 보관. |
| Assets/ThirdParty/Quaternius/SOURCE.md 및 .meta | 제작자/공식 URL/취득 날짜/ZIP SHA256/원본 클립 이름/파생 처리 기록. |
| Assets/ThirdParty/Quaternius.meta, Assets/ThirdParty/Quaternius/Animations.meta | 신규 벤더/애니메이션 폴더의 Unity GUID 메타데이터. |
| Assets/Tests/PlayMode/PlayerCharacterStanceTests.cs 및 .meta | 신규 7개 테스트. 발 고정과 원래 캡슐 복원, 천장/트리거, 이동 속도·달리기/점프 제한, disable/re-enable, 실제 Humanoid 자세/몸 방향/카메라/루트 이동 없음, 실제 Update 걷기 클립, 충분히 앉은 후 탑승·하차/좌석 눈높이를 검사한다. |
| Assets/Tests/PlayMode/PlayerCharacterStanceInputTests.cs 및 .meta | 신규 1개 테스트. 실제 Left Ctrl 전환과 Esc 이후 입력 제한을 검사한다. |
| Docs/AI/PlayerCharacterCrouchChanges.md | 이 보고서 추가. |
| Docs/AI/UnityProjectContext.md | 현재 앉기 구조·값·소스·검증 상태 갱신. |

## 교체한 코드와 보존한 구현

기존 이동 속도 선택은 `IsSprinting ? _sprintSpeed : _moveSpeed`였다. 현재는 앉기 상태일 때 `_stance.MoveSpeed`를 먼저 선택하고, 그 외에는 이전 식을 그대로 사용한다. 점프 분기에는 `!crouched`를 추가했다.

기존 카메라 계산의 `_target.TransformPoint(_firstPersonOffset/_lookOffset)`은 `GetEyeOffset(기존 오프셋)`을 거쳐 현재 높이 차이를 빼도록 확장했다. 기존 설정 필드와 값은 제거/재해석하지 않았다.

기존 애니메이션 Speed 기준 선택에 앉기 속도 분기를 추가했고 Crouched bool만 추가 전달한다. 기존 상태를 새 컨트롤러로 갈아끼우지 않았다. 런타임 메서드/필드/컴포넌트/에셋/이전 참조의 삭제는 없다. Temp에서 다운로드/추출/방향 비교/렌더에 사용한 도구와 원본 FBX는 프로젝트 런타임 구성에 포함하지 않았다.

## 검증 결과

- Unity 6000.4.8f1 분리 검증 프로젝트, **PlayMode 54/54 통과**. 기존 46개 + 신규 8개. 결과 Temp/codex-crouch-facing-fixed.xml, 로그 Temp/codex-crouch-facing-fixed.log.
- 방향 회귀 검사는 수정 전 몸 방향 내적 약 -0.995로 실패했고, RootQ/RootT 보정 후 통과했다. 원본 FBX를 Assets에서 제거한 상태에서도 전체 테스트가 통과했다.
- 걷기 테스트는 수동 dt와 실제 Update의 시간 차이가 영향을 주지 않도록 실제 IPlayerCharacterInput→Update→CharacterController→AnimationView 경로로 검증한다.
- 정지 렌더에서 기존 모델에 실제 앉기/걷기 자세가 적용되고 같은 전방을 향함을 확인한다. 미리보기 Temp/codex-crouch-stand.png, codex-crouch-idle.png, codex-crouch-walk.png. 샘플 자세의 메시 높이 상한은 대기 약 2.23, 걷기 약 2.41로 캡슐 높이 2.5 안에 들어간다. 모든 시간대의 실루엣을 정밀 충돌체로 사용한다는 의미는 아니다.
- 정적 비교: 기존 씬 ID/상태 ID 보존, 씬의 의도한 기존 블록 5개와 새 자세 컴포넌트 1개만 변경, 컨트롤러 기존 블록 4개 확장/새 객체 5개, 이전 입력 설정 보존을 확인했다. 코드/추출 클립/씬/컨트롤러는 검증본과 동일하다. 메타 GUID 형식과 로컬 참조 무결성 검사도 통과했다.
- 사용자 Unity Editor에서 SampleScene Play 후 Ctrl 전환, 낮은 장애물 아래 서기 거부, 1/3인칭·ADS 높이, 앉아서 걷는 발 주기/속도 체감, 차량 탑승/하차를 확인해야 한다. 실제 빌드/성능 프로파일링은 이번 범위에서 실행하지 않았다.

## 설계 검토

입력·자세/캡슐·이동·카메라·애니메이션의 책임을 분리했다. 상태 변경은 TrySetCrouched로 제한하고 외부에는 읽기 속성을 제공한다. 별도 자세 입력 계약으로 기존 이동 인터페이스를 넓히지 않았다. 단일 자세 구현을 읽는 이동/표현 컴포넌트는 명시적 직렬화 참조로 합성하며 불필요한 상속/상태 머신 프레임워크는 추가하지 않았다. 충돌 검사는 요청 시 NonAlloc 캡슐 조회를 사용하고 자기 몸/트리거를 제외한다. 기존 탑승 수명과 원래 보행 컴포넌트 활성 상태 복원은 보존했다. 신규 필드에 금지된 Range 속성은 사용하지 않았다.
