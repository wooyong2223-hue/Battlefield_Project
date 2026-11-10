# 탄환 예광 궤적

2026-10-29. 사용자 요청: 임시 탄두에 예광 효과 추가. 기존 보병/기관포 모델·물리 수치를 보존하고 시각효과만 추가한다.

## 작업 전 비교와 구조

직전 작업의 BulletVisual/투사체/프리팹/테스트/Scene/Fighter 파일은 격리 검증본과 바이트 동일했다. 사용자 변경 없음. 원래는 위치갱신/충돌 처리 후 시각효과에 좌표를 전달하지 않았다. 이를 삭제·교체하지 않고 ProjectileBase에 기본no-op OnAdvanced(Vector3) 확장지점과 이동/실충돌 좌표 전달만 추가했다. 선택적인 표현 확장으로 이동/팀·피해 순서는 그대로다.

지속시간이 탄환보다 조금 긴 효과이므로 별도 BulletTracer 컴포넌트가 renderer/상태/잔상종료를 소유한다. Bullet은 시작/위치/종료만 전달, BulletVisual은 현재메시의 diameter만 공개. 매니저/전역pool/추가프로필 없음. 기존 상속의 실제 variation point에만 virtual hook을 사용하고 미사일 등은 default no-op. OOP/SOLID 의도적예외 없음.

## 변경 코드

- ProjectileBase.cs: `protected virtual void OnAdvanced(Vector3 position) { }` 추가. 기존 Rigidbody.MovePosition(previous+displacement) 다음 같은 목표위치를 전달. HandleHit는 ShouldIgnore 통과 후 hit.point 전달, 그 뒤 기존 아군차단/표면효과/피해/풀반환 순서를 유지. Raycast·속도·rigidbody위치를 예광 때문에 바꾸지 않는다.
- Bullet.cs: private serialized BulletTracer _tracer와 캐시BulletVisual _visual. Awake에서 base.Awake 후visual캐시. 기존 Initialize 이후 tracer.Begin(발사원점, 현재 탄두지름). OnAdvanced override에서Advance. OnDisable에서Stop. OnDestroy에서부모에서분리된소유tracer GameObject만 Destroy해scene/pool정리. 기존 _damage/OnHit/Initialize의 기존 코드 유지.
- BulletVisual.cs: read-only Diameter = 현재sharedMesh.bounds.size.x × abs(lossyScale.x). 기존 메시선택/OnEnable 기본메시복원 그대로.
- 새 BulletTracer.cs: RequireComponent LineRenderer, private SerializeField+Min _length4m/_fadeDuration.04s/_widthMultiplier6. Begin에서소유child를부모분리,활성화,기존두끝점을모두새원점으로재설정,현재지름으로굵기재설정. Advance는권한있는현재위치로끝점을갱신하며길이최대4m/발사원점뒤로가지않도록제한. Stop은중복안전하게fade시작. Update는width를.04초동안0으로줄이고positionCount0,GameObject비활성화. 모든탄환에표시(격발N회비율선택미구현).

## 프리팹과 재질

Bullet.prefab: 기존 root.m_Children []→Tracer child1개, Bullet._tracer 참조 추가. child는 Transform+LineRenderer+BulletTracer. LineRenderer는world space,기본위치0개,startWidth1/endWidth0의taper,cap2,view alignment,shadow/receiveShadowOff. 기존 루트/메시/재질/rigidbody/탄속1000/수명1/중력0/히트마스크 유지.

BulletTracer.mat: URP/Unlit, BaseColor HDR(4,1.8,.3,1). 밝은황금색선,기본깊이검사로벽뒤가림. bloom/렌더파이프라인설정변경없음. 보병지름.02→선굵기.12m,기관포.04→.24m,길이는둘다4m. 미세먼지/연기/별도발광후처리추가없음.

탄환을활성화할때마다새효과오브젝트를생성하지않는다. 프리팹인스턴스당자식렌더러하나가있고,발사시분리된자식은소유자가참조해재사용한다. 풀반환뒤.04초잔상이종료돼도소유권은유지. 매우빠른같은탄환재사용은이전잔상을새발사로교체해개수증가를막는다. 탄환소유자가삭제되면분리된오브젝트도정리한다.

## 검사

새 BulletTracerTests.cs3개: 실제아군더미→적15m사격에서실충돌점일치(.03m)/4m길이한도/기존10피해,탄환풀반환뒤잔상유지후종료; 재시작시과거두끝점삭제·두지름에따른.12/.24폭; 탄환삭제시분리된tracer정리. 기존 BulletVisual/TrainingGround 검사도 유지해풀복원·기관포연결·두외형팀규칙·재질이펙트/탄흔·착지를확인.

집중 -noaudio9/9 통과 후 전체 PlayMode154/154 통과(제품147+격리임시진단/캡처7). 실제 캡처에서 처음 .03/.06m 선이 서브픽셀로 안 보이는 문제가 있어 새 예광의 시각 배율만1.5→6으로 보완했다. 탄두·물리·충돌 수치는 유지. 보완한 최종 상태에서 집중10/10 재통과(제품9+실제발사캡처1). 60fps90프레임에서 실제 아군더미 소유자3회 사격/3회 적명중, 활성 궤적21샘플 확인. 최종 RGB픽셀에서도 보병/기관포 노란선이 검출되고 렌더프레임을 직접 보았다. 전체154 검사는 두께보완 전, 최종10 검사는 보완 후 결과다. 검증본과 적용파일 바이트 동일. 영상 Library/CodexBulletTracer/tracer-test.mp4, 로그 full.xml/log 및 final.xml/log. 원본Scene/Fighter프리팹/무기설정과 원래 Models/Materials 폴더메타는 그대로 보존했다. 원본Editor UI/타겟빌드는검사하지않았다. 사용자실제시점에서밝기/굵기/보이는시간체감은추가확인필요.

## 파일 전체 (meta 포함)

- `Assets/Features/Projectile/Scripts/Bullet.cs`
- `Assets/Features/Projectile/Scripts/ProjectileBase.cs`
- `Assets/Features/Projectile/Scripts/BulletVisual.cs`
- `Assets/Features/Projectile/Prefabs/Bullet.prefab`
- `Assets/Features/Projectile/Scripts/BulletTracer.cs`
- `Assets/Features/Projectile/Scripts/BulletTracer.cs.meta`
- `Assets/Features/Projectile/Materials/BulletTracer.mat`
- `Assets/Features/Projectile/Materials/BulletTracer.mat.meta`
- `Assets/Tests/PlayMode/BulletTracerTests.cs`
- `Assets/Tests/PlayMode/BulletTracerTests.cs.meta`

문서: Docs/AI/BulletTracerChanges.md 신규, Docs/AI/UnityProjectContext.md 결과 추가. 기존 컴포넌트/메서드/필드/에셋 삭제 없음. 임시 생성/캡처 도구는 Library에만 존재.
