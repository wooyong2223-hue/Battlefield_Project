# 무기 슬롯·장착 오브젝트 및 ADS 팔 보정 (2026-10-30)

## 결과와 사용법

사용자가 번호를 소총1 / 권총2 / 맨손3으로 변경하고 AK-47을 기본 장착하도록 요청했다. ‘3번’은 맨손임을 사용자 응답으로 확인했다.

```text
PlayerCharacter
├─ Equipment                       (신규, 로컬 위치0 / 회전0 / 배율1)
│  ├─ Rifle                        (신규, PlayerCharacterWeaponEquipment)
│  │  └─ Definition = AK47.asset   ← M4A1.asset으로 교체 가능
│  └─ Pistol                       (신규, PlayerCharacterWeaponEquipment)
│     └─ Definition = Pistol.asset / Existing Magazine = 기존 PersonalCombat의 탄창
└─ PersonalCombat                  (기존 구조 그대로 유지)
```

Rifle의 Definition은 에디터 설정과 Play 중 모두 교체할 수 있다. 런타임 변경은 `TryEquip(InfantryWeaponDefinition)` 또는 Inspector Definition 변경으로 수행한다. 종류별 탄약은 각 장착 컴포넌트가 보존하고, 종류/슬롯 변경은 진행 중인 장전만 취소한다. 부활은 보유 종류별 탄약을 초기화하고 슬롯1을 선택한다. 선택된 종류는 사용자가 마지막으로 설정한 값으로 유지한다. 시작 설정은 AK-47이다.

장착 오브젝트는 설정/탄약 수명의 소유자다. 실제 표시 모델은 기존 CombatView가 선택한 정의에서 생성하고 오른손 및 1인칭 리그 아래에 부착한다. 새 장착 오브젝트에 고정된 복제 메시를 중복 생성하지 않는다.

## 작업 전 비교와 교체 계획

이전 두 작업의 적용 파일을 합친 최신 SHA256과 비교해 새 사용자/Unity 변경이 없음을 확인했다. 수정 대상 테스트 파일도 별도로 기준 해시를 기록했다. 제품 적용 전 같은 기준을 다시 비교하며 차이가 있으면 덮어쓰지 않는다.

기존 Loadout은 슬롯1=권총 / 슬롯2=맨손 / 슬롯3=소총, Entry 안에 `_definition`과 `_existingMagazine`을 직접 보관했다. 내부 Item과 Update가 종류별 탄약 상태를 생성/진행했다. 이를 슬롯 선택과 장착 객체의 책임으로 분리한다고 사용자에게 설명한 후 변경했다.

ADS의 기존 자세는 외부 소총 모션을 손 기준으로 화면에 복사하면서 실제 오른쪽 어깨가 시야 왼쪽으로 넘어가고 팔꿈치가 카메라 바로 앞을 통과했다. 변경 전 AK-47 ADS 좌표는 어깨/상완(-.24,.03,.46), 팔꿈치(.11,.07,.17), 손(0,-.17,.45)였다. 메시를 숨기거나 카메라/모션 클립을 바꾸는 대신 1인칭 복사 리그만 리타깃하도록 설명한 후 변경했다.

## 변경한 런타임 코드

| 파일 (Assets/Features/PlayerCharacter/Scripts/) | 구체적 변경 / 이유 |
|---|---|
| PlayerCharacterWeaponEquipment.cs (신규) | `_definition`, `_existingMagazine` 참조 및 정의별 WeaponMagazineState 사전을 소유한다. Definition/Magazine 읽기, TryEquip, ResetAmmo, Changed 이벤트 제공. Inspector 변경도 Update에서 같은 취소/전환 경로를 거친다. 권총은 기존 WeaponMagazine을 사용하고 소총 종류별 상태는 각자 유지한다. |
| PlayerCharacterLoadout.cs | RifleSlot=1/PistolSlot=2/UnarmedSlot=3 상수 추가. Entry `_definition`/`_existingMagazine`을 `_equipment` 참조로 교체. 내부 Item/탄약 생성·Tick 구현을 제거하고 Dictionary<int, PlayerCharacterWeaponEquipment>로 선택만 관리한다. GetEquipment 제공. 장착 객체의 Changed를 현재 슬롯에 대해서만 전달한다. 활성화/비활성화에 구독/해제. 기본/부활 슬롯은 소총1로 변경. |
| PlayerCharacterInput.cs | SelectRifle→1, SelectPistol→2, SelectUnarmed→3 반환. 액션 이름/ID는 유지하며 슬롯 의미는 Loadout 상수를 사용한다. |
| PlayerCharacterController.cs | 로드아웃 없는 레거시 폴백의 권총/맨손 번호를2/3으로 변경. OnEnable에서 현재 장비 알림을 보내 비활성화 중 변경한 장비를 재활성화 시 표시/총구에 반영한다. 기존 발사체/피해/가림 검사와 무기 성능은 유지한다. |
| PlayerCharacterCombatView.cs | 장비 선택 알림에서 AttachWeapon(false)로 원본 손에 우선 연결하고 LateUpdate에서 화면용 손으로 옮긴다. 아직 갱신되지 않은 1인칭 복사 손에 붙어 재활성화 직후 첫 발이 잘못된 위치에서 나가는 것을 방지한다. 모델 localScale은 이전 프레임의 복사 손 배율 대신 안정된 원본 손 배율의 역수로 정규화한다. 1인칭 전용 배율은 복사 손이 상속하므로 정의의 표시 배율은 유지한다. |
| FirstPersonArmsView.cs | 신규 `_adsShoulder=(.45,-.4,-.1)`, `_adsElbowHint=(.65,-.6,.15)` 설정. 두 FirstPersonArmPose를 초기화 때 바인딩하고 원본 복사 뒤 custom rifle의 ADS blend × (1-melee blend)로 적용한다. 왼쪽은 X를 대칭 적용한다. 권총/맨손 및 일반 지향 사격은 기존 경로를 사용한다. |
| FirstPersonArmPose.cs (신규 일반 C# 객체) | 상완/하완/손 및 관련 스킨 본을 초기화 때 캐싱한다. 손은 움직이지 않고 어깨/팔꿈치만 두 관절 길이/힌트 기준으로 재배치한다. 도달 한계는 어깨 위치를 조절해 손 접촉을 보존한다. 평면으로 복사된 스킨 본에 각각 위치/회전 보정을 적용한다. 원본 Animator 뼈/카메라/손가락은 수정하지 않는다. 매 프레임 새 배열/목록을 만들지 않는다. |

```csharp
// 이전: 슬롯이 정의와 탄약을 직접 보유
Entry { int _slot; InfantryWeaponDefinition _definition; WeaponMagazine _existingMagazine; }
// 이후: 슬롯 선택과 장착/탄약 수명을 구분
Entry { int _slot; PlayerCharacterWeaponEquipment _equipment; }

// 1인칭 원본 복사 후 ADS 전용 팔 보정 (손·총구 유지)
_rightArmPose.Apply(eye + aim * shoulder, eye + aim * elbowHint, adsBlend * (1f - meleeBlend));
```

## 씬·입력 값

- `Assets/Scenes/SampleScene.unity`: Equipment/Rifle/Pistol GameObject3개, Transform3개, 장착 컴포넌트2개 추가. 루트 캐릭터 Transform의 자식 목록에 Equipment를 추가한다. 기존 오브젝트 위치/회전/배율/부모와 기존 PersonalCombat 탄창 컴포넌트는 유지한다.
- Loadout(fileID950000026)의 슬롯1→Rifle 장착 컴포넌트(fileID534477463), 슬롯2→Pistol(fileID192532999). 슬롯3은 기존처럼 별도 무기 객체 없는 맨손 상태다.
- Rifle Definition: `AK47.asset` GUID643768a2657c2884cbbef55196648d94. Pistol Definition: 기존 Pistol.asset GUID9ff421d9ea9385e4dab063bf89bd9249. ExistingMagazine 참조fileID870274802는 Pistol 장착 객체로 옮긴다. 기존 탄창15/90/1.666667초는 변경하지 않는다.
- 기존 FP Arms 컴포넌트(fileID1653572103)에 신규 어깨/팔꿈치 기본값2개를 직렬화한다. 나머지 기존 필드는 유지한다.
- 에디터 저장 결과 중 기존 문서3개(루트 자식 목록, Loadout, FP Arms 신규 필드)와 신규 문서8개만 옮기는 방식으로 검증했다. 무관한 씬 재저장은 제외한다.
- `Assets/InputSystem_Actions.inputactions`: PlayerCharacter 맵의 기존 SelectRifle 키3→1, SelectPistol 키1→2, SelectUnarmed 키2→3. 기존 액션/바인딩ID/다른 입력과 Fighter 맵은 유지한다.

## 테스트 파일 변경

신규 `Assets/Tests/PlayMode/PlayerEquipmentTests.cs`: 실제 씬의 AK 기본 장착, 계층/슬롯, 기존 권총 탄창 값, 종류별 탄약·장전 취소·표시/총구 전환, Inspector 변경, 비활성화 중 장비 변경 후 재활성화, 부활 초기화 검사.

신규 `Assets/Tests/PlayMode/FirstPersonAdsArmPoseTests.cs`: AK/M4 양 모델의 ADS 및 상하 조준에서 오른쪽 상완이 화면 오른쪽, 팔꿈치가 시야 아래에 남는지 검사한다. 양손 접촉/총구 조준/장전 전환과 원본 뼈/카메라 불변도 검사한다. 변경 전 좌표는 이 조건을 만족하지 못한다.

기존 파일은 검증하는 동작을 바꾸지 않고 사용자 요청으로 달라진 슬롯 선택만 이전 의미에 맞춰 옮겼다. 기존 동작 허용 오차는 변경하지 않는다.

- `AssaultRifleTests.cs`: 소총3→1/권총1→2/맨손2→3, 기본 소총 이름 AK-47 및 부활 기본 슬롯 반영.
- `PlayerCharacterCameraTests.cs`: 공유 CharacterViewTestContext가 기존 권총·시점 테스트를 명시적으로 권총2로 시작하게 한다. 실제 씬 기본 AK-47은 새 장착 테스트에서 별도로 검증한다.
- `PlayerCharacterCombatInputTests.cs`: 실제1/2/3 키 검사에3번 맨손 입력을 추가한다.
- `PlayerCharacterCombatTests.cs`, `PlayerCharacterFistAnimationTests.cs`, `PlayerCharacterHandActionTests.cs`, `PlayerCharacterTorsoGaitTests.cs`, `PlayerCharacterWeaponPoseTests.cs`: 기존 권총/맨손 시나리오에 맞춰 선택 슬롯 상수를 사용한다.
- PlayerCharacterCombatTests의 권총 전용 탑승 검사는 새 기본 AK-47과 구분해 권총2를 명시적으로 선택한다. 기존 14발 유지/탑승 중 장전 취소 기대는 그대로 유지한다.
- `RealRifleModelTests.cs`: 내부 사전의 Item.Definition을 리플렉션으로 바꾸던 시험용 경로를 제거한다. 공개 GetEquipment(1).TryEquip(definition)으로 실제 장착 API를 검사한다.

신규 스크립트4개의 .meta도 추가한다. 기존 스크립트 GUID는 유지한다. 임시 진단/생성 도구 및 캡처 검사는 Library에만 두고 제품 Assets에는 포함하지 않는다.

## 제거/교체 내역과 설계 검토

Loadout.Entry의 `_definition`·`_existingMagazine`, 중첩 Item 클래스, 소유 MagazineState 생성/Tick 루프를 장착 컴포넌트로 옮겼다. 기존 씬의 슬롯별 직접 정의/탄창 참조는 장착 객체 참조로 교체했다. 기존 권총/소총 모델, 무기 정의 에셋, 탄창 컴포넌트, 외부 모션 클립은 삭제하지 않았다.

장착 객체는 선택된 종류와 탄약 수명, Loadout은 슬롯 선택, Controller는 전투 실행, 기존 CombatView는 표시 모델 수명, FirstPersonArmsView/일반 ArmPose는 1인칭 표시 보정을 맡는다. 상태 변경은 TryEquip·이벤트로 전달하고 정의 데이터는 인스턴스별 선택 참조로 사용한다. 새 상속/인터페이스/관리자 없이 실제 수명/책임 차이만 분리했다. 의도적인 OOP/SOLID 예외는 없다.

## 검증

Unity6000.4.8f1 격리 프로젝트의 -batchmode -noaudio 검사. 사용자 Editor/오디오 설정을 변경하지 않는다.

- 수정 전 AK/M4 ADS 가림을 실제 캡처/뼈 좌표로 재현했다. 초기 중앙 물리 레이 표본은 화면 오른쪽 가림을 나타내지 못해 증거로 사용하지 않았다. 실제 렌더와 상완/팔꿈치 좌표를 기준으로 판단했다.
- 팔 보정 후 모델 파지/장전 포함3/3 통과. AK ADS 상완 좌표는(.45,-.4,-.1), 팔꿈치(.30,-.37,.33), 손(0,-.17,.45). 손/총구 위치는 유지하며 팔만 시야에서 내려간다. Before/After PNG를 확인했다.
- 장착/슬롯/입력/ADS/기존 권총 집중13/13 통과(제품12 + 임시 캡처1), 기존 검증 허용 오차 변경 없음.
- 첫 전체167개 검사 중165개 통과. 재활성화 직후 총구가 오래된 화면용 손을 참조하는 표시 수명 문제와, 기본 권총을 가정한 탑승 테스트를 발견했다. 선택 직후 원본 손/후반 화면용 손으로 부착 단계를 분리하고 권총 전용 테스트의 장비를 명시했다. 해당 아군 더미 발사/탑승 및 장착·ADS 집중12/12 통과. 발사체/피해 알고리즘 및 기존 테스트 허용 오차는 수정하지 않았다.
- 최종 final.xml 전체 제품167/167 통과. 임시 캡처 검사는 최종 전체 실행에서 제외했다. 신규 클래스/메타/씬/입력/테스트를 검증한 격리본과 동일 바이트로 적용한다.
- 실제 사용자 Editor 조작감/장전 중 상세 손가락 밀착과 타깃 빌드는 별도 확인이 필요하다. 카메라 설정·외부 모션 클립·무기 성능/탄약 값은 그대로다.

## 변경 파일 전체

런타임/씬/입력/테스트/메타24개와 문서2개, 총26개 파일이다. 신규 스크립트/테스트 GUID4개의 충돌이 없고 기존 메타는 변경하지 않았다. 적용 파일24개는 검증한 격리본과 바이트가 같음을 확인했다.

- `Assets/Features/PlayerCharacter/Scripts/FirstPersonArmPose.cs`
- `Assets/Features/PlayerCharacter/Scripts/FirstPersonArmPose.cs.meta`
- `Assets/Features/PlayerCharacter/Scripts/FirstPersonArmsView.cs`
- `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterCombatView.cs`
- `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterController.cs`
- `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterInput.cs`
- `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterLoadout.cs`
- `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterWeaponEquipment.cs`
- `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterWeaponEquipment.cs.meta`
- `Assets/InputSystem_Actions.inputactions`
- `Assets/Scenes/SampleScene.unity`
- `Assets/Tests/PlayMode/AssaultRifleTests.cs`
- `Assets/Tests/PlayMode/FirstPersonAdsArmPoseTests.cs`
- `Assets/Tests/PlayMode/FirstPersonAdsArmPoseTests.cs.meta`
- `Assets/Tests/PlayMode/PlayerCharacterCameraTests.cs`
- `Assets/Tests/PlayMode/PlayerCharacterCombatInputTests.cs`
- `Assets/Tests/PlayMode/PlayerCharacterCombatTests.cs`
- `Assets/Tests/PlayMode/PlayerCharacterFistAnimationTests.cs`
- `Assets/Tests/PlayMode/PlayerCharacterHandActionTests.cs`
- `Assets/Tests/PlayMode/PlayerCharacterTorsoGaitTests.cs`
- `Assets/Tests/PlayMode/PlayerCharacterWeaponPoseTests.cs`
- `Assets/Tests/PlayMode/PlayerEquipmentTests.cs`
- `Assets/Tests/PlayMode/PlayerEquipmentTests.cs.meta`
- `Assets/Tests/PlayMode/RealRifleModelTests.cs`
- `Docs/AI/EquipmentAndAdsChanges.md`
- `Docs/AI/UnityProjectContext.md`
