# 수류탄 장착·좌클릭 투척과 진단 음소거 — 2026-11-04

## 현재 동작

- `4`를 누르면 장착한다. 키를 놓아도 준비 자세에서 계속 들고 있다. 수량과 신관은 변하지 않는다.
- 왼쪽 마우스를 누르면 한 번 투척한다. 장착 모션 도중 누른 클릭도 한 번 예약한다. 누르고 있거나 버튼을 놓는 동작으로 추가 투척하지 않는다.
- `1/2/3`은 미방출 수류탄을 넣고 선택한 무기로 전환한다. 같은 무기 슬롯을 눌러도 취소된다. 외부 Loadout 변경도 취소한다.
- 정상 투척 뒤에는 변경하지 않았던 이전 무기를 다시 표시한다. 투척에 사용한 클릭은 버튼을 놓을 때까지 소비하므로 총 발사로 이어지지 않는다.
- 단순 장착 상태에서는 이동·점프·자세 변경이 가능하다. 실제 투척이 시작되면 기존 동작 제한을 따른다. 엎드리기는 기존 원본 지원 범위에 맞춰 수류탄 장착을 취소한다.
- 수류탄 장착 중 총기 ADS/장전/사격을 차단한다. 원본 클립·표시 오프셋·발사 속도·피해·거리·신관·수량은 변경하지 않았다.

EA 공식 설명은 수류탄 키로 던지는 형태다. 이번 조작은 사용자가 지정한 **4 장착 → 좌클릭 투척**을 우선 적용했다. BF6 전체 수류탄 기능을 동일하게 복제했다고 주장하지 않는다. 참고: [EA 수류탄 설명](https://www.ea.com/games/battlefield/battlefield-6/news/loadout-creation-and-weapon-classes).

## 변경 전 비교

이전 CodexGrenadeMantle/applied.json의 관련 파일은 작업 시작 시 모두 동일했다. 당시 미수정이었던 PlayerCharacterView.cs도 이전 before.json과 동일함을 확인했다. 격리 프로젝트에서 먼저 구현·검사했고, 본 프로젝트로 옮기기 직전에 대상 파일의 사용자 변경이 없는지 해시로 다시 검사했다. 씬·계층·기존 설정값을 덮어쓰지 않았다.

## 교체 코드·제거된 멤버

이전 흐름:

```csharp
if (_input.ThrowPressed) TryThrow();
if (_holdEligible && !_released)
    _elapsed = Mathf.Min(_elapsed, clip.length * .5f);
// 4 키를 놓으면 모션 시간이 다시 진행돼 자동 방출
```

현재 흐름:

```csharp
if (equipPressed) TryEquip();
if (primaryPressed && IsEquipped) TryThrow();
// Ready에서는 클릭 요청이 올 때까지 시간이 진행되지 않음
```

- `ThrowPressed`/`ThrowHeld` 인터페이스 멤버와 PlayerCharacterInput 구현을 제거하고 `EquipGrenadePressed`/`ThrowGrenadePressed`로 교체했다.
- 입력 계약에 기존 무기 슬롯 선택과 `ConsumePrimaryUntilRelease()`를 추가했다. 원시 키 조회를 게임 상태 컴포넌트로 옮기지 않았다.
- `_holdEligible` 필드는 제거했다. `_state`와 `_throwRequested`가 장착 상태·단일 클릭 예약을 소유한다.
- `IsThrowing`의 private 자동 setter를 상태 기반 읽기 전용 계산으로 교체했다. 장착만 한 상태와 실제 투척을 구분한다.
- `TryThrow()`는 이제 장착한 수류탄의 투척만 요청한다. 장착은 새 `TryEquip()`가 담당한다.
- 기존 키 유지/키 해제 투척 회귀 검사는 새 입력 요구에 맞춘 장착/클릭/취소 검사로 교체했다. 높은 턱·접지·일시 중단 복귀 검사는 유지했다.
- 기존 메서드 이름·직렬화 참조·씬·원본 모션·에셋은 삭제하지 않았다. Input System의 기존 ThrowGrenade 액션 이름/ID/4 바인딩도 보존했다.

## 변경 파일 전체와 구체 내용

| 파일(저장소 기준) | 코드 변경·이유 |
|---|---|
| Assets/Features/PlayerCharacter/Scripts/IPlayerCharacterThrowInput.cs | 위 입력 계약 교체. 장착·클릭·슬롯 전환·클릭 소비를 명확히 구분. |
| Assets/Features/PlayerCharacter/Scripts/PlayerCharacterInput.cs | 기존 4 액션을 장착 입력으로 읽고, Attack의 WasPressedThisFrame을 투척 클릭으로 제공. 기존 `_suppressFireUntilRelease`를 의도별 API로 소비. 기존 G와 1/2/3 바인딩 유지. |
| Assets/Features/PlayerCharacter/Scripts/PlayerCharacterGrenadeThrow.cs | Holstered/Equipping/Ready/Throwing 상태, TryEquip, 클릭 예약, Ready 대기, 취소/복귀. 무기 슬롯 키 및 EquipmentChanged를 관찰해 취소. 초기화·파괴 때 이벤트 중복/누수 방지. 소비는 실제 방출 때만 수행. |
| Assets/Features/PlayerCharacter/Scripts/PlayerCharacterActionPolicy.cs | CanUseHands/CanAimWeapon 추가. CanAttack은 수류탄 장착 중 false. 실제 투척과 단순 장착을 구분해 점프/자세/달리기 정책 적용. |
| Assets/Features/PlayerCharacter/Scripts/PlayerCharacterView.cs | 수류탄 장착 중 기존 ADS·견착 입력 블록을 건너뛰고 추적 상태 해제. 시점·마우스 회전·카메라 설정값은 유지. |
| Assets/Features/PlayerCharacter/Scripts/PlayerCharacterTacticalAnimationView.cs | IsPresenting을 IsEquipped 기준으로 확장. IsGrenadeEquipped 추가. 장착 대기 중에도 원본 상체 레이어와 총 숨김 유지. |
| Assets/Features/PlayerCharacter/Scripts/FirstPersonArmsView.cs | 수류탄 전용 표시 오프셋의 적용 조건을 IsThrowing→IsGrenadeEquipped로 변경. 대기 중에도 손/수류탄 표시 유지. 수치와 메시 수정 없음. |
| Assets/Features/UI/PlayerCombatHud.cs | 장착 시 FRAG GRENADE 수량과 LMB THROW /1/2/3 WEAPON 안내. 비장착 시4 EQUIP GRENADE. 기존 체력·자세·소생 등 유지. |
| Assets/Tests/PlayMode/TacticalActionTests.cs | 기존 두 키 유지 투척 검사를 장착 지속·좌클릭·무기 슬롯/외부 교체 취소·동일 클릭 총 발사 방지·장착 중 클릭 예약·벽 앞 차단 검사4개로 교체. 직렬화 재초기화도 TryEquip으로 검증. 1·3인칭 대기 표시 및 기존 올라가기 유지. |
| Assets/Tests/PlayMode/PlayerCharacterCombatInputTests.cs | 4는 장착 입력이며 G 근접과 독립, 마우스 누름은 투척 클릭, 클릭 소비 후 FireWeapon/ThrowGrenadePressed 차단 검증. |
| Assets/Tests/PlayMode/AutomatedTestAudioScope.cs | 신규 테스트 전용 Editor 초기화. batchmode + runTests + noaudio에만 마스터 음소거 적용/유지. SessionState로 원래 값을 도메인 재로드 사이에 보관하고 종료 시 복원. 일반 Editor/Player와 음원·AudioSource·AudioListener·프로젝트 설정을 변경하지 않음. |
| Assets/Tests/PlayMode/AutomatedTestAudioScope.cs.meta | Unity가 생성한 신규 스크립트 GUID/임포트 정보. 컴포넌트를 씬에 추가하지 않는다. |
| Docs/AI/GrenadeEquipmentChanges.md | 이 변경·제거·검증 및 전체 파일 목록. |
| Docs/AI/UnityProjectContext.md | 최신 입력 의미·상태 소유·테스트 음소거 운영 방식·근거 추가. |

## 설계 검토

장착 상태는 기존 수류탄 컴포넌트에 유지했다. 입력 소비는 Input, 행동 허용은 ActionPolicy, 표시·ADS는 기존 View가 담당한다. 입력 계약과 읽기 전용 상태로 직접 필드 변경을 막았다. 새로운 장비 관리자나 상속 계층을 만들지 않았다. 별도 생명주기를 가진 테스트 음소거는 테스트 전용 클래스로 분리했다. OOP/SOLID에 대한 의도적 예외는 없다.

## 검증

- 집중8/8 통과 후 전체 PlayMode205/205, 실패/건너뜀0,198.2753707초. -noaudio로 실행했다.
- 실제 마스터 음소거 확인이 빠져 있었음을 보완했다. 임시 보호 코드로8/8, 재사용할 AutomatedTestAudioScope로 다시8/8 통과했다.
- 재사용 보호 코드의 로그: captured previous=False → ExitingEditMode/EnteredPlayMode/ExitingPlayMode/EnteredEditMode masterMuted=True → restored=False. 단순 명령행 옵션 기재만을 음소거 확인으로 취급하지 않는다.
- 현재 본 프로젝트 Editor는 Play 모드로 별도 실행 중인 것을 화면에서 관찰했다. 소리가 들렸던 순간 어느 세션이 원인인지는 확정하지 않았다. Windows 볼륨 믹서 접근은 Computer Use 앱 권한에서 거부됐고 Editor 창 활성화도 실패해 OS별 출력 상태는 검증하지 못했다. 본 Editor 오디오/재생 상태를 변경했다고 주장하지 않는다.
- 근거: Library/CodexGrenadeEquip/{focused,full,audio-muted,mute-scope}.xml/log 및 audio-guard.txt.
- 수류탄 물리·피해·신관·탄수·클립·Animator·씬·카메라 수치·오디오 소스/프로필·프로젝트 Audio 설정은 그대로다.
- 이번 입력 변경 후 Windows Player 빌드는 재실행하지 않았다. 사용자 Editor에서 Play를 재시작해 실제 조작감·모션 연결을 확인해야 한다.

마스터 음소거 API 근거: [Unity 공식 C# 참조](https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/EditorUtility.bindings.cs). 기존 대화의 4 유지/해제 설명보다 이 문서의 현재 동작이 우선한다. 커밋하지 않았다.
