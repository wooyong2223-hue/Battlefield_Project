# 상체 조준 기준 공유 — PUBG 3인칭 참고 방향 (2026-10-28)

사용자는 상체 조준 표현을 배틀그라운드 3인칭 기준으로 맞추도록 지정했다. 이번에는 기존 3인칭의 몸통/머리 조준 추적을 월드 캐릭터 몸의 공통 기준으로 삼고, 1인칭 전환 때문에 그 자세가 풀리는 부분을 수정했다. 1인칭 화면용 팔·총은 별도 리그로 계속 표현한다. Q/E와 기존 카메라/입력 설정은 유지한다.

PUBG의 [공식 Animation Refresh 개발자 글](https://www.pubg.com/en/news/1565)은 FPP와 TPP 표현을 구분하지만 본별 상하 조준 가중치나 구현 코드를 공개하지 않는다. 이 작업에서 PUBG의 본별 회전 비율을 측정하거나 동일한 모션 에셋을 재현한 것은 아니다. **현재 프로젝트의 검증된 3인칭 몸통/머리 자세를 시점과 독립시킨 변경**이다. 전용 포즈의 추가 조정은 실제 모델을 보며 별도 진행한다.

## 변경 전 비교와 확인한 원인

WeaponAimView/FirstPersonArmsView/View/CombatTests/SampleScene이 직전 검증본과 같았다. 기준 해시를 `Library/CodexPubgBodyAim/baseline.json`과 `Baseline`에 보관하고 원본 반영 직전 다시 확인했다. 새 사용자/Editor 변경과의 충돌은 없었다.

3인칭에는 이미 Base Layer LookAt의 몸통/머리 추적이 있었다. 그러나 기존 `CanAim`은 1인칭을 제외하면서 손 IK와 몸통 추적 가중치에 함께 사용됐고, ApplyUpperBodyAim도 1인칭에서 즉시 가중치를 0으로 했다. 수정 전 검사에서 위 60도 조준 후 1인칭으로 바꾸면 가슴 회전이 **29.605도** 달라지는 현상을 재현했다.

## 변경한 모든 파일

1. `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterWeaponAimView.cs`
   - 신규 private 계산 속성 `CanTrackBody`: 기존 생존/무장/장전·근접/동작 정책 조건으로 몸통 추적 가능 여부를 판단한다.
   - 기존 `CanAim` 표현을 `CanTrackBody && !_view.IsFirstPersonView`로 교체한다. 기존 손 IK의 3인칭 제한은 여기 남는다.
   - ApplyUpperBodyAim의 `if (!_combat.CanUseCombat || _view.IsFirstPersonView)`를 `if (!_combat.CanUseCombat)`로 교체한다.
   - `_bodyBlend`의 목표를 `CanAim`에서 `CanTrackBody`로 교체한다. 1인칭에서도 실제 몸통/머리가 기존 3인칭 조준 방향을 유지한다.
   - Base Layer에서 한 번 저장하는 `_aimRotation`은 그대로 사용한다. Alt 자유 시점으로 실제 몸을 카메라 쪽으로 돌리지 않는다.
2. `Assets/Tests/PlayMode/PlayerCharacterCombatTests.cs`
   - `WorldBodySharesThirdPersonAimAcrossPerspectiveChangesWithoutTiltingTheLegs`: 서기/앉기, 상하 60도에서 시점 전환 후 가슴/머리 차이 3도 미만, 골반/양다리 로컬 회전 차이 1도 미만, 캐릭터 루트/모델 회전 불변을 검사한다.
   - `FirstPersonFreeLookKeepsTheWorldUpperBodyOnTheWeaponAim`: 1인칭에서 Alt로 시선을 돌려도 실제 상체가 무기 조준 방향을 유지하는지 검사한다.
3. `Docs/AI/PlayerCharacterSharedUpperBodyAimChanges.md`: 이 변경/대체/검증 보고서.
4. `Docs/AI/UnityProjectContext.md`: 최신 상체 추적 정책 기록.

기존 메서드/필드/컴포넌트/참조/에셋 삭제는 없다. 교체한 것은 위 조건식과 가중치 목표 선택뿐이다. Scene/Prefab/AnimatorController/AvatarMask/클립/메타는 수정하지 않았다. 몸통 0.3, 머리 0.85, clamp 0.2와 자세 진입/해제 속도 12, 손 그립/도달 범위, 카메라/FOV, 직전 Q/E 카메라 롤 제거를 모두 유지한다. 기존 1인칭 가시성 정책도 유지한다.

```csharp
// 이전: 손과 몸통에 동일한 시점 제한을 사용
private bool CanAim => /* 동작 가능 조건 */ && !_view.IsFirstPersonView;
if (!_combat.CanUseCombat || _view.IsFirstPersonView) { /* 몸통 추적 중단 */ }

// 변경: 월드 몸통과 화면용 손의 시점 요구를 구분
private bool CanTrackBody => /* 기존 동작 가능 조건 */;
private bool CanAim => CanTrackBody && !_view.IsFirstPersonView;
if (!_combat.CanUseCombat) { /* 몸통 추적 중단 */ }
```

## 구조 검토

몸통/머리와 양손 조준은 같은 조준 방향을 공유하는 기존 WeaponAimView의 표현 책임이다. 새 수명이나 독립 상태가 필요하지 않아 새 컴포넌트 대신 기존 클래스에서 조건만 분리했다. 상태는 private로 유지하고 기존 직렬화 의존성과 동작 정책을 재사용한다. 불필요한 인터페이스/상속/관리자를 추가하지 않았다. OOP/SOLID의 의도적 예외는 없다.

## 검증과 남은 확인

- 수정 전 새 검사 2개 중 1개 실패: 1인칭 전환 시 가슴이 29.605도 풀리는 현상을 확인했다 (`before.xml/log`).
- 최종 제품 PlayMode **111/111 통과**, 60.56초 (`verified.xml/log`). 기존 상하 조준/그립·장전·근접·시점/카메라·팀 판정·탑승·소생 검사 포함.
- 별도 렌더 검사 **1/1 통과**, 3.77초 (`render.xml/log`). 서기/앉기 0도 및 상하 60도와 실제 1인칭 상하 무기 화면을 확인했다.
- 캡처: `Library/CodexPubgBodyAim/stand--60.png`, `stand-60.png`, `crouch--60.png`, `crouch-60.png`, `first-person--60.png`, `first-person-60.png` 등. 렌더 전용 코드는 제품에서 제외하고 같은 Library 폴더에 보관했다.
- 원본 스크립트/검증본 일치, Scene/FirstPersonArmsView/View 해시 불변, 수정 C# Git 공백 검사 통과 (`final-audit.json`). 이번 실행 로그에서 AABB 오류는 재현되지 않았다. 이전 간헐적 오류를 해결했다는 뜻은 아니다.
- 실제 Unity Editor에서는 SampleScene을 다시 열고 3인칭에서 위아래 조준 → 같은 방향으로 V 전환 → Alt 자유 시점 → 앉기/장전/복귀 순서로 실시간 연결감을 확인해야 한다. 현재 무료 모델의 포즈와 PUBG 실제 캐릭터의 시각적 일치, 전용 자세 세부 조정은 별도 확인이 남는다. Player 빌드/커밋/푸시는 수행하지 않았다.
