# 점프 카메라 관성·착지 반응 적용

2026-10-29. 사용자 요청: BF6를 참고해 부족한 점프 카메라 반응 개선.

## 확인과 참고 범위

직전 작업 이후 캐릭터 코드/씬/설정/PlayMode 파일의 사용자 추가 변경은 없었다. 현재 캐릭터 위치·계층·카메라 구도·조작 설정을 baseline으로 기록하고 보존했다.

이전 MotionState는 지상 보행 흔들림과 착지 spring만 계산했다. 이륙/공중의 별도 관성은 없었다. 카메라는 1인칭에서만 CameraOffset/CameraRoll을 적용하고, 3인칭에서는 둘 다 적용하지 않았다. 따라서 카메라 반응 부족이 체감의 한 원인일 수 있다. 캐릭터의 한 다리 공중 포즈나 전환 타이밍까지 카메라로 해결됐다고 판단하지 않는다.

참고 영상은 기존 확보한 [BF6 Advanced Movement Guide](https://www.youtube.com/watch?v=BWMlR_ViPgA)의 점프/착지 구간이다. 이전 프레임 검토에서 배경 높이와 무기 자세가 변하는 반응을 확인했다. 해당 영상은2025-10-26 업로드이며 공중 crouch/steady-aim jump 등 특수 조작도 포함하므로 일반 점프의 정량 기준이 아니다.2D 영상에서 실제 카메라 이동량/각도를 추정해 복사하지 않았다. 이번 수치는 프로젝트 조정값이다. BF6와 정확히 동일하거나2026 최신 버전과 일치한다고 주장하지 않는다.

## 구조 선택

기존 PlayerCharacterMotionState는 보행/질주/sway/착지 표시 상태의 소유자다. 점프의 수직 관성은 같은 표시 책임과 활성/초기화 수명에 속하므로 기존 상태에 작은 spring을 추가했다. 새 MonoBehaviour/매니저/상속/인터페이스는 필요하지 않았다. Profile은 private serialized tuning/read-only properties, State는 private spring state/read-only outputs, Camera는 결과 소비만 담당한다. OOP/SOLID 책임 변경이나 의도적 예외 없음.

## 변경한 모든 제품 파일

### PlayerCharacterMotionState.cs

신규 private `_jumpLag`, `_jumpVelocity`; public read-only `JumpCameraOffset`, `CameraPitch` 추가. Reset에서 모두0으로 초기화한다.

Tick은 수직 속도에 따라 목표 관성을 계산한다.

```csharp
float targetLag = grounded ? 0f : Mathf.Clamp(-localVelocity.y * _profile.JumpVerticalLag,
    -_profile.MaximumJumpLag, _profile.MaximumJumpLag);
```

오차/속도에 기존 착지와 같은 형식의 정확한 임계 감쇠 spring step을 적용한다. 올라갈 때 카메라가 몸보다 조금 늦고, 내려올 때 반대 관성이 생기며 접지 후0으로 복귀한다. 힘이나 캐릭터 속도를 변경하지 않는다.

기존 CameraOffset의 보행/착지 계산은 유지하고 새 출력만 추가했다.

```csharp
JumpCameraOffset = Vector3.up * (_jumpLag + _landing * (_profile.LandingCameraMultiplier - 1f)) * stabilization;
CameraPitch = (_jumpLag * _profile.JumpPitchPerMeter - _landing * _profile.LandingPitchPerMeter) * stabilization;
```

1인칭 CameraOffset에는 원래 착지 변위1회가 있으므로 새 출력은 multiplier-1을 더한다. 기존 ADS 감쇠를 함께 사용한다. 랜덤 흔들림/누적 Euler 회전은 추가하지 않았다.

### PlayerCharacterMotionProfile.cs

`Jump camera` Header 아래 신규 필드7개와 read-only clamp properties 추가. 기존 필드/기본값 변경/삭제 없음. SerializeField+Min을 사용하며 Range 없음.

### Settings/PlayerCharacterMotion.asset

기존 전체 serialized 내용을 유지하고 아래 신규 값만 추가했다.

| 새 값 | 기본값 | 의미 |
|---|---:|---|
| `_jumpVerticalLag` |0.025|수직 속도에 대한 목표 관성 비율|
| `_maximumJumpLag` |0.12|수직 관성의 목표 최대 이동량(m), 착지 추가분과 별도|
| `_jumpRecovery` |10|관성 spring 복귀 계수|
| `_landingCameraMultiplier` |3|기존1인칭 착지 위치 반응의 전체 배율|
| `_jumpPitchPerMeter` |8|관성 변위에 따른 시야 pitch(deg/m)|
| `_landingPitchPerMeter` |40|기존 착지 spring 변위에 따른 시야 pitch(deg/m)|
| `_thirdPersonJumpMultiplier` |0.6|3인칭에 적용하는 새 변위/회전 배율|

기존 LandingImpulse0.12/MaximumLandingImpulse2/LandingRecovery14, 보행·질주·ADS·sway·무기 offset/Euler 값은 모두 보존했다. 이7개는 BF6 측정값이 아니다.

### PlayerCharacterCamera.cs

private JumpOffset property 추가. 타깃 회전으로 새 local jump displacement를 world space로 변환한다.

```text
이전 FP: EyePosition + 기존 CameraOffset
이후 FP: EyePosition + 기존 CameraOffset + JumpOffset

이전 TP pivot: authored eye pivot + LeanOffset
이후 TP pivot: authored eye pivot + LeanOffset + JumpOffset * ThirdPersonJumpMultiplier
```

기존 VisualEyePosition expression과3인칭 pivot expression을 위 식으로 확장했다. LateUpdate는 LookRotation에 CameraPitch를 곱하고, 3인칭은60%만 적용한다. 구체적 변경 위치는 VisualEyePosition/LateUpdate/GetAimRay다. GetAimRay의3인칭 위치도 같은 추가 pivot을 사용하지만 방향은 기존 AimRotation을 유지한다. 1인칭 origin은 VisualEyePosition을 계속 사용한다.

새 pitch는 시각 표현만 담당한다. View의 입력 조준값/캐릭터 root 회전/발사 방향은 변경하지 않는다. 기존 카메라 구도/FOV/충돌 거리 제한은 유지한다. 새3인칭 pivot도 기존 SphereCast 경로로 충돌 거리를 계산한다. 기존 카메라/메서드/필드/참조/컴포넌트를 제거하지 않았다.

### PlayerCharacterMotionTests.cs

기존 테스트/허용 범위를 유지하고4개 추가했다.

- JumpInertiaFollowsVerticalVelocityAndRecoversWithoutDrift: 상승/하강 부호, 복귀, 비활성화 초기화.
- JumpCameraLagIsBoundedAndAdsStabilizesTheReaction: 빠른 낙하의 목표 상한/ADS 위치·회전 감쇠.
- JumpCameraResponseMatchesAtThirtyAndOneHundredTwentyFps:30/120fps spring 출력 차이 검사.
- JumpCameraRespondsInBothViewsWithoutChangingAimOrMovementSettings: 실제 CharacterController 점프로 양 시점 카메라 반응, 조준값/속도/FOV 보존 검사.

### 문서

이 문서 신규. UnityProjectContext.md에 최신 동작과 검증 요약 추가. 그 외 제품 스크립트/씬/Animator/마스크/애니메이션/캐릭터 점프 설정/메타 변경 없음.

## 실행 검증과 결과

격리 Unity6000.4.8f1 / DX11, `Library/CodexInfantryWork/Project`에서 수행했다.

- 관련 MotionTests **14/14 Passed** (`Library/CodexJumpCamera/motion.xml/log`).
- 전체 **144/144 Passed**, failed/skipped0 (`full.xml/log`). 기존 제품134+새 제품4=138, 격리 임시 진단/캡처6개 포함.
- 동일 입력으로 Before/After × FP/TP, 각120프레임30fps 촬영. 프레임20에서 같은 점프를 시작했다. 원래 동작 비교는 새 설정만 메모리 Profile 복사본에서 꺼 재현했다. 기존 원본 Profile이나 키프레임을 바꿔 촬영하지 않았다.
- 양쪽 모두 공중28프레임, 입력 Aim pitch 변화0. 기본 점프에서 state JumpCameraOffset 약-0.0632~+0.0628m, CameraPitch 최대약1.038도.3인칭 실제 적용은 state 출력의60%이므로 회전약0.623도이며 boom 회전에 따른 추가 시점 위치 변화도 있다. 이 값은 모든 낙하 높이의 최대치가 아니다.
- 실제 Before/After FPS/TP 프레임을 비교했다. 무기/모델 애니메이션은 같고 카메라의 이륙·착지 반응이 달라진다. 사용자의 실제 입력 체감이나 BF6와 동일함을 자동 테스트만으로 보증하지 않는다.
- 현재 원본 baseline 확인 후 검증한5개 코드/테스트/설정 파일을 바이트 동일하게 반영했다. 기존 사용자 위치/계층/설정과 나머지 파일은 보존한다.

## 자료와 사용자 확인

`Library/CodexJumpCamera/FP-comparison.mp4`, `TP-comparison.mp4`: 왼쪽Before/오른쪽After. `camera-comparison.jpg`, `Preview/trace.csv`, `trace-summary.json`에 시점/수직 관성/grounded/조준 추적이 있다.

원본 SampleScene에서 정지 점프·이동 점프·질주 점프, V 전환, ADS 중 점프와 착지 후 복귀를 확인하면 된다. 설정은 PlayerCharacterMotion.asset의 Jump camera에 있다. 3인칭 효과만 줄이려면 ThirdPersonJumpMultiplier를 조정할 수 있다. 기존 카메라 구도/FOV를 바꿀 필요는 없다.

원본 사용자 Editor 직접 조작/플레이어 빌드/다른 낙하 높이의 체감 평가까지 수행한 것은 아니다. 점프 자세 자체와 이전 실험용 몸통 비교 씬의 알려진 문제는 이번에 수정하지 않았다. 커밋·푸시 없음.

임시 `CodexJumpCameraPreview.cs`(+meta)는 격리 PlayMode 폴더에만 두고 제품으로 복사하지 않았다. 촬영 후 captureDeltaTime/렌더 타깃을 복원하고 생성 리소스를 정리한다.
