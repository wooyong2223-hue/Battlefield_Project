# 임시 보병·기관포 탄두 외형

2026-10-29. 임시 모델 적용 요청. 실제 무기 구경 확정이나 탄도 변경 아님. 외부 에셋 없이16분할 탄두 메시를 생성했다.

## 이전 상태와 교체

Bullet prefab/MachineGun/Fighter prefab은 이전 검증본과 같았다. SampleScene은 Unity 저장으로 테스트장 인스턴스 문서순서와 TMP TextStyleHashCode override가 갱신돼 최신파일을 보존했다. 기존 Models 폴더/meta GUID도 보존했다.

Bullet.prefab의 MeshFilter 기본 메시(fileID10207)→InfantryBullet, MeshRenderer 재질 GUID31321ba15b8f8eb4c954353edc038b1d→TemporaryBulletCopper. 기본primitive 외형을 작은 구리색 탄두로 바꾸기 위함. 기존 기본메시/재질 에셋은 삭제하지 않았다. BulletVisual 컴포넌트만 추가. root position(613.3667,-97.62,4.54909)/rotation/scale1, Rigidbody, Bullet._speed1000/_lifeTime1/_gravityScale0/_hitMask전체를 유지한다.

## 코드와 데이터

- 새 BulletVisual.cs: RequireComponent MeshFilter. Awake에 filter와 defaultMesh 캐시, OnEnable→SetMesh(null)로 기본메시 복구, SetMesh(mesh)는 sharedMesh만 바꾸며 null은 기본Mesh. 기본외형복원수명을 이 컴포넌트가 소유해 기관포외형이 보병발사에 남지 않는다.
- MachineGun.cs: `[SerializeField] private Mesh _projectileMesh` 추가. 기존 BulletPool.Get() 다음 BulletVisual.TryGetComponent 성공 시 SetMesh(_projectileMesh). null이면기본. 기존 총구/과열/탄속/피해/ProjectilePrefab계약 유지.
- Fighter.prefab: MachineGun._projectileMesh=CannonBullet 참조 한 줄만 추가. Unity SaveAsPrefabAsset의 불필요한 오디오 기본curve 직렬화는 제외하고 최종본을 다시 검증했다. 원래AudioSource/다른컴포넌트 문서는 그대로다.
- InfantryBullet.asset: 지름.02m/길이.08m, 원점중심,+Z전방. 권총·더미 및 메시미지정 기관총 기본.
- CannonBullet.asset: 지름.04m/길이.16m, 원점중심,+Z전방. 전투기기관총 연결. 치수는 임시비교용으로 실제구경 고증 아님.
- TemporaryBulletCopper.mat: URP/Lit, BaseColor(.72,.36,.12),Metallic.75,Smoothness.5. 발광/Trail추가 없음. 메시 좌표로 크기를 표현해 루트Transform과 물리는 유지.
- 새 BulletVisualTests.cs: 실제 같은 풀 인스턴스의 기관포→기본외형 복원, 모델bounds/설정보존, 아군더미 소유자의 두외형×적/중립/아군 실제 사격, 현재씬 전투기의 CannonBullet/공통pool연결 검사3개.

외형책임은 BulletVisual, 선택데이터는 Mesh/Material, 발사자는 메시만 제공. Bullet의 피해/이동 및 BulletPool 변경 없음. 독립수명상태를 컴포넌트로 분리하고 추가 매니저/프로필/인터페이스/상속은 만들지 않았다. OOP/SOLID 의도적예외 없음.

## 검증

격리 Unity6000.4.8f1 -noaudio 집중6/6 통과: 새visual3 + 기존TrainingGround3. 실제 아군 DummyShooter 소유자에서 MachineGun으로 두메시 각각 적/중립/아군 피해10/10/0 유지. 동일풀객체 반환→재획득시 기본Mesh복구. 기본탄속1000/수명1/중력0/rootScale1유지. 기존9표적 규칙/5재질 이펙트·탄흔/점프착지검사 통과. 최종Prefab/코드는 검증본과 바이트 동일하게 적용.

확대정적렌더 comparison.png에서 형태와2배크기차이를 확인(왼쪽기관포/오른쪽보병). 실제1000m/s 비행가시성·무기별조작체감은 사용자Editor에서 Play재시작후확인 필요. 풀은 새프리팹으로 재생성해야 한다. 작은모델만 적용했으며 예광/Trail 미구현. 전체회귀/타겟빌드는 이번집중검증에 포함하지 않았다. Scene/게임오디오 설정 유지. 임시Editor 생성기/렌더도구는Library에만 존재. 로그 Library/CodexBulletVisual/final.xml/log,preview.log.

## 파일 전체 (meta 포함)

- `Assets/Features/Projectile/Prefabs/Bullet.prefab`
- `Assets/Features/Weapon/Scrips/MachineGun.cs`
- `Assets/Features/Fighter/Prefabs/Fighter.prefab`
- `Assets/Features/Projectile/Models/Temporary.meta`
- `Assets/Features/Projectile/Models/Temporary/CannonBullet.asset`
- `Assets/Features/Projectile/Models/Temporary/CannonBullet.asset.meta`
- `Assets/Features/Projectile/Models/Temporary/InfantryBullet.asset`
- `Assets/Features/Projectile/Models/Temporary/InfantryBullet.asset.meta`
- `Assets/Features/Projectile/Models/Temporary/TemporaryBulletCopper.mat`
- `Assets/Features/Projectile/Models/Temporary/TemporaryBulletCopper.mat.meta`
- `Assets/Features/Projectile/Scripts/BulletVisual.cs`
- `Assets/Features/Projectile/Scripts/BulletVisual.cs.meta`
- `Assets/Tests/PlayMode/BulletVisualTests.cs`
- `Assets/Tests/PlayMode/BulletVisualTests.cs.meta`

문서: Docs/AI/TemporaryBulletVisualChanges.md 신규, Docs/AI/UnityProjectContext.md 결과 추가.
