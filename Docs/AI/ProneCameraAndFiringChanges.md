# 포복 사격 그립 및 포복 전용 카메라 — 2026-11-01

사용자가 승인한 순서대로 사격 손 튐을 먼저 원인 분리·수정한 뒤 카메라를 조정했다. 배필6/배그의 내부 수치를 복제한 작업이 아니라, 이전 조사에서 정한 방향에 맞춘 프로젝트용 시험값이다.

## 사전 비교와 보존
이전 ProneSpeed 적용 기록과 현재 파일을 비교하고, 관련 스크립트·에셋·씬·포복 테스트 총64개를 격리 프로젝트와 비교했다. 모두 일치해 사용자/Editor 후속 변경과의 충돌이 없었다. 최종 복사 직전에도 같은 해시를 확인했다. 포복 진입 .55초/해제 .45초, 이동3m/s와 전환 중 이동 잠금, 기어가기1.5배, 기존 클립/카메라·무기 기본값은 보존했다. 원본 FBX/곡선/Animator는 수정하지 않았다.

## 1. 손 튐 원인과 단일 조건 수정
기존 CanUseItemGrip의 `_shotTime <= 0f` 때문에 발사 순간 그립이 해제됐다. 이를 읽는 기존 FirstPersonArmsView는 sourceGrip=false에서 완전한 팔 메시로 바꾸고 다른 weaponBasis를 사용했다. 기존 FirstPersonArmsView 파일을 수정하지 않고 이 상태 전환 원인을 바로잡았다.

```csharp
// 전: 사격 때 false
... && !IsReloading && _shotTime <= 0f && _meleeBlend <= .001f ...
// 후: 사격도 기존 그립으로 재생
... && !IsReloading && _meleeBlend <= .001f ...
```

원본 그대로 실제 발사 재현 → 조건 하나 수정 → 같은 재현 순서로 검증했다. 각 경우 1.6초 동안 실제 발사15~16회이며, 진단의 의도적 카메라 반동만 임시 비활성화했다(제품 값 변경 없음).

| 1인칭 측정 | 수정 전 | 수정 후 |
|---|---:|---:|
| AK47/M4A1 최대 총-조준 각도 | 60.93° | 1.14° |
| 권총 최대 총-조준 각도 | 76.55° | 3.80° |
| 권총 손 최대 상승 | .932m | .0068m |
| 권총 손 최대 프레임 이동 | 1.179m | .0567m |
| 발사 구간 팔 메시 교체 | 2회 | 0회 |

남은 작은 각도 차이는 기존 무기 반동을 포함한 값이다. 3인칭 손 최대 프레임 이동 약 .055m는 전후 동일해 원본 발사 동작을 없애거나 얼린 수정이 아니다. 소총/권총 모두 1·3인칭으로 확인했다.

## 2. 카메라 설정과 동작
프로필 위치: Assets/Features/PlayerCharacter/Settings/ProneCamera.asset. PlayerCharacter의 PlayerCharacterView → Prone Camera 참조로 연결된다. 카메라는 기존 View 참조를 통해 같은 프로필을 읽는다.

| 항목/직렬화 필드 | 이전 포복(일반 설정 재사용) | 새 포복 설정 |
|---|---|---|
| _pivotFromFeet | 발 기준 높이 .7 / 앞1.3 | (0,1.7,.25) |
| _thirdPersonOffset | (.8,.25,-5) | (.6,.5,-3.6) |
| _aimOffset | (.65,.05,-3) | (.5,.3,-2.6) |
| _aimPitchUp / _aimPitchDown | 80 / 80 | 30 / 45 |
| _freePitchUp / _freePitchDown | 80 / 80 | 30 / 65 |
| _aimYawLimit | 마우스 yaw가 바로 몸 전체 회전 | 몸 기준 ±35도까지 눈에서 회전, 초과분은 몸이 따라감 |

1인칭 포복 눈 위치는 기존 발 기준 약 .7m 높이/앞1.3을 유지한다. 머리 뼈의 흔들림을 카메라로 직접 복사하지 않는다. 진입/해제 눈 위치와 3인칭 설정/각도 제한은 기존 자세 전환 진행률에 맞춰 보간한다. 1인칭 자유 yaw90도/3인칭180도는 기존 값을 유지하며, 자유 시점에서는 몸과 총의 조준 방향을 바꾸지 않는다. 해제 시 잔여 조준 yaw를 몸에 넘겨 시선 방향을 보존한다.

3인칭 회전 중심과 오프셋은 포복용 데이터로 분리했다. 기존 standing/crouch 벡터/한계 필드는 삭제·변경하지 않고 정상 자세와 프로필 없는 환경의 기준값으로 유지한다. 카메라와 GetAimRay 모두 같은 계산을 사용한다.

동일한 최대 상향 입력 비교(기존80도/새30도, 같은 평지): 기존 설정은 바닥 충돌로 거리 .4696m까지 줄었고, 포복 설정은3.6837m를 유지했다. 거리 수치는 벡터 길이이므로 뒤쪽 오프셋3.6m와 다르다. 벽 충돌에 따른 거리 축소와 제거 후 복원도 유지된다.

## 바꾼 모든 파일

- [Assets/Features/PlayerCharacter/Scripts/PlayerCharacterProneAnimationView.cs](E:/GitHub/Battlefield_Project/Assets/Features/PlayerCharacter/Scripts/PlayerCharacterProneAnimationView.cs)
  - CanUseItemGrip에서 사격 시간 조건 `_shotTime <= 0f`만 제거하고 의도 주석 추가. 사격 클립/반동 재생은 유지하면서 그립 및 1인칭 메시·기준 프레임 교체를 방지한다. 장전/근접/진입/해제의 해제 조건은 그대로다.

- [Assets/Features/PlayerCharacter/Scripts/PlayerCharacterView.cs](E:/GitHub/Battlefield_Project/Assets/Features/PlayerCharacter/Scripts/PlayerCharacterView.cs)
  - private _proneCamera 참조와 ProneCamera/ProneBlend 읽기 전용 속성, 자세 버전별 보간 상태 및 _proneAimYaw 추가. OnEnable/UpdateProneBlend/ClampAimPitch/ClampFreePitch와 방향별 제한 속성 추가. AimRotation/LookRotation/Tick은 포복 제한과 몸 기준 잔여 조준 yaw를 반영. ±35도 안에서는 몸 회전 없이 시선을 돌리고 초과분만 기존 AllowsHeading 검사 후 몸에 전달. 포복 해제 완료 때 잔여 yaw를 몸으로 넘겨 바라보던 방향 보존. 자유 시점/복귀는 더 넓은 look 제한을 사용해 아래 65도에서 45도로 갑자기 잘리지 않게 한다. 글로벌 기존 80도/자유 yaw90·180 값은 보존.

- [Assets/Features/PlayerCharacter/Scripts/PlayerCharacterCamera.cs](E:/GitHub/Battlefield_Project/Assets/Features/PlayerCharacter/Scripts/PlayerCharacterCamera.cs)
  - 자세 버전과 시작 높이/전방 오프셋을 보관해 기존 Stance.ProneTransitionProgress의 SmoothStep에 맞춰 눈 위치 변경. GetThirdPersonPivot/GetThirdPersonOffset 추가. LateUpdate/GetAimRay의 중복된 pivot/offset 계산을 이 함수로 교체해 표시 카메라와 조준선에 같은 프로필 사용. 기존 충돌 SphereCast와 즉시 거리 축소/완만한 복원 유지.

- [Assets/Scenes/SampleScene.unity](E:/GitHub/Battlefield_Project/Assets/Scenes/SampleScene.unity)
  - PlayerCharacterView 문서(fileID950000004)에 `_proneCamera: {fileID:11400000, guid:4ee883c1208933f489a18f191478baa8, type:2}` 1줄 추가. 다른 문서·값·계층·위치 변경 없음.

- [Assets/Features/PlayerCharacter/Scripts/ProneCameraProfile.cs](E:/GitHub/Battlefield_Project/Assets/Features/PlayerCharacter/Scripts/ProneCameraProfile.cs)
  - 신규 ScriptableObject. private 벡터3개(pivotFromFeet/thirdPersonOffset/aimOffset), aim/free 상하 한계4개, aimYawLimit을 소유하고 읽기 전용으로 제공. 시점과 카메라가 같은 데이터를 읽는다. 새 런타임 컴포넌트/관리자 없음.

- [Assets/Features/PlayerCharacter/Scripts/ProneCameraProfile.cs.meta](E:/GitHub/Battlefield_Project/Assets/Features/PlayerCharacter/Scripts/ProneCameraProfile.cs.meta)
  - Unity가 생성한 신규 GUID 및 임포트 메타데이터. 기존 .meta는 변경하지 않았다.

- [Assets/Features/PlayerCharacter/Settings/ProneCamera.asset](E:/GitHub/Battlefield_Project/Assets/Features/PlayerCharacter/Settings/ProneCamera.asset)
  - 신규 포복 카메라 시험값 프로필. 아래 전체 직렬화 값 표 참조.

- [Assets/Features/PlayerCharacter/Settings/ProneCamera.asset.meta](E:/GitHub/Battlefield_Project/Assets/Features/PlayerCharacter/Settings/ProneCamera.asset.meta)
  - Unity가 생성한 신규 GUID 및 임포트 메타데이터. 기존 .meta는 변경하지 않았다.

- [Assets/Tests/PlayMode/ProneFiringStabilityTests.cs](E:/GitHub/Battlefield_Project/Assets/Tests/PlayMode/ProneFiringStabilityTests.cs)
  - 신규 회귀 테스트. AK47/M4A1/권총 × 1/3인칭에서 실제 Fired 이벤트를 생성하는 연사 검사. 의도적 카메라 반동은 진단 세션에만 꺼서 그립 문제를 분리. 손 상승<.08m/프레임 이동<.1m, 총-조준 오차<8도, 팔 메시 유지, 실제 발사 횟수>5 확인.

- [Assets/Tests/PlayMode/ProneFiringStabilityTests.cs.meta](E:/GitHub/Battlefield_Project/Assets/Tests/PlayMode/ProneFiringStabilityTests.cs.meta)
  - Unity가 생성한 신규 GUID 및 임포트 메타데이터. 기존 .meta는 변경하지 않았다.

- [Assets/Tests/PlayMode/ProneCameraTests.cs](E:/GitHub/Battlefield_Project/Assets/Tests/PlayMode/ProneCameraTests.cs)
  - 신규 통합 테스트. 원본 프로필 연결, 낮은 바닥에서 상향 최대 조작 시 3m 이상 거리 유지, 조준선 원점 정합, 벽 거리 축소/복원, 견착 거리, 1인칭 눈 높이 .7m와 작은 pitch/yaw 때 위치 유지, 몸 회전 경계35도, 자유 시점/부드러운 복귀, 해제 후 바라보던 방향과 기존 3인칭 거리 복원을 검사.

- [Assets/Tests/PlayMode/ProneCameraTests.cs.meta](E:/GitHub/Battlefield_Project/Assets/Tests/PlayMode/ProneCameraTests.cs.meta)
  - Unity가 생성한 신규 GUID 및 임포트 메타데이터. 기존 .meta는 변경하지 않았다.

- [Docs/AI/ProneCameraAndFiringChanges.md](E:/GitHub/Battlefield_Project/Docs/AI/ProneCameraAndFiringChanges.md): 이 변경의 전체 전후·파일·값·검증 기록(신규).
- [Docs/AI/UnityProjectContext.md](E:/GitHub/Battlefield_Project/Docs/AI/UnityProjectContext.md): 최신 상태/프로필 위치/검증 근거 추가.

## 제거·교체한 구현
제거한 기존 필드·메서드·컴포넌트·참조·에셋은 없다. CanUseItemGrip에서 사격 시간 조건 하나를 제거했다. View의 글로벌 pitch clamp 직접 호출은 자세별 ClampAimPitch/ClampFreePitch로 교체했다. Camera의 공유 eye pivot/일반 offset 직접 계산은 자세별 공용 두 함수로 교체했다. 기존 직렬화 값은 프로필이 적용되지 않는 경로에 유지한다. Source 발사 클립과 _shotTime/재생 분기는 그대로 남는다. 임시 진단/에셋 생성 도구는 제품에 포함하지 않는다.

## 구조 검토
ProneCameraProfile은 실제로 두 소비자가 공유하는 자세별 데이터 책임이 있어 분리했다. View는 입력·시선·몸 방향 정합, Camera는 표시 위치·충돌, 기존 ProneAnimationView는 포복 원본 재생/그립 허용을 담당한다. 자세 시간의 권한은 기존 Stance가 유지한다. private 상태·읽기 전용 속성·기존 명시적 참조를 사용한다. 상속/범용 관리자/불필요한 인터페이스를 추가하지 않았다. 기존 Camera→View 직접 참조를 통해 데이터를 공유하는 것은 동일 캐릭터 표현 조합이라는 책임에 맞춘 선택이며 OOP/SOLID 의도적 예외는 없다. 프레임 루프의 새 동작에 컬렉션 할당/오브젝트 생성은 없다(성능 프로파일링은 별도 실행하지 않음).

## 검증과 남은 확인
- Unity6000.4.8f1, 모든 진단/검사 `-batchmode -noaudio`. 제품 오디오 값/설정 변경 없음.
- 원본 사격 재현1개 → 그립 단일 조건 수정 후 관련2개 통과 → 카메라 관련15개 통과 → 최종 전체189/189 통과, 실패/스킵0, 178.90초(제품 검사188개+임시 카메라 캡처1개).
- 실제 발사, 1/3인칭, 세 무기, 포복 전환·잠금/장전/근접·기존 이동/무기/탑승 회귀 검사 포함. 명중·피해·탄약·발사 속도 규칙은 바꾸지 않았다.
- 소총/권총 사격 전후 및 카메라 평지/상향/하향/견착/1인칭/ADS 실제 렌더 캡처를 검토했다. 진단은 격리 SampleScene 복사본에서 수행했고 별도 테스트 씬을 만들지 않았다.
- 대상 플랫폼 빌드는 실행하지 않았다. 본 Editor에서 실제 지형·경사면·극단 조작 및 사용자가 원하는 구도를 확인해야 한다. 포복 전용 좌우 이동/권총 모션의 원본 자원 부족이나 기존 무기별 1인칭 배치를 완성한 작업으로 주장하지 않는다. BF6/배그와 프레임 단위로 동일하다고 주장하지 않는다.
- 제품에 복사한 파일은 검증 소스와 동일 해시. 증거: Library/CodexProneAimCamera/baseline-fire.json, fixed-fire.json, camera-comparison.json, full.xml, scope.patch, applied.json.

## 테스트/조정
SampleScene에서 Play를 다시 시작한다. Z 포복 후 1/2 무기 연사, V 시점 전환, 우클릭 견착/ADS, 작은 좌우 회전과35도 초과, 위/아래 최대 입력, Alt 자유 시점, 벽 접근, 장전, Z 해제를 확인한다. 구도/각도는 위 ProneCamera 프로필에서 조절한다. 기존 View의 글로벌80도는 일반 자세의 값이며 포복에서는 프로필 한계를 사용한다.
