# 모션 캡처 기반 주먹·근접 공격 교체

> 이 문서는 이전 Rokoko 적용 이력이다. 현재 사용자는 Quaternius를 메인으로 선택했고 해당 근접 참조와 1인칭 기준 변환은 [최신 적용 기록](QuaterniusMainAnimationChanges.md)으로 교체됐다. 과거 구현/검증은 이력으로 보존한다.

## 문제와 선택

이전 PlayerCharacterFistPose는 준비·타격 위치를 코드로 정하고 손목/팔꿈치 IK로 기존 클립을 덮어썼다. 전방 도달 검사에는 통과했지만 사용자가 부자연스러운 자세를 확인했다. 이번에는 이 구현을 제거하고 실제 복싱 동작을 재생한다.

공식 무료 자료를 조사했다. Quaternius/KayKit의 기존 클립, Rokoko 무술·격투 자료를 비교했고 Rokoko의 Boxing_mixamo.fbx를 선택했다. 공식 리소스 페이지: https://www.rokoko.com/resources/rokoko-mocap-13-free-fight-animations . 원본·변형·사용 허용 안내·해시는 Assets/ThirdParty/Rokoko/SOURCE.md에 기록한다. 이 자료를 CC0라고 표기하지 않는다.

## 이전 작업과 비교

작업 시작 시 이전 검증본과 관련 스크립트·씬·컨트롤러가 일치함을 확인했다. Library/CodexMeleeClips/baseline.json에 시작 해시와 Baseline 사본을 보관한다. 반영 직전에 같은 해시를 다시 검사한다. 사용자/Editor가 새로 저장한 관련 값·참조·계층 변경은 발견하지 않았다. 다른 미커밋 작업은 이번 수정 범위에 포함하지 않는다.

## 변경 파일 전체

| 파일 | 코드·값 및 동작 변경 |
| --- | --- |
| Assets/Features/PlayerCharacter/Scripts/PlayerCharacterCombatView.cs | MeleeCombat 레이어를 검증·선택한다. Play에 melee 인수를 추가하고 OnMelee가 새 레이어의 상태를 재생한다. 진입 CrossFade 0.05초, 대기 복귀 0.08초, 레이어 가중치 변경 속도 12.5/s. 읽기 전용 MeleePoseWeight는 1인칭 표시 전환에만 제공한다. 비활성화 시 새 레이어·상태도 초기화한다. 기존 권총 조준/발사/장전 경로와 이벤트 수명은 유지한다. |
| Assets/Features/PlayerCharacter/Scripts/PlayerCharacterWeaponAimView.cs | 이전 주먹 IK 객체·구독·호출 제거. 새 MeleeCombat 평가에서는 기존 클립의 손 위치를 덮어쓰지 않는다. 3인칭 상하 조준만 Spine/UpperChest에 각각 50% 추가한다. 1인칭은 표시 리그에서 시선 회전을 한 번 적용한다. 기존 권총 조준·상체 LookAt 경로는 유지한다. 본/레이어 ID는 Awake에서 캐시한다. |
| Assets/Features/PlayerCharacter/Scripts/FirstPersonArmsView.cs | 근접 레이어 가중치로 권총/근접의 기준 회전·위치를 블렌드한다. 기존 `_poseAnchor`(UpperChest) 원점을 재사용하고 근접 표시에서는 가슴의 yaw/roll을 제거하되 pitch를 보존해 카메라에 정렬한다. `_unarmedOffset` 기본값을 (0,0.75,0.1)→(0,-0.7,0.25)로 변경한다. 새 `_meleeMesh`는 근접 중에만 사용하고 권총 상태에는 원래 메시를 복원한다. 원래 권총 `_poseEuler`, `_offset`, `_adsOffset` 값과 계산은 보존한다. |
| Assets/Features/PlayerCharacter/Animation/PlayerCharacter.controller | UpperCombat의 PunchJab/PunchCross/FistsHold/PistolMelee 상태 4개를 제거하고 MeleeCombat에 같은 이름으로 새 상태 4개를 연결한다. 기존 UpperCombat 상태 머신과 컨트롤러 레이어 목록 두 객체만 수정하고 다른 기존 객체는 보존한다. 새 상태 머신/상태 객체 5개 추가. 기존 이동·생명·장전 상태와 매개변수는 유지한다. |
| Assets/Features/PlayerCharacter/Animation/MeleeUpperBody.mask | 새 마스크. Body, Head, 양팔, 양손가락을 포함하고 Root·다리·손/발 IK는 제외한다. 몸통이 빠진 기존 팔 전용 마스크는 권총에 그대로 사용한다. 새 레이어의 IK Pass는 손 목표를 강제하기 위한 것이 아니라 상하 조준의 척추 회전을 평가하기 위한 것이다. |
| Assets/Features/PlayerCharacter/Animation/MeleeUpperBody.mask.meta | 새 마스크 GUID. |
| Assets/Features/PlayerCharacter/Models/FirstPersonMeleeArms.asset | 기존 Armature 메시로부터 만든 새 1인칭 근접용 파생 메시. 아래팔·손뿐 아니라 위팔 전체를 포함한다. 위팔을 절단하지 않고 표시 좌표계로 시작 부분을 시야 밖에 배치한다. 26,616 삼각형·재질 슬롯 1개. 기존 FirstPersonArms.asset과 원본 Armature는 변경하지 않는다. |
| Assets/Features/PlayerCharacter/Models/FirstPersonMeleeArms.asset.meta | 새 파생 메시 GUID. |
| Assets/Scenes/SampleScene.unity | 기존 FirstPersonArmsView(fileID 1653572103)의 `_unarmedOffset` 한 값을 위와 같이 수정하고 `_meleeMesh` 참조 하나를 추가한다. 다른 씬 값·카메라·기존 메시 참조·계층·컴포넌트 수는 보존한다. |
| Assets/ThirdParty/Rokoko.meta | 새 제작자 폴더 GUID. |
| Assets/ThirdParty/Rokoko/BoxingGuard.anim | Boxing의 11.0–11.4초 구간을 준비 자세 루프로 사용한다. 마지막 0.15초를 시작 자세로 연결한다. |
| Assets/ThirdParty/Rokoko/BoxingGuard.anim.meta | 새 클립 GUID. |
| Assets/ThirdParty/Rokoko/BoxingJab.anim | Boxing의 2.208333–2.708333초 왼손 잽을 사용하고 준비 자세로 복귀하도록 가공한다. 총 0.8초, 재생 속도 1. |
| Assets/ThirdParty/Rokoko/BoxingJab.anim.meta | 새 클립 GUID. |
| Assets/ThirdParty/Rokoko/BoxingCross.anim | Boxing의 9.875–10.208333초 오른손 스트레이트를 사용하고 준비 자세로 복귀하도록 가공한다. 뒤에 이어지는 두 번째 펀치는 제외한다. 총 0.8초, 재생 속도 1. PistolMelee에도 사용하며 전용 pistol-whip 원본이라고 주장하지 않는다. |
| Assets/ThirdParty/Rokoko/BoxingCross.anim.meta | 새 클립 GUID. |
| Assets/ThirdParty/Rokoko/SOURCE.md | 공식 출처·사용 안내·해시·잘라낸 구간·복귀 가공·손가락 자세의 출처를 기록한다. |
| Assets/ThirdParty/Rokoko/SOURCE.md.meta | 출처 문서 GUID. |
| Assets/Tests/PlayMode/PlayerCharacterFistAnimationTests.cs | 실제 접지 상태를 만들고 검사한다. 기존 양 시점 전방 도달 검사에 몸통 회전 검사를 추가한다. 새로 ±35도 상하 조준과 근접 메시 노출/재질 슬롯/권총 메시 복원 검사를 추가한다. 테스트 도우미의 공중 자세와 서 있는 자세를 구분한다. |
| Assets/Tests/PlayMode/PlayerCharacterCombatTests.cs | 근접 재생 확인 경로를 UpperCombat.PunchJab→MeleeCombat.PunchJab으로 변경하고 진입 블렌드 이후 상태를 확인한다. 피해·장전·조준 검사는 보존한다. |
| Docs/AI/PlayerCharacterMocapMeleeChanges.md | 이 변경 보고서. |
| Docs/AI/PlayerCharacterFistForwardChanges.md | 이전 IK 보고서에 이번 구현으로 대체되었다는 안내를 추가한다. 과거 검증 기록은 보존한다. |
| Docs/AI/UnityProjectContext.md | 현재 구현과 근거·검증 한계를 추가한다. |

## 제거·교체한 구현

- Assets/Features/PlayerCharacter/Scripts/PlayerCharacterFistPose.cs 및 .meta 삭제. 이 클래스의 생성자, OnMelee, Reset, Apply, Hand, Reach와 모든 내부 상태가 불필요해졌다. 삭제 필드: `_animator`, `_combat`, `_root`, `_rightShoulder`, `_leftShoulder`, `_rightHand`, `_leftHand`, `_rightKnuckles`, `_leftKnuckles`, `_rightReach`, `_leftReach`, `_leftStrike`, `_startedAt`.
- WeaponAimView의 `_fists`, 주먹 전용 OnEnable/OnEquipmentChanged, OnDisable의 해당 이벤트 해제·Reset, 생성/Apply 호출을 제거했다. 주먹 팔꿈치 힌트 초기화 두 줄도 이 구현과 함께 제거했다. 권총 조준의 기존 SetWeight는 유지한다.
- 검증 중 추가했던 FirstPersonArmsView의 `_meleeAnchor` 머리 본 캐시와 머리 높이 원점 계산도 제거하고 기존 `_poseAnchor`를 재사용한다. 위팔을 자르는 실험용 메시 가공은 최종 에셋에 사용하지 않는다.
- 이전 상수 목표 `(±0.32,0.15,0.45) → (±0.12,0.15,1)`와 손목/팔꿈치 목표 강제 재생을 실제 클립 재생으로 교체했다.
- 기존 UpperCombat의 근접 상태 4개와 해당 모션 참조는 새 레이어/모션으로 교체했다. 원본 Quaternius PunchJab/PunchCross 에셋 자체는 보존한다. 손가락 자세는 이 CC0 클립에서 샘플링했다.
- 모든 새 복싱 클립에는 기존 Quaternius 주먹의 LeftHand/RightHand 손가락 근육 곡선 40개를 고정 자세로 넣었다. 몸통과 팔 경로는 Rokoko 모션이며, 원본 파일 그대로라고 표기하지 않는다.

```csharp
// 이전: 손/팔꿈치 목표로 클립 자세를 덮어씀
_fists.Apply(fistAim, rightGoalOffset, leftGoalOffset);

// 현재: 근접 전용 레이어에서 선택한 실제 모션 재생
_animator.CrossFadeInFixedTime("MeleeCombat." + state, .05f, _meleeLayer, 0f);
```

## 유지한 동작과 설계

주먹 피해 20, 권총 근접 피해 30, 판정 준비 0.2초, 회복 0.6초, 전체 공격 0.8초를 유지한다. MeleeAttack/Controller 판정 코드는 변경하지 않는다. Animator.applyRootMotion=false를 유지하고 CharacterController가 이동을 소유한다. 캐릭터 크기·좌석·카메라 구도·시야각·입력 바인딩은 변경하지 않는다.

애니메이션 상태 선택/블렌드는 CombatView, 조준의 추가 회전은 WeaponAimView, 눈 기준 표시·메시 선택은 FirstPersonArmsView, 모션 데이터는 클립/마스크가 소유한다. 별도 관리자·상속·인터페이스는 추가하지 않았다. 두 표시 컴포넌트 사이에는 읽기 전용 MeleePoseWeight 계약만 추가했다. 기존 구체 컴포넌트 합성을 유지하며, 판정/입력/표시를 섞지 않는 설계를 선택했다.

## 검증과 남은 확인

사용자의 열린 Editor를 조작하지 않고 Library/CodexInfantryWork/Project에서 검사했다. 최종 전체 검사 116/116 통과(제품 PlayMode 검사 115개 + 임시 렌더 검사 1개). 근거: Library/CodexMeleeClips/complete.xml, complete.log. 최종 근접 표시 값 (0,-0.7,0.25)과 위팔 전체를 포함한 메시로 실행했다. 테스트 픽스처가 이동을 꺼 공중 자세를 사용하던 조건을 발견해 실제 바닥·접지를 구성했다. 원본 장면에는 테스트용 바닥이나 강제 상태를 추가하지 않았다.

양 시점에서 좌우 전방 공격, 원본 몸통 회전, 판정 시점 부근의 전방 도달, ±35도 조준, 캐릭터 루트 회전 불변, 근접용 메시의 실제 재질 슬롯/삼각형 존재, 권총 조준·근접 후 원래 메시 복귀를 검사했다. 기존 전투·생명·이동·입력·탑승 회귀 검사도 포함했다. 준비·좌우 주먹·권총 근접 상태를 양 시점에서 렌더한 PNG 8개를 확인했다. 반영 후 스크립트/컨트롤러 객체/새 에셋이 검증본과 일치하고, 씬 변경은 두 필드뿐이며, 새 GUID 중복이 없음을 확인했다. 기존 권총 메시/마스크와 MeleeAttack/Controller 판정 파일은 보존했다.

중간 전체 검사에서는 이전 조사에서도 관측된 Invalid worldAABB가 입력 테스트에서 간헐적으로 발생했다. 해당 로그를 무시하거나 LogAssert.Expect로 숨기지 않았다. 최종 전체 실행에서는 재현하지 않았다. 이 오류의 원인은 확정하지 않았으며, 이번 모션 교체만으로 해결되었다고 주장하지 않는다. 실패 기록 full.xml, verified.xml, full.log 등을 Library/CodexMeleeClips에 보존한다.

실제 Editor에서는 Play를 재시작하고 2번 슬롯의 주먹 공격, V 시점 전환, 상하 조준, 이동/앉기/점프 중 공격, 권총 근접 후 조준 복귀를 확인해야 한다. 캡처된 정지 화면과 정량 검사는 동작의 자연스러움에 대한 사용자 평가 전체를 대신하지 않는다. 전용 1인칭 모션이나 전용 pistol-whip 모션을 확보한 것으로 표기하지 않는다.
