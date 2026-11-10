# 다운로드한 원본 슬라이드 모션 복원

사용자는 직전 방향 보정이 더 이상해졌다고 보고했고, 다운로드한 애니메이션 그대로 사용할 것을 요청했다. 이전의 방향 정렬/게임 모방 보정을 성공으로 간주하지 않는다. 직전 applied.json 비교에서 사용자/Editor 추가 변경 없음, 대상8개 파일 before.json 보존/반영 직전 해시 재확인.

## 파일과 변경

| 파일 | 변경/이유 |
|---|---|
| `Assets/Features/PlayerCharacter/Editor/MixamoSlideImporter.cs` | 0–11.5/11.5–36.8/36.8–46의 세 구간 생성 코드를 제거하고 source take 전체(firstFrame–lastFrame), `RunningSlide` 하나 생성. lockRootRotation/keepOriginalOrientation false→true로 복원. 명시적 메뉴 `Tools/Battlefield/Animations/Mixamo/Restore Original Running Slide` 추가. 일반 Import는 기존 사용자 설정 보존, Restore는 전체 클립/회전/프로필을 명시적으로 복원 |
| `Assets/Features/PlayerCharacter/Scripts/SlideAnimationSet.cs` | `IsContinuous` 읽기 전용 프로퍼티 추가. Start/Loop/Exit가 동일한 유효 클립이면 전체 연속 재생 데이터로 판별. 기존 필드/3단계 데이터 호환 유지 |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterAnimationView.cs` | optional `_slideAnimations` 명시 데이터 참조와 `SetSlideAnimations` 추가. IsContinuous일 때 mode2/SlideLoop 한 상태에서 TraversalTime=전체 Progress. 기존 모션 데이터는 이전3단계 로직 유지 |
| `Assets/Features/PlayerCharacter/Editor/LocalSlideAnimationSetup.cs` | 기존 view/profile 연결 직후 presentation.SetSlideAnimations(profile) 호출. Play/빌드 씬 복사본에 같은 데이터 공급. 원본 SampleScene 변경 없음 |
| `Assets/Tests/PlayMode/PlayerCharacterTraversalAnimationTests.cs` | 로컬 전체클립은 mode2만 재생하고 정상 종료 서기 검증, 외부파일 없는 기존 데이터는 이전1/2/3단계 검사 유지. 메서드 이름을 configured timeline으로 변경 |
| `Assets/Tests/PlayMode/SlideAnimationOverrideTests.cs` | 직전 HasLocalMotion helper와 mid-slide35도 heading 검사 제거. 대신 전체 source frame range/단일 임포트/원본 방향 설정/3참조 동일성/TraversalTime=Progress 연속성 검사. 기존 루트 이동/장비/종료 검사 유지 |
| `Assets/ThirdParty/MixamoLocal/RunningSlide.fbx.meta` **Git 제외** | clipAnimations3→1, 이름RunningSlide, frames0–46. loopBlendOrientation0→1/keepOriginalOrientation0→1. Body Orientation 기준 재해석/회전 추출 제거. XZ RootMotion 추출/Y pose 보존/Avatar/scale/compression 등 기존 설정 유지 |
| `Assets/ThirdParty/MixamoLocal/RunningSlide.asset` **Git 제외** | _start/_loop/_exit 모두 전체 RunningSlide subasset(fileID -203655887218126122) 참조. 기존 _start와 동일 ID가 전체클립 ID로 재사용됨. _loop의 이전4061839508520268033, _exit의 이전-2061910772549006292 참조는 교체. SO/FBX GUID, 원본 Quaternius 키3개 유지 |
| `Docs/AI/OriginalSlideRestoreChanges.md` | 이 보고서 |
| `Docs/AI/UnityProjectContext.md` | 사용자 최신 원본 우선 요구와 이전 보정의 철회 기록 |

## 재생 차이

```cs
// 전체 클립일 때: 한 상태/한 시간축
mode = 2;
time = progress;
// 이전의3단계 로컬 재생 시간 리셋은 이 데이터에서 사용하지 않음.
```

원본0–46프레임(30fps, 약1.533초) 전체를 한 번 재생한다. 기존 이동 길이0.75초는 유지하므로 게임에서의 재생 시간은0.75초다(네이티브1.533초 속도와 동일하다는 뜻이 아님). 원본 FBX/근육/관절 키프레임 바이트는 변경하지 않았다. 원본 회전을 pose에 유지하며 임의 heading offset/Body Orientation correction/새 procedural bone pose를 적용하지 않는다. 기존 무기 상체 그립·조준·접지 연동은 유지하므로 무기를 든 실제 캐릭터의 전신 결과를 원본 preview와 완전히 동일하다고 주장하지 않는다. C입력/종료 후 일어서기/천장 안전 검사 유지.

## 제거·교체 내역

기존3개 분할 FBX subasset은 전체1개로 대체하고 사용하던 프로필 참조를 갱신했다. 분할 생성문, Body Orientation(false/false) 방향 보정,35도 제한 assert/HasLocalMotion helper를 제거했다. 원본에 들어있는 몸 방향을 오류로 간주해 다시 정렬하지 않는다. 이 요청에 맞춰 검증 계약을 원본 전체 사용/연속 재생으로 바꾼 것이며, 방향 개선을 테스트 통과로 주장하지 않는다. 기존 runtime field/asset 파일/컴포넌트 삭제 없음. legacy3단계는 Quaternius fallback/기존 데이터 호환용으로 유지한다.

## 구조 검토

클립 데이터는 기존 SO, 재생 시간은 기존 AnimationView 한 곳, 로컬 공급은 Editor setup, 임포트는 기존 importer가 담당한다. optional 명시 데이터 주입으로 runtime에 Mixamo 경로나 AssetDatabase 의존성을 넣지 않았다. 추가 manager/class/인터페이스 없이 기존 책임 안에 작은 분기를 추가했다. 상태는 읽기 전용 IsContinuous로 노출하며 구성만 바꿔 전체/기존3단계를 선택한다. OOP/SOLID상 의도적 예외 없음.

## 검증

- Unity6000.4.8f1 격리 프로젝트 `-batchmode -noaudio`: 관련 검사+캡처 **48/48 통과**, 41.6초. 로컬 Mixamo 분리 fallback **8/8 통과**.
- source frames0–46 그대로/단일 Humanoid clip/모든 replacement 동일/Original 방향(true,true)/전체 진행률 연속 재생, 기존 이동·접지·종료 서기·장비 전환·탑승·전투 회귀 검사. 소총/권총/맨손21장 실제 캡처 검토.
- 원본 SHA256 07bfb606c06cc696252fb027c4a2075330b200f886304a8e9f6674ef18c7321d 유지. 적용8개 코드/로컬 파일이 검사 프로젝트와 바이트 동일. 추가meta/컴포넌트/제품 씬 없음. 임시 캡처 코드는 Library 보관. 기존 Git 제외 정책/오디오 설정 유지. 커밋/스테이징 없음.
- 전체182 검사는 이번 복원에서 재실행하지 않았다. 직전 기준 전체182/182 통과 기록 이후 영향 범위 검사를 수행했다. Player 빌드/실제 사용자 Editor 화면/1인칭·복잡한 지형 체감은 추가 확인 필요. 사용자에게 이 원본 자세가 원하는 동작인지 확인하도록 안내한다.

증거: `Library/CodexOriginalSlide/{before.json,import.log,focused.xml,fallback.xml,applied.json,original_slot*.png}`.
