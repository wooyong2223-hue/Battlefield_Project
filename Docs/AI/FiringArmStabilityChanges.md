# 발사 시 팔 흔들림 수정

2026-10-30. 권총·AK-47·M4A1, 1·3인칭 대상.

## 사전 비교와 조사

이전 `CodexRifleReloadStation/applied.json`과 관련 원본 코드·씬·프리팹·컨트롤러를 비교했으며 변경이 없었다. 기존 CombatTests는 이전 EquipmentAds 적용 해시와 비교한다. 새 작업은 Library 격리 프로젝트에서 진행하며 원본 Editor와 오디오 상태는 조작하지 않는다.

최초 반복 발사 측정에서 AK 3인칭 팔꿈치의 카메라 기준 프레임 간 이동 최대 0.28798m를 기록했다. 먼저 외부 클립 혼합(유지90/사격10), 손 IK의 발사 순간 3cm 이동 제거를 각각/함께 시험했다. 이 조합들만으로는 큰 급변을 해결하지 못했다. 특히 소총은 발사하지 않는 대조군에서도 팔꿈치 방향 급변이 확인됐다. 따라서 특정 반동 값 하나를 원인으로 단정하지 않았다.

최종 수정은 (1) 매 발사마다 전신 팔을 포함한 사격 클립을 0초로 재시작하는 경로 제거, (2) 월드 팔 IK의 일관된 팔꿈치 보조 목표, (3) 1인칭 소총 비조준 자세에도 기존 그립 팔 보정 적용이다. 이 조합에서 큰 급변이 감소했다. 시험용 BlendTree/ShotPoseWeight와 손 IK 반동 제거는 최종 변경에 포함하지 않는다.

## 변경한 모든 파일

경로는 저장소 루트 기준이다. 신규 런타임 스크립트·컴포넌트·직렬화 필드는 추가하지 않는다.

### 1. Assets/Features/PlayerCharacter/Scripts/PlayerCharacterCombatView.cs

`OnFired()`의 기존 호출:

```csharp
_weaponPose.RequireReady();
_recoil = 1f;
Play("PistolShoot", _combat.ShotInterval);
```

변경:

```csharp
_weaponPose.RequireReady();
_recoil = 1f;
_meleeAction = false;
_actionEndsAt = Time.time + _combat.ShotInterval;
```

기존 `Play()`는 `Animator.Play("UpperCombat.PistolShoot", layer, 0f)`로 팔 전체 모션을 매 발사 처음부터 재시작했다. 이제 현재 외부 유지 자세를 계속 재생하고 기존 총 모델 반동과 별도 PlayerCharacterRecoil 시점 반동으로 발사를 표현한다. 발사 준비 유지 시간·근접 상태 해제는 기존 의미를 보존한다. **사격 클립, Animator 상태, OverrideController 참조를 삭제하지 않았지만 일반 발사에서는 그 사격 상태에 진입하지 않는다.** 장전·근접 공격의 `Play()` 호출은 유지한다.

### 2. Assets/Features/PlayerCharacter/Scripts/PlayerCharacterWeaponAimView.cs

`ApplyGrip()`에서 기존 손 IK 목표 설정 전에 팔꿈치 힌트 추가:

```csharp
SetIKHintPositionWeight(RightElbow, 1f);
SetIKHintPositionWeight(LeftElbow, 1f);
rightHint = rightShoulder + character.right * .5f - character.up * .6f;
leftHint  = leftShoulder  - character.right * .5f - character.up * .6f;
```

위 코드는 API/값을 요약한 것으로 실제 구현은 `_animator`, `AvatarIKHint`, `_rightGripShoulder/_leftGripShoulder`를 사용한다. 기존에는 손 위치·회전 목표만 제공하고 팔꿈치 방향은 애니메이션/Unity 추정에 맡겼다. 이제 몸 기준 바깥·아래 방향의 보조 목표로 팔이 반대 방향으로 급하게 꺾이는 것을 줄인다.

`SetWeight()` 시작에서 두 힌트 가중치를 0으로 초기화한다. 그립을 적용하는 평가에서만 다시 1로 설정하므로 장전·근접 모션에 고정 힌트를 강제로 남기지 않는다. 기존 손 목표·도달 범위·3cm 손 반동·기존 serialized 값은 변경하지 않는다. FP source arms의 별도 LateUpdate solver도 유지한다.

### 3. Assets/Features/PlayerCharacter/Scripts/FirstPersonArmsView.cs

기존 소총 복제 팔 보정 가중치:

```csharp
Max(adsBlend, reloadBlend) * (1f - melee)
```

변경:

```csharp
Max(presentation.UsesItemGrip ? 1f : adsBlend, reloadBlend) * (1f - melee)
```

기존에는 ADS·장전 때만 복제 팔의 어깨/팔꿈치 방향을 보정했다. 이제 소총 그립 중에는 비조준에서도 적용한다. 기존 `_adsShoulder=(.45,-.4,-.1)`, `_adsElbowHint=(.65,-.6,.15)`를 그대로 사용하고 왼쪽은 X를 반전한다. 값 자체를 수정하지 않는다. 양손·총의 위치를 소유하는 기존 그립, 마우스 상하 조준, 월드 스켈레톤, 카메라를 덮어쓰지 않는다. 근접 가중치와 장전의 기존 보정 경로를 유지한다. 권총은 `custom` 소총 분기 대상이 아니다.

### 4. Assets/Tests/PlayMode/PlayerCharacterCombatTests.cs

기존 `UpperBodyActionStatesPlayAndCrouchRemainsOnBaseLayer`를 `FiringKeepsGripWhileReloadAndMeleePlayTheirActionStates`로 이름 변경. 발사 후 PistolShoot 상태를 기대하던 assertion을 **PistolHold 유지 + RecoilAmount > 0**으로 교체했다. 의도적으로 바뀐 발사 표현 계약을 검증하기 위한 변경이며, 장전 PistolReload·근접 PunchJab·앉기 검증은 그대로 유지한다. 기존 수치 허용오차는 완화하지 않았다.

### 5–6. Assets/Tests/PlayMode/FiringArmStabilityTests.cs 및 .meta (신규)

AK/M4/권총 × 1/3인칭 × 비발사/반복발사 대조군 검사. 실제 SampleScene의 Animator·IK·복제 팔을 사용한다. 발사 표현만 분리하려고 OnFired를 호출하며 별도 카메라 반동과 탄환 물리를 이 측정에 섞지 않는다. 기존 실제 발사·탄약·피해 회귀 검사는 별도로 전체 suite에서 유지한다.

고정 20ms 평가에서 두 팔꿈치의 카메라 기준 프레임 간 이동을 측정한다. FP 최대 2.5cm 미만, TP 8cm 미만이며 발사로 추가되는 최대 이동은 같은 자세의 대조군 +2.5cm 미만이어야 한다. 이 값은 기존 테스트의 허용오차 변경이 아니라 새로운 급변 회귀 기준이다. 원본 재현값 28.8cm는 이 기준을 통과하지 못한다. 총 반동이 매 발사 살아 있고 종료 후 0으로 회복되며 PistolHold를 유지하는지도 검사한다. meta는 테스트 GUID를 위한 파일이다.

### 7–8. 문서

- `Docs/AI/FiringArmStabilityChanges.md`: 본 파일. 전체 파일/교체 코드/이유/검증·한계 기록.
- `Docs/AI/UnityProjectContext.md`: 현재 동작과 증거 경로를 덧붙임. 기존 기록 유지.

## 변경하지 않은 에셋·기능과 설계

씬·프리팹·AnimatorController·원본/파생 AnimationClip·무기 정의·입력·오디오 설정 변경 없음. 클립/상태/메서드/필드/컴포넌트/오브젝트 삭제 없음. 제거한 실행 경로는 OnFired의 PistolShoot 재생 호출뿐이며 명시적 상태 유지로 교체했다. 시험 코드/생성 도구는 Library에서만 보관한다.

발사 표현의 이벤트 반응은 기존 CombatView, 월드 팔 IK는 WeaponAimView, 복제 팔 표현은 FirstPersonArmsView 책임이다. 새로운 상태 소유자나 Manager를 만들 필요가 없어 기존 책임 범위 안에서 수정했다. 상속/인터페이스를 새로 추가하지 않았다. 입력·탄약·피해 같은 게임플레이 책임은 표현 코드로 이동시키지 않았다.

## 검증·한계

Unity6000.4.8f1 격리 `-batchmode -noaudio`. 단계별 대조 측정은 `Library/CodexFiringArms/`의 regressionBefore/compare/details/idleCompare/poles 로그 및 XML에 보관한다. 최종 전체 결과는 final.xml, 화면은 AK47/M4A1/Pistol의 FP/TP PNG를 따른다. 원본 적용 전 후속 변경을 해시로 재확인하고 검증본과 바이트 일치로 적용한다.

최종 전체 PlayMode **172/172 통과**, 실패/스킵0개,144.6초. 추가 실제 발사 촬영 검사1/1 통과 및 AK/M4/권총의 FP/TP 화면6장 검토. 촬영 전용 테스트는 Library로 옮겨 제품에서 제외했다. 테스트/코드6개 파일은 후속 사용자 변경이 없는 것을 확인한 뒤 격리 검증본과 바이트 동일하게 적용했다. 씬·AnimatorController·InputAction·AK/M4 프리팹은 원본과 동일함을 확인했다. 기존 테스트에서는 의도적으로 변경한 상태 기대값만 교체했으며 기존 수치 허용오차를 완화하지 않았다. 스크립트 git diff --check 통과, 적용 후 해시 재검증 통과. 문서2개를 포함해 변경 파일 총8개다.

큰 팔꿈치 급변을 줄이는 검증이며 모든 조준 각도/속도·손가락 접촉·메시 관통을 보장하지 않는다. 실제 Editor에서 마우스 상하 조준·이동 중 발사·장전 전후·근접 전환의 자연스러움을 확인해야 한다. 타깃 Player 빌드와 사운드 청취 테스트는 실행하지 않는다.
