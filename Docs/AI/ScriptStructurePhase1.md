# 스크립트 구조 정리 1단계

기준: 44b692c. 시작 시 git working tree clean. 구조 점검 이후 커밋된 최신 파일을 보존한다.

## 변경 범위

파일 배치·타입 이름·사용처·에디터 클래스 식별자만 변경했다. 게임 규칙, 필드 이름, 수치, 계층 구조, asmdef, 실행 순서는 유지했다. 기존 타입 이름은 아래 이름으로 교체했으며 메서드·필드를 삭제하지 않았다.

## 타입 이름 변경

| 이전 | 이후 |
|---|---|
| `PlayerCharacterController` | `PlayerCharacterCombatController` |
| `PlayerCharacterView` | `PlayerCharacterLookController` |
| `PlayerCharacterWeaponMount` | `PlayerCharacterWeaponBracing` |
| `FighterEngine` | `FighterEngineActivation` |
| `LocalArmedLocomotionSetup` | `ArmedLocomotionSceneSetup` |
| `LocalSlideAnimationSetup` | `SlideAnimationSceneSetup` |
| `LocalProneAnimationSetup` | `ProneAnimationSceneSetup` |
| `LocalTacticalActionSetup` | `TacticalActionSceneSetup` |
| `LocalArmedLocomotionBuildProcessor` | `ArmedLocomotionBuildProcessor` |
| `LocalSlideAnimationBuildProcessor` | `SlideAnimationBuildProcessor` |
| `LocalProneAnimationBuildProcessor` | `ProneAnimationBuildProcessor` |
| `LocalTacticalActionBuildProcessor` | `TacticalActionBuildProcessor` |

`Battlefield.Framework.Unility` namespace는 `Battlefield.Framework.Utility`로 교체했다. OverHeat.cs 파일명만 Overheat.cs로 수정했다.

## 이동 파일 전체 목록

각 .cs는 같은 이름의 .meta를 함께 이동했고 내용과 GUID를 보존했다. 두 오타 폴더의 .meta도 기존 GUID를 유지해 이동했다.

| 이전 경로 | 이후 경로 |
|---|---|
| `Assets/Features/Weapon/Scrips.meta` | `Assets/Features/Weapon/Scripts.meta` |
| `Assets/Framework/Unility.meta` | `Assets/Framework/Utility.meta` |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterInput.cs` | `Assets/Features/PlayerCharacter/Scripts/Input/PlayerCharacterInput.cs` |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterInput.cs.meta` | `Assets/Features/PlayerCharacter/Scripts/Input/PlayerCharacterInput.cs.meta` |
| `Assets/Features/PlayerCharacter/Scripts/IPlayerCharacterAimSource.cs` | `Assets/Features/PlayerCharacter/Scripts/Input/Contracts/IPlayerCharacterAimSource.cs` |
| `Assets/Features/PlayerCharacter/Scripts/IPlayerCharacterAimSource.cs.meta` | `Assets/Features/PlayerCharacter/Scripts/Input/Contracts/IPlayerCharacterAimSource.cs.meta` |
| `Assets/Features/PlayerCharacter/Scripts/IPlayerCharacterCombatInput.cs` | `Assets/Features/PlayerCharacter/Scripts/Input/Contracts/IPlayerCharacterCombatInput.cs` |
| `Assets/Features/PlayerCharacter/Scripts/IPlayerCharacterCombatInput.cs.meta` | `Assets/Features/PlayerCharacter/Scripts/Input/Contracts/IPlayerCharacterCombatInput.cs.meta` |
| `Assets/Features/PlayerCharacter/Scripts/IPlayerCharacterInput.cs` | `Assets/Features/PlayerCharacter/Scripts/Input/Contracts/IPlayerCharacterInput.cs` |
| `Assets/Features/PlayerCharacter/Scripts/IPlayerCharacterInput.cs.meta` | `Assets/Features/PlayerCharacter/Scripts/Input/Contracts/IPlayerCharacterInput.cs.meta` |
| `Assets/Features/PlayerCharacter/Scripts/IPlayerCharacterInteractionInput.cs` | `Assets/Features/PlayerCharacter/Scripts/Input/Contracts/IPlayerCharacterInteractionInput.cs` |
| `Assets/Features/PlayerCharacter/Scripts/IPlayerCharacterInteractionInput.cs.meta` | `Assets/Features/PlayerCharacter/Scripts/Input/Contracts/IPlayerCharacterInteractionInput.cs.meta` |
| `Assets/Features/PlayerCharacter/Scripts/IPlayerCharacterLifeInput.cs` | `Assets/Features/PlayerCharacter/Scripts/Input/Contracts/IPlayerCharacterLifeInput.cs` |
| `Assets/Features/PlayerCharacter/Scripts/IPlayerCharacterLifeInput.cs.meta` | `Assets/Features/PlayerCharacter/Scripts/Input/Contracts/IPlayerCharacterLifeInput.cs.meta` |
| `Assets/Features/PlayerCharacter/Scripts/IPlayerCharacterStanceInput.cs` | `Assets/Features/PlayerCharacter/Scripts/Input/Contracts/IPlayerCharacterStanceInput.cs` |
| `Assets/Features/PlayerCharacter/Scripts/IPlayerCharacterStanceInput.cs.meta` | `Assets/Features/PlayerCharacter/Scripts/Input/Contracts/IPlayerCharacterStanceInput.cs.meta` |
| `Assets/Features/PlayerCharacter/Scripts/IPlayerCharacterTacticalInput.cs` | `Assets/Features/PlayerCharacter/Scripts/Input/Contracts/IPlayerCharacterTacticalInput.cs` |
| `Assets/Features/PlayerCharacter/Scripts/IPlayerCharacterTacticalInput.cs.meta` | `Assets/Features/PlayerCharacter/Scripts/Input/Contracts/IPlayerCharacterTacticalInput.cs.meta` |
| `Assets/Features/PlayerCharacter/Scripts/IPlayerCharacterThrowInput.cs` | `Assets/Features/PlayerCharacter/Scripts/Input/Contracts/IPlayerCharacterThrowInput.cs` |
| `Assets/Features/PlayerCharacter/Scripts/IPlayerCharacterThrowInput.cs.meta` | `Assets/Features/PlayerCharacter/Scripts/Input/Contracts/IPlayerCharacterThrowInput.cs.meta` |
| `Assets/Features/PlayerCharacter/Scripts/IPlayerCharacterTraversalInput.cs` | `Assets/Features/PlayerCharacter/Scripts/Input/Contracts/IPlayerCharacterTraversalInput.cs` |
| `Assets/Features/PlayerCharacter/Scripts/IPlayerCharacterTraversalInput.cs.meta` | `Assets/Features/PlayerCharacter/Scripts/Input/Contracts/IPlayerCharacterTraversalInput.cs.meta` |
| `Assets/Features/PlayerCharacter/Scripts/IPlayerCharacterViewInput.cs` | `Assets/Features/PlayerCharacter/Scripts/Input/Contracts/IPlayerCharacterViewInput.cs` |
| `Assets/Features/PlayerCharacter/Scripts/IPlayerCharacterViewInput.cs.meta` | `Assets/Features/PlayerCharacter/Scripts/Input/Contracts/IPlayerCharacterViewInput.cs.meta` |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterActionPolicy.cs` | `Assets/Features/PlayerCharacter/Scripts/Actions/PlayerCharacterActionPolicy.cs` |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterActionPolicy.cs.meta` | `Assets/Features/PlayerCharacter/Scripts/Actions/PlayerCharacterActionPolicy.cs.meta` |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterMovement.cs` | `Assets/Features/PlayerCharacter/Scripts/Locomotion/PlayerCharacterMovement.cs` |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterMovement.cs.meta` | `Assets/Features/PlayerCharacter/Scripts/Locomotion/PlayerCharacterMovement.cs.meta` |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterStance.cs` | `Assets/Features/PlayerCharacter/Scripts/Locomotion/PlayerCharacterStance.cs` |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterStance.cs.meta` | `Assets/Features/PlayerCharacter/Scripts/Locomotion/PlayerCharacterStance.cs.meta` |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterTraversal.cs` | `Assets/Features/PlayerCharacter/Scripts/Locomotion/PlayerCharacterTraversal.cs` |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterTraversal.cs.meta` | `Assets/Features/PlayerCharacter/Scripts/Locomotion/PlayerCharacterTraversal.cs.meta` |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterMantle.cs` | `Assets/Features/PlayerCharacter/Scripts/Locomotion/PlayerCharacterMantle.cs` |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterMantle.cs.meta` | `Assets/Features/PlayerCharacter/Scripts/Locomotion/PlayerCharacterMantle.cs.meta` |
| `Assets/Features/PlayerCharacter/Scripts/MantleDefinition.cs` | `Assets/Features/PlayerCharacter/Scripts/Locomotion/MantleDefinition.cs` |
| `Assets/Features/PlayerCharacter/Scripts/MantleDefinition.cs.meta` | `Assets/Features/PlayerCharacter/Scripts/Locomotion/MantleDefinition.cs.meta` |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterController.cs` | `Assets/Features/PlayerCharacter/Scripts/Combat/PlayerCharacterCombatController.cs` |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterController.cs.meta` | `Assets/Features/PlayerCharacter/Scripts/Combat/PlayerCharacterCombatController.cs.meta` |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterGrenadeThrow.cs` | `Assets/Features/PlayerCharacter/Scripts/Combat/PlayerCharacterGrenadeThrow.cs` |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterGrenadeThrow.cs.meta` | `Assets/Features/PlayerCharacter/Scripts/Combat/PlayerCharacterGrenadeThrow.cs.meta` |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterRecoil.cs` | `Assets/Features/PlayerCharacter/Scripts/Combat/PlayerCharacterRecoil.cs` |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterRecoil.cs.meta` | `Assets/Features/PlayerCharacter/Scripts/Combat/PlayerCharacterRecoil.cs.meta` |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterWeaponMount.cs` | `Assets/Features/PlayerCharacter/Scripts/Combat/PlayerCharacterWeaponBracing.cs` |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterWeaponMount.cs.meta` | `Assets/Features/PlayerCharacter/Scripts/Combat/PlayerCharacterWeaponBracing.cs.meta` |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterLoadout.cs` | `Assets/Features/PlayerCharacter/Scripts/Equipment/PlayerCharacterLoadout.cs` |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterLoadout.cs.meta` | `Assets/Features/PlayerCharacter/Scripts/Equipment/PlayerCharacterLoadout.cs.meta` |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterWeaponEquipment.cs` | `Assets/Features/PlayerCharacter/Scripts/Equipment/PlayerCharacterWeaponEquipment.cs` |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterWeaponEquipment.cs.meta` | `Assets/Features/PlayerCharacter/Scripts/Equipment/PlayerCharacterWeaponEquipment.cs.meta` |
| `Assets/Features/PlayerCharacter/Scripts/InfantryLifeState.cs` | `Assets/Features/PlayerCharacter/Scripts/Life/InfantryLifeState.cs` |
| `Assets/Features/PlayerCharacter/Scripts/InfantryLifeState.cs.meta` | `Assets/Features/PlayerCharacter/Scripts/Life/InfantryLifeState.cs.meta` |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterRevive.cs` | `Assets/Features/PlayerCharacter/Scripts/Life/PlayerCharacterRevive.cs` |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterRevive.cs.meta` | `Assets/Features/PlayerCharacter/Scripts/Life/PlayerCharacterRevive.cs.meta` |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterView.cs` | `Assets/Features/PlayerCharacter/Scripts/Camera/PlayerCharacterLookController.cs` |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterView.cs.meta` | `Assets/Features/PlayerCharacter/Scripts/Camera/PlayerCharacterLookController.cs.meta` |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterCamera.cs` | `Assets/Features/PlayerCharacter/Scripts/Camera/PlayerCharacterCamera.cs` |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterCamera.cs.meta` | `Assets/Features/PlayerCharacter/Scripts/Camera/PlayerCharacterCamera.cs.meta` |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterLean.cs` | `Assets/Features/PlayerCharacter/Scripts/Camera/PlayerCharacterLean.cs` |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterLean.cs.meta` | `Assets/Features/PlayerCharacter/Scripts/Camera/PlayerCharacterLean.cs.meta` |
| `Assets/Features/PlayerCharacter/Scripts/ProneCameraProfile.cs` | `Assets/Features/PlayerCharacter/Scripts/Camera/ProneCameraProfile.cs` |
| `Assets/Features/PlayerCharacter/Scripts/ProneCameraProfile.cs.meta` | `Assets/Features/PlayerCharacter/Scripts/Camera/ProneCameraProfile.cs.meta` |
| `Assets/Features/PlayerCharacter/Scripts/PlayerVehicleInteraction.cs` | `Assets/Features/PlayerCharacter/Scripts/Interaction/PlayerVehicleInteraction.cs` |
| `Assets/Features/PlayerCharacter/Scripts/PlayerVehicleInteraction.cs.meta` | `Assets/Features/PlayerCharacter/Scripts/Interaction/PlayerVehicleInteraction.cs.meta` |
| `Assets/Features/PlayerCharacter/Scripts/PlayerVehicleOccupant.cs` | `Assets/Features/PlayerCharacter/Scripts/Interaction/PlayerVehicleOccupant.cs` |
| `Assets/Features/PlayerCharacter/Scripts/PlayerVehicleOccupant.cs.meta` | `Assets/Features/PlayerCharacter/Scripts/Interaction/PlayerVehicleOccupant.cs.meta` |
| `Assets/Features/PlayerCharacter/Scripts/PlayerRifleStationInteraction.cs` | `Assets/Features/PlayerCharacter/Scripts/Interaction/PlayerRifleStationInteraction.cs` |
| `Assets/Features/PlayerCharacter/Scripts/PlayerRifleStationInteraction.cs.meta` | `Assets/Features/PlayerCharacter/Scripts/Interaction/PlayerRifleStationInteraction.cs.meta` |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterAnimationView.cs` | `Assets/Features/PlayerCharacter/Scripts/Presentation/Locomotion/PlayerCharacterAnimationView.cs` |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterAnimationView.cs.meta` | `Assets/Features/PlayerCharacter/Scripts/Presentation/Locomotion/PlayerCharacterAnimationView.cs.meta` |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterProneAnimationView.cs` | `Assets/Features/PlayerCharacter/Scripts/Presentation/Locomotion/PlayerCharacterProneAnimationView.cs` |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterProneAnimationView.cs.meta` | `Assets/Features/PlayerCharacter/Scripts/Presentation/Locomotion/PlayerCharacterProneAnimationView.cs.meta` |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterSlideAnimationView.cs` | `Assets/Features/PlayerCharacter/Scripts/Presentation/Locomotion/PlayerCharacterSlideAnimationView.cs` |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterSlideAnimationView.cs.meta` | `Assets/Features/PlayerCharacter/Scripts/Presentation/Locomotion/PlayerCharacterSlideAnimationView.cs.meta` |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterTacticalAnimationView.cs` | `Assets/Features/PlayerCharacter/Scripts/Presentation/Locomotion/PlayerCharacterTacticalAnimationView.cs` |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterTacticalAnimationView.cs.meta` | `Assets/Features/PlayerCharacter/Scripts/Presentation/Locomotion/PlayerCharacterTacticalAnimationView.cs.meta` |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterPostureView.cs` | `Assets/Features/PlayerCharacter/Scripts/Presentation/Locomotion/PlayerCharacterPostureView.cs` |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterPostureView.cs.meta` | `Assets/Features/PlayerCharacter/Scripts/Presentation/Locomotion/PlayerCharacterPostureView.cs.meta` |
| `Assets/Features/PlayerCharacter/Scripts/ArmedLocomotionAnimationSet.cs` | `Assets/Features/PlayerCharacter/Scripts/Presentation/Locomotion/ArmedLocomotionAnimationSet.cs` |
| `Assets/Features/PlayerCharacter/Scripts/ArmedLocomotionAnimationSet.cs.meta` | `Assets/Features/PlayerCharacter/Scripts/Presentation/Locomotion/ArmedLocomotionAnimationSet.cs.meta` |
| `Assets/Features/PlayerCharacter/Scripts/ProneAnimationSet.cs` | `Assets/Features/PlayerCharacter/Scripts/Presentation/Locomotion/ProneAnimationSet.cs` |
| `Assets/Features/PlayerCharacter/Scripts/ProneAnimationSet.cs.meta` | `Assets/Features/PlayerCharacter/Scripts/Presentation/Locomotion/ProneAnimationSet.cs.meta` |
| `Assets/Features/PlayerCharacter/Scripts/SlideAnimationSet.cs` | `Assets/Features/PlayerCharacter/Scripts/Presentation/Locomotion/SlideAnimationSet.cs` |
| `Assets/Features/PlayerCharacter/Scripts/SlideAnimationSet.cs.meta` | `Assets/Features/PlayerCharacter/Scripts/Presentation/Locomotion/SlideAnimationSet.cs.meta` |
| `Assets/Features/PlayerCharacter/Scripts/TacticalAnimationSet.cs` | `Assets/Features/PlayerCharacter/Scripts/Presentation/Locomotion/TacticalAnimationSet.cs` |
| `Assets/Features/PlayerCharacter/Scripts/TacticalAnimationSet.cs.meta` | `Assets/Features/PlayerCharacter/Scripts/Presentation/Locomotion/TacticalAnimationSet.cs.meta` |
| `Assets/Features/PlayerCharacter/Scripts/SkinnedPoseContactData.cs` | `Assets/Features/PlayerCharacter/Scripts/Presentation/Locomotion/SkinnedPoseContactData.cs` |
| `Assets/Features/PlayerCharacter/Scripts/SkinnedPoseContactData.cs.meta` | `Assets/Features/PlayerCharacter/Scripts/Presentation/Locomotion/SkinnedPoseContactData.cs.meta` |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterCombatView.cs` | `Assets/Features/PlayerCharacter/Scripts/Presentation/Combat/PlayerCharacterCombatView.cs` |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterCombatView.cs.meta` | `Assets/Features/PlayerCharacter/Scripts/Presentation/Combat/PlayerCharacterCombatView.cs.meta` |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterWeaponAimView.cs` | `Assets/Features/PlayerCharacter/Scripts/Presentation/Combat/PlayerCharacterWeaponAimView.cs` |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterWeaponAimView.cs.meta` | `Assets/Features/PlayerCharacter/Scripts/Presentation/Combat/PlayerCharacterWeaponAimView.cs.meta` |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterRifleReloadView.cs` | `Assets/Features/PlayerCharacter/Scripts/Presentation/Combat/PlayerCharacterRifleReloadView.cs` |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterRifleReloadView.cs.meta` | `Assets/Features/PlayerCharacter/Scripts/Presentation/Combat/PlayerCharacterRifleReloadView.cs.meta` |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterWeaponPoseState.cs` | `Assets/Features/PlayerCharacter/Scripts/Presentation/Combat/PlayerCharacterWeaponPoseState.cs` |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterWeaponPoseState.cs.meta` | `Assets/Features/PlayerCharacter/Scripts/Presentation/Combat/PlayerCharacterWeaponPoseState.cs.meta` |
| `Assets/Features/PlayerCharacter/Scripts/EquippedAnimationControllers.cs` | `Assets/Features/PlayerCharacter/Scripts/Presentation/Combat/EquippedAnimationControllers.cs` |
| `Assets/Features/PlayerCharacter/Scripts/EquippedAnimationControllers.cs.meta` | `Assets/Features/PlayerCharacter/Scripts/Presentation/Combat/EquippedAnimationControllers.cs.meta` |
| `Assets/Features/PlayerCharacter/Scripts/HumanoidArmSolver.cs` | `Assets/Features/PlayerCharacter/Scripts/Presentation/Combat/HumanoidArmSolver.cs` |
| `Assets/Features/PlayerCharacter/Scripts/HumanoidArmSolver.cs.meta` | `Assets/Features/PlayerCharacter/Scripts/Presentation/Combat/HumanoidArmSolver.cs.meta` |
| `Assets/Features/PlayerCharacter/Scripts/FirstPersonArmsView.cs` | `Assets/Features/PlayerCharacter/Scripts/Presentation/FirstPerson/FirstPersonArmsView.cs` |
| `Assets/Features/PlayerCharacter/Scripts/FirstPersonArmsView.cs.meta` | `Assets/Features/PlayerCharacter/Scripts/Presentation/FirstPerson/FirstPersonArmsView.cs.meta` |
| `Assets/Features/PlayerCharacter/Scripts/FirstPersonArmPose.cs` | `Assets/Features/PlayerCharacter/Scripts/Presentation/FirstPerson/FirstPersonArmPose.cs` |
| `Assets/Features/PlayerCharacter/Scripts/FirstPersonArmPose.cs.meta` | `Assets/Features/PlayerCharacter/Scripts/Presentation/FirstPerson/FirstPersonArmPose.cs.meta` |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterVisibility.cs` | `Assets/Features/PlayerCharacter/Scripts/Presentation/FirstPerson/PlayerCharacterVisibility.cs` |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterVisibility.cs.meta` | `Assets/Features/PlayerCharacter/Scripts/Presentation/FirstPerson/PlayerCharacterVisibility.cs.meta` |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterMotionProfile.cs` | `Assets/Features/PlayerCharacter/Scripts/Presentation/FirstPerson/PlayerCharacterMotionProfile.cs` |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterMotionProfile.cs.meta` | `Assets/Features/PlayerCharacter/Scripts/Presentation/FirstPerson/PlayerCharacterMotionProfile.cs.meta` |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterMotionState.cs` | `Assets/Features/PlayerCharacter/Scripts/Presentation/FirstPerson/PlayerCharacterMotionState.cs` |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterMotionState.cs.meta` | `Assets/Features/PlayerCharacter/Scripts/Presentation/FirstPerson/PlayerCharacterMotionState.cs.meta` |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterJumpPoseState.cs` | `Assets/Features/PlayerCharacter/Scripts/Presentation/FirstPerson/PlayerCharacterJumpPoseState.cs` |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterJumpPoseState.cs.meta` | `Assets/Features/PlayerCharacter/Scripts/Presentation/FirstPerson/PlayerCharacterJumpPoseState.cs.meta` |
| `Assets/Features/PlayerCharacter/Scripts/SeatedCharacterPose.cs` | `Assets/Features/PlayerCharacter/Scripts/Presentation/Seating/SeatedCharacterPose.cs` |
| `Assets/Features/PlayerCharacter/Scripts/SeatedCharacterPose.cs.meta` | `Assets/Features/PlayerCharacter/Scripts/Presentation/Seating/SeatedCharacterPose.cs.meta` |
| `Assets/Features/PlayerCharacter/Scripts/SeatedAnimationPlayback.cs` | `Assets/Features/PlayerCharacter/Scripts/Presentation/Seating/SeatedAnimationPlayback.cs` |
| `Assets/Features/PlayerCharacter/Scripts/SeatedAnimationPlayback.cs.meta` | `Assets/Features/PlayerCharacter/Scripts/Presentation/Seating/SeatedAnimationPlayback.cs.meta` |
| `Assets/Features/PlayerCharacter/Scripts/InfantryTrainingDummy.cs` | `Assets/Features/Training/Scripts/InfantryTrainingDummy.cs` |
| `Assets/Features/PlayerCharacter/Scripts/InfantryTrainingDummy.cs.meta` | `Assets/Features/Training/Scripts/InfantryTrainingDummy.cs.meta` |
| `Assets/Features/PlayerCharacter/Scripts/RifleSelectionStation.cs` | `Assets/Features/Training/Scripts/RifleSelectionStation.cs` |
| `Assets/Features/PlayerCharacter/Scripts/RifleSelectionStation.cs.meta` | `Assets/Features/Training/Scripts/RifleSelectionStation.cs.meta` |
| `Assets/Features/PlayerCharacter/Editor/LocalArmedLocomotionSetup.cs` | `Assets/Features/PlayerCharacter/Editor/Composition/ArmedLocomotionSceneSetup.cs` |
| `Assets/Features/PlayerCharacter/Editor/LocalArmedLocomotionSetup.cs.meta` | `Assets/Features/PlayerCharacter/Editor/Composition/ArmedLocomotionSceneSetup.cs.meta` |
| `Assets/Features/PlayerCharacter/Editor/LocalSlideAnimationSetup.cs` | `Assets/Features/PlayerCharacter/Editor/Composition/SlideAnimationSceneSetup.cs` |
| `Assets/Features/PlayerCharacter/Editor/LocalSlideAnimationSetup.cs.meta` | `Assets/Features/PlayerCharacter/Editor/Composition/SlideAnimationSceneSetup.cs.meta` |
| `Assets/Features/PlayerCharacter/Editor/LocalProneAnimationSetup.cs` | `Assets/Features/PlayerCharacter/Editor/Composition/ProneAnimationSceneSetup.cs` |
| `Assets/Features/PlayerCharacter/Editor/LocalProneAnimationSetup.cs.meta` | `Assets/Features/PlayerCharacter/Editor/Composition/ProneAnimationSceneSetup.cs.meta` |
| `Assets/Features/PlayerCharacter/Editor/LocalTacticalActionSetup.cs` | `Assets/Features/PlayerCharacter/Editor/Composition/TacticalActionSceneSetup.cs` |
| `Assets/Features/PlayerCharacter/Editor/LocalTacticalActionSetup.cs.meta` | `Assets/Features/PlayerCharacter/Editor/Composition/TacticalActionSceneSetup.cs.meta` |
| `Assets/Features/PlayerCharacter/Editor/MixamoArmedLocomotionImporter.cs` | `Assets/Features/PlayerCharacter/Editor/Importers/MixamoArmedLocomotionImporter.cs` |
| `Assets/Features/PlayerCharacter/Editor/MixamoArmedLocomotionImporter.cs.meta` | `Assets/Features/PlayerCharacter/Editor/Importers/MixamoArmedLocomotionImporter.cs.meta` |
| `Assets/Features/PlayerCharacter/Editor/MixamoSlideImporter.cs` | `Assets/Features/PlayerCharacter/Editor/Importers/MixamoSlideImporter.cs` |
| `Assets/Features/PlayerCharacter/Editor/MixamoSlideImporter.cs.meta` | `Assets/Features/PlayerCharacter/Editor/Importers/MixamoSlideImporter.cs.meta` |
| `Assets/Features/PlayerCharacter/Editor/TacticalActionImporter.cs` | `Assets/Features/PlayerCharacter/Editor/Importers/TacticalActionImporter.cs` |
| `Assets/Features/PlayerCharacter/Editor/TacticalActionImporter.cs.meta` | `Assets/Features/PlayerCharacter/Editor/Importers/TacticalActionImporter.cs.meta` |
| `Assets/Features/PlayerCharacter/Editor/GrenadeHandlingAnimationAuthoring.cs` | `Assets/Features/PlayerCharacter/Editor/Authoring/GrenadeHandlingAnimationAuthoring.cs` |
| `Assets/Features/PlayerCharacter/Editor/GrenadeHandlingAnimationAuthoring.cs.meta` | `Assets/Features/PlayerCharacter/Editor/Authoring/GrenadeHandlingAnimationAuthoring.cs.meta` |
| `Assets/Features/PlayerCharacter/Editor/GrenadeThrowCompositionSetup.cs` | `Assets/Features/PlayerCharacter/Editor/Authoring/GrenadeThrowCompositionSetup.cs` |
| `Assets/Features/PlayerCharacter/Editor/GrenadeThrowCompositionSetup.cs.meta` | `Assets/Features/PlayerCharacter/Editor/Authoring/GrenadeThrowCompositionSetup.cs.meta` |
| `Assets/Features/PlayerCharacter/Editor/InfantryTestTools.cs` | `Assets/Features/PlayerCharacter/Editor/Diagnostics/InfantryTestTools.cs` |
| `Assets/Features/PlayerCharacter/Editor/InfantryTestTools.cs.meta` | `Assets/Features/PlayerCharacter/Editor/Diagnostics/InfantryTestTools.cs.meta` |
| `Assets/Features/PlayerCharacter/Editor/PlayerCharacterAimDiagnostics.cs` | `Assets/Features/PlayerCharacter/Editor/Diagnostics/PlayerCharacterAimDiagnostics.cs` |
| `Assets/Features/PlayerCharacter/Editor/PlayerCharacterAimDiagnostics.cs.meta` | `Assets/Features/PlayerCharacter/Editor/Diagnostics/PlayerCharacterAimDiagnostics.cs.meta` |
| `Assets/Features/Weapon/Scrips/WeaponBase.cs` | `Assets/Features/Weapon/Scripts/Firing/WeaponBase.cs` |
| `Assets/Features/Weapon/Scrips/WeaponBase.cs.meta` | `Assets/Features/Weapon/Scripts/Firing/WeaponBase.cs.meta` |
| `Assets/Features/Weapon/Scrips/WeaponController.cs` | `Assets/Features/Weapon/Scripts/Firing/WeaponController.cs` |
| `Assets/Features/Weapon/Scrips/WeaponController.cs.meta` | `Assets/Features/Weapon/Scripts/Firing/WeaponController.cs.meta` |
| `Assets/Features/Weapon/Scrips/MachineGun.cs` | `Assets/Features/Weapon/Scripts/Firing/MachineGun.cs` |
| `Assets/Features/Weapon/Scrips/MachineGun.cs.meta` | `Assets/Features/Weapon/Scripts/Firing/MachineGun.cs.meta` |
| `Assets/Features/Weapon/Scrips/MachineGunAimPredictor.cs` | `Assets/Features/Weapon/Scripts/Firing/MachineGunAimPredictor.cs` |
| `Assets/Features/Weapon/Scrips/MachineGunAimPredictor.cs.meta` | `Assets/Features/Weapon/Scripts/Firing/MachineGunAimPredictor.cs.meta` |
| `Assets/Features/Weapon/Scrips/AirToAirMissileLauncher.cs` | `Assets/Features/Weapon/Scripts/Firing/AirToAirMissileLauncher.cs` |
| `Assets/Features/Weapon/Scrips/AirToAirMissileLauncher.cs.meta` | `Assets/Features/Weapon/Scripts/Firing/AirToAirMissileLauncher.cs.meta` |
| `Assets/Features/Weapon/Scrips/IWeaponInput.cs` | `Assets/Features/Weapon/Scripts/Firing/IWeaponInput.cs` |
| `Assets/Features/Weapon/Scrips/IWeaponInput.cs.meta` | `Assets/Features/Weapon/Scripts/Firing/IWeaponInput.cs.meta` |
| `Assets/Features/Weapon/Scrips/OverHeat.cs` | `Assets/Features/Weapon/Scripts/Firing/Overheat.cs` |
| `Assets/Features/Weapon/Scrips/OverHeat.cs.meta` | `Assets/Features/Weapon/Scripts/Firing/Overheat.cs.meta` |
| `Assets/Features/Weapon/Scrips/WeaponMagazine.cs` | `Assets/Features/Weapon/Scripts/Ammunition/WeaponMagazine.cs` |
| `Assets/Features/Weapon/Scrips/WeaponMagazine.cs.meta` | `Assets/Features/Weapon/Scripts/Ammunition/WeaponMagazine.cs.meta` |
| `Assets/Features/Weapon/Scrips/WeaponMagazineState.cs` | `Assets/Features/Weapon/Scripts/Ammunition/WeaponMagazineState.cs` |
| `Assets/Features/Weapon/Scrips/WeaponMagazineState.cs.meta` | `Assets/Features/Weapon/Scripts/Ammunition/WeaponMagazineState.cs.meta` |
| `Assets/Features/Weapon/Scrips/IWeaponMagazine.cs` | `Assets/Features/Weapon/Scripts/Ammunition/IWeaponMagazine.cs` |
| `Assets/Features/Weapon/Scrips/IWeaponMagazine.cs.meta` | `Assets/Features/Weapon/Scripts/Ammunition/IWeaponMagazine.cs.meta` |
| `Assets/Features/Weapon/Scrips/IRefillProgressSource.cs` | `Assets/Features/Weapon/Scripts/Ammunition/IRefillProgressSource.cs` |
| `Assets/Features/Weapon/Scrips/IRefillProgressSource.cs.meta` | `Assets/Features/Weapon/Scripts/Ammunition/IRefillProgressSource.cs.meta` |
| `Assets/Features/Weapon/Scrips/MeleeAttack.cs` | `Assets/Features/Weapon/Scripts/Melee/MeleeAttack.cs` |
| `Assets/Features/Weapon/Scrips/MeleeAttack.cs.meta` | `Assets/Features/Weapon/Scripts/Melee/MeleeAttack.cs.meta` |
| `Assets/Features/Weapon/Scrips/InfantryWeaponDefinition.cs` | `Assets/Features/Weapon/Scripts/Definitions/InfantryWeaponDefinition.cs` |
| `Assets/Features/Weapon/Scrips/InfantryWeaponDefinition.cs.meta` | `Assets/Features/Weapon/Scripts/Definitions/InfantryWeaponDefinition.cs.meta` |
| `Assets/Features/Weapon/Scrips/GrenadeDefinition.cs` | `Assets/Features/Weapon/Scripts/Definitions/GrenadeDefinition.cs` |
| `Assets/Features/Weapon/Scrips/GrenadeDefinition.cs.meta` | `Assets/Features/Weapon/Scripts/Definitions/GrenadeDefinition.cs.meta` |
| `Assets/Features/Weapon/Scrips/WeaponModel.cs` | `Assets/Features/Weapon/Scripts/Models/WeaponModel.cs` |
| `Assets/Features/Weapon/Scrips/WeaponModel.cs.meta` | `Assets/Features/Weapon/Scripts/Models/WeaponModel.cs.meta` |
| `Assets/Features/Weapon/Scrips/MachineGunAudio.cs` | `Assets/Features/Weapon/Scripts/Audio/MachineGunAudio.cs` |
| `Assets/Features/Weapon/Scrips/MachineGunAudio.cs.meta` | `Assets/Features/Weapon/Scripts/Audio/MachineGunAudio.cs.meta` |
| `Assets/Features/Weapon/Scrips/MissileLaunchAudio.cs` | `Assets/Features/Weapon/Scripts/Audio/MissileLaunchAudio.cs` |
| `Assets/Features/Weapon/Scrips/MissileLaunchAudio.cs.meta` | `Assets/Features/Weapon/Scripts/Audio/MissileLaunchAudio.cs.meta` |
| `Assets/Features/Fighter/Scripts/Movement/FighterEngine.cs` | `Assets/Features/Fighter/Scripts/Movement/FighterEngineActivation.cs` |
| `Assets/Features/Fighter/Scripts/Movement/FighterEngine.cs.meta` | `Assets/Features/Fighter/Scripts/Movement/FighterEngineActivation.cs.meta` |
| `Assets/Framework/Unility/CursorLock.cs` | `Assets/Framework/Utility/CursorLock.cs` |
| `Assets/Framework/Unility/CursorLock.cs.meta` | `Assets/Framework/Utility/CursorLock.cs.meta` |
| `Assets/Features/Dummy/Scripts/DummyDebug.cs` | `Assets/Features/Training/Scripts/DummyDebug.cs` |
| `Assets/Features/Dummy/Scripts/DummyDebug.cs.meta` | `Assets/Features/Training/Scripts/DummyDebug.cs.meta` |
| `Assets/Features/Dummy/Scripts/DummyShooter.cs` | `Assets/Features/Training/Scripts/DummyShooter.cs` |
| `Assets/Features/Dummy/Scripts/DummyShooter.cs.meta` | `Assets/Features/Training/Scripts/DummyShooter.cs.meta` |
| `Assets/Features/Dummy/Scripts/DummyMissileShooter.cs` | `Assets/Features/Training/Scripts/DummyMissileShooter.cs` |
| `Assets/Features/Dummy/Scripts/DummyMissileShooter.cs.meta` | `Assets/Features/Training/Scripts/DummyMissileShooter.cs.meta` |
| `Assets/Features/Dummy/Scripts/DummyAirTargetLockDebug.cs` | `Assets/Features/Training/Scripts/DummyAirTargetLockDebug.cs` |
| `Assets/Features/Dummy/Scripts/DummyAirTargetLockDebug.cs.meta` | `Assets/Features/Training/Scripts/DummyAirTargetLockDebug.cs.meta` |
| `Assets/Features/UI/PlayerCombatHud.cs` | `Assets/Features/UI/Player/PlayerCombatHud.cs` |
| `Assets/Features/UI/PlayerCombatHud.cs.meta` | `Assets/Features/UI/Player/PlayerCombatHud.cs.meta` |
| `Assets/Features/UI/GrenadeTrajectoryView.cs` | `Assets/Features/UI/Player/GrenadeTrajectoryView.cs` |
| `Assets/Features/UI/GrenadeTrajectoryView.cs.meta` | `Assets/Features/UI/Player/GrenadeTrajectoryView.cs.meta` |
| `Assets/Features/UI/VehicleInteractionView.cs` | `Assets/Features/UI/Player/VehicleInteractionView.cs` |
| `Assets/Features/UI/VehicleInteractionView.cs.meta` | `Assets/Features/UI/Player/VehicleInteractionView.cs.meta` |
| `Assets/Features/UI/HUD/Scripts/AirTargetLockView.cs` | `Assets/Features/UI/Fighter/AirTargetLockView.cs` |
| `Assets/Features/UI/HUD/Scripts/AirTargetLockView.cs.meta` | `Assets/Features/UI/Fighter/AirTargetLockView.cs.meta` |
| `Assets/Features/UI/HUD/Scripts/FighterCombatHudView.cs` | `Assets/Features/UI/Fighter/FighterCombatHudView.cs` |
| `Assets/Features/UI/HUD/Scripts/FighterCombatHudView.cs.meta` | `Assets/Features/UI/Fighter/FighterCombatHudView.cs.meta` |
| `Assets/Features/UI/HUD/Scripts/FighterGunAimReticleMover.cs` | `Assets/Features/UI/Fighter/FighterGunAimReticleMover.cs` |
| `Assets/Features/UI/HUD/Scripts/FighterGunAimReticleMover.cs.meta` | `Assets/Features/UI/Fighter/FighterGunAimReticleMover.cs.meta` |
| `Assets/Features/UI/HUD/Scripts/FighterGunAimView.cs` | `Assets/Features/UI/Fighter/FighterGunAimView.cs` |
| `Assets/Features/UI/HUD/Scripts/FighterGunAimView.cs.meta` | `Assets/Features/UI/Fighter/FighterGunAimView.cs.meta` |
| `Assets/Features/UI/HUD/Scripts/FighterHudView.cs` | `Assets/Features/UI/Fighter/FighterHudView.cs` |
| `Assets/Features/UI/HUD/Scripts/FighterHudView.cs.meta` | `Assets/Features/UI/Fighter/FighterHudView.cs.meta` |
| `Assets/Features/UI/HUD/Scripts/HeatDotRingBuilder.cs` | `Assets/Features/UI/Shared/HeatDotRingBuilder.cs` |
| `Assets/Features/UI/HUD/Scripts/HeatDotRingBuilder.cs.meta` | `Assets/Features/UI/Shared/HeatDotRingBuilder.cs.meta` |
| `Assets/Features/UI/HUD/Scripts/HeatDotsView.cs` | `Assets/Features/UI/Shared/HeatDotsView.cs` |
| `Assets/Features/UI/HUD/Scripts/HeatDotsView.cs.meta` | `Assets/Features/UI/Shared/HeatDotsView.cs.meta` |
| `Assets/Features/UI/HUD/Scripts/LockOnAreaGraphic.cs` | `Assets/Features/UI/Fighter/LockOnAreaGraphic.cs` |
| `Assets/Features/UI/HUD/Scripts/LockOnAreaGraphic.cs.meta` | `Assets/Features/UI/Fighter/LockOnAreaGraphic.cs.meta` |
| `Assets/Features/UI/HUD/Scripts/MissileDirectionIndicatorGraphic.cs` | `Assets/Features/UI/Fighter/MissileDirectionIndicatorGraphic.cs` |
| `Assets/Features/UI/HUD/Scripts/MissileDirectionIndicatorGraphic.cs.meta` | `Assets/Features/UI/Fighter/MissileDirectionIndicatorGraphic.cs.meta` |
| `Assets/Features/UI/HUD/Scripts/MissileDirectionIndicatorView.cs` | `Assets/Features/UI/Fighter/MissileDirectionIndicatorView.cs` |
| `Assets/Features/UI/HUD/Scripts/MissileDirectionIndicatorView.cs.meta` | `Assets/Features/UI/Fighter/MissileDirectionIndicatorView.cs.meta` |
| `Assets/Features/UI/HUD/Scripts/MissileWarningView.cs` | `Assets/Features/UI/Fighter/MissileWarningView.cs` |
| `Assets/Features/UI/HUD/Scripts/MissileWarningView.cs.meta` | `Assets/Features/UI/Fighter/MissileWarningView.cs.meta` |
| `Assets/Features/UI/HUD/Scripts/RefillProgressView.cs` | `Assets/Features/UI/Shared/RefillProgressView.cs` |
| `Assets/Features/UI/HUD/Scripts/RefillProgressView.cs.meta` | `Assets/Features/UI/Shared/RefillProgressView.cs.meta` |
| `Assets/Features/UI/HUD/Scripts/WeaponSelectionView.cs` | `Assets/Features/UI/Shared/WeaponSelectionView.cs` |
| `Assets/Features/UI/HUD/Scripts/WeaponSelectionView.cs.meta` | `Assets/Features/UI/Shared/WeaponSelectionView.cs.meta` |

## 내용 변경 파일 전체 목록

C#는 위 타입·namespace 참조 치환만 수행했다. 씬·프리팹·설정 에셋은 같은 타입 이름 표기 치환만 수행한다. Docs/Manual은 현재 링크·이름 갱신이다.
- `Assets/Features/Fighter/Prefabs/Fighter.prefab`
- `Assets/Features/Fighter/Scripts/Movement/FighterEngineActivation.cs`
- `Assets/Features/PlayerCharacter/Editor/Composition/ArmedLocomotionSceneSetup.cs`
- `Assets/Features/PlayerCharacter/Editor/Composition/ProneAnimationSceneSetup.cs`
- `Assets/Features/PlayerCharacter/Editor/Composition/SlideAnimationSceneSetup.cs`
- `Assets/Features/PlayerCharacter/Editor/Composition/TacticalActionSceneSetup.cs`
- `Assets/Features/PlayerCharacter/Editor/Diagnostics/InfantryTestTools.cs`
- `Assets/Features/PlayerCharacter/Editor/Diagnostics/PlayerCharacterAimDiagnostics.cs`
- `Assets/Features/PlayerCharacter/Scripts/Actions/PlayerCharacterActionPolicy.cs`
- `Assets/Features/PlayerCharacter/Scripts/Camera/PlayerCharacterCamera.cs`
- `Assets/Features/PlayerCharacter/Scripts/Camera/PlayerCharacterLookController.cs`
- `Assets/Features/PlayerCharacter/Scripts/Combat/PlayerCharacterCombatController.cs`
- `Assets/Features/PlayerCharacter/Scripts/Combat/PlayerCharacterGrenadeThrow.cs`
- `Assets/Features/PlayerCharacter/Scripts/Combat/PlayerCharacterRecoil.cs`
- `Assets/Features/PlayerCharacter/Scripts/Combat/PlayerCharacterWeaponBracing.cs`
- `Assets/Features/PlayerCharacter/Scripts/Interaction/PlayerRifleStationInteraction.cs`
- `Assets/Features/PlayerCharacter/Scripts/Life/InfantryLifeState.cs`
- `Assets/Features/PlayerCharacter/Scripts/Presentation/Combat/PlayerCharacterCombatView.cs`
- `Assets/Features/PlayerCharacter/Scripts/Presentation/Combat/PlayerCharacterRifleReloadView.cs`
- `Assets/Features/PlayerCharacter/Scripts/Presentation/Combat/PlayerCharacterWeaponAimView.cs`
- `Assets/Features/PlayerCharacter/Scripts/Presentation/FirstPerson/FirstPersonArmsView.cs`
- `Assets/Features/PlayerCharacter/Scripts/Presentation/FirstPerson/PlayerCharacterVisibility.cs`
- `Assets/Features/PlayerCharacter/Scripts/Presentation/Locomotion/PlayerCharacterProneAnimationView.cs`
- `Assets/Features/PlayerCharacter/Scripts/Presentation/Locomotion/PlayerCharacterSlideAnimationView.cs`
- `Assets/Features/PlayerCharacter/Scripts/Presentation/Locomotion/PlayerCharacterTacticalAnimationView.cs`
- `Assets/Features/UI/Player/PlayerCombatHud.cs`
- `Assets/Framework/Utility/CursorLock.cs`
- `Assets/Scenes/SampleScene.unity`
- `Assets/Tests/PlayMode/AssaultRifleTests.cs`
- `Assets/Tests/PlayMode/BulletTracerTests.cs`
- `Assets/Tests/PlayMode/BulletVisualTests.cs`
- `Assets/Tests/PlayMode/CombatStructureTests.cs`
- `Assets/Tests/PlayMode/FighterEngineTests.cs`
- `Assets/Tests/PlayMode/FighterScaleTests.cs`
- `Assets/Tests/PlayMode/InfantryTrainingGroundTests.cs`
- `Assets/Tests/PlayMode/PersonalCombatMechanicsTests.cs`
- `Assets/Tests/PlayMode/PlayerCharacterAnimationTests.cs`
- `Assets/Tests/PlayMode/PlayerCharacterCameraTests.cs`
- `Assets/Tests/PlayMode/PlayerCharacterCombatTests.cs`
- `Assets/Tests/PlayMode/PlayerCharacterDirectionalAnimationTests.cs`
- `Assets/Tests/PlayMode/PlayerCharacterInfantryTests.cs`
- `Assets/Tests/PlayMode/PlayerCharacterMovementTests.cs`
- `Assets/Tests/PlayMode/PlayerCharacterNativeAimInputTests.cs`
- `Assets/Tests/PlayMode/PlayerCharacterViewTests.cs`
- `Assets/Tests/PlayMode/PlayerEquipmentTests.cs`
- `Assets/Tests/PlayMode/ProneAnimationTests.cs`
- `Docs/Manual/Architecture.md`
- `Docs/Manual/CodeIndex.md`
- `Docs/Manual/Features.md`
- `Docs/Manual/README.md`
- `Docs/Manual/Review.md`
- `Docs/Manual/SceneAndInput.md`

## 신규 파일 전체 목록

폴더 meta는 폴더 식별용이며 기존 스크립트 GUID와 무관하다. ScriptNavigation은 기능별 읽기 순서 안내다.
- `Assets/Features/PlayerCharacter/Scripts/Input.meta`
- `Assets/Features/PlayerCharacter/Scripts/Input/Contracts.meta`
- `Assets/Features/PlayerCharacter/Scripts/Actions.meta`
- `Assets/Features/PlayerCharacter/Scripts/Locomotion.meta`
- `Assets/Features/PlayerCharacter/Scripts/Combat.meta`
- `Assets/Features/PlayerCharacter/Scripts/Equipment.meta`
- `Assets/Features/PlayerCharacter/Scripts/Life.meta`
- `Assets/Features/PlayerCharacter/Scripts/Camera.meta`
- `Assets/Features/PlayerCharacter/Scripts/Interaction.meta`
- `Assets/Features/PlayerCharacter/Scripts/Presentation.meta`
- `Assets/Features/PlayerCharacter/Scripts/Presentation/Locomotion.meta`
- `Assets/Features/PlayerCharacter/Scripts/Presentation/Combat.meta`
- `Assets/Features/PlayerCharacter/Scripts/Presentation/FirstPerson.meta`
- `Assets/Features/PlayerCharacter/Scripts/Presentation/Seating.meta`
- `Assets/Features/Training.meta`
- `Assets/Features/Training/Scripts.meta`
- `Assets/Features/PlayerCharacter/Editor/Composition.meta`
- `Assets/Features/PlayerCharacter/Editor/Importers.meta`
- `Assets/Features/PlayerCharacter/Editor/Authoring.meta`
- `Assets/Features/PlayerCharacter/Editor/Diagnostics.meta`
- `Assets/Features/Weapon/Scripts/Firing.meta`
- `Assets/Features/Weapon/Scripts/Ammunition.meta`
- `Assets/Features/Weapon/Scripts/Melee.meta`
- `Assets/Features/Weapon/Scripts/Definitions.meta`
- `Assets/Features/Weapon/Scripts/Models.meta`
- `Assets/Features/Weapon/Scripts/Audio.meta`
- `Assets/Features/UI/Player.meta`
- `Assets/Features/UI/Fighter.meta`
- `Docs/Manual/ScriptNavigation.md`
- `Docs/AI/ScriptStructurePhase1.md` (이 보고서)

## OOP·SOLID 검토

기존 상태 소유권·상속·좁은 계약을 보존했다. 역할 이름과 기능별 응집도를 개선했다. 신규 추상화는 추가하지 않았다. CombatView 모델 책임 분리와 WeaponAimView 계산 분리는 아직 적용하지 않은 후속 단계다.

## 검증

정적 검증과 전후 PlayMode 결과는 아래 최종 검증 결과에 기록했다.

## 제거된 빈 폴더 메타

- `Assets/Features/Dummy/Scripts.meta`
- `Assets/Features/UI/HUD/Scripts.meta`

이동 후 빈 폴더 식별자만 제거했다. 두 GUID는 다른 에셋에서 참조하지 않는다. 스크립트 메타는 모두 새 경로에 보존했다. `Scrips`, `Unility` 빈 폴더도 제거했으며 해당 폴더 메타는 새 이름으로 이동되어 유지된다.

## 파일별 코드 치환 상세

메서드 본문, 조건, 수치, 필드는 아래 이름 치환 외 바이트 단위로 동일함을 확인했다.

| 파일 | 변경한 코드 토큰 |
|---|---|
| `Assets/Features/Fighter/Scripts/Movement/FighterEngineActivation.cs` | `FighterEngine` → `FighterEngineActivation` |
| `Assets/Features/PlayerCharacter/Editor/Composition/ArmedLocomotionSceneSetup.cs` | `LocalArmedLocomotionSetup` → `ArmedLocomotionSceneSetup`, `LocalArmedLocomotionBuildProcessor` → `ArmedLocomotionBuildProcessor` |
| `Assets/Features/PlayerCharacter/Editor/Composition/ProneAnimationSceneSetup.cs` | `PlayerCharacterController` → `PlayerCharacterCombatController`, `LocalProneAnimationSetup` → `ProneAnimationSceneSetup`, `LocalProneAnimationBuildProcessor` → `ProneAnimationBuildProcessor` |
| `Assets/Features/PlayerCharacter/Editor/Composition/SlideAnimationSceneSetup.cs` | `PlayerCharacterController` → `PlayerCharacterCombatController`, `LocalSlideAnimationSetup` → `SlideAnimationSceneSetup`, `LocalSlideAnimationBuildProcessor` → `SlideAnimationBuildProcessor` |
| `Assets/Features/PlayerCharacter/Editor/Composition/TacticalActionSceneSetup.cs` | `PlayerCharacterController` → `PlayerCharacterCombatController`, `PlayerCharacterView` → `PlayerCharacterLookController`, `LocalTacticalActionSetup` → `TacticalActionSceneSetup`, `LocalTacticalActionBuildProcessor` → `TacticalActionBuildProcessor` |
| `Assets/Features/PlayerCharacter/Editor/Diagnostics/InfantryTestTools.cs` | `PlayerCharacterController` → `PlayerCharacterCombatController` |
| `Assets/Features/PlayerCharacter/Editor/Diagnostics/PlayerCharacterAimDiagnostics.cs` | `PlayerCharacterController` → `PlayerCharacterCombatController`, `PlayerCharacterView` → `PlayerCharacterLookController` |
| `Assets/Features/PlayerCharacter/Scripts/Actions/PlayerCharacterActionPolicy.cs` | `PlayerCharacterController` → `PlayerCharacterCombatController`, `PlayerCharacterView` → `PlayerCharacterLookController` |
| `Assets/Features/PlayerCharacter/Scripts/Camera/PlayerCharacterCamera.cs` | `PlayerCharacterView` → `PlayerCharacterLookController` |
| `Assets/Features/PlayerCharacter/Scripts/Camera/PlayerCharacterLookController.cs` | `PlayerCharacterView` → `PlayerCharacterLookController` |
| `Assets/Features/PlayerCharacter/Scripts/Combat/PlayerCharacterCombatController.cs` | `PlayerCharacterController` → `PlayerCharacterCombatController` |
| `Assets/Features/PlayerCharacter/Scripts/Combat/PlayerCharacterGrenadeThrow.cs` | `PlayerCharacterController` → `PlayerCharacterCombatController` |
| `Assets/Features/PlayerCharacter/Scripts/Combat/PlayerCharacterRecoil.cs` | `PlayerCharacterController` → `PlayerCharacterCombatController`, `PlayerCharacterView` → `PlayerCharacterLookController`, `PlayerCharacterWeaponMount` → `PlayerCharacterWeaponBracing` |
| `Assets/Features/PlayerCharacter/Scripts/Combat/PlayerCharacterWeaponBracing.cs` | `PlayerCharacterController` → `PlayerCharacterCombatController`, `PlayerCharacterView` → `PlayerCharacterLookController`, `PlayerCharacterWeaponMount` → `PlayerCharacterWeaponBracing` |
| `Assets/Features/PlayerCharacter/Scripts/Interaction/PlayerRifleStationInteraction.cs` | `PlayerCharacterController` → `PlayerCharacterCombatController` |
| `Assets/Features/PlayerCharacter/Scripts/Life/InfantryLifeState.cs` | `PlayerCharacterController` → `PlayerCharacterCombatController` |
| `Assets/Features/PlayerCharacter/Scripts/Presentation/Combat/PlayerCharacterCombatView.cs` | `PlayerCharacterController` → `PlayerCharacterCombatController`, `PlayerCharacterView` → `PlayerCharacterLookController` |
| `Assets/Features/PlayerCharacter/Scripts/Presentation/Combat/PlayerCharacterRifleReloadView.cs` | `PlayerCharacterController` → `PlayerCharacterCombatController`, `PlayerCharacterView` → `PlayerCharacterLookController` |
| `Assets/Features/PlayerCharacter/Scripts/Presentation/Combat/PlayerCharacterWeaponAimView.cs` | `PlayerCharacterController` → `PlayerCharacterCombatController`, `PlayerCharacterView` → `PlayerCharacterLookController`, `PlayerCharacterWeaponMount` → `PlayerCharacterWeaponBracing` |
| `Assets/Features/PlayerCharacter/Scripts/Presentation/FirstPerson/FirstPersonArmsView.cs` | `PlayerCharacterController` → `PlayerCharacterCombatController`, `PlayerCharacterView` → `PlayerCharacterLookController` |
| `Assets/Features/PlayerCharacter/Scripts/Presentation/FirstPerson/PlayerCharacterVisibility.cs` | `PlayerCharacterView` → `PlayerCharacterLookController` |
| `Assets/Features/PlayerCharacter/Scripts/Presentation/Locomotion/PlayerCharacterProneAnimationView.cs` | `PlayerCharacterController` → `PlayerCharacterCombatController` |
| `Assets/Features/PlayerCharacter/Scripts/Presentation/Locomotion/PlayerCharacterSlideAnimationView.cs` | `PlayerCharacterController` → `PlayerCharacterCombatController` |
| `Assets/Features/PlayerCharacter/Scripts/Presentation/Locomotion/PlayerCharacterTacticalAnimationView.cs` | `PlayerCharacterView` → `PlayerCharacterLookController` |
| `Assets/Features/UI/Player/PlayerCombatHud.cs` | `PlayerCharacterController` → `PlayerCharacterCombatController`, `PlayerCharacterWeaponMount` → `PlayerCharacterWeaponBracing` |
| `Assets/Framework/Utility/CursorLock.cs` | `Battlefield.Framework.Unility` → `Battlefield.Framework.Utility` |
| `Assets/Tests/PlayMode/AssaultRifleTests.cs` | `PlayerCharacterController` → `PlayerCharacterCombatController` |
| `Assets/Tests/PlayMode/BulletTracerTests.cs` | `PlayerCharacterController` → `PlayerCharacterCombatController` |
| `Assets/Tests/PlayMode/BulletVisualTests.cs` | `PlayerCharacterController` → `PlayerCharacterCombatController` |
| `Assets/Tests/PlayMode/CombatStructureTests.cs` | `PlayerCharacterController` → `PlayerCharacterCombatController` |
| `Assets/Tests/PlayMode/FighterEngineTests.cs` | `FighterEngine` → `FighterEngineActivation` |
| `Assets/Tests/PlayMode/FighterScaleTests.cs` | `FighterEngine` → `FighterEngineActivation` |
| `Assets/Tests/PlayMode/InfantryTrainingGroundTests.cs` | `PlayerCharacterController` → `PlayerCharacterCombatController` |
| `Assets/Tests/PlayMode/PersonalCombatMechanicsTests.cs` | `PlayerCharacterController` → `PlayerCharacterCombatController` |
| `Assets/Tests/PlayMode/PlayerCharacterAnimationTests.cs` | `PlayerCharacterController` → `PlayerCharacterCombatController` |
| `Assets/Tests/PlayMode/PlayerCharacterCameraTests.cs` | `PlayerCharacterController` → `PlayerCharacterCombatController`, `PlayerCharacterView` → `PlayerCharacterLookController` |
| `Assets/Tests/PlayMode/PlayerCharacterCombatTests.cs` | `PlayerCharacterController` → `PlayerCharacterCombatController` |
| `Assets/Tests/PlayMode/PlayerCharacterDirectionalAnimationTests.cs` | `PlayerCharacterController` → `PlayerCharacterCombatController`, `PlayerCharacterView` → `PlayerCharacterLookController` |
| `Assets/Tests/PlayMode/PlayerCharacterInfantryTests.cs` | `PlayerCharacterWeaponMount` → `PlayerCharacterWeaponBracing` |
| `Assets/Tests/PlayMode/PlayerCharacterMovementTests.cs` | `PlayerCharacterController` → `PlayerCharacterCombatController` |
| `Assets/Tests/PlayMode/PlayerCharacterNativeAimInputTests.cs` | `PlayerCharacterController` → `PlayerCharacterCombatController`, `PlayerCharacterView` → `PlayerCharacterLookController` |
| `Assets/Tests/PlayMode/PlayerCharacterViewTests.cs` | `PlayerCharacterView` → `PlayerCharacterLookController` |
| `Assets/Tests/PlayMode/PlayerEquipmentTests.cs` | `PlayerCharacterController` → `PlayerCharacterCombatController` |
| `Assets/Tests/PlayMode/ProneAnimationTests.cs` | `PlayerCharacterController` → `PlayerCharacterCombatController` |

## 최종 검증 결과

- 격리 프로젝트 Unity 6000.4.8f1에서 `-batchmode -noaudio`로 전후 동일 필터 실행.
- 전: {'total': '43', 'passed': '39', 'failed': '4', 'skipped': '0', 'duration': '36.0526555'}
- 후: {'total': '43', 'passed': '39', 'failed': '4', 'skipped': '0', 'duration': '34.7220051'}
- 결과가 바뀐 테스트: []
- 기존 실패 4건은 그대로이며 테스트 기대값·입력·실패를 수정하거나 숨기지 않았다.
- 격리 프로젝트에만 있던 과거 제작 도구 5개도 동일한 타입명 치환이 필요했다. 첫 컴파일 실패 로그는 보존했고 재실행에서 테스트 실행까지 도달했다.
- 기존 C# 238개는 명시한 타입/namespace 치환 외 변경 없음. 메타·asmdef 450개 내용 유지(별도 빈 폴더 메타 2개 제거). 이동 스크립트 108개의 GUID 유지, Assets 내 GUID 중복 없음.
- SampleScene.unity의 타입명 3곳과 Fighter.prefab의 타입명 1곳만 변경. 수치·필드·파일ID·GUID·계층 구조 보존.
- 현재 매뉴얼의 로컬 파일 링크 미해결 0건.
- 테스트 대상: PlayerCharacterCombatTests, PlayerCharacterCombatInputTests, TacticalActionTests, FighterEngineTests, VehicleSeatTests, CombatStructureTests.
- 증거: `Library/CodexStructure/baseline.xml`, `after.xml`, `after-retry.log`, `static-validation.json`, `test-comparison.json`.

기존 실패:
- `Battlefield.Tests.PlayMode.PlayerCharacterCombatTests.EmptyMagazineReloadsOnlyAfterFireIsPressedAgain`
- `Battlefield.Tests.PlayMode.PlayerCharacterCombatTests.FiringKeepsGripWhileReloadAndMeleePlayTheirActionStates`
- `Battlefield.Tests.PlayMode.PlayerCharacterCombatTests.RealMuzzleAmmoReloadAndFirstPersonPresentationAreConnected`
- `Battlefield.Tests.PlayMode.TacticalActionTests.GrenadeClickCannotFireTheGunAfterReturningButANewClickCan`

### 검증 한계

원본 프로젝트의 첫 배치 실행은 종료 코드 1로 종료되어 격리 복사본을 사용했다. 원본 Editor의 재임포트/Inspector 연결, 애니메이션 시각 품질, 전체 테스트와 Player 빌드는 미확인이다. 기존 실패가 남아 있으므로 전체 테스트 통과 또는 릴리스 준비 완료를 주장하지 않는다. 기능별 컴파일과 이번 선택 테스트에서 새 회귀는 확인되지 않았다. CombatView 모델 책임·WeaponAimView 계산 분리는 후속 단계로 남아 있다.
