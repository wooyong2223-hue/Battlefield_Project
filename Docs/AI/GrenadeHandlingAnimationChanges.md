# 수류탄 장착·대기 애니메이션 제작 (2026-11-04)

## 탐색 결과와 제작 범위

무료이며 장착·대기·던지기를 제공하고 Unity에서 이용 가능한 원본 세트는 이번 검색에서도 확인하지 못했다. 무료 원본이 세상에 없다는 결론은 아니다.

- https://discussions.unity.com/t/1st-person-animation-set-s-59-and-counting-100-free/556214 : 수류탄 8종은 Upcoming 목록. 확보된 무료 세트로 취급하지 않았다.
- https://powstudio.itch.io/grenade-flash-player-enemy-animations : 장착·대기·던지기 명시, 다운로드는 유료. 구매·다운로드하지 않았다.
- Quaternius·ActorCore·RancidMilk에서는 필요한 무료 장착·대기 원본을 확인하지 못했다. 다른 게임에서 추출한 모션이나 출처/허가가 불명확한 다운로드는 사용하지 않았다.

새 GrenadeEquip/GrenadeIdle은 자체 제작한 Humanoid muscle 키프레임 클립이다. Editor 제작 도구가 .anim에 키를 저장하고, 런타임 Animator가 재생한다. 매 프레임 손/팔 본의 회전값을 코드로 덮어쓰는 시스템은 추가하지 않았다. 던지기는 보유 Mixamo Throw Grenade 원본을 그대로 사용한다. 전용 핀 뽑기·수납·1인칭 전용 모션은 새로 제작하지 않았다.

## 사전 비교

Library/CodexGrenadeEquip/applied.json 및 더 이전 CodexGrenadeMantle/applied.json의 관련 파일을 비교했다. 최신 기록을 우선해 비교했을 때 후속 사용자/Editor 변경은 없었다. 기존의 다른 미커밋 변경은 유지한다. 아래 모든 후보는 격리 프로젝트에서 제작·검증한 뒤 동일 파일을 적용한다.

## 교체 전후

기존 PlayerCharacterGrenadeThrow.Tick:

```csharp
_elapsed = Mathf.Min(_elapsed + deltaTime, _definition.ThrowAnimation.length * .5f);
if (Progress >= .5f) _state = _throwRequested ? Throwing : Ready;
// Ready에서는 시간 진행을 멈춤. 클릭하면 이 지점부터 계속 재생.
```

변경: .6초 Equip 클립 완료 → 2초 Idle 클립 반복 → 클릭하면 기존 Throw Grenade 클립의 0초부터 재생 → 기존 .7 releaseFraction에 실제 발사 → 원래 무기로 복귀. 클릭 예약·취소·누르고 있던 발사 입력 소비·수류탄 수량 규칙은 유지한다. 전체 던지기 원본을 재생하므로 클릭 후 종료 대기시간은 이전보다 길다. 신관은 실제 투척 이후에 시작하는 기존 규칙을 유지한다.

## 변경 파일과 구체적인 내용

| 파일 (저장소 루트 기준) | 변경 내용 및 이유 |
|---|---|
| Assets/Features/Weapon/Scrips/GrenadeDefinition.cs | 기존 _throwAnimation과 모든 전투 값을 유지. _equipAnimation/_idleAnimation, 읽기 전용 EquipAnimation/IdleAnimation/HasHandlingAnimations 추가. 기존 _firstPersonOffset=(-.35,.6,1.2)를 유지하고, 새 _handlingFirstPersonOffset=(-.1,-.4,1.2) 및 HandlingFirstPersonOffset 추가. 중간 던지기 자세용 화면 배치와 새 장착·대기 배치를 구분. |
| Assets/Features/PlayerCharacter/Scripts/PlayerCharacterGrenadeThrow.cs | _equipElapsed 추가, HandlingProgress로 장착 진행·대기 루프 진행을 노출. Progress는 던지기 전용으로 유지. TryEquip은 두 클립이 모두 있어야 시작. Tick의 .5 고정/정지 분기를 별도 장착 시간·반복 대기 시간으로 교체. TryThrow는 Ready에서 _elapsed=0으로 시작. PoseWeight의 진입 페이드는 장착 시간 기준, 장착 뒤에는 1을 유지. 기존 Tick/TryEquip/TryThrow/PoseWeight 내부만 변경하며 메서드·직렬화 필드 삭제 없음. |
| Assets/Features/PlayerCharacter/Scripts/TacticalAnimationSet.cs | _equipPlaceholder/_idlePlaceholder 및 유효성·적용 여부 검사 추가. Read/Restore의 이전 두 슬롯 KeyValuePair 반환/인자를 네 슬롯 배열로 교체. Apply/Restore는 장착·대기·던지기·등반 매핑을 함께 적용/복원. 새 클립으로 바꿔도 기존 런타임 override controller 소유권/복원이 유지되도록 확장. |
| Assets/Features/PlayerCharacter/Scripts/PlayerCharacterTacticalAnimationView.cs | _borrowed의 값 타입을 네 슬롯 배열로 변경. GrenadeHandlingTime과 Equip/Idle/Throw 상태를 실행 중인 수류탄 상태에 맞춰 갱신. 상태 전환은 .12초 CrossFadeInFixedTime. FirstPersonThrowOffset은 장착·대기용 오프셋을 사용하고, 실제 Throw에서 Progress 0.. .5 동안 기존 던지기 오프셋으로 SmoothStep 보간. 기존 등반·무기·카메라 구현은 유지. |
| Assets/Features/PlayerCharacter/Editor/TacticalActionImporter.cs | Import 완료 시 GrenadeHandlingAnimationAuthoring.Ensure 호출 추가. 로컬 Mixamo 자산을 재생성하는 기존 메뉴에서도 새 클립 참조/상태를 연결. 기존 외부 FBX와 임포트 설정 변경 없음. |
| Assets/Features/PlayerCharacter/Editor/GrenadeHandlingAnimationAuthoring.cs | 새 Editor 전용 제작/연결 도구. Tools/Battlefield/Animations/Create Missing Grenade Handling Clips 메뉴. 기존 Armature 임시 인스턴스에서 손목 목표=Chest+(.18,.12,.36)×humanScale, 팔꿈치 힌트=어깨+(.18,-.35,.08)×humanScale로 2관절 해를 구해 Humanoid muscle 키로 저장. IK 계산은 제작 시에만 수행하며 런타임에는 없음. 60fps 곡선과 누락된 참조·상태만 작성. 이미 존재하는 클립과 사용자 참조는 덮어쓰지 않음. 임시 rig/pose handler는 finally에서 정리. |
| Assets/Features/PlayerCharacter/Editor/GrenadeHandlingAnimationAuthoring.cs.meta | Unity가 생성한 새 스크립트 GUID. |
| Assets/Features/PlayerCharacter/Animation/Tactical/GrenadeEquip.anim | 자체 제작 .6초 비반복 클립. 오른팔을 낮게 시작해 .16/.43/.6초 키로 가슴 앞에 드는 자세로 연결. 다리/발의 muscle 곡선을 작성하지 않아 기존 하체 이동은 유지. |
| Assets/Features/PlayerCharacter/Animation/Tactical/GrenadeEquip.anim.meta | 새 장착 클립 GUID. |
| Assets/Features/PlayerCharacter/Animation/Tactical/GrenadeIdle.anim | 자체 제작 2초 반복 클립. 제작용 rig에서 손목·팔꿈치 목표로 구한 자세를 저장. 장착 종료와 같은 기본 자세, Chest Front-Back +.015와 Right Arm Down-Up +.008의 작은 닫힌 루프. 원본 던지기 중간 프레임 정지를 대체. |
| Assets/Features/PlayerCharacter/Animation/Tactical/GrenadeIdle.anim.meta | 새 대기 클립 GUID. |
| Assets/Features/PlayerCharacter/Animation/PlayerCharacter.controller | 기존 GrenadeUpperBody.Throw/Mantle/기타 레이어 유지. Float GrenadeHandlingTime 및 해당 시간 파라미터를 쓰는 Equip/Idle 상태 추가, 제작 클립 참조. |
| Assets/ThirdParty/MixamoLocal/ArmedLocomotion/RifleLocomotion.controller | 같은 파라미터·장착/대기 상태 추가. 소총 상태에서도 동일 클립을 재생하도록 연결. 기존 소총 모션 변경 없음. 기존 Git-ignore 정책 유지. |
| Assets/ThirdParty/MixamoLocal/ArmedLocomotion/PistolLocomotion.controller | 같은 파라미터·장착/대기 상태 추가. 권총 상태에서도 동일 클립을 재생하도록 연결. 기존 권총 모션 변경 없음. 기존 Git-ignore 정책 유지. |
| Assets/ThirdParty/MixamoLocal/TacticalActions/FragGrenade.asset | _equipAnimation/_idleAnimation을 새 .anim에 연결. 기존 projectile/throw/fuse3/damage100/radius6/launch18/up4/release.7/기존 FPS 오프셋 유지. 새 handling FPS 오프셋은 코드 기본값 적용. 로컬 자산으로 유지. |
| Assets/ThirdParty/MixamoLocal/TacticalActions/TacticalActions.asset | _equipPlaceholder/_idlePlaceholder 참조 추가. 기존 등반/던지기 placeholder와 정의 참조 유지. 로컬 자산으로 유지. |
| Assets/Tests/PlayMode/TacticalActionTests.cs | 던지기 원본 전체 재생에 맞춰 완료 관찰 대기 프레임 110→200, 130→220, 190→240. 기존 기능 assertion을 삭제하거나 약화하지 않음. |
| Assets/Tests/PlayMode/GrenadeHandlingAnimationTests.cs | 실제 SampleScene 캐릭터로 별도 Equip/Idle/Throw 상태, 던지기 타임라인 미소비, 2초 루프 경계 손 위치 연속성, 대기 중 수량 유지, 던지기 0초 시작 확인. 별도 씬 추가 없음. |
| Assets/Tests/PlayMode/GrenadeHandlingAnimationTests.cs.meta | 새 테스트 스크립트 GUID. |
| Docs/AI/GrenadeHandlingAnimationChanges.md | 이 변경 보고서. |
| Docs/AI/UnityProjectContext.md | 새 구조·검증·한계 기록 추가. |

## 설계 검토

게임 상태·탄환 생성 권한은 GrenadeThrow, 표시 전환은 TacticalAnimationView, 설정은 GrenadeDefinition, Editor 제작은 Authoring이 소유한다. 기존 컴포넌트를 조합하고 새로운 manager/상속/인터페이스는 추가하지 않았다. 공개 읽기 전용 속성만 늘렸고 데이터는 private SerializeField로 유지한다. 기존 추상 계약을 위반하거나 의도적으로 SOLID를 벗어나는 변경은 없다.

## 검증과 한계

Unity 무음 PlayMode 전체 207/207 통과(캡처 진단 1개 포함), 실패 0개, 시간 195.9236955초. 새 제작 클립·상태 검사 1개와 기존 205개 회귀 검사를 포함한다. TEST_AUDIO 로그에서 실행 전 원래 False / Play 중 True / 종료 후 False 복원을 확인했다. 메인 Editor의 오디오 및 게임 AudioSource·프로필·프로젝트 설정은 변경하지 않았다.

정면/1인칭 캡처를 확인하고 수류탄이 화면 위·아래로 잘리던 기존 오프셋의 부적합을 수정했다. 다만 최종 1인칭 캡처에서도 어깨/상완이 화면 안에 크게 보이는 구도는 남아 있으며, 완성된 FPS 연출로 판단하지 않는다. 캡처는 Library/CodexGrenadeAuthored에만 있으며 제작/캡처 임시 스크립트는 Assets에서 제거한다. 이 검사는 동작의 예술적 완성도를 증명하지 않는다. 장착은 아직 초안이며, 손바닥/임시 수류탄 모델의 정확한 그립과 핀·레버 동작, 이동 중/상하 조준/여러 장비에서의 주관적 자연스러움은 실제 Editor 확인이 필요하다. 기존 소총/권총/맨손 1인칭 팔 렌더링을 공유하며 전용 1인칭 애니메이션 세트를 새로 만든 것은 아니다. 빌드 검증은 이번에 수행하지 않았다.
