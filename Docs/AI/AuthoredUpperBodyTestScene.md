> 현재 상태: 사용자 요청으로 비교 씬과 전용 controller/mask를 삭제했습니다. 아래 내용은 과거 실험 기록입니다. 현재 제작·플레이 씬은 SampleScene 하나이며, 실험용 Body/Head 마스크는 적용하지 않았습니다.

# Quaternius 몸통 조준 비교 씬 적용

2026-10-29. 사용자 요청: 검증 결과를 직접 테스트할 수 있도록 적용.

## 실행

Unity Play 모드를 종료하고 `Assets/Tests/Scenes/CharacterUpperBodyAnimation/CharacterUpperBodyAnimationTest.unity`를 연 뒤 Play한다. 기존 캐릭터 조작을 그대로 사용한다.

1. 권총을 든 3인칭에서 우클릭을 유지하며 마우스를 위·아래로 움직인다.
2. WASD 이동과 앉기 중 같은 조준을 비교한다. V로 시점을 전환한다.
3. 장전·근접·탑승도 기존 조작으로 확인할 수 있지만 아래 실험의 알려진 차이를 고려한다.
4. 기존 결과와 비교하려면 Play 종료 후 원래 `Assets/Scenes/SampleScene.unity`를 연다.

우클릭 조준을 하지 않는 Carry/Sprint의 총을 낮추는 기존 동작은 유지된다. 일반 휴대 상태에서 손이 마우스 상하를 완전히 따라가도록 수정한 작업은 아니다. 소총/KayKit을 새 장비로 추가하지 않았으며, 현재 보유 Quaternius 권총 클립의 몸통·머리 채널을 살린 실험이다.

## 적용 구조와 변경

원본 SampleScene의 비교용 복사본, 전용 Animator Controller, 전용 AvatarMask만 추가했다. 새 런타임 클래스나 관리자는 필요하지 않으며 기존 애니메이션 표시 책임에 데이터 에셋으로 연결했다. 제품의 OOP/SOLID 책임 경계 변경이나 예외는 없다.

| 신규 파일 | 구체적 내용 |
|---|---|
| `Assets/Tests/Scenes/CharacterUpperBodyAnimation/CharacterUpperBodyAnimationTest.unity` | 현재 SampleScene 복사본. 캐릭터 Armature Animator에 `m_Controller` 프리팹 override를 추가해 아래 전용 컨트롤러 참조. 원본 대비 이 참조 변경만 기능 차이. Unity 저장 과정에서 PostureView의 기존 필드3개 순서만 재정렬됐고 값은 동일 |
| 같은 폴더 `AuthoredUpperBody.controller` | 현재 PlayerCharacter.controller 복사본. 이름을 AuthoredUpperBody로 설정하고 UpperCombat 레이어의 avatarMask만 전용 마스크로 연결. 상태·전환·클립·파라미터·다른 레이어 유지 |
| 같은 폴더 `AuthoredUpperBody.mask` | 기존 UpperCombat.mask 복사본. Body/Head 슬롯만0→1. 팔·손가락1 유지, 루트·다리·IK 슬롯0 유지 |
| 위3파일의 `.meta` | 각각 독립 GUID. 원본 GUID 변경 없음 |
| `Assets/Tests/Scenes.meta`, `Assets/Tests/Scenes/CharacterUpperBodyAnimation.meta` | 신규 테스트 폴더 GUID |
| 이 문서 | 실행법, 변경 값, 실험 한계와 증거 |

테스트 마스크의 Humanoid 슬롯:

```text
기존 UpperCombat: [0,0,0,0,0,1,1,1,1,0,0,0,0]
테스트 마스크:    [0,1,1,0,0,1,1,1,1,0,0,0,0]
```

몸통·머리 키가 잘리던 기존 팔 전용 재생과 달리, 비교 씬에서는 외부 클립에 들어 있는 몸통 굽힘과 머리 자세가 함께 재생된다. 다리는 기존 이동 레이어를 사용한다. 키프레임을 새로 제작하거나 관절 보정 코드를 추가하지 않았다.

기존 원본 파일의 메서드/필드/직렬화 값/컴포넌트/참조/에셋을 삭제하거나 대체하지 않았다. 테스트 씬의 새 프리팹 override만 테스트 컨트롤러를 선택한다. 원본 씬, 모델 프리팹, 캐릭터 코드, 원본 컨트롤러/마스크, 이동 속도, 카메라/그립/클립은 보존했다. 테스트 씬은 생성 시점 복사본이므로 이후 SampleScene 변경이 자동으로 반영되는 구조는 아니다.

## 왜 원본 전체 교체 대신 비교 씬인가

격리 프로젝트에서 원래 UpperCombat.mask의 Body/Head만 켜는 실험을 먼저 했다. 137개 중133통과/4실패였다(`Library/CodexAuthoredUpperBody/mask.xml/log`).

- `CombatLayerPreservesLocomotionHeadAndTorsoFacingInThirdPerson`: 정지 PistolHold 몸통 방향 차이 약23.94도, 기존 기준1도 미만.
- `TorsoAndWeaponReachTheNewAimInTheSameAnimationEvaluation`: 몸통 즉시/안정 자세 차이 약4.97도, 기존 기준2도 미만.
- `WorldBodySharesThirdPersonAimAcrossPerspectiveChangesWithoutTiltingTheLegs`: 시점 전환 몸통 차이 약4.80도, 기존 기준3도 미만.
- `MissingClipRetainsTheExistingManualSeatPose`: 수동 착석 위치 차이 약0.0122m, 기존 기준0.002m 미만.

이는 비교 씬에도 남아 있는 알려진 한계다. 기존 LookAt·관절 보정·시점별 피치 처리와 원본 몸통 모션을 함께 사용하는 현재 조합을 정식 완료로 판정하지 않는다. 이 문제를 숨기거나 테스트 기준을 느슨하게 하지 않고 원본 기능과 분리해 사용자가 직접 비교하도록 했다. 기존 마스크를 바꾼 격리 실험은 복원한 후 최종 검증했다.

## 검증

- 작업 시작 시 직전 후보 검사 baseline과 원본 캐릭터/PlayMode/씬 해시 차이0. 배포 직전에도 다시 확인해 사용자/Editor 변경이 없음을 확인했다.
- Unity6000.4.8f1 격리 프로젝트에서 최종 **138/138 Passed**, 실패·건너뜀0 (`Library/CodexAuthoredUpperBody/final.xml/log`). 제품 기존133개 + 기존 임시 진단/캡처4개 + 새 비교 씬 smoke1개다.
- 기존 테스트는 원본 SampleScene을 사용한다. 따라서138개 통과는 원본 기능 보존과 새 씬의 제한된 smoke 검증 결과이며, 위 비교 씬 회귀4개가 해결됐다는 뜻이 아니다.
- 새 smoke는 실제 테스트 씬을 열어 전용 컨트롤러/마스크 참조, Body·Head 활성/Root·다리 제외, 마우스 pitch0/-60/+60, 이동 중 몸통 변화8도 초과, 총구 정렬2도 미만, 캐릭터 루트 회전 보존을 검사했다.
- 마우스 상하3방향 이동 프레임72개를 생성해 `Library/CodexAuthoredUpperBody/test-scene-preview.jpg`에서 직접 확인했다. `aim-0.mp4`, `aim--60.mp4`, `aim-60.mp4`는 수동 Animator 평가24fps 영상이다.
- 검증 도구의 최초 컴파일에서 임시 smoke의 Dummy 네임스페이스 누락을 수정한 후 최종 실행했다. 제품 스크립트 컴파일 오류를 수정한 것은 아니다.
- 검증한 신규 씬/컨트롤러/마스크 및 메타8개를 원본으로 바이트 동일하게 복사했다. 새 GUID 중복 검사와 기존 baseline 보존을 확인한다.
- 원본 사용자 Editor에서 직접 Play 조작이나 플레이어 빌드는 하지 않았다. 사용자는 위 실행 절차로 실제 입력 느낌, 시점 전환, 장전/근접/탑승 차이를 확인해야 한다. 빌드 씬 목록/현재 열린 씬은 변경하지 않았다.

격리 전용 생성 도구는 `Library/CodexInfantryWork/Project/Assets/Editor/CreateAuthoredUpperBodyTestScene.cs`(+meta), smoke는 같은 격리 프로젝트 `Assets/Tests/PlayMode/CodexAuthoredUpperSceneSmoke.cs`(+meta)다. 제품으로 복사하지 않았다. 전체 복사 목록은 `Library/CodexAuthoredUpperBody/promoted-files.json`에 있다. 커밋·푸시 없음.
