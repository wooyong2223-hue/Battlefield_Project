# 포복 속도·진입/해제 이동 잠금 변경 — 2026-11-01

사용자 요청: 포복 진입/해제 속도 증가, 포복 이동 속도 증가, 전환 도중 이동 금지.

## 사전 비교
이전 적용 기록 Library/CodexProneApply/applied.json과 비교했으며 관련 파일의 사용자/Editor 후속 변경은 없었다. 수정 전 9개 파일은 Library/CodexProneSpeed/before.json 및 Before/에 보관했다. 격리 프로젝트에서 구현/검증한 파일만 본 프로젝트로 복사한다.

## 바뀐 모든 스크립트·에셋
모든 경로는 E:/GitHub/Battlefield_Project/ 기준이다.

| 파일 | 구체적인 코드·직렬화 값 / 이유 |
|---|---|
| Assets/Features/PlayerCharacter/Scripts/PlayerCharacterStance.cs | `_proneMoveSpeed` 기본값 2→3. `_proneEnterDuration=.55`, `_proneExitDuration=.45`, 전환 시작 시간/지속 시간 추가. TrySetStance에서 성공한 포복 진입/해제에만 타이머 설정. IsProneTransitioning/ProneTransitionProgress 및 두 지속 시간을 읽기 전용으로 노출. 기존 OnDisable에 타이머 초기화를 추가하고 기존 서 있는 캡슐/자세 복원은 유지. 자세 상태와 전환 시간의 단일 소유자. |
| Assets/Features/PlayerCharacter/Scripts/PlayerCharacterMovement.cs | Tick에서 IsProneTransitioning이면 이동 입력·평면 관성·대기 중 외부 수평 변위를 0으로 처리하고 sprint/jump를 막으며 진행 중 traversal을 취소한다. CharacterController의 중력·접촉/충돌 처리 루프는 유지한다. 첫 프레임의 잔여 관성과 전환 도중 슬라이드/도약으로 움직이는 경로를 막기 위함. |
| Assets/Features/PlayerCharacter/Scripts/PlayerCharacterProneAnimationView.cs | 별도 `_stageTime` 필드와 그 증가/초기화 코드를 제거. 원본 길이에 도달하면 전환을 끝내던 조건을 Stance.IsProneTransitioning으로 교체. 진입/해제 클립 시간은 `ProneTransitionProgress * clip.length`. 기존 해제의 원본 길이 뒤 추가 `.08초` 대기도 제거. 같은 상태 타이머로 재생과 이동 해제를 동기화. 이동 클립(앞/뒤/좌/우) 시간만 CrawlPlaybackSpeed 배율로 재생하며 대기/발사/장전/사망 시간은 유지. |
| Assets/Features/PlayerCharacter/Scripts/ProneAnimationSet.cs | private SerializeField `_crawlPlaybackSpeed=1.5`와 읽기 전용 CrawlPlaybackSpeed 추가. 실제 이동 속도를 1.5배 올린 데 맞춰 기어가는 원본도 빠르게 재생. 기존 클립 선택/참조 보존. |
| Assets/Scenes/SampleScene.unity | PlayerCharacterStance 문서(fileID 950000018)에서 `_proneMoveSpeed: 2→3`, `_proneEnterDuration: 0.55`, `_proneExitDuration: 0.45`만 변경/추가. 오브젝트 수/이름/계층/위치 및 그 외 값은 그대로. |
| Assets/ThirdParty/MixamoLocal/Prone/ProneAnimations.asset | `_crawlPlaybackSpeed: 1.5` 추가. 기존 모든 클립·마스크·GUID 참조 유지. |
| Assets/Tests/PlayMode/ProneAnimationTests.cs | 진입 첫 프레임부터 입력·관성/대기 외부 변위가 막히고, .55초/.45초 전환이 완료되며 종료 후 이동이 재개되는지 검사. 실제 포복 이동 거리가 설정 속도에 맞는지도 검증. 기존 조준·좌우 원본·Root Motion OFF·근접/장전·비활성화·해제 검증 유지. |
| Assets/Tests/PlayMode/PlayerCharacterInfantryTests.cs | 충돌/캡슐 전용 테스트 helper는 진입/해제를 0초로 설정해 원래 충돌 검사 의미를 유지하고, 이동 거리 기대값은 MoveSpeed*.25로 계산. 전환 이동 잠금은 위 PlayMode 테스트가 별도 검증. 실제 SampleScene 자세 검사는 새 Stance 진입 시간+.12초를 기다리도록 교체. |
| Assets/Tests/PlayMode/PlayerCharacterReloadPoseTests.cs | 기존 원본 클립 길이+.12초 대기 대신 Stance.ProneEnterDuration+.12초 대기. 실제 전환 완료 뒤 포복 장전 자세를 확인. 허용 범위 변경 없음. |
| Docs/AI/ProneSpeedAndTransitionChanges.md | 이 변경의 전후 코드/값·검증·설계 기록(신규). |
| Docs/AI/UnityProjectContext.md | 최신 포복 전환 소유권·속도·검증 근거 추가. |

## 동작 전후
- 이전: 원본 클립 길이로 진입/해제하여 모션별 시간이 달랐고, 동작 중에도 수평 이동 가능. 포복 이동 2m/s, 기어가는 원본 1배.
- 이후: 진입 .55초, 해제 .45초에 원본 전체 구간을 재생. 두 전환 동안 수평 이동/새 점프/달리기 잠금, 중력·충돌 유지. 완료 뒤 입력에 따른 이동 재개. 포복 이동 3m/s, 이동 모션 1.5배.
- 원본 FBX/애니메이션 키프레임/아바타/클립 참조·카메라·무기 데이터는 수정하지 않았다. 포복 속도 값은 기존 Stance Inspector, 이동 모션 배율은 기존 ProneAnimations 프로필에서 조정한다.

## 삭제·교체 항목
제품 코드에서 제거한 기존 필드는 표현 컴포넌트의 `_stageTime` 하나다. 그 타이머 증가/리셋 및 원본 길이 비교·해제 뒤 .08초 대기를 Stance의 전환 진행률/종료 판단으로 대체했다. 기존 메서드·컴포넌트·참조·에셋 삭제는 없다. 이동 Tick의 기존 처리 앞에 전환 잠금 조건을 추가했다.

## 구조 검토
별도 관리자를 추가하지 않았다. 자세 변경과 전환 상태는 기존 Stance 책임에 포함된다. 이동은 읽기 전용 전환 상태만 소비하고, 재생은 동일 진행률에 원본을 맞춘다. 데이터 배율은 기존 프로필이 소유한다. private 상태와 명시적 기존 참조를 사용하며 불필요한 인터페이스/상속을 추가하지 않았다. OOP/SOLID 의도적 예외 없음. 표현과 이동에서 서로 다른 전환 시계를 운용하던 중복을 제거했다.

## 검증
- Unity 6000.4.8f1 격리 프로젝트에서 `-batchmode -noaudio` 전체 PlayMode 186/186 통과, 실패/스킵 0, 180.71초.
- 진입 첫 프레임 이동/대기 외부 수평 변위 잠금, 설정된 .55초/.45초 전환, 완료 후 이동 재개, 설정 속도에 맞는 포복 변위, 기존 좌우/조준/장전/근접/탑승·사격·슬라이드 회귀 검사 포함.
- 처음 추가한 OnDisable 중복 컴파일 오류는 기존 OnDisable 내부로 타이머 초기화를 합쳐 해결했다. 최종 컴파일 및 테스트 실행에는 해당 오류가 없다.
- 수정한 모든 제품 파일을 격리 검증 소스와 동일한 해시로 반영했다. Scene diff는 해당 Stance 문서의 속도 1개 변경과 전환 지속 시간 2개 추가뿐이다. 원본 FBX/클립 곡선 변경 없음.
- 남은 확인: 본 Editor SampleScene에서 사용자가 원하는 속도감, 1/3인칭 전환의 시각적 자연스러움과 경사면에서의 감각. 별도 대상 플랫폼 빌드는 실행하지 않았다.

최종 해시: Library/CodexProneSpeed/applied.json. 기존 사용자 수치 중 위에서 명시한 포복 이동 속도 외의 값은 유지했다. 새 클래스/에셋/씬 및 .meta 추가는 없다.
