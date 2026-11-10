# 이동 중 과도한 몸통·어깨 회전 완화

2026-10-29. 사용자가 골반 회전 수정 후에도 몸통·어깨가 좌우로 과하게 흔들린다고 확인했다. 골반 수정은 유지하고, 이동 중 최종 상체 포즈의 좌우 회전을 줄였다. Quaternius 모션 자체를 교체하지 않았다.

## 변경 전 비교와 확인

직전 작업의 검증본과 이동 클립4개/회귀 테스트/AnimationView/CombatView/WeaponAimView/Movement를 비교했으며 관련 사용자·Editor 변경은 없었다. 이번 baseline은 Library/CodexTorsoGait/baseline.json. 원본 반영 직전에 모든 baseline 파일이 동일한 해시인지 확인했다. 기존 미커밋 변경과 설정값은 보존했다.

이전 작업의 회전 Bake 설정을 적용한 상태에서도, 실제 게임 레이어를 포함한 측정에서 권총 조깅의 골반은 약±2도 이내인데 어깨는 -65.3°~+21.8°로 움직였다. Quaternius 원본 조깅 전신의 큰 상체 회전에 무장 상체 레이어가 결합된 결과였다. 따라서 골반/루트를 다시 고정하거나 클립을 바꾸지 않고 최종 상체 포즈에만 보정을 적용한다.

## 모든 변경 파일

### Assets/Features/PlayerCharacter/Scripts/PlayerCharacterWeaponAimView.cs

선택한 구조: 기존 OnAnimatorIK 조준 조정기가 이미 척추·가슴·손 IK·머리 시선을 처리하며 필요한 참조를 소유한다. 같은 포즈 평가 안에서의 작은 FK 보정이므로 별도 컴포넌트·매니저·인터페이스·씬 참조를 추가하지 않았다.

추가한 데이터:

- `_locomotionHead`: Awake에서 캐시하는 Head Transform. 보정 전의 머리 방향 보존용.
- `LocomotionTorsoYawLimit = 12f`: 좌우 흔들림의 부드러운 압축 한계(도). 기존 사용자 설정의 교체값이 아니며 새 보정의 내부 상수다. 피크를 딱 잘라 멈추는 하드 클램프가 아니다.
- `GaitSpeed`: 기존 Animator Speed 파라미터의 캐시된 해시. 새 Animator 파라미터는 추가하지 않는다.

새 메서드:

- `LocomotionTorsoYawCorrection()`: 좌우 어깨 사이 벡터를 캐릭터 up 평면에 투영해 현재 yaw를 계산한다. 목표 yaw와 현재 yaw의 차이를 반환한다. 이동/충돌/시점 상태를 변경하지 않는다.
- `StabilizeLocomotionTorso()`: 무장 상체 레이어의 최종 포즈에 보정 적용.
- `ApplyTorsoRotation(pitch, yaw)`: 척추와 가슴에 회전을 절반씩 배분하고, 추가 yaw의 반대 회전을 머리에 적용해 기존 시선을 유지한다.
- `YawBone()`: 머리의 yaw 보정 적용.

핵심 계산:

```csharp
float target = LocomotionTorsoYawLimit * (2f / Mathf.PI)
    * Mathf.Atan(yaw / LocomotionTorsoYawLimit);
return (target - yaw) * weight;
```

원본 모션의 흔들림 타이밍과 일부 움직임은 남기면서 큰 회전량을 압축한다. `weight = InverseLerp(0.05, 0.3, Speed)`이며 기존 Speed damping으로 출발·정지 시 보정도 페이드된다. 별도 누적 타이머나 회전 상태는 만들지 않았다.

적용 범위: 살아 있고 공격 가능한 상태, 지상 Locomotion/CrouchLocomotion, 포복이 아닌 자세, Speed가 있는 경우. 실제 공격/장전 중에는 즉시 제외한다. 점프·슬라이드·Vault·사망·소생 등 다른 base state에서도 제외한다. 이동 중 소유 시점과 관계없이 적용하며, 무장에서는 UpperCombat 평가에, 비무장에서는 MeleeCombat의 가드 포즈 평가에 적용한다. 원본 기본/앉은 이동·클립·골반·루트는 바꾸지 않는다.

변경·대체한 기존 코드:

```csharp
// 이전 MeleeCombat의 상하 조준
PitchBone(HumanBodyBones.Spine, _meleeSpine, meleePitch * .5f);
PitchBone(_meleeChestId, _meleeChest, meleePitch * .5f);

// 이후: 같은 pitch와 이동 중 yaw를 한 번에 합성
ApplyTorsoRotation(meleePitch, gaitYaw);
```

기존 `PitchBone(id, bone, pitch)`를 `PitchBone(id, bone, pitch, yaw = 0f)`로 확장했다. 회전은 부모 좌표계에서 `yaw * pitch * localRotation`으로 합성한다. yaw=0이면 기존 상하 조준을 유지한다. 분리된 SetBoneLocalRotation 호출로 pitch/yaw를 순서대로 쓰면 같은 IK 평가 안에서 앞의 값을 덮을 수 있어 한 번에 합성한다. 초기 실험에서 비무장 보정이 덮이는 현상을 확인해 최종 구현에서 해결했다.

기존 메서드/필드/컴포넌트/참조/에셋을 삭제하지 않았다. 손 목표·그립·반동·LookAt weight와 사용자 설정을 변경하지 않았다. 기존의 상하 조준 호출 두 개를 합성 호출로 교체한 내용과 PitchBone 확장이 위에 모두 명시돼 있다.

### Assets/Tests/PlayMode/PlayerCharacterTorsoGaitTests.cs 및 .meta (신규)

- ArmedGaitsLimitShoulderSwingInBothPerspectives
- UnarmedGaitsLimitShoulderSwingInBothPerspectives

실제 CharacterController의 walk/jog/sprint/left/diagonal/crouch를 각180프레임 평가하고 초반30프레임 뒤 어깨 방향이 정면에서14도 이내인지, 루트/모델 회전이 보존되는지 검사한다. 1·3인칭 모두 확인한다. 무장에서는 머리 yaw가5도 이내인지도 검사한다. 수평 조준으로 먼저 전체 회귀를 실행한 뒤, 테스트를 +30°/-30° 상하 조준으로 확장해 두 테스트를 다시 실행했다. 최종 테스트에는 무장 머리 방향의 상하 부호/도달량 검사도 포함돼 있다.

### Docs/AI/PlayerCharacterTorsoGaitStabilization.md (신규)

이 문서. 변경 전후·파일·함수·원인·검증·남은 확인 기록.

### Docs/AI/UnityProjectContext.md

이동 상체 보정 단락 추가. 책임/적용 조건/시선 보존/검증 증거 기록.

## 검증과 결과

격리 Unity6000.4.8f1에서 측정한 정면 조깅의 범위:

| 최종 포즈 | 이전 어깨 yaw | 이후 어깨 yaw |
|---|---|---|
| 권총 조깅 | -65.3°~+21.8° | 약 -10.7°~+8.3° |
| 비무장 조깅 | -13.9°~+22.7° | 약 -7.4°~+7.7° |

골반 범위는 약 -1.4°~+1.7°로 유지됐고 권총의 머리 yaw도 약0도를 유지했다. 기본 레이어만 재생하면 원래 큰 상체 운동이 유지됨을 비교해 클립 자체를 변경하지 않았음을 확인했다. idle은 이전 포즈를 유지한다.

- before.json / after-composed.json: 무장·비무장·상체 레이어 비활성 비교. 초기 after.json은 비무장 yaw가 pitch에 덮이던 중간 실험이며 최종 결과가 아니다.
- full.xml/log: 전체129/129 통과(제품127개 + 임시 캡처2개). 수평 조준의 신규 이동 상체 검사2개 포함. 이동·회전·조준·근접·장전·발사·자세·탑승의 기존 회귀도 통과했다.
- pitched.xml/log: 최종 신규 테스트2/2 추가 통과. 1·3인칭에서 위/아래를 조준하면서 어깨 제한 및 머리 방향 검증. 두 실행 사이 런타임 수정 없음; 테스트에 상하 조준 범위만 추가했다.
- jog/sprint/left.png 게임 화면 확인. 증거 파일은 모두 Library/CodexTorsoGait에 있다.
- 원본 반영 전 baseline 전체 보존 확인. 검증한 런타임 파일과 최종 테스트/.meta를 그대로 반영. 씬/Controller/모든 애니메이션 클립·사용자 수치 변경 없음.

## 설계 검토·제한

포즈 표현 책임 안에서 합성했고, 가속·이동·게임플레이 상태의 소유권은 기존 컴포넌트에 남아 있다. 새 상속·인터페이스·전역 관리자는 필요하지 않다. OOP/SOLID에 대한 의도적 예외 없음. 추가 검색/객체 생성/매프레임 컬렉션 생성 없이 기존 캐시된 뼈와 Animator를 사용한다. 성능 개선을 측정했다고 주장하지 않는다.

사용자 Editor UI는 조작하지 않았다. SampleScene에서 어깨 흔들림의 체감, 출발·정지와 행동 전환의 자연스러움은 사용자 환경 확인이 필요하다. 이 작업은 지상 이동 상체 보정이며 모든 원본 애니메이션의 스타일이나 발 미끄러짐까지 수정한 것은 아니다. 커밋·푸시 없음.
