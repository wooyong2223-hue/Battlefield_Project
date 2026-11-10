# 구조 점검 결과

[목차](<README.md>) · [개발 구조](<Architecture.md>) · [검증 근거](<Validation.md>)

## 판단

입력·게임 상태·표시·설정 데이터·풀링이 여러 컴포넌트로 분리되어 있다. 특히 Loadout의 장비 상태, MagazineState의 탄약 상태, VehicleSeat/Occupant의 역할 분리와 JetCamera의 Orbit/Lens 합성은 유지할 가치가 있다. 집중 개선 후보는 보병 Controller의 무기별 분기, WeaponAimView의 자세별 보정, 로컬 Editor 연결 의존, 제거된 HUD의 잔여 참조다.

줄 수만으로 SRP 위반을 판정하지 않는다. 기능 변경 이유, 소유 상태, 의존 관계, 독립 테스트 가능성을 기준으로 판단한다. 예를 들어 MissileWarningView 370줄은 경고 표시라는 공통 책임 아래 여러 시각 상태를 다뤄, 줄 수만으로 분리할 근거가 약하다.

## 우선순위 목록

| ID | 우선순위·성격 | 관측·영향 | 해결 방향 | 완료 확인 |
|---|---|---|---|---|
| A01 | 높음 · 검증 실패 확인 | 최신 주요 테스트43개 중4개 실패. 권총이 FirePressed를 사용하지만 테스트 입력이 이전 유지 입력을 보냄 | 공통 테스트 입력에서 Press/Release를 명확히 구현하고 두 입력 계약을 같은 가짜 입력으로 주입. 실제 Input System 새 클릭 테스트도 보완 | 현재 실패4개와 단발/연사/빈 탄창/수류탄 복귀가 함께 통과 |
| A02 | 중간 · 책임 분리 후보 | WeaponAimView 449줄에 상체 조준, 그립, 장전 보정, 보행 흔들림 억제, 포복 접촉이 함께 존재 | 자세별 계산을 순수 계산 객체로 나눌지 검토. Animator 콜백과 적용 순서는 한 곳에서 조정 | 기존 시점·무기·자세 조합의 캡처와 회귀 테스트 유지 |
| A03 | 중간 · 확장성 후보 | Controller가 슬롯별 발사 모드 결정, 조준 Raycast, 총구 엄폐 검사, 풀 획득을 함께 처리. 모드는 캐릭터당1개 | 현재 범위를 명시. 무기 종류별 모드가 필요하면 지원 모드는 Definition, 선택 상태는 Equipment로 이동. 발사 계산 분리는 별도 검토 | 새 무기 추가 시 Controller 분기 수정 범위와 무기별 모드 유지 검사 |
| A04 | 중간 · 재현 조건 확인 | LocalTacticalActionSetup이 gitignore된 로컬 프로필을 읽고 유효성 실패 시 return. 수류탄 구성도 mantle clip 유효성에 묶임 | 기능별 필수 데이터/검증 경계를 나누고, 필요한 로컬 에셋·임포트 절차와 누락 진단 제공 | 로컬 모션이 없는 체크아웃의 기능 목록과 명시적 진단 확인 |
| A05 | 낮음 · 미사용 연결 확인 | HUD 텍스트 삭제 뒤 stance/traversal/mount/life/revive/rifleStations 필드와 사용되지 않는 mantle 저장이 남음 | UI 분리 계획에 맞춰 호출자·씬 직렬화 참조를 함께 정리. grenade 참조는 발사모드 표시 조건에서 여전히 사용 | Inspector 참조·빌드 연결·컴파일 확인 |
| A06 | 낮음 · 설정 의미 확인 | Definition.FireRate=0이면 Controller의 Damage도 기존값 사용. 개별 피해량 수정이 적용되지 않을 수 있음 | 현재 연동을 매뉴얼에 명시하고 독립 override가 필요한지 결정 | FireRate0/양수, Damage 변경 조합 테스트 |
| A07 | 낮음 · 꺼진 경로 확인 | 기체 CameraDistance 바인딩/메서드는 존재하지만 UpdateCameraDistance 호출이 주석 처리됨 | 거리 고정 요구와 비활성 상태를 명시. 재활성화 또는 제거는 요구 결정 후 진행 | 휠로 거리 변화를 기대하는 사용자가 설정 위치를 찾을 수 있는지 확인 |

A01은 테스트 실패가 확인된 항목이다. A02·A03은 설계 개선 후보이며 현재 기능 오류의 확정 판정은 아니다. A04는 코드/버전관리 조건이 확인됐고 새 체크아웃의 실제 빌드는 아직 실행하지 않았다.

## 근거 위치

- A01 — [PlayerCharacterCombatTests.cs](<../../Assets/Tests/PlayMode/PlayerCharacterCombatTests.cs#L42>), [TacticalActionTests.cs](<../../Assets/Tests/PlayMode/TacticalActionTests.cs#L201>)와 검증 XML.
- A02 — [PlayerCharacterWeaponAimView](<../../Assets/Features/PlayerCharacter/Scripts/PlayerCharacterWeaponAimView.cs>)의 GetReloadGrip, StabilizeArmedTorso, StabilizeLocomotionTorso, ApplyUpperBodyAim, FitProneItemContact.
- A03 — [PlayerCharacterController.cs](<../../Assets/Features/PlayerCharacter/Scripts/PlayerCharacterController.cs#L40>), Update/TryFire. [PlayerCharacterWeaponEquipment](<../../Assets/Features/PlayerCharacter/Scripts/PlayerCharacterWeaponEquipment.cs>)는 장비별 상태 이동 후보.
- A04 — [LocalTacticalActionSetup.cs](<../../Assets/Features/PlayerCharacter/Editor/LocalTacticalActionSetup.cs#L22>), [TacticalAnimationSet](<../../Assets/Features/PlayerCharacter/Scripts/TacticalAnimationSet.cs>), [.gitignore](<../../.gitignore>).
- A05 — [PlayerCombatHud.cs](<../../Assets/Features/UI/PlayerCombatHud.cs#L12>). 단순 문자열 제거 작업에서 필드/씬 연결 정리는 보류되어 남은 상태다.
- A06 — [InfantryWeaponDefinition.cs](<../../Assets/Features/Weapon/Scrips/InfantryWeaponDefinition.cs#L25>)와 Controller의 FireRate/Damage 속성.
- A07 — [JetCameraOrbit.cs](<../../Assets/Features/Fighter/Scripts/Camera/JetCameraOrbit.cs#L58>).

## 객체지향 4원칙

| 원칙 | 확인된 사용 | 추가 검토 |
|---|---|---|
| 추상화 | 입력 계약·IWeaponMagazine·IDamageable로 필요한 동작 노출 | 구체형 의존을 무조건 인터페이스로 바꾸지 않고 교체 필요성을 판단 |
| 캡슐화 | CurrentSlot/IsRifleSingleFire의 private set, TryEquip/TryReload 요청 | 상태의 변경 제한뿐 아니라 적절한 소유자도 검토(A03) |
| 상속 | WeaponBase의 발사 틀, ObjectPool/ProjectileBase의 공통 생명주기 | 파생 구현의 실패·재사용 시 계약 유지 검증 필요 |
| 다형성 | 입력·탄창·피해·좌석 계약에 여러 구현 연결 | 테스트 가짜 입력도 새 계약 의미를 충족해야 함(A01) |

## SOLID 5원칙

| 원칙 | 현재 판단 | 근거/검토 대상 |
|---|---|---|
| SRP | 시스템 수준 분리는 존재, 자세 보정과 보병 발사 내부는 분리 후보 | A02·A03. 오디오 재생과 개요 Editor, Camera와 Orbit/Lens 분리는 긍정적 |
| OCP | 정의 에셋으로 수치/모델 변경 가능. 무기 종류별 발사 규칙은 Controller 수정 필요 | A03·A06 |
| LSP | 코드 구조와 일부 테스트로 계약 사용 확인. 전체 파생형 호환 보장은 미검증 | WeaponBase.Fire 성공 여부, Pool OnSpawn/OnDespawn, 입력 테스트 구현 |
| ISP | 이동·시점·전투·생명·탑승 입력이 좁은 계약으로 나뉨 | CombatInput에 입력 추가 시 모든 구현 갱신 필요. 이것만으로 원칙 위반은 아님 |
| DIP | Controller가 입력/탄창/조준 계약을 사용, Projectile이 IHitEffectPlayer를 사용 | 구체형 Pool/Loadout/Unity컴포넌트 의존은 현재 단일 구현 맥락에서 판단. 불필요한 추상화 추가는 보류 |

## 정리 순서

1. A01의 테스트 입력을 최신 단발 동작에 맞춰 보완하고 B 전환 회귀 근거 확보.
2. A05의 HUD 잔여 참조를 새 UI 배치 계획과 함께 정리.
3. A04의 로컬 구성 조건을 기능별로 명확히 하고 새 체크아웃 재현 절차 마련.
4. A02·A03을 동작 캡처/테스트를 기준으로 작은 단위로 분리.

이번 점검은 문서화와 검증을 수행했다. 위 항목의 실제 수정은 후속 작업이다.
