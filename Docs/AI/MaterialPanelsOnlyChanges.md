# 재질 더미 제거 및 판만 남기기

2026-10-29. 사용자 요청: 판만 배치하고 거리/사격 위치 표시 제거, 기존 재질 더미5개 삭제 확인.

## 작업 전 확인

SampleScene/TrainingGround/PlayMode 검사를 직전 검증본과 비교, 관련 사용자/Editor 변경 없음. 원본에 적용 전 baseline 해시로 재검사한다. 기존 팀별9표적/그 거리 표시는 이번 재질 구역 단순화 대상이 아니므로 유지한다.

## 삭제한 오브젝트와 이전 동작

- SampleScene: Dummy_Concreat, Dummy_Dirt, Dummy_Metal, Dummy_Sand, Dummy_Wood. 각각 Transform/MeshFilter/MeshRenderer/BoxCollider/Team/Health/DummyDebug를 갖는 재질 테스트 더미였다. 이5개 GameObject와 총40직렬화 문서를 삭제했다. 부모 Dummys Transform.m_Children에서 해당5개 항목만 제거. 다른 문서/참조/설정은 그대로다. 피격 효과 database/pool/material asset 자체는 삭제하지 않았다.
- TrainingGround prefab: FiringLine_Concreat/Dirt/Metal/Sand/Wood 5개. 이전 local(X,.015,-58.5),scale(6,.03,.25)의 주황색 사격 위치 표시였다. GameObject/Transform/MeshFilter/MeshRenderer/BoxCollider 삭제.
- 같은 prefab: Label_CONCRETE/DIRT/METAL/SAND/WOOD 5개. 이전 local(X,7,-48.6),TMPfontSize8, text 재질명과10m의 표지였다. GameObject/RectTransform/MeshRenderer/TextMeshPro 삭제.
- prefab에서 총45직렬화 문서 제거, 부모 MaterialImpactRange Transform.m_Children에서10항목만 제거. 새컴포넌트/자산 추가 없음.

## 남긴 구성

MaterialImpactRange 자식은 MaterialTarget_Concreat/Dirt/Metal/Sand/Wood 판5개뿐. 기존 위치 localX=-12/0/12/24/36,Y3,Z-48 및 크기6x6x1,Renderer재질/Collider.sharedMaterial 연결은 그대로 보존했다. 세계 위치/기존 테스트장 크기·캐릭터/팀별표적9개/이동시설은 유지. 사격 거리/지점을 제한하는 코드 없음. 자동검사의 발사 시작점은 재현용 메모리 설정이며 씬에 표식이나거리제한을 추가하지 않는다.

## 변경 파일과 검사 교체

- `Assets/Scenes/SampleScene.unity`
- `Assets/Features/TrainingGround/InfantryTrainingGround.prefab`
- `Assets/Tests/PlayMode/PlayerCharacterAimTests.cs`
- `Assets/Tests/PlayMode/PersonalCombatMechanicsTests.cs`
- `Assets/Tests/PlayMode/PlayerCharacterInfantryTests.cs`
- `Assets/Tests/PlayMode/PlayerCharacterMovementTests.cs`
- `Assets/Tests/PlayMode/PlayerCharacterMotionTests.cs`
- `Assets/Tests/PlayMode/CombatStructureTests.cs`
- `Assets/Tests/PlayMode/PlayerCharacterCameraTests.cs`
- `Assets/Tests/PlayMode/InfantryTrainingGroundTests.cs`

- PlayerCharacterAimTests, PersonalCombatMechanicsTests, PlayerCharacterInfantryTests, PlayerCharacterMovementTests, PlayerCharacterMotionTests, CombatStructureTests의 적/중립/아군 검사: 과거 목재 더미를 임시 아군으로 바꾸던 이름 참조 Dummy_Wood → 기존 Ally_10m. 적/중립 피해량/아군차단 assert·타이밍·임계값 그대로 유지.
- CombatStructureTests.MaterialDummyHitsSpawnParticlesAndBulletHoles → MaterialPanelsSpawnParticlesAndBulletHoles. 첫5대상을 MaterialTarget_재질로 변경, Enemy/Neutral검사는 유지. 새 판에는Health가 없으므로 표면찾기 Health.Single→Collider.Single. 실제 MachineGun 발사·입자재생·탄흔풀 증가 검사는 모두 유지.
- PlayerCharacterCameraTests.AuthoredSpawnStartsOutsideMetalAndShowsTheThirdPersonModel → AuthoredSpawnStartsOutsideMaterialPanelsAndShowsTheThirdPersonModel. 삭제할 metal dummy 대신 현재 MaterialImpactRange와의 실제 캡슐 겹침검사. 카메라거리>4.8/3인칭 외형 assert는 유지.
- InfantryTrainingGroundTests: 삭제할 Dummy_재질에서 PhysicsMaterial비교하던 코드를 Assets/Art/PhysicMaterial/{name}.physicMaterial의 명시적 에셋 비교로 교체, 구형더미가없음/판자식5/BoxCollider5 확인 추가. 같은 아군 DummyShooter 실제 탄환/충돌/효과풀/탄흔풀/위치 assert 유지. 테스트 발사Z는 root-58.5→targetZ-10.5로 계산하고씬표시와무관하게 구성.

게임 런타임 메서드/필드 수정 없음. 정적배치책임은 기존 프리팹 유지, 판/피격효과는 기존Collider/HitEffectDatabase/풀 책임 사용. 새 매니저·상속·인터페이스 불필요, OOP/SOLID 의도적예외없음.

## 문서 변경

Docs/AI/MaterialPanelsOnlyChanges.md 신규 작성(이 문서), Docs/AI/UnityProjectContext.md 현재 배치/삭제/검증기록 추가. 기존 작업 문서는 과거 기록으로 보존.

## 검증

정적 비교: scene40문서/prefab45문서 삭제, 각 부모Transform의 자식목록 변경 외 모든 남은 직렬화문서 동일. 검사 어셈블리에 TMPro 의존성을 추가하지 않고 Collider로 검사하도록 보완했다. 최초 컴파일은 TMP타입 직접검사 의존성 누락으로 실패, 제품 의존성 확장 없이 해결. 최종 Unity6000.4.8f1 전체 무음 PlayMode147/147 통과 (제품141 +격리 임시진단/캡처6), 실패/건너뜀0. 로그 Library/CodexMaterialPanelsOnly/full-final.xml/log. 기존 팀별사격/재질별입자·탄흔/조준·카메라/보병·탑승 회귀검사 유지. 최종 화면에서 기존큰더미/새거리표지/새사격선이제거되고 판5개만 남았음을 직접 확인. 모든 적용파일은 검증본과 바이트 동일. Unity 원본 Editor에서 실제 이동/발사·탄흔외형 확인 및빌드실행은 하지 않았다.
