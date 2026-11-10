# 장전 중 몸통과 손의 결합 수정

2026-10-30. 권총·AK-47·M4A1, 1·3인칭 대상.

## 사전 비교·재현

이전 장전/교체대 및 발사 안정성 적용 해시를 병합해 원본과 비교했다. 후속 사용자/Editor 변경은 없었다. 원본 Editor 대신 격리 프로젝트에서 `-batchmode -noaudio`로 검사했다.

기존 탄창 접촉 검사는 왼손과 ReloadGrip 거리만 검사했다. 몸통과 손이 같은 기준으로 움직이는지는 확인하지 못했다. 새 검사에서는 장전 중 Animator 시간을 멈추고 Chest를 12cm 이동시킨 뒤 실제 장전 보정과 1인칭 복제를 호출했다.

| 기존 동작 | 몸통 이동에 대한 오른손 이동 | 기대 |
|---|---:|---:|
| 소총 3인칭 | 약 0.1cm | 12cm |
| 소총 1인칭 | 0cm | 표시 배율0.7에 따른 8.4cm |
| 권총 3인칭 | 약 12cm | 12cm |
| 권총 1인칭 | 0cm | 12cm |

소총 월드 손은 카메라에 고정된 목표를 사용했다. 1인칭은 매 프레임 현재 손/poseAnchor 위치와 회전을 역변환해 몸통 이동까지 상쇄했다. 권총 3인칭은 이 재현에서 정상이라 해당 장전 모션을 교체하지 않았다.

## 변경한 모든 파일 (7개)

### Assets/Features/PlayerCharacter/Scripts/PlayerCharacterRifleReloadView.cs

기존:

```csharp
rightOffset = Inverse(aimRotation) * (rightHand.position - camera.EyePosition);
right = camera.EyePosition + aimRotation * rightOffset;
rightRotation = aimRotation * Slerp(startRotation, inverseGripRotation, progress);
```

변경:

```csharp
rightTorsoOffset = torso.InverseTransformPoint(rightHand.position);
rightRotationInTorso = Inverse(torso.rotation) * rightHand.rotation;
right = torso.TransformPoint(rightTorsoOffset);
rightRotation = torso.rotation * rightRotationInTorso;
```

위는 핵심 계산 발췌다. 실제 코드는 기존 private 필드와 Quaternion API를 사용한다. `_torso`는 Animator의 UpperChest, 없으면 Chest에서 캐시한다. `_rightOffset`을 `_rightTorsoOffset`, `_rightRotationInAim`을 `_rightRotationInTorso`로 교체했다. 두 필드는 런타임 private 상태이며 직렬화 필드가 아니다.

카메라/조준 기준 고정 목표와 장전 초반 aim 회전 보간을 제거하고, 장전 시작 손 자세를 몸통 로컬 기준으로 저장한다. 이후 몸통 애니메이션·이동·회전이 손과 총을 함께 운반한다. 왼손의 탄창 접촉, 양팔 도달 범위, 기존 탄창 이동·복원·취소 수명주기는 유지한다. 기존 serialized 참조와 오프셋 값은 변경하지 않는다.

### Assets/Features/PlayerCharacter/Scripts/PlayerCharacterCombatView.cs

읽기 전용 `ReloadPoseWeight => _reloadBlend` 추가. 장전 완료 후 기존 상체 레이어가 페이드되는 구간까지 1인칭 복제가 같은 가중치로 원래 그립에 복귀할 수 있게 한다. 장전 시간·애니메이션 선택·반동·이벤트·상태 소유권은 변경하지 않는다.

### Assets/Features/PlayerCharacter/Scripts/FirstPersonArmsView.cs

- `_reloadTorso`, `_hasReloadFrame`, `_reloadTorsoPosition`, `_reloadTorsoRotation`, `_reloadBasis` 런타임 캐시 추가. 첫 복제에서 몸통 위치/회전과 기존 표시 구도의 변환을 캐릭터 루트 기준으로 저장한다.
- 기존 매 프레임 그립/poseAnchor 정규화는 유지한다. 그 위에 몸통의 이후 변화량을 **양팔 전체에 동일한 이동·회전**으로 적용한다. 1인칭 몸통 변화량은 추가 이동 최대0.2m(소총 표시 배율 적용 전), 회전 최대15도다. 외부 모션의 큰 몸통 회전 때문에 총이 시야 밖으로 크게 벗어나는 것을 줄이는 새 런타임 표현 제한이다. 기존 Inspector 값 변경은 없다.
- 회전은 복제된 몸통 위치를 중심으로 적용한다. 양손·손가락·팔 뼈 모두 공통 변환을 적용하고, 기존 소총 어깨/팔꿈치의 화면 보정은 유지한다. 1인칭은 월드 몸통의 모든 움직임을 1:1 복제하는 것이 아니라 기존 별도 팔 리그 안에서 제한된 몸통 동작을 표현한다. 3인칭은 이런 제한 없이 실제 몸통을 따른다.
- 기존 ReloadPoseWeight로 공통 변화량을 페이드한다. 가중치0, 3인칭/조작 불가, OnDisable에서 캐시를 해제한다. 장전 중 1인칭으로 전환하면 현재 자세를 새 기준으로 잡는다.
- 기존 FP 오프셋, ADS 어깨·팔꿈치 값, 장전 ViewOffset, 표시 배율, 카메라 자체는 변경하지 않는다. 일반 그립·발사·ADS·근접 경로도 유지한다.

### Assets/Tests/PlayMode/ReloadBodyFrameTests.cs 및 .meta (신규 2개)

AK/M4/권총 × 1·3인칭의 실제 SampleScene Animator/장전 보정/복제 팔을 사용한다. 장전 중 몸통12cm 이동과12도 회전이 손에도 같은 변환으로 적용되는지 검사한다. FP는 장전 도중 시점 전환으로 새 기준을 잡아 제한 범위 안의 결합과 캐시 수명주기를 검사한다. FP 소총의 기존 표시 배율0.7을 반영한다. 목표 오차 <1.5cm. 이 검사는 제어된 몸통 변환으로 좌표 결합을 분리해 검증하며 제한을 넘는 월드 몸통 움직임의 1:1 복제나 실제 이동 조작의 모든 시각 품질을 보장하는 검사는 아니다. 기존 테스트 파일/허용오차는 변경하지 않는다. meta는 테스트 GUID 파일이다.

### Docs/AI/ReloadBodyAttachmentChanges.md 및 Docs/AI/UnityProjectContext.md

본 변경 보고와 현재 상태·검증 증거를 기록한다. 기존 문서 내용을 유지한다.

## 유지한 구조·설계

장전 상태와 모델 접촉은 기존 RifleReloadView, 복제 좌표계는 기존 FirstPersonArmsView가 소유한다. CombatView는 기존 장전 가중치를 읽기 전용으로만 제공한다. 기존 책임에 속하는 좌표 보정이므로 새 Manager·상속·인터페이스·컴포넌트를 추가하지 않았다. 표현 상태를 게임플레이 장전/탄약 소유자와 혼합하지 않았다.

씬·프리팹·Animator Controller·클립·무기 정의·AudioSource·입력 설정 변경 없음. 기존 외부 Quaternius 클립을 유지한다. 삭제한 에셋/컴포넌트/직렬화 필드/참조 없음. 교체한 코드는 위 몸통 기준 손 목표 계산과 1인칭 복제의 공통 몸통 변화량이며, 교체한 private 필드2개도 위에 명시했다. 메서드 전체 삭제는 없다.

## 검증

수정 전 재현 실패: `Library/CodexReloadBody/before.xml/log`. 초기 집중 검사5/5 통과: 몸통 이동 결합, 기존 탄창 접촉/교체 취소, ADS, 발사 안정성. 수정 후 소총 TP 이동 약12.01cm, FP 약8.22cm(기대8.4cm), 권총FP 약12.04cm였다. 확장한 몸통 회전/M4 및 전체 최종 결과는 final.xml, 실제 장전 화면 검사는 capture.xml과 FP/TP PNG에 보관한다. 임시 촬영 코드는 제품에 포함하지 않는다.

초기 몸통 전체 정규화 고정 후보는 전체173/173을 통과했지만 화면 검사에서 1인칭 구도 문제가 확인돼 최종 방식으로 교체했다. 중간 공통 변환 후보도 실제 모션이 이미 제한에 도달한 상태에서 추가12도 회전을 요구하던 검사에는 실패했다. 최종 검사는 실제 장전 중 시점 전환으로 표현 기준을 재설정하고 제한 범위 안의 이동/회전 결합을 검증한다. 기존 수치 허용오차를 늘려 통과시키지 않았다. 최종 집중 검사6/6(제품5+임시 촬영1) 통과. AK/M4/권총의 TP/FP 제어된 몸통 변환 결합 오차 최대 약1.4mm. 최종 전체 결과는 `final2.xml`을 따른다. 기존 탄창 접촉·장전 취소·ADS·발사 안정성·전투/탑승 회귀를 포함한다. 씬·컨트롤러·프리팹·입력은 원본과 동일함을 확인한다. 문서2개 포함 변경 파일 총7개.

최종 후보 전체 PlayMode `final2.xml`: **173/173 통과**, 실패/스킵0개,159.6초. 원본의 중간 적용 상태 이후 사용자 변경이 없는 것을 다시 확인하고 최종5개 코드/테스트/meta를 검증본과 바이트 동일하게 적용했다. 적용 후 스크립트 diff 및 해시 재검사 통과. 실제 장전 화면6장 검토 및 동일 프레임 비교 검사 `captureCompare.xml` 1/1 통과. 임시 촬영 코드는 Library로 이동해 제품에서 제외했다.

화면 검토의 한계: 권총 장전 중간은 기존 정규화(`Pistol_FP_BaselineNormalization.png`)와 현재 결과(`Pistol_FP.png`) 모두 총이 대부분 화면 밖으로 벗어나는 구간이 있다. 이번 작업에서 이 구도/클립 품질 문제까지 해결했다고 주장하지 않는다. 기존 및 현재 소총 캡처에서 보이는 소매 절단면도 별도 표시 리그/메시 품질 문제로 남아 있다. 몸통 좌표 결합 수정과 이 시각 품질 이슈는 구분한다.

사용자 Editor에서 이동·상하 조준·장전 종료 시 시각적 연결과 손가락/장비 관통 확인은 남는다. 타깃 Player 빌드와 사운드 청취 검사는 실행하지 않는다.
