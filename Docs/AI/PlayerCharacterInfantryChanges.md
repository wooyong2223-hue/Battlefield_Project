# 보병 기능 확장 — 2026-10-27

상태: 로컬 보병 프로토타입을 SampleScene에 통합했다. 배틀필드의 모든 기능을 동일하게 완성한 상태가 아니다. 전용 모션과 미구현 기능은 아래에 구분한다. 작업 시작일은 2026-10-27이며 재개·최종 검증은 2026-10-28이다.

## 작업 전 비교와 보존 기준

중단 전에는 보병 스크립트가 추가됐지만 원본 SampleScene/Animator 연결은 반영되지 않았다. 사용량 제한으로 자동 승인 검토가 실패한 후속 테스트 편집/동기화도 재개 시 따로 확인했다. Unity가 Temp를 정리해 이전 검증 프로젝트와 로그가 없어졌으므로 이전 검사 횟수를 최신 검증으로 재사용하지 않는다. 재개 시점의 파일 해시와 씬/Controller 사본은 Library/CodexInfantryWork에 보관하고, 별도 Project 폴더에서 재검증한다.

현재 SampleScene의 플레이어 최대 체력은 100이다. 탄창과 주먹/권총 근접 공격은 각각 별도 오브젝트의 컴포넌트를 참조한다(_magazine=870274802, _fistAttack=393391954, _pistolAttack=61377339). 이 참조와 배치를 유지한다. 기존 오브젝트의 이름·부모·위치·크기, 플레이어 캡슐 4/0.5, 모델 배치, 이동 8/달리기 12/점프 1.2, 앉기 높이 2.5/속도 4, 카메라 오프셋/FOV, 전투기 수치는 변경하지 않는다.

## 구조 선택

- Stance는 자세와 차지하는 공간, Traversal은 슬라이딩/장애물 통과의 수명과 경로, Movement는 CharacterController 이동을 소유한다. 특수 이동도 Movement가 Step을 호출하므로 독립 Update 두 곳에서 플레이어 이동을 경쟁시키지 않는다.
- ActionPolicy는 읽기 전용 상태로 달리기·공격·점프·소생의 충돌 규칙만 결정한다. Lean/Mount/Recoil은 서로 다른 상태와 수명이 있어 별도 컴포넌트다.
- Health는 기존 HP를 계속 소유한다. InfantryLifeState는 Alive/Downed/Eliminated 및 출혈·소생·리스폰을 소유하고, IHealthDamagePolicy로 다운 중 추가 피해를 처리한다. 현재 HP와 다운 중 마무리 피해량은 서로 다른 상태의 자원이다.
- Revive는 대상 탐색·아군/거리/시야·단일 소생자 점유·진행/취소·끌기를 담당한다. HUD와 자세 표현은 이 상태를 읽기만 한다.
- 입력은 좁은 Traversal/Tactical/Life 인터페이스로 분리한다. 입력 해석·피해·HUD·애니메이션을 하나의 관리자로 합치지 않는다. 새로운 상속 계층이나 전역 상태는 추가하지 않는다.

## 조작과 새 설정

다음은 이 프로젝트의 초기 테스트 수치다. 배틀필드의 비공개 수치를 추출한 값이 아니다. 기존 조작과 사용자 수치는 보존했다.

| 조작 | 동작/설정 |
|---|---|
| Z | 엎드리기/일어서기. 캡슐 높이 1.1, 포복 속도 2, 자세에 따른 눈 위치 전방 보정 1.3. 낮은 천장에서는 일어서기/앉기를 거부한다. |
| 달리던 중 Left Ctrl | 슬라이딩. 0.75초 동안 진입 속도에서 종료 속도 4로 감속하고 앉은 상태로 끝난다. 벽/낙하/자세 변경/탑승으로 중단한다. |
| Space | 평소 점프. 정면 장애물 조건이 맞으면 높이 2.4, 깊이 2, 탐색 거리 2.5, 시간 0.65의 통과 경로를 사용한다. 착지 공간을 검사하며 이동 중 벽/천장과 충돌하면 중단한다. |
| Q / E | 좌우 기울이기. 최대 눈 이동 0.45, 회전 12도, 추적 속도 10. 카메라 이동은 반지름 0.2의 SphereCast로 제한하고 상체/총도 따라간다. |
| 조준 중 B 유지 | 엄폐물 윗면에 무기 거치. 거리 1.3, 거치 시 반동 배율 0.4. 이동·과도한 회전·해제·엄폐 소실·장전/소생 등으로 풀린다. |
| 발사 | 실제 조준 회전에도 피치 1.2/좌우 0.2의 반동을 더하고 최대 피치 6, 복구 속도 8을 적용한다. ADS 0.65, 앉기 0.8, 엎드리기 0.55, 거치 0.4 배율을 조합한다. 기존 총 모델의 표현 반동은 유지한다. |
| 아군 근처 F 유지 | 소생. 범위 3, 시간 3.5초, 같은 비중립 팀/시야/대상 상태를 검사한다. 놓기·거리 이탈·벽·사망으로 취소한다. 소생은 최대 체력의 35%를 복구한다. |
| 소생 중 Shift | 서기/앉기에서 대상을 끌면서 소생한다. 대상의 이동 요청은 해당 Movement에 전달하고 포복 공간/벽 충돌을 적용한다. 끌기 속도 2.5, 소생자의 이동 배율 0.3. 엎드린 소생자는 끌지 않는다. |
| 다운 상태 Space | 소생 포기. 다운 출혈 시간은 20초, 추가 마무리 피해 허용량은 40이다. |
| 사망 후 Enter | 최초 시작 위치로 리스폰. 공간이 막혔으면 거부한다. 체력·자세·탄약·공격 상태를 초기화하고, 차량 조종사라면 좌석을 해제한다. |

달리기는 전방 입력이 있을 때만 허용하며 조준/ADS/발사/근접/장전/소생과 경쟁하지 않는다. 소생·장애물 통과 중 점프도 차단한다. 사망/다운 중 이동 입력은 차단하지만 중력은 계속 적용한다. 기존 1/2 무기 선택, R 장전, G 근접, V 시점, Alt 자유 시점, 우클릭 유지 견착/탭 ADS, F 탑승 기능은 유지한다. 소생 대상이 있으면 같은 F 입력의 탑승보다 소생을 우선한다.

서서 소생을 시작하면 임시로 앉는다. 시작 후 자세 변경이 없을 때만 종료/취소 시 서기로 복귀한다. Stance.ChangeVersion으로 사용자가 명시적으로 바꾼 자세를 구분하며, 천장 때문에 서기가 불가능하면 낮은 자세를 유지한다. 엎드린 소생자는 무릎 클립을 재생하지 않는다.

## 변경한 런타임 스크립트 전체

경로 기준은 E:/GitHub/Battlefield_Project이다. 새 스크립트는 같은 경로의 .meta도 추가해 GUID를 유지한다.

| 파일 | 구체적 변경/이유 |
|---|---|
| Assets/Features/PlayerCharacter/Scripts/PlayerCharacterStance.cs | IsCrouched의 단일 저장 상태를 CharacterStance enum으로 교체하고 기존 TrySetCrouched를 호환 래퍼로 유지. TrySetStance, 임의 높이 HasClearance, ProneSpaceIsClear/AllowsHeading/ConstrainPlanarMovement 추가. 머리/발 길이를 포함한 수평 포복 공간·회전·이동을 검사한다. 사망 중 자세 입력을 차단한다. |
| Assets/Features/PlayerCharacter/Scripts/PlayerCharacterMovement.cs | 특수 이동 Step 호출, 행동 정책의 달리기/점프/사망 입력 제한, 포복 속도와 충돌, 외부 끌기 요청을 최종 이동에 조합. Stop은 특수 이동과 대기 이동도 정리한다. 사망 중 중력은 유지한다. |
| Assets/Features/PlayerCharacter/Scripts/PlayerCharacterTraversal.cs (신규) | 슬라이딩/장애물 통과의 시작 조건, 타이머, 경로, 착지 공간, 충돌 취소 및 disable 정리. |
| Assets/Features/PlayerCharacter/Scripts/PlayerCharacterActionPolicy.cs (신규) | Health/View/Combat/Traversal/Revive의 읽기 상태로 CanAttack/CanSprint/CanJump/MovementMultiplier 결정. 상태를 중복 소유하지 않는다. |
| Assets/Features/PlayerCharacter/Scripts/PlayerCharacterLean.cs (신규) | 기울이기 입력 보간, 눈 위치 SphereCast, 회전/위치 출력, disable 복구. |
| Assets/Features/PlayerCharacter/Scripts/PlayerCharacterWeaponMount.cs (신규) | 윗면 탐색, 거치 접점과 반동 배율, 이동/회전/엄폐 소실 등에 따른 해제. |
| Assets/Features/PlayerCharacter/Scripts/PlayerCharacterRecoil.cs (신규) | Fired 구독/해제, 조준 반동 누적/상한/복구, 자세/ADS/거치 배율, disable/사망 초기화. |
| Assets/Features/PlayerCharacter/Scripts/PlayerCharacterPostureView.cs (신규) | 기본 IK 평가에서 절차적 포복 자세와 상체 기울이기 적용. 포복 중 몸통 LookAt 배율을 낮추고 사망/무릎 소생 클립의 지면 높이를 보정한다. 포복 지면 높이 0.3/사망 0.25/소생 1.3, 자세 블렌드 속도 8. 기존 모델 Transform을 바꾸지 않는다. |
| Assets/Features/PlayerCharacter/Scripts/InfantryLifeState.cs (신규) | 생존 상태, 출혈, 마무리 피해, 단일 소생자 점유, 소생/포기/리스폰/끌기 요청. Health 초기화 전 오판을 피하려고 최초 사망 상태 확인을 Start 이후에 한다. 차량 탑승 중에도 활성 상태를 유지한다. |
| Assets/Features/PlayerCharacter/Scripts/PlayerCharacterRevive.cs (신규) | 아군 검색, 거리/시야/상태 확인, 소생 점유/진행/취소, 끌기. 소생 시작 시 임시 앉기와 변경 버전에 따른 조건부 복귀를 요청한다. 다른 입력/무기 상태를 직접 소유하지 않는다. |
| Assets/Features/PlayerCharacter/Scripts/IPlayerCharacterTraversalInput.cs (신규) | PronePressed만 노출. |
| Assets/Features/PlayerCharacter/Scripts/IPlayerCharacterTacticalInput.cs (신규) | Lean/MountHeld만 노출. |
| Assets/Features/PlayerCharacter/Scripts/IPlayerCharacterLifeInput.cs (신규) | InteractHeld/GiveUpPressed/RespawnPressed만 노출. |
| Assets/Features/PlayerCharacter/Scripts/PlayerCharacterInput.cs | 새 3개 입력 계약과 5개 InputAction 연결. 사망한 조종사도 리스폰할 수 있도록 포기/리스폰은 보행 권한 대신 포인터 캡처로 제한한다. 기존 바인딩은 삭제/교체하지 않는다. |
| Assets/Features/PlayerCharacter/Scripts/PlayerCharacterView.cs | 기존 각도에 조준 반동/기울이기 출력 조합, 포복 시 불가능한 몸 회전 거부, 사망 입력 차단. 기존 마우스 민감도/시점 규칙 유지. |
| Assets/Features/PlayerCharacter/Scripts/PlayerCharacterCamera.cs | 자세에 따른 전방 눈 위치와 충돌 제한된 기울이기 위치를 카메라/조준 Ray/IK 눈 위치에 공통 적용. 기존 오프셋·FOV 수치는 보존. |
| Assets/Features/PlayerCharacter/Scripts/PlayerCharacterWeaponAimView.cs | 기본 IK에서 Posture.Apply, 포복에 따른 몸통 보정 배율, 거치 접점, 행동 차단 상태를 양손 IK에 연결. 기존 같은 프레임 조준 회전과 손 축/도달 거리 보정 유지. |
| Assets/Features/PlayerCharacter/Scripts/PlayerCharacterCombatView.cs | 특수 이동/소생 중 무기와 팔 표현 및 전투 레이어를 내렸다가 복구. 기존 발사/장전/근접 모션 유지. |
| Assets/Features/PlayerCharacter/Scripts/PlayerCharacterAnimationView.cs | Life/Reviving 파라미터 전달. 포복/슬라이딩에서 기본 걷기 속도 모션을 억제한다. 기존 이동/앉기/점프 파라미터는 유지한다. |
| Assets/Features/PlayerCharacter/Scripts/PlayerCharacterController.cs | 행동 정책으로 특수 이동/소생 중 공격/장전을 차단. ResetForRespawn으로 공격 취소·권총 선택·탄약 초기화·표현 이벤트 연결. 기존 피해 10, 발사 간격, 엄폐 보호 계산은 유지한다. |
| Assets/Features/PlayerCharacter/Scripts/PlayerVehicleInteraction.cs | 소생 후보/진행 중 F 탑승을 중복 실행하지 않도록 우선순위 연결. 탑승 중 F 하차는 유지한다. |
| Assets/Features/Weapon/Scrips/WeaponMagazine.cs | Awake의 기존 초기화 코드를 재사용 가능한 ResetAmmo로 이동. 장전 취소와 15/90 초기 탄약은 그대로다. |
| Assets/Features/UI/PlayerCombatHud.cs | 자세·슬라이딩/통과·거치·체력·다운/포기·사망/리스폰·소생 진행/후보 표시. |
| Assets/Framework/Core/Health.cs | 기존 사망 피해 무시 앞에 선택적 피해 정책 호출 추가. Bind/UnbindDamagePolicy, 명시적 TryRevive 추가. 일반 Heal로 사망자를 복구하지 않는 규칙과 정책 없는 기존 소비자의 동작은 유지한다. 이벤트 기본값을 new UnityEvent로 초기화해 동적 테스트 객체의 구독도 지원한다. |
| Assets/Framework/Core/IHealthDamagePolicy.cs (신규) | 보병 다운 피해를 공용 Health의 구체 타입 검사 없이 처리하는 좁은 계약. |

## Editor 테스트 도구와 테스트 파일

| 파일 | 변경 |
|---|---|
| Assets/Features/PlayerCharacter/Editor/Battlefield.Features.PlayerCharacter.Editor.asmdef (신규, .meta 포함) | Editor 전용 어셈블리. 플레이어 빌드에는 포함하지 않는다. |
| Assets/Features/PlayerCharacter/Editor/InfantryTestTools.cs (신규, .meta 포함) | Play Mode 전용 플레이어 다운/소생용 아군 생성 메뉴. 현재 모델을 재사용하며 복제한 스크립트가 원본 플레이어 참조를 변경하지 않도록 정리한다. 1인칭 모델의 ShadowsOnly가 아군에 복제되지 않도록 복구한다. 기존 더미/체력/팀/씬 에셋은 바꾸지 않는다. |
| Assets/Features/PlayerCharacter/Scripts/InfantryTrainingDummy.cs (신규, .meta 포함) | 생성된 테스트 아군의 수동 입력 없는 중력/끌기 이동과 IK 자세 평가. Unity가 Editor 어셈블리의 컴포넌트 부착을 거부해 Editor 폴더에서 런타임 Scripts로 옮겼다. GUID는 유지했고 메뉴는 Editor 전용이다. |
| Assets/Features/PlayerCharacter/Editor.meta (신규) | 새 Editor 폴더 메타. |
| Assets/Tests/PlayMode/PlayerCharacterInfantryTests.cs (신규, .meta 포함) | 포복 공간/회전/충돌, 천장, 슬라이딩, 통과, 달리기 제약, 기울이기 안정성, 거치, 실제 씬 카메라/총/자세, 끌기/리스폰, 탑승 사망/복구, 아군 더미의 반동/마무리 피해와 팀 규칙, Editor 아군 생성/소생, 명시적 자세 선택 보존과 무릎 자세 발 접지 검증. |
| Assets/Tests/PlayMode/InfantryLifeStateTests.cs (신규, .meta 포함) | 다운/마무리 피해/일반 회복 제한, 소생/출혈/리스폰, 아군/거리/시야/취소, 단일 소생자 점유, 정책 없는 Health 호환성. |
| Assets/Tests/PlayMode/PlayerCharacterStanceInputTests.cs | 실제 Z/Q/E/B/Enter 입력과 Esc 후 차단 검사 추가. 기존 Left Ctrl 검사 유지. |

## 씬·Animator·애니메이션·입력 에셋

검증 후 원본에 반영한 정확한 부분은 다음과 같다. 기존 직렬화 객체를 삭제하거나 파일 전체를 새 구현으로 덮어쓰지 않았다.

| 파일 | 변경 |
|---|---|
| Assets/Scenes/SampleScene.unity | 기존 PlayerCharacter에 ActionPolicy/Traversal/Lean/Mount/Recoil/LifeState/Revive 7개, 기존 Armature에 PostureView 1개 추가하고 명시적 참조 연결. 보행 중단 목록에 7개를 추가하되 LifeState는 탑승 중에도 활성 상태로 유지. 기존 GameObject와 Transform 및 탄창/근접 참조는 보존. |
| Assets/Features/PlayerCharacter/Animation/PlayerCharacter.controller | Life(int), Reviving(bool), Eliminated/Revive 상태 및 진입/복귀 전환 추가. 기존 빈 AnyStateTransitions 목록만 새 전환 참조 목록으로 확장. 기존 상태·레이어·마스크·클립·IK Pass를 삭제하지 않는다. |
| Assets/ThirdParty/Quaternius/Animations/InfantryDeath.anim 및 .meta (신규) | 공식 CC0 Standard 팩의 Armature\|Death01 추출. 기존과 같은 Humanoid RootQ/RootT 180도 전방 보정, 반복 OFF. |
| Assets/ThirdParty/Quaternius/Animations/InfantryRevive.anim 및 .meta (신규) | 같은 팩의 Armature\|Fixing_Kneeling 추출, 반복 ON. 전용 소생 클립이 아니라 무릎 작업 모션을 테스트용으로 재사용한다. |
| Assets/ThirdParty/Quaternius/SOURCE.md | 두 클립의 출처·원본 이름·보정·대체 모션 사용 기록 추가. 원본 FBX는 Assets에 포함하지 않는다. |
| Assets/InputSystem_Actions.inputactions | Prone(Z), Mount(B), Lean(Q/E 1DAxis), GiveUp(Space), Respawn(Enter) 추가. 기존 action/binding ID와 바인딩은 보존. GiveUp은 다운 상태에서만 소비해 기존 점프와 충돌하지 않는다. |
| Docs/AI/PlayerCharacterInfantryChanges.md | 이 보고서. |
| Docs/AI/PlayerCharacterFeatureRoadmap.md | 실제 구현한 기능과 전용 모션/나머지 기능을 구분해 최신 상태 반영. |
| Docs/AI/UnityProjectContext.md | 현재 보병 상태·연결·검증 결과와 다음 작업 반영. |

## 제거/교체 내역

- Stance의 단일 IsCrouched 저장 상태를 Current enum과 읽기 전용 IsCrouched로 교체했다. 서기/앉기 호출은 TrySetCrouched로 그대로 지원한다.
- HasStandingClearance를 임의 목표 높이 HasClearance로 교체했다. 서기뿐 아니라 포복에서 앉기 진입도 천장 검사가 필요하기 때문이다.
- Movement의 `!crouched && sprint && input.sqrMagnitude > 0`를 전방 입력/행동 정책을 확인하는 조건으로 교체했다. 단순 캡슐 이동 식은 포복 제한과 끌기를 조합하는 식으로 확장했다.
- Health의 초기 `if (IsDead) return`은 정책 호출 이후로 이동했다. 정책이 없는 차량/더미에는 같은 규칙이 적용된다.
- Magazine.Awake의 초기화 본문은 ResetAmmo로 이동했다. 메서드나 초기 탄약 수치를 삭제하지 않았다.
- 기존 컴포넌트·참조·모델·마스크·클립·게임플레이 설정을 삭제하지 않았다. 개발 중 추가했으나 사용하지 않던 PostureView의 Movement 참조는 최종 설계에서 제외했다.
- InfantryTrainingDummy.cs/.meta는 Editor에서 Scripts로 이동했고 네임스페이스는 Battlefield.Editor.PlayerCharacter에서 Battlefield.Features.PlayerCharacter로 변경했다. Unity의 Editor 컴포넌트 부착 제한 때문에 필요한 수정이며 GUID는 보존했다. 기존 게임플레이 객체를 삭제한 것은 아니다.

## 검증과 남은 범위

최신 격리 프로젝트 Library/CodexInfantryWork/Project의 제품 PlayMode 검사는 **95/95 통과**했다(verified-playmode.xml, 54.18초). 이전 Temp 검사 횟수 대신 현재 씬/코드로 재실행한 결과다. 컴파일 오류·예상하지 못한 런타임 예외·Invalid worldAABB는 이 실행에서 발생하지 않았다.

별도 실제 씬 렌더 검사는 **1/1 통과**했다(verified-capture.xml, 8.04초). 포복·소생·다운·사망 화면을 직접 확인하고, 씬을 다시 연 뒤 모든 Renderer bounds가 유한한지 검사했다. 최종 소생 자세의 낮은 발 뼈는 바닥에서 약 0.105m였다. 이미지/로그는 Library/CodexInfantryWork/infantry-*.png 및 verified-capture.log에 보관한다. 임시 캡처 스크립트는 제품 Assets에 포함하지 않는다.

정적 검사(audit.py/audit.json)는 기존 씬 직렬화 객체 674개와 Controller 객체 29개 보존, 각각 컴포넌트 8개/상태·전환 객체 6개 추가, 기존 Transform/RectTransform 및 탄창·근접 참조 보존을 확인했다. C#과 새 클립은 검증본과 바이트 단위로 일치하고, 씬/Controller는 객체 ID별 내용이 일치한다(Controller의 새 객체 저장 순서만 다를 수 있음). Assets GUID 1122개가 고유하며 새 의존성은 모두 해석된다. Range 속성은 추가하지 않았다. C#/문서 공백을 확인했다. Git 전체 diff의 공백 검사는 이전 변경과 Unity YAML의 빈 값(m_Name 등)에 남는 표준 후행 공백을 보고하며, 이를 지우기 위한 씬 전체 재직렬화는 하지 않았다.

소생 중 떠 있던 발 위치는 실제 씬에서 측정해 보정했다. 초기 0.9 보정은 발 뼈가 바닥보다 약 0.295m 낮아 최종 1.3으로 조정했다. 제품 검사에서도 낮은 쪽 발 뼈가 바닥 -0.05~0.25m 이내인지 검사한다. 중간에 발생한 테스트 도구의 Editor MonoBehaviour 부착 오류도 실제 생성/소생 회귀 검사로 수정했다.

이전 임시 검사 중 관찰됐던 Invalid worldAABB의 정확한 원인은 확정하지 못했다. 최신 제품 검사와 별도 실제 씬 검사에서는 재현 여부를 확인하고, 원본 Editor에서도 반복 플레이/씬 재시작을 최종 확인해야 한다. 오류를 무시하도록 테스트를 변경하지 않았다.

원본 Editor의 저장되지 않은 상태를 강제 저장/재로드하지 않았다. 별도 플레이어 빌드, 커밋/푸시는 수행하지 않았다. 최초 씬 컴포넌트 추가이므로 사용자는 SampleScene을 다시 연 뒤 Play해야 한다. 실제 마우스 조작감, 지형별 슬라이딩/통과, 극단 포복 조준의 모델/지면 겹침, UI 배치, 원본 Editor 반복 플레이는 수동 확인이 남아 있다.

OOP/SOLID 검토: 상태 소유와 입력/판정/표현을 분리하고 필요한 읽기 계약 및 피해 정책 계약만 추가했다. MonoBehaviour 간 동일 캐릭터 내부 참조는 명시적으로 연결하며, 실재하지 않는 변형을 위한 상속/관리자/광범위 인터페이스는 만들지 않았다. 의도적으로 원칙을 위반한 예외는 없다. 다만 현재 무기 확장·네트워크·전용 모션 등은 별도 미완료 범위이지 이 설계가 모두 구현했다는 뜻은 아니다.

## 직접 확인하는 순서

1. SampleScene을 다시 열어 Play하고 Game 뷰를 클릭해 마우스를 캡처한다. Z 포복, 달리기 중 Ctrl 슬라이딩, 낮은 장애물을 향해 Space, Q/E 기울이기, 조준 중 B 거치를 확인한다.
2. Tools > Battlefield > Infantry > Spawn Downed Ally (Play Mode)를 실행하면 기존 더미를 변경하지 않고 플레이어 옆에 소생 가능한 아군을 생성한다. F 유지로 소생하고 Shift를 함께 눌러 끌기, F 해제로 취소를 확인한다. 이 테스트 아군은 Play 종료 시 사라진다.
3. 같은 메뉴의 Down Player (Play Mode)로 현재 최대 체력을 바꾸지 않고 다운시킨다. 다운 중 Space 포기, 사망 후 Enter 리스폰을 확인한다.
4. 1·3인칭, 서기/앉기/엎드리기, 장전·근접, 탑승·하차와 조종사 사망/리스폰을 함께 확인한다.

포복은 절차적 테스트 자세이며 전용 팔다리 포복 모션은 없다. 슬라이딩은 앉기 자세, 장애물 통과는 기존 이동/점프 표현을 사용한다. 소생도 전용 연출이 아니다. 따라서 배틀필드와 같은 시각적 완성도로 보고하지 않는다.

수류탄/장비·병과, 무기/인벤토리 확장, 실제 부착물, 정교한 엄폐/장애물 모션, 수영·잠수·낙하산, 래그돌/피격 반응, 보병 오디오, 네트워크 동기화 등은 이 변경으로 구현한 기능이 아니다. 전체 요청은 미완료이며 Roadmap에서 계속 추적한다.

## 신규/이동 메타 파일 목록

각 경로의 GUID를 확인했고 중복이 없었다. TrainingDummy는 이동 전 GUID를 그대로 사용한다.

| 메타 파일 | GUID |
|---|---|
| Assets/Features/PlayerCharacter/Scripts/IPlayerCharacterTraversalInput.cs.meta | 55a85c8ba4a3487d8c507470b4fea4d8 |
| Assets/Features/PlayerCharacter/Scripts/IPlayerCharacterTacticalInput.cs.meta | 6572300f7d9549da8c8b2d707c7e926d |
| Assets/Features/PlayerCharacter/Scripts/IPlayerCharacterLifeInput.cs.meta | 9c3da68d22fd485f880633a799f91947 |
| Assets/Features/PlayerCharacter/Scripts/InfantryLifeState.cs.meta | 81bb6bc85d62467585e630d91c48f31f |
| Assets/Features/PlayerCharacter/Scripts/InfantryTrainingDummy.cs.meta | 0e3383e6de6f35140a655cc5d17a89d3 |
| Assets/Features/PlayerCharacter/Scripts/PlayerCharacterActionPolicy.cs.meta | 2f5136d30be047c8bc537a2b79426b71 |
| Assets/Features/PlayerCharacter/Scripts/PlayerCharacterTraversal.cs.meta | 2f8e8873df7645c8a08811d29311bf81 |
| Assets/Features/PlayerCharacter/Scripts/PlayerCharacterLean.cs.meta | b07e5cdc649349ef86c3792c4f28b201 |
| Assets/Features/PlayerCharacter/Scripts/PlayerCharacterWeaponMount.cs.meta | 1ffc0dbba3de4a14aa81dfcf26df5dc6 |
| Assets/Features/PlayerCharacter/Scripts/PlayerCharacterRecoil.cs.meta | 9c0468b5097148afb9009afdba7b9154 |
| Assets/Features/PlayerCharacter/Scripts/PlayerCharacterPostureView.cs.meta | c24eecf0ef4a4ee79bf7d41f0294578a |
| Assets/Features/PlayerCharacter/Scripts/PlayerCharacterRevive.cs.meta | 2e8c801d22ac49888e1ec76717cd883c |
| Assets/Framework/Core/IHealthDamagePolicy.cs.meta | ab91378860f7444191e8311b695d4258 |
| Assets/Features/PlayerCharacter/Editor/InfantryTestTools.cs.meta | 768cfb034c465504b81107b0a4fb9035 |
| Assets/Features/PlayerCharacter/Editor/Battlefield.Features.PlayerCharacter.Editor.asmdef.meta | ed54cf65f445e274f811cabe68954273 |
| Assets/Features/PlayerCharacter/Editor.meta | 9d7c7f834d98b8047a2f3076f1a991e8 |
| Assets/Tests/PlayMode/InfantryLifeStateTests.cs.meta | ae7c6414f55c479dad6b6bd5857cf5b8 |
| Assets/Tests/PlayMode/PlayerCharacterInfantryTests.cs.meta | 526cacfb01df4250a1dca02baf53132f |
| Assets/ThirdParty/Quaternius/Animations/InfantryDeath.anim.meta | a115df9efbd1d564bb12b07df4d6705f |
| Assets/ThirdParty/Quaternius/Animations/InfantryRevive.anim.meta | 78b51ecad8c59324aa0aa10cae5a883a |

