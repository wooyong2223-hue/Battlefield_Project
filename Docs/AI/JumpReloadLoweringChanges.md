# 점프 중 높은 팔 자세에서 장전 위치로 내려오기

2026-10-30. AK-47/M4A1, 1·3인칭.

## 사전 비교와 원인

이전 소총 장전/몸통/발사 작업의 최신 적용 해시를 병합해 원본과 비교했다. 후속 사용자/Editor 변경 없음. 원본은 수정하지 않고 Library 격리 프로젝트에서 재현·검증했다. 모든 Unity 실행은 `-batchmode -noaudio`이며 사용자 Editor나 영구 오디오 설정을 조작하지 않았다.

실제 CharacterController 점프와 현재 Animator로 팔이 높아지는 프레임을 찾은 다음 장전을 시작했다. 기존 코드는 그 순간의 손 위치를 `_rightTorsoOffset`에 저장하고 내내 유지했다. 높은 시작 위치가 장전 목표로 굳는 문제였다. FP에서는 기존 양수 장전 Y 오프셋과 몸통 상승까지 더해져 실제 손이 약38cm 올라가는 것도 측정했다.

수정 전 AK/M4 × TP/FP 모두 높은 팔 상태를 재현했다. 장전0.44초 뒤에도 손은 높은 몸통 로컬 위치 약Y0.30~0.31에 남았고, 지상 장전 위치는 약Y0.20~0.21이었다. 이 수치는 본 모델 뼈의 로컬 단위이며 월드 미터와 동일하게 해석하지 않는다. 증거 `Library/CodexJumpReload/before.xml/log`.

## 변경한 모든 파일 (8개)

### 1. Assets/Features/PlayerCharacter/Scripts/PlayerCharacterWeaponAimView.cs

- 기존 OnAnimatorIK 안의 손 목표 계산을 private `CalculateGrip(...)`으로 이동했다. 높이·전방 오프셋, 경사 보정, 장착 지점, 반동·도달 범위 계산식과 수치는 유지하고 기존 OnAnimatorIK가 같은 함수를 호출하게 했다. 기존 기능 삭제는 없다.
- public `GetReloadGrip(out positionInTorso, out rotationInTorso)` 추가. 기존 조준 오프셋으로 준비된 그립 목표를 계산하고, 점프 이전의 정상 몸통 기준에 대한 로컬 자세로 반환한다. 원치 않는 공중 자세를 새 장전 기준으로 저장하지 않는다.
- `_body` CharacterController 캐시와 런타임 지상 참조 데이터 `_reloadEyeToTorso`, `_reloadReferenceRotation`, `_reloadGripCorrection`, `_reloadReferenceDefinition` 추가. Awake는 기존 몸통 기준을 초기값으로 사용한다.
- LateUpdate에서 **지상·일반 이동/앉기 상태·전환 중 아님·그립 사용 중**일 때 정상 몸통 참조를 갱신한다. 공중/점프/장전 중에는 갱신하지 않는다. 준비 그립·반동 없음 조건에서 실제 해결된 손과 계산 목표 사이 차이도 기록해, 기존 지상 그립과 같은 위치에 도달하도록 한다. 다른 무기의 손 보정은 Definition이 일치할 때만 사용한다.
- 기존 serialized 값·몸통 안정화의 `_chestReference`는 변경하지 않는다. 새 참조는 현재 캐릭터의 실제 모델·카메라·그립 데이터에서 계산한다.

### 2. Assets/Features/PlayerCharacter/Scripts/PlayerCharacterRifleReloadView.cs

serialized `_aim` 참조 추가. 새 private `_entryTorsoOffset`, `_entryRotationInTorso`, `_loweringRaisedHands` 상태 추가. 기존 `_rightTorsoOffset/_rightRotationInTorso`는 이제 정상 장전 목표를 저장한다.

```csharp
// 이전: 현재 손 위치가 장전 내내 목표
_rightTorsoOffset = _torso.InverseTransformPoint(_rightHand.position);
right = _torso.TransformPoint(_rightTorsoOffset);

// 변경: 현재 손은 전환 시작점, 정상 그립은 도착점
_entryTorsoOffset = _torso.InverseTransformPoint(_rightHand.position);
_aim.GetReloadGrip(out _rightTorsoOffset, out _rightRotationInTorso);
float settle = Mathf.SmoothStep(0f, 1f, progress / .18f);
right = _torso.TransformPoint(Vector3.Lerp(_entryTorsoOffset, _rightTorsoOffset, settle));
```

회전도 진입 자세에서 목표 자세로 Slerp한다. 최초 프레임은 현재 자세를 유지해 순간이동하지 않는다. 기존 왼손이 탄창을 잡는 첫18% 구간과 맞춰, 장전1.6초의 약0.288초 동안 내려온다. 실제 탄약 장전 시간은 바꾸지 않는다.

- 현재 손이 정상 그립보다 월드 위쪽으로5cm 이상 높으면 `IsLoweringRaisedHands`가 참이다. 이 경우 ViewOffset의 양수 Y를 억제해 기존 장전 화면 오프셋이 팔을 다시 올리지 않게 한다. 기존 hip `(-.08,.32,.3)` / ADS `(-.06,.26,.55)` 값 자체는 유지한다.
- 읽기 전용 `HandSettleDelta`로 현재 손 전환 이동량을 월드 벡터로 제공한다. 기존 장전 마지막15% 페이드 구간에 맞춰0으로 돌아간다.
- 기존 탄창 이동·손 접촉·양팔 도달 범위 제한·취소/종료 복원은 유지한다. Restore 시 새 raised 상태도 초기화한다. 기존 serialized 참조/필드 삭제 없음.

### 3. Assets/Features/PlayerCharacter/Scripts/FirstPersonArmsView.cs

- FP 복제는 현재 손을 기준으로 정규화하기 때문에 월드 손이 내려와도 화면에서는 그 이동이 사라졌다. `HandSettleDelta`를 기존 복제 좌표로 변환해 양팔/총 표시에도 전달한다.
- 추가 손 전환 벡터는 원본20cm 이내로 제한하고 기존 무기 FP 배율을 적용한다(현재 소총0.7이면 최대14cm). 기존 몸통 변화량/15도 제한은 유지한다.
- raised 장전의 첫18%에만 몸통 피드백이 만드는 순수 위쪽 이동을 제거한다. 이후에는 원래 몸통 추적을 그대로 따른다. 장전 ViewOffset의 양수 Y 억제와 함께, 아래로 내려오는 전환을 다른 보정이 다시 끌어올리지 않게 한다.
- 손 추가 이동은 복제 기준 좌표에 한 번만 적용한다. 몸통 회전 뒤에 추가 회전을 중복 적용하지 않아 기존 몸통 회전 결합과 피벗을 유지한다.
- 일반 발사·ADS·근접·권총 경로, Inspector 값, 실제 카메라는 변경하지 않는다.

### 4. Assets/Scenes/SampleScene.unity

기존 RifleReloadView 컴포넌트 문서 `1601236115`에 한 줄만 추가한다.

```yaml
  _aim: {fileID: 1601236113}
```

같은 Animator 오브젝트의 기존 PlayerCharacterWeaponAimView 참조다. 오브젝트 추가/이동/삭제 없음. 기존 값·참조·부모/자식 구조 유지. 이 한 줄을 제거하면 원본 씬 바이트와 일치하는지 확인한다.

### 5–6. Assets/Tests/PlayMode/JumpReloadLoweringTests.cs 및 .meta (신규)

현재 SampleScene, 실제 CharacterController.Tick 점프, 현재 모델 Animator를 사용한다. AK/M4 × TP/FP에서 지상 장전과 공중 높은 팔 장전을 비교한다.

- 실제 점프 중 손이 몸통 기준으로10cm 이상 높아진 프레임을 반드시 찾는다.
- 첫 장전 평가의 손 순간이동이2cm 미만이어야 한다.
- 0.44초 후 지상 장전과 몸통 로컬 손 차이가0.04 미만이어야 한다.
- FP 화면 기준 손도3cm 이상 내려와야 한다. 소스 뼈만 바꾸고 화면에서 높게 고정되는 회귀를 방지한다.

테스트는20ms 프레임을 사용하고 기존 테스트/허용오차는 변경하지 않았다. meta는 신규 테스트 GUID 파일이다.

### 7–8. Docs/AI/JumpReloadLoweringChanges.md 및 Docs/AI/UnityProjectContext.md

본 변경 보고와 최신 프로젝트 상태·증거 경로를 기록한다. 기존 문서 내용은 유지한다.

## 구조·설계 검토 및 교체 목록

AimView는 기존 그립 계산과 정상 그립 참조, ReloadView는 장전 시작/목표/전환 상태, FP view는 표시 변환을 소유한다. 별도 Manager나 새 컴포넌트를 추가하지 않았다. 명시적인 serialized AimView 참조로 목표를 요청하며, 새 공개 데이터는 읽기 전용 자세/전환 정보로 제한했다. 탄약·입력·피해 책임을 표현 코드로 옮기지 않았다. 새로운 상속/인터페이스가 필요한 변형이 없어 추가하지 않았다.

교체한 기존 구현: OnAnimatorIK의 계산 블록을 CalculateGrip으로 이동(계산 유지), Begin의 현재 손 위치를 목표로 저장하던 경로를 시작점+정상 목표로 교체, LateUpdate의 고정 위치/회전을 보간으로 교체, raised 상태에서 ViewOffset의 양수 Y 적용을 조건부로 억제. 기존 메서드/에셋/컴포넌트/serialized 필드/참조 삭제 없음. 일반 상태의 오프셋 수치나 기존0.2m/15도 몸통 표시 제한은 변경하지 않는다.

## 검증과 남은 확인

수정 전 실제 점프 재현 실패. 초기 후보는 점프 내림을 해결했으나 FP 몸통 회전 결합 검사에서 실패했다. 보정을 도입 구간으로 제한하고 추가 이동의 좌표계/피벗을 수정한 최종 집중 검사 `focused3.xml` **9/9 통과**. AK/M4의 FP 손은 약7.3~7.5cm 하강했고, 점프/지상 장전의 로컬 위치 차이는0.002 이하로 줄었다. 기존 몸통 결합·탄창 접촉·ADS·발사 안정성·재장전 기준 검사도 포함한다.

최종 전체 PlayMode **177/177 통과**, 실패/스킵0개,170.4초(`Library/CodexJumpReload/final.xml`). 실제 점프 캡처1/1 통과 및 AK/M4 × TP/FP × raised/lowering/settled/reload 화면16장 검토. 임시 촬영 코드는 Library로 이동해 제품에서 제외했다. 원본 기준 해시 재검사 통과 후 코드/씬/테스트/meta6개를 검증본과 바이트 동일하게 적용했다. 씬은 `_aim` 참조 한 줄을 제거하면 원본 바이트와 일치함을 검증했다. 컨트롤러·프리팹·입력·CombatView는 원본 그대로다. 문서2개 포함 변경 파일은 총8개다.

사용자 Editor에서 다양한 점프 시점·이동/조준 각도·착지 직전 장전의 자연스러움과 미세한 손가락/메시 관통 확인은 남는다. 초기 점프의 일부 캡처에서는 캐릭터가 화면 하단으로 치우치는 구간도 관찰됐다. 이번 작업에서 카메라 설정·점프 클립은 변경하지 않았으며 전체 점프 구도 품질까지 완료했다고 주장하지 않는다. 기존 소매 메시 절단면 등 미술 품질도 별도다. 타깃 Player 빌드와 사운드 청취 검사는 실행하지 않는다.
