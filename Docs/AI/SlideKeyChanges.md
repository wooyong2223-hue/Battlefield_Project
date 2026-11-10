# C 슬라이드 입력 분리

직전 Mixamo 적용 기록15개 파일 해시와 현재 파일을 비교했고 사용자 추가 변경은 없었다. 대상8개 파일의 원본 해시도 별도 보존 후 재확인했다. 기존 구조/설정과 겹치는 변경 없음.

## 파일별 변경

| 파일 | 변경 및 이유 |
|---|---|
| `Assets/InputSystem_Actions.inputactions` | PlayerCharacter 맵에 Button `Slide`, `<Keyboard>/c` 바인딩 추가. 기존 Crouch/LeftCtrl과 Fighter ChangeCamera/C는 유지. 달리기 입력과 슬라이드 진입 검사는 기존 이동 계층에서 수행 |
| `Assets/Features/PlayerCharacter/Scripts/IPlayerCharacterTraversalInput.cs` | `SlidePressed` 읽기 전용 속성 추가. 기존 `PronePressed` 유지 |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterInput.cs` | `_slide` 액션 참조/FindAction과 `SlidePressed => HasControl && _slide != null && _slide.WasPressedThisFrame()` 추가. 탑승/포커스 해제 시 기존 HasControl로 차단 |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterTraversal.cs` | `_input`의 계약과 GetComponent를 `IPlayerCharacterStanceInput`→`IPlayerCharacterTraversalInput`으로 교체. 진입 조건은 아래 비교 참조 |
| `Assets/Tests/PlayMode/PlayerCharacterInfantryTests.cs` | 슬라이드 가짜 입력을 새 계약/SlidePressed로 변경. 슬라이드 검사는 사전 강제 앉기를 없애고 서서 달리기→입력→자동 앉기→종료 앉기를 검증. 검사 이름도 SprintSlideInput으로 변경 |
| `Assets/Tests/PlayMode/PlayerCharacterTraversalAnimationTests.cs` | 가짜 입력 계약/속성만 새 SlidePressed로 변경. 기존 모션 단계/접지/종료 기준 유지 |
| `Assets/Tests/PlayMode/SlideAnimationOverrideTests.cs` | 가짜 입력 계약/속성만 새 SlidePressed로 변경. 기존 무기 override/모터 기준 유지 |
| `Assets/Tests/PlayMode/PlayerCharacterCombatInputTests.cs` | 실제 Keyboard 이벤트로 C→Slide만, Ctrl→Crouch만 확인. 탑승 제어 중 C도 차단되는지 기존 검사에 추가 |
| `Docs/AI/SlideKeyChanges.md` | 이 변경 기록 |
| `Docs/AI/UnityProjectContext.md` | 최신 조작키와 검증 결과 추가 |

## 기존 코드 교체

```cs
// 이전: 앉기 입력을 먼저 받아 낮아진 상태에서 슬라이드 시작
_input.CrouchPressed && _movement.IsSprinting && _stance.IsCrouched && _body.isGrounded
// 변경: 별도 슬라이드 입력을 받아 진입할 때만 자세 변경
_input.SlidePressed && _movement.IsSprinting && _body.isGrounded && _stance.TrySetCrouched(true)
```

따라서 Ctrl은 달리는 중에도 일반 앉기이며 C는 달리는 지상 상태에서만 슬라이드한다. 서 있거나 공중에서 C를 누르면 슬라이드/앉기 동작을 시작하지 않는다. 슬라이드0.75초/감속값/종료 앉기/모션/카메라/전투기 C 시점 전환 유지. 컴포넌트/에셋/메서드 삭제 없음. `_input`의 타입·획득 계약과 위 조건만 기존 구현을 교체했다. 새로운 씬/컴포넌트/메타 파일 없음.

입력 책임은 기존 Input, 자세는 Stance, 진입 조건/이동은 Traversal에 유지했다. 같은 traversal 입력 계약에 의도적인 진입 명령을 추가해 기존 앉기 소비자와 분리했으며 별도 manager/상속 계층은 만들지 않았다. 상태 직접 변경 대신 기존 TrySetCrouched 사용. OOP/SOLID 검토상 추가 예외 없음.

## 검증

Unity6000.4.8f1 격리 프로젝트 `-batchmode -noaudio`: 관련 PlayMode **35/35 통과** (18.5초). 실제 키 이벤트, 탑승 입력 차단, 자세/슬라이드/바닥 접촉/무기 모션 회귀 포함. 전체181개 검사는 이번 키 변경에서 다시 실행하지 않았다. 기존에181/181 통과한 기준에서 영향 범위 검사를 선택했다. 기존 검사 허용오차 완화 없음. 제품 복사8개 파일은 검사 프로젝트와 바이트 동일. 커밋/스테이징/사운드 설정 변경 없음. 실제 사용자 Editor의 Shift+W→C 조작감과 키 충돌 체감은 확인 필요.

증거: `Library/CodexSlideKey/{before.json,focused.xml,focused.log,applied.json}`.
