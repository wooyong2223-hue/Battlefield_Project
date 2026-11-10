# 상황별 손 동작 (2026-10-29)

요청: 장전은 장전 모션, 장착 중에는 그립 유지, 맨손은 이동·점프·공격에 맞는 원본 모션 사용.

작업 전 baseline.json과 관련 7개 파일 SHA256을 비교해 모두 동일함을 확인했다. 이전 Starter Assets 점프 적용 상태를 보존했다. Scene, 권총 프리팹, WeaponModel, 모델 배치, 사용자 손 좌표, 애니메이션 에셋/컨트롤러, 이동·카메라·오디오 설정 변경 없음.

## 수정 파일과 기존 구현 교체

- PlayerCharacterWeaponPoseState.cs: UpperLayerWeight의 `1 - CarryBlend * SprintBlend * .7`을 `1`로, SupportHandWeight의 `1 - CarryBlend * SprintBlend * .9`를 `1`로 변경. 달리기 때 상체 총기 포즈 .3/보조 손 .1까지 약해지던 동작을 제거하고 장착 그립을 유지한다. Carry/Sprint 위치·도달 거리·높이 전환, Rotation(aim)은 유지. 공개 프로퍼티 제거 없음.
- PlayerCharacterCombatView.cs: 읽기 전용 UsesItemGrip 추가. 활성·장착·공격 가능 상태이며 장전/근접 공격 중이 아니고 Reload/Melee 오버레이가 끝났을 때만 true. 기존 맨손 `IsFirstPersonView || IsAiming` 가드를 `grounded && (IsAiming || (IsFirstPersonView && Speed < .05))`로 교체해 맨손 이동/점프의 팔을 대기 주먹 자세가 덮지 않게 한다. 실제 주먹 공격은 기존 _meleeAction이 우선한다. ResetPose에 _meleeBlend=0 및 MeleeCombat weight0 추가해 장비 전환 시 이전 주먹 레이어 잔여 혼합을 제거한다. 장전 클립/전용 레이어/재생시간 유지.
- PlayerCharacterWeaponAimView.cs: 기존 private CanAim 프로퍼티를 제거하고 UsesItemGrip 계약과 시점 분기로 대체. TP의 기존 양손 Animator IK를 ApplyGrip 메서드로 추출; 그립 가중치는 항상1이므로 예전 `SupportHandWeight > .5` 분기 및 별도 보조 손 가중치 설정을 제거. FP에서는 전신 Animator IK를 사용하지 않고 LateUpdate에서 양팔 도달 범위에 맞춘 공통 그립 목표를 계산한 후 SolveArm으로 어깨-팔꿈치-손 2관절 IK 적용. 기존 오른손 방향과 몸통을 보존한 채 FirstPersonArmsView가 보정된 원본 팔을 복사한다. _rightElbow/_leftElbow는 기존 Awake 검색 결과를 캐시한다. 이전 LateUpdate의 매핑 후 즉시 반환 구조를 매핑 처리+FP 보정 구조로 교체. 기존 매핑/상체 조준/3인칭 방향 계산/손 좌표는 보존한다. 장전/근접 공격 및 잔여 액션 혼합 동안 양 시점 그립 보정을 중단한다.
- PlayerCharacterWeaponPoseTests.cs: 달리기 그립 해제를 기대하던 상태/통합 테스트를 유지 기대값1로 변경하고 통합 메서드 이름을 SprintKeepsTheGripAndRelaxedUnarmedMovementUsesTheBaseGait로 변경. 기존 맨손 팔 스윙/조준·상체/카메라 방향 검사 유지.
- PlayerCharacterHandActionTests.cs 및 .meta 신규: 고정 .02s 시간으로 실제 달리기/점프 중 양손 상대 그립 오차 <.04m, 상체/손 가중치1, 장전·근접 공격 동안 그립 해제와 복귀, 맨손 이동·점프 원본 팔, 주먹 공격, 재장착 시 레이어 복귀를 양 시점 검사. 임시 floor와 테스트 시간 설정을 정리/복원한다.

기존 컴포넌트·필드·에셋·씬 참조 삭제 없음. 기존 프레임별 그립 해제 계산과 CanAim 조건을 위 동작으로 교체했다. 장전·공격 애니메이션 키프레임 제작/편집 없음. 새 총기 종류/개별 그립 데이터는 추가하지 않았으며 현재 연결된 권총 그립 좌표를 사용한다.

## 구조 검토

CombatView는 표현 상태/레이어 우선순위, WeaponPoseState는 휴대 자세 전환, WeaponAimView는 팔 목표와 IK를 소유한다. 새 관리자/계층/인터페이스 없이 기존 책임 안에서 해결했다. UsesItemGrip은 읽기 전용 의도 계약이고 구현 상태의 외부 변경을 허용하지 않는다. IK 수학은 private 정적 함수로 분리했다. SOLID/OOP 의도적 예외 없음.

## 검증

격리 프로젝트 Unity6000.4.8f1 -batchmode -noaudio 사용. 변경 전 달리기 실제 보조 손 가중치 .1074098로 회귀 검사 실패. FP 보조 손만 목표 설정 시 도달 불가 오차 .2879m, 양손 Animator IK 시 .0748m 및 총 방향 2.64도 회귀를 확인해 해당 중간 구현은 채택하지 않았다. 최종 집중 검사 48/48 통과(after4.xml/log): 손 동작·휴대 포즈·전투 표현·장전 어깨·점프 애니메이션·이동·카메라 모션. 추가 실제 양 시점 달리기/점프 렌더 캡처 1/1 통과(capture.xml/log), FP/TP 각 달리기·점프 총4장 캡처 및 직접 확인. 전체 프로젝트 테스트/플랫폼 빌드는 이번 작업에서 실행하지 않았다. 검증 파일과 원본 반영 파일 바이트 일치 확인.

사용자 Editor의 실제 시각 품질, 손가락 접촉, 조작감과 빌드 대상 확인은 별도 필요하다. 자동 검사는 관절 목표 오차/상태/기존 회귀 범위를 검증하며 모든 미래 장비의 그립을 검증한 것은 아니다.
