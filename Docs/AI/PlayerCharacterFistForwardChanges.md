> 이전 IK 구현 기록입니다. 현재 구현은 [모션 캡처 기반 근접 공격 교체](PlayerCharacterMocapMeleeChanges.md)를 참고하세요. 이 문서의 과거 검증 기록은 보존합니다.

# 주먹 공격 방향 보정

## 확인한 문제와 범위

1인칭·3인칭에서 기존 주먹 모션이 전방보다 옆 방향으로 크게 움직였다. 원본 측정에서는 왼손 약 1m, 오른손 약 0.85m의 측방 변위가 나타났다. 추가로 FirstPersonArmsView가 주먹에도 권총용 회전 보정을 적용했다. 판정 방향을 바꾸거나 캐릭터 루트를 회전시켜 해결하지 않는다.

작업 시작 시점의 CombatView, WeaponAimView, FirstPersonArmsView, AnimatorController, SampleScene 해시와 반영 직전 파일을 비교한다. 관련 사용자 변경은 덮어쓰지 않는다. 전체 저장소에는 이전 작업의 미커밋 변경이 다수 있으며 이번 변경 범위에 포함하지 않는다.

## 변경 파일과 코드

| 파일 | 변경 내용·이유 |
| --- | --- |
| Assets/Features/PlayerCharacter/Scripts/PlayerCharacterFistPose.cs | 새 일반 C# 자세 클래스. 기존 MeleeStarted 이벤트로 좌우 교대와 공격 시작 시간을 기억한다. 손목·팔꿈치 IK 목표 계산만 소유하며 피해·입력·공격 허용 여부를 변경하지 않는다. |
| Assets/Features/PlayerCharacter/Scripts/PlayerCharacterFistPose.cs.meta | 새 스크립트의 Unity GUID. 새 컴포넌트를 씬에 추가하지 않는다. |
| Assets/Features/PlayerCharacter/Scripts/PlayerCharacterWeaponAimView.cs | Awake에서 자세 객체 구성. OnEnable에서 공격·장비 이벤트 구독, OnDisable에서 해제·초기화. UpperCombat IK 패스에서 비무장일 때 자세 객체 호출. 권총 경로는 유지하고 매 평가 시작 시 양 팔꿈치 힌트 가중치를 0으로 초기화해 주먹 힌트가 권총에 남지 않게 한다. |
| Assets/Features/PlayerCharacter/Scripts/FirstPersonArmsView.cs | CopyPose의 회전 기준·위치 보정·복사 원점을 무장 여부에 따라 선택한다. 권총용 보정이 주먹 전방 축에 섞이지 않도록 분리한다. |
| Assets/Features/PlayerCharacter/Scripts/PlayerCharacterController.cs | 읽기 전용 MeleeWindup 추가. 현재 선택한 근접 공격의 Windup을 전달한다. 공격 처리 변경 없음. |
| Assets/Features/Weapon/Scrips/MeleeAttack.cs | 읽기 전용 Windup => _windup 추가. 표시 코드가 실제 판정 시간을 읽게 한다. 피해·팀 판정·범위·예약·취소 코드는 유지한다. |
| Assets/Scenes/SampleScene.unity | 기존 FirstPersonArmsView(fileID 1653572103)에 `_unarmedOffset: {x: 0, y: 0.75, z: 0.1}` 한 필드만 추가한다. 기존 카메라·권총 값·계층·컴포넌트 참조를 유지한다. |
| Assets/Tests/PlayMode/PlayerCharacterFistAnimationTests.cs | 실제 Animator를 이용해 루트 yaw 53도에서 양손 교대, 양 시점 전방 뻗기·측방 편차·타격 시점·1인칭 준비/타격 손목 노출·루트 회전 불변을 검사한다. |
| Assets/Tests/PlayMode/PlayerCharacterFistAnimationTests.cs.meta | 새 회귀 검사의 Unity GUID. |
| Docs/AI/PlayerCharacterFistForwardChanges.md | 이 변경 보고서. |
| Docs/AI/UnityProjectContext.md | 이번 자세 보정 및 검증 근거를 기록한다. |

상체 간헐 현상 조사에서 추가한 진단 파일 네 개와 별도 보고서는 PlayerCharacterAimRuntimeInvestigation.md에 기록한다.

## 위치·시간 계산

어깨 두 개의 평균 높이를 사용하되, 목표의 앞뒤 원점은 캐릭터 루트에 고정한다. 기존 모션에서 어깨가 뒤로 움직여도 준비 주먹이 카메라 뒤로 밀리지 않게 한다. 아래 벡터는 캐릭터 배율을 적용하기 전 값이다.

- 준비 위치: `(좌우 ±0.32, 0.15, 0.45)`.
- 타격 위치: `(좌우 ±0.12, 0.15, 1.0)`.
- 준비→타격: 실제 Windup 동안 SmoothStep으로 전진한다.
- 복귀: `(Duration - Windup) × 0.5` 동안 SmoothStep으로 돌아온다. 남은 회복 시간에는 준비 자세를 유지한다.
- 팔 길이의 97% 안으로 목표를 제한한다. 손목 방향은 손→중지 첫 마디 방향이 전방을 향하도록 보정하며, 아바타의 IK 회전 축 보정은 기존 조준 계산에서 전달한다.
- 3인칭은 조준 회전으로 목표를 만든다. 1인칭의 원본 몸은 수평 방향으로 계산하고 복사된 팔에서 상하 회전을 한 번 적용한다.

현재 설정은 피해 20, Windup 0.2초, Recovery 0.6초, Duration 0.8초로 유지한다. 표시 객체는 이 값을 읽기만 한다.

## CopyPose 교체 부분

기존에는 모든 장비에 다음 권총 보정을 사용했다.

```csharp
rotation = aim * motion * Quaternion.Euler(_poseEuler)
           * Quaternion.Inverse(_poseAnchor.rotation);
offset = Vector3.Lerp(_offset, _adsOffset, blend);
anchor = _poseAnchor.position;
```

변경 후 권총은 같은 계산을 유지하고 주먹만 분기한다.

```csharp
basis = _combat.IsArmed
    ? Quaternion.Euler(_poseEuler) * Quaternion.Inverse(_poseAnchor.rotation)
    : Quaternion.Inverse(_characterRoot.rotation);
authoredOffset = _combat.IsArmed
    ? Vector3.Lerp(_offset, _adsOffset, blend) : _unarmedOffset;
anchor = _combat.IsArmed ? _poseAnchor.position : _eye.position;
```

삭제한 기존 메서드·필드·컴포넌트·참조·에셋은 없다. CopyPose의 위 지역 계산만 조건부로 교체했다. PunchJab/PunchCross/FistsHold/PistolMelee 상태, 원본 Quaternius 클립, 재생 속도, 상태 연결과 CombatView의 교대 코드는 유지한다. 검토용 대체 모션·FBX·캡처 도구는 Library의 격리 프로젝트에만 있으며 제품 Assets에는 추가하지 않는다.

## 설계 검토

별도 MonoBehaviour를 늘리지 않고 기존 IK 평가 지점에서 일반 C# 자세 객체를 합성한다. 공격 상태와 손목 목표는 자세 객체, 판정은 MeleeAttack, 선택·이벤트는 Controller, 눈 기준 복사는 FirstPersonArmsView가 담당한다. 내부 상태는 비공개이며 읽기 전용 공격 시간만 노출한다. 실제 대체 구현이 없어 새 인터페이스·상속 계층은 추가하지 않았다. 기존 구체 컴포넌트 간 연결을 유지하는 의도적 선택이다.

## 검증

검증은 사용자의 열린 Editor를 조작하지 않고 Library/CodexInfantryWork/Project에서 수행했다.

- Unity 6000.4.8f1에서 전체 114/114 통과: 제품 PlayMode 검사 113개와 임시 렌더 캡처 검사 1개. 결과는 Library/CodexFistForward/verified-all.xml 및 verified-all.log에 있다.
- 양 시점에서 좌우 손목의 어깨 기준 최대 전방 도달 약 1.13~1.14m, 측방 편차 약 0.09~0.21m를 확인했다. 실제 Windup ±0.06초 구간에서 전방 도달 0.8m 이상도 검사했다. 움직이는 어깨 기준의 최대치 시각만으로 타격 동기화를 판정하지 않는다.
- 준비 자세와 좌우 타격을 1·3인칭에서 각각 렌더해 6개 PNG를 확인했다. Library/CodexFistForward/{0,1}-{guard,0-strike,1-strike}.png. 1은 1인칭이다.
- 기존 권총 발사·장전·ADS·상체 조준·장비 전환·근접 판정·차량 탑승 등의 회귀 검사를 포함한다. 이번 변경은 피해나 발사 동작을 추가하지 않으므로 더미 판정 구현을 새로 통합하지 않았다.
- 원본 반영 전 시작 해시를 다시 검사했다. 컨트롤러와 원본 클립은 유지했고, 씬은 기존 FirstPersonArmsView의 필드 한 항목만 추가했다. 스크립트의 격리 검증본과 원본 반영본은 주석·줄 끝 공백을 제외한 코드가 같다.

SampleScene의 실제 Editor에서 2번 슬롯으로 주먹 선택 후 공격하고 V로 시점을 전환해 체감을 확인해야 한다. 상하·좌우 조준, 이동·앉기 중 반복 공격, 권총 전환 후 조준을 함께 확인한다. 격리 자동 검사·정지 렌더는 사용자가 느끼는 동작의 자연스러움 전체를 대신하지 않는다. 상체 간헐 현상은 원인 미확정이며 별도 진단 보고서를 참고한다. 커밋·푸시는 수행하지 않았다.

