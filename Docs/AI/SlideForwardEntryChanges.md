# 승인된 전방 슬라이드 진입 적용

## 변경 전 비교와 승인

직전 SlidePose applied.json의 7개 파일 해시가 모두 동일했다. 임포터, 프로필, 원본 FBX/meta, Quaternius 진입 클립도 비교 프로젝트와 동일했다. 추가 사용자/Unity 변경이나 충돌은 발견하지 않았다. 사용자가 진입 교체 비교본 적용을 승인했다.

## 파일별 변경과 교체

| 파일 | 구체적 변경 / 이유 |
|---|---|
| `Assets/Features/PlayerCharacter/Editor/ForwardSlideClipComposer.cs` (신규) | Editor 전용 클립 생성. `Compose(original, entry)`가 비교본과 동일한 원본 곡선 합성을 수행한다. `Apply()`/`Tools > Battlefield > Animations > Mixamo > Use Forward Slide Entry`로 로컬 파생 애셋 저장 및 기존 프로필 참조 교체. 재생성 시 CopySerialized로 기존 GUID를 보존한다. 원본 클립을 프로필에서 역추적하지 않고 보존된 FBX에서 명시적으로 읽어 재생성 누적을 방지한다. |
| `Assets/Features/PlayerCharacter/Editor/ForwardSlideClipComposer.cs.meta` (신규) | Unity가 생성한 새 Editor 스크립트 GUID. |
| `Assets/Tests/PlayMode/SlideAnimationOverrideTests.cs` | `OptionalLocalProfileIsAppliedWhenPresentAndNotRequiredWhenMissing`에서 활성 클립을 ModelImporter로 단정하던 코드를 보존된 원본 FBX 경로 검사로 교체했다. 활성 클립이 .anim인 경우에도 원본 전체 프레임/방향 설정 검증을 유지한다. `ForwardEntryUsesExternalMotionAndRetainsTheOriginalExitCurves` 추가: Humanoid/원본 길이, 외부 진입 곡선 실제 사용, 마지막 20%의 101개 표본이 원본과 .0001 이내인지 검사. |
| `Assets/ThirdParty/MixamoLocal/RunningSlideForwardEntry.anim` (신규, Git 제외) | 승인된 비교본을 저장한 단일 Humanoid 클립. 원본 복제로 이벤트/설정/진입 클립에 없는 곡선을 보존한다. 공유 곡선은 진행률 0–.5에서 Quaternius Slide_Start 전체를 재생, .5–.8에서 끝 자세를 유지하며 SmoothStep으로 같은 시간의 Mixamo에 연결, .8 이후 원본 키/접선을 보존한다. 경계 접선은 비교본과 동일한 수치 미분을 사용하며 원본 곡선과 허용오차 검사한다. 직접 관절 자세를 제작하거나 임의 yaw를 상쇄하지 않는다. |
| `Assets/ThirdParty/MixamoLocal/RunningSlideForwardEntry.anim.meta` (신규, Git 제외) | Unity가 생성한 파생 클립 GUID. |
| `Assets/ThirdParty/MixamoLocal/RunningSlide.asset` (Git 제외) | `_start`, `_loop`, `_exit` 모두 FBX 내 RunningSlide(fileID -203655887218126122) → 새 RunningSlideForwardEntry.anim(fileID 7400000) 참조. 원본 Quaternius 키 3개, `_lowerBodyOnly=1`, 프로필 GUID 유지. 세 참조가 같아 기존 IsContinuous 재생을 그대로 사용한다. |
| `Docs/AI/SlideForwardEntryChanges.md` (신규) | 이 변경 보고서. |
| `Docs/AI/UnityProjectContext.md` | 승인된 모션 선택/현재 구성과 검증 기록 추가. |

```cs
// 이전: 세 단계 모두 원본 전체 클립
_start = _loop = _exit = RunningSlide;
// 이후: 세 단계 모두 승인된 합성 클립, 시간축 재시작 없음
_start = _loop = _exit = RunningSlideForwardEntry;
```

기존 메서드/필드/컴포넌트/파일 삭제 없음. 교체한 것은 프로필의 재생 참조 3개와 테스트의 활성 클립 임포터 경로 가정이다. 원본 FBX/임포트 설정, 원본 Quaternius 애셋, 컨트롤러/마스크/씬/입력/카메라/무기 재생 코드와 계층은 수정하지 않았다. C 달리기 슬라이드, 0.75초, 종료 후 서기 및 천장 안전 검사 유지. 원본 전체 길이 약 1.533초를 기존 .75초 게임 시간으로 재생하는 기존 리타이밍은 유지된다.

## 구조 검토

클립 합성/저장은 별도 Editor 도구가 담당하며, 게임 상태·입력·재생 컴포넌트를 확장하지 않는다. 기존 SlideAnimationSet과 명시적인 클립 입력을 활용한다. 생성 비용은 Editor에서만 발생한다. 추가 상속/인터페이스/manager 없이 책임과 가변 상태를 기존 경계 안에 유지한다. OOP/SOLID상 의도적 예외 없음. 원본 복원은 기존 Restore Original Running Slide 메뉴로 가능하며 일반 Import는 기존 프로필 선택을 보존한다. 새 체크아웃에서 적용하려면 로컬 원본을 Import한 다음 Use Forward Slide Entry 메뉴를 실행한다. 기존 Git 제외 정책 유지.

## 검증

- Unity 6000.4.8f1 격리 프로젝트 `-batchmode -noaudio` 전체 Play Mode **185/185 통과**, 178.1초. 제품 검사 184개 + 임시 캡처 검사 1개. 컴파일 오류/실패/스킵 0개. 소총·권총·맨손 자세, 종료 후 서기/천장 안전, 기존 장전·점프·무기 전환·탑승 검사 포함.
- 생성된 애셋의 실제 렌더와 승인된 임시 합성본을 대조했다. 44프레임 × 7개 골격 위치의 최대 차이 약 3.6mm. 마지막 원본 곡선 표본 및 Humanoid/길이 검사 통과. 이것이 주관적인 자연스러움 전체를 보증하지는 않는다.
- 적용한 스크립트/테스트/메타/로컬 생성 애셋과 검사 프로젝트의 바이트 동일성 확인. 프로필 diff는 세 참조 교체만 있다. 원본 FBX/meta와 Quaternius 진입 클립 해시 유지. 임시 진단 스크립트는 검사 후 Library로 이동했으며 제품 Assets에 포함하지 않았다.
- 실제 사용자 Editor에서 `SampleScene` Play → 소총/권총/맨손 각각 달리기 중 C로 전환 체감 확인 필요. 1인칭·극단 조준·경사면 및 대상 Player 빌드는 이번에 별도로 검증하지 않았다.
- 증거: `Library/CodexSlideDirections/{apply-before.json,apply-isolated.log,applied-suite.xml,applied-suite.log,mode7*.png,mode7.json,apply-applied.json}`. 커밋/스테이징 없음.
