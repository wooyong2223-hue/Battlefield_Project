# 탑승과 연결한 전투기 시동

2026-10-27. 직전 탑승 작업의 검증 복사본과 관련 코드·SampleScene·프리팹이 일치함을 확인한 후 작업했다. 기존 비행 튜닝·위치·계층·좌석·음원 프로필 값은 보존했다.

## 동작

빈 전투기는 시동이 꺼져 있다. 좌석이 사용 가능한 상태에서 캐릭터가 탑승하면 시동이 켜지고 기존 엔진 루프음이 재생된다. 하차하면 추진·애프터버너·엔진음과 애프터버너음·엔진 분사가 중단된다. 재탑승하면 재시동한다. Esc로 포인터만 해제할 때는 여전히 탑승 중이므로 시동을 유지한다. 파괴된 전투기는 조종사가 남아 있어도 시동이 꺼진다.

엔진 정지는 순간 정지가 아니다. 목표 추진 속도만 0으로 지우고 기존 Rigidbody 속도를 보존한다. 비행 중 하차하면 기존 관성과 중력/충돌 처리가 계속 적용된다. 별도 시동 시작/종료 효과음은 추가하지 않았으며 기존 루프음을 사용한다. VFX는 기존처럼 스로틀/애프터버너가 요구될 때 분사하되 시동이 꺼져 있으면 분사하지 않는다. 이미 방출한 입자는 기존 StopEmitting 동작에 따라 수명이 끝나며 사라진다.

## 변경 파일 전체

| 파일 | 구체적 변경·이유 |
|---|---|
| Assets/Features/Fighter/Scripts/Movement/FighterEngine.cs 및 .meta | 신규. 직렬화한 `_seat: VehicleSeat`, `_movement: JetMovement`를 사용한다. `Start`/`Update`에서 `_seat.IsAvailable && _seat.IsOccupied`를 시동 상태로 전달한다. 실행 순서 -25로 상호작용(-75) 이후, 일반 비행 입력·추진(0) 이전에 갱신한다. 컴포넌트 비활성화 시 시동을 끈다. 좌석 코드를 전투기 전용 기능으로 확장하지 않도록 별도 정책 컴포넌트로 분리했다. |
| Assets/Features/Fighter/Scripts/Movement/JetMovement.cs | `IsEngineRunning { get; private set; } = true`와 `SetEngineRunning(bool)` 추가. 끌 때 `_targetSpeed=0`, `_queuedEnergyLoss=0`, `_afterburner.Stop()`을 적용한다. Rigidbody 속도는 바꾸지 않는다. Update에서 시동이 꺼져 있으면 애프터버너 회복만 진행하고 추진 계산은 건너뛴다. FixedUpdate에서 시동이 꺼져 있으면 기존 무추진 관성/충돌 분기를 사용한다. `IsUnpowered`에 `!IsEngineRunning` 조건을 추가해 기존 JetPhysics가 중력을 적용하게 한다. 기본 true는 좌석 정책이 없는 독립 사용의 기존 동작을 보존한다. |
| Assets/Features/Fighter/Scripts/Audio/FighterEngineAudio.cs | 기존 정지 조건 `_health.IsDead || !_movement.isActiveAndEnabled`에 `|| !_movement.IsEngineRunning` 추가. 기존 StopPlayback으로 두 소스의 재생·볼륨과 크로스페이드 상태를 초기화한다. 기존 음원·믹스·프로필은 보존했다. |
| Assets/Features/Fighter/Scripts/VFX/JetEngineVfx.cs | Awake에서 같은 오브젝트의 JetMovement를 캐시하고 기존 `Throttle > 0 || Afterburner.IsActive` 앞에 시동 조건을 추가했다. JetMovement가 없는 기존 독립 VFX 사용은 기존 조건을 유지한다. 기존 이펙트 크기/앵커/배출 설정은 보존했다. |
| Assets/Features/Fighter/Prefabs/Fighter.prefab | 기존 루트에 FighterEngine 컴포넌트 하나를 추가했다. `_seat`는 기존 VehicleSeat(fileID 2963293008867098661), `_movement`는 기존 JetMovement(fileID 6082039962316065886)에 연결했다. 기존 컴포넌트/자식/설정값을 제거하지 않았다. SampleScene은 이 프리팹을 사용하므로 씬 파일은 수정하지 않았다. |
| Assets/Tests/PlayMode/FighterEngineTests.cs 및 .meta | 신규 3개 PlayMode 테스트. 빈 상태→탑승→Esc→하차→재탑승의 시동/루프 재생/음소거/분사 상태, 시동 정지 시 속도 보존 및 최소 속도로 인한 재시동 방지, 파괴 시 정지를 검증한다. 관성 검사에서는 테스트 준비용 FreezeAll을 해제한다. |
| Docs/AI/FighterEngineChanges.md | 이 변경 보고서 추가. |
| Docs/AI/UnityProjectContext.md | 현재 시동 정책과 관성 보존 설명 추가. |

기존 메서드·직렬화 필드·컴포넌트·참조·에셋의 삭제는 없다. 기존 IsUnpowered 식과 Update/FixedUpdate, 음원/VFX 조건만 시동 상태를 반영하도록 확장했다. StopImmediately()는 시동 종료에 사용하지 않았다.

## 설계와 검증

FighterEngine은 좌석에 따른 시동 정책만, JetMovement는 추진 상태만, Audio/VFX는 표현만 담당한다. 합성과 명시적 직렬화 참조를 사용했다. 현재 시동 구현과 추진 구현이 각각 하나이므로 별도 인터페이스/상속 계층을 추가하지 않았다. 이 범위에서 구체 컴포넌트 참조가 불필요한 추상화를 줄인다. 기존 VehicleSeat/캐릭터/카메라 책임은 변경하지 않았다.

분리한 동일 Unity 버전 프로젝트에서 기존 38개 및 신규 3개, 총 41개 PlayMode 테스트가 모두 통과했다. 최종 결과 파일은 Temp/codex-engine-final.xml, 로그는 Temp/codex-engine-final.log다. 원본/검증본 코드·프리팹 일치, 씬/프리팹 로컬 참조·ID, 신규 스크립트 .meta 존재와 git diff --check도 통과했다. 사용자 Editor에서 실제 엔진음 청취·분사 잔광·비행 중 하차 체감은 추가 확인이 필요하다. 자동 테스트의 재생 상태/볼륨 검사는 청취 품질 확인을 대신하지 않는다.
