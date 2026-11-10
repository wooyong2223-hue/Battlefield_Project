# 보병 이동·사격 테스트장 추가

2026-10-29. 캐릭터 모델과 동일한 Unity Starter Assets 로컬 캐시의 Environment를 사용했다. 외부 다운로드·패키지 설치·새 컨트롤러 가져오기 없음. 기존 Unity Companion License 유지.

## 작업 전 비교와 보존

직전 점프 카메라 작업의 baseline/적용 목록과 비교했다. SampleScene의 활주로 표시 구조물 9개 Y=-95.777→-98 변경을 보존했다. PlayerCharacter 루트 위치 (79.5,-97,-544.1), 모든 기존 직렬화 문서/참조/계층·설정·Build Settings를 유지했다. SampleScene은 Unity 전체 재저장 대신 새 PrefabInstance 문서와 SceneRoots 한 항목만 추가했다.

## 구조와 값

- `Assets/Scenes/SampleScene.unity`: InfantryTrainingGround 프리팹 인스턴스 한 개 추가, position=(145,-99,-500), rotation=identity, scale=1. 기존 문서 제거/교체 없음.
- `Assets/Features/TrainingGround/InfantryTrainingGround.prefab`: 정적 테스트 공간. 새 런타임 MonoBehaviour 없음. 원본 nested prefab과 Collider, 기존 Health/Team/DummyDebug 조합. 책임이 레벨 배치이므로 별도 런타임 관리자/인터페이스/상속 계층을 추가하지 않았다. OOP/SOLID 의도적 예외 없음.
- 바닥 120×120m, 윗면 Y=-99. 진입로 local(-64,-.5,-44), size(8,1,8): 현재 플레이어 출발점에서 +X 방향으로 연결.
- 원본 구조물 9종: 낮은 계단·높은 계단·경사로·상자 3종·터널·벽·구조물. 배율 2.270328: 기존 4m 캐릭터/Armature 비율에 맞춤. 원본 프리팹/모델 값은 보존하고 새 인스턴스에만 배율/콘크리트 Collider 재질 적용.
- 점프 발판 높이 .30/.55/.80/1.05m, 폭5m/깊이4m. 캐릭터 점프 높이1.2m 설정은 유지.
- 웅크리기 통로: 바닥→천장 아랫면2.7m, 폭9.5m, 깊이8m. 서있는4m 캐릭터가 통과할 수 없는 구간. 별도 신규 크롤링 기능 아님.
- 사격선 local Z=-40. 표적은 적/중립/아군 각10/25/50m, 총9개. 가까운 표적이 먼 표적을 가리지 않도록 X=-3/0/+3m 엇갈림. 거리는 사격선에서 Z 방향 거리이며 표적 중앙 기준.
- 새 표적은 기존 Dummy_Enemy/Dummy_Neutral을 복제. Health/Collider/PhysicsMaterial/피격 이벤트는 기존 연결을 재사용. Ally 복제본의 Team._team만 Player(1)로 설정. 기존 더미는 변경하지 않았다. 적·중립은 기존10damage, 아군은 기존 팀 규칙으로 피해 차단.
- 사격장 후면 벽 size(48,16,2), 옆벽 size(1,8,74). 기존 Concreat PhysicsMaterial을 새 구조물에 연결해 기존 피격 효과/탄흔 선택 가능. 자동 표적 재생성 기능 없음: Play를 다시 시작해 초기화.
- 전용 URP Lit 재질5개: TrainingConcrete RGB(.3,.34,.38), TrainingMarkings(1,.5,.08), TargetEnemy(.8,.08,.06), TargetNeutral(.9,.7,.08), TargetAlly(.05,.35,.9), smoothness=.1. 원본 재질 변경 없음.
- 안내: MOVEMENT / JUMP, FIRING RANGE, 팀/거리, 발판 높이, CROUCH2.7m TextMeshPro3D.
- `Assets/Tests/PlayMode/InfantryTrainingGroundTests.cs`: 실제 아군 DummyShooter.Fire로9개 표적 피해/팀 차단 검사; 실제 CharacterController로 새 바닥의 접지·1.2m 점프·착지 검사. 테스트의 텔레포트/입력 차단은 PlayMode 메모리에만 적용.

## 실제 적용 경로와 외부 파일

초기에 격리 프로젝트에서 기존1.1.5 캐시의 Environment를 선택 임포트했지만, 도중 사용자가 최신 패키지를 Assets/Starter Assets에 가져온 것을 확인했다. 같은 GUID가 있어 원본에는 중복 복사하지 않고 현재 `Assets/Starter Assets/Sample/Environment`의 기존 지형을 그대로 재사용했다. 최신 환경94파일을 격리 프로젝트에 동기화해 충돌·메시·머티리얼을 재확인했다. 이 환경94파일은 사용자의 임포트이며 이번 작업의 수정/추가 파일이 아니다. 원본의 RenderPipelineProfiles/프로젝트 렌더 설정도 건드리지 않았다.

새 TextMesh 표지는 초기 시각 검사에서 벽을 뚫고 보여, 최종 적용 전에 TextMeshPro3D로 교체. 기존 LiberationSans SDF 재사용, fontSize 정상8/제목14/발판6.5, rect45x5, 그림자 끔. 기존 사용자 컴포넌트 제거 없음.

## 변경/추가 파일 전체

기존 변경: Assets/Scenes/SampleScene.unity (새 인스턴스/SceneRoots 한 항목), Assets/ThirdParty/UnityStarterAssets/SOURCE.md (초기 임포트 설명을 과거형으로 변경, 최신 이벤트·Environment 사용 경로 반영), Docs/AI/UnityProjectContext.md (작업 결과 추가). 새 문서: Docs/AI/InfantryTrainingGroundChanges.md.

새 스크립트/에셋/메타 파일:

- `Assets/Features/TrainingGround/InfantryTrainingGround.prefab`
- `Assets/Features/TrainingGround/InfantryTrainingGround.prefab.meta`
- `Assets/Features/TrainingGround/TargetAlly.mat`
- `Assets/Features/TrainingGround/TargetAlly.mat.meta`
- `Assets/Features/TrainingGround/TargetEnemy.mat`
- `Assets/Features/TrainingGround/TargetEnemy.mat.meta`
- `Assets/Features/TrainingGround/TargetNeutral.mat`
- `Assets/Features/TrainingGround/TargetNeutral.mat.meta`
- `Assets/Features/TrainingGround/TrainingConcrete.mat`
- `Assets/Features/TrainingGround/TrainingConcrete.mat.meta`
- `Assets/Features/TrainingGround/TrainingMarkings.mat`
- `Assets/Features/TrainingGround/TrainingMarkings.mat.meta`
- `Assets/Features/TrainingGround.meta`
- `Assets/Tests/PlayMode/InfantryTrainingGroundTests.cs`
- `Assets/Tests/PlayMode/InfantryTrainingGroundTests.cs.meta`

## 검증과 남은 확인

격리 Unity6000.4.8f1에서 전체 PlayMode146/146 통과 (제품140 + 이전 임시 진단/캡처6). 이 전체 검사는 도중 사용자 재임포트와 최종 표지 보완 전 결과다. 이후 최신 Armature21파일과 현재 경로 Environment94파일을 동기화하고 새 표지를 TextMeshPro로 바꾼 최종 상태에서 신규 실제 사격/점프2개 + 임시 PlayMode 화면 캡처1개 = 3/3 통과. 새 파일과 SampleScene은 최종 검증본과 바이트 동일하게 적용. 전체/입구 화면을 직접 확인했다. 임시 생성·검증·캡처 스크립트는 Library 격리에만 있고 원본 Assets에 추가하지 않았다.

사용자 Editor에서 실제 코스 이동·계단·점프·웅크리기·엄폐 조작감, 사격 이펙트/탄흔 가독성 확인은 남아 있다. 자동 검사는 바닥 접지·점프·착지와 아군 더미 소유자의9개 표적 피해/아군차단을 확인했고 모든 지형을 자동 완주한 것은 아니다. 대상 플랫폼 빌드 없음. 표적 자동 리셋 기능은 추가하지 않았으며 Play 재시작 시 초기화된다.

PlayMode 캡처에서만 SRP배칭을 잠시 껐다 복구: batch Editor의 최초 즉시 Render에서 잘못된 색상이 나타나 실제 PlayMode 색상/깊이 가림을 확인하기 위함. ProjectSettings는 변경하지 않았다.

## 보존한 도중 사용자/Editor 변경

기존 Armature 애니메이션·재질·모델·텍스처21파일: AssetOrigin2.0.2, legacy Walk/Run의 OnFootstep, Jump의 OnLand 복원, 재질 URP float추가. 이 작업에서 수정하지 않았다. 개별 경로:

- `Assets/ThirdParty/UnityStarterAssets/Animations/Jump--InAir.anim.fbx.meta`
- `Assets/ThirdParty/UnityStarterAssets/Animations/Jump--Jump.anim.fbx.meta`
- `Assets/ThirdParty/UnityStarterAssets/Animations/Locomotion--Run_N.anim.fbx.meta`
- `Assets/ThirdParty/UnityStarterAssets/Animations/Locomotion--Walk_N.anim.fbx.meta`
- `Assets/ThirdParty/UnityStarterAssets/Animations/Stand--Idle.anim.fbx.meta`
- `Assets/ThirdParty/UnityStarterAssets/Materials/M_Armature_Arms.mat`
- `Assets/ThirdParty/UnityStarterAssets/Materials/M_Armature_Arms.mat.meta`
- `Assets/ThirdParty/UnityStarterAssets/Materials/M_Armature_Body.mat`
- `Assets/ThirdParty/UnityStarterAssets/Materials/M_Armature_Body.mat.meta`
- `Assets/ThirdParty/UnityStarterAssets/Materials/M_Armature_Legs.mat`
- `Assets/ThirdParty/UnityStarterAssets/Materials/M_Armature_Legs.mat.meta`
- `Assets/ThirdParty/UnityStarterAssets/Models/Armature.fbx.meta`
- `Assets/ThirdParty/UnityStarterAssets/Textures/Armature_Arms_AlbedoTransparency.tif.meta`
- `Assets/ThirdParty/UnityStarterAssets/Textures/Armature_Arms_MetallicSmoothness.tif.meta`
- `Assets/ThirdParty/UnityStarterAssets/Textures/Armature_Arms_Normal.tif.meta`
- `Assets/ThirdParty/UnityStarterAssets/Textures/Armature_Body_AlbedoTransparency.tif.meta`
- `Assets/ThirdParty/UnityStarterAssets/Textures/Armature_Body_MetallicSmoothness.tif.meta`
- `Assets/ThirdParty/UnityStarterAssets/Textures/Armature_Body_Normal.tif.meta`
- `Assets/ThirdParty/UnityStarterAssets/Textures/Armature_Legs_AlbedoTransparency.tif.meta`
- `Assets/ThirdParty/UnityStarterAssets/Textures/Armature_Legs_MetallicSmoothness.tif.meta`
- `Assets/ThirdParty/UnityStarterAssets/Textures/Armature_Legs_Normal.tif.meta`

기존 Environment 재사용 파일은 `Library/CodexTrainingGround/user-environment-baseline.json`에94개 경로/해시 기록. 기존 씬 문서/새파일/사용자 보존 해시 검사는 `Library/CodexTrainingGround/final-audit.json`.

## 폴더 통합 후 현재 경로

테스트장 에셋은 Features/TrainingGround로 이동했다. 프리팹/재질/메타 바이트와 GUID는 동일하며 SampleScene은 재저장하지 않았다. 자동 테스트 코드는 Tests/PlayMode에 유지한다. 비교용 CharacterUpperBodyAnimationTest 씬은 삭제했다. 일반 자동 검사/캡처는 -noaudio 실행, 사운드 검사만 예외.
