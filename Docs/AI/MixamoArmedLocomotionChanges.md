**Mixamo 권총·소총 기본 이동 적용 — 2026-11-02**

이번 범위는 선정한 소총9·권총7 원본을 사용한 서있는 무장 대기/걷기/달리기/좌우 및 대각 혼합이다. 추가 원본이 필요하면 중단한다는 사용자 지시는 유지하며, 이번 범위는 기존 보유16개로 진행했다. 장전/발사/근접/피격/수류탄/무릎 앉기/점프의 새 원본은 이번에 교체하지 않는다.

**적용 전 비교와 보존**

최근 NativeMotionMigration 적용16파일 해시와 신규 MixamoCatalog 원본81개 해시를 비교했고 이후 사용자/Editor 변경이 없었다. 격리 프로젝트의 관련 기존 Assets도 본 프로젝트와 동일했다. 저장된 SampleScene, 캐릭터/기체 크기·계층·좌석, 이동8/달리기12/포복3, 카메라와 탄약·피해·발사 속도·그립·장전 값은 변경하지 않았다. 원본81개는 검증 전후 바이트 동일하다.

**교체 전후와 책임**

기존 CombatView는 장비 definition.Animations 또는 _baseController를 Animator에 바로 지정했다. 이후에는 무장이고 선택 프로필이 있을 때만 기존 장비 컨트롤러를 키로 이동 템플릿을 찾고, 임시 AnimatorOverrideController에 기존 장비 클립 오버라이드를 합성한다. 데이터/임시 컨트롤러 수명/씬 제공/원본 임포트 책임을 별도 객체로 분리했다. 기존 CombatView의 모델·이벤트·재생 역할 안에서는 선택/해제만 연결한다.

```csharp
// 이전
var controller = definition != null && definition.Animations != null
    ? definition.Animations : _baseController;
// 추가된 선택 단계
if (_combat.IsArmed && _locomotionAnimations != null)
    controller = _equipmentControllers.Resolve(_locomotionAnimations.Find(controller), controller);
```

ArmedLocomotionAnimationSet에는2개 매핑이 있다. 기본 컨트롤러→권총 이동, 기존 AssaultRifle 오버라이드→소총 이동. AK-47/M4A1은 기존 장비 컨트롤러를 공유하므로 같은 소총 이동을 사용한다. 새로운 장비 컨트롤러는 데이터 매핑으로 확장하고, 매핑/로컬 프로필이 없으면 원래 장비 컨트롤러를 사용한다. 맨손은 원래 컨트롤러를 사용한다.

기본 PlayerCharacter.controller 자체는 바꾸지 않는다. 로컬2개 복사본에서 Locomotion Motion만 새 트리로 바꾸고 나머지 기존 상태/6개 레이어/마스크/파라미터를 유지한다. Speed0은 새 무장 대기, .5는 걷기,1은 달리기,2는 같은 달리기의1.5배 재생이다. 좌우/전후4방향을 MoveX/MoveY로 혼합해 대각을 만든다. 권총의 느린 좌우는 동일 원본을 .5배 재생한다. 기존 입력과 실제 이동 속도는 유지한다. 원본 부족을 임의 관절 포즈로 메우거나 새 합성 .anim을 만들지 않았다.

상체 그립은 기존 UpperCombat의 Quaternius/KayKit와 IK가 계속 담당한다. Base Locomotion은 새 원본이지만, 최종 전신 자세를 원본 프리뷰와 완전히 같다고 주장하지 않는다. 기존 상체 안정화와 포복/슬라이드/장전/근접/Starter 점프/착석 구현은 유지한다. 전체 모션 구조 전환이나 다른 새 행동까지 끝난 것이 아니다.

**Root 기준 수정**

최초 후보의 새 클립 keepOriginalOrientation=true에서 권총 대기 골반이 약42.6도 틀어져 기존20도 방향 검사에 실패했다. 새로운16개만 Root 회전을 Body Orientation 기준(keepOriginalOrientation=false, lockRootRotation=true)으로 임포트해 원본 본 회전 코드를 추가하지 않고 기준 방향을 맞췄다. 기존 포복/슬라이드 임포트 설정은 건드리지 않았다. 수정 후 대기 골반 최대 오차 권총3.87도/소총0.29도, 전진 보행/달리기/전력질주/앉기까지 양쪽 장비의 기존20도 한계를 통과했다. 원본 FBX·키프레임은 그대로이며 Humanoid 임포트/Root 기준만 다르다.

**모든 파일과 직렬화 변경**

MixamoArmedLocomotionFiles.csv에 신규/수정 스크립트, 각각의 신규 .meta, 선택16개 FBX와메타, 폴더메타, 로컬2개 컨트롤러와프로필 및 문서를 한 줄씩 기록했다. 기존 메서드·필드·컴포넌트·원본 자산은 삭제하지 않았다. 기존 장비 컨트롤러 선택에 합성 단계를 추가했고, 로컬 복사본의 기존 Locomotion Motion 참조만 대체했다. 기존 원본/기본 그래프와 사용자 구조는 유지한다.

**사용 위치와 재구성**

- 기존 Armature의 PlayerCharacterCombatView가 재생한다. 새 런타임 컴포넌트는 추가하지 않는다.
- 로컬 Assets/ThirdParty/MixamoLocal/ArmedLocomotion/ArmedLocomotion.asset을 Play/빌드 씬 복사본에서만 주입한다. 편집 상태의 Scene 파일에는 새 참조를 저장하지 않는다.
- Tools/Battlefield/Animations/Mixamo/Import Armed Locomotion 메뉴가 보유 LocalAssets/Mixamo의 Basic Shooter Pack9개/Pistol_Handgun Locomotion Pack7개로 재구성한다. 원본16개 존재 여부를 먼저 검사하고 누락 시 수입을 중단한다.
- 기존 MixamoLocal Git 제외 정책을 유지한다. 새 로컬42파일은 공개 Git 원본 공유 대상으로 추가하지 않는다. 재수입은 기존 프로필·컨트롤러·임포트 설정을 보존한다.
- 로컬 컨트롤러는 생성 당시 기본 그래프의 복사본이다. 이후 기본 그래프에 새 상태/레이어를 추가하는 작업에서는 이2개 템플릿도 함께 갱신해야 한다. 일반 수입은 사용자가 수정한 템플릿을 자동 덮어쓰지 않는다.

**검증**

Unity 6000.4.8f1 격리 프로젝트에서 SampleScene과 제품 Play Mode 전체 194/194 통과, 실패/건너뜀0, 181.05초. 모든 세션은 -batchmode -noaudio. 임시 캡처·진단 코드를 Assets에서 제외한 상태로 실행했다. 로그: Library/CodexMixamoArmedApply/full-final.xml 및 full-final.log. 검증한57개 코드·메타·로컬 자산을 본 프로젝트에 바이트 동일 복사했다.

Root 기준 수정 후 비교24개 통과, 두 슬롯 전체 방향/무장 근접 원래 대비 비교2개 통과, 원본 설정/장비와 이동 양쪽 오버라이드 합성/8방향/장전·발사·슬라이드 관련 검사도 확인했다. 최초192개에서 방향1개 실패를 찾아 고쳤으며, 최종 결과와 혼동하지 않는다. 재수입 전후 로컬41파일(루트 폴더메타 제외) 해시가 모두 같았다. 실제 소총/권총 이동, 슬라이드, 장전, 포복 ADS 캡처를 확인했다. 임시 캡처·Root비교·근접 진단 코드는 Library에 보관하고 제품 Assets에 포함하지 않는다.

사용자 Editor에서 Play 모드를 재시작해 SampleScene의 1=소총/2=권총/3=맨손, WASD/Shift, 마우스 상하/ADS, R장전, 달리기+C, 포복·기체 탑승/하차를 직접 확인해야 한다. 경사면 발 접지, 방향 전환의 발 위상·미끄러짐 등 시각적 선호와 대상 Player 빌드/성능 비교는 아직 확인하지 않았다. 해당 완성도를 테스트 통과만으로 보장하지 않는다.

**객체지향·SOLID 검토**

데이터 선택(프로필), Unity 컨트롤러 합성/캐시(일반C#객체), 기존 표현 연결(CombatView), 원본 수입(EditorImporter), 씬/빌드 제공(EditorSetup)으로 책임을 분리했다. 상태와 캐시는 책임 객체 안에 캡슐화하고 읽기 조회/의도 있는 초기화/Dispose만 노출한다. Unity RuntimeAnimatorController/ScriptableObject의 기존 계약과 구성 방식을 사용하며 새 상속·넓은 인터페이스를 만들지 않았다. 이동·장비 양쪽 오버라이드 템플릿을 지원하고 장비별 변형은 데이터로 확장한다. 입력·이동·피해의 고수준 정책은 수정하지 않았고 의존성은 명시적 참조로 제공했다. 기존 상체 안정화·손 접촉 IK는 회귀 방지와 총기 맞춤을 위해 의도적으로 유지한다.
