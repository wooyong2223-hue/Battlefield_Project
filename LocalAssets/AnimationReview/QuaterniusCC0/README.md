# Quaternius CC0 animation review clips

Collected 2026-10-28. These nine standalone Humanoid clips are **candidates**, not approved Battlefield-equivalent motions. They are outside Unity Assets and are not connected to gameplay. No scene, controller, camera or movement settings were changed.

Sources: https://quaternius.itch.io/universal-animation-library and https://quaternius.itch.io/universal-animation-library-2 . Author Quaternius, CC0; see LICENSE.txt.

Extracted by Unity 6000.4.8f1 from each free Standard archive's Unity/UAL1_Standard.fbx or Unity/UAL2_Standard.fbx (non-RM source). RootQ/RootT receives the existing project's 180-degree Y forward correction. Muscle curves remain authored; only Slide_Loop/Swim loops use loop settings. Source gameplay/controller/model code was not imported.

The current Starter Assets Armature was sampled at 10%, 50%, 90% of each clip in an isolated Unity project with its runtime controller removed and the rig rebound before each clip. Humanoid mapping and finite bone positions passed. This does not test ground contact, military posture quality, weapon grips, paired actions or first-person framing. Preview.jpg contains midpoint samples without environment/equipment fitting.

Source archive hashes:
- UniversalAnimationLibrary-Standard.zip: cc73fc4e495b82958207316596317a3f40b9fa38065bde1027937452da537724
- UniversalAnimationLibrary2-Standard.zip: 4008ea208a604773a2b2177d965f0f5d3195498b5bf838c3f5785d68e95f2a68

| File | Source | Seconds |
|---|---|---|
| Animations/Interact.anim (+ .meta) | QuaterniusUAL1: Interact | 2.000000 |
| Animations/Swim_Idle_Loop.anim (+ .meta) | QuaterniusUAL1: Swim_Idle_Loop | 3.333333 |
| Animations/Roll.anim (+ .meta) | QuaterniusUAL1: Roll | 1.466667 |
| Animations/Swim_Fwd_Loop.anim (+ .meta) | QuaterniusUAL1: Swim_Fwd_Loop | 1.333333 |
| Animations/OverhandThrow.anim (+ .meta) | QuaterniusUAL2: OverhandThrow | 1.333333 |
| Animations/Slide_Start.anim (+ .meta) | QuaterniusUAL2: Slide_Start | 0.833333 |
| Animations/Slide_Exit.anim (+ .meta) | QuaterniusUAL2: Slide_Exit | 0.500000 |
| Animations/ClimbUp_1m.anim (+ .meta) | QuaterniusUAL2: ClimbUp_1m | 0.666667 |
| Animations/Slide_Loop.anim (+ .meta) | QuaterniusUAL2: Slide_Loop | 2.000000 |

Slide poses need military weapon fitting; ClimbUp_1m is not a complete vault set; OverhandThrow is a generic throw without grenade pin handling; Interact is not a dedicated door push; Roll and swimming still need contextual visual verification. Do not copy these into the live Animator until a specific motion is selected.
