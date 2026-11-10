# Quaternius 우선 적용 — 이동 세트 1단계

2026-10-29. 사용자가 보유 모션 매칭을 승인하고 기존 구현이 있어도 Quaternius를 우선하도록 지정했다. 앞서 제안한 이동 → 권총 → 근접·투척 순서 중 이번 단계는 이동 세트다. 추가 입력이나 이동 시스템 없이 기존 resolved velocity와 Animator를 사용한다.

## 변경 전 비교

이전 애니메이션 선정 시 저장한 Library/CodexBattlefieldAnimationMatch/baseline.json과 비교했으며 관련 기존 파일 변화는 없었다. 새 baseline인 Library/CodexQuaterniusIntegration/baseline.json으로 작업 중 사용자 변경도 감시했다. 검증된 Controller 반영 직전 원본이 시작 시점과 같은 바이트인지 다시 확인했다. 기존 미커밋 작업은 보존했다.

## 모든 변경 파일과 정확한 내용

### Assets/Features/PlayerCharacter/Animation/PlayerCharacter.controller

기존 Idle Walk Run BlendTree(fileID -5493868147706495702)의 children만 변경했다.

```text
이전: Speed 0 → LocomotionIdle / 1 → Walk Direction / 2 → Run Direction
이후: Speed 0 → LocomotionIdle / 0.5 → Walk Direction
            1 → Jog Direction / 2 → Run Direction
```

- Walk Direction의 threshold를 1에서 0.5로 옮겼다. 걷기 트리와 클립은 삭제하지 않았다.
- 같은 Controller 안에 Jog Direction BlendTree(fileID 5856855049332177232)를 추가하고 threshold 1에 연결했다.
- Jog Direction은 기존 Walk Direction의 방향/재생 배율을 유지하며 정면 motion만 Assets/ThirdParty/Quaternius/Library/UAL1/InPlace/Jog_Fwd_Loop.anim으로 지정한다. 저장한 네이티브 클립을 직접 참조하여 중복 에셋을 만들지 않았다.
- 방향은 MoveX/MoveY, 정면 timeScale 1, 뒤 1, 좌우 0.75를 그대로 사용한다. 후진 WalkBackward와 StrafeLeft/Right는 현재 보유 Quaternius Standard에 대응 동작이 없어 기존 KayKit 보조를 유지한다.
- 일반 전진 속도 8에서는 Speed=1로 조깅, 절반 입력으로 약 4에서는 Speed=0.5로 걷기, 질주 12에서는 Speed=2로 기존 질주가 재생된다. 그 사이 속도는 기존 Speed damping과 BlendTree로 혼합된다. 키보드용 별도 걷기 키는 추가하지 않았다.
- 낮은 속도의 정면 움직임을 제외하면 기존 방향 클립/배율이 유지된다. 완성된 Quaternius 8방향 세트를 확보했다는 뜻은 아니다.
- 기존 Controller 오브젝트 중 위 트리 하나만 바뀌고 새 트리 하나만 추가됨을 fileID별 비교했다. 다른 상태·전환·레이어·마스크·파라미터·참조는 보존했다. Controller .meta도 동일하다.

기존 구현을 바꾼 이유: 기존에는 비질주 최대 속도에서도 걷기를 재생했다. 보유한 Quaternius 조깅을 일반 달리기 위치에 배치하고 걷기를 저속에 남겨 세 가지 이동 용도를 구분한다. 동작 기반의 매칭이며 배필의 보폭·타이밍·카메라를 동일하게 복제했다고 주장하지 않는다.

### Assets/Tests/PlayMode/PlayerCharacterAnimationTests.cs

MovementDrivesIdleWalkRunJumpFallAndLandingWithoutRootMotion에서 일반 전진 속도 8의 기대 클립을 LocomotionWalk → Jog_Fwd_Loop로 교체했다. 기존 grounded/속도/질주/점프·공중·착지/모델 Transform 검증은 보존했다.

### Assets/Tests/PlayMode/PlayerCharacterDirectionalAnimationTests.cs

- LocalVelocitySelectsCardinalAndDiagonalGaitsWithoutRotatingTheModel: 일반 전진 성분의 기대 클립을 LocomotionWalk → Jog_Fwd_Loop로 교체. 8방향 이동과 모델/뼈 방향 검증 유지.
- SprintPreservesForwardRunAndBlendsTheSideGaitOnDiagonals: 질주 중 조깅 클립이 남지 않는 검사 추가. 기존 질주/좌우 혼합/걷기 배제 검사 유지.
- ResolvedSpeedSelectsWalkJogAndSprintAndReturnsThroughJog 신규: 실제 CharacterController 이동으로 0.5 입력 걷기 → 일반 조깅 → 질주 → 조깅 복귀 → 대기 검증. 애니메이션 속성만 강제로 지정하는 테스트가 아니다.

### Docs/AI/QuaterniusLocomotionIntegration.md

이 문서 신규. 교체 전후, Quaternius 우선 정책, 기존 설정 보존, 검증 및 남은 범위를 기록한다.

### Docs/AI/UnityProjectContext.md

Quaternius locomotion integration 단락 추가. 속도별 실제 클립 연결, 변경 범위, 검증 증거와 다음 단계 기록.

## 보존·구조 검토

런타임 C# 변경 없음. 이동 8 / 질주 12 / 앉기 4 / 포복 2, 가속·감속, 카메라·모델·좌석·캡슐·씬 계층과 설정을 변경하지 않았다. 기존 입력·충돌·판정 권한은 CharacterController와 각 담당 컴포넌트에 있다. Animator는 기존 AnimationView가 전달하는 실제 이동 속도를 표현한다.

새 클래스/필드/컴포넌트/인터페이스는 필요하지 않다. 같은 표현 책임의 BlendTree를 합성해 확장했으며 OOP/SOLID에 대한 의도적 예외는 없다. 기존 메서드/필드/컴포넌트/에셋/상태를 제거하지 않았다. 제거·교체된 내용은 걷기 트리 threshold 1→0.5와 일반 전진에 대한 테스트의 기대 클립뿐이다. 기존 원본 애니메이션 파일을 삭제하지 않았다.

현재 Controller는 Quaternius 계열 고유 클립 23개와 KayKit 3개를 참조하며, 별도 조종석 착석도 Quaternius를 사용한다. 기존 권총·근접·점프·앉기·슬라이드·사망·소생 임시 표현은 그대로다. FistsGuard는 Quaternius에서 파생한 포즈이고, InfantryRevive/PistolMelee는 여전히 임시 재사용 표현이다.

## 검증

- 격리 Unity 6000.4.8f1 import/컴파일 및 전체 PlayMode 126/126 통과: 제품 테스트 124개 + 임시 렌더 테스트 2개. 새 제품 테스트 1개 포함.
- 증거: Library/CodexQuaterniusIntegration/tests.xml 및 tests.log. 결과 result=Passed, failed=0. 원본 사용자 Editor UI는 조작하지 않았다.
- 실제 이동의 걷기/조깅/질주 선택·복귀, 대각 이동/정지 방향 유지, jump/land, crouch/prone, slide/vault, 조준/근접/무기, 차량 탑승 관련 기존 회귀 테스트 포함.
- walk.png/jog.png/sprint.png, first-person-idle.png/first-person-sprint.png 및 slide-18.png 화면을 확인했다. 캡처와 테스트는 격리 프로젝트에서 수행했다. 원본과 테스트한 Controller/제품 테스트 두 파일의 바이트 동일성 확인.
- 시작 baseline의 151개 캐릭터/테스트/씬 파일 중 이번 변경은 Controller와 테스트 두 개뿐이다. 해당 baseline 밖의 문서 두 개는 위 목록에 명시했다.
- 실제 Editor의 SampleScene에서 사용자 체감, 장시간 방향 전환, 접지/보폭과 무장 조합의 세밀한 품질은 추가 확인이 필요하다. 특히 기존 좌우/후진 보조 클립과 전진 조깅의 스타일 차이는 남아 있다.

## 다음 단계

이번 단계에서 신규 피격·투척·구르기·수영·상호작용 재생은 추가하지 않았다. 해당 클립의 소유/진입/취소/복귀와 실제 게임 상태 연결은 후속 단계다. Quaternius에 같은 용도의 모션이 있으면 우선 연결하고, 목적이 다른 모션을 포복·소생·승하차 등으로 잘못 대체하지 않는다. 별도 유료판 구매, 파일 삭제, 커밋·푸시 없음.
