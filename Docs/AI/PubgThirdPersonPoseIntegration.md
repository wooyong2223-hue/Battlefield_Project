# PUBG 3인칭 기준 — 무기 자세 구조 개선

2026-10-29. 사용자가 PUBG 3인칭을 단일 기준으로 선택했다. 이전의 상체 흔들림 제한만으로는 해결되지 않은 구조적 차이(항상 조준 높이의 권총, 항상 활성인 비무장 가드)를 수정했다. 이 기록은 완전히 동일한 PUBG 애니메이션을 구현했다는 완료 선언이 아니다.

## 실제 비교 자료와 차이

- [PUBG 공식 Dev Letter: Animation Refresh](https://www.pubg.com/en/news/1565), 2022-05-17. 공식 TPP 비교 GIF의 전체300프레임/30초를 구간별로 확인했다. 이동/준비 자세와 총을 올린 조준 자세가 구분되며 속도 전환에 체중 이동이 있다.
- [PUBG 공식 Update19.2](https://pubg.com/en-asia/news/1363), 2022-08-31. 공식 TPP locomotion GIF189프레임/5.67초의 측면 출발·이동·정지를 확인했다. 자료는 소총 기반이므로 현재 권총의 손 그립을 동일하게 검증한 자료는 아니다. 현재 패치의 모든 움직임과 같다고 주장하지 않는다.
- [Why So Wookie — Animation Refresh Overview](https://www.youtube.com/watch?v=UqI70TgC3v8), 2022-05-25. 영상 전체를10초 간격으로 확인했고 TPP 소개/공식 비교 구간을 맥락으로 사용했다. 독립적인 현재 권총 TPP 실측 자료로 간주하지 않는다.
- Quaternius에 있는 기본 이동·권총·근접 클립 우선 정책은 유지했다. 다른 캐릭터 모델/외부 모션을 새로 대체하지 않았다. Toon Shooter Kit 등은 후보 탐색만 했고 받거나 적용하지 않았다.

기존 구현은 일반 이동에서도 총이 조준선에 고정됐고 비무장에서는 FistsHold 레이어가 항상 켜졌다. 이는 다리만 움직이고 상체가 고정된 인상을 만들었다. 이번에는 휴대·질주·조준·동작 상태를 분리했다. 숫자는 현재 모델의 프로젝트용 튜닝값이며 영상에서 PUBG의3D 내부 수치를 추출한 값이 아니다.

## 변경 전 비교

직전 검증본과 AimView/상체 테스트/.meta를 비교했고 동일했다. Library/CodexPubgLocomotion/baseline.json에 현재 캐릭터·테스트·씬 파일 해시를 저장했다. 원본 반영 직전 관련 모든 파일이 시작 해시와 같은지 다시 확인했다. 사용자·Editor의 관련 변경은 발견되지 않았다. 기존 미커밋 변경을 보존한다.

## 모든 변경 파일

### Assets/Features/PlayerCharacter/Scripts/PlayerCharacterWeaponPoseState.cs 및 .meta (신규)

Unity 컴포넌트가 아닌 작은 독립 상태 객체다. 모드(예: Carry/Sprint/Ready), 부드러운 전환, 발사 후 준비 자세 유지 시간만 소유한다. 입력·이동·조준·공격 권한을 소유하지 않는다. 새 씬 오브젝트/컴포넌트/매니저/직렬화 참조는 추가하지 않았다.

- `CharacterWeaponPoseMode`: Hidden / FirstPerson / Ready / Carry / Sprint.
- `Tick(mode, deltaTime)`: `1-exp(-15*dt)`로 CarryBlend/SprintBlend를 혼합. FirstPerson은 즉시 원래 준비 포즈로 복귀, Hidden은 초기화한다.
- `RequireReady()`: 발사 순간 휴대 혼합을0으로 만들고0.35초 준비 자세를 유지한다.
- `Reset()`: 혼합과 남은 준비 시간을 초기화한다.
- `Rotation(aim)`: 준비 상태는 기존 시점 회전 그대로. 휴대 상태는35도, 질주는55도 아래 방향으로 보간한다. 고개를 위로 보더라도 비조준 휴대 자세가 뒤집히지 않게 현재 시점 pitch와 목표 휴대 pitch를 혼합한다.
- ForwardScale: 준비1 / 휴대0.8 / 질주0.6. 추가 높이 오프셋: 준비0 / 휴대-0.1 / 질주-0.2.
- 질주가 완전히 혼합되면 UpperCombat weight는1→0.3, 보조 손 IK weight는1→0.1. 오른손은 무기를 유지하고 왼팔은 기본 이동 모션에 더 가깝게 움직인다.

### Assets/Features/PlayerCharacter/Scripts/PlayerCharacterCombatView.cs

추가: readonly `_weaponPose`, 기존 Speed 파라미터 해시 `GaitSpeed`, Awake에서 명시적 `_combat` 소유 루트로부터 캐시하는 optional `_stance`. 씬 참조를 추가하지 않았다. 외부에는 WeaponForwardScale/WeaponVerticalOffset/SupportHandWeight와 WeaponPoseRotation()의 읽기 전용 결과만 제공한다. 상태 객체 자체를 노출해 다른 컴포넌트가 Reset/RequireReady를 직접 호출하게 만들지 않았다.

`LateUpdate()` 변경:

```text
이전: 무장 UpperCombat weight = 1
      비무장 MeleeCombat target = 1 (항상 가드)
이후: 휴대/질주/조준 모드에 따른 UpperCombat 혼합
      비무장 3인칭 평소에는 기본 모션의 팔 사용
      조준·실제 근접 동작·1인칭에서는 가드/공격 레이어 사용
```

우선순위: 표현 불가/비무장 → Hidden; 1인칭 → FirstPerson; 엎드림/조준/ADS/공격/장전/진행 중 상체 동작 → Ready; 지상 이동 Speed>1.1 → Sprint; 나머지 → Carry. 엎드림에서는 총이 지면을 향하지 않도록 기존 전방 준비 자세를 보존한다.

기존 CombatPitch의 시점 기반 상하 값은 유지한다. 휴대 방향은 손 IK에 적용하고, 고개/몸통의 상하 시점 반응은 기존대로 유지했다. `OnFired()`는 기존 반동·PistolShoot 재생에 RequireReady를 추가했다. ResetPose()/OnDisable()에서 새 상태도 초기화한다. 이벤트 구독/해제·공격 시간·탄약·탄환·피격 코드는 그대로다.

### Assets/Features/PlayerCharacter/Scripts/PlayerCharacterWeaponAimView.cs

- 손 목표에 기존 `_aimRotation` 대신 `WeaponPoseRotation(_aimRotation)`을 적용. 기존 `_rightHandOffset`, 그립과 보정값은 준비/조준 상태의 기준으로 보존한다.
- 손의 전방 거리에는 WeaponForwardScale, 높이에는 WeaponVerticalOffset을 추가 적용한다.
- 기존 양손 동일 IK weight=1 대신 오른손은1을 유지하고 왼손에는 SupportHandWeight를 적용한다. 보조 손 weight가0.5 이하이면 왼팔 reach로 오른손 목표를 강제로 끌어당기지 않는다.
- `CanTrackBody`: 무장 외에 비무장 기본 이동도 머리/몸통 시점을 추적한다. 가드/공격 레이어가 소유하는 비무장 포즈는 기존 별도 pitch 처리를 유지한다.
- `CanAim`은 무장 조건을 명시해 비무장에 권총 손 목표가 적용되지 않게 한다.
- `OnAnimatorIK`: 비무장 가드가 활성일 때는 최종 MeleeCombat에서 보정하고, 일반 비무장은 LookAt 평가 후 UpperCombat IK pass에서 안정화한다. 기본 pass에서 먼저 보정하던 실험은 이후 LookAt에 의해 덮이는 문제가 있어 최종 구현에서 교체했다.
- 이전 yaw/roll3도 안정화는 유지한다. 기존 private 메서드/필드/클립을 삭제하지 않고 호출 조건·손 목표·가중치를 변경했다.

### Assets/Tests/PlayMode/PlayerCharacterCombatTests.cs

Enable(context, bool aiming=false) 테스트 준비 함수에 실제 AimHeld 입력을 지정할 수 있게 했다. 다음 여섯 기존 조준 검사는 Enable(context,true)를 사용한다:

- ThirdPersonPistolMuzzleFollowsAimWhileTurningLookingAndCrouching
- UpperBodyAndHeadParticipateInVerticalAimingWithoutRotatingTheCharacterRoot
- WorldBodySharesThirdPersonAimAcrossPerspectiveChangesWithoutTiltingTheLegs
- TorsoAndWeaponReachTheNewAimInTheSameAnimationEvaluation
- BothHandsKeepTheirGripAtMaximumVerticalAim
- ThirdPersonAimReleasesHandsForReloadAndMelee

기존의2도 정렬/그립/모델 보존/해제 검사를 완화하지 않았다. 일반 휴대와 조준이 분리됐으므로 기존의 항상 조준 상태 가정을 명시적 조준 입력으로 교체했다. 자유 시점·1인칭·공격 등의 다른 준비 상태는 기본false를 유지한다.

### Assets/Tests/PlayMode/PlayerCharacterInfantryTests.cs

LeanMovesTheTorsoWithoutAccumulatingRotationOrLosingGunAim에서 AimHeld=true 입력을 명시했다. 기울이면서 조준을 유지하는 기존2도 검사와 누적 회전 검사는 그대로다. 포복 테스트는 수정하지 않았고 실제 코드에서 포복의 전방 준비 자세를 보존해 통과시켰다.

### Assets/Tests/PlayMode/PlayerCharacterWeaponPoseTests.cs 및 .meta (신규)

순수 상태 검사3개: Carry/Sprint/Ready/FirstPerson의 서로 다른 포즈, 발사 후 준비 유지·해제와 Hidden 초기화,30/60/144fps 전환 결과 일치.

실제 PlayMode 검사3개: 휴대 시 총 낮추기→조준 시 기존 정렬/높이 복귀→휴대 복귀→1인칭 기준 보존; 질주 보조 팔 해제와 비무장 기본 팔 스윙/가드 전환; disable/re-enable 초기화. 비무장 팔은 레이어 값만 보지 않고 실제 손의 전후 운동 범위0.3m 초과를 검사한다.

### Docs/AI/PubgThirdPersonPoseIntegration.md (신규) / Docs/AI/UnityProjectContext.md (추가 기록)

이 문서와 프로젝트 최신 문맥 기록이다. 정확히 바뀐 코드/값/호출·이전 가정, 검증과 완전히 같지 않은 범위를 기록한다.

## 보행 측정과 적용하지 않은 실험

Library/CodexPubgLocomotion/stride-active.json은 실제 사용 중인 Quaternius 클립과 현재 Avatar/모델 배율에서 발 접지 구간 속도를 비교한 자료다. Walk 약2.05–2.10m/s, Jog 약8.06–9.98, Sprint 약6.72–8.93, CrouchWalk 약1.36–1.72였다. 발 접지 판정은 발 높이/후방 속도 기반의 분석용 근사이며 정확한 물리 접촉 데이터가 아니다. RootMotion 변형의 평균 속도와도 차이가 있어 이 값 하나로 모든 재생률을 고정하지 않았다. 미사용 컬렉션을 측정했던 초기 stride.json 대신 실제 연결 클립을 측정한 stride-active.json을 판단 자료로 사용했다.

position-bake.json은 클립 복사본의 Y/XZ Bake만 켜 본 실험이다. 원본에 저장하지 않았다. 조깅의 body Y가 고정된 상태에서 약0.56m 진폭으로 변하는 등 상하 움직임이 크게 증가해 단독 적용하지 않았다. 이 설정이 모든 접지 문제의 해결책이라고 주장하지 않는다. 기존 클립/.meta/Controller는 바이트 그대로 보존한다.

## 검증

- 신규 기능 검사6/6 선행 통과: pose-tests.xml/log.
- 첫 전체 검사136개 중133통과/3실패: 조준 입력 없는 lean, 포복 휴대 방향, 비무장 보정 평가 순서. lean은 명시적 조준으로 검사했고, 포복·비무장은 런타임 처리로 수정했다. 실패 기록 full.xml/log도 보관한다.
- 최종 Unity6000.4.8f1 전체 **136/136 통과**: 제품133개 + 임시 화면 캡처3개. final.xml/log. 기존 dummy 발사·충돌·팀/피해 검사, 근접·장전·시점·탑승·이동 회귀 포함. 새로운 피해/탄환 규칙은 추가하지 않았다.
-6개 모드별48프레임/24fps/2초 프리뷰 생성. 임시 캡처에서 Animator의 자동 갱신을 끄고 명시적으로1/24초 평가해 중복 시간 진행을 피했다. 실제 발 위치가 변하는지도 확인했다. mode-comparison.jpg와 idle-carry/carry-jog/sprint/aim-jog/unarmed-jog/unarmed-guard.mp4를 확인했다. 미디어는 Library에만 있으며 사용자 카메라 설정은 변경하지 않았다.
- 실제 총구 pitch는 휴대35도, 질주55도, 조준약0도(수평 조준)로 구분됐다. 기존 목표 조준 정확도·상하 시선·손 그립·disable/re-enable·전체 회귀도 확인했다.
- 최종 코드/테스트와 원본 반영 파일 동일성, 새 GUID 고유성 및 baseline 보존 확인. 검증은 격리 프로젝트에서 했으며 사용자 Editor UI를 조작하지 않았다.

## 설계와 남은 범위

새 상태 객체는 포즈 혼합/짧은 준비 유지 시간만 소유하고 CombatView는 기존 이벤트와 표현 연결, AimView는 뼈/IK 적용을 담당한다. 읽기 전용 결과로 조합하고 새 상속·인터페이스·전역 관리자는 추가하지 않았다. OOP/SOLID에 대한 의도적 예외 없음. 원본 메서드/필드/컴포넌트/참조/에셋 삭제 없음. 교체한 기존 처리는 항상100%였던 레이어 목표와 항상 조준선이던 손 목표 및 기존 테스트의 암묵적 조준 준비다.

**PUBG와 완전히 동일하지 않은 부분:** 보유 Quaternius 기본 이동의 다리 궤적/보폭/높이, 무장 출발·정지 전용 동작, 완성된 방향별 전술 이동, 포복 전용 클립과 장총 그립. 현재 팩의 가드/권총/일반 이동을 조합한 결과이며 PUBG 원본 모션을 확보한 것이 아니다. 동일한 발 접지와 체중 이동까지 맞추려면 적합한 무장 locomotion/start/stop 원본과 발 접촉 기준을 확보한 뒤 다시 리타깃/전환을 조정해야 한다. 이번 결과를 모션 전체 동일 구현 완료로 취급하지 않는다.

기존 이동8/질주12, 가속·감속, 모델/캡슐/좌석·카메라·씬·클립·Controller·첫 시점 무기 보정값은 유지했다. SampleScene에서 낮춘 무기 높이, 행동 전환 느낌과 사용자 재현 상태는 추가 확인이 필요하다. 새 패키지 구매·모션 교체·커밋·푸시 없음.
