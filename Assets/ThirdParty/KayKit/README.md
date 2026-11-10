# KayKit Character Animations 1.1 — selected locomotion clips

Author: Kay Lousberg (KayKit).
Official source: https://kaylousberg.itch.io/kaykit-character-animations
License: CC0; see LICENSE.txt (original archive notice).

Downloaded free archive: KayKit-Character-Animations-Free.zip
SHA256: 65882f31f905ad2e953819648a59287cdeab8f623908d5ef701971d3758be20f
Source FBX: KayKit_Character_Animations_1.1/Animations/fbx/Rig_Medium/Rig_Medium_MovementAdvanced.fbx

Only three Humanoid clips were extracted using Unity 6000.4.8f1:
- Walking_Backwards -> Animations/WalkBackward.anim (1.066667 s)
- Running_Strafe_Left -> Animations/StrafeLeft.anim (0.8 s)
- Running_Strafe_Right -> Animations/StrafeRight.anim (0.8 s)

Importer: Humanoid, Create From This Model, no materials. Extracted clips use Loop Time/Loop Pose, preserve original root orientation/position, and blend loop orientation/XZ. No 180-degree root correction was needed. Root motion stays disabled on the player Animator; CharacterController owns displacement.

Standing locomotion uses existing Starter Assets forward walk/run. The side clips play at 0.75x in the walk direction tree and 1x in the run direction tree; backward plays at 1x/1.3333333x respectively. These are retargeted test motions, not Battlefield assets. They do not provide dedicated military starts/stops, crouch strafes, turn-in-place, or foot planting.
