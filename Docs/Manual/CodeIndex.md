# 전체 자체 스크립트 색인

[목차](<README.md>)

Features·Framework 아래 179개 C# 파일. 외부 패키지/에셋 코드는 제외한다. 줄 수는 책임 분리 판단의 보조 정보다. 메서드/필드는 정적 추출 목록이며 호출 순서가 아니다.

씬 연결은 SampleScene 및 Features 프리팹의 YAML을 읽었다. `[Prefab instance]`는 원본 프리팹/인스턴스 매핑을 추가 확인해야 하는 위치다. 연결이 없는 파일도 일반 C# 객체, 인터페이스, Editor 도구 또는 런타임 생성형일 수 있다.

## Assets/Features/Audio

### AudioOverviewEditor (193줄)

[AudioOverviewEditor.cs](<../../Assets/Features/Audio/Editor/AudioOverviewEditor.cs>)

- 메서드 — OnInspectorGUI, DrawProfiles, DrawConnections, DrawGameObject, DrawProfile
- 설정 필드 — 인라인 SerializeField 없음; 파일의 다른 선언 확인
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### AudioOverview (10줄)

[AudioOverview.cs](<../../Assets/Features/Audio/Scripts/AudioOverview.cs>)

- 메서드 — 메서드 없는 계약/데이터 또는 별도 구문 확인
- 설정 필드 — _relatedPrefabs
- 연결 — Assets/Features/Fighter/Prefabs/Fighter.prefab → Fighter

### AudioProfile (12줄)

[AudioProfile.cs](<../../Assets/Features/Audio/Scripts/AudioProfile.cs>)

- 메서드 — 메서드 없는 계약/데이터 또는 별도 구문 확인
- 설정 필드 — _outputGroup
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### FighterEngineAudioProfile (26줄)

[FighterEngineAudioProfile.cs](<../../Assets/Features/Audio/Scripts/FighterEngineAudioProfile.cs>)

- 메서드 — 메서드 없는 계약/데이터 또는 별도 구문 확인
- 설정 필드 — _engineLoop, _afterburnerLoop, _idleVolume, _flightVolume, _afterburnerVolume, _idlePitch, _flightPitch, _transitionSeconds
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### MachineGunAudioProfile (16줄)

[MachineGunAudioProfile.cs](<../../Assets/Features/Audio/Scripts/MachineGunAudioProfile.cs>)

- 메서드 — 메서드 없는 계약/데이터 또는 별도 구문 확인
- 설정 필드 — _fireLoop, _volume, _fadeSeconds
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### MissileFlightAudioProfile (14줄)

[MissileFlightAudioProfile.cs](<../../Assets/Features/Audio/Scripts/MissileFlightAudioProfile.cs>)

- 메서드 — 메서드 없는 계약/데이터 또는 별도 구문 확인
- 설정 필드 — _flightLoop, _volume
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### MissileLaunchAudioProfile (14줄)

[MissileLaunchAudioProfile.cs](<../../Assets/Features/Audio/Scripts/MissileLaunchAudioProfile.cs>)

- 메서드 — 메서드 없는 계약/데이터 또는 별도 구문 확인
- 설정 필드 — _launch, _volume
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

## Assets/Features/Dummy

### DummyAirTargetLockDebug (23줄)

[DummyAirTargetLockDebug.cs](<../../Assets/Features/Dummy/Scripts/DummyAirTargetLockDebug.cs>)

- 메서드 — Awake, Update
- 설정 필드 — 인라인 SerializeField 없음; 파일의 다른 선언 확인
- 연결 — Assets/Scenes/SampleScene.unity → PlayerCharacter

### DummyDebug (26줄)

[DummyDebug.cs](<../../Assets/Features/Dummy/Scripts/DummyDebug.cs>)

- 메서드 — Awake, LogDamaged, LogDeath
- 설정 필드 — 인라인 SerializeField 없음; 파일의 다른 선언 확인
- 연결 — Assets/Scenes/SampleScene.unity → Dummys/Dummy_Enemy / Assets/Scenes/SampleScene.unity → PlayerCharacter / Assets/Scenes/SampleScene.unity → MissileTargetDummy_Enemy / Assets/Scenes/SampleScene.unity → Dummys/Dummy_Neutral / Assets/Features/TrainingGround/InfantryTrainingGround.prefab → InfantryTrainingGround/Enemy_50m / Assets/Features/TrainingGround/InfantryTrainingGround.prefab → InfantryTrainingGround/Ally_50m / Assets/Features/TrainingGround/InfantryTrainingGround.prefab → InfantryTrainingGround/Ally_10m / Assets/Features/TrainingGround/InfantryTrainingGround.prefab → InfantryTrainingGround/Enemy_25m / Assets/Features/TrainingGround/InfantryTrainingGround.prefab → InfantryTrainingGround/Neutral_10m / Assets/Features/TrainingGround/InfantryTrainingGround.prefab → InfantryTrainingGround/Neutral_50m / Assets/Features/TrainingGround/InfantryTrainingGround.prefab → InfantryTrainingGround/Neutral_25m / Assets/Features/TrainingGround/InfantryTrainingGround.prefab → InfantryTrainingGround/Ally_25m / Assets/Features/TrainingGround/InfantryTrainingGround.prefab → InfantryTrainingGround/Enemy_10m

### DummyMissileShooter (147줄)

[DummyMissileShooter.cs](<../../Assets/Features/Dummy/Scripts/DummyMissileShooter.cs>)

- 메서드 — Awake, OnEnable, OnDisable, Update, AdvanceState, EnterState, ReportLockProgress, Fire
- 설정 필드 — _target, _missilePool, _firePoint, _fireInterval, _startDelay, _lockDuration, _lockedDuration, _damage
- 연결 — Assets/Scenes/SampleScene.unity → EnemyMissileShooterDummy

### DummyShooter (66줄)

[DummyShooter.cs](<../../Assets/Features/Dummy/Scripts/DummyShooter.cs>)

- 메서드 — Awake, OnEnable, Update, Fire
- 설정 필드 — _target, _bulletPool, _fireInterval, _damage, _startDelay, _muzzleOffset
- 연결 — Assets/Scenes/SampleScene.unity → PlayerCharacter

## Assets/Features/Fighter

### FighterEngineAudio (133줄)

[FighterEngineAudio.cs](<../../Assets/Features/Fighter/Scripts/Audio/FighterEngineAudio.cs>)

- 메서드 — Awake, LateUpdate, OnDisable, StopPlayback
- 설정 필드 — _movement, _afterburner, _health, _profile, _engineSource, _afterburnerSource
- 연결 — Assets/Features/Fighter/Prefabs/Fighter.prefab → Fighter

### JetCamera (119줄)

[JetCamera.cs](<../../Assets/Features/Fighter/Scripts/Camera/JetCamera.cs>)

- 메서드 — Awake, OnEnable, Update, LateUpdate, IsFollowing, BeginDestructionView, GetCurrentViewPoint
- 설정 필드 — _input, _firstPersonCameraPoint, _thirdPersonCameraPoint, _rearViewCameraPoint, _orbit, _lens, _destructionView
- 연결 — Assets/Scenes/SampleScene.unity → Main Camera

### JetCameraLens (53줄)

[JetCameraLens.cs](<../../Assets/Features/Fighter/Scripts/Camera/JetCameraLens.cs>)

- 메서드 — Initialize, UpdateFieldOfView, GetTargetFieldOfView
- 설정 필드 — _firstPersonFieldOfView, _thirdPersonFieldOfView, _zoomFieldOfView, _fieldOfViewChangeSpeed
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### JetCameraOrbit (236줄)

[JetCameraOrbit.cs](<../../Assets/Features/Fighter/Scripts/Camera/JetCameraOrbit.cs>)

- 메서드 — Initialize, UpdatePose, UpdateFreeLook, UpdateCameraDistance, GetViewPointRotation, Update, Reset
- 설정 필드 — _freeLookSensitivity, _maximumFreeLookPitch, _cameraDistanceStep, _minimumThirdPersonDistance, _maximumThirdPersonDistance, _thirdPersonOrbitFollowSpeed, _thirdPersonRollMultiplier, _thirdPersonRollFollowSpeed, _thirdPersonRollReturnSpeed
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### JetDestructionView (40줄)

[JetDestructionView.cs](<../../Assets/Features/Fighter/Scripts/Camera/JetDestructionView.cs>)

- 메서드 — Begin, Track
- 설정 필드 — _trackingSpeed
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### FighterCollisionConstraint (52줄)

[FighterCollisionConstraint.cs](<../../Assets/Features/Fighter/Scripts/Collision/FighterCollisionConstraint.cs>)

- 메서드 — RegisterContact, RemoveContact, ClearContacts, ConstrainVelocity, OnDisable
- 설정 필드 — 인라인 SerializeField 없음; 파일의 다른 선언 확인
- 연결 — Assets/Features/Fighter/Prefabs/Fighter.prefab → Fighter

### FighterCollisionDamage (154줄)

[FighterCollisionDamage.cs](<../../Assets/Features/Fighter/Scripts/Collision/FighterCollisionDamage.cs>)

- 메서드 — Awake, Apply, CalculateDamageRatio, CalculateDamageImpactSpeed, CalculateImpact
- 설정 필드 — _minimumDamageSpeedRatio, _instantDestructionSpeedRatio, _maximumNonLethalDamageRatio, _wallTangentialDamageMultiplier, _minimumLandingUpDot
- 연결 — Assets/Features/Fighter/Prefabs/Fighter.prefab → Fighter

### FighterCollisionResponse (113줄)

[FighterCollisionResponse.cs](<../../Assets/Features/Fighter/Scripts/Collision/FighterCollisionResponse.cs>)

- 메서드 — Awake, OnCollisionEnter, OnCollisionStay, OnCollisionExit, CalculateContactNormal, ClassifyCollisionSurface, LimitAngularVelocity
- 설정 필드 — _maxAngularSpeed, _groundNormalUpDot
- 연결 — Assets/Features/Fighter/Prefabs/Fighter.prefab → Fighter

### FighterMeshCollider (33줄)

[FighterMeshCollider.cs](<../../Assets/Features/Fighter/Scripts/Collision/FighterMeshCollider.cs>)

- 메서드 — Awake
- 설정 필드 — _collisionMesh
- 연결 — Assets/Features/Fighter/Prefabs/Fighter.prefab → Fighter

### MissileThreatDetector (220줄)

[MissileThreatDetector.cs](<../../Assets/Features/Fighter/Scripts/Combat/MissileThreatDetector.cs>)

- 메서드 — Update, OnDisable, ReportThreatState, ReportThreatProgress, ClearThreatState, RefreshIncomingThreats, ClearThreats, RefreshWarningState, RefreshLockProgress, SetWarningState, SetLockProgress
- 설정 필드 — 인라인 SerializeField 없음; 파일의 다른 선언 확인
- 연결 — Assets/Scenes/SampleScene.unity → [Prefab instance] / Assets/Features/Fighter/Prefabs/Fighter.prefab → Fighter

### IJetInput (22줄)

[IJetInput.cs](<../../Assets/Features/Fighter/Scripts/Controller/IJetInput.cs>)

- 메서드 — 메서드 없는 계약/데이터 또는 별도 구문 확인
- 설정 필드 — 인라인 SerializeField 없음; 파일의 다른 선언 확인
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### KeyboardJetInput (172줄)

[KeyboardJetInput.cs](<../../Assets/Features/Fighter/Scripts/Controller/KeyboardJetInput.cs>)

- 메서드 — Awake, OnEnable, OnDisable, OnDestroy, Update, SelectPrimaryWeapon, SelectSecondaryWeapon, SetFreeLookAllowed, SetWeaponFireAllowed, ResetInput
- 설정 필드 — _inputActions
- 연결 — Assets/Scenes/SampleScene.unity → [Prefab instance] / Assets/Features/Fighter/Prefabs/Fighter.prefab → Fighter

### FighterDestruction (174줄)

[FighterDestruction.cs](<../../Assets/Features/Fighter/Scripts/Destruction/FighterDestruction.cs>)

- 메서드 — Awake, OnEnable, OnDisable, HandleDamaged, HandleDeath, BeginDestructionCamera, PlayExplosion, GetEffectLifetime, ApplyDamagedAppearance, DisableControls, BeginFalling, DisableComponent
- 설정 필드 — _explosionPrefab, _explosionScale, _damagedColor
- 연결 — Assets/Features/Fighter/Prefabs/Fighter.prefab → Fighter

### Afterburner (69줄)

[Afterburner.cs](<../../Assets/Features/Fighter/Scripts/Movement/Afterburner.cs>)

- 메서드 — Tick, Stop, OnDisable
- 설정 필드 — _accelerationMultiplier, _maxSpeedMultiplier, _duration, _recoveryPerSecond, _recoveryDelay
- 연결 — Assets/Features/Fighter/Prefabs/Fighter.prefab → Fighter

### FighterEngine (40줄)

[FighterEngine.cs](<../../Assets/Features/Fighter/Scripts/Movement/FighterEngine.cs>)

- 메서드 — Start, Update, RefreshState, OnDisable
- 설정 필드 — _seat, _movement
- 연결 — Assets/Features/Fighter/Prefabs/Fighter.prefab → Fighter

### JetMovement (229줄)

[JetMovement.cs](<../../Assets/Features/Fighter/Scripts/Movement/JetMovement.cs>)

- 메서드 — SetEngineRunning, ApplyTurnEnergyLoss, QueueEnergyLoss, StopImmediately, Awake, OnDisable, Update, FixedUpdate
- 설정 필드 — _acceleration, _deceleration, _maxSpeed, _minSpeed, _stopSpeedThreshold, _fallRecoveryAcceleration, _velocityAlignmentSpeed
- 연결 — Assets/Scenes/SampleScene.unity → [Prefab instance] / Assets/Features/Fighter/Prefabs/Fighter.prefab → Fighter

### JetPhysics (42줄)

[JetPhysics.cs](<../../Assets/Features/Fighter/Scripts/Movement/JetPhysics.cs>)

- 메서드 — Awake, FixedUpdate
- 설정 필드 — _mass, _linearDamping
- 연결 — Assets/Features/Fighter/Prefabs/Fighter.prefab → Fighter

### JetRotation (143줄)

[JetRotation.cs](<../../Assets/Features/Fighter/Scripts/Movement/JetRotation.cs>)

- 메서드 — Awake, FixedUpdate, ApplyAngularVelocity, OnDisable, CalculateControlMultiplier, ApplyTurnEnergyLoss, ApplyThrustVectoringEnergyLoss
- 설정 필드 — _rotationSpeed, _pitchSpeed, _rollSpeed, _yawSpeed, _minimumControlSpeed, _fullControlSpeed, _turnEnergyLossPerSecond, _pitchEnergyLossMultiplier, _rollEnergyLossMultiplier, _yawEnergyLossMultiplier
- 연결 — Assets/Features/Fighter/Prefabs/Fighter.prefab → Fighter

### ThrustVectoring (109줄)

[ThrustVectoring.cs](<../../Assets/Features/Fighter/Scripts/Movement/ThrustVectoring.cs>)

- 메서드 — Awake, GetPitchControlMultiplier, GetYawControlMultiplier, CalculateEnergyLoss, GetControlMultiplier
- 설정 필드 — _maximumActivationSpeed, _brakeInputThreshold, _minimumControlAuthority, _pitchControlMultiplier, _yawControlMultiplier, _velocityAlignmentMultiplier, _baseEnergyLossPerSecond, _inputEnergyLossPerSecond
- 연결 — Assets/Features/Fighter/Prefabs/Fighter.prefab → Fighter

### JetEngineVfx (173줄)

[JetEngineVfx.cs](<../../Assets/Features/Fighter/Scripts/VFX/JetEngineVfx.cs>)

- 메서드 — Awake, Update, OnDisable, CreateEffects, SetPlaying, ApplyMultipliers
- 설정 필드 — _effectPrefab, _engineAnchors, _normalSizeMultiplier, _afterburnerSizeMultiplier, _afterburnerEmissionMultiplier, _afterburnerLifetimeMultiplier
- 연결 — Assets/Features/Fighter/Prefabs/Fighter.prefab → Fighter

## Assets/Features/PlayerCharacter

### GrenadeHandlingAnimationAuthoring (177줄)

[GrenadeHandlingAnimationAuthoring.cs](<../../Assets/Features/PlayerCharacter/Editor/GrenadeHandlingAnimationAuthoring.cs>)

- 메서드 — Ensure, AddState, AssignMissing, CreateClip, MuscleProperty, Ready
- 설정 필드 — 인라인 SerializeField 없음; 파일의 다른 선언 확인
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### GrenadeThrowCompositionSetup (110줄)

[GrenadeThrowCompositionSetup.cs](<../../Assets/Features/PlayerCharacter/Editor/GrenadeThrowCompositionSetup.cs>)

- 메서드 — Apply, Import
- 설정 필드 — 인라인 SerializeField 없음; 파일의 다른 선언 확인
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### InfantryTestTools (64줄)

[InfantryTestTools.cs](<../../Assets/Features/PlayerCharacter/Editor/InfantryTestTools.cs>)

- 메서드 — 메서드 없는 계약/데이터 또는 별도 구문 확인
- 설정 필드 — 인라인 SerializeField 없음; 파일의 다른 선언 확인
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### LocalArmedLocomotionSetup (33줄)

[LocalArmedLocomotionSetup.cs](<../../Assets/Features/PlayerCharacter/Editor/LocalArmedLocomotionSetup.cs>)

- 메서드 — Apply, OnProcessScene
- 설정 필드 — 인라인 SerializeField 없음; 파일의 다른 선언 확인
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### LocalProneAnimationSetup (120줄)

[LocalProneAnimationSetup.cs](<../../Assets/Features/PlayerCharacter/Editor/LocalProneAnimationSetup.cs>)

- 메서드 — Import, Apply, OnProcessScene
- 설정 필드 — 인라인 SerializeField 없음; 파일의 다른 선언 확인
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### LocalSlideAnimationSetup (56줄)

[LocalSlideAnimationSetup.cs](<../../Assets/Features/PlayerCharacter/Editor/LocalSlideAnimationSetup.cs>)

- 메서드 — Apply, OnProcessScene
- 설정 필드 — 인라인 SerializeField 없음; 파일의 다른 선언 확인
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### LocalTacticalActionSetup (63줄)

[LocalTacticalActionSetup.cs](<../../Assets/Features/PlayerCharacter/Editor/LocalTacticalActionSetup.cs>)

- 메서드 — Apply, OnProcessScene
- 설정 필드 — 인라인 SerializeField 없음; 파일의 다른 선언 확인
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### MixamoArmedLocomotionImporter (92줄)

[MixamoArmedLocomotionImporter.cs](<../../Assets/Features/PlayerCharacter/Editor/MixamoArmedLocomotionImporter.cs>)

- 메서드 — Import
- 설정 필드 — 인라인 SerializeField 없음; 파일의 다른 선언 확인
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### MixamoSlideImporter (99줄)

[MixamoSlideImporter.cs](<../../Assets/Features/PlayerCharacter/Editor/MixamoSlideImporter.cs>)

- 메서드 — ImportRunningSlide, RestoreOriginalRunningSlide, UseRunningSlideLowerBody, UseForwardSlideEntry, Import
- 설정 필드 — 인라인 SerializeField 없음; 파일의 다른 선언 확인
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### PlayerCharacterAimDiagnostics (100줄)

[PlayerCharacterAimDiagnostics.cs](<../../Assets/Features/PlayerCharacter/Editor/PlayerCharacterAimDiagnostics.cs>)

- 메서드 — OnPlayMode, Begin, Stop, WriteStatus, Observe
- 설정 필드 — 인라인 SerializeField 없음; 파일의 다른 선언 확인
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### TacticalActionImporter (114줄)

[TacticalActionImporter.cs](<../../Assets/Features/PlayerCharacter/Editor/TacticalActionImporter.cs>)

- 메서드 — Import
- 설정 필드 — 인라인 SerializeField 없음; 파일의 다른 선언 확인
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### ArmedLocomotionAnimationSet (23줄)

[ArmedLocomotionAnimationSet.cs](<../../Assets/Features/PlayerCharacter/Scripts/ArmedLocomotionAnimationSet.cs>)

- 메서드 — Find
- 설정 필드 — _equipment, _movement, _entries
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### EquippedAnimationControllers (42줄)

[EquippedAnimationControllers.cs](<../../Assets/Features/PlayerCharacter/Scripts/EquippedAnimationControllers.cs>)

- 메서드 — Resolve, Dispose
- 설정 필드 — 인라인 SerializeField 없음; 파일의 다른 선언 확인
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### FirstPersonArmPose (61줄)

[FirstPersonArmPose.cs](<../../Assets/Features/PlayerCharacter/Scripts/FirstPersonArmPose.cs>)

- 메서드 — Apply
- 설정 필드 — 인라인 SerializeField 없음; 파일의 다른 선언 확인
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### FirstPersonArmsView (204줄)

[FirstPersonArmsView.cs](<../../Assets/Features/PlayerCharacter/Scripts/FirstPersonArmsView.cs>)

- 메서드 — SetTacticalPresentation, SetPronePresentation, Awake, Initialize, LateUpdate, CopyPose, FitPronePistolAim, OnDisable, OnDestroy
- 설정 필드 — _source, _renderer, _animator, _characterRoot, _eye, _poseAnchor, _poseEuler, _view, _combat, _offset, _adsOffset, _motionSource, _unarmedOffset, _meleeMesh, _reloadPose, _adsShoulder, _adsElbowHint, _prone, _tactical, _pronePistolAimDistance
- 연결 — Assets/Scenes/SampleScene.unity → FirstPersonArms

### HumanoidArmSolver (31줄)

[HumanoidArmSolver.cs](<../../Assets/Features/PlayerCharacter/Scripts/HumanoidArmSolver.cs>)

- 메서드 — Solve
- 설정 필드 — 인라인 SerializeField 없음; 파일의 다른 선언 확인
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### InfantryLifeState (139줄)

[InfantryLifeState.cs](<../../Assets/Features/PlayerCharacter/Scripts/InfantryLifeState.cs>)

- 메서드 — Awake, OnEnable, Start, Update, Tick, OnDeath, TryHandleDamage, GiveUp, TryClaimRevive, ReleaseRevive, TryRevive, TryRespawn, TryDrag, OnDisable
- 설정 필드 — _stance, _movement, _combat, _occupant, _bleedOutDuration, _downedHealth, _reviveHealthFraction
- 연결 — Assets/Scenes/SampleScene.unity → PlayerCharacter

### InfantryTrainingDummy (16줄)

[InfantryTrainingDummy.cs](<../../Assets/Features/PlayerCharacter/Scripts/InfantryTrainingDummy.cs>)

- 메서드 — Initialize, Update, OnAnimatorIK
- 설정 필드 — 인라인 SerializeField 없음; 파일의 다른 선언 확인
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### IPlayerCharacterAimSource (9줄)

[IPlayerCharacterAimSource.cs](<../../Assets/Features/PlayerCharacter/Scripts/IPlayerCharacterAimSource.cs>)

- 메서드 — 메서드 없는 계약/데이터 또는 별도 구문 확인
- 설정 필드 — 인라인 SerializeField 없음; 파일의 다른 선언 확인
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### IPlayerCharacterCombatInput (10줄)

[IPlayerCharacterCombatInput.cs](<../../Assets/Features/PlayerCharacter/Scripts/IPlayerCharacterCombatInput.cs>)

- 메서드 — 메서드 없는 계약/데이터 또는 별도 구문 확인
- 설정 필드 — 인라인 SerializeField 없음; 파일의 다른 선언 확인
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### IPlayerCharacterInput (11줄)

[IPlayerCharacterInput.cs](<../../Assets/Features/PlayerCharacter/Scripts/IPlayerCharacterInput.cs>)

- 메서드 — 메서드 없는 계약/데이터 또는 별도 구문 확인
- 설정 필드 — 인라인 SerializeField 없음; 파일의 다른 선언 확인
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### IPlayerCharacterInteractionInput (8줄)

[IPlayerCharacterInteractionInput.cs](<../../Assets/Features/PlayerCharacter/Scripts/IPlayerCharacterInteractionInput.cs>)

- 메서드 — 메서드 없는 계약/데이터 또는 별도 구문 확인
- 설정 필드 — 인라인 SerializeField 없음; 파일의 다른 선언 확인
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### IPlayerCharacterLifeInput (9줄)

[IPlayerCharacterLifeInput.cs](<../../Assets/Features/PlayerCharacter/Scripts/IPlayerCharacterLifeInput.cs>)

- 메서드 — 메서드 없는 계약/데이터 또는 별도 구문 확인
- 설정 필드 — 인라인 SerializeField 없음; 파일의 다른 선언 확인
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### IPlayerCharacterStanceInput (7줄)

[IPlayerCharacterStanceInput.cs](<../../Assets/Features/PlayerCharacter/Scripts/IPlayerCharacterStanceInput.cs>)

- 메서드 — 메서드 없는 계약/데이터 또는 별도 구문 확인
- 설정 필드 — 인라인 SerializeField 없음; 파일의 다른 선언 확인
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### IPlayerCharacterTacticalInput (8줄)

[IPlayerCharacterTacticalInput.cs](<../../Assets/Features/PlayerCharacter/Scripts/IPlayerCharacterTacticalInput.cs>)

- 메서드 — 메서드 없는 계약/데이터 또는 별도 구문 확인
- 설정 필드 — 인라인 SerializeField 없음; 파일의 다른 선언 확인
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### IPlayerCharacterThrowInput (15줄)

[IPlayerCharacterThrowInput.cs](<../../Assets/Features/PlayerCharacter/Scripts/IPlayerCharacterThrowInput.cs>)

- 메서드 — 메서드 없는 계약/데이터 또는 별도 구문 확인
- 설정 필드 — 인라인 SerializeField 없음; 파일의 다른 선언 확인
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### IPlayerCharacterTraversalInput (8줄)

[IPlayerCharacterTraversalInput.cs](<../../Assets/Features/PlayerCharacter/Scripts/IPlayerCharacterTraversalInput.cs>)

- 메서드 — 메서드 없는 계약/데이터 또는 별도 구문 확인
- 설정 필드 — 인라인 SerializeField 없음; 파일의 다른 선언 확인
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### IPlayerCharacterViewInput (15줄)

[IPlayerCharacterViewInput.cs](<../../Assets/Features/PlayerCharacter/Scripts/IPlayerCharacterViewInput.cs>)

- 메서드 — 메서드 없는 계약/데이터 또는 별도 구문 확인
- 설정 필드 — 인라인 SerializeField 없음; 파일의 다른 선언 확인
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### MantleDefinition (19줄)

[MantleDefinition.cs](<../../Assets/Features/PlayerCharacter/Scripts/MantleDefinition.cs>)

- 메서드 — 메서드 없는 계약/데이터 또는 별도 구문 확인
- 설정 필드 — _animation, _minimumHeightFraction, _maximumHeightFraction, _reach, _reachDuration
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### PlayerCharacterActionPolicy (42줄)

[PlayerCharacterActionPolicy.cs](<../../Assets/Features/PlayerCharacter/Scripts/PlayerCharacterActionPolicy.cs>)

- 메서드 — SetTacticalActions, Awake
- 설정 필드 — _health, _view, _combat, _traversal, _revive, _mantle, _grenade
- 연결 — Assets/Scenes/SampleScene.unity → PlayerCharacter

### PlayerCharacterAnimationView (110줄)

[PlayerCharacterAnimationView.cs](<../../Assets/Features/PlayerCharacter/Scripts/PlayerCharacterAnimationView.cs>)

- 메서드 — SetMantle, SetSlideAnimations, Awake, Update, UpdateTraversal, OnDisable
- 설정 필드 — _movement, _animator, _stance, _life, _revive, _traversal, _slideAnimations, _mantle, _speedDamping
- 연결 — Assets/Scenes/SampleScene.unity → [Prefab instance]

### PlayerCharacterCamera (197줄)

[PlayerCharacterCamera.cs](<../../Assets/Features/PlayerCharacter/Scripts/PlayerCharacterCamera.cs>)

- 메서드 — Awake, OnEnable, Start, LateUpdate, GetAimRay, GetThirdPersonPivot, GetThirdPersonOffset, GetEyeOffset, GetSafeDistance, OnDisable
- 설정 필드 — _target, _view, _stance, _lean, _offset, _lookOffset, _firstPersonOffset, _aimOffset, _followSpeed, _normalFieldOfView, _aimFieldOfView, _adsFieldOfView, _collisionRadius, _collisionPadding, _collisionMask, _movement, _actions, _traversal, _motionProfile
- 연결 — Assets/Scenes/SampleScene.unity → Main Camera

### PlayerCharacterCombatView (253줄)

[PlayerCharacterCombatView.cs](<../../Assets/Features/PlayerCharacter/Scripts/PlayerCharacterCombatView.cs>)

- 메서드 — SetPronePresentation, SetTacticalPresentation, SetLocomotionAnimations, WeaponPoseRotation, Awake, OnEnable, RefreshEquipment, AttachWeapon, TryGetSupportHandPose, LateUpdate, Play, OnFired, OnReload, OnMelee, OnEquipmentChanged, ResetPose, OnDisable, OnDestroy
- 설정 필드 — _combat, _view, _animator, _actions, _pistolPrefab, _firstPersonArms, _firstPersonRig, _gripPosition, _gripEuler, _reloadAnimation, _prone, _tactical, _locomotionAnimations
- 연결 — Assets/Scenes/SampleScene.unity → [Prefab instance]

### PlayerCharacterController (221줄)

[PlayerCharacterController.cs](<../../Assets/Features/PlayerCharacter/Scripts/PlayerCharacterController.cs>)

- 메서드 — BindMuzzle, ResetForRespawn, Awake, OnEnable, OnLoadoutChanged, Update, Equip, TryMelee, CancelActions, OnDisable, TryFire, FindNearestSurface
- 설정 필드 — _bulletPool, _fireRate, _damage, _muzzleOffset, _aimSourceBehaviour, _aimDistance, _aimMask, _magazine, _loadout, _fistAttack, _pistolAttack, _actions
- 연결 — Assets/Scenes/SampleScene.unity → PlayerCharacter

### PlayerCharacterGrenadeThrow (223줄)

[PlayerCharacterGrenadeThrow.cs](<../../Assets/Features/PlayerCharacter/Scripts/PlayerCharacterGrenadeThrow.cs>)

- 메서드 — Awake, Initialize, OnEquipmentChanged, OnLifeChanged, Update, TryEquip, TryBeginAim, TryThrow, TryToggleThrowMode, Tick, Release, TryGetAimTrajectory, GetLaunch, Clear, IsOwnedCollider, AttachHeldVisual, Cancel, OnDisable, OnDestroy
- 설정 필드 — _definition, _capacity, _combat, _stance, _movement, _actions, _aimSourceBehaviour, _effectSourceBehaviour, _life
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### PlayerCharacterInput (174줄)

[PlayerCharacterInput.cs](<../../Assets/Features/PlayerCharacter/Scripts/PlayerCharacterInput.cs>)

- 메서드 — ConsumePrimaryUntilRelease, ConsumeAimUntilRelease, Awake, SetGameplayEnabled, OnEnable, Update, SetCaptured, OnApplicationFocus, OnDisable, OnDestroy
- 설정 필드 — _inputActions
- 연결 — Assets/Scenes/SampleScene.unity → PlayerCharacter

### PlayerCharacterJumpPoseState (38줄)

[PlayerCharacterJumpPoseState.cs](<../../Assets/Features/PlayerCharacter/Scripts/PlayerCharacterJumpPoseState.cs>)

- 메서드 — Tick, Reset
- 설정 필드 — 인라인 SerializeField 없음; 파일의 다른 선언 확인
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### PlayerCharacterLean (42줄)

[PlayerCharacterLean.cs](<../../Assets/Features/PlayerCharacter/Scripts/PlayerCharacterLean.cs>)

- 메서드 — Awake, Update, Tick, OnDisable
- 설정 필드 — _camera, _actions, _distance, _angle, _speed, _radius, _obstacleMask
- 연결 — Assets/Scenes/SampleScene.unity → PlayerCharacter

### PlayerCharacterLoadout (63줄)

[PlayerCharacterLoadout.cs](<../../Assets/Features/PlayerCharacter/Scripts/PlayerCharacterLoadout.cs>)

- 메서드 — GetEquipment, Awake, OnEnable, OnEquipmentChanged, TryEquip, ResetForRespawn, OnDisable
- 설정 필드 — _slot, _equipment, _weapons
- 연결 — Assets/Scenes/SampleScene.unity → PlayerCharacter

### PlayerCharacterMantle (81줄)

[PlayerCharacterMantle.cs](<../../Assets/Features/PlayerCharacter/Scripts/PlayerCharacterMantle.cs>)

- 메서드 — Awake, Initialize, Step, TryBegin, HasSpace, Ray, Cancel, OnDisable
- 설정 필드 — _definition, _stance, _traversal, _actions
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### PlayerCharacterMotionProfile (62줄)

[PlayerCharacterMotionProfile.cs](<../../Assets/Features/PlayerCharacter/Scripts/PlayerCharacterMotionProfile.cs>)

- 메서드 — 메서드 없는 계약/데이터 또는 별도 구문 확인
- 설정 필드 — _walkStride, _sprintStride, _walkCameraBob, _sprintCameraBob, _cameraRoll, _adsMotionMultiplier, _weaponBob, _lookSway, _maximumSway, _sprintOffset, _sprintEuler, _response, _aimResponse, _landingImpulse, _maximumLandingImpulse, _landingRecovery, _jumpVerticalLag, _maximumJumpLag, _jumpRecovery, _landingCameraMultiplier, _jumpPitchPerMeter, _landingPitchPerMeter, _thirdPersonJumpMultiplier
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### PlayerCharacterMotionState (110줄)

[PlayerCharacterMotionState.cs](<../../Assets/Features/PlayerCharacter/Scripts/PlayerCharacterMotionState.cs>)

- 메서드 — Reset, Tick
- 설정 필드 — 인라인 SerializeField 없음; 파일의 다른 선언 확인
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### PlayerCharacterMovement (120줄)

[PlayerCharacterMovement.cs](<../../Assets/Features/PlayerCharacter/Scripts/PlayerCharacterMovement.cs>)

- 메서드 — SetMantle, Awake, Update, Tick, Stop, OnDisable, QueueExternalDisplacement
- 설정 필드 — _moveSpeed, _sprintSpeed, _jumpHeight, _stance, _actions, _traversal, _acceleration, _deceleration, _airAcceleration, _mantle
- 연결 — Assets/Scenes/SampleScene.unity → PlayerCharacter

### PlayerCharacterPostureView (99줄)

[PlayerCharacterPostureView.cs](<../../Assets/Features/PlayerCharacter/Scripts/PlayerCharacterPostureView.cs>)

- 메서드 — SetPronePresentation, Awake, Apply, LateUpdate, OnDisable
- 설정 필드 — _stance, _root, _body, _life, _lean, _revive, _traversal, _skin, _slideContactData, _bodyGroundClearance, _eliminatedGroundClearance, _reviveGroundClearance, _blendSpeed, _authoredProne
- 연결 — Assets/Scenes/SampleScene.unity → [Prefab instance]

### PlayerCharacterProneAnimationView (188줄)

[PlayerCharacterProneAnimationView.cs](<../../Assets/Features/PlayerCharacter/Scripts/PlayerCharacterProneAnimationView.cs>)

- 메서드 — Initialize, OnEnable, Subscribe, Unsubscribe, EquipmentChanged, BindMagazine, ReloadStarted, MeleeStarted, Fired, Begin, LateUpdate, Release, OnDisable, OnDestroy
- 설정 필드 — _animator, _movement, _stance, _combat, _life, _animations
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### PlayerCharacterRecoil (44줄)

[PlayerCharacterRecoil.cs](<../../Assets/Features/PlayerCharacter/Scripts/PlayerCharacterRecoil.cs>)

- 메서드 — OnEnable, Update, Tick, OnFired, OnDisable
- 설정 필드 — _combat, _view, _stance, _mount, _pitchPerShot, _yawPerShot, _maximumPitch, _recoverySpeed
- 연결 — Assets/Scenes/SampleScene.unity → PlayerCharacter

### PlayerCharacterRevive (100줄)

[PlayerCharacterRevive.cs](<../../Assets/Features/PlayerCharacter/Scripts/PlayerCharacterRevive.cs>)

- 메서드 — Awake, Update, CanRevive, FindCandidate, TryBegin, Tick, Cancel, OnDisable
- 설정 필드 — _health, _team, _stance, _traversal, _range, _duration, _dragSpeed
- 연결 — Assets/Scenes/SampleScene.unity → PlayerCharacter

### PlayerCharacterRifleReloadView (160줄)

[PlayerCharacterRifleReloadView.cs](<../../Assets/Features/PlayerCharacter/Scripts/PlayerCharacterRifleReloadView.cs>)

- 메서드 — SetPronePresentation, Awake, OnEnable, OnEquipment, Bind, Begin, Completed, LateUpdate, Restore, OnDisable
- 설정 필드 — _combat, _presentation, _view, _camera, _animator, _aim, _hipViewOffset, _adsViewOffset, _prone
- 연결 — Assets/Scenes/SampleScene.unity → [Prefab instance]

### PlayerCharacterSlideAnimationView (125줄)

[PlayerCharacterSlideAnimationView.cs](<../../Assets/Features/PlayerCharacter/Scripts/PlayerCharacterSlideAnimationView.cs>)

- 메서드 — Initialize, OnEnable, Bind, OnEquipment, Update, UpdateLowerBody, ResetLowerBody, Apply, Restore, Unbind, ClearControllers, Release, OnDisable, OnDestroy
- 설정 필드 — _animator, _combat, _animations, _traversal
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### PlayerCharacterStance (170줄)

[PlayerCharacterStance.cs](<../../Assets/Features/PlayerCharacter/Scripts/PlayerCharacterStance.cs>)

- 메서드 — Awake, Update, TrySetCrouched, TrySetStance, HasClearance, ProneCapsule, ProneSpaceIsClear, AllowsHeading, ConstrainPlanarMovement, OnDisable
- 설정 필드 — _crouchHeight, _crouchMoveSpeed, _standingObstacleMask, _proneHeight, _proneMoveSpeed, _proneEyeForward, _actions, _proneEnterDuration, _proneExitDuration
- 연결 — Assets/Scenes/SampleScene.unity → PlayerCharacter

### PlayerCharacterTacticalAnimationView (134줄)

[PlayerCharacterTacticalAnimationView.cs](<../../Assets/Features/PlayerCharacter/Scripts/PlayerCharacterTacticalAnimationView.cs>)

- 메서드 — Awake, CachePose, Initialize, Update, ApplyController, LateUpdate, UpdateHeldVisual, OnDisable, OnDestroy
- 설정 필드 — _animator, _profile, _mantle, _throw, _stance, _view, _actions, _contactData, _skin
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### PlayerCharacterTraversal (140줄)

[PlayerCharacterTraversal.cs](<../../Assets/Features/PlayerCharacter/Scripts/PlayerCharacterTraversal.cs>)

- 메서드 — Awake, Step, TryBeginVault, AdvanceVault, Ray, HasSpace, FinishSlide, Cancel, OnDisable
- 설정 필드 — _movement, _stance, _actions, _slideDuration, _slideEndSpeed, _vaultReach, _maximumVaultHeight, _maximumVaultDepth, _vaultDuration, _obstacleMask
- 연결 — Assets/Scenes/SampleScene.unity → PlayerCharacter

### PlayerCharacterView (181줄)

[PlayerCharacterView.cs](<../../Assets/Features/PlayerCharacter/Scripts/PlayerCharacterView.cs>)

- 메서드 — ClampAimPitch, ClampFreePitch, Awake, OnEnable, Update, UpdateProneBlend, Tick, ResetTransientState, OnDisable
- 설정 필드 — _lookSensitivity, _pitchLimit, _freeLookYawLimit, _firstPersonFreeLookYawLimit, _freeLookReturnSpeed, _adsTapWindow, _stance, _actions, _recoil, _proneCamera
- 연결 — Assets/Scenes/SampleScene.unity → PlayerCharacter

### PlayerCharacterVisibility (56줄)

[PlayerCharacterVisibility.cs](<../../Assets/Features/PlayerCharacter/Scripts/PlayerCharacterVisibility.cs>)

- 메서드 — Awake, LateUpdate, OnDisable
- 설정 필드 — _view, _camera, _renderers, _hideDistance
- 연결 — Assets/Scenes/SampleScene.unity → PlayerCharacter

### PlayerCharacterWeaponAimView (449줄)

[PlayerCharacterWeaponAimView.cs](<../../Assets/Features/PlayerCharacter/Scripts/PlayerCharacterWeaponAimView.cs>)

- 메서드 — SetTacticalPresentation, SetPronePresentation, Awake, OnAnimatorIK, GetReloadGrip, CalculateGrip, GetSupportPose, ApplyGrip, StabilizeArmedTorso, StabilizeLocomotionTorso, LocomotionTorsoCorrection, CompressGaitAngle, ApplyTorsoRotation, RotateBoneWorld, WithinReach, ApplyUpperBodyAim, LateUpdate, FitProneItemContact, SetWeight, OnDisable
- 설정 필드 — _combat, _presentation, _view, _camera, _animator, _posture, _mount, _actions, _rightHandOffset, _leftHandOffset, _leftHandEuler, _bodyAimWeight, _headAimWeight, _lookAtClamp, _poseFollowSpeed, _minimumDownwardReach, _prone, _tactical
- 연결 — Assets/Scenes/SampleScene.unity → [Prefab instance]

### PlayerCharacterWeaponEquipment (72줄)

[PlayerCharacterWeaponEquipment.cs](<../../Assets/Features/PlayerCharacter/Scripts/PlayerCharacterWeaponEquipment.cs>)

- 메서드 — Awake, Initialize, TryEquip, ApplyDefinition, Update, ResetAmmo, OnDisable
- 설정 필드 — _definition, _existingMagazine
- 연결 — Assets/Scenes/SampleScene.unity → PlayerCharacter/Equipment/Pistol / Assets/Scenes/SampleScene.unity → PlayerCharacter/Equipment/Rifle

### PlayerCharacterWeaponMount (58줄)

[PlayerCharacterWeaponMount.cs](<../../Assets/Features/PlayerCharacter/Scripts/PlayerCharacterWeaponMount.cs>)

- 메서드 — Awake, Update, Tick, Release, OnDisable
- 설정 필드 — _camera, _view, _movement, _combat, _actions, _reach, _recoilMultiplier, _coverMask
- 연결 — Assets/Scenes/SampleScene.unity → PlayerCharacter

### PlayerCharacterWeaponPoseState (37줄)

[PlayerCharacterWeaponPoseState.cs](<../../Assets/Features/PlayerCharacter/Scripts/PlayerCharacterWeaponPoseState.cs>)

- 메서드 — Reset, RequireReady, Tick, Rotation
- 설정 필드 — 인라인 SerializeField 없음; 파일의 다른 선언 확인
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### PlayerRifleStationInteraction (70줄)

[PlayerRifleStationInteraction.cs](<../../Assets/Features/PlayerCharacter/Scripts/PlayerRifleStationInteraction.cs>)

- 메서드 — Awake, Update, FindNearest, HasSight, TryUseNearest, OnDisable
- 설정 필드 — _inputBehaviour, _combat, _loadout, _actions, _vehicles, _revive, _stations
- 연결 — Assets/Scenes/SampleScene.unity → PlayerCharacter

### PlayerVehicleInteraction (55줄)

[PlayerVehicleInteraction.cs](<../../Assets/Features/PlayerCharacter/Scripts/PlayerVehicleInteraction.cs>)

- 메서드 — Awake, Update, FindNearestSeat
- 설정 필드 — _input, _occupant, _seats, _revive
- 연결 — Assets/Scenes/SampleScene.unity → PlayerCharacter

### PlayerVehicleOccupant (119줄)

[PlayerVehicleOccupant.cs](<../../Assets/Features/PlayerCharacter/Scripts/PlayerVehicleOccupant.cs>)

- 메서드 — Awake, Start, TryAttachToSeat, DetachFromSeat, RestoreStanding, OnDisable
- 설정 필드 — _input, _seatedPose, _onFootComponents, _vehicleCamera, _vehicleHud
- 연결 — Assets/Scenes/SampleScene.unity → PlayerCharacter

### ProneAnimationSet (27줄)

[ProneAnimationSet.cs](<../../Assets/Features/PlayerCharacter/Scripts/ProneAnimationSet.cs>)

- 메서드 — GetClip
- 설정 필드 — _idle, _forward, _backward, _left, _right, _enter, _exit, _fire, _reload, _death, _armedEnter, _armedExit, _meleeMask, _meleeWeapon, _meleeJab, _meleeCross, _retargetStandingMelee, _crawlPlaybackSpeed
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### ProneCameraProfile (25줄)

[ProneCameraProfile.cs](<../../Assets/Features/PlayerCharacter/Scripts/ProneCameraProfile.cs>)

- 메서드 — 메서드 없는 계약/데이터 또는 별도 구문 확인
- 설정 필드 — _pivotFromFeet, _thirdPersonOffset, _aimOffset, _aimPitchUp, _aimPitchDown, _freePitchUp, _freePitchDown, _aimYawLimit
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### RifleSelectionStation (16줄)

[RifleSelectionStation.cs](<../../Assets/Features/PlayerCharacter/Scripts/RifleSelectionStation.cs>)

- 메서드 — IsInRange
- 설정 필드 — _definition, _interactionPoint, _range
- 연결 — Assets/Scenes/SampleScene.unity → RifleSelectionStations/AK47_Station / Assets/Scenes/SampleScene.unity → RifleSelectionStations/M4A1_Station

### SeatedAnimationPlayback (61줄)

[SeatedAnimationPlayback.cs](<../../Assets/Features/PlayerCharacter/Scripts/SeatedAnimationPlayback.cs>)

- 메서드 — Sample, Dispose
- 설정 필드 — 인라인 SerializeField 없음; 파일의 다른 선언 확인
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### SeatedCharacterPose (239줄)

[SeatedCharacterPose.cs](<../../Assets/Features/PlayerCharacter/Scripts/SeatedCharacterPose.cs>)

- 메서드 — Awake, BeginPose, EndPose, LateUpdate, OnDisable, CacheRig, HasRequiredBones, CaptureBonePose, RestoreBonePose, ApplyPose, TryGetPelvisFrame, PoseLeg, PoseArm, AlignBone, Bone
- 설정 필드 — _animator, _seatedAnimation, _torsoRecline, _thighDirection, _lowerLegDirection, _upperArmDirection, _forearmDirection
- 연결 — Assets/Scenes/SampleScene.unity → PlayerCharacter

### SkinnedPoseContactData (105줄)

[SkinnedPoseContactData.cs](<../../Assets/Features/PlayerCharacter/Scripts/SkinnedPoseContactData.cs>)

- 메서드 — CreateSampler, TryGetLowestHeight, WorldY, BakeFrom
- 설정 필드 — _sourceMesh, _boneCount, _points
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### SlideAnimationSet (42줄)

[SlideAnimationSet.cs](<../../Assets/Features/PlayerCharacter/Scripts/SlideAnimationSet.cs>)

- 메서드 — TryApply
- 설정 필드 — _originalStart, _originalLoop, _originalExit, _start, _loop, _exit, _lowerBodyOnly
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### TacticalAnimationSet (56줄)

[TacticalAnimationSet.cs](<../../Assets/Features/PlayerCharacter/Scripts/TacticalAnimationSet.cs>)

- 메서드 — IsApplied, Restore, Apply
- 설정 필드 — _mantlePlaceholder, _throwPlaceholder, _equipPlaceholder, _idlePlaceholder, _leftThrowPlaceholder, _mantle, _grenade
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

## Assets/Features/Projectile

### Bullet (54줄)

[Bullet.cs](<../../Assets/Features/Projectile/Scripts/Bullet.cs>)

- 메서드 — Awake, Initialize, InitializeWithTracer, OnAdvanced, OnDisable, OnDestroy, OnHit
- 설정 필드 — _tracer
- 연결 — Assets/Features/Projectile/Prefabs/Bullet.prefab → Bullet

### BulletPool (22줄)

[BulletPool.cs](<../../Assets/Features/Projectile/Scripts/BulletPool.cs>)

- 메서드 — Create
- 설정 필드 — _prefab, _hitEffectPlayer
- 연결 — Assets/Scenes/SampleScene.unity → ObjectPools/BulletPool

### BulletTracer (70줄)

[BulletTracer.cs](<../../Assets/Features/Projectile/Scripts/BulletTracer.cs>)

- 메서드 — Awake, Begin, Advance, Stop, Update
- 설정 필드 — _length, _fadeDuration, _widthMultiplier
- 연결 — Assets/Features/Projectile/Prefabs/Bullet.prefab → Bullet/Tracer

### BulletVisual (26줄)

[BulletVisual.cs](<../../Assets/Features/Projectile/Scripts/BulletVisual.cs>)

- 메서드 — Awake, OnEnable, SetMesh
- 설정 필드 — 인라인 SerializeField 없음; 파일의 다른 선언 확인
- 연결 — Assets/Features/Projectile/Prefabs/Bullet.prefab → Bullet

### GrenadeProjectile (75줄)

[GrenadeProjectile.cs](<../../Assets/Features/Projectile/Scripts/GrenadeProjectile.cs>)

- 메서드 — Awake, Arm, FixedUpdate, Detonate, OnDisable
- 설정 필드 — 인라인 SerializeField 없음; 파일의 다른 선언 확인
- 연결 — Assets/Features/Projectile/Prefabs/FragGrenade.prefab → FragGrenade

### HomingMissile (169줄)

[HomingMissile.cs](<../../Assets/Features/Projectile/Scripts/HomingMissile.cs>)

- 메서드 — FixedUpdate, Initialize, SetExplosionEffectPlayer, OnSpawn, OnDespawn, RotateToward, OnHit, TryDetonateProximityFuse, PlayExplosion, IsHostileTarget, ResetGuidance, ClearWarningReceiver
- 설정 필드 — _turnSpeed, _maximumSpeed, _acceleration, _proximityFuseRadius
- 연결 — Assets/Features/Projectile/Prefabs/HomingMissile.prefab → HomingMissile

### HomingMissilePool (22줄)

[HomingMissilePool.cs](<../../Assets/Features/Projectile/Scripts/HomingMissilePool.cs>)

- 메서드 — Create
- 설정 필드 — _prefab, _hitEffectPlayer, _explosionPool
- 연결 — Assets/Scenes/SampleScene.unity → ObjectPools/HomingMissilePool

### MissileFlightAudio (48줄)

[MissileFlightAudio.cs](<../../Assets/Features/Projectile/Scripts/MissileFlightAudio.cs>)

- 메서드 — Awake, OnEnable, LateUpdate, OnDisable
- 설정 필드 — _source, _profile
- 연결 — Assets/Features/Projectile/Prefabs/HomingMissile.prefab → HomingMissile

### ProjectileBase (163줄)

[ProjectileBase.cs](<../../Assets/Features/Projectile/Scripts/ProjectileBase.cs>)

- 메서드 — Awake, FixedUpdate, SetVelocity, SetHitEffectPlayer, MoveProjectile, OnAdvanced, Initialize, OnSpawn, OnDespawn, HandleHit, ShouldIgnore, IsAlly, OnHit, LifeExpired, DestroyProjectile
- 설정 필드 — _speed, _lifeTime, _gravityScale, _hitMask
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### ProjectileData (16줄)

[ProjectileData.cs](<../../Assets/Features/Projectile/Scripts/ProjectileData.cs>)

- 메서드 — 메서드 없는 계약/데이터 또는 별도 구문 확인
- 설정 필드 — 인라인 SerializeField 없음; 파일의 다른 선언 확인
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

## Assets/Features/Targeting

### AirTarget (32줄)

[AirTarget.cs](<../../Assets/Features/Targeting/Scripts/AirTarget.cs>)

- 메서드 — Awake, OnEnable, OnDisable
- 설정 필드 — 인라인 SerializeField 없음; 파일의 다른 선언 확인
- 연결 — Assets/Scenes/SampleScene.unity → Dummys/Dummy_Enemy / Assets/Scenes/SampleScene.unity → [Prefab instance] / Assets/Scenes/SampleScene.unity → MissileTargetDummy_Enemy / Assets/Scenes/SampleScene.unity → Dummys/Dummy_Neutral / Assets/Features/TrainingGround/InfantryTrainingGround.prefab → InfantryTrainingGround/Enemy_50m / Assets/Features/TrainingGround/InfantryTrainingGround.prefab → InfantryTrainingGround/Ally_50m / Assets/Features/TrainingGround/InfantryTrainingGround.prefab → InfantryTrainingGround/Ally_10m / Assets/Features/TrainingGround/InfantryTrainingGround.prefab → InfantryTrainingGround/Enemy_25m / Assets/Features/TrainingGround/InfantryTrainingGround.prefab → InfantryTrainingGround/Neutral_10m / Assets/Features/TrainingGround/InfantryTrainingGround.prefab → InfantryTrainingGround/Neutral_50m / Assets/Features/TrainingGround/InfantryTrainingGround.prefab → InfantryTrainingGround/Neutral_25m / Assets/Features/TrainingGround/InfantryTrainingGround.prefab → InfantryTrainingGround/Ally_25m / Assets/Features/TrainingGround/InfantryTrainingGround.prefab → InfantryTrainingGround/Enemy_10m / Assets/Features/Fighter/Prefabs/Fighter.prefab → Fighter

### AirTargetLock (82줄)

[AirTargetLock.cs](<../../Assets/Features/Targeting/Scripts/AirTargetLock.cs>)

- 메서드 — Awake, Update, OnDisable, SetSearchingEnabled, ResetLock
- 설정 필드 — _searchOrigin, _lockDuration, _scanner, _screenArea
- 연결 — Assets/Scenes/SampleScene.unity → [Prefab instance] / Assets/Scenes/SampleScene.unity → PlayerCharacter / Assets/Features/Fighter/Prefabs/Fighter.prefab → Fighter

### AirTargetLockState (9줄)

[AirTargetLockState.cs](<../../Assets/Features/Targeting/Scripts/AirTargetLockState.cs>)

- 메서드 — 메서드 없는 계약/데이터 또는 별도 구문 확인
- 설정 필드 — 인라인 SerializeField 없음; 파일의 다른 선언 확인
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### AirTargetScanner (52줄)

[AirTargetScanner.cs](<../../Assets/Features/Targeting/Scripts/AirTargetScanner.cs>)

- 메서드 — FindBestTarget, IsHostile
- 설정 필드 — _maximumDistance
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### AirTargetScreenArea (42줄)

[AirTargetScreenArea.cs](<../../Assets/Features/Targeting/Scripts/AirTargetScreenArea.cs>)

- 메서드 — TryGetCenterDistance
- 설정 필드 — _worldCamera, _reticle
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### IMissileWarningReceiver (11줄)

[IMissileWarningReceiver.cs](<../../Assets/Features/Targeting/Scripts/IMissileWarningReceiver.cs>)

- 메서드 — 메서드 없는 계약/데이터 또는 별도 구문 확인
- 설정 필드 — 인라인 SerializeField 없음; 파일의 다른 선언 확인
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### MissileWarningState (10줄)

[MissileWarningState.cs](<../../Assets/Features/Targeting/Scripts/MissileWarningState.cs>)

- 메서드 — 메서드 없는 계약/데이터 또는 별도 구문 확인
- 설정 필드 — 인라인 SerializeField 없음; 파일의 다른 선언 확인
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

## Assets/Features/UI

### GrenadeTrajectoryView (153줄)

[GrenadeTrajectoryView.cs](<../../Assets/Features/UI/GrenadeTrajectoryView.cs>)

- 메서드 — Initialize, Awake, Configure, LateUpdate, StyleGuide, PixelWidth, OnDisable
- 설정 필드 — _grenade, _material, _viewCamera
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### AirTargetLockView (108줄)

[AirTargetLockView.cs](<../../Assets/Features/UI/HUD/Scripts/AirTargetLockView.cs>)

- 메서드 — OnEnable, LateUpdate, OnDisable, HideTargetMarker
- 설정 필드 — _targetLock, _weaponController, _jetCamera, _worldCamera, _lockAreaGraphic, _targetMarker, _targetMarkerGraphic, _acquiringColor, _lockedColor, _acquiringSize, _lockedSize
- 연결 — Assets/Scenes/SampleScene.unity → FighterUI/FighterHUD/MissileAimUI

### FighterCombatHudView (65줄)

[FighterCombatHudView.cs](<../../Assets/Features/UI/HUD/Scripts/FighterCombatHudView.cs>)

- 메서드 — OnEnable, OnDisable, HandleSelectionChanged, UpdateSelectedWeapon, RefreshVisibility, SetActive
- 설정 필드 — _weaponController, _gunAimUI, _missileAimUI, _overheatUI
- 연결 — Assets/Scenes/SampleScene.unity → FighterUI/FighterHUD

### FighterGunAimReticleMover (33줄)

[FighterGunAimReticleMover.cs](<../../Assets/Features/UI/HUD/Scripts/FighterGunAimReticleMover.cs>)

- 메서드 — MoveTo
- 설정 필드 — 인라인 SerializeField 없음; 파일의 다른 선언 확인
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### FighterGunAimView (167줄)

[FighterGunAimView.cs](<../../Assets/Features/UI/HUD/Scripts/FighterGunAimView.cs>)

- 메서드 — Awake, Start, OnEnable, FixedUpdate, LateUpdate, SetReticleColor, UpdateGunAimReticle, ApplyReticleColor, SetGunAimReticleVisible, SetBoresightVisible, OnValidate
- 설정 필드 — _machineGun, _ownerRigidbody, _worldCamera, _jetCamera, _boresightImage, _gunAimReticleImage, _reticleColor, _predictionDistance, _reticleMoveSpeed
- 연결 — Assets/Scenes/SampleScene.unity → FighterUI/FighterHUD/GunAimUI

### FighterHudView (76줄)

[FighterHudView.cs](<../../Assets/Features/UI/HUD/Scripts/FighterHudView.cs>)

- 메서드 — LateUpdate, UpdateSpeed, UpdateHealth
- 설정 필드 — _movement, _health, _speedText, _healthSegments
- 연결 — Assets/Scenes/SampleScene.unity → FighterUI/FighterHUD

### HeatDotRingBuilder (61줄)

[HeatDotRingBuilder.cs](<../../Assets/Features/UI/HUD/Scripts/HeatDotRingBuilder.cs>)

- 메서드 — GenerateDots, ClearChildren
- 설정 필드 — _dotSprite, _dotCount, _dotSize, _radius, _startFromTop, _defaultDotColor
- 연결 — Assets/Scenes/SampleScene.unity → FighterUI/FighterHUD/OverheatUI/OverHeatDots

### HeatDotsView (92줄)

[HeatDotsView.cs](<../../Assets/Features/UI/HUD/Scripts/HeatDotsView.cs>)

- 메서드 — CollectDotsFromChildren, LateUpdate, ColorForHeat, ColorForCooling
- 설정 필드 — _dots, _weapon, _offColor, _normalColor, _warnColor, _hotColor, _activeAlpha, _inactiveAlpha
- 연결 — Assets/Scenes/SampleScene.unity → FighterUI/FighterHUD/OverheatUI/OverHeatDots

### LockOnAreaGraphic (71줄)

[LockOnAreaGraphic.cs](<../../Assets/Features/UI/HUD/Scripts/LockOnAreaGraphic.cs>)

- 메서드 — OnPopulateMesh, AddQuad, OnValidate
- 설정 필드 — _thickness, _cornerLength
- 연결 — Assets/Scenes/SampleScene.unity → FighterUI/FighterHUD/MissileAimUI/LockOnAreaRing / Assets/Scenes/SampleScene.unity → FighterUI/FighterHUD/MissileAimUI/TargetLockMarker

### MissileDirectionIndicatorGraphic (283줄)

[MissileDirectionIndicatorGraphic.cs](<../../Assets/Features/UI/HUD/Scripts/MissileDirectionIndicatorGraphic.cs>)

- 메서드 — Awake, Update, SetThreat, OnPopulateMesh, DrawLockState, DrawIncoming, DrawSideArcs, DrawArc, GetArcPoint, DrawTriangle, GetDrawColor, AddLine, AddTriangle
- 설정 필드 — _triangleWidth, _triangleHeight, _triangleSpacing, _arcRadius, _arcLength, _arcSegments, _lineThickness, _outlineThickness, _incomingTriangleScale, _lockingColor, _dangerColor, _blinkDuration, _minimumBlinkAlpha
- 연결 — Assets/Scenes/SampleScene.unity → FighterUI/FighterHUD/MissileDirectionIndicator / Assets/Scenes/SampleScene.unity → FighterUI/FighterHUD/MissileDirectionIndicator_02 / Assets/Scenes/SampleScene.unity → FighterUI/FighterHUD/MissileDirectionIndicator_03 / Assets/Scenes/SampleScene.unity → FighterUI/FighterHUD/MissileDirectionIndicator_04

### MissileDirectionIndicatorView (134줄)

[MissileDirectionIndicatorView.cs](<../../Assets/Features/UI/HUD/Scripts/MissileDirectionIndicatorView.cs>)

- 메서드 — OnEnable, OnDisable, LateUpdate, UpdateIndicator, GetScreenDirection, HideAllIndicators, SetIndicatorVisible
- 설정 필드 — _threatDetector, _worldCamera, _indicatorRadius, _indicatorSize
- 연결 — Assets/Scenes/SampleScene.unity → FighterUI/FighterHUD

### MissileWarningView (370줄)

[MissileWarningView.cs](<../../Assets/Features/UI/HUD/Scripts/MissileWarningView.cs>)

- 메서드 — Awake, OnEnable, OnDisable, Update, HandleWarningStateChanged, HandleLockProgressChanged, GetWarningText, SetWarningVisible, SetBackgroundForState, SetTrianglesForState, SetLockProgressTriangles, SetIncomingTriangles, CaptureTriangleSizes, RestoreTriangleSize, SetTriangleColor, SetBorderColor
- 설정 필드 — _threatDetector, _warningGroup, _warningText, _warningBackground, _warningBorder, _leftWarningTriangles, _rightWarningTriangles, _triangleOutlineSprite, _triangleFillSprite, _lockingText, _lockedText, _incomingText, _lockingColor, _dangerColor, _normalBackgroundColor, _lockedBackgroundColor, _incomingBackgroundColor, _transparentBackgroundColor, _incomingTriangleRedColor, _incomingTriangleDarkColor, _backgroundTransitionDuration, _incomingTriangleScale
- 연결 — Assets/Scenes/SampleScene.unity → FighterUI/FighterHUD

### RefillProgressView (85줄)

[RefillProgressView.cs](<../../Assets/Features/UI/HUD/Scripts/RefillProgressView.cs>)

- 메서드 — Awake, Bind, LateUpdate, OnDisable, SetProgressVisible
- 설정 필드 — _progressType, _progressLine, _progressGraphic, _filledArea, _filledAreaGraphic
- 연결 — Assets/Scenes/SampleScene.unity → FighterUI/FighterHUD/WeaponSelectionUI/UnselectedSlot/Background/RefillProgressUI / Assets/Scenes/SampleScene.unity → FighterUI/FighterHUD/WeaponSelectionUI/SelectedSlot/Background/RefillProgressUI

### WeaponSelectionView (136줄)

[WeaponSelectionView.cs](<../../Assets/Features/UI/HUD/Scripts/WeaponSelectionView.cs>)

- 메서드 — OnEnable, OnDisable, LateUpdate, UpdateSelection, PresentWeapon, SetActive
- 설정 필드 — _root, _weaponIcon, _ammoText, _infiniteAmmoIcon, _numberBackground, _numberText, _refillProgress, _controller, _selectedSlot, _unselectedSlot, _gunIcon, _missileIcon
- 연결 — Assets/Scenes/SampleScene.unity → FighterUI/FighterHUD/WeaponSelectionUI

### PlayerCombatHud (44줄)

[PlayerCombatHud.cs](<../../Assets/Features/UI/PlayerCombatHud.cs>)

- 메서드 — SetTacticalActions, Awake, LateUpdate
- 설정 필드 — _combat, _label, _stance, _traversal, _mount, _health, _life, _revive, _rifleStations, _grenade, _mantle
- 연결 — Assets/Scenes/SampleScene.unity → VehicleInteractionUI

### ForwardUiFollower (125줄)

[ForwardUiFollower.cs](<../../Assets/Features/UI/Shared/ForwardUiFollower.cs>)

- 메서드 — Awake, OnEnable, OnDisable, UpdateReticlePosition, TryGetForwardScreenPosition
- 설정 필드 — _forwardSource, _worldCamera, _viewStateSource, _projectionDistance, _moveSpeed, _centerOnFreeLook
- 연결 — Assets/Scenes/SampleScene.unity → FighterUI/FighterHUD/MissileAimUI/LockOnAreaRing / Assets/Scenes/SampleScene.unity → FighterUI/FighterHUD/OverheatUI

### VehicleInteractionView (27줄)

[VehicleInteractionView.cs](<../../Assets/Features/UI/VehicleInteractionView.cs>)

- 메서드 — Awake, LateUpdate
- 설정 필드 — _interaction, _label
- 연결 — Assets/Scenes/SampleScene.unity → VehicleInteractionUI

## Assets/Features/VFX

### ExplosionEffect (33줄)

[ExplosionEffect.cs](<../../Assets/Features/VFX/Explosion/Scripts/ExplosionEffect.cs>)

- 메서드 — Play, OnSpawn, OnDespawn, ReturnEffect
- 설정 필드 — 인라인 SerializeField 없음; 파일의 다른 선언 확인
- 연결 — Assets/Features/VFX/Explosion/Prefabs/Missile ExplosionEffect.prefab → Missile ExplosionEffect

### ExplosionEffectPool (34줄)

[ExplosionEffectPool.cs](<../../Assets/Features/VFX/Explosion/Scripts/ExplosionEffectPool.cs>)

- 메서드 — Create, Play
- 설정 필드 — _effectPrefab, _effectScale
- 연결 — Assets/Scenes/SampleScene.unity → ObjectPools/MissileExplosionPool

### BulletHole (45줄)

[BulletHole.cs](<../../Assets/Features/VFX/Hit/Scripts/BulletHole.cs>)

- 메서드 — Awake, Play, OnSpawn, OnDespawn, ReturnHole
- 설정 필드 — _decal
- 연결 — Assets/Features/VFX/Hit/Prefabs/Concrete BulletHole.prefab → Concrete BulletHole / Assets/Features/VFX/Hit/Prefabs/Dirt BulletHole.prefab → Dirt BulletHole / Assets/Features/VFX/Hit/Prefabs/Metal BulletHole.prefab → Metal BulletHole / Assets/Features/VFX/Hit/Prefabs/Sand BulletHole.prefab → Sand BulletHole / Assets/Features/VFX/Hit/Prefabs/Wood BulletHole.prefab → Wood BulletHole

### BulletHolePool (51줄)

[BulletHolePool.cs](<../../Assets/Features/VFX/Hit/Scripts/BulletHolePool.cs>)

- 메서드 — Create, Get, Return
- 설정 필드 — _prefab, _maxSize
- 연결 — Assets/Scenes/SampleScene.unity → ObjectPools/BulletHolePools/Concrete BulletHolePool / Assets/Scenes/SampleScene.unity → ObjectPools/BulletHolePools/Dirt BulletHolePool / Assets/Scenes/SampleScene.unity → ObjectPools/BulletHolePools/Metal BulletHolePool / Assets/Scenes/SampleScene.unity → ObjectPools/BulletHolePools/Sand BulletHolePool / Assets/Scenes/SampleScene.unity → ObjectPools/BulletHolePools/Wood BulletHolePool

### HitEffect (34줄)

[HitEffect.cs](<../../Assets/Features/VFX/Hit/Scripts/HitEffect.cs>)

- 메서드 — Play, OnSpawn, OnDespawn, ReturnEffect
- 설정 필드 — 인라인 SerializeField 없음; 파일의 다른 선언 확인
- 연결 — Assets/Features/VFX/Hit/Prefabs/Concrete HitEffect.prefab → Concrete HitEffect / Assets/Features/VFX/Hit/Prefabs/Dirt HitEffect.prefab → Dirt HitEffect / Assets/Features/VFX/Hit/Prefabs/Metal HitEffect.prefab → Metal HitEffect / Assets/Features/VFX/Hit/Prefabs/Sand HitEffect.prefab → Sand HitEffect / Assets/Features/VFX/Hit/Prefabs/Wood HitEffect.prefab → Wood HitEffect

### HitEffectDatabase (31줄)

[HitEffectDatabase.cs](<../../Assets/Features/VFX/Hit/Scripts/HitEffectDatabase.cs>)

- 메서드 — GetEntry
- 설정 필드 — _entries
- 연결 — Assets/Scenes/SampleScene.unity → HitEffect/HitEffectDatabase

### HitEffectManager (61줄)

[HitEffectManager.cs](<../../Assets/Features/VFX/Hit/Scripts/HitEffectManager.cs>)

- 메서드 — Play
- 설정 필드 — _database, _destroyDelay, _effectScale, _bulletHoleDuration, _bulletHoleScale
- 연결 — Assets/Scenes/SampleScene.unity → HitEffect/HitEffectManager

### HitEffectPool (16줄)

[HitEffectPool.cs](<../../Assets/Features/VFX/Hit/Scripts/HitEffectPool.cs>)

- 메서드 — Create
- 설정 필드 — _effectPrefab
- 연결 — Assets/Scenes/SampleScene.unity → ObjectPools/HitEffectPools/Concrete HitEffectPool / Assets/Scenes/SampleScene.unity → ObjectPools/HitEffectPools/Dirt HitEffectPool / Assets/Scenes/SampleScene.unity → ObjectPools/HitEffectPools/Metal HitEffectPool / Assets/Scenes/SampleScene.unity → ObjectPools/HitEffectPools/Sand HitEffectPool / Assets/Scenes/SampleScene.unity → ObjectPools/HitEffectPools/Wood HitEffectPool

### MuzzleFlash (105줄)

[MuzzleFlash.cs](<../../Assets/Features/VFX/Muzzle/Scripts/MuzzleFlash.cs>)

- 메서드 — Awake, Start, Play, StopParticles, FlashLight, WarmUp
- 설정 필드 — _particleSystems, _muzzleLight, _lightDuration, _warmUpOnStart, _effectDuration
- 연결 — Assets/Features/Fighter/Prefabs/Fighter.prefab → Fighter/FirePoint/MuzzleFirePoint/MuzzleFlash

## Assets/Features/Vehicles

### IVehicleSeatInput (7줄)

[IVehicleSeatInput.cs](<../../Assets/Features/Vehicles/Scripts/IVehicleSeatInput.cs>)

- 메서드 — 메서드 없는 계약/데이터 또는 별도 구문 확인
- 설정 필드 — 인라인 SerializeField 없음; 파일의 다른 선언 확인
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### IVehicleSeatOccupant (19줄)

[IVehicleSeatOccupant.cs](<../../Assets/Features/Vehicles/Scripts/IVehicleSeatOccupant.cs>)

- 메서드 — 메서드 없는 계약/데이터 또는 별도 구문 확인
- 설정 필드 — 인라인 SerializeField 없음; 파일의 다른 선언 확인
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### VehicleSeat (339줄)

[VehicleSeat.cs](<../../Assets/Features/Vehicles/Scripts/VehicleSeat.cs>)

- 메서드 — Awake, OnEnable, Update, CanEnter, TryEnter, TryExit, RefreshDriverControl, TryFindExitPose, TryPlaceCapsule, GetCapsule, HandleVehicleDeath, OnDisable, ReleaseOccupant, FindEmergencyExitPose, HasExitPoint, IsVehicleCollider, IsOccupantCollider, HasRoot, UprightRotation
- 설정 필드 — _seatPoint, _entryPoint, _exitPoints, _vehicleBody, _health, _driverInputBehaviours, _interactionDistance, _useVehicleSurfaceForEntry
- 연결 — Assets/Scenes/SampleScene.unity → [Prefab instance] / Assets/Features/Fighter/Prefabs/Fighter.prefab → Fighter

## Assets/Features/Weapon

### AirToAirMissileLauncher (211줄)

[AirToAirMissileLauncher.cs](<../../Assets/Features/Weapon/Scrips/AirToAirMissileLauncher.cs>)

- 메서드 — Awake, OnEnable, OnDisable, Fire, EvaluateAmmoState, TryStartReload, ReloadAfterDelay, TryStartReserveRefill, RefillReserveAfterDelay, TryGetRefillProgress, TrackProgress, HandleSelectionChanged, UpdateTargetSearching
- 설정 필드 — _missilePool, _targetLock, _firePoint, _maximumAmmo, _reserveAmmo, _reloadDuration, _reserveRefillDuration
- 연결 — Assets/Features/Fighter/Prefabs/Fighter.prefab → Fighter

### GrenadeDefinition (41줄)

[GrenadeDefinition.cs](<../../Assets/Features/Weapon/Scrips/GrenadeDefinition.cs>)

- 메서드 — 메서드 없는 계약/데이터 또는 별도 구문 확인
- 설정 필드 — _projectile, _useAnimations, _throwAnimation, _leftThrowAnimation, _equipAnimation, _idleAnimation, _fuse, _damage, _radius, _launchSpeed, _upwardSpeed, _underhandLaunchSpeed, _underhandUpwardSpeed, _releaseFraction, _firstPersonOffset, _handlingFirstPersonOffset
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### InfantryWeaponDefinition (43줄)

[InfantryWeaponDefinition.cs](<../../Assets/Features/Weapon/Scrips/InfantryWeaponDefinition.cs>)

- 메서드 — 메서드 없는 계약/데이터 또는 별도 구문 확인
- 설정 필드 — _displayName, _fireRate, _damage, _capacity, _reserve, _reloadDuration, _modelPrefab, _animations, _customGrip, _gripPosition, _gripEuler, _firstPersonEuler, _firstPersonOffset, _firstPersonAdsOffset, _firstPersonScale, _recoilDegrees, _recoilDistance
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### IRefillProgressSource (16줄)

[IRefillProgressSource.cs](<../../Assets/Features/Weapon/Scrips/IRefillProgressSource.cs>)

- 메서드 — 메서드 없는 계약/데이터 또는 별도 구문 확인
- 설정 필드 — 인라인 SerializeField 없음; 파일의 다른 선언 확인
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### IWeaponInput (8줄)

[IWeaponInput.cs](<../../Assets/Features/Weapon/Scrips/IWeaponInput.cs>)

- 메서드 — 메서드 없는 계약/데이터 또는 별도 구문 확인
- 설정 필드 — 인라인 SerializeField 없음; 파일의 다른 선언 확인
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### IWeaponMagazine (19줄)

[IWeaponMagazine.cs](<../../Assets/Features/Weapon/Scrips/IWeaponMagazine.cs>)

- 메서드 — 메서드 없는 계약/데이터 또는 별도 구문 확인
- 설정 필드 — 인라인 SerializeField 없음; 파일의 다른 선언 확인
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### MachineGun (57줄)

[MachineGun.cs](<../../Assets/Features/Weapon/Scrips/MachineGun.cs>)

- 메서드 — Fire
- 설정 필드 — _bulletPool, _firePoint, _projectileMesh, _tracerLengthMultiplier, _muzzleFlash
- 연결 — Assets/Scenes/SampleScene.unity → [Prefab instance] / Assets/Features/Fighter/Prefabs/Fighter.prefab → Fighter

### MachineGunAimPredictor (186줄)

[MachineGunAimPredictor.cs](<../../Assets/Features/Weapon/Scrips/MachineGunAimPredictor.cs>)

- 메서드 — RecordTrajectorySample, ResetTrajectoryHistory, TryGetPredictedPoint, InitializeTrajectoryHistory, GetSampleAt, GetChronologicalSample
- 설정 필드 — 인라인 SerializeField 없음; 파일의 다른 선언 확인
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### MachineGunAudio (111줄)

[MachineGunAudio.cs](<../../Assets/Features/Weapon/Scrips/MachineGunAudio.cs>)

- 메서드 — Awake, OnEnable, LateUpdate, OnDisable, HandleFired, StopPlayback
- 설정 필드 — _gun, _health, _source, _profile
- 연결 — Assets/Features/Fighter/Prefabs/Fighter.prefab → Fighter

### MeleeAttack (110줄)

[MeleeAttack.cs](<../../Assets/Features/Weapon/Scrips/MeleeAttack.cs>)

- 메서드 — TryAttack, Update, Resolve, IsOwner, Cancel, OnDisable
- 설정 필드 — _damage, _range, _radius, _windup, _recovery, _hitMask
- 연결 — Assets/Scenes/SampleScene.unity → PlayerCharacter/PersonalCombat/PistolMelee / Assets/Scenes/SampleScene.unity → PlayerCharacter/PersonalCombat/Fists

### MissileLaunchAudio (91줄)

[MissileLaunchAudio.cs](<../../Assets/Features/Weapon/Scrips/MissileLaunchAudio.cs>)

- 메서드 — Awake, OnEnable, LateUpdate, OnDisable, HandleFired
- 설정 필드 — _launcher, _health, _source, _profile
- 연결 — Assets/Features/Fighter/Prefabs/Fighter.prefab → Fighter

### OverHeat (50줄)

[OverHeat.cs](<../../Assets/Features/Weapon/Scrips/OverHeat.cs>)

- 메서드 — CanFire, AddHeat, CoolDown
- 설정 필드 — _maxHeat, _heatPerShot, _coolDownPerSecond, _recoverHeat
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### WeaponBase (57줄)

[WeaponBase.cs](<../../Assets/Features/Weapon/Scrips/WeaponBase.cs>)

- 메서드 — Update, TryFire, Fire
- 설정 필드 — _displayName, _fireRate, _damage, _overheat
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### WeaponController (48줄)

[WeaponController.cs](<../../Assets/Features/Weapon/Scrips/WeaponController.cs>)

- 메서드 — GetWeapon, Awake, Start, Update, SelectSlot
- 설정 필드 — _weapons
- 연결 — Assets/Scenes/SampleScene.unity → [Prefab instance] / Assets/Features/Fighter/Prefabs/Fighter.prefab → Fighter

### WeaponMagazine (38줄)

[WeaponMagazine.cs](<../../Assets/Features/Weapon/Scrips/WeaponMagazine.cs>)

- 메서드 — Awake, Update, ResetAmmo, TryConsumeRound, TryReload, CancelReload, OnDisable
- 설정 필드 — _capacity, _startingReserve, _reloadDuration
- 연결 — Assets/Scenes/SampleScene.unity → PlayerCharacter/PersonalCombat

### WeaponMagazineState (56줄)

[WeaponMagazineState.cs](<../../Assets/Features/Weapon/Scrips/WeaponMagazineState.cs>)

- 메서드 — Configure, ResetAmmo, Tick, TryConsumeRound, TryReload, CancelReload
- 설정 필드 — 인라인 SerializeField 없음; 파일의 다른 선언 확인
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### WeaponModel (20줄)

[WeaponModel.cs](<../../Assets/Features/Weapon/Scrips/WeaponModel.cs>)

- 메서드 — 메서드 없는 계약/데이터 또는 별도 구문 확인
- 설정 필드 — _muzzle, _supportHandGrip, _magazine, _reloadGrip, _reloadTravel, _reloadTilt
- 연결 — Assets/Features/Weapon/Prefabs/AK47.prefab → AK47 / Assets/Features/Weapon/Prefabs/M4A1.prefab → M4A1 / Assets/Features/Weapon/Prefabs/PrototypePistol.prefab → PrototypePistol / Assets/Features/Weapon/Prefabs/PrototypeRifle.prefab → PrototypeRifle

## Assets/Framework

### DamageInfo (11줄)

[DamageInfo.cs](<../../Assets/Framework/Core/DamageInfo.cs>)

- 메서드 — 메서드 없는 계약/데이터 또는 별도 구문 확인
- 설정 필드 — 인라인 SerializeField 없음; 파일의 다른 선언 확인
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### Health (63줄)

[Health.cs](<../../Assets/Framework/Core/Health.cs>)

- 메서드 — Awake, TakeDamage, Heal, Die, BindDamagePolicy, UnbindDamagePolicy, TryRevive
- 설정 필드 — _maxHealth
- 연결 — Assets/Scenes/SampleScene.unity → Dummys/Dummy_Enemy / Assets/Scenes/SampleScene.unity → PlayerCharacter / Assets/Scenes/SampleScene.unity → [Prefab instance] / Assets/Scenes/SampleScene.unity → MissileTargetDummy_Enemy / Assets/Scenes/SampleScene.unity → Dummys/Dummy_Neutral / Assets/Features/TrainingGround/InfantryTrainingGround.prefab → InfantryTrainingGround/Enemy_50m / Assets/Features/TrainingGround/InfantryTrainingGround.prefab → InfantryTrainingGround/Ally_50m / Assets/Features/TrainingGround/InfantryTrainingGround.prefab → InfantryTrainingGround/Ally_10m / Assets/Features/TrainingGround/InfantryTrainingGround.prefab → InfantryTrainingGround/Enemy_25m / Assets/Features/TrainingGround/InfantryTrainingGround.prefab → InfantryTrainingGround/Neutral_10m / Assets/Features/TrainingGround/InfantryTrainingGround.prefab → InfantryTrainingGround/Neutral_50m / Assets/Features/TrainingGround/InfantryTrainingGround.prefab → InfantryTrainingGround/Neutral_25m / Assets/Features/TrainingGround/InfantryTrainingGround.prefab → InfantryTrainingGround/Ally_25m / Assets/Features/TrainingGround/InfantryTrainingGround.prefab → InfantryTrainingGround/Enemy_10m / Assets/Features/Fighter/Prefabs/Fighter.prefab → Fighter

### IDamageable (7줄)

[IDamageable.cs](<../../Assets/Framework/Core/IDamageable.cs>)

- 메서드 — 메서드 없는 계약/데이터 또는 별도 구문 확인
- 설정 필드 — 인라인 SerializeField 없음; 파일의 다른 선언 확인
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### IHealable (7줄)

[IHealable.cs](<../../Assets/Framework/Core/IHealable.cs>)

- 메서드 — 메서드 없는 계약/데이터 또는 별도 구문 확인
- 설정 필드 — 인라인 SerializeField 없음; 파일의 다른 선언 확인
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### IHealthDamagePolicy (8줄)

[IHealthDamagePolicy.cs](<../../Assets/Framework/Core/IHealthDamagePolicy.cs>)

- 메서드 — 메서드 없는 계약/데이터 또는 별도 구문 확인
- 설정 필드 — 인라인 SerializeField 없음; 파일의 다른 선언 확인
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### ITargetable (9줄)

[ITargetable.cs](<../../Assets/Framework/Core/ITargetable.cs>)

- 메서드 — 메서드 없는 계약/데이터 또는 별도 구문 확인
- 설정 필드 — 인라인 SerializeField 없음; 파일의 다른 선언 확인
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### IThirdPersonViewState (8줄)

[IThirdPersonViewState.cs](<../../Assets/Framework/Core/IThirdPersonViewState.cs>)

- 메서드 — 메서드 없는 계약/데이터 또는 별도 구문 확인
- 설정 필드 — 인라인 SerializeField 없음; 파일의 다른 선언 확인
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### Team (10줄)

[Team.cs](<../../Assets/Framework/Core/Team.cs>)

- 메서드 — 메서드 없는 계약/데이터 또는 별도 구문 확인
- 설정 필드 — _team
- 연결 — Assets/Scenes/SampleScene.unity → Dummys/Dummy_Enemy / Assets/Scenes/SampleScene.unity → PlayerCharacter / Assets/Scenes/SampleScene.unity → MissileTargetDummy_Enemy / Assets/Scenes/SampleScene.unity → EnemyMissileShooterDummy / Assets/Scenes/SampleScene.unity → Dummys/Dummy_Neutral / Assets/Features/TrainingGround/InfantryTrainingGround.prefab → InfantryTrainingGround/Enemy_50m / Assets/Features/TrainingGround/InfantryTrainingGround.prefab → InfantryTrainingGround/Ally_50m / Assets/Features/TrainingGround/InfantryTrainingGround.prefab → InfantryTrainingGround/Ally_10m / Assets/Features/TrainingGround/InfantryTrainingGround.prefab → InfantryTrainingGround/Enemy_25m / Assets/Features/TrainingGround/InfantryTrainingGround.prefab → InfantryTrainingGround/Neutral_10m / Assets/Features/TrainingGround/InfantryTrainingGround.prefab → InfantryTrainingGround/Neutral_50m / Assets/Features/TrainingGround/InfantryTrainingGround.prefab → InfantryTrainingGround/Neutral_25m / Assets/Features/TrainingGround/InfantryTrainingGround.prefab → InfantryTrainingGround/Ally_25m / Assets/Features/TrainingGround/InfantryTrainingGround.prefab → InfantryTrainingGround/Enemy_10m / Assets/Features/Fighter/Prefabs/Fighter.prefab → Fighter

### TeamType (9줄)

[TeamType.cs](<../../Assets/Framework/Core/TeamType.cs>)

- 메서드 — 메서드 없는 계약/데이터 또는 별도 구문 확인
- 설정 필드 — 인라인 SerializeField 없음; 파일의 다른 선언 확인
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### ParticlePlayer (48줄)

[ParticlePlayer.cs](<../../Assets/Framework/Particle/ParticlePlayer.cs>)

- 메서드 — Awake, PlayParticles, StopParticles, GetMaxDuration
- 설정 필드 — 인라인 SerializeField 없음; 파일의 다른 선언 확인
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### CollisionSurfaceType (8줄)

[CollisionSurfaceType.cs](<../../Assets/Framework/Physics/CollisionSurfaceType.cs>)

- 메서드 — 메서드 없는 계약/데이터 또는 별도 구문 확인
- 설정 필드 — 인라인 SerializeField 없음; 파일의 다른 선언 확인
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### IPool (7줄)

[IPool.cs](<../../Assets/Framework/Pool/IPool.cs>)

- 메서드 — 메서드 없는 계약/데이터 또는 별도 구문 확인
- 설정 필드 — 인라인 SerializeField 없음; 파일의 다른 선언 확인
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### IPoolable (9줄)

[IPoolable.cs](<../../Assets/Framework/Pool/IPoolable.cs>)

- 메서드 — 메서드 없는 계약/데이터 또는 별도 구문 확인
- 설정 필드 — 인라인 SerializeField 없음; 파일의 다른 선언 확인
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### ObjectPool (66줄)

[ObjectPool.cs](<../../Assets/Framework/Pool/ObjectPool.cs>)

- 메서드 — Awake, Create, Register, Get, Return
- 설정 필드 — _initialSize
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### PoolableBehaviour (26줄)

[PoolableBehaviour.cs](<../../Assets/Framework/Pool/PoolableBehaviour.cs>)

- 메서드 — SetPool, OnSpawn, OnDespawn, ReturnToPool
- 설정 필드 — 인라인 SerializeField 없음; 파일의 다른 선언 확인
- 연결 — 직접 직렬화 연결 없음. 기능 목록과 생성/초기화 코드 확인

### CursorLock (13줄)

[CursorLock.cs](<../../Assets/Framework/Unility/CursorLock.cs>)

- 메서드 — 메서드 없는 계약/데이터 또는 별도 구문 확인
- 설정 필드 — 인라인 SerializeField 없음; 파일의 다른 선언 확인
- 연결 — Assets/Scenes/SampleScene.unity → CursorLock
