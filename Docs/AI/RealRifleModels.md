# 실사형 무료 소총 모델 적용 (2026-10-30)

## 선택과 적용 범위

사용자가 실사형·PBR 재질을 우선하도록 선택했다. 3DModelsCC0의 CC0 배포 원본에서 M4A1과 AK-47을 확보했다. M4A1은 기존 슬롯3의 기본 소총 모델로 연결하고 AK-47은 교체 가능한 프리팹 및 무기 데이터로 준비했다. 새로운 입력 슬롯이나 씬 오브젝트는 추가하지 않았다.

| 모델 | 출처 | 임포트 메시 삼각형 수 | 텍스처 | 상태 |
|---|---|---:|---|---|
| M4A1 | https://opengameart.org/content/m4a1-assault-rifle (3DModelsCC0 원본, nisu 미러) | 9246 | 2048² BaseColor, Normal, Metallic, Roughness, Height | 현재 슬롯3 기본 모델 |
| AK-47 | https://3dmodelscc0.itch.io/free-cc0-guns-explosives-pack | 4805 | 2048² BaseColor, DirectX Normal, Metallic, Roughness, Height | 프리팹·정의 및 양 시점 런타임 검증 준비 완료 |

제작자 배포 페이지가 CC0를 명시한다. 원본 FBX/PNG 해시와 파생 데이터 변환 내역은 `Assets/ThirdParty/3DModelsCC0/Rifles/SOURCE.md`에 기록했다. SCAR는 이번 적용 대상으로 확정하지 않았다. 무료 페이지 존재만으로 원본 재배포 가능한 PBR 모델이라고 간주하지 않았다.

## 기존 작업과 비교 / 변경된 기존 에셋

이전 작업 적용 목록54개 파일의 SHA256이 그대로임을 작업 전 확인했다. 적용 전 다시 비교하며 사용자/에디터 변경이 발견되면 덮어쓰지 않는다.

`Assets/Features/Weapon/Definitions/AssaultRifle.asset`에서 `_modelPrefab` 참조만 교체한다.

```yaml
# 이전: 단색 재질 Quaternius PrototypeRifle
_modelPrefab: {fileID: 8733604918273818375, guid: 1f4540a733be39341a2a3a1556875ee4, type: 3}
# 이후: URP PBR M4A1
_modelPrefab: {fileID: 5533685770023181170, guid: 6a12457b5ea24d04a97c4457860ea5b0, type: 3}
```

기존 PrototypeRifle 프리팹/메시/재질은 삭제하지 않는다. 교체된 것은 현재 무기의 모델 참조 한 개다. 기본 HUD 이름 `ASSAULT RIFLE`도 보존한다. 명시적 M4A1 정의를 쓰면 이름은 M4A1으로 표시된다.

기존 탄창30/예비탄180/초당10발/피해10/장전1.6초, 그립 및1인칭 보정, ADS `(0,-.16834281,.45)`를 유지한다. 기존 런타임 C# 스크립트·입력·SampleScene·카메라·이동/상체/애니메이션·오디오·패키지는 수정하지 않는다.

## 신규 에셋의 구체적 내용

- `Assets/Features/Weapon/Prefabs/M4A1.prefab`, `AK47.prefab`: 원본 메시 부품 구조를 유지한다. FBX의 -Z 총열을 +Z로 돌리고 전체 길이를 기존 소총과 같은1.8 월드 단위로 맞춘다. 기존4m 캐릭터에서 비슷한 상대 크기를 유지하기 위한 프로젝트용 프리팹 설정이다. 원본 FBX 자체는 변경하지 않는다. 별도 충돌/자동 발사 컴포넌트를 붙이지 않는다.
- M4A1 Geometry 배율2.095842, AK47 배율2.053555. 방아쇠 기준점 뒤/아래에 손목 기준을 두고 Muzzle 및 SupportHandGrip을 연결한다. 정확한 위치/회전은 프리팹 직렬화 값에 기록된다. 지지 손 회전은 기존 소총의 검증된 회전을 재사용한다.
- `Assets/Features/Weapon/Definitions/M4A1.asset`, `AK47.asset`: 기존 소총 정의를 복사해 각각의 모델과 표시 이름만 바꾼다. 두 정의는 기존 외부 소총 모션 오버라이드를 그대로 사용한다. 성능 수치는 임시 공통값이며 실제 총기 성능을 재현한 것이 아니다.
- `Assets/Features/Weapon/Models/RealRifles/M4A1/M4A1_PBR.mat`, `AK47/AK47_PBR.mat`: URP Lit. BaseMap은 원본 Base_Color, BumpMap은 노멀, MetallicGlossMap은 변환한 MetallicSmoothness이다. `_NORMALMAP`, `_METALLICSPECGLOSSMAP` 활성화, Metallic/Smoothness 값1.
- 각 모델 폴더의 `MetallicSmoothness.png`: 원본 Metallic을 R, `1-Roughness`를 A로 패킹한2048² RGBA. G/B=0. 선형 색공간으로 임포트한다. 색상 맵만 sRGB, 노멀 맵은 NormalMap 임포터를 사용한다.
- `Assets/ThirdParty/3DModelsCC0/Rifles/M4A1/`: 원본 FBX와5종 PNG를 그대로 보관한다.
- `Assets/ThirdParty/3DModelsCC0/Rifles/AK47/`: 원본 FBX와5종 PNG를 그대로 보관한다. 추가 `AK47_Normal_Unity.png`는 원본 DirectX 노멀맵의 G 채널만 반전한 파생본이다. 원본은 남겨 둔다.
- 두 Height 맵은 원본 데이터로 보관하며 현재 재질의 패럴랙스에는 사용하지 않는다.
- `Assets/ThirdParty/3DModelsCC0/Rifles/SOURCE.md`: 배포·CC0 링크, 수집 방법, 원본 및 파생 텍스처 SHA256/변환 기록.
- `Assets/Tests/PlayMode/RealRifleModelTests.cs`: PBR 텍스처/키워드/임포트 색공간, 분리 탄창/총구/충돌 없음, 기본 모델 참조, 두 모델의 양 시점 파지·상하 조준·장전을 검사한다. 테스트에서만 로드아웃 내부 정의를 바꿔 두 모델을 같은 캐릭터로 검증한다. 생산 코드에 시험용 선택 API를 추가하지 않는다.
- 신규 에셋/테스트/폴더의 .meta를 함께 추가한다. 기존 .meta는 변경하지 않는다. 다운로드 ZIP/RAR, 생성용 Editor 도구, 임시 캡처 테스트, 로그·캡처는 Library에만 둔다.
- `Docs/AI/RealRifleModels.md`: 이 보고서. `Docs/AI/UnityProjectContext.md`: 현재 모델과 검증 상태 추가.

## 사용 방법

현재 SampleScene의 기존 슬롯3은 M4A1을 표시한다. AK-47을 직접 시험하려면 PlayerCharacter의 PlayerCharacterLoadout → Weapons → Slot3 항목의 Definition에 `Assets/Features/Weapon/Definitions/AK47.asset`을 연결한다. 모델·파지점·총구·표시 이름·장전 모션은 기존 구조를 통해 선택 정의에 맞춰 연결된다. 1번 권총과2번 맨손은 유지된다.

## 검증과 남은 문제

Unity6000.4.8f1 격리 프로젝트에서 `-batchmode -noaudio`로 검사했다. 사용자 Editor 오디오와 게임 오디오 설정은 변경하지 않았다. 첫 집중검사10/10 통과 후1·3인칭 파지/ADS/장전 전·중·후 캡처를 확인했다. 기존 사격/더미 팀 피해/탄약/장전/권총/상체 안정화 검사도 포함했다. 최종 release.xml 10/10 통과(제품 테스트9 + 임시 캡처1). 검사한54개 런타임 데이터/테스트/에셋/메타를 동일 바이트로 적용했다. 빌드 및 전체 테스트 재실행은 하지 않았다.

실사형의 얇은 리시버가 기존1인칭 오른팔 포즈의 시야 가림을 드러낸다. ADS 표시 위치만 조정하는 시도는 이를 해결하지 못했으므로 해당 임시 보정은 폐기하고 원래 ADS 값을 유지했다. **ADS 팔 가림은 이번 모델 교체로 해결됐다고 주장하지 않는다.** 모델을 먼저 적용하라는 요청에 맞춰 상체/손 동작 수정은 후속 작업으로 남긴다. 분리된 탄창 메시가 있다고 해서 탄창 탈착 애니메이션이 연결된 것은 아니다.

설계는 기존 모델/무기 정의의 데이터 교환 지점을 사용한다. 새 관리자/상속 계층/런타임 컴포넌트 없이 모델별 차이를 프리팹과 정의에 넣었다. 기존 책임·캡슐화·의존성 경계를 확장할 필요가 없었으며 의도적인 OOP/SOLID 예외는 없다. Editor에서 실제 조작/시야/재질의 주관적 품질 확인이 남아 있다.

## 변경 파일 전체

기존 에셋1개 수정, 신규 에셋·테스트·메타53개 추가, 보고서 추가 및 컨텍스트 문서 갱신을 합쳐56개 파일이다. 신규GUID30개의 중복 및 기존 GUID 충돌이 없음을 확인했다.

- `Assets/Features/Weapon/Definitions/AK47.asset`
- `Assets/Features/Weapon/Definitions/AK47.asset.meta`
- `Assets/Features/Weapon/Definitions/AssaultRifle.asset`
- `Assets/Features/Weapon/Definitions/M4A1.asset`
- `Assets/Features/Weapon/Definitions/M4A1.asset.meta`
- `Assets/Features/Weapon/Models/RealRifles.meta`
- `Assets/Features/Weapon/Models/RealRifles/AK47.meta`
- `Assets/Features/Weapon/Models/RealRifles/AK47/AK47_PBR.mat`
- `Assets/Features/Weapon/Models/RealRifles/AK47/AK47_PBR.mat.meta`
- `Assets/Features/Weapon/Models/RealRifles/AK47/MetallicSmoothness.png`
- `Assets/Features/Weapon/Models/RealRifles/AK47/MetallicSmoothness.png.meta`
- `Assets/Features/Weapon/Models/RealRifles/M4A1.meta`
- `Assets/Features/Weapon/Models/RealRifles/M4A1/M4A1_PBR.mat`
- `Assets/Features/Weapon/Models/RealRifles/M4A1/M4A1_PBR.mat.meta`
- `Assets/Features/Weapon/Models/RealRifles/M4A1/MetallicSmoothness.png`
- `Assets/Features/Weapon/Models/RealRifles/M4A1/MetallicSmoothness.png.meta`
- `Assets/Features/Weapon/Prefabs/AK47.prefab`
- `Assets/Features/Weapon/Prefabs/AK47.prefab.meta`
- `Assets/Features/Weapon/Prefabs/M4A1.prefab`
- `Assets/Features/Weapon/Prefabs/M4A1.prefab.meta`
- `Assets/Tests/PlayMode/RealRifleModelTests.cs`
- `Assets/Tests/PlayMode/RealRifleModelTests.cs.meta`
- `Assets/ThirdParty/3DModelsCC0.meta`
- `Assets/ThirdParty/3DModelsCC0/Rifles.meta`
- `Assets/ThirdParty/3DModelsCC0/Rifles/AK47.meta`
- `Assets/ThirdParty/3DModelsCC0/Rifles/AK47/AK47.fbx`
- `Assets/ThirdParty/3DModelsCC0/Rifles/AK47/AK47.fbx.meta`
- `Assets/ThirdParty/3DModelsCC0/Rifles/AK47/AK47_Base_Color.png`
- `Assets/ThirdParty/3DModelsCC0/Rifles/AK47/AK47_Base_Color.png.meta`
- `Assets/ThirdParty/3DModelsCC0/Rifles/AK47/AK47_Height.png`
- `Assets/ThirdParty/3DModelsCC0/Rifles/AK47/AK47_Height.png.meta`
- `Assets/ThirdParty/3DModelsCC0/Rifles/AK47/AK47_Metallic.png`
- `Assets/ThirdParty/3DModelsCC0/Rifles/AK47/AK47_Metallic.png.meta`
- `Assets/ThirdParty/3DModelsCC0/Rifles/AK47/AK47_Normal_DirectX.png`
- `Assets/ThirdParty/3DModelsCC0/Rifles/AK47/AK47_Normal_DirectX.png.meta`
- `Assets/ThirdParty/3DModelsCC0/Rifles/AK47/AK47_Normal_Unity.png`
- `Assets/ThirdParty/3DModelsCC0/Rifles/AK47/AK47_Normal_Unity.png.meta`
- `Assets/ThirdParty/3DModelsCC0/Rifles/AK47/AK47_Roughness.png`
- `Assets/ThirdParty/3DModelsCC0/Rifles/AK47/AK47_Roughness.png.meta`
- `Assets/ThirdParty/3DModelsCC0/Rifles/M4A1.meta`
- `Assets/ThirdParty/3DModelsCC0/Rifles/M4A1/M4A1.fbx`
- `Assets/ThirdParty/3DModelsCC0/Rifles/M4A1/M4A1.fbx.meta`
- `Assets/ThirdParty/3DModelsCC0/Rifles/M4A1/M4A1_Base_Color.png`
- `Assets/ThirdParty/3DModelsCC0/Rifles/M4A1/M4A1_Base_Color.png.meta`
- `Assets/ThirdParty/3DModelsCC0/Rifles/M4A1/M4A1_Height.png`
- `Assets/ThirdParty/3DModelsCC0/Rifles/M4A1/M4A1_Height.png.meta`
- `Assets/ThirdParty/3DModelsCC0/Rifles/M4A1/M4A1_Metallic.png`
- `Assets/ThirdParty/3DModelsCC0/Rifles/M4A1/M4A1_Metallic.png.meta`
- `Assets/ThirdParty/3DModelsCC0/Rifles/M4A1/M4A1_Normal.png`
- `Assets/ThirdParty/3DModelsCC0/Rifles/M4A1/M4A1_Normal.png.meta`
- `Assets/ThirdParty/3DModelsCC0/Rifles/M4A1/M4A1_Roughness.png`
- `Assets/ThirdParty/3DModelsCC0/Rifles/M4A1/M4A1_Roughness.png.meta`
- `Assets/ThirdParty/3DModelsCC0/Rifles/SOURCE.md`
- `Assets/ThirdParty/3DModelsCC0/Rifles/SOURCE.md.meta`
- `Docs/AI/RealRifleModels.md`
- `Docs/AI/UnityProjectContext.md`
