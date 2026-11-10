# Quaternius 이동 중 과도한 좌우 회전 수정

2026-10-29. 사용자가 이동 모션 전반에서 캐릭터가 좌우로 심하게 회전한다고 보고했다. 이 작업은 신규 모션 추가가 아니라 앞선 이동 세트의 회전 처리 결함 수정이다.

## 변경 전 비교와 원인

직전 검증본과 원본 Controller, 제품 테스트 두 파일, 조깅 클립이 동일한 바이트임을 확인했다. 이번 수정 대상 6개 클립의 시작 해시는 Library/CodexLocomotionYaw/fix-baseline.json에 기록하고 반영 직전에 다시 비교했다. 사용자·Editor의 관련 변경은 발견되지 않았다. 기존 미커밋 변경과 사용자 이동/모델/카메라 값은 보존했다.

격리 PlayMode에서 루트와 모델 Transform의 yaw는 0도로 고정됐지만 골반/어깨 방향이 크게 벗어났다. 레이어를 끈 클립 비교에서도 재현돼 상체 조준 레이어만의 문제가 아님을 확인했다. 저장한 이동 클립 중 Idle/Walk/Jog/Sprint의 Root Transform Rotation → Bake Into Pose가 꺼져 있었다. applyRootMotion=false 상태에서 몸의 기준 회전이 제거되는 반면 근육 포즈의 반대 방향 회전은 남아 골반과 머리 방향이 어긋났다.

같은 조깅 클립을 61시점으로 평가해 해당 설정 하나를 비교했다:

| 측정 | 수정 전 | 회전 Bake 후 |
|---|---|---|
| 골반 방향 yaw | 약 -56.0°~-1.5° | 약 -1.5°~+1.7° |
| 머리 forward yaw | 약 -55.4°~-1.9° | 약 -1.9°~+2.3° |

Animator.bodyRotation은 Humanoid 몸 기준이며 실제 골반 방향과 같은 값이 아니다. 회전 Bake는 몸 기준의 원본 회전을 포즈 안에서 평가해 근육 포즈와 결합한다. 몸통·팔의 원래 운동을 강제로 잠그거나 새 클립으로 교체한 것이 아니다.

관련 Unity 설명: https://docs.unity.com/en-us/engine/6000.3/manual/animation-section/animation-mecanim/avatar-creationand-setup/root-motion

## 모든 변경 파일

### 애니메이션 4개

- Assets/ThirdParty/Quaternius/Animations/LocomotionIdle.anim
- Assets/ThirdParty/Quaternius/Animations/LocomotionWalk.anim
- Assets/ThirdParty/Quaternius/Animations/LocomotionSprint.anim
- Assets/ThirdParty/Quaternius/Library/UAL1/InPlace/Jog_Fwd_Loop.anim

각 파일의 유일한 직렬화 변경:

```diff
-    m_LoopBlendOrientation: 0
+    m_LoopBlendOrientation: 1
```

이는 Animation Clip의 **Root Transform Rotation → Bake Into Pose** 설정이다. 필드 이름의 LoopBlend를 근거로 일반적인 전환 블렌드나 Loop Pose 설정과 혼동하지 않는다. 곡선/길이/fps/루프/재생 속도/GUID/파일 ID는 보존했다. .meta 변경 없음. 조깅은 현재 Controller에서 사용하는 라이브러리 클립 자체의 재생 설정을 수정했고, 원본 ZIP/FBX/GLB와 RootMotion 변형은 보존했다. 미사용 라이브러리 클립들을 일괄 수정하지 않았다.

처음에는 앉기까지 포함한 여섯 모션을 수정 대상으로 살폈지만, CrouchIdle.anim과 CrouchWalk.anim은 이미 m_LoopBlendOrientation=1이었다. 두 파일은 바이트 그대로 유지했다. Controller/씬/프리팹/입력/이동 코드와 사용자 설정값도 변경하지 않았다.

### Assets/Tests/PlayMode/PlayerCharacterLocomotionFacingTests.cs 및 .meta (신규)

GroundedGaitsKeepThePelvisFacingTheCharacterHeadingAcrossFullCycles 추가. 실제 CharacterController 이동을 사용해 idle/walk/jog/sprint/crouch-idle/crouch-walk마다 전환 대기 30프레임 뒤 180샘플(명시적 Animator.Update 0.02초)을 검사한다. 각 클립 한 주기 이상을 관찰하고, 골반의 정면 이탈이 20도 미만인지와 루트/모델 회전 고정, applyRootMotion=false를 확인한다. 한 장의 캡처나 특정 순간의 클립 가중치 검사로 대체하지 않는다. 기존 테스트/기대값/오류 로그를 완화하거나 숨기지 않았다.

### Docs/AI/QuaterniusLocomotionYawFix.md (신규)

이 문서. 원인, 설정 전후, 파일 목록, 재현 및 검증, 제한 기록.

### Docs/AI/UnityProjectContext.md

Quaternius locomotion rotation bake correction 단락을 추가해 재임포트/후속 클립 적용 때 같은 결함이 생기지 않도록 기록했다. 이전 단계의 검증 기록은 당시 사실로 보존한다.

## 검증

- **수정 전 실패 재현:** Library/CodexLocomotionYaw/before.xml/log. 새 테스트가 jog frame49에서 pelvis yaw=22.01411로 실패. 별도 전체 주기 측정에서는 약56도까지 벗어남.
- **설정 단독 비교:** clips.json / clips-baked.json 및 probe-fixed.json. 일반 루트/모델 Transform 고정, 상체 레이어를 끈 원본 클립 평가까지 비교. 초기에 시간 감쇠/빠른 자동 프레임의 영향을 받은 probe.json은 원인 확정 근거로 사용하지 않고 명시적인 시간 평가로 재검사했다.
- **수정 후 전체 통과:** 격리 Unity6000.4.8f1 PlayMode127/127. 제품125개 + 임시 렌더2개. 새 회귀 테스트1개 포함. Library/CodexLocomotionYaw/after.xml 및 after.log.
- 수정 후 실제 이동 검사 최대 골반 yaw: idle13.45°, walk2.30°, jog1.73°, sprint0.86°, crouch-idle3.66°, crouch-walk7.32°. idle은 원래 비대칭 대기 자세이며 심한 주기적 회전이 아님.
- 걷기·조깅·질주 화면을 확인했다. 1·3인칭 조준/무기/근접/슬라이드/점프/방향별 이동/탑승의 기존 회귀 검사도 통과했다.
- 원본 반영 전에 파일별 차이가 위 한 줄뿐인지 확인. 테스트한 4개 클립과 새 테스트/.meta를 그대로 반영했다. 앉기 두 파일과 Controller/씬/런타임 스크립트 보존 확인.

## 구조와 남은 확인

런타임 클래스/컴포넌트/메서드/필드/참조/애니메이션을 제거하거나 교체하지 않았다. 변경된 기존 구현은 이동용 클립 4개의 회전 추출 설정이다. 기존 CharacterController가 이동/방향 권한을 유지하고 Animator가 포즈를 표현한다. 별도 보정 매니저·매프레임 회전 강제 코드·인터페이스가 필요하지 않아 추가하지 않았다. OOP/SOLID에 대한 의도적 예외 없음.

실제 사용자 Editor UI를 조작하지 않았다. 사용자가 관찰한 모든 상태와 정확히 같은 환경에서 확인한 것은 아니므로, SampleScene에서 해당 좌우 회전이 사라졌는지와 보폭/방향 전환 체감은 확인이 필요하다. 기존 무장/비무장 상체 운동과 KayKit 방향별 보조의 스타일 차이까지 없앴다고 주장하지 않는다. 커밋/푸시 없음.
