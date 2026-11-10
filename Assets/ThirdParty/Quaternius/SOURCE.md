# Quaternius crouch clips

Source: https://quaternius.itch.io/universal-animation-library
Author: Quaternius. License: CC0 1.0 Universal; see License.txt.
Official free Standard edition downloaded 2026-10-27.
Archive SHA256: cc73fc4e495b82958207316596317a3f40b9fa38065bde1027937452da537724

CrouchIdle.anim and CrouchWalk.anim were extracted by Unity 6000.4.8f1 from Unity/UAL1_Standard.fbx (no root motion), clips Armature|Crouch_Idle_Loop and Armature|Crouch_Fwd_Loop. Imported as Humanoid and retargeted to the existing Starter Assets Armature. Loop/root-pose settings are configured. The extracted Humanoid RootQ/RootT curves are rotated 180 degrees about Y to match the existing Armature forward axis; source muscle poses and actor/model transforms are preserved. Model and movement/controller code are not imported from the pack. Original archive and source FBX remain in Temp only.

## Personal combat clips (2026-10-27)

From the same CC0 Standard archive and no-root-motion Unity/UAL1_Standard.fbx, seven additional Humanoid clips were extracted:

- PistolHoldDown.anim: Armature|Pistol_Aim_Down
- PistolHoldNeutral.anim: Armature|Pistol_Aim_Neutral
- PistolHoldUp.anim: Armature|Pistol_Aim_Up
- PistolShoot.anim: Armature|Pistol_Shoot (0.6333334 seconds)
- PistolReload.anim: Armature|Pistol_Reload (1.666667 seconds)
- PunchJab.anim: Armature|Punch_Jab (0.8666667 seconds)
- PunchCross.anim: Armature|Punch_Cross (1 second)

All use the same 180-degree Humanoid RootQ/RootT forward correction as the crouch clips. Aim poses loop; actions do not. UpperCombat.mask preserves pelvis/spine and lower-body locomotion. PunchCross is reused for striking with the pistol held; this is not a dedicated pistol-whip source clip. Source FBX remains outside Assets and is not needed at runtime. Gun provenance is in Guns/SOURCE.md.

## Infantry state clips (2026-10-27)

From the same official free CC0 Standard archive (SHA256 cc73fc4e495b82958207316596317a3f40b9fa38065bde1027937452da537724), Unity 6000.4.8f1 extracted:

- InfantryDeath.anim: Armature|Death01, no loop.
- InfantryRevive.anim: Armature|Fixing_Kneeling, loop enabled. This is a kneeling work animation reused for prototype revival, not a dedicated revive animation.

Both use Humanoid import and the same 180-degree RootQ/RootT forward correction. The source model/controller and full FBX are not included in Assets. The source archive was restored under Library/CodexInfantryWork after Unity cleared the old Temp cache; its hash matches the archive previously used for crouch and combat clips. Prone is a project-owned procedural test pose; no dedicated crawl/slide/vault clip is claimed from this Standard archive.

## Seated idle (2026-10-28)

SeatedIdle.anim is extracted from Armature|Sitting_Idle_Loop in the same official CC0 Standard archive, Unity/UAL1_Standard.fbx (SHA256 cc73fc4e495b82958207316596317a3f40b9fa38065bde1027937452da537724). Humanoid import, 1.666667 seconds, looping, with the same 180-degree RootQ/RootT forward correction. Only the standalone clip is included; source FBX and import tools remain under Library/CodexPortfolioReuse.

This is a general seated idle reused for the pilot, not a dedicated cockpit or enter/exit recording. SeatedCharacterPose samples it before applying the existing authored cockpit recline/limb fitting and hip-to-seat lock. SeatedAnimationPlayback owns the temporary manual graph and restores Animator flags on exit. The previous note about procedural prone remains current: the evaluated KayKit Crawling candidate was rejected after visual inspection and is not integrated.

## Main animation selection (2026-10-28)

UAL1 free Standard now supplies LocomotionIdle (Idle_Loop), LocomotionWalk (Walk_Loop), LocomotionSprint (Sprint_Loop), JumpStart (Jump_Start), JumpAir (Jump_Loop), JumpLand (Jump_Land). UAL2 free Standard supplies Slide_Start/Loop/Exit and ClimbUp_1m. They use the existing 180-degree Humanoid RootQ/RootT correction; loops are enabled only for idle/walk/sprint/air/slide loop. Existing Quaternius PunchJab/Cross replace the prior boxing recordings in MeleeCombat, including the reused pistol strike.

FistsGuard is a prepared derivative: PunchJab time0 held for0.4s, left arm/hand muscle values mirrored to the right, then Right Arm Down-Up=-0.09470423 and Front-Back=-0.5211526 for current first-person framing. It is not a separately recorded guard clip. Original source actions and timings remain documented above. UAL1 archive SHA256 cc73fc4e495b82958207316596317a3f40b9fa38065bde1027937452da537724; UAL2 SHA256 4008ea208a604773a2b2177d965f0f5d3195498b5bf838c3f5785d68e95f2a68. Full change/validation report: Docs/AI/QuaterniusMainAnimationChanges.md.
