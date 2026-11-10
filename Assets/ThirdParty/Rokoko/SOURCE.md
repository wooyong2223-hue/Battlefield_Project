# Selected Rokoko boxing motions

Official resource: https://www.rokoko.com/resources/rokoko-mocap-13-free-fight-animations
Creator: Rokoko; the resource page credits capture to Sam Lazarus.
The creator explicitly permits use in personal and commercial animation, VFX, game and 3D-art projects. These motions are not labeled CC0. No signup or marketing consent was submitted by this task; the download link is publicly provided by the resource page.

Official page -> public Google Drive folder Combat_LetsGetPhysical_11Assets -> Combat.zip.
Archive SHA256: 380c9854b4ac21de8a2ee436ae3d2bbef9afddb6af3bc97345eeb49b93de7cf8
Selected source: Combat/Boxing_mixamo.fbx. Actual Unity import reports 24 fps and 17.875 seconds. The downloaded archive contains eleven FBX files, although the resource page is titled thirteen animations.

Imported as Humanoid, Create From This Model, no materials, Unity 6000.4.8f1. Only the following standalone Humanoid clips are included in product Assets; the full source FBX/archive and analysis tools remain under Library/CodexMeleeClips.

- BoxingGuard.anim: source 11.0–11.4 seconds; 0.4-second loop with the last 0.15 seconds blending back to its start.
- BoxingJab.anim: source 53/24–65/24 seconds (2.208333–2.708333); captured left jab plus a return blend to the guard pose, total 0.8 seconds.
- BoxingCross.anim: source 237/24–245/24 seconds (9.875–10.208333); captured right straight plus a return blend to the guard pose, total 0.8 seconds. The later second punch in the original recording is excluded.

The action peak is approximately 0.208 seconds from clip start. Gameplay still owns the existing 0.2-second hit windup and 0.6-second recovery. Animation does not move the character root or authorize hits.

Finger pose: the selected recording's open/relaxed fingers are replaced with forty constant LeftHand/RightHand muscle curves sampled at 0.2 seconds from the project's existing Quaternius PunchJab.anim. Quaternius material is CC0; see ../Quaternius/License.txt and ../Quaternius/SOURCE.md. Arm and torso motion comes from Rokoko. The resulting clips are prepared derivatives, not unchanged original recordings.

PistolMelee reuses BoxingCross with the existing pistol attached to the right hand. It is a right-hand strike while holding the pistol, not a dedicated pistol-whip source animation.

## Runtime selection update (2026-10-28)

The provenance above describes the earlier integration. The current PlayerCharacter Controller no longer references BoxingGuard/Jab/Cross; Quaternius is the main animation source. These files remain preserved as legacy assets, not newly selected runtime motions. See Docs/AI/QuaterniusMainAnimationChanges.md.
