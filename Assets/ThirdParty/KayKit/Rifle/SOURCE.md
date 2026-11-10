# KayKit rifle animation source

KayKit Character Animations 1.1 by Kay Lousberg. Official source: https://kaylousberg.itch.io/kaykit-character-animations. CC0; see ../LICENSE.txt.
Original archive: KayKit-Character-Animations-Free.zip; file KayKit_Character_Animations_1.1/Animations/fbx/Rig_Medium/Rig_Medium_CombatRanged.fbx, copied here as CombatRanged.fbx. SHA256: b70e1d5ef64fafaad5c50ca2225c238f33ec9622876014f38478deeb10ee2ba4.

Humanoid mapping is built from the included T-Pose, including Chest and wrist/limb mappings. Source keyframes are unchanged. Imported clips: T-Pose, Ranged_2H_Aiming, Ranged_2H_Shoot, Ranged_2H_Reload.

Project derivatives RifleHold.anim (1.6s), RifleShoot.anim (1.066667s), RifleReload.anim (1.6s) copy those three action clips. Hold loops. Animation events are removed from derivatives only. The source has no finger bones, so right-hand finger channels are reused from the existing CC0 Quaternius PistolHoldNeutral.anim and left-hand finger channels from CC0 Quaternius FistsGuard.anim. These correspond to the project's existing Quaternius UAL sources; see ../../Quaternius provenance/license files. No arm, spine, root or hand-IK transform curves are copied from the finger sources.

This is a generic third-person two-handed weapon action retarget, not a dedicated first-person weapon/animated detachable magazine rig. Model-specific grip offsets and first-person presentation are project-side calibration.
