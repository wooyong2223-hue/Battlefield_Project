# 무장 몸통·팔 안정화 (2026-10-30)

사용자 요청: 총을 든 팔과 몸통을 최대한 고정하고 다른 게임의 처리 방식 확인.

## 근거와 범위

Epic 공식 Layered Animations 예제는 하체 이동 위에 상체 발사 모션을 별도 적용한다. 일반적인 무기 표현은 이동/상체 무기 자세/조준 보정/손 IK를 조합한다. 특정 상용 게임의 비공개 구현과 같다고 주장하지 않는다.
- https://dev.epicgames.com/documentation/en-us/unreal-engine/using-layered-animations-in-unreal-engine
- https://academy.unity.com/courses/animation-rigging-overview

현재 프로젝트는 팔 전용 UpperCombat + 몸통 LookAt/보행 보정 + 양손 IK 구조였다. 기존 보행 보정은 지상 Locomotion/CrouchLocomotion의 어깨 yaw/roll만 제한한다. 점프/착지 pitch는 남아 실제 무장 달리기·점프에서 몸통이 대기 자세 대비42.702도 변했다(before.xml/log).

기존 애니메이션을 그대로 사용하면서 무장 상태의 몸통 회전을 안정화한다. 의도적인 마우스 조준/기울이기/반동/장전·근접·엎드림·슬라이딩/맨손 모션은 유지한다. 새 모션 키프레임/클립·마스크·레이어·씬 오브젝트·프로필 수정은 없다. 현재 검증 장비는 권총이다.

## 작업 전 비교

직전 StarterRootCamera applied.json6개 파일 동일. AimView/CombatView/FPArms/WeaponPoseState/controller/UpperCombat.mask/Scene도 격리본과 동일. baseline.json에7개 해시 기록; 원본 반영 직전 재비교했고 동일했다. 최신 사용자 값/구조와 충돌 없음. 원본 반영3개 파일과 검증본 바이트 일치를 확인했다.

## 제품 변경 파일

### Assets/Features/PlayerCharacter/Scripts/PlayerCharacterWeaponAimView.cs

추가 데이터:
- _chestReference: Awake에서 `inverse(combat.rotation) * chest.rotation`으로 모델의 기준 본 축을 보존. Chest가 없으면 identity.
- _lean: 명시된 _combat 소유자의 기존 PlayerCharacterLean을 Awake에서 캐시. 입력을 읽지 않고 읽기 전용 Roll만 소비해 의도적 기울이기를 보존한다.
- _rightGripShoulder/_leftGripShoulder: 현재 Animator 평가에서 몸통 보정 후 예상 어깨 위치. 직렬화 값이 아니다.
- ArmedTorsoPitchLimit=3도: 새 앞뒤 숙임 잔여 제한. 기존 yaw/roll 제한3도는 변경하지 않는다.

OnAnimatorIK의 기존 분기:
`if (armed || meleeWeight <= .01) StabilizeLocomotionTorso();`
를
`if (CanStabilizeArmedTorso) StabilizeArmedTorso(); else 기존 보행 보정;`
으로 교체했다. 기존 보행/맨손/근접 보정 메서드들은 삭제하지 않았다.

새 CanStabilizeArmedTorso는 UsesItemGrip + Posture.UpperBodyAimMultiplier>.99 + TraversalMode==0을 요구한다. 장전/공격 및 남은 액션 혼합, 비활성/사망/탑승, 엎드림/슬라이딩/넘기는 기존 동작이 소유한다. 지상 여부/속도 제한을 제거한 무장 전용 경로이므로 대기·이동·점프·착지에서 동작한다.

새 StabilizeArmedTorso:
1. 기준 본 축을 제거한 캐릭터 로컬 몸통 pitch를 구한다. 의도적 pitch는 `AimRotation.x * Clamp01(_bodyAimWeight)`이며, 그 외 잔여 숙임만 기존 atan 압축으로3도까지 완만히 제한한다.
2. 어깨 연결선의 yaw와 roll을 구한다. yaw 잔여는3도, roll은 기존 Lean.Roll을 기준으로 잔여만3도 제한한다. 사용자 Q/E 의도 각도를 제한값으로 대체하지 않는다.
3. 어깨 선을 목표 방향으로 맞춘 후 그 축 주위 twist를 구한다. Rodrigues 공식의 수직 성분 `A*cos(t)+B*sin(t)+C=-sin(targetPitch)`를 풀어 어깨 정렬을 깨지 않고 조준 pitch를 유지한다. 두 해 중 해부학적 up이 캐릭터 up에 가까운 해를 선택해 뒤집힘을 방지한다. Euler 보정을 순서대로 겹쳐 조준이 약해지던 중간안은 채택하지 않았다.
4. 전체 보정의 half quaternion을 Spine과 Chest/UpperChest에 분산 적용한다. Head에는 역보정을 적용해 기존 LookAt의 시선을 유지한다. 몸통 본의 회전만 바꾸며 캐릭터/모델 Transform이나 하체 본은 직접 수정하지 않는다.
5. 같은 두 본의 회전이 어깨 위치에 미칠 결과를 예측한다. 이후 손 목표의 도달 범위를 이 새 위치로 계산해 수정 전 어깨 위치와 수정 후 몸통 사이 불일치를 막는다.

ApplyGrip 변경:
- WithinReach의 기존 `_rightUpperArm.position`/`_leftUpperArm.position` 중심 → 보정 후 어깨 중심.
- 두 팔 도달 구 교차 투영 반복4회 →16회. 기존 위치·그립·회전 오프셋/팔 길이/무기 반동 값은 유지하며 손 접점 오차 허용값을 늘리지 않았다.
- FPS LateUpdate의 기존 팔 솔버는 평가 완료된 실제 본 위치를 사용하므로 변경하지 않았다.

삭제한 기존 메서드/직렬화 필드/컴포넌트/참조/에셋 없음. 교체한 것은 위 OnAnimatorIK 분기와 손 도달 중심/반복 횟수다. 기존 UpperCombat mask, 컨트롤러, CombatView/FirstPersonArmsView/Camera/RootFollow 상태·설정은 원본 그대로다.

### Assets/Tests/PlayMode/PlayerCharacterArmedStabilityTests.cs 및 .meta 신규

양 시점에서 대기→질주→점프→이동을 고정 .02초로 실행한다. 대기 대비 몸통 회전<7도, 실제 공중 진입, 물리 점프높이1.2m(±.15), 하체 발 관절 이동>.15m, 모델 루트 위치/회전 불변을 확인한다. 바닥과 Time.captureDeltaTime을 정리/복원한다. 기존 조준·기울이기·그립 테스트의 기대값/허용폭은 수정하지 않았다.

문서 Docs/AI/ArmedTorsoStabilityChanges.md 신규, Docs/AI/UnityProjectContext.md 기록 추가.

## 구조 검토

몸통과 양손을 같은 Animator IK 평가에서 함께 해결해야 하므로, 기존 AimView 책임 안에 별도 private 메서드로 분리했다. 별도 MonoBehaviour를 만들면 같은 본/참조와 평가 순서 조율이 중복돼 새 관리자/인터페이스는 추가하지 않았다. 기준 본 축과 보정 상태는 내부 소유, 외부에는 추가 가변 상태를 노출하지 않는다. 기존 CombatView의 UsesItemGrip과 Lean.Roll 읽기 계약을 사용해 입력/공격 상태를 중복 소유하지 않는다. OOP/SOLID 의도적 예외 없음. 프레임별 힙 할당/씬 검색 없음.

## 검증 과정과 최종 범위

모든 Unity 실행은 격리 Unity6000.4.8f1 -batchmode -noaudio. 원본 Editor/오디오 설정 조작 없음.

레이어만으로 고정하는 안(Body/Root 마스크 + 기존 중립 권총 클립)은 최대19~38도 변화 및 시점 조준 차이를 만들었고, 몸통 bodyRotation 설정만으로도 목표에 도달하지 못해 폐기했다. 해당 생성기/마스크/진단은 Library/CodexArmedStability/Prototype에 보관하고, 격리 controller/CombatView도 원본으로 복원했다. 제품에 새 레이어나 마스크를 반영하지 않는다.

초기 전체157개 중153개 통과,4개 실패(full.xml/log): 총구 위치, 손 접점, 의도적 lean, 어깨 roll. 수정된 어깨 위치를 도달 계산에 사용하고 반복16회로 변경해 손/총구 회귀를 해결, Lean.Roll 의도를 분리하고 실제 어깨 선을 제한해 lean/어깨 회귀 해결. 순차 pitch/어깨 보정의 간섭은 해석적 twist 계산으로 해결했다. 관련 최종 집중48/48 통과(refined2.xml/log), 실제 몸통 변화5.411도(TP)/5.291도(FP). 이전42.702도 대비 약87% 감소한 해당 테스트 조건의 측정이며 모든 상황의 절대 최대값을 뜻하지 않는다.

최종 Features PlayMode 전체158/158 통과(final.xml/log, 실패/건너뜀0): 제품 검사157개와 Library 전용 캡처1개. 기존 테스트 기대값/허용폭은 변경하지 않았으며 기울이기·어깨·손·조준·장전·맨손·엎드림·탑승·더미 팀/재질 사격장 검사 포함. 원본 기준 영상 capture-before.xml도1/1 통과. 동일 입력/카메라로 양 시점110프레임씩50fps 전후 렌더링 후 직접 화면 확인. Side observer TP 비교에서 몸통이 유지되고 다리 점프/착지는 계속 동작, FPS 총 그립 화면 확인. 관찰용 카메라는 임시 객체이며 제품 카메라 값 변경 없음. Overlay Canvas는 포함하지 않는 캡처다. 비교영상 armed-stability-comparison.mp4 왼쪽 이전/오른쪽 수정. 모든 임시 코드·prototype·이미지·로그는 Library에만 존재하며 원본 반영 코드와 검증본 해시 일치. 사용자 Editor의 실제 시각 품질/체감과 플랫폼 빌드는 별도 확인이 필요하다.
