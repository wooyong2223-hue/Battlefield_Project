# 상하 조준 시 상체 자세 개선 — 2026-10-27

후속 변경: 몸통만 보간되어 총과 추적 시점이 달랐던 부분은 [총·상체 조준 동기화](PlayerCharacterAimSynchronizationChanges.md)에서 수정했다. 아래 보간 설명과 69개 검증은 그 이전 단계의 작업 기록이다.

3인칭에서 위아래 조준 시 손·총만 움직이던 동작을 몸통·목·어깨·팔이 함께 움직이는 자세로 확장했다. 기본 이동/앉기 자세와 캐릭터 루트는 유지하고, 기본 레이어의 Humanoid LookAt 뒤에 전투 레이어의 양손 IK를 적용한다.

아래 파일 경로는 E:/GitHub/Battlefield_Project 기준이다.

## 작업 전 비교와 구조 선택

SampleScene, Animator Controller, UpperCombat.mask, WeaponAimView/CombatView/View가 직전 검증본과 바이트 단위로 같았다. 새 사용자/Editor 저장 변경이나 충돌하는 설정은 없었다. 기존 클래스가 조준 자세를 소유하고 있어 몸통/목 조준도 같은 PlayerCharacterWeaponAimView에서 처리했다. 별도 입력/게임플레이 책임이나 독립 수명이 없어 새 컴포넌트로 분리하지 않았다.

## 이번에 변경한 모든 제품 파일

| 파일 | 변경 |
|---|---|
| Assets/Features/PlayerCharacter/Scripts/PlayerCharacterWeaponAimView.cs | Base Layer OnAnimatorIK 분기와 ApplyUpperBodyAim 추가. SetLookAtWeight/SetLookAtPosition으로 몸통·머리를 조준 방향에 분담하고 지수 보간으로 피치 추적, 가중치 진입/해제를 부드럽게 처리한다. 기존 UpperCombat의 손 IK·아바타 손 축 보정은 유지. 손의 세로 오프셋을 회전에서 분리하고, 깊은 하향 조준의 최소 전방 위치와 극단 각도의 도달 거리를 제한한다. 양팔 길이를 Awake에서 계산하고, 양손이 공유하는 목표를 양쪽 도달 범위에 네 번 투영해 그립이 벌어지지 않도록 한다. OnDisable에서 몸통 보간 상태도 초기화한다. |
| Assets/Features/PlayerCharacter/Animation/PlayerCharacter.controller | Base Layer m_IKPass: 0→1. UpperCombat m_IKPass=1 유지. 다른 상태·전환·클립·마스크·파라미터는 변경하지 않았다. |
| Assets/Scenes/SampleScene.unity | 기존 PlayerCharacterWeaponAimView에 신규 설정 5개만 직렬화: _bodyAimWeight=0.3, _headAimWeight=0.85, _lookAtClamp=0.2, _poseFollowSpeed=12, _minimumDownwardReach=0.35. 기존 참조, 손 오프셋, 오브젝트 위치/크기/계층 유지. |
| Assets/Tests/PlayMode/PlayerCharacterCombatTests.cs | UpperBodyAndHeadParticipateInVerticalAimingWithoutRotatingTheCharacterRoot 추가: 상하 60도에서 실제 가슴/머리 회전과 중립 복귀를 확인하고 루트 회전 불변·총구 오차 2도 미만 검사. BothHandsKeepTheirGripAtMaximumVerticalAim 추가: ±80도에서 중립 대비 손 간격 차이 0.04m 이내 및 총구 오차 2도 미만 검사. 기존 검사 삭제 없음. |
| Docs/AI/PlayerCharacterUpperBodyAimChanges.md | 이 변경 보고서. |
| Docs/AI/PlayerCharacterCombatChanges.md | 이번 개선과 보고서 링크 추가. |
| Docs/AI/UnityProjectContext.md | 현재 상체 조준 순서·설정·검증 결과 추가. |

스크립트/에셋 메타와 GUID는 변경하지 않았다. 기존 컴포넌트·필드·참조·메서드·클립은 삭제하지 않았다. Scene에는 새 컴포넌트를 추가하지 않았다. Range 속성은 사용하지 않고, LookAt 가중치는 호출 시 Clamp01로 처리한다.

## 교체 전후

기존 손 목표는 눈 기준 오프셋 전체를 회전했다.

```csharp
// 이전: 몸통/목은 고정, 손의 높이 오프셋까지 회전
rightPosition = EyePosition + aim * Scale(rightHandOffset, scale);
```

예를 들어 y=-0.4, z=0.9 오프셋을 아래 80도로 회전하면 손 목표의 전방 위치가 음수가 되어 몸 뒤로 돌아갈 수 있었다.

```csharp
// 이후: 몸통/목을 먼저 추적시키고, 어깨 높이 기준에서 팔을 회전
pivot = EyePosition + characterUp * (rightHandOffset.y * scale.y);
rightPosition = pivot + aim * Scale(new Vector3(x, 0f, reach), scale);
// 하향 전방 거리 제한 → 양팔 공동 도달 범위 보정 → 양손 IK
```

회전하던 기존 눈 오프셋 공식만 위 계산으로 교체했다. 중립 자세의 오프셋 값 (0.35,-0.4,0.9), 양손 접점 데이터, 손잡이 회전, 반동 소스, 조준/발사 방향의 소유자는 유지한다. 양팔 도달 제한은 관절 과신전과 위쪽 조준에서 왼손이 그립에서 떨어지는 문제를 줄인다.

## 동작과 설정

1. 기본 레이어 IK에서 몸통 0.3/머리 0.85 가중치로 시선을 조준 방향에 맞춘다. LookAt clamp=0.2, 피치 보간 속도=12.
2. 전투 레이어 손 IK에서 총구의 최종 방향을 맞추고 왼손의 접점을 유지한다.
3. 45~80도 구간에서는 전방 팔 목표 길이를 기존 값의 100~75%로 줄이고, 양팔 최대 도달 길이는 100~92%로 제한해 관절을 펴서 버티는 동작을 줄인다.
4. 아래 조준 시 최소 전방 거리는 루트 기준 0.35다. 최대 상하 시점 제한 80도 자체는 변경하지 않았다.
5. 장전·근접 공격에서는 손 IK를 풀고 몸통 조준 가중치를 내려 기존 모션으로 복귀한다. 1인칭/비활성/사망에서는 몸통 LookAt 가중치를 즉시 0으로 한다. 탑승 시 기존 on-foot 목록으로 함께 중단한다.

입력, 카메라 위치/FOV, 캐릭터 이동/캡슐/루트/모델 크기, 전투기, 피해/탄약/장전 시간은 변경하지 않았다. 표현 상태는 private 필드로 소유하며 게임플레이 상태를 변경하지 않는다. 새 추상화나 상속 없이 기존 명시적 참조로 조합한다. OOP/SOLID 설계 예외는 없다.

## 검증

- 수정 전 Temp/codex-upper-body-before.xml: 새 몸통 회전 검사가 가슴 0.728도 변화로 실패해 손만 움직이던 증상을 재현.
- 최종 Temp/codex-upper-body-validated.xml: 제품 PlayMode 68개 + 임시 화면 캡처 1개, 총 69/69 통과(42.48초). 기존 시점/이동/앉기/발사/장전/근접/팀/엄폐/탑승 검사 포함. 새 컴파일/런타임 오류 없음.
- 실제 SampleScene의 지면 접촉과 정지 자세를 확인한 뒤 0도, 위/아래 60도, 위/아래 80도, 앉은 하향 조준을 캡처했다.

| 조준 | 가슴 회전 변화 | 머리 회전 변화 | 총구 방향 오차 | 양손 뼈 간격 |
|---|---:|---:|---:|---:|
| 위 60도 | 27.94도 | 59.08도 | 0도 | 약 0.2543m |
| 아래 60도 | 33.59도 | 59.34도 | 0도 | 약 0.2543m |
| 위 80도 | 40.97도 | 78.54도 | 0도 | 약 0.2543m |
| 아래 80도 | 44.19도 | 79.25도 | 0도 | 약 0.2543m |

몸통 회전값은 중립 자세 대비 가슴 뼈의 Quaternion 각도 변화이며, 손 간격은 실제 양 손목 뼈의 거리다. 화면은 Temp/codex-upper-up.png, down.png, up-max.png, down-max.png, down-max-game.png, crouch-down.png에서 확인했다. 임시 캡처 스크립트는 제품 Assets에서 제외하고 Temp에만 보관한다.

원본 Editor의 저장되지 않은 상태는 건드리지 않았다. 수정 파일은 검증본과 일치하고 Scene/Controller는 필요한 필드/플래그만 부분 패치했다. 사용자 Editor에서는 SampleScene을 다시 연 뒤 실제 마우스 조작감과 원하는 몸통/목 분담 정도를 최종 확인하면 된다. 별도 플레이어 빌드와 커밋/푸시는 실행하지 않았다.
