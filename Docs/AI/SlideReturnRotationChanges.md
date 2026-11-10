# 슬라이드 복귀 중 옆 회전 수정

## 재현과 원인

직전 apply-applied.json의 12개 관련 파일/원본 해시가 모두 동일했다. 추가 사용자/Unity 변경은 없었다. 사용자 보고: 진입 수정 이후에도 복귀 시 옆으로 돌아오는 동작이 남아 있음.

직전 합성기의 공유 회전 곡선 RootQ/LeftFootQ/RightFootQ도 각 스칼라 성분으로 Mathf.Lerp했다. 두 원본은 같은 자세를 나타낼 때도 쿼터니언 부호가 반대일 수 있다. 이를 그대로 혼합하면 중간 회전 경로가 잘못되고 크기가 작은 회전값 근처를 통과한다. 이전의 마지막 20% 곡선 보존/완성 애셋과 승인 비교본 일치 검사는 이 연결 오류를 발견하지 못했다.

같은 클립/프로필/시간축/상체/접지를 유지한 단일변수 실험에서 회전 성분만 shortest-path Slerp로 바꾸자 큰 옆 회전이 제거되었다. 고정 .02초 샘플에서 복귀 19–30프레임 골반 인접 회전 최대 61.56°→16.48°, 누적 285.84°→109.39°. 누적 값에는 정상적인 일어서기 기울기도 포함되므로 yaw 총량으로 해석하지 않는다. 신규 회귀 검사는 이전 애셋에서 실패(RootQ p=.50833 기대<.1°, 실제 .56786°)했다.

## 변경 파일과 구체적인 교체

| 파일 | 변경 / 필요 이유 |
|---|---|
| `Assets/Features/PlayerCharacter/Editor/ForwardSlideClipComposer.cs` | 공유 Q 곡선은 4성분을 함께 읽어 정규화하고 entry/source Dot<0일 때 entry 부호를 정렬한 뒤 Quaternion.Slerp한다. 기존 .8 원본 키 경계와도 같은 부호를 유지한다. `RotationCurves`, `EvaluateRotation` private helper 추가. 위치/근육 곡선의 기존 Mathf.Lerp, .5–.8 연결 시간, 원본 .8–1 키/접선 복사, 데이터/생성 메뉴 계약은 유지. |
| `Assets/Tests/PlayMode/SlideAnimationOverrideTests.cs` | 기존 외부 진입 스칼라 비교에서 Q곡선을 제외하고 신규 `ForwardSlideReturnUsesTheShortestQuaternionPathInsteadOfSpinningSideways`의 각도 비교로 교체했다. q와 -q는 성분이 달라도 같은 자세이므로 성분 동일성이 적절한 계약이 아니었다. Root/양발3그룹의 진입 회전과 복귀36표본이 Unity shortest-path Slerp와 .1° 이내인지 검사. 원본 마지막20% 전체 곡선101표본 및 근육/위치 진입 검사는 유지. `ReadRotation` 테스트 helper 추가. |
| `Assets/ThirdParty/MixamoLocal/RunningSlideForwardEntry.anim` (Git 제외) | 같은 GUID의 파생 클립을 올바른 회전 보간으로 재생성. 프로필/소스 FBX/원본 Quaternius/메타 변경 없음. |
| `Docs/AI/SlideReturnRotationChanges.md` | 이 보고서. |
| `Docs/AI/UnityProjectContext.md` | 기존 보간 오류 원인과 현재 구현/검증 결과 추가. |

```cs
// 이전: 쿼터니언도 x/y/z/w를 각각 스칼라 혼합
value = Mathf.Lerp(entryComponent, sourceComponent, blend);
// 이후: 회전을 함께 보간하고 출력 곡선 부호를 원본 경계에 정렬
if (Quaternion.Dot(start, target) < 0f)
    start = new Quaternion(-start.x, -start.y, -start.z, -start.w);
value = Quaternion.Slerp(start, target, blend)[component];
```

삭제한 메서드/필드/컴포넌트/애셋 없음. 회전 곡선에 대한 성분별 보간 경로와 회전 성분 동일성 검사만 대체했다. 마지막 원본 구간을 다른 애니메이션으로 교체하지 않았으며, 새 임의 yaw/관절 자세 보정도 없다. 입력C/.75초/이동/종료서기/천장안전/카메라/씬/무기 동작은 유지한다.

## 구조와 원칙

기존 Editor 클립 합성 책임 안에서 회전 데이터 처리를 고친다. private helper가 곡선 읽기/회전 해석을 캡슐화하며 외부 계약 Compose/Apply는 유지한다. 런타임 manager/component/상속/인터페이스 추가 없음. 원본 유지와 기존 SO 주입/재생 구조 그대로이며 OOP/SOLID상 의도적인 예외 없음.

## 검증

- Unity 6000.4.8f1 격리 프로젝트 `-batchmode -noaudio`: 전체 Play Mode **186/186 통과**, 180.46초. 제품 검사185개+임시 캡처1개, 실패/스킵0. 새 회귀 검사는 기존 애셋에서 실패(red.xml), 재생성 이후 통과(final.xml).
- 소총/권총/맨손 각44프레임7골격 표본과30장 캡처 수집. 세 상태 모두 복귀19–30프레임 최대 인접 골반회전16.48°로 같은 결과. 렌더에서 기존 큰 옆 회전 제거 확인. 진입 자세·마지막20% 원본곡선101표본/curve, 전체기존 장전/점프/탑승/전투 회귀 검사 통과.
- 검증한 애셋/두 코드 파일을 바이트 동일하게 반영. 원본 FBX/meta, 파생 애셋meta/GUID, 프로필/씬/컨트롤러/Quaternius 원본 해시 유지. 임시 진단은 Library에 보관하고 제품 Assets에서 제거. 커밋/스테이징 없음.
- 사용자 Editor에서 SampleScene Play를 다시 시작해 달리기+C 복귀 자세 확인 필요. 극단 조준/1인칭 세부 체감/경사면과 대상 Player 빌드는 별도 검증하지 않음. 이번 결과가 모든 동작의 자연스러움을 보증하는 것은 아님.
- 증거: `Library/CodexSlideReturn/{before.json,quaternion.xml,red.xml,rebake.log,final.xml,final.log,applied.json}` 및 `Library/CodexSlideDirections/mode9*`, `mode10*`, `mode11*`.
