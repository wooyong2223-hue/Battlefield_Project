# SampleScene 통합 및 전투기 탑승·하차

작업 기준: 2026-10-27, Unity 6000.4.8f1. 이전 캐릭터 이동·시점·모델 작업 위에 추가했다. 커밋은 만들지 않았다.

## 조작과 동작

- SampleScene에서 기존 캐릭터로 전투기 충돌 표면의 3m 이내에 접근하고 F로 탑승한다. 초기의 조종석 왼쪽 한 지점 판정은 아래 탑승 범위 개선으로 확장했다.
- 기존 캐릭터 인스턴스를 유지한다. Humanoid의 엉덩이 뼈를 PilotSeatPoint에 맞추고 척추·다리·팔을 앉은 자세로 배치한다. 탑승용 모델 복제나 렌더러 숨김으로 대체하지 않는다.
- 탑승 중 보행 이동·발사·시점·일반 애니메이션·보행용 가시성 처리를 멈추고 전투기 입력·카메라·HUD를 사용한다. 캐릭터 입력 컴포넌트는 F/Esc/마우스 재캡처를 위해 유지한다.
- F로 하차한다. 왼쪽·오른쪽·뒤쪽 후보에서 실제 CharacterController 캡슐 전체가 들어갈 위치를 검사한다. 모두 막혔으면 탑승 상태를 유지하고 안내를 표시한다.
- 하차하면 원래 컴포넌트 활성 상태·모델 자세·캐릭터 배율·보행 카메라를 복원한다. 재탑승할 수 있다.
- 캐릭터는 원래 Dummys 부모 아래에 유지한다. LateUpdate에서 좌석의 월드 회전과 엉덩이 위치를 따라가므로, 전투기 회전 중에도 좌석 내부에 유지되며 전투기 삭제가 캐릭터 삭제로 이어지지 않는다.
- Esc로 커서를 풀면 전투기 조종을 정지한다. 재캡처 클릭은 발사하지 않고, 버튼을 놓고 다시 눌러야 발사한다.

## 씬·프리팹·설정 변경

| 파일 | 변경 내용과 이유 |
|---|---|
| Assets/Scenes/SampleScene.unity | CombatTestScene의 Dummys 계층을 이전했다. 기존 Enemy·Neutral·5개 재질 더미, 캐릭터와 Armature, 테스트 발사 기능을 보존했다. 탄환 풀과 보행 조준·가시성 참조는 SampleScene의 기존 풀과 Main Camera로 다시 연결했다. |
| Assets/Scenes/SampleScene.unity | Main Camera에 PlayerCharacterCamera를 이전했다. JetCamera.enabled는 1→0, near clip은 0.3→0.05로 변경했다. 시작은 보행 카메라이고 탑승 시 전투기 카메라로 전환한다. 물리 Camera와 AudioListener는 각각 하나다. |
| Assets/Scenes/SampleScene.unity | 캐릭터에 SeatedCharacterPose·PlayerVehicleOccupant·PlayerVehicleInteraction을 추가하고 참조를 연결했다. SeatedCharacterPose의 이 씬 값은 torsoRecline=30, thighDirection=(0,0.4,1), lowerLegDirection=(0,-0.15,1)이다. 기존 모델 배율·오프셋은 유지했다. |
| Assets/Scenes/SampleScene.unity | VehicleInteractionUI/InteractionHint와 VehicleInteractionView를 추가했다. Overlay, 정렬 20, 기준 해상도 1920×1080, 아래 중앙 위치 (0,55), 크기 1000×60, 글자 크기 28. 기존 Noto Sans KR 폰트를 참조한다. |
| Assets/Features/Fighter/Prefabs/Fighter.prefab | VehicleSeat와 PilotSeatPoint=(0,0.75,6.75), PilotEntryPoint=(-2,0,6.75), PilotExitLeft=(-3,0,6.75), PilotExitRight=(3,0,6.75), PilotExitRear=(0,0,-16)을 추가했다. Rigidbody·Health·KeyboardJetInput·AirTargetLock을 연결했다. 기존 전투기 루트와 비행·무기 설정 및 카메라 포인트는 유지했다. |
| Assets/Features/Fighter/Prefabs/Fighter.prefab | PilotSeatPoint 아래에 충돌체 없는 임시 PilotSeatCushion/Back을 추가했다. 쿠션 로컬 위치 (0,-0.18,-0.05), 크기 (0.6,0.15,0.5). 등받이 위치 (0,0.45,-0.3), 크기 (0.8,1.4,0.14), X회전 -30도. |
| Assets/Features/Vehicles/Materials/PilotSeat.mat | 임시 좌석용 URP Lit 재질 추가. BaseColor=(0.025,0.03,0.04), Smoothness=0.2. 전투기 기존 재질은 변경하지 않았다. |
| Assets/InputSystem_Actions.inputactions | PlayerCharacter 맵에 Interact 버튼/F 바인딩을 추가했다. 기존 키와 액션은 유지했다. |
| Assets/Scenes/CombatTestScene.unity 및 .meta | 삭제했다. 별도 씬의 중복 환경·전투기·카메라·풀·HUD는 SampleScene에 다시 복제하지 않고 기존 구성을 재사용한다. 빌드 설정은 원래 SampleScene만 등록되어 있어 변경하지 않았다. |

기존 SampleScene 오브젝트/프리팹 블록이 삭제되지 않았음을 비교했다. Unity 재저장으로 발생한 기존 오디오 커브·필드 순서 변경은 제외했다. 전투기의 씬별 튜닝 값, 활주로 두께, 캐릭터 높이 4·반지름 0.5·모델 배율 2.270328 및 모델 로컬 오프셋은 보존했다. 캐릭터 시작 로컬 위치 (0,2,10)도 유지했다.

## 변경·추가한 런타임 스크립트 전체

| 파일 | 구체적인 변경 및 책임 |
|---|---|
| Assets/Features/Vehicles/Scripts/IVehicleSeatOccupant.cs | 신규. 좌석이 요구하는 Root·CollisionBody·입력 권한·발사 권한·탑승 가능 여부·부착/해제 계약만 제공한다. |
| Assets/Features/Vehicles/Scripts/IVehicleSeatInput.cs | 신규. SetWeaponFireAllowed(bool)로 좌석이 무기 발사 권한을 전달한다. |
| Assets/Features/Vehicles/Scripts/VehicleSeat.cs | 신규. 단일 점유자·거리·생존·전환 중 상태를 검사한다. 조종 입력의 원래 enabled 값을 기억하고 점유/커서 권한에 따라 제어한다. 차량 물리와 무기 자체를 탑승 때문에 끄지 않는다. 하차는 지면 탐색 및 전체 캡슐 충돌 검사로 결정한다. 좌석 비활성화/차량 제거 시 점유자를 해제한다. |
| Assets/Features/PlayerCharacter/Scripts/PlayerVehicleOccupant.cs | 신규. 같은 캐릭터의 보행/차량 상태, 활성 컴포넌트, 카메라/HUD 전환과 원상 복원을 소유한다. 파괴되는 차량 아래로 캐릭터를 재부모화하지 않는다. 씬 종료 시 이미 제거된 참조를 검사한다. |
| Assets/Features/PlayerCharacter/Scripts/PlayerVehicleInteraction.cs | 신규. 명시적으로 연결한 좌석 중 접근 가능한 좌석을 선택하고 F를 탑승/하차 요청으로 바꾼다. 조종 입력 Update 전에 좌석 권한을 갱신한다. |
| Assets/Features/PlayerCharacter/Scripts/SeatedCharacterPose.cs | 신규. 실제 Humanoid 뼈의 원래 자세/Animator 상태를 보관한다. 탑승 동안 Animator를 멈추고 좌석 기준 팔·다리·척추 자세 및 엉덩이 위치를 적용하며, 하차 시 복원한다. 모델 로컬 위치/크기를 바꾸지 않는다. |
| Assets/Features/UI/VehicleInteractionView.cs | 신규. 탑승/하차/막힌 하차 안내 텍스트만 표시한다. 게임플레이 상태를 변경하지 않는다. |
| Assets/Features/PlayerCharacter/Scripts/PlayerCharacterInput.cs | HasControl을 보행 권한과 포인터 캡처로 분리했다. HasPointerCapture·CanFireWithCapturedPointer·InteractPressed·SetGameplayEnabled를 추가했다. 보행을 멈춰도 F/Esc 입력은 유지한다. |
| Assets/Features/PlayerCharacter/Scripts/PlayerCharacterCamera.cs | OnEnable에서 추적 거리를 초기화하고 _camera가 준비되었으면 기본 FOV를 복원한다. 차량에서 돌아올 때 이전 차량 FOV/거리 상태가 남지 않는다. |
| Assets/Features/Fighter/Scripts/Controller/KeyboardJetInput.cs | IVehicleSeatInput을 구현하고 FireWeapon에 발사 허용 값을 적용한다. 숫자 키 직접 조회를 런타임 맵의 SelectPrimaryWeapon/SelectSecondaryWeapon InputAction으로 대체했다. 키 1/2 동작은 같으며 맵 활성/비활성과 장치 재설정에 같이 대응한다. |
| Assets/Features/Fighter/Scripts/Camera/JetCamera.cs | 초기화 완료 상태와 OnEnable 재초기화를 추가했다. 탑승/재탑승 때 궤도·렌즈를 현재 전투기 기준으로 시작한다. |
| Assets/Features/Fighter/Scripts/Camera/JetCameraOrbit.cs | Initialize에서 자유 시점 yaw/pitch, 궤도 초기화 플래그와 roll 추적을 초기화한다. 보행 카메라의 월드 위치나 이전 자유 시점이 재탑승에 남지 않는다. |

위 신규 스크립트의 동명 .meta와 Vehicles/Scripts·Materials 폴더 .meta도 추가했다. 기존 GUID는 유지했다. 이전 캐릭터 작업에서 이미 추가된 이동·조준·애니메이션 에셋이나 FighterDestruction 변경을 이번 탑승 변경으로 다시 집계하지 않는다.

## 교체한 코드와 제거한 것

기존 보행 권한:

```csharp
HasControl => isActiveAndEnabled && _captured && Time.frameCount != _captureFrame;
```

현재:

```csharp
HasPointerCapture => isActiveAndEnabled && _captured && Time.frameCount != _captureFrame;
HasControl => _gameplayEnabled && HasPointerCapture;
```

기존 숫자 키 조회는 `Keyboard.current.digit1Key/ digit2Key.wasPressedThisFrame`이었다. 현재는 런타임 Fighter 맵에 동일 바인딩의 InputAction을 추가하고 performed 콜백에서 선택 슬롯을 보관한다. 다음 Update에서 WeaponSlotSelection으로 전달하고 보관 값을 비운다. 비활성화 때는 둘 다 0으로 초기화한다. 짧은 입력 이벤트가 Update 사이에서 사라지지 않게 했으며 기존 직렬화 필드는 삭제하지 않았다.

구현 도중 사용한 좌석으로의 `SetParent(..., true)`는 제거했다. 차량 활성/비활성 전환 중 부모 변경 오류와 차량 삭제 시 캐릭터 수명 결합을 막기 위해 원래 부모를 유지하고 월드 자세를 추적한다. 임시 진단 로그/에디터 도구는 Temp 검증 프로젝트 안에만 있으며 Assets에는 추가하지 않았다.

하차 지면 탐색은 발 위 2m에서 시작하던 새 구현을 캡슐 높이+2m 위에서 시작하도록 보완했다. 두꺼운 활주로 내부에서 시작하는 Raycast가 윗면을 놓치는 문제를 해결한다. 캡슐 중앙+stepOffset보다 높은 표면은 제외해 머리 위 장애물을 바닥으로 오인하지 않는다. 비볼록 MeshCollider에 ClosestPoint를 호출하지 않는다.

## 테스트 파일 전체

- Assets/Tests/PlayMode/CombatStructureTests.cs: 테스트 씬을 SampleScene으로 변경. 파괴 카메라 검증은 실제 탑승 후 수행한다. 아군 더미 소유 기관총으로 적/중립/아군 판정과 재질 이펙트·탄흔을 재검증한다.
- Assets/Tests/PlayMode/PlayerCharacterMovementTests.cs: SampleScene에서 기존 이동 검증.
- Assets/Tests/PlayMode/PlayerCharacterCameraTests.cs: SampleScene을 사용하는 공통 시점 테스트 로더로 변경.
- Assets/Tests/PlayMode/PlayerCharacterAnimationTests.cs: SampleScene에서 기존 애니메이션 검증.
- Assets/Tests/PlayMode/VehicleSeatTests.cs 및 .meta: 신규 6개 테스트. 점유/거리/권한, 전체 캡슐 장애물, 사망, 비활성화/삭제 시 해제를 검사한다.
- Assets/Tests/PlayMode/PlayerVehicleBoardingTests.cs 및 .meta: 신규 4개 테스트. 실제 보행 접근, 같은 캐릭터와 렌더러/모델 값 유지, 탑승·하차·재탑승 복원, 이동·뱅크 추적, 차량 삭제 후 생존을 검사한다.
- Assets/Tests/PlayMode/PlayerVehicleInteractionInputTests.cs 및 .meta: 신규 1개 입력 시나리오. F/W/Esc/재캡처/발사/하차와 키 1·2 무기 전환을 검사한다. 테스트 장치 제거 전 씬 입력을 비활성화한다.

## 검증과 남은 확인

- 분리한 동일 Unity 버전 프로젝트에서 전체 PlayMode 38/38 통과. 원본 Editor는 실행 중인 상태를 강제로 종료하지 않았다.
- CombatTestScene 삭제 이후 원본과 동일한 최종 씬·프리팹·런타임/테스트 코드로 재실행해 38/38 통과했다. 결과는 Temp/codex-boarding-final.xml 및 .log에 기록했다. 키 1·2의 실제 무기 전환도 포함했다.
- Assets/ProjectSettings에 삭제 씬 이름/GUID를 사용하는 참조가 없음을 검색으로 확인했다. SampleScene 584개·Fighter 프리팹 112개 로컬 ID의 중복과 미해결 로컬 참조가 없음을 검사했다. 기존 블록 중 의도한 SampleScene 4개·프리팹 2개 외의 내용은 공백 차이를 제외하고 보존했다. 관련 코드의 원본/검증 복사본 일치와 .meta 존재, git diff --check도 통과했다.
- Unity 그래픽 배치 렌더에서 같은 캐릭터의 엉덩이 좌석 정렬과 실제 착석 자세를 확인했다. 외부 렌더와 기체 렌더러를 꺼서 내부를 보인 진단 렌더는 구분한다.
- 현재 캐노피는 불투명한 단일 메시다. 외부에서 조종사가 가려지는 것은 기존 에셋 특성이며 조종사를 숨긴 처리가 아니다. 유리 재질/내부 모델 제작은 포함하지 않았다.
- 사용자 Editor에서 SampleScene을 다시 열고 Play하여 실제 조작감, 기체 진동/카메라 전환 느낌, 1인칭 몸체 가림과 자세, 비행 중 하차 위치, 한국어 안내와 사운드 청취를 확인해야 한다. 자동 테스트와 정지 렌더는 이 체감 확인을 대신하지 않는다.

## 설계 점검

좌석은 점유/권한/하차 공간, 캐릭터는 보행 상태 복원, 포즈는 뼈 표현, UI는 안내만 담당한다. 좌석은 IVehicleSeatOccupant/IVehicleSeatInput 계약에 의존해 캐릭터 구체 타입을 검사하지 않는다. 캐릭터 쪽은 프로젝트의 단일 보행 구성과 구체 입력/포즈 참조를 사용하며, 변형이 없는 부분에 별도 상속 계층을 만들지 않았다. Unity 직렬화 참조를 통한 합성으로 연결했고 공개 상태 변경은 의도가 드러나는 메서드로 제한했다. 현재 플레이어는 하나의 전투기 카메라/HUD를 명시적으로 연결하며 다중 장비 카메라 라우터는 추가하지 않았다.

문서 변경: Docs/AI/VehicleBoardingChanges.md 신규, Docs/AI/UnityProjectContext.md 현재 씬 및 탑승 구조 갱신.

## 탑승 범위 개선 — 2026-10-27

직전 시동 작업 검증본과 관련 코드·씬·프리팹이 일치함을 확인한 뒤 변경했다. 기존 3m 값과 모든 위치/계층/하차 설정을 유지했다.

이전 판정은 `(occupant.Root.position - _entryPoint.position).sqrMagnitude <= distance * distance`였다. 조종석 왼쪽 점 하나를 중심으로 검사해 반대편·날개·꼬리에서는 접근해도 탑승이 어려웠다. 전투기는 캐릭터 캡슐 중심에서 기존 기체 Collider의 ClosestPoint까지 거리로 검사하도록 확장했다. 빈 공간을 포함하는 큰 구체/박스 대신 기체의 실제 충돌 형상을 따라 3m 범위가 만들어진다. 판정은 3차원 거리이므로 높이 차이도 반영한다.

| 변경 파일 | 변경한 코드·설정 및 이유 |
|---|---|
| Assets/Features/Vehicles/Scripts/VehicleSeat.cs | `_useVehicleSurfaceForEntry` bool과 비공개 `_entrySurfaces` 캐시 추가. Awake에서 연결된 Rigidbody 아래 Collider들을 한 번 캐시한다. 실행 순서 25로 기존 FighterMeshCollider의 Awake 생성 이후 수집한다. CanEnter의 거리 계산에 표면 모드를 추가했다. 비활성/트리거/비볼록 메시 충돌체는 제외한다. 표면 모드가 꺼진 기존 좌석은 이전 `_entryPoint` 계산을 그대로 사용한다. 표면 모드에서 모든 표면이 무효하면 탑승을 거부한다. |
| Assets/Features/Fighter/Prefabs/Fighter.prefab | VehicleSeat에 `_useVehicleSurfaceForEntry: 1` 추가. `_interactionDistance: 3` 유지. 기존 Collider·탑승 기준점·좌석·하차 후보와 물리 값은 변경하지 않았다. |
| Assets/Tests/PlayMode/VehicleSeatTests.cs | 신규 테스트 1개: 표면 모드에서 좌우/앞뒤 접근 허용, 멀거나 위에 있는 경우 거부, 비활성 표면 거부. 기존 한 지점 모드 테스트도 유지했다. |
| Assets/Tests/PlayMode/PlayerVehicleBoardingTests.cs | 신규 테스트 1개: 실제 전투기의 런타임 convex MeshCollider 좌우/기수/꼬리 표면에서 탑승 가능 여부와 원거리 거부를 검사한다. |
| Docs/AI/VehicleBoardingChanges.md | 현재 조작 설명과 이 변경 보고를 갱신했다. |
| Docs/AI/UnityProjectContext.md | 기체 표면 기준 탑승 범위를 기록했다. |

메서드/필드/컴포넌트/기존 참조 삭제는 없다. 한 지점 판정을 제거하는 대신 선택 가능한 표면 거리 분기를 추가했다. 거리 판정은 좌석 접근 허용이라는 기존 VehicleSeat 책임에 해당하므로 별도 클래스/매니저/상속을 추가하지 않았다. 표면과 캐시는 내부에 숨기고 기존 CanEnter 계약을 유지했다.

분리된 Unity 6000.4.8f1 프로젝트에서 기존 41개와 신규 2개, PlayMode 43/43 통과했다. 결과는 Temp/codex-entry-range.xml, 로그는 Temp/codex-entry-range.log다. 원본/검증본 일치, 씬/프리팹 로컬 참조 및 git diff --check를 확인했다. SampleScene 파일은 이번 범위 변경에서 수정하지 않았다. 사용자 Editor에서 지상에서 좌우/날개/기수/꼬리 쪽으로 실제 접근하며 F 안내와 탑승 거리 체감은 확인해야 한다.

## 탑승 범위 재점검 및 캐시 복구

사용자가 처음에는 왼쪽 앞에서만 탑승된다고 보고했고, 점검 도중 현재는 탑승 가능하다고 확인했다. 저장된 코드/프리팹은 직전 검증본과 같았고, 실제 Unity Inspector에서도 표면 모드 활성화와 거리 3m를 확인했다. 에디터의 미저장 배치(전투기 위치 -1.1,-98.483,-565.56 및 Dummys 위치 79.5,-99,-554.1)는 덮어쓰지 않았다. 임시 에디터 도구로 SaveScene(saveAsCopy:true)를 실행해 현재 배치 복사본만 만들었고, 원본 씬 경로와 dirty=true가 그대로임을 확인했다.

현재 배치 복사본의 지상 접근을 측정한 결과, 좌우/기수/꼬리에서 표면 밖 2.5m 지점은 모두 탑승 가능했다. 꼬리 밖 3.5m 지점은 실제 최단 거리가 약 3.28m여서 거부됐다. 기존 테스트가 공중의 표면 높이에 배치한 것만으로는 실제 지상 접근을 충분히 확인하지 못했다는 검증 한계도 기록한다.

별도로 `_entrySurfaces`가 null인 상태에서 표면 모드가 활성화되면 기존 `if (_useVehicleSurfaceForEntry && _entrySurfaces != null)` 조건을 통과하지 못해 과거 `_entryPoint` 판정으로 돌아가는 결함을 확인했다. Awake 이후 옵션을 활성화하는 회귀 테스트가 수정 전 실패했다(Temp/codex-entry-cache-before.xml). 이 코드 경로의 결함은 재현됐지만, 사용자가 처음 겪은 상황에서 실제로 이 캐시가 비어 있었는지는 직접 측정하지 못했으므로 최초 현상의 원인으로 단정하지 않는다.

이번 재점검의 최종 변경 파일은 다음과 같다.

- Assets/Features/Vehicles/Scripts/VehicleSeat.cs: 조건을 `if (_useVehicleSurfaceForEntry)`로 교체했다. 캐시가 null 또는 빈 배열이면 기존 Rigidbody 아래 Collider를 다시 수집한다. 표면 모드가 켜져 있는 동안 과거 한 지점 판정으로 조용히 바뀌지 않도록 했다. 기존 메서드/필드/참조 삭제는 없고 거리/프리팹/씬 값도 바꾸지 않았다. 접근 판정과 캐시 복구는 기존 VehicleSeat 책임 안에 유지했다.
- Assets/Tests/PlayMode/VehicleSeatTests.cs: `SurfaceEntryInitializesItsCacheWhenEnabledAfterAwake` 테스트 추가. Awake 당시 한 지점 모드였던 좌석을 나중에 표면 모드로 바꿔도 반대편에서 접근 가능한지 검사한다.
- Docs/AI/VehicleBoardingChanges.md: 재현·한계·교체 조건·검증을 기록했다.
- Docs/AI/UnityProjectContext.md: 캐시 복구와 최신 테스트 수를 갱신했다.

임시 Assets/Editor/CodexBoardingSnapshot.cs, 동명 .meta와 이번 진단용으로 만든 빈 Editor 폴더/.meta는 제거했다. 해당 도구는 배치 복사본 생성만 담당했고 런타임 기능에는 필요하지 않다. Temp 검증본에만 넣었던 GroundedBoardingProbe.cs와 .meta도 측정 후 제거했다. 원본에 진단 로그/강제 이동/새 테스트 오브젝트는 남기지 않았다. 원본 씬/프리팹에는 이번 재점검 변경이 없다.

수정 후 전체 PlayMode 44/44 통과(Temp/codex-entry-cache-final.xml 및 .log). 이후 사용자가 에디터 배치를 저장해 SampleScene 파일이 갱신됐다. 현재 씬은 진단 당시 만든 현재 배치 복사본과 공백을 제외하고 일치한다. 이전 저장본 대비 활주로 Y -99.817→-102, Dummys 위치 (100,-99,-500)→(79.5,-99,-554.1), 전투기 Y -96.3→-98.483 및 Unity의 TMP/필드 순서 직렬화 변경을 보존했다. 이 값은 이번 수정으로 작성한 값이 아니다. 최신 저장 배치 검증 결과는 Temp/codex-entry-latest-scene.xml 및 .log에 기록한다. 실제 지상 탑승 가능 여부는 사용자 확인도 받았다. 수치 확장이나 충돌체 형상 변경은 추가하지 않았다.

최신 저장 배치에서도 44/44 통과했다. 원본/검증본의 SampleScene·Fighter 프리팹·변경 코드/테스트 일치를 확인했고, 이번 변경 코드/문서의 diff 공백 검사도 통과했다. 사용자 Editor 저장에서 생긴 씬 YAML 공백은 임의로 정리하지 않았다. 자동 테스트 이후 Unity에서 추가로 확인할 부분은 여러 접근 위치에서의 F 안내 표시와 거리 체감이다.
