# 슬라이드 하체 전용 재생

## 변경 전 비교와 선택한 구조

직전 원본 복원 작업의10개 파일 해시와 현재 파일을 비교했고 추가 사용자/Editor 변경은 없었다. 공용 controller도 직전 테스트 프로젝트와 동일했다. 대상10개 파일을 before.json으로 보존하고 제품 반영 직전 재확인했다. 이전 방식은 원본 전체 클립을 기본 SlideLoop 상태에서 재생해 Root/몸통 회전까지 포함했다. 사용자 요청대로 원본을 손으로 편집하는 대신 별도의 AvatarMask+Animator override layer로 다리 채널만 혼합한다.

기존4개 레이어는 변경/재배치하지 않고 마지막에 LowerBodySlide를 추가(4→5, 새 index4)했다. 실제 원본 FBX/임포트 meta/키프레임/1개 전체 클립0–46frame/Original 회전 설정은 유지한다. Root/몸통은 하체 마스크에서 제외되므로 실제 상체는 원본의 몸통 모션이 아니라 기존 앉기/무기 상체 모션으로 평가된다. 시점/입력C/0.75초/종료 서기/천장 안전 검사는 유지한다.

## 변경 파일과 구체적인 값

| 파일 | 구체적인 변경 |
|---|---|
| `Assets/Features/PlayerCharacter/Animation/SlideLowerBody.mask` **신규** | Humanoid LeftLeg/RightLeg만true. Root/Body/Head/양팔/손가락/모든 IK 그룹false. transform path 수동 편집 없음 |
| `Assets/Features/PlayerCharacter/Animation/SlideLowerBody.mask.meta` **신규** | 새 마스크 GUID. controller가 이 GUID로 참조 |
| `Assets/Features/PlayerCharacter/Animation/PlayerCharacter.controller` | 마지막에 LowerBodySlide override layer, 기본weight0, IKpass=false, 전용mask. default state Slide, placeholder=기존 Quaternius Slide_Loop(로컬 프로필이 원본 전체clip으로 override), timeParameterActive=true/timeParameter=SlideTime. 새 Float SlideTime(default0) 추가. 기존 레이어/상태/전환 문서 유지: 기존 문서 중 controller 본문만 변경, 새 stateMachine/state 문서2개 추가 |
| `Assets/Features/PlayerCharacter/Scripts/SlideAnimationSet.cs` | `_lowerBodyOnly` bool과 읽기 전용 LowerBodyOnly 추가. 완전한 단일클립(IsContinuous)일 때만 하체 모드 활성화. 기존 필드/전체모션 모드 보존 |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterAnimationView.cs` | 하체 모드에서 TraversalMode0으로 기본 전신 슬라이드를 재생하지 않고 기존 crouch locomotion을 유지. TraversalTime/SlideTime에 전체 progress 전달. SlideTime은 종료 후 초기화하지 않아 레이어가 사라질 동안 마지막 자세 유지. 기존 전체clip mode2/legacy3단계/vault 로직 보존 |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterSlideAnimationView.cs` | 명시적 `_traversal` 참조 추가, Initialize3인자→4인자. LateUpdate의 기존 Apply 호출에 UpdateLowerBody 추가. sliding+LowerBodyOnly이면 targetweight1, 그 외0. MoveTowards 속도12.5/s(완전 혼합 약0.08초). Initialize/OnDisable/OnDestroy에서 ResetLowerBody로weight0. 기존 이벤트/캐시/controller ownership 유지 |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterPostureView.cs` | 기존 접지 계산의 실행 조건에 LowerBodySlide weight>.001인 혼합 구간을 포함. 이동 종료 flag가 꺼져도 하체 레이어가 완전히 빠질 때까지 같은 접지 계산 유지. 새로운 높이/본 회전 상수 없음 |
| `Assets/Features/PlayerCharacter/Editor/LocalSlideAnimationSetup.cs` | 이미 순회 중인 traversal을 Initialize의 네 번째 인자로 공급. 다른 주입/탑승 목록/씬 정책 유지 |
| `Assets/Features/PlayerCharacter/Editor/MixamoSlideImporter.cs` | 새 메뉴 Use Running Slide Lower Body: 전체클립 여부 확인 후 프로필 flag=true. 새 일반 Import 프로필도 하체 모드로 생성, Restore Original 명령은 false로 전신 비교 가능. 원본 clip/회전/컷을 수정하는 추가 보정 없음 |
| `Assets/ThirdParty/MixamoLocal/RunningSlide.asset` **로컬·Git 제외** | `_lowerBodyOnly:1` 추가. 기존3개의 원본 전체clip 참조/원본키/GUID 유지 |
| `Assets/Tests/PlayMode/PlayerCharacterTraversalAnimationTests.cs` | 하체 프로필은 기본TraversalMode1/2/3 미사용 검사. 기존 전체clip/legacy 프로필 검사는 유지. 실제 mesh 접지 probe를44frame으로 늘리고 종료 혼합을 샘플링해 ±.15m 경계/실제 샘플 존재 검증 |
| `Assets/Tests/PlayMode/SlideAnimationOverrideTests.cs` | Initialize 호출4인자로 변경. AvatarMask 모든 부위 허용 경계, 전용 레이어 존재, 슬라이드 중weight>.95/기본mode0/기존 UpperCombat 활성, 종료weight<.01 검증. fadeout 중 SlideTime이 되감기지 않는지와 실제 fadeout 경로를 검증. 원본clip/프레임/RootMotion/장비/종료 기준 유지 |
| `Docs/AI/SlideLowerBodyChanges.md` | 이 기록 |
| `Docs/AI/UnityProjectContext.md` | 최신 하체 전용 사용/구조/검증 추가 |

## 재생 전후

```cs
// 이전: 원본 전체가 기본 레이어에 적용
mode = 2; time = progress;
// 하체 전용: 기본 자세 + 다리 레이어
mode = 0; time = progress;
animator.SetFloat(SlideTime, progress); // 다리 레이어만 읽는 시간
// 슬라이드 중 weight → 1, 종료 시 weight → 0
```

기본 레이어가 앉기 자세를 만들고, 원본의 양다리 muscle 채널만 override된다. 무기 그립/조준/발사/장전은 기존 상체 레이어/IK가 계속 담당한다. 맨손은 기존 맨손 상체 동작을 따른다. 높이/접지는 기존 PostureView 처리이며 이번에 새 본 회전/위치 보정 코드를 추가하지 않았다. 무릎/다리 동작을 별도 hand-authored animation 파일로 생성하지 않았다.

## 제거·교체 내역

기존 파일/컴포넌트/필드/클립/상태 삭제 없음. 하체 모드에서만 이전의 전신 SlideLoop 선택을 기본mode0+새 layer로 대체. Initialize signature와 그 호출부4곳을 명시적인 traversal 참조로 갱신. LateUpdate/OnDisable/OnDestroy에 해당 layer lifecycle 처리를 추가. 초기 하체 후보의 공용TraversalTime 연결은 전용SlideTime으로 교체하여 종료 시 time0이 fadeout에 섞이지 않게 했다. 원본 전체 사용을 위한 IsContinuous/3분할 fallback과 Restore Original 메뉴는 비교/호환용으로 유지한다.

## 구조/원칙 검토

AvatarMask는 부위 선택 데이터, SO는 재생 모드, AnimationView는 상태/시간, 기존 slide presenter는 controller와 해당 layer의 혼합/수명을 소유한다. 레이어 혼합은 기존 slide presentation과 같은 수명/의존성을 가져 별도 manager/component를 만들지 않았다. traversal 의존성은 초기화 인자로 전달한다. Stance/Movement/Camera/Combat 정책을 수정하지 않았다. 상태를 읽기 전용 프로퍼티로 노출하고 asset 구성+기존 컴포지션을 사용했다. 추가 상속/인터페이스는 불필요하며 OOP/SOLID상 의도적 예외 없음.

## 사용/검증

- SampleScene 달리기 중 C. 로컬 RunningSlide.asset의 Lower Body Only=true. false로 바꾸면 이전 전신 전체클립 모드와 비교할 수 있다. 0.75초 이동/종료 서기 유지. 기존1.533초 소스는 이전과 같이 게임0.75초에 맞춰 재생한다.
- Unity6000.4.8f1 격리 프로젝트 `-batchmode -noaudio`, 최종 **183/183 통과** (179.3초): 제품182개+진단 캡처1개. 종료 접지 조건 추가 후 최종 전체 검증을 재실행. 최초 집중48/48, 종료 time 분리 전 후보도 전체182/182 통과. 최종 코드는 종료 time 분리 후 다시 전체 검증했다.
- 외부 Mixamo 폴더 실제 분리 fallback **8/8 통과**. 원본 파일/FBXmeta 해시 동일, 임포트 회전/아바타 설정 변경 없음. 제품 복사12개 코드/설정/로컬 파일이 테스트 프로젝트와 바이트 동일.
- 소총/권총/맨손 × TP/FP, 종료 혼합 frame38 포함 총48장 캡처 검토. 상체가 원본의 큰 Root 회전을 따라가지 않고 기존 무기 상태를 유지하는지 확인. 종료 혼합 접지 조건 보완 후 frame38도 재캡처/검사했다. 테스트는 부위 마스크 경계/혼합 수명과 기존 모터·접지·장전·조준·탑승 회귀를 검사하며 특정 게임의 외형과 동일함을 보증하지 않는다.
- 임시 Editor 도구/캡처 코드는 Library에 보관, 제품에 추가 씬/컴포넌트/진단 로그 없음. 사운드 값/영구 audio 설정/커밋/스테이징 변경 없음. 새 mask는 원본 애니메이션 재배포가 아닌 선택 데이터이며 기존 Mixamo 원본·임포트 결과 Git 제외 정책 유지.
- 남은 확인: 사용자 Editor에서 원하는 하체 자세/상하체 연결 느낌, 극단 조준·경사·머리 위 장애물 전환. 기본FP carry 화면을 캡처했으며 모든 ADS/자유 시점 상황을 영상으로 확인한 것은 아니다. Player 빌드는 실행하지 않았다.

증거 `Library/CodexSlideLowerBody/{before.json,focused.xml,final.xml,final2.xml,final3.xml,fallback2.xml,contact.png,applied.json,lower_slot*.png}`.
