# 총·상체 조준 동기화 — 2026-10-27

## 요청과 이번 완료 범위

총과 몸이 함께 회전하도록 개선하고, 배틀필드를 참고한 캐릭터 기능 전반을 구현해 달라는 요청이다. 이번 코드 변경은 서로 다른 피치 추적 지연을 제거하는 조준 동기화다. 전체 캐릭터 기능은 PlayerCharacterFeatureRoadmap.md에 남은 작업을 구분했으며, 전부 구현했다고 보고하지 않는다.

## 편집 전 비교

직전 작업의 검증 프로젝트와 원본의 PlayerCharacterWeaponAimView.cs, PlayerCharacterCombatTests.cs, PlayerCharacter.controller, SampleScene.unity가 바이트 단위로 일치했다. 새 사용자/Unity Editor 저장 변경과 겹치는 차이는 없었다. 편집 직전에도 다시 비교했고, 현재 작업의 원본은 Temp/AimSynchronizationBefore에 보관했다. 원본 Scene/Controller에는 이번 작업으로 변경을 가하지 않았다.

## 변경한 모든 제품 파일

| 파일 | 변경한 부분과 이유 |
|---|---|
| Assets/Features/PlayerCharacter/Scripts/PlayerCharacterWeaponAimView.cs | `_bodyPitch`와 별도 피치 LerpAngle을 제거하고 private Quaternion `_aimRotation`으로 교체했다. Base Layer OnAnimatorIK에서 View.AimRotation을 한 번 저장한 뒤 몸통 LookAt과 UpperCombat 양손 IK가 같은 회전을 사용한다. 총만 먼저 도착하고 몸통이 뒤늦게 꺾이는 현상을 줄인다. OnDisable의 초기화도 `_bodyPitch=0`에서 `_aimRotation=Quaternion.identity`로 교체했다. |
| Assets/Tests/PlayMode/PlayerCharacterCombatTests.cs | TorsoAndWeaponReachTheNewAimInTheSameAnimationEvaluation 추가: 같은 기본 클립 자세에서 서기/앉기 각각 -60, +60, 0도 입력 직후의 실제 가슴 회전을 0.4초 뒤와 비교(2도 미만), 총구 방향 2도 미만 검사. FreeLookPreservesTheAimedTorsoAndWeaponPose 추가: 위 60도 조준 상태에서 자유 시점과 복귀가 총/가슴 방향을 바꾸지 않는지 검사. 기존 테스트 삭제 없음. |
| Docs/AI/PlayerCharacterAimSynchronizationChanges.md | 이번 변경·제거 항목·검증과 한계를 기록한 이 보고서. |
| Docs/AI/PlayerCharacterFeatureRoadmap.md | 요청한 캐릭터 기능 전체에 대해 현재 기반, 미완료 기능, 구현 순서와 책임 분리 작성. 계획을 완료 기능으로 표기하지 않는다. |
| Docs/AI/PlayerCharacterUpperBodyAimChanges.md | 직전 개선의 역사적 기록은 유지하고 최신 동기화 변경 보고서 링크 추가. |
| Docs/AI/UnityProjectContext.md | 최신 조준 동기화와 검증/남은 범위를 추가. |

새 런타임 컴포넌트/에셋/메타/GUID/패키지는 추가하지 않았다. Scene, Animator Controller, AvatarMask, 애니메이션 클립, 권총 프리팹, 카메라, 입력, 게임플레이 수치와 사용자 계층은 수정하지 않았다.

## 교체한 코드와 동작

```csharp
// 이전: 손은 즉시 도달하고 몸통만 이전 프레임의 피치에서 보간한다.
Quaternion aim = _view.AimRotation;
float ratio = 1f - Mathf.Exp(-_poseFollowSpeed * Time.deltaTime);
_bodyPitch = Mathf.LerpAngle(_bodyPitch, targetPitch, ratio);
Quaternion rotation = Quaternion.Euler(_bodyPitch, actorYaw, 0f);
```

```csharp
// 이후: 기본 레이어의 IK 평가 시 저장한다.
_aimRotation = _view.AimRotation;
// 몸통 목표와 손 목표가 이 회전을 함께 사용한다.
_animator.SetLookAtPosition(_camera.EyePosition + _aimRotation * Vector3.forward * 100f);
Quaternion aim = _aimRotation;
```

제거/교체한 항목은 `_bodyPitch`, ApplyUpperBodyAim의 지수 보간용 `ratio`, `_bodyPitch`의 LerpAngle 대입과 그 값으로 만드는 로컬 `rotation`, 손 IK의 View 직접 읽기, OnDisable의 피치 초기화다. 별도 지연 상태가 더 이상 필요하지 않아 교체했으며 기존 메서드·직렬화 필드·컴포넌트·참조·에셋은 삭제하지 않았다.

기존 `_poseFollowSpeed=12` 직렬화 필드는 유지하고 장전/근접/조준 진입·해제의 `_bodyBlend`에 계속 사용한다. 이 값으로 조준 방향 자체를 지연시키지는 않는다. 몸통/머리 가중치 0.3/0.85, clamp 0.2, 손 오프셋과 도달 거리 보정도 그대로다. 총/몸의 동기화란 같은 목표와 같은 평가 시점을 뜻하며 다리·허리·목·총에 똑같은 회전량을 강제로 적용한다는 의미는 아니다.

1인칭 팔/ADS는 기존 전용 표현을 사용한다. 장전·근접에서는 기존 모션을 재생하고, 자유 시점은 LookRotation만 바꾸므로 조준 중인 몸통/무기를 다른 방향으로 돌리지 않는다. 피해/발사/탄도 정책은 이번에 교체하지 않았다.

## 구조 검토

기존 PlayerCharacterWeaponAimView의 단일 책임인 조준 자세 표현을 수정했다. 새 독립 수명/게임플레이 상태/재사용 단위가 생기지 않아 별도 클래스를 만들지 않았다. View가 조준 방향을 소유하고 AimView는 읽기만 한다. 가변 표현 상태는 private이며 입력·피해·탄약·카메라 권한을 변경하지 않는다. 기존 명시적 참조 조합을 유지하며 새 상속/광범위 인터페이스/관리자를 추가하지 않았다. 의도적 OOP/SOLID 예외는 없다.

## 검증

- 수정 전 `Temp/codex-aim-sync-before.xml`: 신규 동기화 검사가 실패했다. 위 60도 입력 후 총이 이미 도달했는데도 가슴이 이후 0.4초 동안 30.554도 더 회전했다.
- 수정 후 `Temp/codex-aim-sync-combat.xml`: 전투/표현 검사 12/12 통과. 신규 두 검사와 기존 1·3인칭, ADS, 앉기, 상하 조준, 장전/근접, 탑승 취소·복귀 포함.
- `Temp/codex-aim-sync-final.xml`: 제품 PlayMode 70개 + 실제 씬 임시 렌더 검사 1개, **71/71 통과**(45.60초). 기존 이동·입력·엄폐·적/중립/아군 판정과 탑승 검사 포함. 테스트 중 컴파일 오류/예상하지 못한 런타임 예외 없음. Unity 시작 시 라이선스 토큰 갱신 경고가 있었지만 기존 라이선스로 실행 및 테스트 완료했다.
- 실제 SampleScene 지면 접촉/Locomotion을 기다린 뒤 중립, 상하 60/80도, 앉은 하향 자세를 캡처했다. 최종 총구 오차는 각 자세에서 측정상 0도, 손목 간격은 약 0.2543m였다. 실제 대기 애니메이션을 포함한 즉시/정착 가슴 변화는 약 2도 이내다. 고정 기본 자세에서의 지연 제거는 제품 회귀 검사로 별도 확인했다.
- 후속 `Temp/codex-aim-sync-rendered.xml` 렌더 검사도 1/1 통과. 정상 렌더 프레임에서 방향 전환 직후 상체/손/총이 함께 반응하고 그립이 붙어 있는 이미지를 직접 확인했다. 상하 60/80도에서 즉시/정착 가슴 변화는 각각 1.931/1.007/0.802/0.972도였고 대기 모션의 변화가 포함된다. 캡처는 `Temp/codex-sync-*.png`, 임시 스크립트/메타는 `Temp/CodexSynchronizedAimCapture.cs`에 보관했다.
- `Temp/audit_personal_combat.py`: 원본/검증본 코드·에셋 일치, GUID 고유성/참조, 기존 직렬화 오브젝트 보존 통과. 이 작업 전 Scene/Controller와 비교해 두 파일은 바이트 단위로 그대로다. 제품 테스트 파일도 검증본과 일치한다. scoped diff 공백 검사 통과.

화면 측정에서 총구 오차 0도는 float 정밀도 내 결과이지 모든 무기/상황의 완전 정확도 보증이 아니다. 직접 Animator.Update 직후에는 스킨 렌더가 아직 이전 프레임을 표시할 수 있어, 최종 즉시 캡처는 정상 애니메이션/LateUpdate를 지난 렌더 프레임으로 확인한다. 임시 캡처는 제품 Assets에 포함하지 않는다.

## Unity Editor에서 남은 확인

원본 Editor의 저장되지 않은 데이터를 조작하지 않고 격리 프로젝트에서 검증했다. 실제 마우스 입력으로 빠르게 상하 전환, 서기/앉기, 견착, 자유 시점 복귀, 장전/근접 후 복귀의 체감을 확인해야 한다. 이번 작업은 Scene 설정을 바꾸지 않았으므로 스크립트 재컴파일 완료 후 기존 SampleScene에서 Play할 수 있다. 별도 플레이어 빌드, 모든 캐릭터 기능 구현, 커밋/푸시는 수행하지 않았다.
