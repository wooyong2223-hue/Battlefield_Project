# Q/E 카메라 기울기 제거 — 2026-10-28

요청 범위는 Q/E에 따른 카메라 좌우 기울기(롤) 제거다. 옆으로 몸을 내미는 위치 이동과 몸통 포즈는 유지했다. 보행에 따른 별도의 카메라 바운스/롤 설정은 변경하지 않았다.

변경 전 View/CameraTests는 직전 검증본과 같았고, SampleScene은 YAML 오브젝트 순서만 달랐으며 ID별 값은 같았다. 원본 반영 직전에도 기준 해시를 확인했다. 새로운 사용자/Editor 설정 변경과 충돌하지 않았다.

## 변경 파일 전체

1. `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterView.cs`
   - 삭제: `[SerializeField] private PlayerCharacterLean _lean;`
   - 삭제: `private float Roll => _lean != null ? _lean.Roll : 0f;`
   - 교체: AimRotation과 LookRotation의 세 Quaternion.Euler 경로에서 마지막 인수 `Roll` → `0f`.
   - 기존 Q/E Amount × angle이 조준/시점 Z 회전에 전달되던 연결을 제거했다. 마우스 좌우/상하, 자유 시점, 반동 계산은 유지한다.
2. `Assets/Scenes/SampleScene.unity`
   - View 컴포넌트 fileID `950000004`에서 `_lean: {fileID: 950000021}`만 제거했다. 삭제된 필드의 불필요한 참조 정리다.
   - Lean 컴포넌트, Camera/PostureView의 Lean 참조, 거리/각도 설정과 모든 오브젝트/transform은 유지한다. 몸통 기울기는 여전히 Lean.Roll을 사용하므로 Lean의 angle 필드는 삭제하지 않았다.
3. `Assets/Tests/PlayMode/PlayerCharacterCameraTests.cs`
   - `LeanKeepsTheCameraAndAimLevelInBothPerspectivesAndFreeLook` 검사 및 작은 전술 입력 대역을 추가했다.
   - 실제 Lean Amount가 좌우 각각 0.9 이상이고 위치 이동이 0.4 이상인 상태에서 1·3인칭/자유 시점의 카메라 및 조준 롤이 0인지 검사한다.
4. `Docs/AI/PlayerCharacterLeanCameraRollRemoval.md` — 이 변경/삭제/검증 기록.
5. `Docs/AI/UnityProjectContext.md` — 최신 사용자 선택을 추가 기록.

기능 책임은 기존 View의 시점 회전 계산에 남겼다. 새 컴포넌트·추상화·상속은 필요하지 않으며 OOP/SOLID의 의도적 예외는 없다. 기존 기울기 계산을 0으로 대체하고 그 입력 의존성을 제거했으므로 시점 책임이 더 작아진다.

## 검증

- 격리 Unity 6000.4.8f1 컴파일 및 CameraTests/ViewTests **12/12 통과**, 5.64초.
- 결과: `Library/CodexRemoveLeanRoll/verified.xml`, 로그: `verified.log`.
- 원본 스크립트/검증본 일치, 씬 ID별 직렬화 값 일치 확인. 씬 변경은 View의 참조 한 줄만이며 나머지 블록/값은 보존했다. 수정 C# Git 공백 검사 통과.
- 실제 Editor에서 SampleScene을 다시 열어 정지 상태의 Q/E와 Alt 자유 시점에서도 수평이 유지되는지 확인이 남는다. 전체 보병 검사/렌더링/Player 빌드는 이번 작은 변경에서 반복 실행하지 않았다. 커밋·푸시는 수행하지 않았다.
