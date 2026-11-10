# Human Soldier Animations FREE — local review copy

Author Kevin Iglesias. Official source: https://kevdev.itch.io/human-soldier-animations-free
Current author terms: https://www.keviniglesias.com/ (LICENSE); local HTML snapshot in kevin-license-page.html.
This is not CC0. This entire folder is deliberately Git-ignored because the source files are subject to redistribution restrictions. Use the author's page to obtain your own copy. Runtime use in a substantial project is permitted by the author's published terms; standalone source sharing is restricted. No purchase, account login or signup was performed.

HumanSoldierAnimationsFREE-15306397.zip is the original FBX distribution. HumanSoldierAnimationsFREE-18766376.unitypackage is the original Unity distribution (gzip tar, not ZIP). FBX/ preserves all 135 FBX files: 120 male/female animation files, 8 masked pose files, 7 reference models. The advertised 128 animation files include gender variants, root-motion variants and masked poses; they are not 128 distinct gameplay actions.

The isolated project imported only 60 male action/movement files, 3 male weapon-hold poses and the required male avatar model. All 63 clips were Humanoid-retarget sampled at 10%, 50%, 90% on the existing player Armature. Female variants and two generic hand poses were inventoried but not retarget tested. Six representative midpoint images were inspected. The player Scene and Animator Controller were not changed.

The pack includes 8-direction walk/run, assault-rifle/rifle/handgun/bazooka aim/shoot/reload, a left-hand grenade throw, damage and death. Dedicated first-person weapon arms, prone, sliding, crouch locomotion, drag/revive and vehicle entry/exit are not provided by these selected free files. Weapon holds are separate poses; do not assume a complete layered military controller or fitted gun from file availability alone.

All filenames, bytes and archive hashes are listed in Docs/AI/Battlefield6AnimationAcquisitionManifest.csv. Clip-level status is in Docs/AI/Battlefield6AnimationClips.csv. This folder is not imported into the product Unity Assets.
