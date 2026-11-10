# 기본 이동 디테일 1차 — 방향별 모션 (2026-10-28)

사용자의 최신 요청은 모든 보병 기능을 한 번에 늘리는 것이 아니라 단계별로 디테일을 다듬는 것이다. 이번 범위는 **서 있는 상태의 앞뒤·좌우·대각선 이동 애니메이션과 정지 전환**이다. 배틀필드와 동일한 이동 완성도를 달성했다는 의미는 아니다.

## 변경 전 비교와 보존 범위

직전 검증본과 현재 Movement/AnimationView/AnimatorController/SampleScene/기존 이동·애니메이션 검사를 비교했다. 추가 사용자·Editor 변경은 없었고 수정 직전에도 해시를 재확인했다. 비교 기준은 `Library/CodexInfantryDetail/baseline.json`이다. 이번 변경은 아래 파일 목록에 한정된다. 기존 문서는 현재 내용을 남기고 최신 기록을 추가했다.

SampleScene, 캐릭터·모델·카메라 위치/크기, 이동 최고 속도 8/12, 체력 100, 별도 탄창/근접 공격 오브젝트 참조와 하이어라키는 수정하지 않았다. 입력·충돌·점프·자세·전투·탑승 구현도 유지한다.

## 스크립트와 애니메이터 변경

`Assets/Features/PlayerCharacter/Scripts/PlayerCharacterAnimationView.cs`에 `MoveX`/`MoveY` Animator 해시 및 방향 전달 코드를 추가했다. 기존 속도 계산과 `Speed`/Grounded/VerticalSpeed/Crouched/Life/Reviving 전달은 유지한다.

```csharp
// 기존: 방향에 상관없이 속도로만 앞걷기/달리기를 선택
_animator.SetFloat(Speed, normalizedSpeed, _speedDamping, Time.deltaTime);

// 추가: 충돌 처리 후 실제 속도를 이동 루트의 로컬 방향으로 변환
Vector3 localVelocity = _movement.transform.InverseTransformDirection(
    new Vector3(velocity.x, 0f, velocity.z));
Vector2 direction = new Vector2(localVelocity.x, localVelocity.z).normalized;
_animator.SetFloat(MoveX, direction.x, _speedDamping, Time.deltaTime);
_animator.SetFloat(MoveY, direction.y, _speedDamping, Time.deltaTime);
```

방향 갱신은 수평 속도 `> 0.05`일 때만 한다. 정지 시에는 마지막 방향을 유지하면서 기존 `_speedDamping`으로 Speed가 0으로 내려간다. 뒤/옆걸음을 멈추며 앞걷기로 바뀌는 현상을 방지한다. 새 직렬화 필드나 설정값은 추가하지 않았다.

`Assets/Features/PlayerCharacter/Animation/PlayerCharacter.controller`에서는 기존 35개 직렬화 오브젝트를 모두 유지하고 BlendTree 2개만 추가했다. 기존 오브젝트 중 변경한 것은 다음 두 개다.

| 오브젝트 | 기존 → 변경 | 이유 |
|---|---|---|
| Controller `9100000` | float 파라미터 `MoveX`, `MoveY` 추가, 기본값 각각 0 | 캐릭터 기준 이동 방향 전달 |
| Idle Walk Run `-5493868147706495702` | threshold 1의 Walk_N 직접 참조 → Walk Direction 트리 `-5074856818806827147`; threshold 2의 Run_N 직접 참조 → Run Direction 트리 `8787377942309909390` | 기존 속도 블렌드를 보존하며 방향 선택 추가 |

제거한 메서드·필드·컴포넌트·상태·전환·에셋은 없다. 교체한 것은 위의 걷기/달리기 직접 참조 두 곳이며, 원래 클립 참조는 각각 새 트리의 정면 자식으로 이동했다. 대기 참조, Speed 임계값 0/1/2, 점프·앉기·전투·사망·소생 상태와 전환은 그대로다.

추가한 두 트리는 `2D Simple Directional`, X=`MoveX`, Y=`MoveY`다.

| 로컬 방향 | Walk Direction | Run Direction |
|---|---|---|
| `(0,1)` | 기존 Walk_N, 1x | 기존 Run_N, 1x |
| `(0,-1)` | WalkBackward, 1x | WalkBackward, 1.3333333x |
| `(-1,0)` | StrafeLeft, 0.75x | StrafeLeft, 1x |
| `(1,0)` | StrafeRight, 0.75x | StrafeRight, 1x |

대각선은 인접 방향을 블렌드한다. Run 트리의 뒤 방향은 트리를 완결하기 위한 표현 경로이며 뒤로 달리기 입력을 새로 허용하지 않는다. 애니메이션이 실제 이동을 제어하지 않으며 `applyRootMotion=false`를 유지한다.

## 추가 파일 전체 목록

| 파일 | 내용/필요성 |
|---|---|
| `Assets/Tests/PlayMode/PlayerCharacterDirectionalAnimationTests.cs` | 실제 SampleScene에서 8방향·회전된 이동 루트·클립 가중치·모델 방향/위치 보존·정지 감쇠·전방/대각선 달리기 회귀 검사 3개 |
| `Assets/Tests/PlayMode/PlayerCharacterDirectionalAnimationTests.cs.meta` | 새 테스트 스크립트 GUID |
| `Assets/ThirdParty/KayKit.meta` | 새 에셋 공급자 폴더 GUID |
| `Assets/ThirdParty/KayKit/Animations.meta` | 모션 폴더 GUID |
| `Assets/ThirdParty/KayKit/Animations/WalkBackward.anim` | 뒤걷기 Humanoid 루프, 1.066667초 |
| `Assets/ThirdParty/KayKit/Animations/WalkBackward.anim.meta` | 뒤걷기 클립 GUID/fileID 7400000 |
| `Assets/ThirdParty/KayKit/Animations/StrafeLeft.anim` | 왼쪽 이동 Humanoid 루프, 0.8초 |
| `Assets/ThirdParty/KayKit/Animations/StrafeLeft.anim.meta` | 왼쪽 클립 GUID/fileID 7400000 |
| `Assets/ThirdParty/KayKit/Animations/StrafeRight.anim` | 오른쪽 이동 Humanoid 루프, 0.8초 |
| `Assets/ThirdParty/KayKit/Animations/StrafeRight.anim.meta` | 오른쪽 클립 GUID/fileID 7400000 |
| `Assets/ThirdParty/KayKit/LICENSE.txt` | 원본 CC0 라이선스 고지 |
| `Assets/ThirdParty/KayKit/LICENSE.txt.meta` | 고지 파일 GUID |
| `Assets/ThirdParty/KayKit/README.md` | 공식 출처, 아카이브 SHA256, 추출 경로/설정/이름, 배속과 한계 |
| `Assets/ThirdParty/KayKit/README.md.meta` | 출처 문서 GUID |

클립 출처는 [KayKit Character Animations 1.1 무료 팩](https://kaylousberg.itch.io/kaykit-character-animations)이다. 각 클립은 Loop Time/Loop Pose를 켜고 원래 루트 방향/위치를 유지하며 루프 방향/XZ 블렌드를 켰다. 이전 Quaternius 모션과 달리 180도 루트 보정은 필요하지 않았다. 전체 FBX·모델·재질은 제품 Assets에 추가하지 않았다.

문서 변경은 이 파일(신규), `Docs/AI/PlayerCharacterFeatureRoadmap.md`(단계별 디테일 순서/진행 기록 추가), `Docs/AI/UnityProjectContext.md`(최신 구현과 검증 기록 추가)다. 수정 스크립트 1개, 수정 controller 1개, 새 파일 14개, 문서 3개로 총 19개 파일이다.

## 설계 검토

방향 계산은 기존 AnimationView의 표현 책임 안에 둔다. Movement는 충돌/변위의 유일한 주체이며 View는 공개된 Velocity/속도 상태만 읽는다. 상태 소유권이나 직접 변조를 추가하지 않았다. 새 관리 계층·인터페이스·상속은 실제 교체 지점이 없어서 만들지 않았다. 방향별 표현 확장은 Animator 합성으로 구성한다. OOP/SOLID의 의도적 예외는 없다.

## 검증과 남은 확인

- 격리 프로젝트 Unity 6000.4.8f1 Humanoid import: Basic/Advanced Avatar 모두 human/valid 통과.
- 제품 PlayMode **98/98 통과**, 59.60초. 기존 95개 + 방향 회귀 3개. `Library/CodexInfantryDetail/verified-playmode.xml` 및 `.log`.
- 별도 그래픽 실행: 방향 검사 3개 + 실제 권총 상체 자세의 모션 캡처 1개, **4/4 통과**. `capture-v2.xml/log`, forward/backward/left/right/diagonal.png를 확인했다.
- 최초 정지 테스트는 고정 프레임 대기가 배치 실행의 실제 감쇠 시간보다 짧았고, 이후에는 대기 클립의 잘못된 이름 가정으로 실패했다. 실제 시간 대기와 원래 클립 에셋 경로 확인으로 검사를 수정했다. 제품 코드를 검사에 맞춰 바꾸지 않았다.
- 원본 반영은 기존 controller 오브젝트 두 개의 부분 패치와 두 트리 추가만 사용했다. 씬 및 나머지 기준 파일 해시가 일치하며 새 클립/스크립트와 controller 오브젝트 전체가 검증본과 일치한다. 에셋 GUID 1,130개가 모두 고유하고 controller 참조가 해석된다. `final-audit.json`에 기록했다. 수정 C#/문서의 Git 공백 검사는 통과했다.
- 기존 장시간 혼합 렌더 검사에서 보고했던 간헐적 AABB 문제의 원인이 해결됐다고 주장하지 않는다. 이번 검증 로그에서는 재현되지 않았다.

Unity Editor에서는 SampleScene을 다시 열고 WASD/대각선/Shift 이동, 키를 놓을 때 전환, 자유 시점 중 이동을 확인해야 한다. 자동 검사와 정지 이미지 확인만으로 실시간 조작감·발 미끄러짐까지 확정할 수 없다. 가속/감속, 전용 시작·정지·급회전·제자리 회전, 발 접지, 앉은 방향별 모션은 다음 디테일 단계로 남는다. 새 피해/발사 판정은 수정하지 않았다. Player 빌드·커밋·푸시는 수행하지 않았다.

## 신규 meta 식별자

| meta 대상 | GUID |
|---|---|
| PlayerCharacterDirectionalAnimationTests.cs | `26cc81c2082aded40a56cbe566330675` |
| KayKit 폴더 | `3ba790ba446b80e4ebad8b75d0deab64` |
| Animations 폴더 | `6d66d63e778b0774d836e32a3e513d59` |
| LICENSE.txt | `80b4d5ec019246dcb434b3c0992835db` |
| README.md | `6d85f1d1fdbc467b9cace49b225d3a9a` |
| StrafeLeft.anim | `f819d272fd5d32245a31f878b4ca3263` |
| StrafeRight.anim | `87702e44e6c790c4c82174f2b08ff259` |
| WalkBackward.anim | `c8dd10b7c3397f44cb47b0b58f2c8968` |
