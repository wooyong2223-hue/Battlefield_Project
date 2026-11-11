# 스크립트 찾기

2026-11-11 구조 개선 반영.

| 수정할 기능 | 먼저 읽을 파일 | 함께 읽을 코드 |
|---|---|---|
| 보병 입력 | Input/PlayerCharacterInput.cs | Input/Contracts |
| 이동·자세 | Locomotion/PlayerCharacterMovement.cs | Stance, Traversal, Mantle |
| 공격·장전 요청 | Combat/PlayerCharacterCombatController.cs | Equipment, Weapon/Scripts/Ammunition |
| 사격 방향·총구 엄폐 검사 | Combat/PlayerCharacterShotPath.cs | CombatController에서 호출 |
| 장비 선택 | Equipment/PlayerCharacterLoadout.cs | PlayerCharacterWeaponEquipment |
| 시선·ADS | Camera/PlayerCharacterLookController.cs | PlayerCharacterCamera |
| 전투 애니메이션·이벤트 연결 | Presentation/Combat/PlayerCharacterCombatView.cs | RifleReloadView, WeaponPoseState |
| 무기 모델 생성·교체·손 부착 | Presentation/Combat/PlayerCharacterWeaponModelPresentation.cs | CombatView에서 장비 전환, 표시, 총구 연결 호출 |
| 조준·양손 IK | Presentation/Combat/PlayerCharacterWeaponAimView.cs | WeaponGripSolver에서 조준 위치·회전과 양팔 도달 범위 계산 |
| 장전 클립·손 위치 | Presentation/Combat/PlayerCharacterRifleReloadView.cs | CombatView, ReloadGripReference, 무기 정의의 Reload Clip |
| 이동 표현 | Presentation/Locomotion/PlayerCharacterAnimationView.cs | Prone/Slide/TacticalAnimationView |
| 1인칭 팔·움직임 표현 | Presentation/FirstPerson/FirstPersonArmsView.cs | MotionState/Profile, JumpPoseState |
| 탑승 | Interaction/PlayerVehicleInteraction.cs | PlayerVehicleOccupant, Vehicles/VehicleSeat |
| 생명·소생 | Life/InfantryLifeState.cs | PlayerCharacterRevive, Framework/Core/Health |
| Play·빌드 씬 구성 | ../Editor/Composition/PlayerCharacterSceneComposition.cs | SlideAnimationSceneSetup, ProneAnimationSceneSetup, ArmedLocomotionSceneSetup, TacticalActionSceneSetup |

표의 상대 경로 기준은 Assets/Features/PlayerCharacter/Scripts다.

- Composition은 Play 및 빌드 시 연결하는 코드다. 원본 씬에 없는 컴포넌트를 추가할 수 있다.
- Importers는 에셋 임포트, Authoring은 명시적 제작 작업, Diagnostics는 진단 도구다. ProneAnimationSceneSetup에는 기존 임포트 메뉴도 유지된다.
- Training/Scripts에는 더미와 무기 교체대가 있다. 훈련 스크립트의 네임스페이스는 모두 `Battlefield.Features.Training`이다.
- UI/Player, UI/Fighter, UI/Shared로 소비 기능을 구분한다.
- 무기 모델 인스턴스는 WeaponModelPresentation이 소유한다. CombatView가 장비 상태를 전달하고, 모델의 총구를 CombatController에 연결한다. WeaponGripSolver는 입력값으로 목표 손 자세를 계산하고, WeaponAimView가 기존 Animator 콜백에서 결과를 적용한다.
