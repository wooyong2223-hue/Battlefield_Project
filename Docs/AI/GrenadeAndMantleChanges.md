# 수류탄 투척·조준 / 높은 턱 올라가기 — 2026-11-03

## 사용 방법과 범위

- `4` 짧게 누르기: 원본 준비·투척 모션을 재생하고 던진다. 유지하면 준비 자세에서 시선으로 조준하고, 놓으면 던지기 동작을 이어간다. 기존 `G` 근접과 `1/2/3` 소총/권총/맨손은 그대로다.
- 지면에서 서 있는 상태로 높은 턱을 바라보고 `Space`: 도달 가능한 높이·윗면·착지 공간이면 올라간 뒤 앉기 자세로 연결한다. 현재 캐릭터 높이4 기준2.4m 초과~3.6m 이하, 앞2.5m 이내다.
- 기존 SampleScene에서 실행한다. 새 컴포넌트는 기존 Mixamo 기능과 같은 Play/build 복사본 주입 방식으로 연결한다. 저장된 씬의 위치·계층·컴포넌트·참조를 덮어쓰지 않았다.
- 생성된 로컬 프로필이 없는 환경에서는 `Tools/Battlefield/Animations/Mixamo/Import Grenade And Mantle`로 준비한다. `LocalAssets/Mixamo/Throw Grenade.fbx`, `Braced Hang To Crouch.fbx`가 필요하다. MixamoLocal은 기존 Git 제외 방침을 유지한다.

## 변경 전 비교와 보존

이전 작업의 CodexSlideRecovery/applied.json, CodexClimbReview/source-before.json 및 이번 before.json과 비교했다. 관련 기존 파일·사용자 원본의 중간 변경은 없었다. 모든 작업은 격리 프로젝트에서 먼저 수행했다. 카메라 값·캡슐·이동 속도·기존 슬라이드 .75초/endSpeed4·포복 속도/전환값·원본 FBX·AudioSource/오디오 프로필을 변경하지 않았다. CombatTestScene 등 새 씬도 만들지 않았다.

## 새 스크립트와 책임

| 파일 | 담당 기능·코드 |
|---|---|
| PlayerCharacter/Scripts/IPlayerCharacterThrowInput.cs | ThrowPressed/ThrowHeld만 노출하는 투척 입력 계약. 원시 키를 게임 로직에 넣지 않는다. |
| Weapon/Scrips/GrenadeDefinition.cs | 투사체·원본 클립·신관·피해·반경·속도·방출 시점·1인칭 표시 오프셋 데이터. |
| Projectile/Scripts/GrenadeProjectile.cs | Arm/FixedUpdate/Detonate가 물리 이동·신관·일회 폭발을 소유. IDamageable별 최대 피해 한 번, 팀·소유자 제외, 콜라이더 가림 검사. IExplosionEffectPlayer를 주입받아 기존 풀 사용. |
| PlayerCharacter/Scripts/PlayerCharacterGrenadeThrow.cs | TryThrow/Tick/Release/Cancel, 수량, 조준 유지, 한 번 방출과 취소. 초기화/빌드 Awake에서 명시적 참조 복구. 사망·비활성화 시 미방출 물체 제거. 실제 리스폰 때만 수량 보충. |
| PlayerCharacter/Scripts/MantleDefinition.cs | 원본 클립, 높이 비율·접근 거리·진입 시간 데이터. |
| PlayerCharacter/Scripts/PlayerCharacterMantle.cs | TryBegin에서 벽·윗면·착지 공간을 검사. Step만 CharacterController.Move를 호출하며, 상승 후 수평 진입. 옆/위 충돌·비활성화·사망 중단. 정상 종료 시 기존 TrySetCrouched(true). |
| PlayerCharacter/Scripts/TacticalAnimationSet.cs | 고유 placeholder 두 개를 원본으로 매핑. 기존 런타임 AOC의 해당 슬롯을 읽고 적용/복구한다. |
| PlayerCharacter/Scripts/PlayerCharacterTacticalAnimationView.cs | MantleTime/ThrowTime, 원본 자세 블렌딩, 투척 상체 레이어. 장비/슬라이드의 disposable AOC 참조를 바꾸지 않고 자기 슬롯만 적용한다. 자산 AOC에는 쓰지 않는다. 임시 중단 시 매핑을 유지하고 파괴 시 자기 변경만 복구한다. 기존 접지 데이터를 재사용해 전신 표시 높이만 맞춘다. |
| PlayerCharacter/Editor/TacticalActionImporter.cs | 원본 검증/임포트, 고유 CC0 placeholder, 임시 수류탄 프리팹·재질·물리 재질과 데이터, Animator 추가분을 한 메뉴로 준비한다. 기존 설정이 있으면 보존한다. |
| PlayerCharacter/Editor/LocalTacticalActionSetup.cs | Play 및 빌드용 씬 복사본에 gameplay2+presentation1 컴포넌트와 직렬화된 의존성을 연결. 기존 on-foot 중단 목록에3개 추가. 기존 참조를 제거하지 않는다. |
| Tests/PlayMode/GrenadeProjectileTests.cs | 실제 아군 DummyShooter를 소유자로 적·중립·아군·엄폐·신관·프리팹 바운스 검증2개. 장비 연결 전에 먼저 통과시켰다. |
| Tests/PlayMode/TacticalActionTests.cs | 장비별 올라가기·접지·착지/높이/공간 거부·중단복귀·모션 되감김 방지·조준 유지·방출/수량·벽 앞 차단·1인칭 표시·직렬화 후 Awake 재구성 검사5개. |

위 경로는 Assets/Features 아래이며 Tests는 Assets/Tests다. 모든 변경 파일은 끝의 표에 절대 기준인 저장소 상대 경로로 각각 기재했다.

## 기존 코드의 교체·추가 구간

### 이동과 행동 정책

PlayerCharacterMovement.Tick의 기존 분기:

```csharp
if (_traversal != null && _traversal.Step(jump, deltaTime))
```

아래처럼 새 이동을 우선 처리하되, 기존 이동 처리 본문과 낮은 장애물/슬라이드 구현은 유지했다.

```csharp
if ((_mantle != null && _mantle.Step(jump, deltaTime)) ||
    (_traversal != null && _traversal.Step(jump, deltaTime)))
```

새 `_mantle` 직렬화 참조/SetMantle를 추가하고 Stop에서도 Cancel한다. 물리 이동이 두 번 적용되지 않는다.

PlayerCharacterActionPolicy에는 `_mantle`, `_grenade` 및 SetTacticalActions를 추가했다. CanJump/CanAttack의 기존 조건 앞에 `!TacticalBusy`를 추가했다. CanChangeStance는 살아 있고 특수 행동 중이 아닐 때만 허용한다. CanStartTacticalAction은 추가로 슬라이드·근접·장전 중 시작을 거부한다. PlayerCharacterStance.Update의 `!IsAlive` 검사만 `!CanChangeStance`로 교체했으며, 직접 자세 설정/충돌 검사와 기존 모든 값은 유지했다.

PlayerCharacterAnimationView에는 새 직렬화 참조/SetMantle가 추가됐다. 올라가기 중 시각적 Grounded=true, TraversalMode=5로 기존 점프 자동 전환의 간섭을 막는다. 실제 물리 IsGrounded는 변경하지 않는다. 나머지 mode0~4와 슬라이드 보간 코드는 유지했다.

### 무기·1인칭 표시

- PlayerCharacterCombatView: 새 선택적 `_tactical`/SetTacticalPresentation. 기존 무기 숨김은 CanAttack 정책을 따르며, FPS 팔 표시는 `presenting || tactical.IsPresenting`으로 확장했다. 그래서 원본 동작의 빈손은 보이고 총은 숨겨진다.
- PlayerCharacterWeaponAimView: 새 선택적 참조. 원본 특수 동작 동안 OnAnimatorIK의 손 IK/LookAt 가중치를0으로 만들고 LateUpdate의 기존 무기 보정을 건너뛴다. 정상 이동·조준·발사·장전 경로는 유지했다.
- FirstPersonArmsView: 새 선택적 참조. 특수 동작에는 기존 맨손 메시/캐릭터 기준 복사를 사용하며, 복사 후 수류탄을 표시용 오른손에 연결한다. 수류탄에만 새 전용 표시 오프셋을 적용한다. 기존 권총·소총·맨손 오프셋은 그대로다.
- PlayerCombatHud: 새 선택적 수류탄/올라가기 참조와 setter. 기존 표시를 유지하면서 CLIMB/THROW/GRENADE AIM, `4 GRENADE 남은수`를 추가했다.
- PlayerCharacterInput: IPlayerCharacterThrowInput 구현, 선택적 ThrowGrenade 액션 조회, HasControl을 따르는 Pressed/Held. 기존 입력 처리와 키는 제거하지 않았다.
- PlayerCharacterCombatInputTests: 기존 G 근접이 투척을 발생시키지 않는지, 새4 입력/차량 제어 중 차단을 추가 검사한다. 기존 검증을 완화하지 않았다.

### Animator 및 모션

Main PlayerCharacter.controller와 기존 PistolLocomotion/RifleLocomotion.controller 각각에만 다음을 추가했다.

- Base Layer.Mantle: 고유 MantlePlaceholder, timeParameter=MantleTime.
- GrenadeUpperBody: 기존 UpperCombat 마스크 재사용, Override, defaultWeight0, Throw 상태/ThrowTime. 기존 레이어 순서를 바꾸지 않고 마지막에 추가.
- float MantleTime/ThrowTime, 기본0. 기존 파라미터/상태/전이 삭제 없음.

실제 원본은 새 로컬 TacticalActions.asset의 매핑으로 연결한다. 원본 팔다리 키는 수정하지 않았다. FBX는 Humanoid, full take, loop=false, 압축 Off, 원래 방향 보존/회전 bake, Y는 feet 기준 추출, XZ 이동 추출이며 applyRootMotion=false다. 이동 권한은 모터가 유지한다. 올라가기 종료 후 MantleTime을0으로 되감지 않고 마지막 자세로 블렌딩한다. 0.12초 복귀 중 무기 보정을 계속 억제하며, 일시 중단 후 Animator가 Mantle에 남아도 기존 이동 상태로 복귀한다.

## 새 설정·자산 값

| 대상 | 새 값·연결 |
|---|---|
| InputSystem_Actions.inputactions | PlayerCharacter.ThrowGrenade/Button, `<Keyboard>/4`, Keyboard&Mouse. 기존 액션/바인딩 값은 동일. |
| 수류탄 컴포넌트 | capacity3, 방출 때만 소모. 반쯤 진행한 원본 준비 자세에서 키 유지 시 시간 정지, 놓으면 계속 재생. 쿠킹 없음. |
| FragGrenade.asset | Throw Grenade 원본0~105프레임/3.5초; fuse3s, damage100, radius6m, launchSpeed18m/s, upwardSpeed4m/s, releaseFraction0.7, firstPersonOffset(-0.35,0.6,1.2). |
| 폭발 정책 | 거리 선형 감쇠, 소유자/아군 제외, 적/중립 대상, 엄폐 차단, 여러 콜라이더도 수신자별 한 번. 기존 IDamageable/Team을 재사용한다. |
| FragGrenade.prefab | root Rigidbody(처음 kinematic), SphereCollider radius0.12, 새 스크립트; Visual 캡슐 scale(0.2,0.14,0.2), 새 재질 연결. |
| FragGrenade.mat | 올리브색 RGB(0.22,0.28,0.1)의 테스트용 표시 재질. |
| FragGrenade.physicMaterial | bounciness0.35, dynamicFriction0.4, staticFriction0.5. 방출 시 gravity/ContinuousDynamic/Interpolate 활성화, angularVelocity(3,5,2), 발사 속도에 모터 속도 합산. |
| Mantle.asset | Braced Hang To Crouch 원본0~34프레임/약1.1333초. min/maxHeightFraction0.6/0.9, reach2.5m, reachDuration0.18s. 정상 종료 crouch, 표시 복귀0.12s. |
| TacticalActions.asset | 두 고유 placeholder 및 Grenade/Mantle 정의 참조. 기존 Vault의 ClimbUp 원본 슬롯은 교체하지 않는다. |

## 제거·대체 내역과 설계 검토

기존 메서드·필드·컴포넌트·참조·에셋은 삭제하지 않았다. 교체된 부분은 위에 기재한 조건식/분기/표시식이다. 이전 구현 전체를 새 구현으로 덮어쓰지 않았다. 임시 진단 스크립트는 Library로 보관하며 본 프로젝트로 옮기지 않았다.

수류탄은 충돌 때 소멸하는 ProjectileBase의 계약을 바꾸지 않도록 별도 컴포넌트로 구성했다. 데이터/입력/캐릭터 상태/물리/표시/에디터 연결의 상태 소유자를 분리했다. 입력과 폭발 표현에는 기존 좁은 계약을 사용하며, 수류탄·올라가기에 불필요한 상속이나 새 전역 관리자를 만들지 않았다. 상태는 읽기 전용 프로퍼티와 의도별 API로 노출한다. 이 작업의 의도적 SOLID 예외는 없다.

## 검증과 남은 확인

- 원본 보존 해시, 실제 Humanoid/avatar 임포트, 기존 파일 비교, 씬/입력 의미 비교.
- 아군 더미 소유자 기반2개 검사를 장비 연결 전에 통과. 최종 프리팹의 물리 바운스도 전체 검사에서 통과.
- 중간 전체 검사에서 컨트롤러 참조 복원3건 실패를 확인했고, 런타임 슬롯을 제자리 연결하는 방식으로 수정 후 관련18개가 통과했다. 기존 테스트 조건은 유지했다.
- 전체 PlayMode203/203, 실패/건너뜀0, -noaudio,193.1276094초. 이후 마지막 원본 자세 유지 수정은 신규 동작5개 집중 검사로 재확인했다.
- 1·3인칭 실제 동작 캡처, 1인칭 조준 대기 시 수류탄 중심이 viewport 안인지 검사. 단순 콘솔 컴파일만으로 완료 판정하지 않았다.
- Windows 진단 빌드/실행: 새 기능의 직렬화된 참조, 수류탄 한 번 방출/수량, 모터를 통한 올라가기/앉기 복귀를 -noaudio로 검사한다. 최종 결과는 Library/CodexGrenadeMantle/build-result.txt와 player-smoke.txt에 기록된다.
- 근거: Library/CodexGrenadeMantle/{dummy-final,lifecycle,final,pose-final}.xml/log 및 캡처. 임시 진단 코드와 실행 파일도 이 작업 폴더에만 둔다. 커밋·스테이징 없음.

사용자 Editor의 실제 지형·경사·이동 물체·다양한 카메라 각도에서 조작감은 추가 확인이 필요하다. 1인칭은 별도 전용 팔 모션이 아니라 현재 원본을 기존 표시 리그로 복사한다. 높은 턱은 지면/서기에서 진입하며 공중 매달리기·사다리 기능은 포함하지 않는다. Climbing To Top/Climbing Ladder는 원본 보관만 유지했다. 수류탄 모델·수치·시간은 이번 기능의 초기 테스트 설정이다.


### 최종 빌드 근거와 별도 관찰

최종 Windows 진단 빌드: Succeeded, errors0/warnings0,16.5373383초. 이후 -batchmode -noaudio -nographics Player에서 수류탄 방출/수량과 높은 턱 이동/앉기 복귀 PASS를 기록했다. 이 실행은 물리·직렬화 검증이며 화면 품질을 검증한 것은 아니다. 화면/자세 확인은 Editor 캡처와 PlayMode 검사로 수행했다.

Player 로그에는 수정하지 않은 FighterMeshCollider.Awake(24/28) 경로에서 `Main_LOD0_00_low.004` 메시가 non-accessible이라 CollisionMeshData를 생성하지 못하는 메시지가 별도로 기록됐다. 새 보병 기능 검사는 통과했으나 전투기 런타임 메시 충돌 생성은 이번 범위에 포함하지 않았고 해결하지 않았다. 빌드 성공을 프로젝트 전체의 런타임 오류 없음으로 해석하면 안 된다.


## 변경 파일 전체

각 `.meta`도 개별 표기했다. 기존 씬·프리팹·원본 FBX와 이전 슬라이드 설정은 보존했다.

| 구분 | 파일 | 변경 내용 |
|---|---|---|
| 수정 | `Assets/Features/PlayerCharacter/Animation/PlayerCharacter.controller` | 아래 구현·설정 설명 참조 |
| 추가 | `Assets/Features/PlayerCharacter/Animation/Tactical.meta` | Unity 생성 GUID `14c24e9e334322b49979909ea6195197`; 신규 리소스 연결/임포트 정보 |
| 추가 | `Assets/Features/PlayerCharacter/Animation/Tactical/MantlePlaceholder.anim` | 아래 구현·설정 설명 참조 |
| 추가 | `Assets/Features/PlayerCharacter/Animation/Tactical/MantlePlaceholder.anim.meta` | Unity 생성 GUID `e904b93605a60f942859aa1ce92e118f`; 신규 리소스 연결/임포트 정보 |
| 추가 | `Assets/Features/PlayerCharacter/Animation/Tactical/ThrowPlaceholder.anim` | 아래 구현·설정 설명 참조 |
| 추가 | `Assets/Features/PlayerCharacter/Animation/Tactical/ThrowPlaceholder.anim.meta` | Unity 생성 GUID `f1ab5a2949d22ac418fb6158d8e5da8f`; 신규 리소스 연결/임포트 정보 |
| 추가 | `Assets/Features/PlayerCharacter/Editor/LocalTacticalActionSetup.cs` | 아래 구현·설정 설명 참조 |
| 추가 | `Assets/Features/PlayerCharacter/Editor/LocalTacticalActionSetup.cs.meta` | Unity 생성 GUID `c4fe258af063aa1489e733d8bc5ea033`; 신규 리소스 연결/임포트 정보 |
| 추가 | `Assets/Features/PlayerCharacter/Editor/TacticalActionImporter.cs` | 아래 구현·설정 설명 참조 |
| 추가 | `Assets/Features/PlayerCharacter/Editor/TacticalActionImporter.cs.meta` | Unity 생성 GUID `711a53d7518824b42a45ec91bccf8bf7`; 신규 리소스 연결/임포트 정보 |
| 수정 | `Assets/Features/PlayerCharacter/Scripts/FirstPersonArmsView.cs` | 아래 구현·설정 설명 참조 |
| 추가 | `Assets/Features/PlayerCharacter/Scripts/IPlayerCharacterThrowInput.cs` | 아래 구현·설정 설명 참조 |
| 추가 | `Assets/Features/PlayerCharacter/Scripts/IPlayerCharacterThrowInput.cs.meta` | Unity 생성 GUID `b9542272176b8384baa829c4cff1ec03`; 신규 리소스 연결/임포트 정보 |
| 추가 | `Assets/Features/PlayerCharacter/Scripts/MantleDefinition.cs` | 아래 구현·설정 설명 참조 |
| 추가 | `Assets/Features/PlayerCharacter/Scripts/MantleDefinition.cs.meta` | Unity 생성 GUID `ca1174e9922ab544a88343d2c567c11e`; 신규 리소스 연결/임포트 정보 |
| 수정 | `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterActionPolicy.cs` | 아래 구현·설정 설명 참조 |
| 수정 | `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterAnimationView.cs` | 아래 구현·설정 설명 참조 |
| 수정 | `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterCombatView.cs` | 아래 구현·설정 설명 참조 |
| 추가 | `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterGrenadeThrow.cs` | 아래 구현·설정 설명 참조 |
| 추가 | `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterGrenadeThrow.cs.meta` | Unity 생성 GUID `36bf80d647fe1814887dcaaf9c91e4b0`; 신규 리소스 연결/임포트 정보 |
| 수정 | `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterInput.cs` | 아래 구현·설정 설명 참조 |
| 추가 | `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterMantle.cs` | 아래 구현·설정 설명 참조 |
| 추가 | `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterMantle.cs.meta` | Unity 생성 GUID `b169c49df41c0454e9387eeafa14ea5f`; 신규 리소스 연결/임포트 정보 |
| 수정 | `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterMovement.cs` | 아래 구현·설정 설명 참조 |
| 수정 | `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterStance.cs` | 아래 구현·설정 설명 참조 |
| 추가 | `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterTacticalAnimationView.cs` | 아래 구현·설정 설명 참조 |
| 추가 | `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterTacticalAnimationView.cs.meta` | Unity 생성 GUID `3a406a32779fd9b40a95b51b7004813a`; 신규 리소스 연결/임포트 정보 |
| 수정 | `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterWeaponAimView.cs` | 아래 구현·설정 설명 참조 |
| 추가 | `Assets/Features/PlayerCharacter/Scripts/TacticalAnimationSet.cs` | 아래 구현·설정 설명 참조 |
| 추가 | `Assets/Features/PlayerCharacter/Scripts/TacticalAnimationSet.cs.meta` | Unity 생성 GUID `cece7f26add48194cbb8ebb3a2f606e0`; 신규 리소스 연결/임포트 정보 |
| 추가 | `Assets/Features/Projectile/Prefabs/FragGrenade.mat` | 아래 구현·설정 설명 참조 |
| 추가 | `Assets/Features/Projectile/Prefabs/FragGrenade.mat.meta` | Unity 생성 GUID `cae81652f4c8a8b438fd83f42c63756f`; 신규 리소스 연결/임포트 정보 |
| 추가 | `Assets/Features/Projectile/Prefabs/FragGrenade.physicMaterial` | 아래 구현·설정 설명 참조 |
| 추가 | `Assets/Features/Projectile/Prefabs/FragGrenade.physicMaterial.meta` | Unity 생성 GUID `9c3151e630fef834abc94bf4d2ca8af9`; 신규 리소스 연결/임포트 정보 |
| 추가 | `Assets/Features/Projectile/Prefabs/FragGrenade.prefab` | 아래 구현·설정 설명 참조 |
| 추가 | `Assets/Features/Projectile/Prefabs/FragGrenade.prefab.meta` | Unity 생성 GUID `36cc37109a3a821439a8e449a9da03a9`; 신규 리소스 연결/임포트 정보 |
| 추가 | `Assets/Features/Projectile/Scripts/GrenadeProjectile.cs` | 아래 구현·설정 설명 참조 |
| 추가 | `Assets/Features/Projectile/Scripts/GrenadeProjectile.cs.meta` | Unity 생성 GUID `56971296122b9454e897541d9114e7d4`; 신규 리소스 연결/임포트 정보 |
| 수정 | `Assets/Features/UI/PlayerCombatHud.cs` | 아래 구현·설정 설명 참조 |
| 추가 | `Assets/Features/Weapon/Scrips/GrenadeDefinition.cs` | 아래 구현·설정 설명 참조 |
| 추가 | `Assets/Features/Weapon/Scrips/GrenadeDefinition.cs.meta` | Unity 생성 GUID `3aa8e154c3474aa4a90ad37da8df4174`; 신규 리소스 연결/임포트 정보 |
| 수정 | `Assets/InputSystem_Actions.inputactions` | 아래 구현·설정 설명 참조 |
| 추가 | `Assets/Tests/PlayMode/GrenadeProjectileTests.cs` | 아래 구현·설정 설명 참조 |
| 추가 | `Assets/Tests/PlayMode/GrenadeProjectileTests.cs.meta` | Unity 생성 GUID `e9c1a38c9829b6740bd35378ea9f5a8f`; 신규 리소스 연결/임포트 정보 |
| 수정 | `Assets/Tests/PlayMode/PlayerCharacterCombatInputTests.cs` | 아래 구현·설정 설명 참조 |
| 추가 | `Assets/Tests/PlayMode/TacticalActionTests.cs` | 아래 구현·설정 설명 참조 |
| 추가 | `Assets/Tests/PlayMode/TacticalActionTests.cs.meta` | Unity 생성 GUID `af870355a6180d944962174ea763bf38`; 신규 리소스 연결/임포트 정보 |
| 수정 | `Assets/ThirdParty/MixamoLocal/ArmedLocomotion/PistolLocomotion.controller` | 아래 구현·설정 설명 참조 |
| 수정 | `Assets/ThirdParty/MixamoLocal/ArmedLocomotion/RifleLocomotion.controller` | 아래 구현·설정 설명 참조 |
| 추가 | `Assets/ThirdParty/MixamoLocal/TacticalActions.meta` | Unity 생성 GUID `afd647f72e9998f4fa880db383d17642`; 신규 리소스 연결/임포트 정보 |
| 추가 | `Assets/ThirdParty/MixamoLocal/TacticalActions/Braced Hang To Crouch.fbx` | 아래 구현·설정 설명 참조 |
| 추가 | `Assets/ThirdParty/MixamoLocal/TacticalActions/Braced Hang To Crouch.fbx.meta` | Unity 생성 GUID `a963a391d2ff41e45b49b9105c092f38`; 신규 리소스 연결/임포트 정보 |
| 추가 | `Assets/ThirdParty/MixamoLocal/TacticalActions/FragGrenade.asset` | 아래 구현·설정 설명 참조 |
| 추가 | `Assets/ThirdParty/MixamoLocal/TacticalActions/FragGrenade.asset.meta` | Unity 생성 GUID `8f26bd481103828449a47046f0d34167`; 신규 리소스 연결/임포트 정보 |
| 추가 | `Assets/ThirdParty/MixamoLocal/TacticalActions/Mantle.asset` | 아래 구현·설정 설명 참조 |
| 추가 | `Assets/ThirdParty/MixamoLocal/TacticalActions/Mantle.asset.meta` | Unity 생성 GUID `1c59064da168030468a072fe9100f0a7`; 신규 리소스 연결/임포트 정보 |
| 추가 | `Assets/ThirdParty/MixamoLocal/TacticalActions/TacticalActions.asset` | 아래 구현·설정 설명 참조 |
| 추가 | `Assets/ThirdParty/MixamoLocal/TacticalActions/TacticalActions.asset.meta` | Unity 생성 GUID `b54cb7c677aadd346bcb9ff176abd650`; 신규 리소스 연결/임포트 정보 |
| 추가 | `Assets/ThirdParty/MixamoLocal/TacticalActions/Throw Grenade.fbx` | 아래 구현·설정 설명 참조 |
| 추가 | `Assets/ThirdParty/MixamoLocal/TacticalActions/Throw Grenade.fbx.meta` | Unity 생성 GUID `65c01309261c7b642919eef9a34f9cd8`; 신규 리소스 연결/임포트 정보 |
| 추가 | `Docs/AI/GrenadeAndMantleChanges.md` | 구현·수치·교체 구간·검증 및 전체 파일 목록 |
| 수정 | `Docs/AI/UnityProjectContext.md` | 새 기능, 구성과 검증 근거 기록 |
