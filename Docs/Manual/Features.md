# 구현 기능 목록

기준일 2026-11-10. 각 행은 현재 코드에 있는 기능 단위다. 런타임 합격 범위는 검증 문서에 별도 기록한다. 입력·값은 현재 에셋이 우선이다.

[목차](<README.md>) · [실행 검증](<Validation.md>)

## 보병 이동

| 기능 | 조작·발생 조건 | 담당 코드 | 동작과 연결 조건 |
|---|---|---|---|
| 이동·달리기·점프 | WASD / Shift / Space | [PlayerCharacterMovement](<../../Assets/Features/PlayerCharacter/Scripts/Locomotion/PlayerCharacterMovement.cs>) · [PlayerCharacterActionPolicy](<../../Assets/Features/PlayerCharacter/Scripts/Actions/PlayerCharacterActionPolicy.cs>) | CharacterController가 위치와 중력을 처리. 행동 가능 여부는 Policy에서 조회. |
| 앉기·엎드리기·포복 | Ctrl / Z | [PlayerCharacterStance](<../../Assets/Features/PlayerCharacter/Scripts/Locomotion/PlayerCharacterStance.cs>) · [PlayerCharacterProneAnimationView](<../../Assets/Features/PlayerCharacter/Scripts/Presentation/Locomotion/PlayerCharacterProneAnimationView.cs>) · [ProneAnimationSet](<../../Assets/Features/PlayerCharacter/Scripts/Presentation/Locomotion/ProneAnimationSet.cs>) | 캡슐 높이·일어설 공간·전환 중 이동을 제어. 포복 클립 연결은 ProneAnimationSceneSetup 경유. |
| 슬라이딩·회복 | 달리면서 C | [PlayerCharacterTraversal](<../../Assets/Features/PlayerCharacter/Scripts/Locomotion/PlayerCharacterTraversal.cs>) · [PlayerCharacterSlideAnimationView](<../../Assets/Features/PlayerCharacter/Scripts/Presentation/Locomotion/PlayerCharacterSlideAnimationView.cs>) · [SlideAnimationSet](<../../Assets/Features/PlayerCharacter/Scripts/Presentation/Locomotion/SlideAnimationSet.cs>) | 이동과 슬라이드 클립/회복 재생을 분리. 로컬 RunningSlide 프로필 필요. |
| 낮은 장애물 넘기·높은 장애물 오르기 | 장애물 앞 Space | [PlayerCharacterTraversal](<../../Assets/Features/PlayerCharacter/Scripts/Locomotion/PlayerCharacterTraversal.cs>) · [PlayerCharacterMantle](<../../Assets/Features/PlayerCharacter/Scripts/Locomotion/PlayerCharacterMantle.cs>) · [MantleDefinition](<../../Assets/Features/PlayerCharacter/Scripts/Locomotion/MantleDefinition.cs>) | 장애물·착지 공간 검사 후 이동. 높은 장애물 기능은 LocalTacticalActionSetup이 연결. |
| 옆으로 내밀기·무기 거치 | Q/E / 조준 중 X | [PlayerCharacterLean](<../../Assets/Features/PlayerCharacter/Scripts/Camera/PlayerCharacterLean.cs>) · [PlayerCharacterWeaponBracing](<../../Assets/Features/PlayerCharacter/Scripts/Combat/PlayerCharacterWeaponBracing.cs>) · [PlayerCharacterPostureView](<../../Assets/Features/PlayerCharacter/Scripts/Presentation/Locomotion/PlayerCharacterPostureView.cs>) | 기울임은 옆 이동·몸 자세에 적용. 거치는 정지·조준·표면 조건을 만족하면 반동 계수를 제공. |

## 시점·애니메이션

| 기능 | 조작·발생 조건 | 담당 코드 | 동작과 연결 조건 |
|---|---|---|---|
| 1·3인칭·자유 시점·견착·ADS | V / Alt / 우클릭 | [PlayerCharacterLookController](<../../Assets/Features/PlayerCharacter/Scripts/Camera/PlayerCharacterLookController.cs>) · [PlayerCharacterCamera](<../../Assets/Features/PlayerCharacter/Scripts/Camera/PlayerCharacterCamera.cs>) · [PlayerCharacterVisibility](<../../Assets/Features/PlayerCharacter/Scripts/Presentation/FirstPerson/PlayerCharacterVisibility.cs>) · [IPlayerCharacterAimSource](<../../Assets/Features/PlayerCharacter/Scripts/Input/Contracts/IPlayerCharacterAimSource.cs>) | View는 시선 상태, Camera는 화면 위치·FOV·가림 보정, Visibility는 모델 표시를 담당. |
| 이동 애니메이션·무장별 클립 | 이동·장비 상태 | [PlayerCharacterAnimationView](<../../Assets/Features/PlayerCharacter/Scripts/Presentation/Locomotion/PlayerCharacterAnimationView.cs>) · [ArmedLocomotionAnimationSet](<../../Assets/Features/PlayerCharacter/Scripts/Presentation/Locomotion/ArmedLocomotionAnimationSet.cs>) · [EquippedAnimationControllers](<../../Assets/Features/PlayerCharacter/Scripts/Presentation/Combat/EquippedAnimationControllers.cs>) | 이동 상태로 Animator 파라미터 갱신. 장비·로컬 모션 프로필에 따라 클립 연결. |
| 몸통·손 조준·그립·장전 보정 | 마우스와 전투 상태 | [PlayerCharacterWeaponAimView](<../../Assets/Features/PlayerCharacter/Scripts/Presentation/Combat/PlayerCharacterWeaponAimView.cs>) · [PlayerCharacterRifleReloadView](<../../Assets/Features/PlayerCharacter/Scripts/Presentation/Combat/PlayerCharacterRifleReloadView.cs>) · [HumanoidArmSolver](<../../Assets/Features/PlayerCharacter/Scripts/Presentation/Combat/HumanoidArmSolver.cs>) | OnAnimatorIK와 LateUpdate에서 상체 조준, 손 그립, 장전·포복 접촉 등을 보정. 집중 검토 대상. |
| 1인칭 팔·점프/이동 화면 움직임 | 시점·이동 상태 | [FirstPersonArmsView](<../../Assets/Features/PlayerCharacter/Scripts/Presentation/FirstPerson/FirstPersonArmsView.cs>) · [FirstPersonArmPose](<../../Assets/Features/PlayerCharacter/Scripts/Presentation/FirstPerson/FirstPersonArmPose.cs>) · [PlayerCharacterMotionState](<../../Assets/Features/PlayerCharacter/Scripts/Presentation/FirstPerson/PlayerCharacterMotionState.cs>) · [PlayerCharacterMotionProfile](<../../Assets/Features/PlayerCharacter/Scripts/Presentation/FirstPerson/PlayerCharacterMotionProfile.cs>) · [PlayerCharacterJumpPoseState](<../../Assets/Features/PlayerCharacter/Scripts/Presentation/FirstPerson/PlayerCharacterJumpPoseState.cs>) · [PlayerCharacterWeaponPoseState](<../../Assets/Features/PlayerCharacter/Scripts/Presentation/Combat/PlayerCharacterWeaponPoseState.cs>) | 눈 기준 팔 표현과 이동/착지/점프 상태를 조합. 실제 애니메이션 품질은 영상 확인 필요. |
| 전투 이벤트의 시각화 | 발사·장전·근접 공격 | [PlayerCharacterCombatView](<../../Assets/Features/PlayerCharacter/Scripts/Presentation/Combat/PlayerCharacterCombatView.cs>) | 무기 모델 부착, 장비 Animator 교체, 반동과 액션 재생. 게임 상태의 생산자는 Controller/탄창. |

## 무기·전투

| 기능 | 조작·발생 조건 | 담당 코드 | 동작과 연결 조건 |
|---|---|---|---|
| 장착·무기 모델·소총 교체대 | 1 소총 / 2 권총 / 3 맨손 / 교체대 F | [PlayerCharacterLoadout](<../../Assets/Features/PlayerCharacter/Scripts/Equipment/PlayerCharacterLoadout.cs>) · [PlayerCharacterWeaponEquipment](<../../Assets/Features/PlayerCharacter/Scripts/Equipment/PlayerCharacterWeaponEquipment.cs>) · [PlayerCharacterWeaponBracing](<../../Assets/Features/PlayerCharacter/Scripts/Combat/PlayerCharacterWeaponBracing.cs>) · [WeaponModel](<../../Assets/Features/Weapon/Scripts/Models/WeaponModel.cs>) · [InfantryWeaponDefinition](<../../Assets/Features/Weapon/Scripts/Definitions/InfantryWeaponDefinition.cs>) · [RifleSelectionStation](<../../Assets/Features/Training/Scripts/RifleSelectionStation.cs>) · [PlayerRifleStationInteraction](<../../Assets/Features/PlayerCharacter/Scripts/Interaction/PlayerRifleStationInteraction.cs>) | 무기 설정은 Definition, 현재 장비는 Loadout, 무기별 모델/탄창은 Equipment. 교체대 안내 문구는 HUD에서 제거됨. |
| 권총 단발·소총 발사 모드 | 좌클릭 / 전환 가능한 무기 B | [PlayerCharacterCombatController](<../../Assets/Features/PlayerCharacter/Scripts/Combat/PlayerCharacterCombatController.cs>) · [PlayerCharacterInput](<../../Assets/Features/PlayerCharacter/Scripts/Input/PlayerCharacterInput.cs>) · [InfantryWeaponDefinition](<../../Assets/Features/Weapon/Scripts/Definitions/InfantryWeaponDefinition.cs>) | 단발은 FirePressed, 연발은 FireWeapon. 전환 가능 여부는 무기 정의, 선택한 모드는 장비 슬롯이 소유한다. |
| 장전·빈 탄창 재클릭 장전 | R / 탄창 0에서 새 좌클릭 | [WeaponMagazine](<../../Assets/Features/Weapon/Scripts/Ammunition/WeaponMagazine.cs>) · [WeaponMagazineState](<../../Assets/Features/Weapon/Scripts/Ammunition/WeaponMagazineState.cs>) · [IWeaponMagazine](<../../Assets/Features/Weapon/Scripts/Ammunition/IWeaponMagazine.cs>) · [PlayerCharacterCombatController](<../../Assets/Features/PlayerCharacter/Scripts/Combat/PlayerCharacterCombatController.cs>) | 탄 수·예비탄·타이머는 탄창이 소유. 입력으로 TryReload 요청. 최신 단발 변경 이후 일부 테스트 입력 보완 필요. |
| 주먹·무기 근접 공격 | 맨손 좌클릭 / G | [MeleeAttack](<../../Assets/Features/Weapon/Scripts/Melee/MeleeAttack.cs>) · [PlayerCharacterCombatController](<../../Assets/Features/PlayerCharacter/Scripts/Combat/PlayerCharacterCombatController.cs>) | 공격 준비시간·범위 검사·피해·취소 처리. Controller가 상황에 맞는 공격을 요청. |
| 조준 반동·거치 반동 감소 | 발사 이벤트 | [PlayerCharacterRecoil](<../../Assets/Features/PlayerCharacter/Scripts/Combat/PlayerCharacterRecoil.cs>) · [PlayerCharacterWeaponBracing](<../../Assets/Features/PlayerCharacter/Scripts/Combat/PlayerCharacterWeaponBracing.cs>) | 전투 결과를 읽어 조준/반동에 반영. |
| 수류탄 장착·취소·위/아래 투척 | 4 / 좌클릭 유지 후 해제 / 우클릭 / 다른 슬롯 | [PlayerCharacterGrenadeThrow](<../../Assets/Features/PlayerCharacter/Scripts/Combat/PlayerCharacterGrenadeThrow.cs>) · [GrenadeDefinition](<../../Assets/Features/Weapon/Scripts/Definitions/GrenadeDefinition.cs>) · [GrenadeProjectile](<../../Assets/Features/Projectile/Scripts/GrenadeProjectile.cs>) | 모드별 발사 위치·속도를 공유. 예비 장착 중 취소 가능. 수류탄 애니메이션은 현재 사용 OFF. |
| 수류탄 궤적 | 좌클릭 조준 중 | [GrenadeTrajectoryView](<../../Assets/Features/UI/Player/GrenadeTrajectoryView.cs>) | 실제 발사 조건으로 중력·첫 충돌 예측. 짧은 흰색/주황색 구간을 표시. Gameplay 상태와 분리. |

## 생명·탑승

| 기능 | 조작·발생 조건 | 담당 코드 | 동작과 연결 조건 |
|---|---|---|---|
| 체력·팀 피해·사망 | 피격 | [Health](<../../Assets/Framework/Core/Health.cs>) · [Team](<../../Assets/Framework/Core/Team.cs>) · [DamageInfo](<../../Assets/Framework/Core/DamageInfo.cs>) · [IDamageable](<../../Assets/Framework/Core/IDamageable.cs>) · [IHealable](<../../Assets/Framework/Core/IHealable.cs>) · [IHealthDamagePolicy](<../../Assets/Framework/Core/IHealthDamagePolicy.cs>) | 공통 체력과 피해 계약. 보병 생명 정책을 별도 구현으로 연결. |
| 다운·출혈·포기·리스폰 | 다운 중 Space / 사망 후 Enter | [InfantryLifeState](<../../Assets/Features/PlayerCharacter/Scripts/Life/InfantryLifeState.cs>) | 다운 상태와 출혈 시간, 부활/리스폰 공간을 관리. 전투 HUD의 안내는 현재 제거됨. |
| 아군 부활·끌기 | 대상 근처 F 유지 / Shift | [PlayerCharacterRevive](<../../Assets/Features/PlayerCharacter/Scripts/Life/PlayerCharacterRevive.cs>) · [InfantryTrainingDummy](<../../Assets/Features/Training/Scripts/InfantryTrainingDummy.cs>) | 부활 대상 점유·진행률·끌기. 안내 문구는 현재 제거됨. |
| 차량 탑승·하차·입력 전환 | 좌석 근처 F | [PlayerVehicleInteraction](<../../Assets/Features/PlayerCharacter/Scripts/Interaction/PlayerVehicleInteraction.cs>) · [PlayerVehicleOccupant](<../../Assets/Features/PlayerCharacter/Scripts/Interaction/PlayerVehicleOccupant.cs>) · [VehicleSeat](<../../Assets/Features/Vehicles/Scripts/VehicleSeat.cs>) · [IVehicleSeatOccupant](<../../Assets/Features/Vehicles/Scripts/IVehicleSeatOccupant.cs>) · [IVehicleSeatInput](<../../Assets/Features/Vehicles/Scripts/IVehicleSeatInput.cs>) | 차량 표면 근접 판정, 좌석 점유, 하차 캡슐 공간 검사, 보병/차량 제어권 전환. |
| 조종석 착석·자세 | 탑승 상태 | [SeatedCharacterPose](<../../Assets/Features/PlayerCharacter/Scripts/Presentation/Seating/SeatedCharacterPose.cs>) · [SeatedAnimationPlayback](<../../Assets/Features/PlayerCharacter/Scripts/Presentation/Seating/SeatedAnimationPlayback.cs>) | 현재 캐릭터를 좌석에 붙이고 착석 표현 적용. 관련 클립과 좌석 Transform 확인 필요. |

## 전투기

| 기능 | 조작·발생 조건 | 담당 코드 | 동작과 연결 조건 |
|---|---|---|---|
| 추력·속도·실속/중력 | W/S / Shift | [JetMovement](<../../Assets/Features/Fighter/Scripts/Movement/JetMovement.cs>) · [Afterburner](<../../Assets/Features/Fighter/Scripts/Movement/Afterburner.cs>) · [JetPhysics](<../../Assets/Features/Fighter/Scripts/Movement/JetPhysics.cs>) · [FighterEngineActivation](<../../Assets/Features/Fighter/Scripts/Movement/FighterEngineActivation.cs>) | 탑승 상태가 엔진을 제어. Movement는 속도, Afterburner는 소모/회복과 배율, Physics는 중력 조건을 담당. |
| 기체 회전·추력 편향 | A/D / Q/E / 마우스 | [JetRotation](<../../Assets/Features/Fighter/Scripts/Movement/JetRotation.cs>) · [ThrustVectoring](<../../Assets/Features/Fighter/Scripts/Movement/ThrustVectoring.cs>) · [IJetInput](<../../Assets/Features/Fighter/Scripts/Controller/IJetInput.cs>) · [KeyboardJetInput](<../../Assets/Features/Fighter/Scripts/Controller/KeyboardJetInput.cs>) | 입력을 회전·편향·에너지 감소에 반영. 실제 축별 매핑은 입력 표와 KeyboardJetInput 확인. |
| 충돌·피해·파괴 | 충돌/체력 | [FighterMeshCollider](<../../Assets/Features/Fighter/Scripts/Collision/FighterMeshCollider.cs>) · [FighterCollisionConstraint](<../../Assets/Features/Fighter/Scripts/Collision/FighterCollisionConstraint.cs>) · [FighterCollisionResponse](<../../Assets/Features/Fighter/Scripts/Collision/FighterCollisionResponse.cs>) · [FighterCollisionDamage](<../../Assets/Features/Fighter/Scripts/Collision/FighterCollisionDamage.cs>) · [FighterDestruction](<../../Assets/Features/Fighter/Scripts/Destruction/FighterDestruction.cs>) | 충돌 형상·속도 제한·피해를 분리. 파괴는 제어 정지·추락·폭발·파괴 시점을 조정. |
| 추적·자유 시점·후방·줌 | C / Ctrl / V / 우클릭 | [JetCamera](<../../Assets/Features/Fighter/Scripts/Camera/JetCamera.cs>) · [JetCameraOrbit](<../../Assets/Features/Fighter/Scripts/Camera/JetCameraOrbit.cs>) · [JetCameraLens](<../../Assets/Features/Fighter/Scripts/Camera/JetCameraLens.cs>) · [JetDestructionView](<../../Assets/Features/Fighter/Scripts/Camera/JetDestructionView.cs>) | Main Camera에서 동작. 휠 거리 액션은 존재하지만 Orbit의 갱신 호출은 주석 처리됨. |
| 무기 선택·기관총·과열 | 무기 선택 / 좌클릭 | [WeaponController](<../../Assets/Features/Weapon/Scripts/Firing/WeaponController.cs>) · [WeaponBase](<../../Assets/Features/Weapon/Scripts/Firing/WeaponBase.cs>) · [MachineGun](<../../Assets/Features/Weapon/Scripts/Firing/MachineGun.cs>) · [OverHeat](<../../Assets/Features/Weapon/Scripts/Firing/Overheat.cs>) · [MachineGunAimPredictor](<../../Assets/Features/Weapon/Scripts/Firing/MachineGunAimPredictor.cs>) | WeaponBase가 발사 간격·과열·Fired 이벤트, MachineGun이 탄환 발사와 예측을 담당. |
| 공대공 미사일·재보급 | 미사일 선택·락온 후 발사 | [AirToAirMissileLauncher](<../../Assets/Features/Weapon/Scripts/Firing/AirToAirMissileLauncher.cs>) · [IRefillProgressSource](<../../Assets/Features/Weapon/Scripts/Ammunition/IRefillProgressSource.cs>) | 발사 성공·잔탄·재보급 시간 및 발사 위치 연결. |

## 표적·투사체

| 기능 | 조작·발생 조건 | 담당 코드 | 동작과 연결 조건 |
|---|---|---|---|
| 표적 등록·탐색·락온 | 표적/시점 상태 | [AirTarget](<../../Assets/Features/Targeting/Scripts/AirTarget.cs>) · [AirTargetScanner](<../../Assets/Features/Targeting/Scripts/AirTargetScanner.cs>) · [AirTargetScreenArea](<../../Assets/Features/Targeting/Scripts/AirTargetScreenArea.cs>) · [AirTargetLock](<../../Assets/Features/Targeting/Scripts/AirTargetLock.cs>) · [AirTargetLockState](<../../Assets/Features/Targeting/Scripts/AirTargetLockState.cs>) · [ITargetable](<../../Assets/Framework/Core/ITargetable.cs>) | 탐색, 화면 영역 조건, 시간에 따른 락온 상태를 나눠 관리. |
| 미사일 경고·위협 탐지 | 적의 조준·미사일 접근 | [MissileThreatDetector](<../../Assets/Features/Fighter/Scripts/Combat/MissileThreatDetector.cs>) · [IMissileWarningReceiver](<../../Assets/Features/Targeting/Scripts/IMissileWarningReceiver.cs>) · [MissileWarningState](<../../Assets/Features/Targeting/Scripts/MissileWarningState.cs>) | 위협 상태를 경고 UI로 전달. 표적·발사체와 경고 표시 연결. |
| 탄환 이동·명중·예광·외형 | 발사 결과 | [ProjectileBase](<../../Assets/Features/Projectile/Scripts/ProjectileBase.cs>) · [ProjectileData](<../../Assets/Features/Projectile/Scripts/ProjectileData.cs>) · [Bullet](<../../Assets/Features/Projectile/Scripts/Bullet.cs>) · [BulletPool](<../../Assets/Features/Projectile/Scripts/BulletPool.cs>) · [BulletVisual](<../../Assets/Features/Projectile/Scripts/BulletVisual.cs>) · [BulletTracer](<../../Assets/Features/Projectile/Scripts/BulletTracer.cs>) | 이동 구간 충돌 검사·소유자/팀 판단·수명·풀 반환. 외형과 예광 표현 분리. |
| 유도 미사일·근접 신관·폭발 | 미사일 발사 | [HomingMissile](<../../Assets/Features/Projectile/Scripts/HomingMissile.cs>) · [HomingMissilePool](<../../Assets/Features/Projectile/Scripts/HomingMissilePool.cs>) | 회전 유도, 직접 충돌/근접 폭발, 경고 수신자 해제, 재사용 초기화. |

## 화면·VFX

| 기능 | 조작·발생 조건 | 담당 코드 | 동작과 연결 조건 |
|---|---|---|---|
| 보병 전투 HUD | 현재 전투 상태 | [PlayerCombatHud](<../../Assets/Features/UI/Player/PlayerCombatHud.cs>) | 무기·탄약·단발/연발·HP 표시. 자세·수류탄·다운/부활/교체대 문구는 제거됨. |
| 탑승 안내 | 차량 접근/탑승 | [VehicleInteractionView](<../../Assets/Features/UI/Player/VehicleInteractionView.cs>) | 보병 전투 HUD와 별도의 탑승 상호작용 안내. |
| 기체 상태·무기 선택·재보급 | 비행/전투 상태 | [FighterHudView](<../../Assets/Features/UI/Fighter/FighterHudView.cs>) · [FighterCombatHudView](<../../Assets/Features/UI/Fighter/FighterCombatHudView.cs>) · [WeaponSelectionView](<../../Assets/Features/UI/Shared/WeaponSelectionView.cs>) · [RefillProgressView](<../../Assets/Features/UI/Shared/RefillProgressView.cs>) | 기체 상태, 선택 무기, 잔탄/재보급을 표시. |
| 기수 방향 UI·조준점·과열 | 고정/자유 시점·기관총 | [ForwardUiFollower](<../../Assets/Features/UI/Shared/ForwardUiFollower.cs>) · [FighterGunAimView](<../../Assets/Features/UI/Fighter/FighterGunAimView.cs>) · [FighterGunAimReticleMover](<../../Assets/Features/UI/Fighter/FighterGunAimReticleMover.cs>) · [HeatDotsView](<../../Assets/Features/UI/Shared/HeatDotsView.cs>) · [HeatDotRingBuilder](<../../Assets/Features/UI/Shared/HeatDotRingBuilder.cs>) · [IThirdPersonViewState](<../../Assets/Framework/Core/IThirdPersonViewState.cs>) | 전방 월드 지점의 화면 투영을 공유. 시점 상태 인터페이스를 읽고 UI 배치. |
| 락온 범위·표적·미사일 방향 | 락온/위협 상태 | [AirTargetLockView](<../../Assets/Features/UI/Fighter/AirTargetLockView.cs>) · [LockOnAreaGraphic](<../../Assets/Features/UI/Fighter/LockOnAreaGraphic.cs>) · [MissileWarningView](<../../Assets/Features/UI/Fighter/MissileWarningView.cs>) · [MissileDirectionIndicatorView](<../../Assets/Features/UI/Fighter/MissileDirectionIndicatorView.cs>) · [MissileDirectionIndicatorGraphic](<../../Assets/Features/UI/Fighter/MissileDirectionIndicatorGraphic.cs>) | 표적·범위 도형과 접근 방향/경고를 표시. Graphic은 메시 생성에 집중. |
| 재질별 피격 이펙트·탄흔 | Physics Material 명중 | [HitEffectManager](<../../Assets/Features/VFX/Hit/Scripts/HitEffectManager.cs>) · [HitEffectDatabase](<../../Assets/Features/VFX/Hit/Scripts/HitEffectDatabase.cs>) · [HitEffect](<../../Assets/Features/VFX/Hit/Scripts/HitEffect.cs>) · [HitEffectPool](<../../Assets/Features/VFX/Hit/Scripts/HitEffectPool.cs>) · [BulletHole](<../../Assets/Features/VFX/Hit/Scripts/BulletHole.cs>) · [BulletHolePool](<../../Assets/Features/VFX/Hit/Scripts/BulletHolePool.cs>) · [CollisionSurfaceType](<../../Assets/Framework/Physics/CollisionSurfaceType.cs>) | 재질→효과/탄흔 풀 조회. 재질 또는 등록 항목이 없으면 표현 생략. |
| 총구·폭발·엔진 효과 | 발사·파괴·엔진 상태 | [MuzzleFlash](<../../Assets/Features/VFX/Muzzle/Scripts/MuzzleFlash.cs>) · [ExplosionEffect](<../../Assets/Features/VFX/Explosion/Scripts/ExplosionEffect.cs>) · [ExplosionEffectPool](<../../Assets/Features/VFX/Explosion/Scripts/ExplosionEffectPool.cs>) · [JetEngineVfx](<../../Assets/Features/Fighter/Scripts/VFX/JetEngineVfx.cs>) · [ParticlePlayer](<../../Assets/Framework/Particle/ParticlePlayer.cs>) | 게임 상태/이벤트에 맞춰 파티클 재생. |

## 사운드

| 기능 | 조작·발생 조건 | 담당 코드 | 동작과 연결 조건 |
|---|---|---|---|
| 엔진·애프터버너 | 속도·엔진·버너 상태 | [FighterEngineAudio](<../../Assets/Features/Fighter/Scripts/Audio/FighterEngineAudio.cs>) · [FighterEngineAudioProfile](<../../Assets/Features/Audio/Scripts/FighterEngineAudioProfile.cs>) | 기체 위치의 반복음·피치/볼륨·전환. 청취 검증은 별도 필요. |
| 기관총·미사일 발사·미사일 비행 | 발사 이벤트/미사일 생명주기 | [MachineGunAudio](<../../Assets/Features/Weapon/Scripts/Audio/MachineGunAudio.cs>) · [MissileLaunchAudio](<../../Assets/Features/Weapon/Scripts/Audio/MissileLaunchAudio.cs>) · [MissileFlightAudio](<../../Assets/Features/Projectile/Scripts/MissileFlightAudio.cs>) · [MachineGunAudioProfile](<../../Assets/Features/Audio/Scripts/MachineGunAudioProfile.cs>) · [MissileLaunchAudioProfile](<../../Assets/Features/Audio/Scripts/MissileLaunchAudioProfile.cs>) · [MissileFlightAudioProfile](<../../Assets/Features/Audio/Scripts/MissileFlightAudioProfile.cs>) | 재생 책임을 무기/투사체에 배치. 코드·프리팹 연결과 실제 들리는 결과는 검증 상태를 별도 기록. |
| 통합 오디오 개요 | Inspector의 AudioOverview | [AudioOverview](<../../Assets/Features/Audio/Scripts/AudioOverview.cs>) · [AudioOverviewEditor](<../../Assets/Features/Audio/Editor/AudioOverviewEditor.cs>) · [AudioProfile](<../../Assets/Features/Audio/Scripts/AudioProfile.cs>) | 루트/관련 프리팹에서 프로필과 소리 연결을 모아 편집. 재생 로직은 각 컴포넌트가 담당. |

## 개발·테스트

| 기능 | 조작·발생 조건 | 담당 코드 | 동작과 연결 조건 |
|---|---|---|---|
| 더미 발사·미사일·락온·팀 테스트 | Dummy 오브젝트/테스트 | [DummyShooter](<../../Assets/Features/Training/Scripts/DummyShooter.cs>) · [DummyMissileShooter](<../../Assets/Features/Training/Scripts/DummyMissileShooter.cs>) · [DummyDebug](<../../Assets/Features/Training/Scripts/DummyDebug.cs>) · [DummyAirTargetLockDebug](<../../Assets/Features/Training/Scripts/DummyAirTargetLockDebug.cs>) | 아군 소유자로 적·중립·아군 피해/충돌 경로를 시험하는 기반. |
| 오브젝트 풀·재사용 계약 | Get / Return | [ObjectPool](<../../Assets/Framework/Pool/ObjectPool.cs>) · [PoolableBehaviour](<../../Assets/Framework/Pool/PoolableBehaviour.cs>) · [IPool](<../../Assets/Framework/Pool/IPool.cs>) · [IPoolable](<../../Assets/Framework/Pool/IPoolable.cs>) | 생성·획득·반환과 OnSpawn/OnDespawn 공통 생명주기. |
| 모션 임포트·로컬 연결·진단 | Tools/Battlefield 메뉴 | [MixamoSlideImporter](<../../Assets/Features/PlayerCharacter/Editor/Importers/MixamoSlideImporter.cs>) · [MixamoArmedLocomotionImporter](<../../Assets/Features/PlayerCharacter/Editor/Importers/MixamoArmedLocomotionImporter.cs>) · [ProneAnimationSceneSetup](<../../Assets/Features/PlayerCharacter/Editor/Composition/ProneAnimationSceneSetup.cs>) · [TacticalActionImporter](<../../Assets/Features/PlayerCharacter/Editor/Importers/TacticalActionImporter.cs>) · [TacticalActionSceneSetup](<../../Assets/Features/PlayerCharacter/Editor/Composition/TacticalActionSceneSetup.cs>) · [SlideAnimationSceneSetup](<../../Assets/Features/PlayerCharacter/Editor/Composition/SlideAnimationSceneSetup.cs>) · [ArmedLocomotionSceneSetup](<../../Assets/Features/PlayerCharacter/Editor/Composition/ArmedLocomotionSceneSetup.cs>) · [PlayerCharacterAimDiagnostics](<../../Assets/Features/PlayerCharacter/Editor/Diagnostics/PlayerCharacterAimDiagnostics.cs>) · [InfantryTestTools](<../../Assets/Features/PlayerCharacter/Editor/Diagnostics/InfantryTestTools.cs>) | 원본 임포트, Play/빌드 복사 씬의 연결, 보병/조준 진단. 로컬 전용 에셋 경로에 유의. |
| 수류탄 클립 제작·합성 도구 | Tools/Battlefield/Animations 메뉴 | [GrenadeHandlingAnimationAuthoring](<../../Assets/Features/PlayerCharacter/Editor/Authoring/GrenadeHandlingAnimationAuthoring.cs>) · [GrenadeThrowCompositionSetup](<../../Assets/Features/PlayerCharacter/Editor/Authoring/GrenadeThrowCompositionSetup.cs>) | 에디터 클립 생성/합성 구현은 존재. 현재 수류탄 모션 OFF와 구분해서 관리. |

## 소총 단발·연사 전환을 수정할 때

1. 입력 바인딩은 InputSystem_Actions의 PlayerCharacter/ToggleFireMode에서 변경한다.
2. PlayerCharacterInput이 WasPressedThisFrame으로 전환 요청을 제공한다.
3. Loadout이 현재 무기 정의의 FireMode를 확인하고, 전환 가능한 경우 장비 슬롯의 모드를 뒤집는다.
4. 단발은 FirePressed, 연사는 FireWeapon을 읽는다. TryFire가 발사 간격·탄창·탄환 생성을 처리한다.
5. PlayerCombatHud가 상태를 읽어 사용자가 정한 `{단발}`·`{연발}` 문구를 붙인다.

현재 모드는 장비 슬롯 단위다. 소총 종류별 모드 기억, 점사, 저장/불러오기 정책은 별도 설계가 필요하다.

## 현재 표현·연결 조건

- 수류탄 기능은 모션 OFF로 운용한다. 로컬 TacticalActions 프로필의 유효성은 여전히 높은 장애물 모션까지 함께 검사한다.
- 캐릭터 애니메이션에는 외부 클립 재생과 IK·몸통 안정화·접촉 보정이 함께 있다. 모든 동작이 원본 클립 그대로인 구조는 아니다.
- HUD에서 제거한 안내와 실제 다운·부활·교체대 기능을 구분한다. 안내의 새 표시 위치는 아직 정해지지 않았다.
- TrainingGround는 씬/에셋 구성이 중심이다. 스크립트가 없는 기능도 씬·입력 문서에서 확인한다.
