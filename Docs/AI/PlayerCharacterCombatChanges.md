# 캐릭터 권총·근접 공격 구현 — 2026-10-27

권총 모델, 실제 탄창·장전, 주먹 공격, 권총을 쥔 손으로 타격하는 근접 공격, 각 동작의 휴머노이드 애니메이션을 SampleScene의 기존 플레이어에 연결했다.

## 조작과 새 설정

| 조작 | 동작 |
|---|---|
| 1 / 2 | 권총 / 맨손 선택 |
| 좌클릭 | 권총 발사 / 맨손 주먹 공격 |
| R | 권총 장전 |
| G | 현재 장비의 근접 공격 |
| V / Alt / 우클릭 / 왼쪽 Ctrl / F | 기존 시점 전환·자유 시점·견착/ADS·앉기·탑승/하차 |

새 탄창: 15발, 예비탄 90발, 장전 1.666667초. 주먹 피해 20, 권총 근접 피해 30. 각 근접 공격은 거리 2.8, 판정 반지름 0.45, 선행 0.2초, 회복 0.6초이며 한 동작에 한 번만 판정한다. 가장 가까운 벽/아군이 공격을 막고 아군에게는 피해를 주지 않는다. 장전 중에는 발사·근접 공격이 차단되고, 장비 전환·탑승·컨트롤러 비활성화·사망은 대기 중인 공격/장전을 취소한다. 취소한 장전은 예비탄을 소모하지 않는다.

발사율 10발/초, 탄환 피해 10, 기존 BulletPool·ProjectileData·물리 탄환·재질별 피격 처리·카메라 조준/엄폐물 검사는 유지했다. 현재 권총은 이전 연속 발사 입력을 그대로 사용한다. 이동 속도 8/12, 캐릭터 높이 4, 앉기 높이 2.5/속도 4, 카메라 설정, 모델 위치/크기, 전투기/더미 위치와 사용자 구조는 바꾸지 않았다.

## 작업 전 비교와 보존

Temp/CombatBefore에 작업 직전 원본을 보관했다. 씬, 캐릭터 Controller/Input/Visibility, Animator Controller, Input Actions를 이전 검증 사본과 비교했다. 관련 기준 상태에 충돌하는 새 사용자 변경은 없었다. 반영 시에도 씬과 Animator Controller의 기존 직렬화 블록을 비교하며 필요한 부분만 패치한다. 기존 씬 오브젝트·컴포넌트·참조·필드는 삭제하지 않는다.

## 변경한 스크립트

아래 경로는 모두 E:/GitHub/Battlefield_Project 기준이다. 새 .cs 파일에는 같은 이름의 .meta를 추가했다. 기존 스크립트 GUID는 유지한다.

| 파일 | 코드·책임·변경 이유 |
|---|---|
| Assets/Features/PlayerCharacter/Scripts/PlayerCharacterController.cs | 기존 Update의 무제한 TryFire 호출을 장비 선택·탄창·공격 상태를 확인하는 흐름으로 교체. _magazine/_fistAttack/_pistolAttack 참조, IsArmed/CanUseCombat/IsAttacking, BindMuzzle, Fired/EquipmentChanged/MeleeStarted를 추가. Equip/TryMelee/CancelActions/OnDisable 추가. TryFire는 성공한 탄환에만 탄약·주기를 소모하며 실제 Muzzle까지의 경로에 벽이 있으면 그 앞에서 발사한다. 기존 FindNearestSurface와 조준 인터페이스/필드 유지. |
| Assets/Features/PlayerCharacter/Scripts/PlayerCharacterInput.cs | IPlayerCharacterCombatInput 구현, Reload/Melee/SelectPistol/SelectUnarmed 액션 연결. WeaponSlotSelection의 상수 0을 실제 1/2 입력으로 교체. 모두 기존 HasControl을 따르므로 탑승/커서 해제 시 보행 공격 입력이 나오지 않는다. |
| Assets/Features/PlayerCharacter/Scripts/IPlayerCharacterCombatInput.cs | ReloadPressed/MeleePressed만 제공하는 작은 입력 계약. 기존 이동/시점 입력에 전투 세부 입력을 섞지 않는다. |
| Assets/Features/Weapon/Scrips/WeaponMagazine.cs | 탄창/예비탄/장전 시간의 유일한 상태 소유자. TryConsumeRound/TryReload/CancelReload와 읽기 전용 상태, ReloadStarted/ReloadCompleted 이벤트. 부분 장전·빈 탄창·예비탄 부족·중단 처리. |
| Assets/Features/Weapon/Scrips/MeleeAttack.cs | 주먹과 권총 타격이 같은 판정 코드를 별도 설정으로 재사용. 소유자 로컬 기준의 공격 위치/방향 저장, 선행 시간 뒤 NonAlloc 겹침/구체 투사로 가장 가까운 충돌 판정, 팀/IDamageable 적용, 회복·취소·사망 처리. 결과 버퍼가 가득 차면 불확실한 관통 판정을 내지 않는다. |
| Assets/Features/Weapon/Scrips/WeaponModel.cs | 프리팹이 Muzzle을 명시적으로 제공. 런타임 문자열 검색으로 총구를 찾지 않는다. |
| Assets/Features/PlayerCharacter/Scripts/PlayerCharacterCombatView.cs | 오른손에 권총 프리팹 생성, 총구 연결, 전투 이벤트에 맞는 Animator 상태·간단한 총 반동·표시 여부 처리. 1인칭/3인칭 손 리그 전환. 탑승 시 구독 해제·표시 중단, 하차 시 복구. 애니메이션이 피해/탄약을 결정하지 않는다. |
| Assets/Features/PlayerCharacter/Scripts/FirstPersonArmsView.cs | 기존 애니메이션의 UpperChest 기준 뼈 자세를 독립 표시용 뼈로 복사. 보정된 표시 앵커는 기본 (-0.189851,-0.922501,-0.165491), ADS (-0.339851,-0.672501,-0.165491), 기준 가슴 회전 (10.811870,38.512090,0.259070). 눈 위치/AimRotation에 매핑해 기본 총구가 눈 기준 (0.15,-0.25,1.4), ADS는 (0,0,1.4)가 되도록 보정한다. 캐릭터 루트 배율을 적용하며 카메라와 월드 캐릭터를 이동하지 않는다. 생성 뼈는 수명 종료 시 정리한다. |
| Assets/Features/UI/PlayerCombatHud.cs | 기존 Canvas의 TMP 텍스트에 PISTOL 탄창/예비탄, RELOADING 또는 FISTS를 표시. 탑승/사망 시 숨긴다. |
| Assets/Tests/PlayMode/PersonalCombatMechanicsTests.cs | 장전·예비탄·취소와 더미 적/중립/아군·벽·단일 타격 판정 3개. 장비 통합 전 먼저 3/3 통과. |
| Assets/Tests/PlayMode/PlayerCharacterCombatTests.cs | 실제 총구·손 장착·탄약 소비/장전 차단·장비 전환·FPS 표시, 상하 조준/ADS/앉기, 20/30 피해, 동작 상태, 탑승 취소·하차 복구. |
| Assets/Tests/PlayMode/PlayerCharacterCombatInputTests.cs | 실제 Input System의 1/2/R/G 바인딩과 탑승 상태 입력 차단. |

발사 흐름 변경의 핵심:

```csharp
// 이전: 탄약 제한 없이 캡슐 앞 계산 위치에서 발사
if (_input.FireWeapon) TryFire();

// 이후: 장비/공격/탄창 확인 후 실제 총구 방향으로 발사
if (IsArmed) TryFire(); else TryMelee();
// TryFire 내부: CanFire 확인 → 조준/엄폐 검사 → BulletPool.Get()
// → TryConsumeRound() → Initialize(owner, damage) → Fired 이벤트
```

## 변경·추가한 씬과 에셋

| 파일 | 구체적인 변경 |
|---|---|
| Assets/InputSystem_Actions.inputactions | PlayerCharacter 맵에 Reload=R, Melee=G, SelectPistol=1, SelectUnarmed=2만 추가. 기존 액션 ID·바인딩 및 Fighter 맵 유지. |
| Assets/Scenes/SampleScene.unity | 기존 PlayerCharacter 아래 PersonalCombat/WeaponMagazine, Fists/MeleeAttack(20), PistolMelee/MeleeAttack(30) 추가. 기존 Controller의 세 참조 연결. Armature에 PlayerCharacterCombatView 추가, 기존 Geometry 아래 FirstPersonArms 렌더러/FirstPersonArmsView 추가. 기존 on-foot 목록에 CombatView 추가. 기존 VehicleInteractionUI Canvas 아래 CombatStatus 텍스트와 PlayerCombatHud 추가. 기존 오브젝트 위치·크기·컴포넌트 설정은 유지. |
| Assets/Features/PlayerCharacter/Animation/PlayerCharacter.controller | CombatPitch(float)와 UpperCombat Override 레이어 추가. PistolHold(Down/Neutral/Up 블렌드), PistolShoot, PistolReload, PunchJab, PunchCross, PistolMelee, FistsHold 상태. 기존 Base Layer/전환/앉기 모션 유지. 발사 모션 속도 6.333334로 기존 0.1초 발사 주기에 맞추고, 타격은 0.8초로 재생. |
| Assets/Features/PlayerCharacter/Animation/UpperCombat.mask | 양팔·손가락만 전투 포즈 적용. Head도 제외해 기존 이동/앉기 애니메이션의 머리 방향을 유지한다. Root·Body(골반/척추)·다리 제외는 유지해 앉기 높이와 몸통 방향을 보존. 아래 머리 방향 수정 기록 참조. |
| Assets/Features/PlayerCharacter/Models/FirstPersonArms.asset | 기존 Starter Assets 메시에서 아래팔·손 삼각형만 추출한 파생 메시(22,620 삼각형). 원본 모델/메시/재질은 변경하지 않음. 1인칭에서 어깨가 화면을 가리는 것을 방지. |
| Assets/Features/Weapon/Prefabs/PrototypePistol.prefab | WeaponModel, Geometry, Muzzle 구성. 원본 FBX의 축 보정 후 +Z 방향/길이 0.65로 배치. 손잡이 로컬 위치 (0.012945553,-0.03099503,0.020586709), 회전 (77.80909,47.97457,202.09026)는 캐릭터 전투 표현 컴포넌트에 저장. 런타임 부모 배율을 상쇄해 총 크기 유지. |
| Assets/Features/Weapon/Models/PrototypePistol/Black.mat, Black2.mat, DarkMetal.mat, DarkWood.mat, Metal.mat | 원본 모델의 다섯 색상을 유지한 URP Lit 재질. Metallic은 금속 0.7/그 외 0.1, Smoothness 0.35. 기존 프로젝트 재질 교체 없음. |
| Assets/ThirdParty/Quaternius/Guns/Pistol_1.fbx | 공식 CC0 권총 모델 한 개만 추가(36,428바이트). |
| Assets/ThirdParty/Quaternius/Guns/License.txt, SOURCE.md | 공식 라이선스 원문, 다운로드 출처/해시/변환 이력 추가. |
| Assets/ThirdParty/Quaternius/Animations/PistolHoldDown.anim, PistolHoldNeutral.anim, PistolHoldUp.anim | 실제 Humanoid 조준 포즈. 기존과 같은 RootQ/RootT 180도 축 보정. |
| Assets/ThirdParty/Quaternius/Animations/PistolShoot.anim, PistolReload.anim | 발사 0.6333334초/장전 1.666667초 원본 모션 추출. 발사 재생 속도만 기존 주기에 맞춤. |
| Assets/ThirdParty/Quaternius/Animations/PunchJab.anim, PunchCross.anim | 주먹 0.8666667초/1초 원본 모션 추출. 두 주먹 번갈아 재생. PunchCross를 권총을 쥔 타격에도 재사용. |
| Assets/ThirdParty/Quaternius/SOURCE.md | 기존 앉기 출처를 보존하고 위 7개 추출 클립과 사용 방식을 추가 기록. |
| Docs/AI/UnityProjectContext.md | 캐릭터 전투 구조와 검증 결과 추가. |
| Docs/AI/PlayerCharacterCombatChanges.md | 이 변경 보고서. |

위 신규 에셋의 동일 경로 .meta를 함께 추가해 참조 GUID를 고정한다. 신규 폴더 메타는 Assets/Features/PlayerCharacter/Models.meta, Assets/Features/Weapon/Models.meta, Assets/Features/Weapon/Models/PrototypePistol.meta, Assets/ThirdParty/Quaternius/Guns.meta이다. 기존 씬·Controller·Input Actions·소스 문서 메타는 변경하지 않는다. 외부 애니메이션 원본 24MB FBX와 다운로드 ZIP, 검증 Editor 스크립트는 Temp에만 있고 제품 Assets에는 넣지 않았다.

## 교체·제거 내역

기존 메서드·필드·컴포넌트·참조·에셋 삭제 없음. PlayerCharacterController.Update/TryFire의 무제한 발사 흐름과 가상 총구 우선 계산, PlayerCharacterInput.WeaponSlotSelection의 상수 반환만 위 새 동작으로 교체했다. 가상 총구는 모델이 없는 기존 테스트/사용처를 위한 fallback으로 남겼다. 애니메이션 레이어는 추가이며 이전 이동/앉기 Controller는 대체하지 않았다. 작업 중 생성한 새 FPS 메시에서 어깨/위팔 삼각형을 제외했으며 원본 Starter Assets 메시는 그대로다.

## 설계 점검

게임플레이(Controller), 탄창(WeaponMagazine), 근접 판정(MeleeAttack), 입력(Input), 월드 전투 표현(CombatView), 1인칭 표시(FirstPersonArmsView), UI(Hud)를 분리했다. 상태는 private 필드/읽기 전용 속성으로 소유하며 입력 인터페이스와 기존 IDamageable/IPlayerCharacterAimSource를 사용한다. 상속 계층을 추가하지 않았다. 주먹/권총 판정 차이는 같은 컴포넌트의 설정으로 조합한다. 이벤트 구독/해제와 생성 오브젝트 수명을 대응시켰다.

현재 요구가 권총/맨손 두 모드라 Controller의 IsArmed와 두 명시적 공격 참조를 유지한다. 무기 종류 전체를 위한 상속·등록 시스템은 도입하지 않았다. 단일 구현인 Unity 컴포넌트는 명시적 직렬화 참조를 사용하며 불필요한 인터페이스를 만들지 않았다. 이는 SOLID를 명목상 맞추기 위한 계층 추가를 피한 의도적인 선택이다.

## 검증

검증은 원본 Editor의 저장되지 않은 상태를 건드리지 않는 Temp/CodexBoardingValidation에서 진행했다.

- 통합 전 더미 테스트: Temp/codex-combat-dummy-final.xml — 3/3 통과. 기존 아군 더미 슈터가 붙은 플레이어를 소유자로 적·중립·아군, 벽·취소·단일 피해를 확인했다.
- 최종 제품 PlayMode: Temp/codex-personal-combat-production.xml — 63/63 통과, 26.57초. 기존 54개와 새 전투 9개 포함. 원본 FBX 없이 추출 클립만 사용했다.
- 렌더 검증: Temp/codex-personal-combat-calibrated.xml — 제품 63개 + 임시 화면 캡처 1개, 64/64 통과. 3인칭/정면/1인칭/발사/장전/맨손/타격/앉기 PNG를 확인했다. 총구 투영, 상하 60도 조준, ADS 중앙, 앉기 중 화면 표시를 검사했다. 캡처 스크립트는 Temp로 옮겨 제품 테스트에 포함하지 않는다.
- 원본·검증본 스크립트/신규 에셋의 바이트 일치, 씬·Animator의 바이트 일치를 확인했다. 씬은 기존 직렬화 오브젝트 586개를 모두 보존하고 새 블록 88개를 추가했으며, 기존 6개 블록에는 새 참조/자식/컴포넌트 연결만 넣었다. Animator는 기존 21개 블록을 보존하고 상태/트리/레이어 머신 9개를 추가했으며, 기존 Controller 블록에 파라미터/레이어만 추가했다.
- 신규 메타 GUID 및 참조, 공식 모델 SHA256, Range 미사용을 점검했다. 기존 작업의 폰트·전투기·다른 에셋 변경은 덮어쓰지 않았다.

Unity Editor에서 남은 확인은 실제 마우스 조작감, 주먹/권총의 손가락 접점과 근접 타격 체감, 현재 씬 조명에서의 최종 구도다. 자동화된 PlayMode/렌더 검증을 수행했으며 별도 플레이어 빌드나 사람이 직접 플레이한 검증으로 보고하지 않는다.

자동 승인 검토가 씬·Controller 전체 파일 복사를 기존 직렬화 설정 덮어쓰기 위험으로 거부했다. 대신 기존 블록 삭제가 없음을 확인하고 부분 패치로만 반영했으며, 검증본과 최종 결과가 일치함을 다시 확인했다. 사용자 승인 대기나 남은 차단 사항은 없다. 커밋/푸시는 하지 않았다.

## 3인칭 머리가 옆을 보는 문제 수정 — 2026-10-27

사용자 증상: 3인칭에서 머리·상체가 정면을 보지 않는다. 작업 전 SampleScene, Animator Controller, UpperCombat.mask, PlayerCharacterView/CombatView/AnimationView/FirstPersonArmsView가 직전 검증본과 바이트 단위로 같음을 확인했다. 사용자 또는 Editor가 새로 변경한 관련 값/구조는 없었다.

전투 모션을 적용하지 않은 이동 자세와 전투 마스크를 비교했다. Body=false/Head=true인 기존 설정에서 권총 대기는 가슴 방향 편차 0도, 머리 편차 -33.35458도였다. Head만 false로 바꾸면 머리 편차 0.02798도, 가슴 0도였다. Body를 다시 켜는 대조 실험은 가슴 26.23888도/머리 -9.81314도로 정면을 복구하지 못했다. 원본 전투 모션이 전제한 몸통 자세를 제외하면서 머리/목 회전만 덮어쓴 것이 원인이다.

변경 파일과 값:

- Assets/Features/PlayerCharacter/Animation/UpperCombat.mask: AvatarMaskBodyPart.Head true→false. 직렬화 m_Mask의 세 번째 32비트 값만 01000000→00000000. 기존 팔/손가락 적용과 Root/Body/다리 제외는 그대로다. 전투 클립의 머리/목 회전 적용을 제거하고 기존 이동/앉기 머리 모션을 유지한다. 모션 에셋 자체, 씬, 모델/루트 회전, 카메라, 입력, 무기 수치는 교체하거나 삭제하지 않았다. 메타/GUID 유지.
- Assets/Tests/PlayMode/PlayerCharacterCombatTests.cs: CombatLayerPreservesLocomotionHeadAndTorsoFacingInThirdPerson 추가. 캐릭터를 Y=53도로 돌린 뒤 서기/걷기/앉기/앉아 걷기의 각 기본 자세에서 권총 대기·발사·장전, 맨손 대기·잽·크로스·권총 타격 7개 모션을 적용한다. 머리/가슴의 기본 자세 대비 수평 방향 편차가 1도 미만인지, 모델 로컬 회전이 바뀌지 않았는지 실제 Animator 뼈 결과로 검사한다. 기존 메서드/검사를 제거하지 않았다.
- Docs/AI/PlayerCharacterCombatChanges.md: 마스크 설명 갱신 및 이 수정의 근거/파일/검증 기록 추가.
- Docs/AI/UnityProjectContext.md: 현재 머리 방향 책임과 검증 결과 추가.

새 책임/상태가 필요하지 않아 컴포넌트나 상속/인터페이스를 추가하지 않았다. 기존 전투 표현과 이동 애니메이션의 책임을 마스크로 분리한 작은 수정이며 OOP/SOLID 설계 예외는 없다. 이전 63개 테스트에는 머리 방향 검사가 없어 이 회귀를 놓쳤고, 이번에 실제 뼈 방향 검사를 추가했다.

수정 전 회귀 재현: Temp/codex-head-facing-before.xml에서 새 검사가 33.56132도 편차로 실패했다. 비교 계측/화면은 Temp/codex-head-facing-probe.xml/log 및 codex-head-current.png, codex-head-head-off.png에 있다.

수정 후 Temp/codex-head-facing-fixed-all.xml에서 제품 테스트 64개와 임시 렌더 캡처 검사 1개, 총 65/65가 통과했다(29.96초). 새 회귀 검사는 기본 자세 4종 × 전투 상태 7종을 확인한다. 발사·장전·근접 공격·탑승·1인칭/ADS/앉기 기존 검사도 통과했다. 실제 권총을 든 3인칭 정면/후면 및 1인칭 화면을 확인했고 새 컴파일 오류는 없었다. 진단/캡처 스크립트는 제품 Assets에서 제외해 Temp에만 보관한다.

원본 Editor의 저장되지 않은 상태는 건드리지 않았다. 원본 씬과 모델 회전은 수정하지 않았으며 마스크/회귀 검사 파일은 검증본과 일치한다. 사용자 Editor에서 SampleScene의 Play 모드를 재시작하여 원래 보였던 자세를 최종 확인하면 된다. 별도 플레이어 빌드는 실행하지 않았다.

## 3인칭 팔·권총 조준 방향 추가 수정 — 2026-10-27

첨부된 실제 화면에서 머리 제외 수정 후에도 팔과 총이 왼쪽으로 돌아가 있음을 확인했다. 앞선 머리 검사는 기본 이동 자세 대비 머리/가슴 방향만 확인했으므로 전체 조준이 해결됐다고 판단하기에 부족했다. 이번 작업 전 씬·전투 마스크·Animator Controller·CombatView·FirstPersonArmsView·전투 테스트가 직전 검증본과 동일했으며 새 사용자/Editor 저장 변경은 없었다.

재현 계측에서 총구는 조준 방향 대비 수평 -34.45505도, 전체 각도 35.74238도 틀어졌다. 기본 몸통을 유지한 채 다른 몸통 자세를 전제하는 팔 모션만 적용해 팔과 총구의 방향이 함께 어긋난 것이다. 모델이나 카메라 전체를 돌리는 대신, 권총을 조준하는 동안 양손에 Humanoid IK를 적용한다. Head 제외는 유지한다.

이번에 변경한 모든 제품 파일:

| 파일 | 정확한 변경과 이유 |
|---|---|
| Assets/Features/PlayerCharacter/Scripts/PlayerCharacterWeaponAimView.cs 및 .meta | 새 조준 표현 컴포넌트. Animator·전투 상태·CombatView·시점·카메라 참조를 명시적으로 받는다. OnAnimatorIK에서 조준 방향과 손잡이 회전으로 오른손 목표를 계산하고 원본 권총 대기 모션에서 추출한 양손 접점으로 왼손을 배치한다. Unity IK 회전과 실제 손 뼈 축의 차이는 알려진 기준 회전을 한 번 적용한 후 평가된 뼈로 보정한다. 조준 상태만 IK 가중치 1, 1인칭/장전/근접/비활성/사망 상태는 0이다. 기존 총 반동량을 손 목표에도 적용한다. 피해·탄약·입력·캐릭터 루트에는 쓰지 않는다. |
| Assets/Features/PlayerCharacter/Scripts/PlayerCharacterCombatView.cs | GripRotation/ RecoilAmount 읽기 전용 속성만 추가. 기존 손잡이 회전과 반동량의 중복 설정 없이 IK가 같은 값을 읽도록 한다. 기존 메서드/필드 제거 없음. |
| Assets/Features/PlayerCharacter/Scripts/PlayerCharacterCamera.cs | EyePosition 읽기 전용 속성 추가: _target.TransformPoint(GetEyeOffset(_firstPersonOffset)). 기존 눈높이/앉기 보정을 IK도 사용하며 카메라 오프셋/FOV/충돌/추적 동작을 교체하지 않는다. |
| Assets/Features/PlayerCharacter/Animation/PlayerCharacter.controller | UpperCombat 레이어의 m_IKPass: 0→1만 변경. 기존 레이어·상태·모션·전환·마스크를 유지한다. |
| Assets/Scenes/SampleScene.unity | 기존 Armature에 PlayerCharacterWeaponAimView 한 개 추가, 참조 5개 연결, 기존 _onFootComponents 목록에 추가해 탑승 시 함께 중단. 새 오른손 눈 기준 오프셋 (0.35,-0.4,0.9), 왼손의 오른손 기준 위치 (0.07169655,0.07274735,0.045949783), 회전 (9.290004,196.23317,193.0969). 왼손 값은 원본 PistolHoldNeutral에서 추출했다. 기존 두 직렬화 블록에 참조만 추가하고 새 컴포넌트 블록 한 개를 추가했으며 사용자 위치·크기·이름·계층은 유지했다. |
| Assets/Tests/PlayMode/PlayerCharacterCombatTests.cs | ThirdPersonPistolMuzzleFollowsAimWhileTurningLookingAndCrouching 추가: 실제 지면 위에서 몸 회전·상하 조준·앉기 조합의 총구 각도 2도 미만, 손 접점 0.4m 미만, 모델 로컬 회전 불변을 검사. ThirdPersonAimReleasesHandsForReloadAndMelee 추가: 장전/타격에서 손이 조준 고정 위치를 벗어나 실제 모션을 재생하고 끝나면 조준으로 복귀하는지 검사. 기존 검사는 유지. |
| Docs/AI/PlayerCharacterCombatChanges.md | 이전 머리 수정의 한계, 전체 조준 원인·수정·파일·검증을 기록. |
| Docs/AI/UnityProjectContext.md | 현재 IK 책임과 최신 검증 결과 갱신. |

기존 애니메이션만으로 권총을 유지하던 흐름은 조준 중 손 IK가 최종 방향을 결정하도록 확장했다. 장전·근접 공격 및 1인칭의 기존 모션/팔 리그는 유지한다. 삭제한 기존 메서드·필드·컴포넌트·참조·에셋은 없다. UpperCombat.mask, 권총 프리팹, 클립, 무기 수치, 카메라 설정은 이번에 변경하지 않았다.

조준 리그는 독립된 표현 책임/상태/수명을 가지므로 별도 컴포넌트를 선택했다. 기존 Controller에 IK 코드를 넣지 않았으며 공개 API는 필요한 읽기 값 두 개와 눈 위치뿐이다. 추가 상속/인터페이스 없이 명시적 Unity 참조로 조합한다. 의도적인 OOP/SOLID 예외는 없다.

검증:

- Temp/codex-weapon-aim-before.xml: 새 총구 회귀 검사가 기존 구현에서 38.21669도 오차로 실패해 증상을 재현했다.
- Temp/codex-weapon-aim-mapping.xml: 전투 관련 8/8 통과. IK 좌표 보정, 회전/상하/앉기, 장전/근접 해제·복귀를 포함한다.
- Temp/codex-third-person-aim-final.xml: 제품 테스트 66개와 실제 SampleScene 캡처 1개, 총 67/67 통과(34.82초). 새로운 컴파일/런타임 오류가 없다. 실제 시작 위치에서 총구 각도/수평 오차 0도를 계측했다.
- 실제 씬 캡처는 Temp/codex-actual-aim-third-person.png 및 codex-actual-aim-first-person.png. 후속 Temp/codex-actual-grounded-aim.xml도 1/1 통과했다. 착지 완료/지면 접촉/Locomotion 대기를 확인한 자세에서 총구 수평/전체 각도 0도, 머리 위치 루트 기준 (0.09,1.63,0.05), 오른손 (0.35,1.20,0.90)을 계측하고 3인칭/1인칭 이미지를 확인했다. 단순 총구 수치뿐 아니라 손이 머리 아래의 정면 조준 자세를 유지하는 것도 확인했다. 별도의 플레이어 빌드는 실행하지 않았다.

원본 Editor의 저장되지 않은 상태를 조작하지 않고 격리된 검증 프로젝트에서 확인했다. 씬/Controller는 전체 복사 대신 필요한 부분만 패치한다. 새 컴포넌트가 씬에 추가됐으므로 사용자 Editor에서는 SampleScene을 다시 열고 Play하여 사진을 찍었던 방향에서도 확인해야 한다. 커밋/푸시는 하지 않았다.

## 상하 조준의 상체 분담 개선 — 2026-10-27

손만 움직이던 조준을 기본 레이어의 몸통/목 LookAt → 전투 레이어의 양손 IK 순서로 확장했다. Base Layer IK Pass를 켜고, 눈 오프셋의 세로 성분을 회전에서 분리했으며, 양팔의 공동 도달 범위를 적용해 깊은 상하 조준에서도 손 접점을 유지한다. 몸통 가중치 0.3/머리 0.85/추적 속도 12를 씬에 기록했다. 제품 68개와 실제 씬 캡처를 포함한 69/69 검사가 통과했다. 전체 변경 파일·코드 교체·설정·측정값은 PlayerCharacterUpperBodyAimChanges.md 참조.
