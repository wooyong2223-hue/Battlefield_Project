# 포트폴리오 외부 애니메이션 활용 — 착석 반복 모션

2026-10-28. 외부 모션을 활용하고 입력·충돌·전투 판정·상태 전환은 프로젝트 코드로 관리한다. 이번 반영 범위는 조종석 착석 반복 모션이다. 승하차 과정 전체의 애니메이션을 완성한 것은 아니다.

## 외부 모션과 직접 구현의 경계

| 동작 | 현재 모션 출처 | 프로젝트에서 담당하는 기능 |
|---|---|---|
| 서기·걷기·달리기·점프 | 기존 Unity Starter Assets | 입력, CharacterController 이동, 가속·제동, 카메라 |
| 뒤걷기·좌우 이동 | 기존 KayKit CC0 클립 | 실제 속도·방향 기반 블렌드 |
| 앉기·권총 조준·발사·장전 | 기존 Quaternius CC0 클립 | 자세 충돌 검사, 탄창, 발사, 조준 IK, 취소 규칙 |
| 주먹·총을 든 타격 | 기존 Rokoko 복싱 모션을 편집한 클립 | 공격 상태·타이밍·피해 판정, 상체 레이어, 1인칭 표시 |
| 조종석 착석 | **이번 Quaternius Sitting_Idle_Loop** | 탑승 권한, 좌석 추적, 조종석 자세 보정, 그래프 해제·Animator 복구 |

Rokoko 클립의 손가락은 기존 Quaternius 주먹 자세를 결합한 파생물이며, 총 근접 공격은 전용 총 타격 원본이 아닌 복싱 동작 재사용이다. 기존 출처 문서의 설명을 유지한다.

## 변경 전 비교와 보존

작업 시작 시 관련 10개 파일의 SHA256과 복사본을 Library/CodexPortfolioReuse/baseline.json 및 Baseline에 저장했다. 반영 직전에도 원본이 그대로인지 확인한다. 이전 주먹 작업 검증도 통과했다.

SampleScene의 SeatedCharacterPose는 코드 기본값과 다르게 사용자가 조정한 값을 가지고 있다. 다음 실제 씬 값을 보존한다.

```yaml
_animator: {fileID: 1601236109}
_torsoRecline: 30
_thighDirection: {x: 0, y: 0.4, z: 1}
_lowerLegDirection: {x: 0, y: -0.15, z: 1}
_upperArmDirection: {x: 0, y: -0.65, z: 0.45}
_forearmDirection: {x: 0, y: 0.1, z: 1}
```

캐릭터·전투기의 크기, 좌석·카메라 위치, 계층, 입력, 기존 Animator Controller와 포복 구현은 이번에 바꾸지 않는다.

## 변경 파일 전체

경로는 저장소 루트 기준이다. `.meta`는 Unity가 생성한 새 GUID를 그대로 복사한다.

1. `Assets/Features/PlayerCharacter/Scripts/SeatedCharacterPose.cs`
   - `_seatedAnimation`과 재생 객체 `_animationPlayback`, 읽기 전용 `IsUsingAnimation` 추가.
   - `CanPose`: 클립이 지정됐다면 비어 있지 않은 Humanoid 클립이어야 탑승 가능. 미지정은 기존 수동 자세 지원.
   - `BeginPose`: 항상 Animator를 끄던 분기를 클립 유무에 따른 재생/기존 수동 자세 분기로 교체.
   - `ApplyPose()` → `ApplyPose(float deltaTime)`: 저장 자세 복원만 하던 부분을 클립 샘플링 또는 기존 복원으로 교체. 이어지는 골반·몸통·머리·팔·다리 보정과 좌석 위치 추적은 유지.
   - `LateUpdate`: Animator의 시간 모드에 따라 scaled/unscaled deltaTime 전달.
   - `EndPose`: 그래프 해제 후 저장한 뼈 자세와 Animator 활성 상태 복구.
2. `Assets/Features/PlayerCharacter/Scripts/SeatedAnimationPlayback.cs` 및 `.meta` **신규**
   - 내부 전용 `IDisposable` 클래스. 수동 PlayableGraph와 AnimationClipPlayable의 생성·평가·파괴를 소유한다.
   - 재생 중 Animator 활성화, root motion 해제, AlwaysAnimate 적용. 기존 enabled/applyRootMotion/cullingMode는 해제 시 복구.
   - 컨트롤러와 Avatar는 교체하지 않는다. Foot IK/Playable IK는 끈다. 매 프레임 객체 할당 없이 평가한다.
3. `Assets/ThirdParty/Quaternius/Animations/SeatedIdle.anim` 및 `.meta` **신규**
   - 공식 CC0 Standard 아카이브의 `Unity/UAL1_Standard.fbx` → `Armature|Sitting_Idle_Loop` 추출.
   - 길이 1.666667초, 반복 재생. 기존 Quaternius 클립과 같은 Humanoid RootQ/RootT Y축 180도 방향 보정.
   - 전용 조종사 모션이 아닌 일반 앉기 모션이며, 프로젝트의 기존 조종석 보정을 위에 적용한다.
4. `Assets/Scenes/SampleScene.unity`
   - SeatedCharacterPose(fileID 950000014)에 `_seatedAnimation` 참조 **하나만** 추가.
   - GUID `165295b01d732b747afc7ca741ae9157`, fileID 7400000.
5. `Assets/Tests/PlayMode/PlayerVehicleBoardingTests.cs`
   - 기존 탑승 검사에서 Animator 비활성 기대를 활성 + `IsUsingAnimation` 검사로 교체. 나머지 좌석·조종사·스케일·이동/선회/파괴·하차 검사 유지.
6. `Assets/Tests/PlayMode/PlayerCharacterSeatedAnimationTests.cs` 및 `.meta` **신규**
   - 모션 변화/일시정지/골반 좌석 고정, 하차 후 Animator 설정 복구 및 재탑승, 클립 미지정 수동 자세, 잘못된 클립 탑승 거절의 네 가지 실제 씬 검사.
7. `Assets/ThirdParty/Quaternius/SOURCE.md`
   - 새 앉기 모션의 원본·길이·방향 보정·재사용 방식 기록 추가.
8. `Docs/AI/PortfolioExternalAnimationChanges.md` **이 문서 신규**
9. `Docs/AI/UnityProjectContext.md` — 현재 착석 구현과 검증 기록 추가.
10. `Docs/AI/PlayerCharacterFeatureRoadmap.md` — 외부 에셋 방침·이번 완료 범위·남은 모션 기록 추가.

## 교체한 코드와 동작

```csharp
// 이전: Animator를 정지시키고 탑승 순간의 자세에서 고정된 착석 자세 구성
_animator.enabled = false;
// ApplyPose() 내부
RestoreBonePose();

// 이후: 외부 반복 모션을 평가한 뒤 같은 조종석 보정 적용
if (_seatedAnimation != null)
    _animationPlayback = new SeatedAnimationPlayback(_animator, _seatedAnimation);
else
    _animator.enabled = false;
// ApplyPose(float deltaTime) 내부
if (IsUsingAnimation) _animationPlayback.Sample(deltaTime);
else RestoreBonePose();
```

삭제한 제품 메서드·필드·컴포넌트·에셋은 없다. `ApplyPose` 서명과 위 두 동작 분기를 교체했다. 기존 보정은 팔·다리 위치를 유지하므로 외부 모션의 모든 움직임이 그대로 표시되지는 않는다. 가슴·머리 등의 작은 반복 움직임이 살아나는지 검사했다.

## 구조 검토

좌석 공간 보정은 SeatedCharacterPose, 재생 리소스 수명은 SeatedAnimationPlayback으로 분리했다. 클립은 직렬화된 명시적 의존성이다. 재생 객체와 상태는 외부에서 수정할 수 없고 확인용 속성만 공개한다. IDisposable은 실제 해제 계약이며 별도 상속 계층이나 한 가지 구현만을 위한 인터페이스를 추가하지 않았다. 기존 차량/입력/이동 책임은 확장하지 않았다. OOP/SOLID에 대한 의도적인 예외는 없다.

## 검증과 한계

- 격리 Unity 6000.4.8f1에서 착석·탑승·전투기 크기·시동 관련 14/14 통과: `Library/CodexPortfolioReuse/seating.xml`.
- 가슴/머리 회전 변화 약 3.901도, 골반-좌석 오차 0.002m 미만, timeScale 0에서 모션 정지 확인.
- 최종 제품 PlayMode **119/119 통과** (기존 115개 + 새 착석 검사 4개). 결과: `Library/CodexPortfolioReuse/final.xml` / `final.log` 참조.
- 조종석 및 기체 렌더러를 테스트 중에만 숨긴 조종사 화면을 각각 렌더 확인했다. 현 기체의 불투명 캐노피 때문에 외부 화면에서는 조종사가 가려진다. 캐노피 재질·좌석 값은 변경하지 않았다.
- 사용자의 Editor는 조작하지 않았다. 실제 Editor에서 탑승/하차 감각, 조종석 가림·관통, 1·3인칭 전환의 시각적 연속성은 직접 확인할 부분이다.

## 적용하지 않은 후보와 다음 순서

KayKit `Crawling`도 격리 프로젝트에서 추출·연결·테스트·렌더했다. 자동 검사는 통과했지만 손·무릎 위주의 동작이고 현재 낮은 포복 높이에 맞추면 무릎이 바닥에 묻혔다. 이 후보의 클립·코드·Controller·테스트 변경은 제품에 적용하지 않았다. 기존 `_bodyGroundClearance`, 캡슐 높이 1.1과 포복 속도 2를 임의로 바꾸지 않았다.

다음에는 낮은 군인식 포복, 슬라이딩/장애물 통과, 전용 총 타격·소생·승하차 모션을 적합한 외부 에셋으로 확보한 뒤 한 동작씩 연결한다. 방향별 포복과 발 접지·전용 전환 모션도 아직 남아 있다. 전체 보병 기능이나 배틀필드와 같은 완성도를 완료했다고 표시하지 않는다.
