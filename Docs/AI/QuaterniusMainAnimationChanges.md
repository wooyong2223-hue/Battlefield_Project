# Quaternius UAL 1·2 메인 적용 및 기존 애니메이션 교체

2026-10-28. 사용자가 Quaternius UAL 1·2를 메인으로 선택하고 같은 동작이 있으면 기존 모션도 교체하도록 지정했다. 보유한 CC0 무료 Standard의 실제 클립으로 적용했다. Kevin 도입 실험은 제품에 반영하지 않았고 유료판 구매도 하지 않았다.

## 교체 결과

| 기존 참조 | 현재 참조 |
|---|---|
| Starter Assets 대기 | Quaternius LocomotionIdle (Idle_Loop) |
| Starter Assets 정면 Walk_N / Run_N | LocomotionWalk (Walk_Loop) / LocomotionSprint (Sprint_Loop) |
| Starter Assets Jump / InAir / Land | JumpStart / JumpAir / JumpLand |
| Rokoko BoxingGuard | Quaternius Punch_Jab 시작 자세에서 파생한 FistsGuard |
| Rokoko BoxingJab / BoxingCross | 기존 Quaternius PunchJab / PunchCross |
| Rokoko 기반 PistolMelee | Quaternius PunchCross 재사용 |
| 전용 표현 없이 진행하던 슬라이드/통과 | Slide_Start/Loop/Exit 및 ClimbUp_1m |

현재 Controller의 외부 클립 참조는 Quaternius 22개 + KayKit 3개다. 별도 착석 클립 SeatedIdle도 Quaternius다. 기존 권총 조준/발사/장전, 앉기, 사망, 소생, 착석은 이미 Quaternius이므로 그대로 유지했다. 무료 Standard에 없는 좌우·후진은 KayKit StrafeLeft/Right/WalkBackward를 유지한다. 기존 Starter Assets 모델/Avatar/메시와 기존 애니메이션 파일은 삭제하지 않았다. Rokoko 클립도 파일을 보관하되 Controller 참조는 제거했다.

## 모든 변경 파일

개별 파일의 전체 경로·해시·크기는 QuaterniusMainAppliedFiles.csv에 기록한다. 아래는 변경 구간과 목적이다.

| 스크립트/에셋 | 실제 변경 |
|---|---|
| Assets/Features/PlayerCharacter/Animation/PlayerCharacter.controller | 위 6개 기본 모션과 근접 4개 상태의 motion 참조 교체. Land speed 1→2로 원래 착지 복귀 시간 유지. PunchJab speed 1→1.0833334, PunchCross/PistolMelee 1→1.25로 0.8초 공격 표현 유지. TraversalMode(int)/TraversalTime(float), SlideStart/Loop/Exit/Vault 상태 및 진입·복귀 전환 추가. 기존 오브젝트 ID/방향별 보조 클립/다른 상태 보존 |
| Assets/Features/PlayerCharacter/Scripts/PlayerCharacterAnimationView.cs | UpdateTraversal() 추가. 기존 Traversal의 IsSliding/IsVaulting/Progress를 읽어 2개 Animator 파라미터 전달. OnDisable에서 두 값 초기화. 기존 이동 속도/방향/생존·소생 파라미터 유지 |
| Assets/Features/PlayerCharacter/Scripts/FirstPersonArmsView.cs | 근접 복사 기준을 상체 Euler X 기반 역회전에서 characterRoot.rotation 역회전으로 교체. Quaternius 몸통 회전을 보존하며 카메라 aim이 시점 피치를 더함. 권총 보정/메시/모든 위치 설정값 유지 |
| Assets/Features/PlayerCharacter/Scripts/PlayerCharacterPostureView.cs | Awake 뼈/Animator/접지 샘플러 캐시, LateUpdate 슬라이드 접지 추가. IK 이후 평가된 접점과 캡슐 바닥 차이만 Hips.position에 반영. 캐릭터/모델 Transform과 기존 포복/사망/소생 설정은 유지. _traversal/_skin/_slideContactData 참조 추가 |
| Assets/Features/PlayerCharacter/Scripts/SkinnedPoseContactData.cs 및 .meta (신규) | Editor에서 스킨 가중치/바인드 포즈로 접점 후보 추출. 런타임 샘플러는 뼈 행렬로 높이만 계산. 원본 메시 Read/Write가 없어도 사용 가능. 런타임 BakeMesh/전체 메시 스키닝/프레임별 할당 없음 |
| Assets/Features/PlayerCharacter/Animation/PlayerSlideContact.asset 및 .meta (신규) | 현재 모델 메시 29,936개 정점에서 1,219개 접점 후보, 뼈 68개를 준비한 데이터. 애니메이션이 아니라 현재 모델에서 추출한 프로젝트용 접지 데이터 |
| Assets/Scenes/SampleScene.unity | PostureView에 _traversal:950000025, _skin:1601236111, _slideContactData:11400000/guid25ac8868e1552554b8494a65e05a3568 세 참조만 추가. 기존 값/계층/카메라/좌석/스케일 보존 |
| Assets/Tests/PlayMode/PlayerCharacterAnimationTests.cs | 정면 걷기/달리기 클립 기대 이름만 LocomotionWalk/Sprint로 갱신. 점프·낙하·착지·root motion 검사는 유지 |
| Assets/Tests/PlayMode/PlayerCharacterDirectionalAnimationTests.cs | 정면/대기 기대 클립 이름을 새 Quaternius 이름으로 갱신. 기존 8방향·정지 방향 유지·회전 검사 유지 |
| Assets/Tests/PlayMode/PlayerCharacterTraversalAnimationTests.cs 및 .meta (신규) | 실제 슬라이드 3단계·취소/복구·모터 진행률과 통과·최종 스킨 메시 접지 검사 4개. 테스트 전용 LateUpdate 관찰자가 실제 BakeMesh 결과를 읽음 |

### 새 애니메이션 11개와 각각의 .meta

모두 Assets/ThirdParty/Quaternius/Animations/ 아래에 있다.

- LocomotionIdle.anim, LocomotionWalk.anim, LocomotionSprint.anim
- JumpStart.anim, JumpAir.anim, JumpLand.anim
- Slide_Start.anim, Slide_Loop.anim, Slide_Exit.anim, ClimbUp_1m.anim
- FistsGuard.anim

기본 6개는 UAL1_Standard.fbx에서, 특수 이동 4개는 UAL2_Standard.fbx에서 추출했다. 기존과 같은 Humanoid RootQ/RootT Y축 180도 방향 보정을 적용했다. 원본/수정본의 출처와 원본 CC0 허가문은 Assets/ThirdParty/Quaternius/SOURCE.md 및 License.txt를 참조한다.

FistsGuard는 전용 원본 대기 모션이 아니라 PunchJab 시작 프레임을 0.4초 고정한 파생 자세다. 왼팔/손 근육 값을 오른쪽에 대칭 복사하고 오른팔 Down-Up=-0.09470423, Front-Back=-0.5211526으로 맞췄다. 몸통/왼팔/손가락은 원본 시작 자세를 사용한다. 기존 1인칭 _unarmedOffset=(0,-0.7,0.25)는 유지했다. 총 근접 공격 역시 전용 총몸 타격 원본이 아닌 PunchCross 재사용이다.

## 중요한 전후 코드와 동작

```csharp
// 이전: 근접 표시에서 상체 Euler를 이용한 보정
Quaternion.Euler(Mathf.DeltaAngle(0f, _poseAnchor.eulerAngles.x), 0f, 0f)
    * Quaternion.Inverse(_poseAnchor.rotation)
// 이후: 캐릭터 기준으로 외부 모션을 보존
Quaternion.Inverse(_characterRoot.rotation)
```

슬라이드는 원래 Speed=0 처리만 있었다. 이제 진행률 0–0.25를 진입, 0.25–0.8을 유지, 0.8–1을 종료로 나누고 각 구간을 TraversalTime 0–1에 매핑한다. 통과는 전체 Progress를 그대로 사용한다. 진입 blend 0.04초, 복귀 blend 0.08초. 종료 시 Grounded/Crouched에 따라 Locomotion/CrouchLocomotion/InAir로 돌아간다. 기존 슬라이드 0.75초·통과 0.65초·최고 속도 8/12는 바꾸지 않았다. 실제 이동/충돌/입력/피해/공격 판정은 기존 코드가 계속 소유한다.

교체한 기존 구현은 위 6개 Starter 클립 참조, 4개 Rokoko 근접 상태 참조, 1인칭 근접 basis 계산과 PostureView Awake의 단일 Spine 캐시 구문이다. 기존 메서드/필드/컴포넌트/에셋 삭제는 없다. 미사용 원본 클립 파일은 보존했다.

## 검증

- 격리 Unity 6000.4.8f1 전체 125/125 통과: 기존 119개 + 새 제품 검사 4개 + 임시 렌더 검사 2개. 제품 검사만 세면 123개다.
- Library/CodexQuaterniusMain/final.xml 및 final.log. 집중 검사 targeted-v5.xml 9/9 통과. 초기 실패 로그는 원인 기록으로 보존한다.
- 주먹의 전후/상하 조준·양손 대기·타격 시점·1/3인칭과 기존 권총 복구 검사를 유지하고 통과했다. 통과시키기 위해 기존 판정 기준을 낮추지 않았다.
- 슬라이드가 약 3.3m 뜨는 현상을 렌더/실제 스킨 메시로 재현했고, IK를 꺼도 재현되어 메시 접점 기반 후처리로 해결했다. 집중 검사 37개 최종 포즈의 최저 정점은 캡슐 바닥 대비 약 0.002–0.025m였다. 테스트의 실제 메시 BakeMesh는 검증에만 사용하며 제품 런타임에서는 하지 않는다.
- 기존 점프 후 복귀 실패는 새 착지 클립이 1.266667초로 기존 0.6333333초보다 길기 때문이었다. Land 재생 배율 2로 기존 복귀 시간을 유지했다.
- 이동·좌우/후진·점프·슬라이드·통과·1인칭·양손 공격 렌더를 확인했다. 모델 크기/루트 위치를 바꾸지 않았다.
- 구조: Animator 상태 전달, 1인칭 표현, 자세 보정, 메시 기반 접점 데이터/인스턴스별 샘플러의 책임을 구분했다. 새 인터페이스·상속 계층·전역 관리자는 추가하지 않았다. OOP/SOLID의 의도적 예외는 없다.

실제 Editor의 조작감·벽/경사면 접촉·무기와 장애물의 손 접점·1인칭 화면 가장자리 표현은 추가 시각 조정 대상이다. 일반 전신 클립을 사용하며 전용 FPS 애니메이션 세트나 배필6의 모든 모션을 완료한 것은 아니다. Player 빌드는 이번에 실행하지 않았다. 사용자의 Editor UI 조작, 커밋/푸시 없음.

## 문서/목록 갱신

- Assets/ThirdParty/Quaternius/SOURCE.md: 새 클립과 파생/방향 보정 기록 추가.
- Assets/ThirdParty/Rokoko/SOURCE.md: 과거 출처는 보존하고 현재 미참조 상태 표시.
- Docs/AI/PlayerCharacterMocapMeleeChanges.md: 이전 Rokoko 적용 이력임을 표시.
- Docs/AI/UnityProjectContext.md 및 PlayerCharacterFeatureRoadmap.md: 최신 적용 범위/검증 추가.
- Docs/AI/Battlefield6AnimationAssetCatalog.md 및 RedistributableAnimationCandidates.md: 메인 선정과 적용 기록 링크 추가.
- Docs/AI/Battlefield6AnimationClips.csv: 새 제품 클립과 실제 적용/유지 상태 갱신. Kevin 제외 상태 유지.
- Docs/AI/QuaterniusMainAnimationChanges.md: 이 문서 신규.
- Docs/AI/QuaterniusMainAppliedFiles.csv: 이번 모든 변경 파일/해시 목록 신규.

원본 다운로드 위치: Library/CodexInfantryWork/UniversalAnimationLibrary-Standard.zip, Library/CodexInfantryDetail/UniversalAnimationLibrary2-Standard.zip. LocalAssets/AnimationReview/QuaterniusCC0는 검토용 추출 사본이며, 실제 게임 연결본은 Assets/ThirdParty/Quaternius/Animations다.
