# Unity project context

Verified 2026-09-23 for fighter audio integration.

- Unity 6000.4.8f1, URP 17.4.0, Input System 1.19.0.
- First-party assemblies: Battlefield.Features depends on Battlefield.Framework; framework EditMode tests exist. No fighter audio PlayMode harness was found.
- SampleScene and CombatTestScene use Assets/Features/Fighter/Prefabs/Fighter.prefab. Each scene has an AudioListener.
- JetMovement owns speed and ticks Afterburner; Afterburner.IsActive is authoritative, including charge depletion. KeyboardJetInput binds afterburner to Left Shift.
- Health and FighterDestruction control destruction; destruction disables movement. JetEngineVfx handles visual presentation separately.
- FighterEngineAudio is a separate presentation component. It reads state without modifying flight, charge, input, or damage behavior. It owns two serialized AudioSources on the fighter root; no new hierarchy objects.

## Temporary audio integration

Assets/Features/Fighter/Audio/EngineLoop_Temporary.wav and AfterburnerLoop_Temporary.wav are byte-for-byte copies of the previously processed Focus5/01_Engine/Natural_Loop.wav and Focus5/02_Afterburner/Natural_Loop.wav recordings in the local audio workspace. They are prototype assets, not original isolated game source recordings.

Both clips loop. Engine volume follows speed from 0.2 to 0.5; pitch follows speed from 0.9 to 1.1. Afterburner volume is 0.5 and pitch is 1. Crossfade duration is 0.2 seconds. Both recordings contain engine sound, so their volumes crossfade instead of playing at full volume together.

AudioSources use full 3D blend, logarithmic attenuation, minimum distance 50, maximum distance 1500, and Doppler 0. Play On Awake is off. WAV import uses PCM, decompression on load, preloading, and preserved sample rate/channels. Pause at timeScale zero pauses playback; destruction, disabled movement, and component disable stop it.

The component directly references existing concrete movement/health components because there is one state implementation. No speculative interfaces or inheritance were introduced. State remains encapsulated; audio has a single presentation responsibility.

## Editor validation remaining

Open SampleScene, allow import and compilation, then Play. Confirm normal engine playback, speed response, Left Shift crossfade, return on release/charge depletion, pause/resume, and stop on destruction. Listen from first/third person for volume and loop seams. Check Console and both clip references. Compiler and serialized-reference checks alone do not establish audible quality or successful Editor import.

## Minigun audio addition

Selected Minigun_Sustain_Loop.wav from the user-provided 2026-09-23 video extraction (12.00-15.90 seconds, gently suppressed background, 40ms loop crossfade). Copied as MinigunLoop_Temporary.wav.
WeaponBase raises Fired after a successful shot and exposes ShotInterval. MachineGunAudio owns playback separately, subscribes to the gun's Fired event, and fades out after firing ceases. Its optional Health reference stops audio on death; it does not require fighter input or weapon selection. Overheat silence follows at most 1.3 shot intervals plus fade. Pause, disable, and destruction stop or pause playback.
Prefab adds one component and one AudioSource, volume 0.65, fade 0.05 seconds, full 3D, distance 50-1500, Doppler 0. Existing engine audio and all flight/weapon tuning remain intact. Scene bullet-pool overrides are preserved. No existing methods, references, or assets removed. Concrete state dependencies follow the existing component architecture; no speculative interfaces introduced.
Validation: original prefab blocks preserved, unique IDs and references checked, scene bullet-pool overrides present. Unity Play Mode/audio listening still required: fire/release, overheat, weapon selection, pause, destruction, loop seam and engine balance.

## User-selected engine and missile launch

Engine source reference changed from EngineLoop_Temporary.wav to JetEngineLoop.wav, a byte-for-byte copy of sound_folder_v3/wav/Jet_Engine_Loop.wav. Old clip retained; existing mix and afterburner unchanged.
MissileLaunch_Original.mp3 is a byte-for-byte copy of user-selected CAS missile launching 66630; no audio editing. AudioSource added to the existing missile fire-point object, volume 0.65, loop off, 3D, distance 50-1500, Doppler 0.
MissileLaunchAudio subscribes to the launcher's successful Fired event and plays one shot, handling pause/disable/death. Its optional Health reference preserves the fighter death behavior without requiring fighter-specific code. Ammo, lock, damage and projectile behavior remain unchanged.
Compiler passed; prefab original blocks preserved except the engine reference and added audio components. Unity import and Play Mode listening still required, including unsuccessful trigger, successful launch, pause and destruction. Flight audio candidates are outside Assets and not wired yet.

## Missile flight loop (2026-09-25)

User approved frozen_point/Frozen_Point_Loop.wav, copied unchanged to Projectile/Audio/MissileFlightLoop.wav (8 seconds). HomingMissile.prefab adds MissileFlightAudio and one full-3D AudioSource: volume 0.4, loop on, play-on-awake off, distances 50-1500, Doppler 0. Audio follows the missile transform. Deferred LateUpdate start avoids prewarm/old pool position playback. OnDisable stops on impact/lifetime return; timeScale zero pauses, resume continues. No projectile gameplay modifications or removed implementations. Presentation is a separate component with an explicit source reference and private lifecycle state.

## Audio ownership

FighterEngineAudio and its engine clips remain under Fighter. MachineGunAudio and MissileLaunchAudio live under Weapon/Scrips, and their clips under Weapon/Audio. MissileFlightAudio and its clip live under Projectile. Existing script and clip GUIDs are preserved so prefab references survive the moves. Weapon audio depends on the reusable WeaponBase.Fired event rather than fighter input or selection; optional Health references preserve death-stop behavior on the fighter prefab.
Prefab pre-existing blocks and clip identity validated; Unity Play Mode verification remains: launch, moving spatial sound, pause/resume, collision/expiry stop, pool reuse, concurrent missiles and mixing with launch sound.
