# Realistic rifle models

Publisher: 3DModelsCC0. License: CC0 1.0 Universal. Retrieved 2026-10-30.
Author/publisher grant: https://3dmodelscc0.itch.io/free-cc0-guns-explosives-pack
M4A1 mirror by nisu: https://opengameart.org/content/m4a1-assault-rifle
CC0 reference: https://creativecommons.org/publicdomain/zero/1.0/

M4A1 was downloaded as the original m4a1_0.zip from OpenGameArt. AK47 was extracted from the publisher archive Guns&Explosives.rar (only the AK-47 FBX and PNG files were selected). Original FBX/PNG bytes are preserved. The archive and downloader tools remain outside Assets in Library/CodexRealRifles.

Project derivatives: AK47_Normal_Unity.png inverts only the green channel of AK47_Normal_DirectX.png for Unity tangent-space normal convention. Generated MetallicSmoothness.png under Features/Weapon/Models/RealRifles/<weapon> packs source metallic into R and (1 - source roughness) into A; G/B are zero, imported in linear color space. Original base color uses sRGB, normal maps use TextureImporter NormalMap. URP Lit materials connect base color, packed metallic/smoothness and normal map. Height maps are retained as original source data but not enabled as parallax.

Prefabs preserve separate magazine/charging/selector parts, rotate the source barrel direction -Z to +Z, normalize total length to1.8 world units for the existing4m character, and author Muzzle and SupportHandGrip transforms. No external runtime scripts, animations or Unity packages are imported. Models have no authored detachable-magazine animation connected yet.

M4A1 is the default model referenced by the existing AssaultRifle.asset. M4A1.asset and AK47.asset are prepared alternative definitions; no new input slots or scene objects are introduced.

## SHA256 (original files and normal derivative)

- M4A1/M4A1.fbx: `d66516a3455b975027556394b9afb92a1e9093d700ccf779aab223cd6ac4076e`
- M4A1/M4A1_Base_Color.png: `e28fb02a6a1d951ec2d356fad8f267ba04da7d294bfa001eb3995549971e1b34`
- M4A1/M4A1_Height.png: `df12e21e2c5a8553bb7b5e9bee80477a123f3c5cda5cf55d50d739f69a51b904`
- M4A1/M4A1_Metallic.png: `e9cc71ff7886c552030586c650e67c78a86c8b2a812357e40edc19d5bc9426ca`
- M4A1/M4A1_Normal.png: `9da0b6da232cc676482eeb2b21fdfe04f03504a0e37407f3bbc35556335bd998`
- M4A1/M4A1_Roughness.png: `708eff9df647f0ecf258467e983052e164286d1dc579e763a6d8261074d3eb9a`
- AK47/AK47.fbx: `d73bbbaa153d49f67a8180d0197c14aaab925540db231db00e3461d74a56246e`
- AK47/AK47_Base_Color.png: `6a4ba8b206ac58ede07f5826a20791d52a0121b1f8d2d517a551805148ac8e62`
- AK47/AK47_Height.png: `e6c1d75b693410e54e82d1003772905f84987f74d6910720905442dbd8bdfb6d`
- AK47/AK47_Metallic.png: `55b80ee2cec9d6933c22c16807f87365f75ab96b1a9bc72a11bcd8b10bb67c06`
- AK47/AK47_Normal_DirectX.png: `52eaa62b293d2c9546c69d48d16e0c85d78c1b84e6ef0592b6fc598679c47144`
- AK47/AK47_Normal_Unity.png: `5720ca214da94d6f7838f49a936987edada13b2e422e42f2d568fea466ea9c48`
- AK47/AK47_Roughness.png: `ebea10558d2b63b0aabd24efad8da8f30316f8662bb9a2de0e01f6b0105034b4`
