# 우클릭 수류탄 오버핸드·언더핸드 전환

2026-11-08. 모션 OFF 상태를 유지하고 투척 방식 선택 기능만 추가했다.

## 변경 전 비교

직전 `Library/CodexGrenadeFunctionOnly/applied.json`의 10개 파일이 모두 일치했다. 기존 투척은 한 종류였고 LaunchSpeed18, UpwardSpeed4, StandingHeight×.3 높이와 시선 방향×.6의 발사 위치를 사용했다. 기존 우클릭은 PlayerCharacterInput의 무기 AimHeld/AimPressed/AimReleased 경로였고 수류탄 중에는 View에서 무기 조준을 막았다.

기존 입력 액션/바인딩을 추가·교체하지 않고 Aim 누름을 수류탄 방식 전환에도 사용한다. 오버핸드 수치와 기존 피해·도화선·모션 OFF·왼쪽 버튼 해제 투척은 보존했다.

## 조작과 데이터

- 4로 새 수류탄을 장착하면 Overhand로 시작.
- 장착한 동안 우클릭을 누를 때마다 Overhand ↔ Underhand. 왼쪽 버튼 유지 중에도 즉시 전환.
- 왼쪽 버튼을 누르는 동안 선택한 방식의 궤적 표시. 놓으면 그 방식으로 실제 발사.
- 전환과 왼쪽 버튼 놓기가 같은 프레임이면 바뀐 방식으로 발사.
- 4/다른 무기 취소와 전환·해제가 동시에 들어오면 취소 우선.
- 우클릭을 계속 누른다고 반복 전환하지 않음. 수류탄 미장착/투척 예약 후에는 전환 거절.
- 선택에 사용한 우클릭은 물리적으로 놓을 때까지 무기 Aim 입력에서 소비해 복귀 후 조준/ADS로 새지 않음. 새 우클릭은 평소처럼 동작.
- HUD에 OVERHAND / 위로 또는 UNDERHAND / 아래로와 RMB 안내 표시.

| 방식 | 전방 속도 | 추가 상향 속도 | 발사 높이(캐릭터 루트 기준) |
|---|---:|---:|---|
| Overhand | 기존18 | 기존4 | 기존 StandingHeight×.3 |
| Underhand | 신규8 | 신규1.5 | StandingHeight×.15 |

두 방식 모두 시선 방향과 모터 이동 속도를 반영한다. 숫자는 이 프로젝트의 테스트 설정이며 배그의 실제 수치를 측정/복제한 것이 아니다. 피해·도화선·탄체·중력과 첫 충돌까지 예측하는 궤적 방식은 그대로다. 별도의 수류탄 모션은 재생하지 않는다.

## 모든 변경 파일

| 파일 | 변경 |
|---|---|
| `Assets/Features/PlayerCharacter/Scripts/IPlayerCharacterThrowInput.cs` | ToggleGrenadeThrowModePressed와 ConsumeAimUntilRelease 추가. 기존 계약 유지 |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterInput.cs` | 기존 Aim 액션의 WasPressedThisFrame을 전환 명령으로 제공. `_suppressAimUntilRelease`가 선택용 우클릭을 소비하고 기존 AimHeld/Pressed/Released를 그동안 차단. 물리 해제 후 복구 |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterGrenadeThrow.cs` | GrenadeThrowMode와 읽기 전용 ThrowMode 추가, 새 장착 시 초기화, TryToggleThrowMode 추가. Update는 취소→전환→발사 순서이며 취소와 겹친 우클릭도 소비. GetLaunch의 기존 한 방식 계산을 방식별 속도·높이 선택으로 교체. 예측과 실제 발사는 계속 같은 함수를 사용 |
| `Assets/Features/Weapon/Scrips/GrenadeDefinition.cs` | `_underhandLaunchSpeed=8`, `_underhandUpwardSpeed=1.5`와 읽기 전용 속성 추가. 기존 설정 보존 |
| `Assets/ThirdParty/MixamoLocal/TacticalActions/FragGrenade.asset` | 위 신규 두 값만 명시적으로 추가 |
| `Assets/Features/UI/PlayerCombatHud.cs` | 수류탄 표시 줄에 선택 방식 추가, 조작 안내에 RMB 위로/아래로 추가. 기존 무기·탄약·상태 안내 유지 |
| `Assets/Tests/PlayMode/TacticalActionTests.cs` | 입력 대역에 새 계약 추가. 실제 입력 분기를 통한 양방향 전환, 낮고 약한 발사, 표시/실제 일치, 장착 초기화, 해제 프레임 전환 및 동시 취소 검사 추가 |
| `Assets/Tests/PlayMode/PlayerCharacterCombatInputTests.cs` | 실제 Input System 우클릭 매핑/소비 검사. 누른 상태에서 복귀하거나 우클릭을 놓아도 무기 ADS가 켜지지 않고 새 클릭은 정상 동작하는 회귀 검사 추가 |
| `Docs/AI/UnityProjectContext.md` | 최신 조작·수치·검증 상태 기록 |
| 이 문서 | 변경 전후와 설계·검증 근거 기록 |

삭제한 에셋·필드·메서드·컴포넌트·참조 없음. 교체한 구현은 단일 발사 조건 계산, Aim 입력의 소비 여부 판단, HUD 텍스트 구성이다.

## 구조 검토

투척 방식은 기존 수류탄 상태의 일부이므로 그 컴포넌트 안에서 관리했다. 속도 설정은 기존 GrenadeDefinition, 입력은 기존 좁은 인터페이스/어댑터, 표시는 기존 HUD, 궤적은 기존 GrenadeTrajectoryView의 책임을 유지한다. 상속/새 관리자/범용 전략 객체를 추가하지 않았다. 현재 두 모드의 차이는 수치와 높이 선택이므로 별도 정책 클래스를 만들 필요가 없다. OOP/SOLID 검토상 의도적 예외 없음.

## 검증

격리 `-noaudio` 관련 PlayMode18/18 통과(13.9938195초). 이후 취소와 전환 동시 입력에서 무기 Aim 소비도 적용하는 분기를 보강했고, 최종 전체 PlayMode214/214 통과(203.2645065초), 실패/스킵0. 이 수에는 임시 캡처1개를 포함한다. 결과: `Library/CodexGrenadeThrowModes/full.xml` 및 `full.log`. TEST_AUDIO는 이전 False / Play 중 True / 종료 후 False를 기록했다. 컴파일 오류 없음.

임시 두 방식 비교 캡처는 격리 Assets에서만 실행하고 종료 후 소스/메타를 Library/CodexGrenadeThrowModes로 이동했다. 1·3인칭 Overhand/Underhand/Overhand 복귀6장을 생성하고 대표 이미지에서 높은 포물선과 낮고 짧은 궤적 차이를 확인했다. 제품 코드·설정8개와 검증본 SHA256 일치 확인, 변경10개 파일 스냅샷: `Library/CodexGrenadeThrowModes/applied.json`. 원본 씬/계층/카메라/오디오/애니메이션 클립 변경 없음. Windows 빌드와 실제 투척 거리의 사용자 조정은 별도다. 커밋 없음.
