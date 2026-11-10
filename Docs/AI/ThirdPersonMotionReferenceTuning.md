# 3인칭 슈팅 영상 비교에 따른 이동 상체 재조정

2026-10-29. 사용자가 이전 상체 보정도 과하다고 보고하고 다른 3인칭 슈팅 게임의 영상을 비교해 다시 조정하도록 요청했다. 비교 범위는 이동 중 상체 안정성이다. 다른 게임의 내부 애니메이션 데이터/관절 각도를 입수하거나 그대로 복제한 작업이 아니다.

## 실제 확인한 영상

| 자료 | 확인 구간 | 관찰 및 용도 |
|---|---|---|
| [dweebh — PUBG Movement Tips](https://www.youtube.com/watch?v=xYHjWoxeCUM&t=399s), 2025-10-20 | 06:39–06:41 앉은 이동, 06:45–06:47 서서 저속 이동 | 어깨가 매 걸음마다 크게 꺾이지 않고 상체 방향을 유지하는 표현. 편집된 비교 화면이므로 연속 속도/보행 주기 측정에는 사용하지 않음 |
| [Heisenberg Blue — Breakpoint Pistol](https://www.youtube.com/watch?v=JudLFGc6fkk&t=9s), 2020-05-08 | 00:09–00:11 권총 앉은 이동 | 총을 든 상체와 어깨가 비교적 안정적인 상태로 이동. 00:01–00:03 장비 전환 구간은 문맥으로만 확인 |

각 선택 구간을 12fps의 연속24프레임으로 확인하고 앞뒤 장면도 확인했다. PUBG와 Breakpoint는 무기·시점·장비·속도가 현재 모델과 다르며, 2D 영상에서 3D 관절 각도를 정확하게 역산했다고 주장하지 않는다. 다음의3도 제한은 영상의 실제 각도 수치가 아니라 안정적인 상체 표현을 목표로 정한 프로젝트 튜닝값이다.

처음 선택한 Ubisoft 공식 Division2/Breakpoint 영상은 연령 확인으로 프레임을 읽지 못해 비교 근거로 쓰지 않았다. 다른 공개 Breakpoint 달리기 영상(zedanave, 2019-09-05)은 초기 베타/헬기 추락 시퀀스라 일반 달리기 기준에서 제외했다. 실제 사용한 자료는 위 두 개다. 포트나이트는 탐색했지만 분석 자료로 사용하지 않았다.

메타데이터/실제 확인 구간은 Library/CodexThirdPersonMotionReference/reviewed-references.json에 기록했다. 외부 영상과 프레임 시트는 Library에만 보관하며 Assets나 배포용 문서에 복제하지 않는다.

## 변경 전 비교와 남아 있던 원인

직전 검증본과 WeaponAimView/상체 테스트/.meta가 동일함을 확인했고 Library/CodexThirdPersonMotionReference/baseline.json에 현재 캐릭터/테스트/씬의 해시를 기록했다. 원본 사용자 변경은 발견되지 않았다. 기존 Quaternius 클립·씬·사용자 수치는 보존한다.

이전 보정은 yaw(좌우 비틀림)만 제한했다. 어깨 사이 벡터의 roll(좌우 기울기)은 검사하지 않아 큰 흔들림이 남았다. Unity에서 실제 측정한 이전 조깅 어깨 yaw는 -10.7°~+8.2°였지만 roll은 -25.8°~-5.5°였고, 질주 roll은 -33.8°~-1.8°였다. 골반 방향은 이전 수정대로 안정돼 있었다.

## 모든 변경 파일과 교체 내용

### Assets/Features/PlayerCharacter/Scripts/PlayerCharacterWeaponAimView.cs

- `LocomotionTorsoYawLimit`: 12f → 3f.
- `LocomotionTorsoRollLimit = 3f` 추가. 두 값은 새 이동 상체 보정 내부 상수이며 기존 사용자 직렬화 값을 덮지 않는다.
- `LocomotionTorsoYawCorrection()`의 float 반환 구현을 `LocomotionTorsoCorrection(float intendedPitch = 0f)`의 Vector2(yaw, roll) 반환 구현으로 교체. 평면 어깨 벡터로 yaw, 높이 성분과 평면 길이의 atan2로 roll을 측정한다. 비무장 상하 조준은 실제 적용할 pitch를 어깨 벡터에 먼저 더한 뒤 보정량을 구한다.
- `CompressGaitAngle(angle, limit)` 추가. 기존 atan 압축 식을 두 축에 재사용하며 작은 움직임의 타이밍은 보존한다. 기존 Speed damping/0.05–0.3 보정 가중치는 유지한다.
- `StabilizeLocomotionTorso()`가 두 축의 보정량을 전달하도록 수정.
- `ApplyTorsoRotation(float pitch, float yaw)`를 `ApplyTorsoRotation(float pitch, Vector2 correction)`으로 교체. yaw 정렬 → roll 보정 순으로 조합한 회전과 기존 pitch를 한 Quaternion으로 합성해 척추와 가슴에 동일한 절반 회전을 적용한다.
- 기존 `PitchBone()`과 `YawBone()`의 개별 축 적용을 제거하고 `RotateBoneWorld()`로 교체. 부모 좌표계로 변환한 동일한 world-space 절반 회전을 두 뼈에 적용해 회전축끼리 덮어쓰거나 적용 순서 때문에 roll이 남지 않도록 했다. 머리에서는 pitch를 유지하며 안정화 회전만 상쇄한다.
- MeleeCombat 분기를 기존 상하 조준 여부/pitch 산출 → 그 pitch를 고려한 이동 보정 → 한 번의 회전 합성 순서로 변경. 기존 공격의 pitch 동작은 유지한다.

```text
이전: yaw만 약±12°로 압축. roll은 원본 그대로.
이후: yaw/roll 각각 약±3°로 압축.
      상하 조준을 먼저 고려하고, 머리 시선은 유지.
```

기존 적용 조건(지상 Locomotion/CrouchLocomotion, 이동 중, 포복 아님)과 공격/장전/사망/소생/점프/슬라이드/Vault 제외 조건은 유지했다. 손 목표·반동·그립·LookAt 설정도 유지한다. 새 컴포넌트/직렬화 필드/Animator 파라미터/외부 의존성은 추가하지 않았다.

### Assets/Tests/PlayMode/PlayerCharacterTorsoGaitTests.cs

기존 두 테스트의 yaw 제한을14도 → 5도로 강화하고 roll4도 미만 검사/최대 roll 로그를 추가했다. 최종 포즈는 목표3도보다 약간 넓은 허용 범위로 검사한다. 기존1·3인칭, 무장/비무장, 여섯 이동 방향/자세, 상하±30도 조준, 머리 방향과 루트/모델 보존 검사는 유지한다. 테스트/.meta GUID 변경 없음.

### Docs/AI/ThirdPersonMotionReferenceTuning.md

이 문서 신규. 실제 영상 구간, 비교 한계, 놓친 roll, 교체 함수/값과 검증을 기록한다.

### Docs/AI/UnityProjectContext.md

영상 비교 후 두 축 안정화와 강화한 검증 내용을 추가한다. 이전 작업 기록은 당시 결과로 보존한다.

## 구조·보존 검토

기존 조준/포즈 조정기 내부에서 동일한 책임의 두 축을 합성한다. 별도 클래스로 분리하면 같은 IK 평가와 뼈 참조를 왕복 전달해야 하므로 기존 컴포넌트를 확장했다. 이동/충돌/입력 권한을 침범하지 않고 상태를 복제하지 않는다. OOP/SOLID에 대한 의도적 예외 없음. Quaternion/Vector2는 값 형식이고 새 프레임별 컬렉션·검색·객체 생성은 없다. 성능 개선을 측정했다고 주장하지 않는다.

제거/교체한 것은 위에 명시한 private 메서드 세 개의 기존 구현/시그니처와 두 축 합성 호출, yaw 내부 상수다. 클래스/컴포넌트/에셋/참조를 삭제하지 않았다. 씬·Animator Controller·애니메이션 클립·카메라·이동 속도·사용자 설정은 변경하지 않는다.

## 검증

- 중간 full.xml 실행에서 신규 비무장 상하 조준 검사가 yaw5.076°로 실패했고, 원인을 intendedPitch 적용 전 보정량 산출로 좁혔다. 실패를 숨기거나 허용 범위를 늘리지 않고 계산 순서를 수정했다.
- 최종 격리 Unity6000.4.8f1 전체 PlayMode **130/130 통과**. 제품127개 + 기존 임시 캡처2개 + 신규 임시 연속 화면 캡처1개. 증거: Library/CodexThirdPersonMotionReference/final.xml 및 final.log. 컴파일 오류와 실패0.
- 상하±30도 조준,1·3인칭, 무장/비무장, walk/jog/sprint/left/diagonal/crouch의24조건에서 yaw5도/roll4도보다 작음을 검사했다. 실제 최대값은 약3도다. 이전의 머리 yaw/상하 방향, 루트/모델 회전 보존도 검사한다. 기존 공격·장전·슬라이드·점프·탑승 회귀 검사는 유지했다.
- 수평 조깅 별도 측정에서 최종 어깨 yaw 약-2.9°~+2.8°, roll 약-3.0°~-2.0°. 질주 yaw 약-2.8°~+2.6°, roll 약-3.0°~-1.0°. 골반 방향과 머리 시선 보존. 비교 데이터는 before-pose.json / after-composed-pose.json. after-pose.json은 회전 합성 순서 조정 전 실험이며 최종값으로 사용하지 않는다.
- jog/sprint 각각72프레임,24fps의 연속3초 프리뷰를 생성하고 프레임 시트로 전체 주기를 확인했다. 해당 프리뷰의 머리 roll은 조깅 최대약2.10°, 질주약1.49°. Preview/*.png와 jog-after.mp4 / sprint-after.mp4는 Library에만 보관한다. 실제 사용자의 Game View 카메라 설정은 수정하지 않았다.
- 최종 테스트한 코드/테스트를 바이트 그대로 원본에 반영하고 baseline 대비 기존 제품 파일의 변경이 두 C# 파일뿐인지 확인했다. 메타/씬/Controller/클립·사용자 설정은 보존했다. 문서 변경 두 개는 위 목록에 별도로 명시했다.

사용자 Editor UI를 조작하지 않았다. SampleScene에서 실제 체감과 출발·정지·행동 전환의 자연스러움은 확인이 필요하다. 이 보정은 지상 이동 상체용이며 원본 모션 전체를 다른 게임과 같게 만들었다고 주장하지 않는다. 커밋/푸시 없음.
