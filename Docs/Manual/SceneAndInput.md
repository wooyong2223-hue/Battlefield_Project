# Unity에서 찾는 위치와 조작

[목차](<README.md>) · [전체 연결 색인](<CodeIndex.md>)

## 시작 위치

- [SampleScene](<../../Assets/Scenes/SampleScene.unity>)을 연다.
- Hierarchy에서 `PlayerCharacter`를 검색하면 입력·이동·자세·전투·탑승 컴포넌트를 찾는다.
- `Main Camera`에는 보병 PlayerCharacterCamera와 전투기 JetCamera가 있다. 실제 제어권은 탑승 상태에 따라 전환된다.
- `VehicleInteractionUI`에는 PlayerCombatHud, 자식 `CombatStatus`에는 TMP 텍스트가 연결된다.
- 전투기 기준 구성은 [Fighter.prefab](<../../Assets/Features/Fighter/Prefabs/Fighter.prefab>)에서 확인하고 SampleScene 인스턴스의 Override를 함께 본다.
- `HitEffect/HitEffectManager`는 재질별 피격 이펙트·탄흔 연결의 시작점이다.

## 입력 바인딩

아래 표는 저장된 Input Actions의 키보드·마우스 바인딩이다. 상태에 따라 기능이 제한되거나 다른 행동으로 해석될 수 있다. Fighter의 게임패드 바인딩은 에셋에서 별도 확인한다.

### Fighter

| 액션 | 방향/파트 | 바인딩 |
|---|---|---|
| Move | up | `<Keyboard>/w` |
| Move | down | `<Keyboard>/s` |
| Move | left | `<Keyboard>/a` |
| Move | right | `<Keyboard>/d` |
| Attack | — | `<Mouse>/leftButton` |
| ChangeCamera | — | `<Keyboard>/c` |
| RearView | — | `<Keyboard>/v` |
| Afterburner | — | `<Keyboard>/leftShift` |
| Pitch | negative | `<Keyboard>/q` |
| Pitch | positive | `<Keyboard>/e` |
| FreeLook | — | `<Keyboard>/leftCtrl` |
| Zoom | — | `<Mouse>/rightButton` |
| CameraDistance | — | `<Mouse>/scroll/y` |

### PlayerCharacter

| 액션 | 방향/파트 | 바인딩 |
|---|---|---|
| Move | up | `<Keyboard>/w` |
| Move | down | `<Keyboard>/s` |
| Move | left | `<Keyboard>/a` |
| Move | right | `<Keyboard>/d` |
| Attack | — | `<Mouse>/leftButton` |
| Aim | — | `<Mouse>/rightButton` |
| Sprint | — | `<Keyboard>/leftShift` |
| Jump | — | `<Keyboard>/space` |
| TogglePerspective | — | `<Keyboard>/v` |
| FreeLook | — | `<Keyboard>/leftAlt` |
| ReleaseCursor | — | `<Keyboard>/escape` |
| Interact | — | `<Keyboard>/f` |
| Crouch | — | `<Keyboard>/leftCtrl` |
| Slide | — | `<Keyboard>/c` |
| Reload | — | `<Keyboard>/r` |
| Melee | — | `<Keyboard>/g` |
| SelectPistol | — | `<Keyboard>/2` |
| SelectUnarmed | — | `<Keyboard>/3` |
| Prone | — | `<Keyboard>/z` |
| Mount | — | `<Keyboard>/x` |
| Lean | negative | `<Keyboard>/q` |
| Lean | positive | `<Keyboard>/e` |
| GiveUp | — | `<Keyboard>/space` |
| Respawn | — | `<Keyboard>/enter` |
| SelectRifle | — | `<Keyboard>/1` |
| ThrowGrenade | — | `<Keyboard>/4` |
| ToggleFireMode | — | `<Keyboard>/b` |

보병 우클릭은 View에서 견착·ADS를 구분하고, 수류탄 장착 중에는 위/아래 투척 선택으로 소비된다. Space는 살아 있을 때 점프/장애물 넘기, 다운 상태에서는 포기에 사용된다. 보병 B는 발사 모드, X는 거치다. 전투기 CameraDistance 입력은 바인딩만 있고 현재 Orbit의 거리 갱신 호출은 꺼져 있다.

## 설정은 어디서 바꾸나

전투기의 `1`·`2` 무기 선택은 KeyboardJetInput.Awake에서 런타임 액션으로 추가된다. 저장된 Input Actions 표에 없더라도 실제 실행 시 사용된다. 캐릭터 Animator와 CombatView/WeaponAimView는 `PlayerCharacter > Armature`에서 확인한다.

| 바꾸는 대상 | 찾는 곳 | 주의할 연결 |
|---|---|---|
| 보병 속도·점프·자세 | PlayerCharacter의 Movement/ Stance Inspector | 코드 기본값보다 씬 직렬화 값 우선 |
| 무기 탄약·피해·모델 | Equipment가 참조하는 InfantryWeaponDefinition 에셋 | FireRate가 0이면 Controller의 피해량 설정도 사용 |
| 기체 속도·회전·버너 | Fighter 프리팹의 Movement/Rotation/Afterburner | 씬 Override 함께 확인 |
| 3인칭 기체 구도 | ThirdPersonCameraPoint와 JetCamera의 Orbit | authored offset과 거리 clamp 연결 |
| 소리·볼륨·반복 설정 | Fighter의 AudioOverview Inspector | Profile과 각 AudioSource 소유 오브젝트 확인 |
| 피격 이펙트·탄흔 | HitEffectDatabase와 대상 Collider Physics Material | Material이 비면 효과 생략 |
| 클립·혼합·자세 보정 | Animator/OverrideController/Mask 및 AnimationSet | Play 시 Editor 설정기가 추가 연결할 수 있음 |

## 주요 설정/프로필 에셋 위치

- [FighterEngineAudioProfile.asset](<../../Assets/Features/Audio/Profiles/FighterEngineAudioProfile.asset>)
- [MachineGunAudioProfile.asset](<../../Assets/Features/Audio/Profiles/MachineGunAudioProfile.asset>)
- [MissileFlightAudioProfile.asset](<../../Assets/Features/Audio/Profiles/MissileFlightAudioProfile.asset>)
- [MissileLaunchAudioProfile.asset](<../../Assets/Features/Audio/Profiles/MissileLaunchAudioProfile.asset>)
- [PlayerSlideContact.asset](<../../Assets/Features/PlayerCharacter/Animation/PlayerSlideContact.asset>)
- [FirstPersonArms.asset](<../../Assets/Features/PlayerCharacter/Models/FirstPersonArms.asset>)
- [FirstPersonMeleeArms.asset](<../../Assets/Features/PlayerCharacter/Models/FirstPersonMeleeArms.asset>)
- [PlayerCharacterMotion.asset](<../../Assets/Features/PlayerCharacter/Settings/PlayerCharacterMotion.asset>)
- [ProneCamera.asset](<../../Assets/Features/PlayerCharacter/Settings/ProneCamera.asset>)
- [CannonBullet.asset](<../../Assets/Features/Projectile/Models/Temporary/CannonBullet.asset>)
- [InfantryBullet.asset](<../../Assets/Features/Projectile/Models/Temporary/InfantryBullet.asset>)
- [AK47.asset](<../../Assets/Features/Weapon/Definitions/AK47.asset>)
- [AssaultRifle.asset](<../../Assets/Features/Weapon/Definitions/AssaultRifle.asset>)
- [M4A1.asset](<../../Assets/Features/Weapon/Definitions/M4A1.asset>)
- [Pistol.asset](<../../Assets/Features/Weapon/Definitions/Pistol.asset>)

## 로컬 모션 프로필

`Assets/ThirdParty/MixamoLocal`과 `LocalAssets/Mixamo`는 .gitignore 대상이다. 로컬 실행과 새 체크아웃에서 필요한 에셋 확보 절차가 달라진다. 실제 라이선스/배포 조건은 각 원본의 출처 기록을 확인한다.

- ArmedLocomotion.asset (`Assets/ThirdParty/MixamoLocal/ArmedLocomotion/ArmedLocomotion.asset`)
- ProneAnimations.asset (`Assets/ThirdParty/MixamoLocal/Prone/ProneAnimations.asset`)
- RunningSlide.asset (`Assets/ThirdParty/MixamoLocal/RunningSlide.asset`)
- FragGrenade.asset (`Assets/ThirdParty/MixamoLocal/TacticalActions/FragGrenade.asset`)
- TacticalActions.asset (`Assets/ThirdParty/MixamoLocal/TacticalActions/TacticalActions.asset`)

## 도구 메뉴

- `Tools/Battlefield/Animations/Create Missing Grenade Handling Clips` — [GrenadeHandlingAnimationAuthoring.cs](<../../Assets/Features/PlayerCharacter/Editor/GrenadeHandlingAnimationAuthoring.cs>)
- `Tools/Battlefield/Animations/Mixamo/Apply Throw With Throw Object Left Arm` — [GrenadeThrowCompositionSetup.cs](<../../Assets/Features/PlayerCharacter/Editor/GrenadeThrowCompositionSetup.cs>)
- `Tools/Battlefield/Animations/Mixamo/Import Prone Animations` — [LocalProneAnimationSetup.cs](<../../Assets/Features/PlayerCharacter/Editor/LocalProneAnimationSetup.cs>)
- `Tools/Battlefield/Animations/Mixamo/Import Armed Locomotion` — [MixamoArmedLocomotionImporter.cs](<../../Assets/Features/PlayerCharacter/Editor/MixamoArmedLocomotionImporter.cs>)
- `Tools/Battlefield/Animations/Mixamo/Import Running Slide` — [MixamoSlideImporter.cs](<../../Assets/Features/PlayerCharacter/Editor/MixamoSlideImporter.cs>)
- `Tools/Battlefield/Animations/Mixamo/Restore Original Running Slide` — [MixamoSlideImporter.cs](<../../Assets/Features/PlayerCharacter/Editor/MixamoSlideImporter.cs>)
- `Tools/Battlefield/Animations/Mixamo/Use Running Slide Lower Body` — [MixamoSlideImporter.cs](<../../Assets/Features/PlayerCharacter/Editor/MixamoSlideImporter.cs>)
- `Tools/Battlefield/Animations/Mixamo/Use Forward Slide Entry` — [MixamoSlideImporter.cs](<../../Assets/Features/PlayerCharacter/Editor/MixamoSlideImporter.cs>)
- `Tools/Battlefield/Diagnostics/Record Upper Body Aim (2 minutes)` — [PlayerCharacterAimDiagnostics.cs](<../../Assets/Features/PlayerCharacter/Editor/PlayerCharacterAimDiagnostics.cs>)
- `Tools/Battlefield/Diagnostics/Stop Upper Body Aim Recording` — [PlayerCharacterAimDiagnostics.cs](<../../Assets/Features/PlayerCharacter/Editor/PlayerCharacterAimDiagnostics.cs>)
- `Tools/Battlefield/Animations/Mixamo/Import Grenade And Mantle` — [TacticalActionImporter.cs](<../../Assets/Features/PlayerCharacter/Editor/TacticalActionImporter.cs>)

InfantryTestTools에는 Play 모드의 Down Player, Spawn Downed Ally 메뉴가 있다. 테스트 지형/재질 판은 SampleScene에서 확인한다. 이번 문서 생성 중 진단 메뉴는 실행하지 않았다.
