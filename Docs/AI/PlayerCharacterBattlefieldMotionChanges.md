# 배틀필드 영상 참고: 기본 이동·1인칭 화면 표현 — 2026-10-28

이번 단계는 **기본 이동 반응과 1인칭 카메라·팔·총의 연동**이다. 배틀필드 보병 전체를 완성했거나 원본의 내부 수치를 복제했다는 의미는 아니다. 컴퓨터 UI 조작 없이 공개 영상을 내려받아 프레임을 추출하고, 격리 Unity 프로젝트에서 구현·검사·렌더링한 뒤 원본에 부분 반영했다.

## 영상에서 확인한 것과 구현 판단

| 자료 | 확인 구간/내용 | 사용 범위 |
|---|---|---|
| [Maxiq: Battlefield 6 Has NEW Movement!](https://www.youtube.com/watch?v=LdPSuJzm93Q), 2025-08-03 | 52초 전체를 4초 간격, 00:01–00:04 / 00:09–00:12를 세분해 확인. 장전·이동·조준으로 무기 화면 구도가 바뀐다. | 베타 시기 돌격소총 영상. 장전 동작을 달리기 동작으로 혼동하지 않으며 현재 권총 각도/속도의 측정 근거로 쓰지 않는다. |
| [aksu02: BF6 Advanced Movement Guide](https://www.youtube.com/watch?v=BWMlR_ViPgA), 2025-10-26 | 00:14–00:20 이동·점프·슬라이드, 03:48–03:54 방향 전환, 05:50–05:56 착지/조준 및 권총 장면. 권총 구간은 8fps 접촉 시트로도 확인했다. | 출시 이후 실제 플레이와 입력 표시. 공중/착지/조준에 따라 시점과 무기 자세가 달라진다는 표현 참고. 가이드에 등장하는 버그성 이동 기술은 이번에 구현하지 않는다. |
| [EA 공식 접근성 자료](https://www.ea.com/able/resources/battlefield-6) | 1인칭 달리기 카메라 바운스를 줄이는 설정이 존재한다. | 보행/달리기 피드백을 조정 가능한 데이터로 분리한 근거. |

관찰과 구현값을 구분한다. 아래 가속도·진폭·각도는 영상에서 역산한 배틀필드 수치가 아니라 **현재 높이 4인 테스트 캐릭터에 맞춘 프로젝트 조정값**이다. 영상만으로 입력 지연, 실제 월드 속도, 본 회전축 또는 원본 물리 코드를 알아냈다고 주장하지 않는다. 참조 영상은 현재 라이브 패치와 다를 수 있다.

원본 영상/메타데이터/접촉 시트는 `Library/CodexBattlefieldReference`에만 보관한다. 제품 Assets에 배틀필드 영상·모델·애니메이션을 포함하지 않는다.

## 변경 전 비교와 보존

수정 전 현재 Movement/Camera/FirstPersonArms/CombatView/SampleScene을 직전 검증본과 비교했고 모두 일치했다. 변경할 애니메이션 표현 및 기존 검사도 원본을 별도로 보관했다. 최종 반영 직전 모든 기준 파일 해시를 다시 확인하여 새 사용자/Editor 변경이 없는 것을 확인했다. 근거: `Library/CodexBattlefieldFeel/baseline.json`, `Baseline`.

사용자가 정한 걷기/달리기 최고 속도 8/12, 점프 높이 1.2, 높이 4 캡슐, 캐릭터/모델/전투기 크기·위치, 기존 카메라 구도와 FOV 60/50/40, 팔의 기존 offset/ADS offset/poseEuler, 별도 탄창·근접 공격 오브젝트와 참조를 보존했다. 마우스 입력/감도, V 전환, Alt 자유 시점, 우클릭 유지 견착/탭 ADS도 바꾸지 않았다.

## 수정한 런타임 파일과 교체 내용

### `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterMovement.cs`

기존에는 입력 방향에 최고 속도를 즉시 곱했다.

```csharp
// 이전
Vector3 displacement = planar * deltaTime + _externalDisplacement;
public Vector3 Velocity => _body != null ? _body.velocity : Vector3.zero;

// 변경
_planarVelocity = Vector3.MoveTowards(_planarVelocity, planar, response * deltaTime);
Vector3 displacement = _planarVelocity * deltaTime + _externalDisplacement;
_resolvedVelocity = (transform.position - started) / deltaTime;
public Vector3 Velocity => _resolvedVelocity;
```

직렬화 필드 `_acceleration=80`, `_deceleration=100`, `_airAcceleration=24`를 추가했다. 단위는 월드 단위/초²다. 평지에서 정지→8은 약 0.10초, 정지→12는 약 0.15초, 12→정지는 약 0.12초에 도달하는 설정이다. 실제 충돌·입력/프레임 간격에 따라 거리는 달라진다. 낮은 자세에서는 그 자세의 최고 속도로 즉시 상한을 제한한다.

Motor가 수평 속도를 소유하고 짧은 가속/제동·방향 전환 관성을 만든다. 공중에서는 낮은 가속도로 방향을 바꾼다. 충돌 후 속도는 전달받은 Tick 시간으로 계산하여 카메라/애니메이션/슬라이드가 같은 기준을 읽는다. 벽에 막히면 실제 도달한 접선 방향 속도를 유지하여 벽 안으로 계속 가속하는 것을 막는다. 외부 끌기 변위는 이 보정에서 제외한다.

`Stop()`에 수평/관측 속도 초기화를 추가했다. 비활성화·탑승 시 운동량이 남지 않으며, 사망 시 자기 이동은 즉시 멈추되 기존 외부 끌기 변위는 처리한다. Traversal이 이동을 소유하는 동안도 자기 수평 속도를 지우고 실제 통과/슬라이드 속도만 기록한다. 기존 점프·중력·충돌 권한은 이 Motor와 CharacterController에 남는다.

### `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterAnimationView.cs`

기존 `IsSprinting ? SprintSpeed : MoveSpeed`와 `* 2` 속도 정규화를 교체했다. 이제 서 있는 상태는 실제 수평 속도 0→8을 Speed 0→1, 8→12를 1→2로 보낸다. 달리기를 해제해도 아직 빠르면 달리기 모션에서 연속해서 감속한다. 앉기는 기존 자세 속도 기준, 포복/슬라이드의 Speed=0 처리는 유지한다. 방향별 트리/클립/AnimatorController는 이번에 수정하지 않았다.

### `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterCamera.cs`

`_movement`, `_actions`, `_traversal`, `_motionProfile` 참조 및 비직렬화 `_motion` 상태를 추가했다. Awake에서 상태를 생성하고 OnEnable/OnDisable에서 초기화한다. Motor/Profile을 둘 다 생략한 기존 사용처는 기존 동작을 유지하며, 둘 중 하나만 연결한 오류는 로그 후 비활성화한다.

기존 1인칭 `position = EyePosition`을 `VisualEyePosition`으로 교체했다. 논리 눈 위치에 캐릭터 기준 보행/착지 오프셋을 더하고 카메라 로컬 Z축 롤만 추가한다. 3인칭에는 이 진동을 적용하지 않는다. 기존 FOV와 장애물 거리/자세 높이 추적 코드는 유지한다.

1인칭 `GetAimRay()`의 시작점도 `EyePosition`→`VisualEyePosition`으로 교체했다. 방향은 기존 `AimRotation`을 그대로 쓰며 롤은 전방 방향을 바꾸지 않는다. 이 원점 변경 때문에 별도의 아군 더미 소유 발사/팀 판정 검사를 제품 반영 전에 실행했다.

공개 읽기 전용 `FirstPersonWeaponOffset`, `FirstPersonWeaponEuler`, `FirstPersonAimBlend`로 팔 표현에 필요한 결과만 제공한다. 포복·슬라이드·장애물 통과·소생 등 동작이 경쟁하는 동안에는 피드백 상태를 초기화한다.

### `Assets/Features/PlayerCharacter/Scripts/FirstPersonArmsView.cs`

`_motionSource` 참조를 추가했다. 기존 즉시 위치 선택을 부드러운 조준 위치 블렌드로 교체하고, 동일한 계산 상태에서 나온 무기 오프셋/회전을 기존 poseEuler 앞에 합성한다.

```csharp
// 이전
Vector3 offset = Vector3.Scale(_view.IsAds ? _adsOffset : _offset, _characterRoot.lossyScale);

// 변경의 핵심
Vector3 offset = Vector3.Scale(Vector3.Lerp(_offset, _adsOffset, blend) + feedback,
    _characterRoot.lossyScale);
```

기존 팔 복제 본, 그립, 모델, 장전·근접 공격 클립은 유지한다. 소스가 연결되지 않은 기존 사용처는 원래 위치 선택을 유지한다. 카메라의 착지 이동은 눈 피벗을 통해 팔에도 전달되고, 별도 무기 흔들림은 그 위에 더해진다.

삭제한 메서드·직렬화 필드·컴포넌트·참조·에셋은 없다. 교체한 기존 구현은 위의 직접 수평 변위/Velocity getter, 속도 정규화, 1인칭 눈 위치 및 조준 원점 선택, 팔의 즉시 offset/회전 합성 표현이다.

## 신규 계산/설정 파일

- `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterMotionState.cs`: 비-MonoBehaviour 상태 객체. 생성자로 Profile을 받아 이동 거리 기반 보행 위상, 시선 회전 관성, 조준/달리기 블렌드, 착지 임펄스를 계산한다. 상태는 외부에서 직접 바꾸지 못한다. 정지/벽 막힘에는 보행 위상을 진행하지 않으며 진폭은 감쇠한다. 착지는 정확한 임계 감쇠 스프링으로 복귀하고 초기 생성 자체는 착지를 만들지 않는다. 게임플레이 변위·피해·탄약은 다루지 않는다.
- `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterMotionProfile.cs`: CreateAssetMenu를 제공하는 ScriptableObject. Inspector 조정값을 화면 계산과 분리하며 런타임에는 읽기 전용 속성으로 노출한다.
- `Assets/Features/PlayerCharacter/Settings/PlayerCharacterMotion.asset`: SampleScene에서 사용하는 설정 인스턴스. 많은 표현 수치를 카메라 컴포넌트에 직접 늘어놓지 않고 이 에셋에서 편집한다.

| 설정 필드 | 최종 값 |
|---|---|
| `_walkStride`, `_sprintStride` | 5.6, 7.5 — 보행 한 주기의 월드 거리 |
| `_walkCameraBob` | (0.012, 0.02) — 좌우/하강 진폭 |
| `_sprintCameraBob` | (0.022, 0.038) |
| `_cameraRoll` | 0.45도 |
| `_adsMotionMultiplier` | 0.1 — 조준 시 보행/관성/착지 표현 감소 |
| `_weaponBob` | (0.022, 0.032, 0.008) |
| `_lookSway` | (0.006, 0.008, 0.006) — 각속도에 대한 회전 반응 계수 |
| `_maximumSway` | 2.5도 — 시선 관성 회전 벡터의 최대 크기 |
| `_sprintOffset` | (0.04, -0.05, -0.04) |
| `_sprintEuler` | (8, -5, -8)도 |
| `_response`, `_aimResponse` | 18, 18 /초 — 지수 블렌드, 목표의 95%까지 약 0.166초 |
| `_landingImpulse`, `_maximumLandingImpulse` | 0.12, 2 |
| `_landingRecovery` | 14 /초 |

초기 달리기 설정 (0.06,-0.18,-0.08)/(18,-8,-14)은 검증 렌더에서 총구가 화면 밖(Y≈-0.13)으로 내려가므로 폐기했다. 최종 설정은 총구가 Y≈0.15에 남는다. 이 과정에서 제품의 기존 사용자 설정은 바꾸지 않았다.

## SampleScene의 정확한 연결

`Assets/Scenes/SampleScene.unity`의 기존 **596개 직렬화 오브젝트를 모두 유지**한다. 오브젝트 추가/삭제/이동 없이 아래 세 컴포넌트의 끝에 필드만 추가했다.

| 컴포넌트 fileID | 추가 필드 |
|---|---|
| Movement `950000008` | `_acceleration:80`, `_deceleration:100`, `_airAcceleration:24` |
| Camera `330585549` | `_movement:950000008`, `_actions:950000022`, `_traversal:950000025`, `_motionProfile:{fileID:11400000,guid:6c3c3063fb00a484aabecbc3f03e7768}` |
| FirstPersonArmsView `1653572103` | `_motionSource:330585549` |

원본 씬 전체 복사를 하지 않고 위 세 YAML 블록의 추가 필드만 부분 반영했다. 이전 모든 블록과 직렬화 값은 그 앞부분 그대로 보존한다.

## 검사 파일 변경과 신규 meta 전체

| 검사 파일 | 변경 이유/검사 내용 |
|---|---|
| `Assets/Tests/PlayMode/PlayerCharacterMovementTests.cs` | 걷기·달리기 최고 속도/대각선 검사 전에 각각 0.2초 가속 구간을 둔다. 기존 0.5초 이동 4/6의 검증은 유지한다. |
| `Assets/Tests/PlayMode/PlayerCharacterInfantryTests.cs` | 슬라이드 검사에서 먼저 15×0.02초 달려 실제 달리기 속도를 만든다. 기존 슬라이드 거리/자세/재시작 방지 검증은 유지한다. |
| `Assets/Tests/PlayMode/PlayerCharacterDirectionalAnimationTests.cs` | 정지 감쇠를 이동 직전 Speed와 비교하고 제동이 끝날 때까지 추가 8프레임의 0입력을 전달한다. 방향 보존/대기 복귀 검사는 유지한다. |
| `Assets/Tests/PlayMode/PlayerCharacterStanceTests.cs` | 앉은 이동 최고 속도 검사에 먼저 15×0.02초 이동 구간을 둔다. 달리기/점프 요청 차단 및 0.1초 이동 0.4 검증은 유지한다. |
| `Assets/Tests/PlayMode/PlayerCharacterMotionTests.cs` | 신규 10개: 정지 드리프트, 이동/조준 진폭, 회전 관성 상한/복귀, 착지 복귀, 30/120fps 상태 일치와 초기화, 달리기/조준 전환, 가감속/방향 전환, 공중 운동량/탑승 정지, 실제 씬 연결·조준 원점·달리기 총 화면 유지, 아군 더미 발사자와 적/중립/아군 피해. |

새 meta 파일은 아래 5개이며 기존 meta는 수정하지 않았다.

| 신규 meta 파일 | GUID |
|---|---|
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterMotionProfile.cs.meta` | `cab848f07f7253e4999d7df8940766c0` |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterMotionState.cs.meta` | `8161c182c2bce824dabe3edea5315c94` |
| `Assets/Features/PlayerCharacter/Settings.meta` | `5822b9b5322cdd84a8d65714316742f9` |
| `Assets/Features/PlayerCharacter/Settings/PlayerCharacterMotion.asset.meta` | `6c3c3063fb00a484aabecbc3f03e7768` |
| `Assets/Tests/PlayMode/PlayerCharacterMotionTests.cs.meta` | `73a2ff18d3ae98a4db7d87cca873ac6d` |

문서는 이 파일(신규), `Docs/AI/UnityProjectContext.md`, `Docs/AI/PlayerCharacterFeatureRoadmap.md`의 최신 기록을 갱신했다. 제품 파일은 기존 9개 수정 + 신규 9개 + 문서 3개 = **21개**다. 기존 CombatView/AnimatorController/모션 클립/모델/무기 데이터/다른 씬·프리팹은 이번에 수정하지 않았다.

## 검증 기록

- Unity 6000.4.8f1 격리 프로젝트 컴파일 및 새 Profile import/씬 참조 확인.
- 최종 전체 PlayMode **107/107 통과**, 56.73초 (`verified-playmode.xml/log`). 기존 98개와 신규 9개를 포함한다.
- 이후 추가한 카메라 조준 원점의 더미 검사 **1/1 통과**, 2.20초 (`dummy-range.xml/log`). 발사자는 기존 아군 DummyShooter가 붙은 PlayerCharacter이며 Enemy/Neutral은 피해 10, Player 팀으로 설정한 아군 더미는 피해 0을 확인했다. 이 검사까지 현재 제품 검사는 108개이며 마지막 한 개는 별도 실행했다.
- 최종 렌더 실행 **10/10 통과**, 8.28초 (`render-v2.xml/log`). 신규 표현 검사 9개 + 제품에 포함하지 않은 캡처 검사 1개다. 960×540, 30fps, 210프레임의 실제 1인칭 이동을 렌더하고 걷기/달리기/ADS/착지 프레임을 확인했다. 영상: `Library/CodexBattlefieldFeel/first-person-motion-review.mp4`.
- 초기 통합 검사는 설정 에셋의 씬 참조 누락과 이전 즉시 가감속을 가정한 검사 때문에 실패했다. 참조 연결/누락 검증과 실제 가속·제동을 반영한 검사를 수정했다. 카메라 설정을 빼서 검사를 통과시키지 않았다.
- 중간 전체 실행에서 기존에 보고됐던 `Invalid worldAABB`가 Z/Lean 입력 검사 중 1회 발생했다. 해당 입력 검사 포함 별도 실행 12/12, 이후 최종 전체 실행 107/107에서는 재현되지 않았다. 로그를 무시하거나 억제하지 않았고 **AABB 원인을 해결했다고 주장하지 않는다**.
- 제품 반영은 원본 기준 해시 확인 후 스크립트 차이와 씬의 세 블록에만 적용했다. 스크립트/신규 에셋/씬은 검증본과 일치하고, 기존 596개 씬 오브젝트의 모든 값·transform을 보존했다. 에셋 GUID 1,135개에 중복이 없으며 새 참조가 해석된다. C#/문서 공백 검사도 통과했다. AnimatorController는 기존부터 파일 내 오브젝트 순서가 다르지만 ID별 값이 검증본과 일치하며 이번에 수정하지 않았다. `Library/CodexBattlefieldFeel/final-audit.json`에 기록했다.

## 구조 검토와 남은 확인

Motor는 이동, Camera/ArmsView는 출력 적용, MotionState는 표현 상태 계산, Profile은 조정 데이터만 맡는다. Motor 공개 상태와 생성자/직렬화 참조로 의존성을 제공하고 계산 상태는 외부 직접 수정을 허용하지 않는다. 의미 없는 인터페이스·상속·전역 관리자는 추가하지 않았다. 기존 사용자 동작을 대체하는 상속도 없다. OOP/SOLID의 의도적 예외는 없다.

Unity Editor에서는 SampleScene을 다시 열어 **V로 1인칭 전환 → WASD → Shift 달리기/해제 → 좌우 회전 → 우클릭 탭 ADS → Space 점프/착지 → F 승하차**를 확인한다. 근접 벽/천장, 실제 마우스 입력, 낮은 프레임률의 조작감은 수동 확인이 남는다. 3인칭 구도는 그대로이며 카메라 바운스/팔·총 피드백은 1인칭 대상이다.

전용 1인칭 달리기/시작·정지/착지 모션, 3인칭 상체의 달리기 자세, 제자리 회전/발 접지, 무기별 조준/반동/장전 세부 동작은 다음 단계다. 현재 무료 테스트 모델과 기존 클립에 절차적 표현을 더한 상태다. 배틀필드와 동일한 시각적 완성도라고 평가하지 않는다. Player 빌드·커밋·푸시는 수행하지 않았다.
