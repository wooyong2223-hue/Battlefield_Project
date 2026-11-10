# 캐릭터 Armature 모델·애니메이션 연결 변경 보고

이번 작업은 로컬에 보관된 공식 Starter Assets ThirdPerson 패키지의 Armature를 기존 캐릭터에 연결한 작업이다. 이전 작업의 캐릭터 이동·발사·피격 수정과 이번 변경을 구분한다.

## 변경 전 비교

기존 검증 사본과 비교해 PlayerCharacterController, PlayerCharacterMovement, 입력 액션, CombatTestScene, CombatStructureTests, PlayerCharacterMovementTests는 같았다. 새로 달라진 Noto Sans KR SDF.asset의 폰트 아틀라스 1×1 → 1024×1024 변경은 보존했다. 패키지 목록과 잠금 파일은 수정하지 않았다.

## 코드와 구성 변경

- PlayerCharacterMovement.cs: 읽기 전용 `public float MoveSpeed => _moveSpeed;`, `public float SprintSpeed => _sprintSpeed;` 추가. 이동값을 복제하지 않고 애니메이션이 현재 설정을 관찰하도록 했다. 이동·충돌·점프 코드는 교체하거나 삭제하지 않았다. 걷기 8, 달리기 12, 점프 높이 1.2를 유지했다.
- PlayerCharacterAnimationView.cs: 새 표현 컴포넌트. 직렬화된 `_movement`, `_animator`, `_speedDamping = 0.1f`를 사용한다. LateUpdate에서 실제 수평 속도를 읽고 걷기 기준 Speed=1, 달리기 기준 Speed=2로 정규화한다. 벽에 막히면 실제 속도 감소가 반영된다. Grounded와 VerticalSpeed를 Animator에 전달하며, 입력이나 이동 상태를 변경하지 않는다. Awake에서 필수 참조를 검사하고 applyRootMotion=false를 적용한다.
- PlayerCharacter.controller: 프로젝트 소유 Animator Controller 추가. Speed(float), Grounded(bool, 기본 true), VerticalSpeed(float). Locomotion의 1D Blend Tree는 Idle=0, Walk_N=1, Run_N=2. 지면 이탈과 상승 속도에 따라 Jump 또는 InAir로 전환하고, 착지하면 Land를 거쳐 Locomotion으로 복귀한다. Land 중 재점프도 허용한다. 일반 전환 시간 0.08초, Land 복귀 exitTime=0.65. writeDefaultValues=false.
- PlayerCharacterArmature.prefab: Humanoid Avatar, 위 컨트롤러, Body·Arms·Legs URP 재질 세 개를 연결한 재사용 모델 프리팹. applyRootMotion=false, cullingMode=AlwaysAnimate. 이동 컨트롤러·입력·Cinemachine은 포함하지 않는다.
- CombatTestScene.unity: 사용자 승인에 따라 기존 Visual의 MeshRenderer(fileID 757094940) `m_Enabled: 1 → 0`. Visual 오브젝트, MeshFilter, MeshRenderer, 배율 (2,4,1)을 모두 보존한다. 캐릭터 루트 Transform(757094942)의 자식에 Armature 프리팹을 추가했다. 모델의 균등 배율은 약 2.270328, 로컬 위치는 약 (0,-2.01822,0). 기존 높이 4의 캡슐에 모델의 높이와 발 위치를 맞춘 값이다. 새 PlayerCharacterAnimationView의 _movement는 기존 이동 컴포넌트(950000002), _animator는 모델의 Animator를 참조한다. 기존 씬의 다른 블록은 유지했다.
- PlayerCharacterAnimationTests.cs: 실제 씬에서 Humanoid·재질·참조·기존 Visual 보존을 확인하는 테스트와, 실제 이동에 따른 Idle/Walk/Run 및 Jump/InAir/Land 전환·루트 위치/회전 유지를 확인하는 테스트 두 개를 추가했다.
- UnityStarterAssets 원본 에셋: Armature 모델 1개, FBX 애니메이션 5개(클립은 6개), 재질 3개, 텍스처 9개, 기존 license.txt를 가져왔다. 원본 GUID와 FBX·텍스처 바이트는 보존했다. 가져오는 FBX 메타데이터에서 패키지 전용 OnFootstep·OnLand 오디오 이벤트를 제외했다. 해당 오디오 처리 스크립트를 도입하지 않아 생길 수 있는 수신자 없는 이벤트 호출을 피하기 위함이다. 기존 프로젝트 이벤트를 삭제한 작업은 아니다. SOURCE.md에 출처와 적용 차이를 기록했다.
- 새 파일과 폴더의 .meta는 검증 프로젝트의 Unity가 생성한 값을 함께 반영했다. 패키지에 있던 에셋의 .meta는 GUID를 유지했다.

## 제거·교체 내역

기존 프로젝트의 메서드, 필드, 컴포넌트, 오브젝트, 참조, 에셋은 삭제하지 않았다. 큐브 표시만 OFF로 바뀌고 새 모델이 대신 표시된다. 원본 패키지의 이동·입력·카메라·샘플 씬은 가져오지 않았다. 현재 캐릭터 카메라와 기존 입력 바인딩은 유지되며, 한 캐릭터의 1인칭·3인칭 전환은 이번 모델 연결의 구현 범위가 아니다.

## 설계 점검

이동 상태의 소유자는 PlayerCharacterMovement, 표현의 소유자는 PlayerCharacterAnimationView, 전환과 클립 구성의 소유자는 Animator Controller다. 읽기 전용 속성과 명시적 직렬화 참조로 상태 변경을 제한했다. 단일 이동 구현에 대한 로컬 표현 연결이므로 별도 인터페이스나 상속 계층을 만들지 않았다. 이는 실제 교환 가능한 구현이 생기기 전의 불필요한 추상화를 피하기 위한 선택이다. 게임플레이 권한을 애니메이션 이벤트나 루트 모션으로 옮기지 않았다.

## 검증

- Unity 6000.4.8f1의 별도 검증 프로젝트에서 임포트·스크립트 컴파일 성공.
- 최종 원본 반영 파일과 같은 검증본에서 PlayMode 12/12 통과. 새 애니메이션 테스트 2개와 기존 이동·점프·충돌·아군 소유 발사·팀 판정·피격 효과·탄흔 회귀 테스트를 포함한다.
- 실제 URP 렌더링 PNG에서 모델 외형과 세 부위 재질 표시를 확인했다.
- 새 에셋 GUID 중복 없음, .meta 짝 존재, 원본/검증본의 관련 57개 파일 일치, git diff --check 통과.
- Noto Sans KR 폰트 변경과 Packages/manifest.json·packages-lock.json은 작업 시작 상태를 보존했다.
- 원본 Unity Editor에서 직접 조작하는 시각 검증은 남아 있다. CombatTestScene을 다시 불러오고 WASD·Shift·Space로 보행·달리기·점프를 확인한다. 현재 이동 속도·키·카메라 구도를 유지했으므로 애니메이션 보폭과 실제 이동 사이의 발 미끄러짐, 시각적 전환 느낌은 추가 확인이 필요하다. 방향별 조준·무기 자세·별도 스트레이프 애니메이션은 추가하지 않았다.

## 이번 작업의 모든 변경 파일

아래 목록은 이전 작업부터 남아 있던 미수정 파일을 제외하고 이번 작업에서 추가·수정한 스크립트·에셋과 그 메타데이터를 포함한다. 이 보고서 자체도 새 문서다.

- [Assets/Features/PlayerCharacter/Animation.meta](E:/GitHub/Battlefield_Project/Assets/Features/PlayerCharacter/Animation.meta)
- [Assets/Features/PlayerCharacter/Animation/PlayerCharacter.controller](E:/GitHub/Battlefield_Project/Assets/Features/PlayerCharacter/Animation/PlayerCharacter.controller)
- [Assets/Features/PlayerCharacter/Animation/PlayerCharacter.controller.meta](E:/GitHub/Battlefield_Project/Assets/Features/PlayerCharacter/Animation/PlayerCharacter.controller.meta)
- [Assets/Features/PlayerCharacter/Prefabs.meta](E:/GitHub/Battlefield_Project/Assets/Features/PlayerCharacter/Prefabs.meta)
- [Assets/Features/PlayerCharacter/Prefabs/PlayerCharacterArmature.prefab](E:/GitHub/Battlefield_Project/Assets/Features/PlayerCharacter/Prefabs/PlayerCharacterArmature.prefab)
- [Assets/Features/PlayerCharacter/Prefabs/PlayerCharacterArmature.prefab.meta](E:/GitHub/Battlefield_Project/Assets/Features/PlayerCharacter/Prefabs/PlayerCharacterArmature.prefab.meta)
- [Assets/Features/PlayerCharacter/Scripts/PlayerCharacterAnimationView.cs](E:/GitHub/Battlefield_Project/Assets/Features/PlayerCharacter/Scripts/PlayerCharacterAnimationView.cs)
- [Assets/Features/PlayerCharacter/Scripts/PlayerCharacterAnimationView.cs.meta](E:/GitHub/Battlefield_Project/Assets/Features/PlayerCharacter/Scripts/PlayerCharacterAnimationView.cs.meta)
- [Assets/Features/PlayerCharacter/Scripts/PlayerCharacterMovement.cs](E:/GitHub/Battlefield_Project/Assets/Features/PlayerCharacter/Scripts/PlayerCharacterMovement.cs)
- [Assets/Scenes/CombatTestScene.unity](E:/GitHub/Battlefield_Project/Assets/Scenes/CombatTestScene.unity)
- [Assets/Tests/PlayMode/PlayerCharacterAnimationTests.cs](E:/GitHub/Battlefield_Project/Assets/Tests/PlayMode/PlayerCharacterAnimationTests.cs)
- [Assets/Tests/PlayMode/PlayerCharacterAnimationTests.cs.meta](E:/GitHub/Battlefield_Project/Assets/Tests/PlayMode/PlayerCharacterAnimationTests.cs.meta)
- [Assets/ThirdParty/UnityStarterAssets.meta](E:/GitHub/Battlefield_Project/Assets/ThirdParty/UnityStarterAssets.meta)
- [Assets/ThirdParty/UnityStarterAssets/Animations.meta](E:/GitHub/Battlefield_Project/Assets/ThirdParty/UnityStarterAssets/Animations.meta)
- [Assets/ThirdParty/UnityStarterAssets/Animations/Jump--InAir.anim.fbx](E:/GitHub/Battlefield_Project/Assets/ThirdParty/UnityStarterAssets/Animations/Jump--InAir.anim.fbx)
- [Assets/ThirdParty/UnityStarterAssets/Animations/Jump--InAir.anim.fbx.meta](E:/GitHub/Battlefield_Project/Assets/ThirdParty/UnityStarterAssets/Animations/Jump--InAir.anim.fbx.meta)
- [Assets/ThirdParty/UnityStarterAssets/Animations/Jump--Jump.anim.fbx](E:/GitHub/Battlefield_Project/Assets/ThirdParty/UnityStarterAssets/Animations/Jump--Jump.anim.fbx)
- [Assets/ThirdParty/UnityStarterAssets/Animations/Jump--Jump.anim.fbx.meta](E:/GitHub/Battlefield_Project/Assets/ThirdParty/UnityStarterAssets/Animations/Jump--Jump.anim.fbx.meta)
- [Assets/ThirdParty/UnityStarterAssets/Animations/Locomotion--Run_N.anim.fbx](E:/GitHub/Battlefield_Project/Assets/ThirdParty/UnityStarterAssets/Animations/Locomotion--Run_N.anim.fbx)
- [Assets/ThirdParty/UnityStarterAssets/Animations/Locomotion--Run_N.anim.fbx.meta](E:/GitHub/Battlefield_Project/Assets/ThirdParty/UnityStarterAssets/Animations/Locomotion--Run_N.anim.fbx.meta)
- [Assets/ThirdParty/UnityStarterAssets/Animations/Locomotion--Walk_N.anim.fbx](E:/GitHub/Battlefield_Project/Assets/ThirdParty/UnityStarterAssets/Animations/Locomotion--Walk_N.anim.fbx)
- [Assets/ThirdParty/UnityStarterAssets/Animations/Locomotion--Walk_N.anim.fbx.meta](E:/GitHub/Battlefield_Project/Assets/ThirdParty/UnityStarterAssets/Animations/Locomotion--Walk_N.anim.fbx.meta)
- [Assets/ThirdParty/UnityStarterAssets/Animations/Stand--Idle.anim.fbx](E:/GitHub/Battlefield_Project/Assets/ThirdParty/UnityStarterAssets/Animations/Stand--Idle.anim.fbx)
- [Assets/ThirdParty/UnityStarterAssets/Animations/Stand--Idle.anim.fbx.meta](E:/GitHub/Battlefield_Project/Assets/ThirdParty/UnityStarterAssets/Animations/Stand--Idle.anim.fbx.meta)
- [Assets/ThirdParty/UnityStarterAssets/Materials.meta](E:/GitHub/Battlefield_Project/Assets/ThirdParty/UnityStarterAssets/Materials.meta)
- [Assets/ThirdParty/UnityStarterAssets/Materials/M_Armature_Arms.mat](E:/GitHub/Battlefield_Project/Assets/ThirdParty/UnityStarterAssets/Materials/M_Armature_Arms.mat)
- [Assets/ThirdParty/UnityStarterAssets/Materials/M_Armature_Arms.mat.meta](E:/GitHub/Battlefield_Project/Assets/ThirdParty/UnityStarterAssets/Materials/M_Armature_Arms.mat.meta)
- [Assets/ThirdParty/UnityStarterAssets/Materials/M_Armature_Body.mat](E:/GitHub/Battlefield_Project/Assets/ThirdParty/UnityStarterAssets/Materials/M_Armature_Body.mat)
- [Assets/ThirdParty/UnityStarterAssets/Materials/M_Armature_Body.mat.meta](E:/GitHub/Battlefield_Project/Assets/ThirdParty/UnityStarterAssets/Materials/M_Armature_Body.mat.meta)
- [Assets/ThirdParty/UnityStarterAssets/Materials/M_Armature_Legs.mat](E:/GitHub/Battlefield_Project/Assets/ThirdParty/UnityStarterAssets/Materials/M_Armature_Legs.mat)
- [Assets/ThirdParty/UnityStarterAssets/Materials/M_Armature_Legs.mat.meta](E:/GitHub/Battlefield_Project/Assets/ThirdParty/UnityStarterAssets/Materials/M_Armature_Legs.mat.meta)
- [Assets/ThirdParty/UnityStarterAssets/Models.meta](E:/GitHub/Battlefield_Project/Assets/ThirdParty/UnityStarterAssets/Models.meta)
- [Assets/ThirdParty/UnityStarterAssets/Models/Armature.fbx](E:/GitHub/Battlefield_Project/Assets/ThirdParty/UnityStarterAssets/Models/Armature.fbx)
- [Assets/ThirdParty/UnityStarterAssets/Models/Armature.fbx.meta](E:/GitHub/Battlefield_Project/Assets/ThirdParty/UnityStarterAssets/Models/Armature.fbx.meta)
- [Assets/ThirdParty/UnityStarterAssets/SOURCE.md](E:/GitHub/Battlefield_Project/Assets/ThirdParty/UnityStarterAssets/SOURCE.md)
- [Assets/ThirdParty/UnityStarterAssets/SOURCE.md.meta](E:/GitHub/Battlefield_Project/Assets/ThirdParty/UnityStarterAssets/SOURCE.md.meta)
- [Assets/ThirdParty/UnityStarterAssets/Textures.meta](E:/GitHub/Battlefield_Project/Assets/ThirdParty/UnityStarterAssets/Textures.meta)
- [Assets/ThirdParty/UnityStarterAssets/Textures/Armature_Arms_AlbedoTransparency.tif](E:/GitHub/Battlefield_Project/Assets/ThirdParty/UnityStarterAssets/Textures/Armature_Arms_AlbedoTransparency.tif)
- [Assets/ThirdParty/UnityStarterAssets/Textures/Armature_Arms_AlbedoTransparency.tif.meta](E:/GitHub/Battlefield_Project/Assets/ThirdParty/UnityStarterAssets/Textures/Armature_Arms_AlbedoTransparency.tif.meta)
- [Assets/ThirdParty/UnityStarterAssets/Textures/Armature_Arms_MetallicSmoothness.tif](E:/GitHub/Battlefield_Project/Assets/ThirdParty/UnityStarterAssets/Textures/Armature_Arms_MetallicSmoothness.tif)
- [Assets/ThirdParty/UnityStarterAssets/Textures/Armature_Arms_MetallicSmoothness.tif.meta](E:/GitHub/Battlefield_Project/Assets/ThirdParty/UnityStarterAssets/Textures/Armature_Arms_MetallicSmoothness.tif.meta)
- [Assets/ThirdParty/UnityStarterAssets/Textures/Armature_Arms_Normal.tif](E:/GitHub/Battlefield_Project/Assets/ThirdParty/UnityStarterAssets/Textures/Armature_Arms_Normal.tif)
- [Assets/ThirdParty/UnityStarterAssets/Textures/Armature_Arms_Normal.tif.meta](E:/GitHub/Battlefield_Project/Assets/ThirdParty/UnityStarterAssets/Textures/Armature_Arms_Normal.tif.meta)
- [Assets/ThirdParty/UnityStarterAssets/Textures/Armature_Body_AlbedoTransparency.tif](E:/GitHub/Battlefield_Project/Assets/ThirdParty/UnityStarterAssets/Textures/Armature_Body_AlbedoTransparency.tif)
- [Assets/ThirdParty/UnityStarterAssets/Textures/Armature_Body_AlbedoTransparency.tif.meta](E:/GitHub/Battlefield_Project/Assets/ThirdParty/UnityStarterAssets/Textures/Armature_Body_AlbedoTransparency.tif.meta)
- [Assets/ThirdParty/UnityStarterAssets/Textures/Armature_Body_MetallicSmoothness.tif](E:/GitHub/Battlefield_Project/Assets/ThirdParty/UnityStarterAssets/Textures/Armature_Body_MetallicSmoothness.tif)
- [Assets/ThirdParty/UnityStarterAssets/Textures/Armature_Body_MetallicSmoothness.tif.meta](E:/GitHub/Battlefield_Project/Assets/ThirdParty/UnityStarterAssets/Textures/Armature_Body_MetallicSmoothness.tif.meta)
- [Assets/ThirdParty/UnityStarterAssets/Textures/Armature_Body_Normal.tif](E:/GitHub/Battlefield_Project/Assets/ThirdParty/UnityStarterAssets/Textures/Armature_Body_Normal.tif)
- [Assets/ThirdParty/UnityStarterAssets/Textures/Armature_Body_Normal.tif.meta](E:/GitHub/Battlefield_Project/Assets/ThirdParty/UnityStarterAssets/Textures/Armature_Body_Normal.tif.meta)
- [Assets/ThirdParty/UnityStarterAssets/Textures/Armature_Legs_AlbedoTransparency.tif](E:/GitHub/Battlefield_Project/Assets/ThirdParty/UnityStarterAssets/Textures/Armature_Legs_AlbedoTransparency.tif)
- [Assets/ThirdParty/UnityStarterAssets/Textures/Armature_Legs_AlbedoTransparency.tif.meta](E:/GitHub/Battlefield_Project/Assets/ThirdParty/UnityStarterAssets/Textures/Armature_Legs_AlbedoTransparency.tif.meta)
- [Assets/ThirdParty/UnityStarterAssets/Textures/Armature_Legs_MetallicSmoothness.tif](E:/GitHub/Battlefield_Project/Assets/ThirdParty/UnityStarterAssets/Textures/Armature_Legs_MetallicSmoothness.tif)
- [Assets/ThirdParty/UnityStarterAssets/Textures/Armature_Legs_MetallicSmoothness.tif.meta](E:/GitHub/Battlefield_Project/Assets/ThirdParty/UnityStarterAssets/Textures/Armature_Legs_MetallicSmoothness.tif.meta)
- [Assets/ThirdParty/UnityStarterAssets/Textures/Armature_Legs_Normal.tif](E:/GitHub/Battlefield_Project/Assets/ThirdParty/UnityStarterAssets/Textures/Armature_Legs_Normal.tif)
- [Assets/ThirdParty/UnityStarterAssets/Textures/Armature_Legs_Normal.tif.meta](E:/GitHub/Battlefield_Project/Assets/ThirdParty/UnityStarterAssets/Textures/Armature_Legs_Normal.tif.meta)
- [Assets/ThirdParty/UnityStarterAssets/license.txt](E:/GitHub/Battlefield_Project/Assets/ThirdParty/UnityStarterAssets/license.txt)
- [Assets/ThirdParty/UnityStarterAssets/license.txt.meta](E:/GitHub/Battlefield_Project/Assets/ThirdParty/UnityStarterAssets/license.txt.meta)
- [Docs/AI/PlayerCharacterAnimationChanges.md](E:/GitHub/Battlefield_Project/Docs/AI/PlayerCharacterAnimationChanges.md)

## 검증 산출물

다음은 Git 대상이 아닌 Temp 검증 산출물이다. 검증용 사본에서만 사용한 Editor 스크립트는 원본 Assets에 추가하지 않았다.

- [Temp/codex-armature-final-playmode.xml](E:/GitHub/Battlefield_Project/Temp/codex-armature-final-playmode.xml)
- [Temp/codex-armature-final-playmode.log](E:/GitHub/Battlefield_Project/Temp/codex-armature-final-playmode.log)
- [Temp/codex-armature-preview.png](E:/GitHub/Battlefield_Project/Temp/codex-armature-preview.png)
- [Temp/codex-armature-import-manifest.json](E:/GitHub/Battlefield_Project/Temp/codex-armature-import-manifest.json)
- [Temp/CodexStructureValidation/Assets/Editor/CodexArmatureSetup.cs](E:/GitHub/Battlefield_Project/Temp/CodexStructureValidation/Assets/Editor/CodexArmatureSetup.cs)
