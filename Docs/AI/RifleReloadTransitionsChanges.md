# 소총 장전 진입·재시작·ADS 위치 수정

2026-10-30. AK-47/M4A1의 간헐적인 장전 위치 오류를 비교해 수정했다.

## 사전 비교와 재현 증거

직전 장전/발사/몸통 수정의 최신 적용 해시를 병합한 기준과 현재 원본을 비교했다. 관련 사용자/Editor 변경 없음. 원본 대신 Library 격리 프로젝트에서 구현·검사했으며 원본 Editor 및 오디오 상태는 조작하지 않았다.

수정 전 재현 검사3개가 모두 실패했다(`Library/CodexReloadTransitions/before.xml/log`).

| 조건 | 수정 전 측정 | 원인 |
|---|---|---|
| 같은 조준 상태에서 TP/FP로 각각 장전 시작 | FP 소총 시작 회전이 조준 방향과 약18.31도 차이. 몸통 로컬 손 위치도 TP `(0.05,0.35,0.16)`, FP `(0.32,0.06,-0.01)`처럼 다름 | FP 소스 스켈레톤은 기존 조준 IK를 건너뛰고 외부 클립의 손 자세를 사용했는데, 장전은 그 소스 자세를 그대로 저장함 |
| 취소 직후 같은 프레임에 재장전 | 새 장전의 복제 결과가 리그 재초기화 후 같은 자세와 약14.9cm 차이 | 이전 장전 페이드가0이 되기 전에는 `_hasReloadFrame`이 남아 옛 몸통 기준을 사용함 |
| 장전 중 ADS 켜기/끄기 | 버튼 전환 순간 추가 오프셋이 약25.79cm 점프 | 기본 위치는 카메라 ADS blend를 사용하고, 장전 추가 위치는 IsAds의 즉시0/1을 사용함 |

## 변경한 모든 파일 (8개)

### 1. Assets/Features/PlayerCharacter/Scripts/PlayerCharacterWeaponAimView.cs

기존 OnAnimatorIK 분기:

```csharp
if (_view.IsFirstPersonView) return;
```

변경:

```csharp
if (_view.IsFirstPersonView &&
    (_presentation.ActiveDefinition == null || !_presentation.ActiveDefinition.CustomGrip)) return;
```

현재 CustomGrip을 사용하는 소총은 FP에서도 기존 TP 그립 계산을 소스 스켈레톤에 적용한다. 렌더링되는 FP 위치는 기존 별도 복제 리그가 계속 담당한다. 따라서 시점별로 서로 다른 손 자세를 장전 기준으로 저장하지 않는다. 기존 팔꿈치 힌트·상하 조준·손 목표/도달 범위 수치는 유지한다. 권총은 기존 FP 경로 유지. 기존 FP LateUpdate 보정도 유지하며 변경된 책임을 설명하도록 주석만 고쳤다. 메서드 삭제/추출 없음.

### 2. Assets/Features/PlayerCharacter/Scripts/PlayerCharacterRifleReloadView.cs

장전 시작 회전:

```csharp
// 이전: 이전 평가에서 남은 손 뼈의 회전 사용
_rightRotationInTorso = Quaternion.Inverse(_torso.rotation) * _rightHand.rotation;
// 이후: 현재 조준 방향과 해당 무기의 기존 그립 회전으로 계산
_rightRotationInTorso = Quaternion.Inverse(_torso.rotation) * _view.AimRotation *
    Quaternion.Inverse(_presentation.GripRotation);
```

FP 클립의 보정 전 손목 회전이나 이전 프레임 회전을 장전 시작 회전으로 보존하는 경로를 교체했다. 몸통 로컬 손 위치 저장과 이후 몸통 결합은 유지한다.

추가 표시 오프셋:

```csharp
// 이전
Vector3.Lerp(_hipViewOffset, _adsViewOffset, _view.IsAds ? 1f : 0f) * Blend;
// 이후
Vector3.Lerp(_hipViewOffset, _adsViewOffset, _camera.FirstPersonAimBlend) * Blend;
```

기존 카메라 보간값을 공유한다. 오프셋 값이나 보간 속도 자체는 바꾸지 않는다.

### 3. Assets/Features/PlayerCharacter/Scripts/PlayerCharacterCombatView.cs

읽기 전용 런타임 속성 `public int ReloadSequence { get; private set; }` 추가. 성공한 ReloadStarted에 연결된 OnReload 시작에서 `ReloadSequence = unchecked(ReloadSequence + 1)`로 증가한다. 실패한 장전 요청에는 이벤트가 발생하지 않아 증가하지 않는다. 장비 변경/일반 ResetPose에서 되돌리지 않는 컴포넌트 수명 범위의 순번이다. 게임플레이 탄약·장전 시간이나 저장 데이터는 소유하지 않는다. 기존 장전 가중치·클립 재생 경로는 유지한다.

### 4. Assets/Features/PlayerCharacter/Scripts/FirstPersonArmsView.cs

런타임 private `_reloadSequence` 추가. 장전 몸통 기준 캡처 조건:

```csharp
// 이전
if (!_hasReloadFrame)
// 이후
if (!_hasReloadFrame || _reloadSequence != _presentation.ReloadSequence)
```

기준을 잡을 때 현재 순번도 함께 저장한다. 취소→재시작 사이에 CopyPose가 한 번도 실행되지 않았더라도 새 장전을 구분해 이전 기준을 버린다. 기존 기준 캐시·페이드·시점 전환/OnDisable 해제와 1인칭 몸통 이동0.2m/회전15도 제한은 유지한다. 기존 필드/메서드 삭제 없음.

### 5–6. Assets/Tests/PlayMode/RifleReloadTransitionTests.cs 및 .meta (신규)

실제 SampleScene의 AK/M4 장비·Animator·IK·복제 리그로 검사한다.

- 양 시점의 같은 조준 상태에서 장전을 시작한 후 월드 그립 방향이 조준 방향과 일치하고, 몸통 로컬 시작 위치 차이가 작아야 한다. 단순 클립 파일 값 비교가 아니라 실제 장전 보정을 평가한다.
- 취소와 재시작을 중간 CopyPose 없이 수행한 결과가 같은 프레임에 리그를 다시 활성화해 얻는 새 기준 결과와 같아야 한다. 몸통 변화가 있던 옛 기준의 재사용을 검출한다.
- ADS 버튼을 누른 같은 프레임의 ViewOffset 변화가3mm 미만이어야 한다. 카메라 보간이 진행된 후 ADS 해제도 같은 조건으로 검사한다.

기존 테스트 파일/수치 허용오차를 변경하지 않았다. meta는 신규 테스트 GUID 파일이다. 재현 로그는 테스트에만 있으며 게임플레이 매 프레임 로그는 추가하지 않았다.

### 7–8. Docs/AI/RifleReloadTransitionsChanges.md 및 Docs/AI/UnityProjectContext.md

본 파일과 프로젝트 상태 기록. 전체 코드 교체·원인·검증·제한을 기록하고 기존 문서 내용을 유지한다.

## 구조·원칙 검토

기존 AimView는 그립 계산, RifleReloadView는 장전 자세, CombatView는 장전 표현 수명/순번, FP view는 복제 기준을 소유한다. 새 책임을 Manager에 합치거나 새 컴포넌트/상속/인터페이스를 추가하지 않았다. 새 읽기 전용 순번만 표현 계층끼리 공유하며 외부에서 직접 변경할 수 없다. 현재 장비 정의의 기존 CustomGrip 분기와 기존 카메라 참조를 재사용한다. 변형이 없는 부분에 추상 계층을 새로 만들지 않았다.

삭제한 에셋·컴포넌트·직렬화 필드·참조·메서드 없음. 교체한 실행 경로는 위 FP 조준 IK 조기 반환 조건, 장전 시작 회전 계산, ADS 오프셋 보간값, 복제 기준 캡처 조건4개다. 순번 속성/캐시2개를 추가했다. 씬·프리팹·Inspector 값·애니메이션·AnimatorController·카메라 설정·반동 값·입력·탄약·발사/피해 알고리즘·오디오 설정은 변경하지 않는다.

## 검증·남은 확인

Unity6000.4.8f1, 격리 `-batchmode -noaudio`. 수정 전3/3 실패 → 수정 후 집중13/13 통과. 시작 회전 오차·재시작 차이·ADS 즉시 점프가 각각 검사 출력 정밀도에서0으로 감소했다. 기존 탄창 접촉·장전 취소·ADS 팔 자세·발사 안정성·몸통 결합·소총 발사/탄약 검사를 포함한다. 전체 결과는 final.xml, 실제 장전 모드별 화면은 capture.xml 및 PNG에 보관한다. 임시 캡처 도구는 제품에서 제외한다.

최종 제품 PlayMode **176/176 통과**, 실패/스킵0개,169.1초(`final.xml`). 추가 캡처 검사1/1 통과. AK/M4의 일반·ADS 직후·취소 후 재시작·장전 중 TP 전환 화면8장을 검토했다. 임시 캡처 코드는 Library로 이동해 제품에서 제외했다. 원본 기준 해시 재검사 통과 후 코드/테스트/meta6개를 검증본과 바이트 동일하게 적용했다. 씬·AnimatorController·AK/M4 프리팹·입력 파일이 원본과 동일한 것도 확인했다. 문서2개 포함 이번 변경 파일은 총8개다.

이전부터 남아 있던 권총 장전 구도/소매 메시 절단면 등 외부 모션·표시 리그의 시각 품질은 이번 소총 진입 오류 수정의 해결 범위가 아니다. 실제 사용자 Editor에서 이동·상하 조준·다양한 장전 진입 타이밍의 자연스러움은 추가 확인이 필요하며 타깃 Player 빌드와 사운드 청취 검사는 실행하지 않는다.
