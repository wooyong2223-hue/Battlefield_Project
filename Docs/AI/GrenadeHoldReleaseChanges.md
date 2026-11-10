# 수류탄 왼쪽 버튼 유지·놓기 조작

2026-11-04. 애니메이션 제작 작업을 보류하고 조작 기능만 수정했다.

## 변경 전 비교

직전 `Library/CodexGrenadeAuthored/applied.json`에 기록된 21개 파일의 SHA256이 작업 시작 시 모두 일치했다. 입력 인터페이스·PlayerCharacterInput·HUD는 `Library/CodexGrenadeEquip/applied.json` 기록과 일치했다. 작업 트리에는 기존의 다수 미커밋 변경이 있어 이번 기능에 관련된 파일만 수정했다. 원본 SampleScene, 무기 수치, 캐릭터/카메라 배치와 애니메이션 클립은 수정하지 않았다.

## 조작

- 4: 수류탄 장착. 장착 중 4를 다시 누르면 보관하고 이전 무기로 복귀.
- 왼쪽 버튼 누름: 투척 궤적 표시. 유지 중 수량 감소·실제 발사·도화선 진행 없음.
- 왼쪽 버튼 놓음: 기존 투척 애니메이션 시작. 실제 발사는 기존 ReleaseFraction 시점에 진행.
- 버튼 유지 중 4 또는 1/2/3: 투척 취소. 수량 유지, 궤적 숨김, 누르고 있던 버튼에 의한 총기 발사 차단.
- 취소 키와 버튼 놓기가 같은 프레임이면 취소 우선.
- 조준 중 포인터/게임플레이 제어 상실, 사망, 비활성화, 탑승 또는 외부 장비 교체도 기존 취소 경로로 정리.

## 변경 파일과 코드

| 파일 | 변경 |
|---|---|
| `Assets/Features/PlayerCharacter/Scripts/IPlayerCharacterThrowInput.cs` | `ThrowGrenadeHeld`, `ThrowGrenadeReleased`, `CanControlGrenade` 읽기 전용 계약 추가. 기존 멤버 유지 |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterInput.cs` | 기존 Attack 액션의 유지/해제/제어 가능 상태 제공. 총기 발사 소비 중에도 수류탄 해제는 읽을 수 있음. 액션 에셋/바인딩 변경 없음 |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterGrenadeThrow.cs` | 누름→즉시 TryThrow 호출을 TryBeginAim으로 교체. 해제 시 TryThrow. `_aiming` 상태 추가, IsAiming은 장착 전체가 아닌 실제 버튼 조준만 의미. 4번 토글 취소와 취소 우선 처리. GetLaunch로 예측·실제 발사의 위치/속도 계산 공유. Clear의 할당형 물리 검색을 32개 NonAlloc 버퍼로 교체하고 포화 시 안전하게 차단 |
| `Assets/Features/UI/GrenadeTrajectoryView.cs` 및 `.meta` | 신규 표시 전용 컴포넌트. LineRenderer, 최대 256점, 물리 고정 간격, 기존 수류탄 반지름 0.12, 중력과 이동 속도를 반영. 첫 충돌까지만 표시. 선 폭 0.035, 청록색. 기존 BulletTracer 재질을 독립 복제해 사용하며 원본 재질은 불변. 비활성화 시 숨기고 소유 재질은 파괴 시 정리 |
| `Assets/Features/PlayerCharacter/Editor/LocalTacticalActionSetup.cs` | 기존 Play/빌드 씬 복사본 설치 경로에서 궤적 컴포넌트와 LineRenderer 추가, 수류탄/재질 명시 연결. on-foot 중단 목록에 trajectory 추가. 원본 씬 저장 없음 |
| `Assets/Features/UI/PlayerCombatHud.cs` | `LMB THROW` 안내를 유지→조준 / 해제→투척 / 4·1·2·3→취소로 교체 |
| `Assets/Tests/PlayMode/TacticalActionTests.cs` | 기존 즉시 클릭 투척 기대를 유지/해제로 교체. 1·3인칭 유지·수량·궤적·단발 투척, 장착 도중 해제 예약, 4·각 무기·제어 상실 취소와 동시 해제, 오발 방지, 궤적 충돌·방향·비활성화 검증 |
| `Assets/Tests/PlayMode/PlayerCharacterCombatInputTests.cs` | 실제 Input System에서 발사 소비 후에도 수류탄 유지/해제 상태를 읽는지 검증 |
| `Docs/AI/UnityProjectContext.md` | 최신 조작과 검증 기록 추가 |
| 이 문서 | 변경 전후·구조·검증과 한계 기록 |

이전: `ThrowGrenadePressed -> TryThrow()` / `IsAiming = Ready && !throwRequested`.

이후: `ThrowGrenadePressed -> TryBeginAim()` / `IsAiming && ThrowGrenadeReleased -> TryThrow()`. `IsAiming = IsEquipped && aiming && !throwRequested`.

삭제한 에셋·직렬화 필드·기존 공개 메서드는 없다. 교체한 구현은 입력 분기, IsAiming의 의미, 중복 발사 조건 계산, Clear 내부 검색 방식, HUD 안내와 이에 대응하는 테스트다. 기존 TryThrow API와 애니메이션/수량/속도/도화선/피해량은 유지했다.

## 구조 검토

입력 어댑터는 입력, 수류탄 컴포넌트는 상태·발사, 궤적 컴포넌트는 표시만 소유한다. 기존 좁은 입력 인터페이스를 확장하고 명시적 참조를 주입했다. 상속이나 범용 관리자 추가 없음. 궤적 표시자가 단일 수류탄 구현을 직접 읽는 것은 현재 사용처가 하나라 추가 추상화가 필요 없다는 판단이다. OOP/SOLID 검토상 별도의 의도적 예외 없음.

## 검증과 한계

격리 프로젝트에서 `-noaudio` PlayMode: 기존 아군 더미 수류탄 검사 2/2 통과(4.4925653초), 관련 조작·궤적·클립·실제 Input System 검사 11/11 통과(9.1094415초). 실패/스킵 0. 처음 관련 검사에서는 LateUpdate 이전에 선의 표시 상태를 확인한 두 테스트가 실패했으며, 프레임 검사 시점을 수정한 후 11개 전체를 재실행해 통과했다. 컴파일 오류 없음. TEST_AUDIO는 이전 False / Play 중 True / 종료 후 False를 기록했다. 전체 208개 테스트와 Windows Player 빌드는 이번에 실행하지 않았다. 결과: `Library/CodexGrenadeRelease/dummy.xml`, `focused-final.xml` 및 각 로그.

궤적은 현재 중력과 속도에 의한 첫 충돌까지의 예상 경로다. 반사 이후 경로, 움직이는 장애물의 미래 위치는 예측하지 않는다. 버튼 해제부터 실제 발사까지 기존 클립의 지연이 있고 발사 순간의 현재 시선을 사용하므로 그 사이 시선을 돌리면 경로도 달라진다. 소스 모션 품질, 궤적 가독성과 실제 조작감은 사용자의 Unity Editor 확인이 남는다.
