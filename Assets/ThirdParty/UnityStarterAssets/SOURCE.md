# Unity Starter Assets Armature

Copyright Unity Technologies. Distributed under the Unity Companion License.

- Source asset: https://assetstore.unity.com/packages/3d/characters/first-person-third-person-character-controllers-196526
- Imported from the user's existing local `Starter Assets - ThirdPerson Updates in new CharacterController package.unitypackage` cache. This is the cached ThirdPerson package, not a claim that the latest combined package was downloaded.
- Original package `license.txt` is retained. License terms: https://unity.com/legal/licenses/unity-companion-license

The initial selective import included only the Armature model, Idle / Walk_N / Run_N / JumpStart / JumpLand / InAir clips, three URP materials and their nine textures are included. Original asset GUIDs are preserved.

The initial selective import omitted `OnFootstep` and `OnLand` events because the package audio/controller scripts were not included. A later user/Editor import restored these events in the legacy walk/run/jump clips and updated existing Armature metadata to package 2.0.2. Those newer files are preserved. The training-ground work does not implement character sounds or change event receivers.

Project-owned presentation lives in `Assets/Features/PlayerCharacter`: `PlayerCharacterAnimationView`, `Animation/PlayerCharacter.controller` and `Prefabs/PlayerCharacterArmature.prefab`. CharacterController remains authoritative; Animator root motion is disabled. The initial selective import excluded Starter Assets movement, input, Cinemachine, environment and demo scene files. The later user/Editor package import now supplies environment assets under `Assets/Starter Assets/Sample/Environment`. The training ground reuses that current path and original GUIDs directly; no duplicate environment asset or runtime controller is imported by the training-ground work. See `Docs/AI/InfantryTrainingGroundChanges.md`.
