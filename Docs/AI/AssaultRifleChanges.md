# 돌격소총 추가 (2026-10-30)

## 동작 및 설정

SampleScene에서 기존 1번 권총·2번 맨손에 3번 돌격소총을 추가한다. 마우스 왼쪽 유지 연사, R 장전, 기존 우클릭 견착/ADS와 V 시점 전환을 사용한다. 소총은 초당 10발, 피해 10, 탄창 30발·예비탄 180발, 장전 1.6초다. 권총은 기존 씬 WeaponMagazine(15/90/1.666667초), 캐릭터 발사/피해 값을 그대로 사용한다. 무기 전환은 장전을 취소하지만 남은 탄약을 초기화하지 않는다. 부활 시 두 무기의 탄약을 초기화하고 권총을 선택한다.

## 변경된 런타임 파일

| 파일(Assets/ 기준) | 변경 내용 및 이유 |
|---|---|
| Features/Weapon/Scrips/IWeaponMagazine.cs (신규) | MonoBehaviour 권총 탄창과 소총의 일반 C# 탄약 상태를 같은 계약으로 소비하도록 탄수/장전 API 및 이벤트를 정의한다. |
| Features/Weapon/Scrips/WeaponMagazineState.cs (신규) | 탄수·예비탄·장전 종료 시간·보충을 소유한다. 시간/사용 가능 여부를 주입하며 모델/입력/Unity 오브젝트 생성과 무관하다. |
| Features/Weapon/Scrips/WeaponMagazine.cs | 기존 직렬화 필드 3개와 공개 조작/이벤트를 유지하고 내부 상태/타이머/보충 로직을 WeaponMagazineState에 위임한다. 기존 씬 컴포넌트를 삭제하거나 이동하지 않는다. |
| Features/Weapon/Scrips/InfantryWeaponDefinition.cs (신규) | 표시 이름·사격·탄약·모델·애니메이션·그립·1인칭 배치·반동 데이터를 묶는다. 권총의 FireRate=0/CustomGrip=false는 기존 캐릭터/뷰 설정 사용을 뜻한다. |
| Features/PlayerCharacter/Scripts/PlayerCharacterLoadout.cs (신규) | 슬롯/선택 상태와 각 무기의 탄약 수명을 소유한다. 권총은 기존 탄창 참조, 소총은 별도 WeaponMagazineState를 사용한다. 동일 슬롯/알 수 없는 슬롯 선택은 무시한다. 전환·비활성화는 장전을 취소한다. |
| Features/PlayerCharacter/Scripts/PlayerCharacterController.cs | Equip(bool) 두 상태를 Equip(int) 슬롯 선택으로 교체한다. IsArmed 자동 상태를 로드아웃 조회로, Magazine 반환형을 IWeaponMagazine으로 교체한다. 사격/장전/취소/부활이 현재 장비를 사용한다. 로드아웃이 없는 기존 오브젝트에는 1/2번 동작을 유지한다. 기존 총구 가림 검사·발사체·피해 정책은 유지한다. |
| Features/PlayerCharacter/Scripts/PlayerCharacterInput.cs | SelectRifle 액션을 선택적으로 찾고 3번 슬롯을 반환한다. 기존 1/2 및 다른 입력은 유지한다. |
| Features/PlayerCharacter/Scripts/PlayerCharacterCombatView.cs | 단일 _pistol 표시에서 원래 권총+선택된 모델 캐시로 확장한다. 장비 전환 시 구 탄창 이벤트 해제/신 탄창 구독, AnimatorOverrideController 선택, 총구 재연결을 수행한다. AttachWeapon이 선택 무기의 그립/표시 배율을 적용한다. TryGetSupportHandPose가 모델 파지점을 오른손 로컬 좌표로 제공한다. 기존 레이어/근접/장전/포즈 정책은 유지한다. |
| Features/Weapon/Scrips/WeaponModel.cs | 선택적 SupportHandGrip 참조/읽기 전용 접근자를 추가한다. 기존 권총은 null이므로 기존 손 오프셋을 계속 사용한다. |
| Features/PlayerCharacter/Scripts/PlayerCharacterWeaponAimView.cs | 기존 고정 왼손 오프셋 대신 모델 파지점이 있으면 그 위치/회전을 양손 IK에 전달한다. 없으면 기존 값으로 복귀한다. 기존 상체 안정화·상하 조준·관절 해법은 유지한다. |
| Features/PlayerCharacter/Scripts/FirstPersonArmsView.cs | 현재 무기 데이터가 CustomGrip이면 전용 보정/배율을 사용한다. 파지 중에는 총구를 시선에 맞추고, 장전/공격 중에는 외부 모션 회전을 보존한다. 소총은 오른손 위치를 화면 배치 기준으로 삼는다. 권총/맨손의 기존 기준점/배치/카메라 피드백은 유지한다. |
| Features/UI/PlayerCombatHud.cs | 고정 PISTOL 문자열을 현재 WeaponName으로 바꾸고 현재 무기의 탄수/장전 상태를 표시한다. |
| Tests/PlayMode/AssaultRifleTests.cs (신규) | 더미 팀별 피해, 연사 간격·빈 탄창, 전환·재장전·부활·모델/총구 연결, 양 시점 상하 조준/점프 중 지지 손 접촉을 검증한다. |

위 신규 스크립트 및 테스트의 .meta도 추가한다. 기존 스크립트 .meta/GUID는 변경하지 않는다.

## 씬·입력 및 새 에셋

- `Assets/Scenes/SampleScene.unity`: 기존 PlayerCharacter 루트에 Loadout 컴포넌트 1개 추가, Controller._loadout 연결. 슬롯1은 Pistol 정의 및 기존 탄창 fileID870274802, 슬롯3은 AssaultRifle 정의에 연결. 기존 컴포넌트/오브젝트/참조/Transform/직렬화 값은 보존한다. 에디터 저장의 무관한 변경을 제외하고 위 문서만 패치했다.
- `Assets/InputSystem_Actions.inputactions`: 기존 PlayerCharacter 맵 끝에 SelectRifle Button과 `<Keyboard>/3` 바인딩만 추가한다.
- `Assets/Features/Weapon/Definitions/Pistol.asset`: 기존 권총 설정을 참조하는 정의. 모델/애니메이션/그립 재정의 없음.
- `Assets/Features/Weapon/Definitions/AssaultRifle.asset`: 위 소총 수치, 모델/오버라이드/그립/1인칭 배치와 반동1.5도·.01m를 연결한다. 그립 위치(0.012945553,-0.03099503,0.020586709), 소총 원본 자세 기준 회전(55.290474,255.3365,328.1393). 1인칭 전용 배율 .7, 위치(.3,-.42,.65), ADS(0,-.16834281,.45), 보정 회전(20.779833,.12529466,357.8087)이다. 이는 새 소총 표시 데이터이며 캐릭터나 카메라 Transform/FOV 설정 변경이 아니다.
- `Assets/Features/Weapon/Definitions/AssaultRifle.overrideController`: 원래 PlayerCharacter.controller의 PistolHoldUp/Neutral/Down, PistolShoot, PistolReload 클립만 소총 클립으로 바꾼다. 원본 컨트롤러/마스크/하체/점프/맨손 클립은 수정하지 않는다.
- `Assets/Features/Weapon/Prefabs/PrototypeRifle.prefab`: Quaternius 원본 메시, 길이1.8 월드 단위, +Z 총열, Muzzle(0,.24048974,1.3899999) 및 SupportHandGrip(-.14,.08,.52). 지지 손 회전은 대상 Humanoid의 손바닥/손가락 축을 총열 아래를 받치는 방향으로 맞춘다. 메시 원점은 방아쇠 손 높이에 맞춰 Geometry.y=-.13607196으로 보정했다. 별도 충돌/게임플레이 컨트롤러를 붙이지 않는다.
- `Assets/Features/Weapon/Models/PrototypeRifle/{Black,DarkMetal,DarkWood,Metal}.mat`: 원본 모델의 네 색상을 URP Lit 재질로 연결한다.
- `Assets/ThirdParty/Quaternius/Guns/AssaultRifle_1.fbx`: 공식 Ultimate Guns Pack CC0 모델 및 임포터 .meta. Guns/SOURCE.md에 배포 출처·해시 추가, 기존 License.txt 재사용.
- `Assets/ThirdParty/KayKit/Rifle/CombatRanged.fbx`: 공식 Character Animations1.1 CC0 원본. 포함 T-Pose로 Chest 포함 Humanoid 매핑을 구성한다.
- 같은 폴더 `RifleHold.anim`, `RifleShoot.anim`, `RifleReload.anim`: 외부 Ranged_2H_Aiming/Shoot/Reload의 프로젝트용 파생 클립. 원본 손/상체 키프레임 유지, 이벤트 제거. 소스에 손가락 뼈가 없어 오른손은 기존 CC0 Quaternius PistolHoldNeutral, 왼손은 FistsGuard의 손가락 채널만 보완한다. 원본 FBX/기존 클립은 수정하지 않는다.
- 같은 폴더 `SOURCE.md`: 원본/파생/손가락 포즈 출처와 라이선스를 기록한다. 기존 KayKit/LICENSE.txt 재사용.
- 모든 새 에셋과 새 폴더의 Unity .meta 추가. 생성/진단용 Editor 도구·임시 캡처 테스트·ZIP·로그·PNG는 Library 아래에만 두며 제품 Assets에 포함하지 않는다.
- `Docs/AI/AssaultRifleChanges.md` 및 `Docs/AI/UnityProjectContext.md`: 이 변경 및 검증 상태를 기록한다.

## 교체/제거한 구현

```csharp
// 이전: 장비는 권총/맨손 bool, Magazine은 단일 컴포넌트
Equip(slot == 1);
public WeaponMagazine Magazine => _magazine;
// 이후: 장비 선택은 슬롯, 탄약 소비는 현재 장비의 계약
Equip(slot);
public IWeaponMagazine Magazine => _loadout != null ? _loadout.Magazine : _magazine;
```

- Controller의 `Equip(bool)`/IsArmed 자동 속성 상태는 슬롯 장비 상태/레거시 폴백으로 교체했다.
- WeaponMagazine 내부 `_reloadEndsAt`, Rounds/Reserve/IsReloading 저장 및 Update 보충 본문은 WeaponMagazineState로 옮겼다. 공개 필드가 아닌 내부 구현 교체이며 직렬화 필드/이벤트 계약은 유지했다.
- CombatView의 단일 권총 표시/직접 탄창 구독/현재 탄창에서 해제하는 구현은 선택 모델 표시 및 관찰 중인 탄창을 정확히 해제하는 구현으로 교체했다. 기존 Pistol 접근자는 유지한다.
- AimView의 고정 왼손 위치/회전 사용 두 곳은 모델 앵커 우선 조회로 교체했다. 기존 직렬화 오프셋은 삭제하지 않았다.
- 기존 UI PISTOL 상수 표시는 장비 이름 조회로 교체했다.
- 기존 에셋/컴포넌트/직렬화 필드 삭제는 없다.

## 설계 검토

데이터(Definition), 장비/탄약 수명(Loadout), 재사용 가능한 탄약 상태(State), 입력, 전투 실행, 애니메이션/모델 표시는 기존 책임 경계에 맞춰 분리했다. IWeaponMagazine은 실제 두 구현의 교환 지점이며 넓은 장비 인터페이스나 새 관리자를 만들지 않았다. 모델별 차이는 데이터/오버라이드로 확장한다. 현재 소총은 기존 장비 근접 공격/공유 탄환 시스템을 사용하며 전용 소총 개머리판 모션·새 사운드·탄창 분리 메시 애니메이션까지 추가한 것은 아니다.

## 검증

테스트는 Unity6000.4.8f1 별도 프로젝트에서 -batchmode -noaudio로 실행하며 사용자 Editor/게임 오디오 설정을 변경하지 않는다.

- 작업 전/적용 전 기준 파일11개의 SHA256을 비교한다. 새 사용자/에디터 변경 및 새 에셋 이름 충돌이 없음을 확인한다. 기존 미커밋 작업은 보존한다.
- 장비 연결 전 아군 DummyShooter로 사격장9개 표적의 적/중립 피해10, 아군 피해0을 확인했다. 탄약 상태 검사 포함 range.xml 2/2 통과.
- 첫 통합 검사에서 잘못된 권총 탄창 참조(null)를 발견하고 기존 fileID870274802로 복구했다. 1인칭 소총 배치는 실제 Humanoid 자세로 캡처하며 보정했다.
- 첫 전체 회귀 161/162: 권총 재장착 직후 비활성화되는 회귀를 발견했다. RefreshEquipment가 기존 동작처럼 즉시 선택 모델을 활성화하도록 수정했다. 테스트 기대값/허용 오차는 바꾸지 않았다.
- 수정 후 전체 final.xml 163/163 통과(제품 테스트162 + 임시 캡처1). 기존 권총/맨손/상체 안정화/점프/상하 조준/탑승/사격장 검사 포함.
- 마지막 메시 기준점/ADS 높이/소총 원본 손목 회전 조정 후 releaseFocused.xml 집중8개를 실행했다. 카메라 평면상 팔의 롤 검사도 추가해 피치 조준과 손목 보정 오류를 구분한다. 최종 releaseFocused.xml 8/8 통과(제품7 + 임시 캡처1). 첫 롤 검사는 정상 조준 피치까지 포함했으므로 카메라 평면 투영으로 롤만 분리했다. 기존 테스트의 기대값/허용 오차는 변경하지 않았다.
- 1·3인칭 파지, ADS, 장전 전/중/후 캡처를 확인했다. 소총은 일반 3인칭 외부 클립을 리타깃한 것이므로 전용 1인칭 손/탈착 탄창 모션의 완성도를 주장하지 않는다. 실제 Editor에서 손가락 밀착·장전 시야·이동 조작감을 확인해야 한다. 타깃 플레이어 빌드는 실행하지 않았다.
- 컴파일 경고: 기존 GetInstanceID obsolete 등 및 격리 프로젝트의 패키지 테스트 어셈블리/선택적 HDRP 참조 안내가 있다. 새 스크립트 컴파일 오류는 없다.

적용 검증: 신규 에셋/폴더 GUID22개가 서로 중복되지 않고 기존 Assets GUID와 충돌하지 않음을 확인했다. 적용 시 기존 씬 문서 중 루트 컴포넌트 목록과 Controller 참조 두 문서만 변경됐음을 검사하고, 기존 입력 액션/바인딩/컨트롤 스킴 보존도 검사한다. 런타임/에셋/메타51개는 격리 프로젝트에서 검증한 바이트를 그대로 복사한다. Guns/SOURCE.md와 이 문서/프로젝트 컨텍스트를 포함한 적용 파일은54개다.
