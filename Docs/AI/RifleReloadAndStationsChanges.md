# 소총 장전 표현 및 인게임 소총 교체대

2026-10-30. 대상: SampleScene, AK-47 / M4A1. 소총 1·권총 2·맨손 3, 기본 AK-47, 기존 무기별 탄약·발사·피해 설정을 유지한다.

## 사전 비교 및 교체한 구현

이전 작업 적용 해시와 현재 소스를 비교했으며 사용자/Editor의 후속 변경은 없었다. Animator Controller도 이전 소총 작업의 기준 해시와 일치했다. 원본 Editor 대신 Library의 격리 프로젝트에서 구현·검증했다.

- 기존 `AssaultRifle.overrideController`의 장전 매핑: KayKit 파생 `RifleReload.anim` → 기존 CC0 Quaternius `PistolReload.anim`. KayKit 원본과 파생 클립은 삭제하지 않았다. 기존 클립의 왼손이 탄창에서 크게 벗어나고 총 자세가 맞지 않아, Quaternius 모션을 기반으로 무기별 접촉 보정을 적용했다. **새 소총 전용 애니메이션을 다운로드한 작업은 아니다.**
- 기존 `PlayerCharacterWeaponAimView.SolveArm()`은 삭제하고 같은 계산을 `HumanoidArmSolver.Solve()`로 이동했다. 기존 조준의 두 호출도 이 공용 함수로 연결했다. 조준과 장전에서 같은 계층형 팔 IK를 재사용하기 위한 이동이며 계산 방식은 바꾸지 않았다. 복제된 1인칭 스킨용 `FirstPersonArmPose`는 별도 책임이라 유지했다.
- 기존 고정 장전 재생 속도에 `ReloadSpeed` 파라미터를 연결했다. 클립 길이 / 실제 장전 시간으로 계산한다. 소총은 약 1.666667 / 1.6 = 1.041667배, 권총은 기존 시간에 맞는 약 1배다. 무기의 실제 장전 시간은 변경하지 않았다.
- 기존 장전 중 총·손 위치를 외부 클립에만 맡기던 방식에 접촉 보정을 추가했다. 오른손은 총을 유지하고, 왼손은 앞손잡이 → 탄창 → 앞손잡이로 이동한다. 탄창은 기존 부모 아래에서 분리·재삽입 표현만 한다. 버려지는 별도 탄창이나 새 탄창 생성 기능은 없다.

## 변경한 모든 스크립트

경로는 저장소 루트 기준이다. 신규 5개 런타임 스크립트와 신규 테스트 2개에는 각각 Unity GUID를 위한 `.meta`도 추가했다. 기존 파일의 `.meta`는 유지했다.

| 파일 | 변경한 코드 / 책임 |
|---|---|
| `Assets/Features/PlayerCharacter/Scripts/IPlayerCharacterInteractionInput.cs` **신규 + .meta** | `HasControl`, `InteractPressed`만 노출하는 입력 계약. 교체대가 전체 플레이어 입력 구현에 의존하지 않게 한다. |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterInput.cs` | 기존 입력 클래스가 위 인터페이스를 추가 구현한다. 기존 F 입력과 속성을 그대로 사용하며 InputAction 변경은 없다. |
| `Assets/Features/PlayerCharacter/Scripts/RifleSelectionStation.cs` **신규 + .meta** | `_definition`, `_interactionPoint`, `_range=3` 소유. 표시 소총과 유효 거리 조회만 담당한다. |
| `Assets/Features/PlayerCharacter/Scripts/PlayerRifleStationInteraction.cs` **신규 + .meta** | 명시적으로 연결된 교체대에서 가장 가까운 유효 대상을 선택한다. 거리·가시선·조작 가능 상태를 검사하고 F로 기존 `equipment.TryEquip(definition)` 및 `loadout.TryEquip(RifleSlot)`을 호출한다. 차량 탑승·아군 소생 우선순위를 유지한다. 새 탄약 소유자나 보충 기능을 만들지 않는다. |
| `Assets/Features/PlayerCharacter/Scripts/HumanoidArmSolver.cs` **신규 + .meta** | 기존 `SolveArm` 본문을 이동한 내부 공용 두 관절 IK 계산. 상태·입력·장전 진행을 소유하지 않는다. |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterWeaponAimView.cs` | 기존 private `SolveArm` 메서드 제거, 두 호출을 `HumanoidArmSolver.Solve`로 교체. 기존 조준 위치/값 유지. |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterRifleReloadView.cs` **신규 + .meta** | 장전 시작/완료·장비 변경 이벤트를 관찰한다. 총·탄창·손의 시작 자세와 장전 진행을 소유하고 LateUpdate에서 접촉을 맞춘다. 완료/취소/장비 변경/비활성화 시 탄창 localPosition/localRotation을 정확히 복원한다. 기존 장전 상체 레이어의 페이드가 끝날 때까지 상태를 유지한다. |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterCombatView.cs` | `_reloadAnimation` 참조와 ReloadSpeed 파라미터 유무 캐시 추가. `OnReload()`에서 현재 OverrideController의 실제 클립 길이를 사용해 장전 속도를 설정한다. 기존 장전·근접 레이어 동작 유지. |
| `Assets/Features/PlayerCharacter/Scripts/FirstPersonArmsView.cs` | `_reloadPose` 참조 추가. 소총 장전 중에도 오른손 기준 총 방향 보정을 적용하고 일시적 ViewOffset을 더한다. 복제 팔 보정 가중치를 `Max(ADS blend, reload blend) * (1-melee)`로 계산해 비조준 장전도 화면 안에서 보이게 한다. 카메라 자체는 이동시키지 않는다. |
| `Assets/Features/Weapon/Scrips/WeaponModel.cs` | `_magazine`, `_reloadGrip`, `_reloadTravel`, `_reloadTilt` 및 읽기 전용 속성 추가. 모델별 장전 접촉점·탄창 이동량만 제공하며 게임플레이 장전 상태는 소유하지 않는다. |
| `Assets/Features/UI/PlayerCombatHud.cs` | 선택적 `_rifleStations` 참조 추가. 기존 생명·소생 안내 우선순위 뒤에 `F  EQUIP <소총명>` 또는 `<소총명> EQUIPPED` 표시. |

## 변경한 모든 에셋 및 직렬화 값

| 파일 | 구체적인 변경 |
|---|---|
| `Assets/Features/Weapon/Definitions/AssaultRifle.overrideController` | 장전 클립 매핑 1개만 KayKit RifleReload → Quaternius PistolReload로 교체. Hold/Shoot 등 나머지 매핑 유지. |
| `Assets/Features/PlayerCharacter/Animation/PlayerCharacter.controller` | float `ReloadSpeed=1` 추가. 기존 두 PistolReload 상태의 speedParameter를 ReloadSpeed로 활성화. 변경 문서 3개, 삭제 0개. 기본 state speed·마스크·레이어 구성 유지. |
| `Assets/Features/Weapon/Prefabs/AK47.prefab` | 기존 Magazine 아래 `ReloadGrip` GO/Transform 추가. Magazine의 자식 목록과 WeaponModel 참조 필드만 수정(기존 문서 2개, 신규 2개). Travel `(0,-0.4,-0.08)`, Tilt `(-15,0,0)`. |
| `Assets/Features/Weapon/Prefabs/M4A1.prefab` | 같은 구조로 ReloadGrip 추가. Travel `(0,-0.4,-0.08)`, Tilt `(0,0,0)`. 기존 문서 2개, 신규 2개. |
| `Assets/Scenes/SampleScene.unity` | 기존 문서 6개에 새 컴포넌트·참조·SceneRoots 연결만 변경, 신규 문서 44개, 삭제 0개. 기존 오브젝트 위치나 장비 구성은 바꾸지 않는다. 상세 아래. |

ReloadGrip은 각 기존 Magazine 렌더러 bounds 중심에서 월드 X -0.1 위치를 탄창 로컬 위치로 저장하고, 기존 SupportHandGrip 회전에 Z -90도를 적용한 방향이다. 모델의 기존 머즐·앞손잡이·메시·재질 참조는 유지한다.

SampleScene 연결:

- 기존 캐릭터 Animator 오브젝트에 `PlayerCharacterRifleReloadView` 추가. CombatController/CombatView/View/Camera/Animator 참조 연결. 1인칭 장전 중 추가 오프셋: 비조준 `(-0.08,0.32,0.3)`, ADS `(-0.06,0.26,0.55)`. 기존 무기 정의의 오프셋·카메라 설정을 덮어쓰지 않는다.
- 기존 캐릭터 루트에 `PlayerRifleStationInteraction` 추가. 입력·Loadout·행동 정책·차량·소생·교체대 2개 참조 연결. HUD에도 같은 컴포넌트를 연결한다.
- 새 루트 `RifleSelectionStations`: 월드 `(79.5,-99,-536.1)`로 시작점 8m 앞. 자식 `AK47_Station` 로컬 X -3, `M4A1_Station` X +3.
- 각 교체대 `InteractionPoint (0,2,-0.7)`, 범위 3m. `Bench` 위치 `(0,0.65,0)`, 크기 `(3,1.3,1.5)`, 기존 TrainingConcrete/Concreat PhysicsMaterial 사용. 장식 FrontMarking은 충돌 없는 신규 큐브로 기존 TrainingMarkings 사용.
- 각 전시용 소총은 기존 AK47/M4A1 프리팹 인스턴스, 위치 `(-0.25,1.8,0)`, Y 90도, scale1. `Label`은 위치 `(0,3,0)`, 크기 `(4,1.5)`, fontSize5, 기존 LiberationSans SDF, 총 이름과 `[F] EQUIP` 표기.
- 기존 InputAction, 플레이어 시작 위치·크기, 기존 구조물, 카메라, AudioSource, 외부 원본 모델·텍스처·애니메이션은 변경하지 않았다.

## 동작 변화와 취소 처리

```csharp
// 이전: 장전 중 손과 분리되지 않는 탄창이 서로 다른 위치에 표시됨.
// 이후: 기존 외부 모션 + 모델별 ReloadGrip 접촉 보정.
// 완료/교체/취소 시:
magazine.localPosition = originalLocalPosition;
magazine.localRotation = originalLocalRotation;

// 이전: Inspector에서 Rifle.Definition 변경.
// 이후: 교체대에 접근하고 F. 기존 장비 상태 소유자를 그대로 사용.
equipment.TryEquip(station.Definition);
loadout.TryEquip(PlayerCharacterLoadout.RifleSlot);
```

위는 핵심 동작 발췌이며 전체 소스 대체용 코드는 아니다. 탄약 보존·장전 취소는 기존 Equipment/Loadout 책임이다. 교체대를 반복 사용해도 탄약이 채워지지 않는다. 벽 뒤·범위 밖·조작 불가·탑승 중에는 사용할 수 없다.

## 검증과 설계 검토

신규 테스트 파일(각 `.meta` 포함):

- `Assets/Tests/PlayMode/RifleReloadContactTests.cs`: AK/M4 × 1·3인칭 손과 ReloadGrip 거리 <0.06m, 탄창 이동,1.6초 장전 완료·원위치 복원·장전 중 발사 금지, 교체 취소 시 숨겨진 이전 모델의 탄창 복원.
- `Assets/Tests/PlayMode/RifleSelectionStationTests.cs`: 실제 F 입력·양쪽 교체대·소총 슬롯 선택·기존28발 보존, 차량 상호작용 영역과 겹치지 않음, 거리·벽·조작 불가 제한.

Unity 6000.4.8f1 격리 프로젝트, `-batchmode -noaudio`. 집중 검사 9/9와 실제 시작 화면/근접 HUD 촬영 검사 1/1 통과. 촬영용 임시 테스트는 제품에 포함하지 않았다. 전체 PlayMode 최종 결과는 아래 최종 검증 기록을 따른다. 기존 dummy 충돌/팀 피해·권총·장비·탑승 회귀 검사를 함께 실행한다. 이번 작업은 발사·피해 알고리즘을 바꾸지 않았다.

클립·몸통은 Animator, 모델 접촉·취소 복원은 ReloadView, 데이터는 WeaponModel, 상호작용은 StationInteraction, 탄약은 기존 Equipment, 표시는 HUD로 분리했다. 입력 인터페이스는 필요한 두 속성만 노출한다. 불필요한 상속/새 총괄 Manager는 추가하지 않았다. Unity 직렬화된 구체 View/장비 컴포넌트 참조는 하나뿐인 현재 구현을 연결하기 위해 유지했다. 교체 가능한 입력만 좁은 인터페이스로 추상화했다.

남은 확인: 실제 사용자 Editor에서 장전의 속도감·손가락 접촉·여러 조준 각도·움직이면서 장전하는 시각 품질. 자동 접촉 검사는 모든 미세한 손가락/메시 관통이나 모션 취향을 보장하지 않는다. 타깃 Player 빌드와 사운드 청취 검사는 실행하지 않았다.

변경 문서: 본 파일 `Docs/AI/RifleReloadAndStationsChanges.md`, 현재 상태 기록 `Docs/AI/UnityProjectContext.md`. 원본 소스 적용 전 기준 해시를 재검사하고, 적용 후 격리 프로젝트와 파일 바이트 일치를 검증한다. 증거 경로: `Library/CodexRifleReloadStation/`의 `focused.xml`, `stationCapture.xml`, `final.xml`, `serializedChanges.json`, `applied.json`, `After_*Reload*.png`, `StationsAtStart.png`.

최종 결과: 제품 PlayMode **171/171 통과**, 실패/스킵 0개(139.7초). 기존 테스트 허용오차는 변경하지 않았다. 원본 기준 해시 재검사 통과, 코드·에셋·테스트·meta 25개를 검증한 격리 파일과 바이트 동일하게 적용했다. 문서 2개를 포함해 이번 작업 변경 파일은 총 27개다. 촬영 전용 검사 최초 실행은 TMP 테스트 어셈블리 참조 오류로 실패했고, 테스트에서 문자열을 리플렉션으로 조회하도록 수정한 후 1/1 통과했다. 촬영 코드와 생성 도구는 Library에만 보관한다.

스크립트 `git diff --check` 통과. 전체 에셋까지 포함한 검사는 Unity YAML의 빈 문자열 뒤 공백(`m_Name: ` 등)을 보고한다. 기존 누적 미커밋 씬 변경도 포함된 Git 기준 결과이며, Unity 직렬화 포맷은 별도로 정규화하지 않았다.
