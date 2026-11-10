# 테스트장 재질별 피격 비교 구역

2026-10-29. 기존 콘크리트/흙/금속/모래/나무 재질 더미와 HitEffectDatabase 등록을 재사용한다. 직전 테스트 정리 이후 SampleScene/TrainingGround prefab/자동검사 코드의 변경이 없음을 이전 검증본과 비교했다.

## 수정 파일과 값

1. `Assets/Features/TrainingGround/InfantryTrainingGround.prefab`: 루트에 `MaterialImpactRange` 자식 추가. 기존 직렬화 문서에서 바뀐 것은 루트 Transform.m_Children의 새 자식 참조 한 항목뿐이다. 이전 문서/컴포넌트 제거·교체 없음. 새 직렬화 문서72개 추가.
2. `Assets/Tests/PlayMode/InfantryTrainingGroundTests.cs`: VFX namespace import와 `AlliedDummyHitsEachMaterialAndSpawnsItsEffectAndBulletHole` UnityTest 추가. 기존 두 검사 메서드·값·판정 변경 없음.
3. `Docs/AI/UnityProjectContext.md`: 이번 배치/검증 기록 추가.
4. `Docs/AI/MaterialImpactRangeChanges.md`: 이 변경 보고서 신규 작성.

## 배치

| 패널 | PhysicsMaterial 및 외형 참조 | local X | local 위치/크기 |
|---|---|---:|---|
| MaterialTarget_Concreat | 기존 Dummy_Concreat Collider/Renderer | -12 | (X,3,-48), scale(6,6,1) |
| MaterialTarget_Dirt | 기존 Dummy_Dirt Collider/Renderer | 0 | 동일 |
| MaterialTarget_Metal | 기존 Dummy_Metal Collider/Renderer | 12 | 동일 |
| MaterialTarget_Sand | 기존 Dummy_Sand Collider/Renderer | 24 | 동일 |
| MaterialTarget_Wood | 기존 Dummy_Wood Collider/Renderer | 36 | 동일 |

테스트장 root world(145,-99,-500)은 유지. 패널 중심 world X=133/145/157/169/181, Y=-96,Z=-548. 패널 앞면Z=-548.5. 각 주황색 사격선 local(X,.015,-58.5), size(6,.03,.25), 앞면까지 +Z로10m. 기존 TrainingMarkings 재질 재사용. 레이블 local(X,7,-48.6), CONCRETE/DIRT/METAL/SAND/WOOD와10m, 기존 TextMeshPro3D/LiberationSans SDF, fontSize8,rect15x4, 그림자Off.

첫 격리 실험 localX=-44/-25/-6/25/44는 기존 모래 더미에 사선이 막혔다. 기존 사용자 더미를 옮기지 않고 새 패널·레이블·사격선만 최종 X로 조정했다. 최초 시도는 원본에 적용하지 않았다.

패널에는 MeshRenderer/BoxCollider만 있고 Health·Team·새 런타임 스크립트가 없다. 기존 표적9개 Health 카운트와 적/중립/아군 피해 검사를 유지한다. 목적이 재질별 표면 효과 비교이므로 별도 관리 클래스/이벤트/상속/인터페이스는 추가하지 않았다. 기존 탄환→Collider.sharedMaterial→HitEffectDatabase→효과풀/탄흔풀 흐름을 그대로 쓴다. OOP/SOLID 의도적 예외 없음.

## 보존

SampleScene 파일/캐릭터/이동 구역/기존 표적·더미·효과 데이터베이스·풀·재질·피격/데미지 런타임 코드는 수정하지 않았다. 피격 효과/탄흔의 기존 크기와 유지 시간도 유지했다. 새 씬·새재질·새 런타임 컴포넌트 없음. GUID/fileID 보존.

## 검증

격리 Unity6000.4.8f1 `-batchmode -noaudio`: 최초4개 중1실패로 기존 모래 더미 사선 방해를 재현. 배치 조정 후 최종4/4 통과 (제품 검사3 + 임시 화면 캡처1). 실제 아군 DummyShooter.Fire에서 기존 BulletPool로 탄환 발사, 각 패널에 대한 ray 첫 충돌·원래PhysicsMaterial 동일성·해당 database entry의 EffectPool/BulletHolePool 활성 객체가 각1개 증가·탄흔이 충돌 위치0.3m 이내인지 검사했다. 기존9표적 적/중립 피해·아군차단 및 바닥 접지/점프/착지 검사도 통과. 새로운 판정/차량 통합 없음.

PlayMode 전체 배치와 정면 화면을 직접 확인했다. 모든 기존 prefab 문서는 root child 추가 외 바이트 동일. 테스트본과 적용한 prefab/test코드 바이트 동일. 임시 Editor 생성기 `Library/CodexInfantryWork/Project/Assets/Editor/AddMaterialImpactRange.cs`는 원본에 추가하지 않았다. 검증 결과 `Library/CodexMaterialRange/final.xml/log`. 전체 회귀/타겟 빌드는 이번 정적 배치 변경에 재실행하지 않았다.

사용자 확인: SampleScene에서 테스트장 앞쪽 새5종 패널 앞의 주황선에 서서 해당 패널로 사격. 이펙트/탄흔의 실제 모양·가독성·수명 체감은 사용자 Editor에서 확인 필요. 일반검증은무음이며 이 작업은 사운드를 검증하지 않는다.
