# Throw + Throw Object 왼팔 테스트 조합

2026-11-04. 사용자 요청에 따라 원본 클립을 수정하지 않고 Animator 레이어로 조합했다.

## 변경 전 비교와 교체

직전 `Library/CodexGrenadeRelease/applied.json`의 11개 파일 해시가 모두 일치했다. 사용자가 새로 넣은 `LocalAssets/Mixamo/Throw.fbx`(510624 bytes), `Throw Object.fbx`(704624 bytes)를 확인했다. 기존 컨트롤러 3개와 로컬 프로필 2개는 이전 검증 복사본과 일치했으며, 반영 직전에도 해시가 그대로임을 확인했다. 격리 프로젝트에서 생성·검증한 에셋만 반영했다. 이전 버전은 `Library/CodexGrenadeComposition/Before`에 보존했다.

기존 `_throwAnimation`: `Throw Grenade`, 약 3.5초 → `Throw`, 2.2초.

신규 `_leftThrowAnimation`: `Throw Object`, 4.866667초. `GrenadeLeftArm` Override 레이어의 Avatar Mask에서 LeftArm과 LeftFingers만 활성화한다. 기존 GrenadeUpperBody 마스크(양팔·손)를 유지하므로 새 전신 투척/하체 적용을 추가한 것은 아니다. 원본 Throw Grenade와 기존 장착/대기 클립은 삭제하지 않았다.

## 파일별 변경

### 런타임 스크립트

- `Assets/Features/Weapon/Scrips/GrenadeDefinition.cs`: 선택적 `_leftThrowAnimation`과 읽기 전용 `LeftThrowAnimation` 추가. 기존 필드 이름/피해/속도/도화선/방출 비율 유지.
- `Assets/Features/PlayerCharacter/Scripts/TacticalAnimationSet.cs`: `_leftThrowPlaceholder`, `HasLeftThrow` 추가. IsApplied/Read/Apply/Restore가 설정된 경우에만 다섯 번째 클립 매핑을 관리한다. 기존 네 슬롯과 매핑 소유권/복원 동작 유지.
- `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterTacticalAnimationView.cs`: Throwing 중에만 추가 레이어의 가중치를 활성화한다. 기존 ThrowTime을 공유하며 진입 .12초 가중치 혼합, 종료는 기존 PoseWeight를 사용한다. OnDisable에서도 가중치 0. 새 관절 회전/위치 보정 코드 없음.

### Editor 및 테스트

- `Assets/Features/PlayerCharacter/Editor/GrenadeThrowCompositionSetup.cs` 및 `.meta`: 새 명시적 메뉴 `Tools/Battlefield/Animations/Mixamo/Apply Throw With Throw Object Left Arm`. 사용자 FBX 두 개를 로컬 제외 폴더로 복사하고 Humanoid(압축 없음/루프 없음/원본 전체 프레임)로 가져온다. 마스크·고유 CC0 자리표시 클립·컨트롤러 레이어와 로컬 연결을 설정한다. 새 클래스는 Editor 에셋 설치 책임이며 자동 실행하지 않는다. 재실행 시 기존 FBX import 값과 마스크/레이어를 보존하고 지정된 클립 연결을 적용한다.
- `Assets/Tests/PlayMode/GrenadeThrowCompositionTests.cs` 및 `.meta`: 마스크/컨트롤러 3개/클립 연결 검사와 3장비×2시점 실제 재생 검사. 가중치·클립·ThrowTime·왼팔 변경·오른팔/몸통/다리 보존·Root Motion OFF·취소 복귀 확인.

### 신규 애니메이션 에셋

- `Assets/Features/PlayerCharacter/Animation/Tactical/GrenadeLeftArm.mask` 및 `.meta`: 왼팔·왼손 손가락만 활성화.
- `Assets/Features/PlayerCharacter/Animation/Tactical/ThrowLeftArmPlaceholder.anim` 및 `.meta`: 기존 CC0 ThrowPlaceholder 복사본. 공개 컨트롤러가 로컬 Mixamo 원본을 직접 참조하지 않도록 별도 override 키 제공. 모션 키 제작/수정 없음.
- `Assets/ThirdParty/MixamoLocal/TacticalActions/Throw.fbx` 및 `.meta`: 사용자 원본과 SHA256 동일한 로컬 복사본, Humanoid import.
- `Assets/ThirdParty/MixamoLocal/TacticalActions/Throw Object.fbx` 및 `.meta`: 사용자 원본과 SHA256 동일한 로컬 복사본, Humanoid import. 두 FBX는 기존 MixamoLocal Git 제외 정책 유지.

### 수정한 연결 에셋

- `Assets/Features/PlayerCharacter/Animation/PlayerCharacter.controller`
- `Assets/ThirdParty/MixamoLocal/ArmedLocomotion/RifleLocomotion.controller`
- `Assets/ThirdParty/MixamoLocal/ArmedLocomotion/PistolLocomotion.controller`

위 3개는 각각 기존 내용 삭제 없이 GrenadeLeftArm 레이어·Throw Object 상태를 60줄 추가했다. 기본 가중치 0, Override, write-defaults OFF, timeParameter=기존 ThrowTime.

- `Assets/ThirdParty/MixamoLocal/TacticalActions/FragGrenade.asset`: `_throwAnimation`을 Throw로 교체하고 `_leftThrowAnimation`에 Throw Object 연결. 기존 수치/장착/대기/카메라 오프셋 보존.
- `Assets/ThirdParty/MixamoLocal/TacticalActions/TacticalActions.asset`: `_leftThrowPlaceholder`만 추가 연결.
- `Docs/AI/UnityProjectContext.md`: 최신 적용/검증 기록.
- 이 문서: 변경·검증·한계 기록.

## 구조와 동작

기존 애니메이션 표시 컴포넌트 안에서 담당하므로 추가 런타임 관리자나 상속은 필요하지 않았다. 클립 선택은 GrenadeDefinition, override 슬롯은 TacticalAnimationSet, 레이어 가중치는 TacticalAnimationView, 설치는 Editor 클래스가 소유한다. 기존 입력·투척·피해 코드와 분리한다. 선택적 연결을 사용해 왼팔 클립이 없는 프로필의 기존 동작도 유지한다. OOP/SOLID 검토상 추가 추상화나 의도적 예외 없음.

4 장착 → 왼쪽 버튼 유지 시 궤적 → 놓으면 투척 → 4/다른 무기 취소 조작은 그대로다. ThrowTime 0..1을 공유하므로 4.866667초 왼팔은 2.2초에 맞춰 약 2.21배 빠르게 재생된다. 방출 비율 0.7은 그대로여서 새 기본 모션의 실제 방출 시점은 시작 후 약 1.54초다. 서로 다른 모션의 준비/던짐 순간이 일치한다는 보장은 없다.

## 검증

격리 Unity `-noaudio` PlayMode 최종 14/14 통과, 실패/스킵 0, 11.5629751초. 구성: 기존 관련 11개 + 신규 합성 2개 + 임시 캡처 1개. TEST_AUDIO는 이전 False / Play 중 True / 종료 후 False 확인. 컴파일 오류 없음. 첫 검사에서 Unity 상태 normalizedTime을 클립 Motion Time으로 간주한 검사가 실패했으며, 실제 공유 ThrowTime과 포즈 영향 검사로 수정 후 모두 재실행했다.

동일한 포즈에서 가중치 0/1을 비교해 왼팔만 바뀌는 것을 검사했다. 같은 프레임 스킨 렌더 캐시를 피하려고 진단 캡처는 각 포즈의 BakeMesh 결과로 비교했으며, 중복 배율을 제거해 최종 15장(전신 비교 10장+1인칭 5장)을 생성하고 대표 전신·1인칭 이미지를 확인했다. 진단 코드/이미지/로그/XML은 `Library/CodexGrenadeComposition`에 보관하고 제품 Assets에 캡처 스크립트를 남기지 않는다.

전신 비교에서 왼팔 자세 차이를 시각적으로 확인했다. 다만 1인칭 캡처에는 팔이 위로 크게 올라가 화면 밖으로 나가는 구간이 있어 현 상태를 최종 품질로 판단하지 않는다. 기존 1인칭 오프셋/모션은 이번에 보정하지 않았다. 안전핀/그립 일치·자연스러운 타이밍은 사용자가 SampleScene에서 판단할 테스트 항목이다. 전체 테스트 스위트와 Windows 빌드는 실행하지 않았다. 원본 파일·기존 에셋/메서드 삭제 없음, 커밋 없음.
