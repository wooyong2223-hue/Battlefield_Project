# 총기 상하 추적 복원과 BF6 점프 후보 비교

2026-10-29. 요청: 총·상체가 이전처럼 마우스 상하를 따라가도록 복원하고, BF6 점프를 확인해 비슷한 무료 외부 클립을 찾는다. 이번에는 점프 클립/Controller/물리 설정을 교체하지 않는다.

## 작업 전 사용자 변경 보존

이전 검증본 대비 SampleScene에서 PlayerCharacter가 부모 그룹에서 루트로 이동했고 로컬 위치 `(0,2,10)`이 `(79.5,-97,-544.1)`로 바뀌었다. 부모가 바뀌었으므로 단순 로컬 Z 차이만으로 같은 좌표계 이동이라고 해석하지 않는다. Canvas 추가 셰이더 채널0→25, TMP TextStyleHashCode0→-1183493901 및 일부 필드 저장 순서 변화도 있었다. 캐릭터 스크립트/애니메이션/비교 씬은 이전과 같았다.

현재 원본 상태를 baseline으로 저장했고 최신 씬을 격리 프로젝트에 복사했다. 사용자 위치·계층·UI 설정은 수정하지 않았다. 원본 반영 직전에도 source baseline이 그대로인지 확인했다.

## 변경 파일과 정확한 변경

### PlayerCharacterWeaponPoseState.cs

휴대/질주가 무기 방향을 덮던 Rotation 구현만 변경했다.

```csharp
// 이전: CarryBlend=1이면 마우스 pitch 대신35/55도를 사용
float pitch = Mathf.DeltaAngle(0f, aim.eulerAngles.x);
return Quaternion.Euler(
    Mathf.Lerp(pitch, Mathf.Lerp(35f, 55f, SprintBlend), CarryBlend),
    aim.eulerAngles.y, 0f);

// 이후: 휴대 자세는 위치만 바꾸고 방향은 해결된 조준을 유지
public Quaternion Rotation(Quaternion aim) => aim;
```

이전3줄의 pitch 추출/고정각 보간/Quaternion 재조립을 제거했다. Rotation 메서드와 호출 계약은 유지하며 필드/컴포넌트/참조/에셋을 제거하지 않았다. 휴대 높이/거리, 질주 지원 손 IK, 레이어 가중치, 발사 후 준비 유지시간은 모두 기존대로다. 새 클래스/인터페이스/관절 보정은 추가하지 않았다. 자세 상태 객체가 조준 표시 정책을 계속 소유하므로 OOP/SOLID 책임 경계 변경이나 예외는 없다.

### PlayerCharacterWeaponPoseTests.cs

- 상태 테스트의 휴대35도/질주55도 고정 기대를 입력 방향 유지 기대0도로 교체했다. 기존 거리/보조 손/초기화/프레임 독립 검사는 유지했다.
- `ThirdPersonCarryLowersTheGunAndAimingRestoresTheOriginalAlignment`를 `ThirdPersonCarryLowersTheGunWithoutReplacingTheAimDirection`으로 변경. 휴대/조준 해제 후에도 총구 정렬2도 미만을 검사한다.
- 기존 휴대→조준 높이 차이 `>.15m`는35도에서0도로 회전하는 효과까지 포함하던 기대다. 각도 고정 제거 후 남는 기존 위치 오프셋.1m에 맞춰 `carriedHeight+.1m ±.04m`로 변경했다. 임의로 실패를 무시한 것이 아니라 새 동작의 위치 계약을 검사한다.
- `MousePitchTracksTheWeaponAndTorsoWithoutHoldingAim` 신규. RMB 없이1·3인칭/일반 이동·질주/pitch±60도, 총구 방향2도 미만, 몸통 변화8도 초과를 검사한다.
- 1인칭의 기대 방향은 기존 `AimRotation * Quaternion.Euler(FirstPersonWeaponEuler)`다. 질주 연출 `(8,-5,-8)`과 sway를 제거하거나 잘못 오류로 판정하지 않는다. 3인칭은 AimRotation을 그대로 기준으로 한다.

### PlayerCharacterCameraTests.cs

`AuthoredSpawnStartsOutsideMetalAndShowsTheThirdPersonModel`의 고정 `localPosition.z == 10` 검사를 실제 CharacterController 캡슐과 Dummy_Metal의 비겹침 검사로 교체했다. 현재 위치를 옛 값으로 되돌리지 않으면서 기존 검사의 목적을 유지하기 위해서다. 현재 캡슐 center/height/radius/scale로 Physics.OverlapCapsule을 계산하고 Dummy_Metal 하위 Collider와 겹치지 않는지 검사한다. 이후 카메라 거리4.8m 초과와3인칭 모델 표시 검사는 그대로다.

### 문서

이 문서 신규, `Docs/AI/UnityProjectContext.md`에 최신 복원 결과와 점프 비교 링크를 추가했다. 그 외 제품 스크립트/씬/컨트롤러/마스크/클립/설정 변경 없음.

## 재현과 검증

- `before.xml/log`: 새 무조준 회귀가 기존 코드에서 실패. 3인칭 일반 이동/pitch-60도에서 총구 오차약94.9997도.
- 첫 수정 후 1인칭 질주 약9.34도 오차가 관찰됐다. 처음에는 레이어 혼합을 의심해 임시 가중치1을 실험했지만 개선되지 않았다. 원인은 기존 SprintEuler 연출이었다. 가중치 변경은 격리에서 되돌렸으며 제품에 반영하지 않았다.
- `after.xml/log`, `pose-final.xml/log`: 중간 실험/이전 각도 기반 높이 기대의 실패를 보존한다. 질주 연출 기준과 높이 계약을 바로잡았다.
- `full.xml/log`:139개 중138통과/1실패. 남은 실패는 사용자 이동 후에도 Z10을 요구한 오래된 시작 위치 테스트였다. 조준 수정 및 새 무조준 회귀는 통과했다.
- `final.xml/log`: **139/139 Passed**, failed/skipped0. 기존 제품133개+새 무조준 회귀1개+격리 진단/캡처5개다. 기존 SampleScene 및 비교 씬 smoke, 무기/근접/이동/탑승 회귀 포함.
- 검증 위치는 `Library/CodexInfantryWork/Project`, Unity6000.4.8f1. 반영한3개 코드/테스트 파일은 테스트한 복사본과 바이트 동일하다.
- 타격/탄환/피해 권한을 바꾸지 않았다. 기존 더미 검사를 포함한 회귀만 실행했으며 신규 전투 판정 기능을 추가했다고 표시하지 않는다.
- 원본 Editor에서 직접 입력한 수동 테스트나 플레이어 빌드는 하지 않았다. SampleScene에서 권총을 들고 RMB를 누르지 않은 일반 이동/질주 중 상하 조작, V 전환, Alt 자유 시점을 사용자 확인 대상으로 남긴다.
- 이전 별도 몸통 포함 비교 씬의4개 자세/시점/착석 차이는 이번의 해결 범위가 아니다. 이번 수정으로 그 문제까지 해결됐다고 표시하지 않는다.

## BF6 점프 영상 확인

직접 프레임을 확인한 영상은 [BF6 Advanced Movement Guide](https://www.youtube.com/watch?v=BWMlR_ViPgA),2025-10-26 업로드다. 로컬의 기존 확보 영상을 다시 샘플링했다.5:53.4~5:55.8 구간에 다른 병사의 외형 점프가 보인다. 무릎을 접으며 이동하고 총을 앞으로 유지한다. 화면은 플레이어1인칭에서 다른 병사를 관찰한 것이며, 영상 자체가 공중 crouch를 활용한 steady-aim jump 설명이어서 일반 점프의 모든 단계/정확한 속도를 대표하지 않는다.2026 최신 패치와의 일치도 주장하지 않는다.

`bf6-jump-detailed.jpg`는 위 구간20프레임, `bf6-thirdperson-jumps.jpg`와 `bf6-jump-contact.jpg`는 주변 문맥 확인용이다.2D 영상으로3D 관절 각도나 점프 높이를 추정해 값으로 사용하지 않았다.

## 무료 후보와 실제 포즈 비교

[KayKit Character Animations](https://kaylousberg.itch.io/kaykit-character-animations)는 CC0 무료 FBX/glTF를 제공한다. 보유 원본의 실제 이름을 확인하고 현재 캐릭터에 리타깃했다. [Quaternius UAL1](https://quaternius.itch.io/universal-animation-library)·UAL2의 보유 Standard 클립도 비교했다. 무료판에 없는 유료 모션을 포함했다고 계산하지 않는다.

| 후보 | 실제 비교 결과 | 판정 |
|---|---|---|
| KayKit `Jump_Full_Short`1.1667초 | 두 무릎을 접는 작은 점프 형태. 현재 Q의 한 다리 과장 자세보다 단순하고 작은 동작 | **다음 일반 점프 교체 시험 우선 후보**. 실제 비행/착지 동기화와 보행 연결은 미검증 |
| KayKit `Jump_Start`/`Jump_Idle`/`Jump_Land` | 준비/공중/착지를 별도 상태에 연결할 수 있음. 권총 팔 레이어와 혼합해 외형 확인 | 분리 상태 구성 후보. 실제 착지 타이밍에 맞추는 후속 시험 필요 |
| KayKit `Jump_Full_Long`2.3333초 | 더 긴 전체 점프 클립 | 달리며 긴 점프 후보. 현재 일반 점프에 그대로 넣지 않음 |
| 현재 Quaternius `JumpStart`/`JumpAir`/`JumpLand` | 한쪽 무릎을 크게 들고 반대 다리를 펴는 자세가 공중에 유지됨 | 현재 부자연스러움 비교 기준. 자동 제거하지 않음 |
| Quaternius UAL2 `NinjaJump_Start/Idle_Loop/Land` | 무릎을 크게 올리고 몸을 웅크리는 동작, 일반 점프보다 과장됨 | 이번 일반 보병 점프 우선 후보에서 제외 |
| [MoCap Online Free Pistol](https://mocaponline.itch.io/free-pistol-animation-starter-pack) | 제작자 무료 목록에 군사 권총 Jump Start/Air/End 및 걷기/조깅 착지 명시 | 무기 파지 일관성 면에서 조사할 후보. 이번에 파일 확보/리타깃하지 않았고 공개 Git 원본 재배포 조건 미확인으로 반입 보류 |

11클립을 원본 전신과 기존 권총 팔 레이어 혼합2방식으로 평가했다.22조합/528프레임, humanMotion/유한 hips 좌표 검사 통과. 비교 대상은 현재 Armature/배율2.270328, root motion off다. 시험용 메모리 복사본의 Rotation Bake Into Pose만 켜고 원본 키프레임/클립 파일은 수정하지 않았다.

이는 Playables의 제자리 포즈 재생이다. 상체는 기존 PistolHoldNeutral/팔 마스크를 사용하며 실제 총 메시/손 IK/물리 점프의 접점이나 착지 연결을 검증한 것은 아니다. 눈에 보이는 하체 포즈 비교와 최종 채택을 구분한다. BF6와 동일한 동작이라고 판정하지 않는다.

현재 점프 높이1.2m/중력-9.81/캐릭터 이동/Controller는 보존했다. 현재 Jump_Start1.3333초, Land 원본1.2667초와 상태 재생속도/접지 전환의 조합도 체감에 영향을 주므로 새 클립 선정 후에 실제 이륙·정점·접지 단계가 맞는지 확인해야 한다. 클립만 교체하면 해결된다고 단정하지 않는다.

## 자료 위치

`Library/CodexAimRestoreJumpSearch`:

- `baseline.json`, `promoted-files.json`, `final-audit.json`: 원본 보존/코드 반영 증거.
- `before/after/pose-final/full/final.xml` 및 대응 `.log`: 실패 재현·실험·최종139개 회귀.
- `jump-comparison.jpg`, `jump-candidates-a.jpg`, `jump-candidates-b.jpg`: 포즈 비교, 직접 확인.
- `K_Jump_Full_Short.mp4`, `K_Jump_Full_Long.mp4`, `Current_JumpStart.mp4`, `Q_NinjaJump_Start.mp4`: 원본 길이에 맞춰 샘플링 후30fps 출력. 물리 이동 없는 포즈 영상이다.
- `JumpPreview/clips.json`:11클립/22조합의 길이와 기술 검사.
- `PreviewJumpCandidates.cs`와 격리 프로젝트의 동명 Editor 스크립트(+meta): 임시 촬영 도구. 제품에 포함하지 않는다.

점프 에셋은 새로 제작하거나 제품에 교체하지 않았다. 커밋·푸시 없음.
