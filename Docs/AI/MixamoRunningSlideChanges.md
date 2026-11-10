# Mixamo Running Slide 적용

## 변경 전 비교

- 직전 작업의 파일 해시 목록과 비교했으며 사용자/Editor의 추가 변경은 발견되지 않았다. 이번 작업의 `.gitignore` 추가 내용도 재확인했다.
- 사용자 원본 `LocalAssets/Mixamo/Running Slide.fbx`는 이름과 바이트를 보존한다. SHA256: `07bfb606c06cc696252fb027c4a2075330b200f886304a8e9f6674ef18c7321d`.
- 기존 이동, 캡슐, 슬라이드 시간 0.75초, 감속 종료 속도 4, 종료 후 앉기, 무기/조준/장전 데이터는 유지한다.

## 변경 파일과 책임

| 파일 (프로젝트 루트 기준) | 구체적인 변경 | 이유/동작 |
|---|---|---|
| `.gitignore` | `/LocalAssets/Mixamo/`, `/Assets/ThirdParty/MixamoLocal/`, `/Assets/ThirdParty/MixamoLocal.meta` 제외 추가 | Mixamo 원본/추출 가능한 애니메이션을 공개 Git에 올리지 않음. 기존 제외 규칙 유지 |
| `Assets/Features/PlayerCharacter/Scripts/SlideAnimationSet.cs` | 새 SO. `_originalStart`, `_originalLoop`, `_originalExit`와 `_start`, `_loop`, `_exit` 보유. `TryApply`는 원본 키 3개가 모두 있을 때만 교체 | 에셋 공급자와 무관한 3단계 슬라이드 데이터 |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterSlideAnimationView.cs` | 새 컴포넌트. `_animator`, `_combat`, `_animations` 명시 참조. 실행 순서 42. 장비 변경 이벤트와 LateUpdate에서 현재 컨트롤러에 슬라이드만 합성 | 무기 override를 보존하고 소총/권총/맨손 전환 후에도 슬라이드 적용. 비활성화는 이벤트만 해제하여 탑승 중 Animator 재바인딩 방지. 자신의 controller 복원/캐시 해제는 재초기화·파괴 때 수행 |
| `Assets/Features/PlayerCharacter/Editor/MixamoSlideImporter.cs` | 새 메뉴 `Tools/Battlefield/Animations/Mixamo/Import Running Slide`. 원본을 로컬 Assets 폴더로 복사. Humanoid/자체 Avatar/30fps 원본/압축 Off, 카메라·라이트 임포트 Off. 최초 임포트 시 아래 3구간 구성 | 원본 파일은 변경하지 않고 로컬 복제본만 임포트. 재실행 시 이미 존재하는 프로필/사용자 임포트 설정 보존 |
| `Assets/Features/PlayerCharacter/Editor/LocalSlideAnimationSetup.cs` | 새 Editor 공급자. Play 시작/씬 로딩 시 유효한 로컬 프로필이 있으면 기존 Animator 오브젝트에 view 추가/초기화하고, 현재 씬 복사본의 `PlayerVehicleOccupant._onFootComponents` 마지막에 새 view를 등록. 기존 view가 있으면 보존. 빌드 씬 처리에도 같은 데이터 주입 | SampleScene에 재배포 제한 에셋 참조를 저장하지 않음. 탑승 중 새 view도 기존 보행 표현과 함께 중지하고 하차 시 마지막에 복원. 파일 없는 Git 복제본은 기존 Quaternius 모션으로 실행 |
| `Assets/Features/PlayerCharacter/Animation/PlayerCharacter.controller` | 전환 ID `6013037597774752302` (TraversalMode 2)와 `-922784355774615617` (mode 3)의 `m_TransitionDuration: 0.04` → `0` | 공용 TraversalTime 리셋 시 outgoing clip이 시작 자세로 되감기는 혼합 제거. 다른 전환/상태/클립 참조 보존 |
| `Assets/Tests/PlayMode/SlideAnimationOverrideTests.cs` | 새 PlayMode 4검사: 무기 override 보존/일시 중지 시 controller 유지/null 재초기화 시 복원, 로컬 프로필 유무 처리, 실제 슬라이드의 모델/모터 분리 방지, null 데이터 무변경 | 선택적 외부 에셋 적용과 기존 장비 lifecycle 검증. synthetic CC0 모션으로도 테스트 가능 |
| `Assets/Tests/PlayMode/PlayerCharacterSeatedAnimationTests.cs` | 기존 `MissingClipRetainsTheExistingManualSeatPose`의 실시간 위치 직접 비교를 `SeatedFrameProbe`의 동일 프레임 LateUpdate 기록 비교로 교체. 실행순서10000. `Captured==true` 추가, 기존 `.002f` 기준 유지 | 물리/좌석이 다음 프레임에 움직인 뒤 이전 프레임 골반과 비교하는 간헐 오판 방지. 실제 렌더 프레임에 좌석과 골반이 맞는지 검증. 다른 좌석 검사/제품 코드 유지 |
| 위 5개 새 `.cs`의 `.meta` | 새 GUID/기본 importer. 파일명: `SlideAnimationSet.cs.meta`, `PlayerCharacterSlideAnimationView.cs.meta`, `MixamoSlideImporter.cs.meta`, `LocalSlideAnimationSetup.cs.meta`, `SlideAnimationOverrideTests.cs.meta` | Unity 참조 GUID 유지. 기존 meta 변경 없음 |
| `Docs/AI/UnityProjectContext.md` | 적용 구조, Mixamo 우선 탐색 방침, 검증 기록 추가 | 후속 작업 문맥 유지 |
| `Docs/AI/MixamoRunningSlideChanges.md` | 이 변경 보고서 | 변경/제외/재설치/검증 공개 |

## 로컬 생성 에셋 (Git 제외)

- `Assets/ThirdParty/MixamoLocal.meta`: 로컬 폴더 GUID.
- `Assets/ThirdParty/MixamoLocal/RunningSlide.fbx`: 사용자 원본과 동일 바이트의 복제본.
- `Assets/ThirdParty/MixamoLocal/RunningSlide.fbx.meta`: Humanoid avatar와 3개 클립 임포트 설정.
- `Assets/ThirdParty/MixamoLocal/RunningSlide.asset`: SlideAnimationSet. 원본 키는 기존 Quaternius `Slide_Start/Slide_Loop/Slide_Exit`, 대체 값은 FBX의 `RunningSlide_Start/RunningSlide_Loop/RunningSlide_Exit`.
- `Assets/ThirdParty/MixamoLocal/RunningSlide.asset.meta`: 로컬 프로필 GUID.

원본은 0–46프레임(약1.533초)이다. 최초 전체 분할(0–11.5/11.5–36.8/36.8–46)을 격리 테스트했으나 완전히 일어나는 후반부가 게임의 앉기 종료와 맞지 않았다. 최종 임포트는 **0–11.5 / 11.5–21.5 / 21.5–27프레임**만 사용한다. 손으로 키프레임/관절 곡선을 만들지 않았다. 세 구간은 기존 TraversalTime 정규화로 0.75초에 재생한다. `loopTime=false`, `loopPose=false`이며 Loop는 기존 단계명일 뿐 무한 반복이 아니다.

루트 회전/Y는 pose에 유지하고 XZ 이동은 추출한다. Animator의 기존 `applyRootMotion=false`로 원본 약6.2m 이동을 중복 적용하지 않는다. 실제 이동 주체는 기존 CharacterController motor이다. 원본에서 제외한 27–46프레임은 삭제하지 않았다.

## 연결 및 사용

기존 흐름: `Movement → Traversal → AnimationView → Animator`.
추가 흐름: 로컬 Editor 공급자 → `SlideAnimationSet + PlayerCharacterSlideAnimationView` → 기존 무기 컨트롤러의 슬라이드 3개만 override.

Play 중 기존 `PlayerCharacterAnimationView`와 동일한 Animator 오브젝트에 새 view가 붙는다. 편집 씬에 영구 컴포넌트/새 오브젝트/참조를 저장하지 않는다. 무기 선택과 무기 상체/장전 모션은 기존 컴포넌트가 계속 담당한다.

현재 SampleScene의 원본 `_onFootComponents` 16개는 그대로이며, 유효한 로컬 프로필이 있는 Play/빌드 씬 복사본만 마지막에 새 view를 추가해17개가 된다. 탑승 시 기존 항목 뒤에서 중지되고, 하차 시 기존 무기 표현 뒤에서 복원된다.

SampleScene에서 지상 달리기 중 **Left Ctrl(앉기)**로 기존 슬라이드 진입을 테스트한다. 캐릭터 입력 데이터의 현재 바인딩을 따른다. 다른 PC에서는 사용자가 Mixamo에서 받은 같은 파일을 `LocalAssets/Mixamo/Running Slide.fbx`에 두고 위 Import 메뉴를 실행한다. 이미 프로필이 있으면 임포트 메뉴는 사용자가 고친 컷을 덮어쓰지 않는다. 커스텀 파일/다른 길이의 모션은 해당 FBX의 컷을 검토해야 한다.

## 제거/교체 내역

- 기존 메서드/필드/컴포넌트/에셋/참조 삭제 없음.
- 기존 슬라이드 Quaternius 3개는 컨트롤러에 유지. 로컬 프로필이 있을 때만 런타임 재생 값 교체.
- 새 view의 초기 실험 버전에 있던 `OnDisable`의 `Restore()/ClearControllers()` 호출은 최종 버전에서 제거했다. 일시 중지 때 controller를 재바인딩하거나 캐시 동일성을 잃지 않도록 하고, 두 처리는 `Initialize`/`OnDestroy`에서 계속 수행한다. 메서드 자체를 삭제하지 않았다.
- 컨트롤러의 위 두 전환 duration 값만 교체. 단계 내 같은 타임라인을 이어 읽는 구간이라 중간 blend가 오히려 역재생처럼 보였다.
- 기존 좌석 테스트의 `Vector3.Distance(hips.position, seat.position)` 즉시 검사를 `frame.Distance` 기록 검사로 교체했다. 진단20회 중 한 프레임은 좌석만 약1.15cm 움직이고 골반은 고정돼 있었으며, 직전 LateUpdate 오차는0이었다 (`order.log`). 보간 Off 실험은 해결하지 못해 폐기했고 `PlayerVehicleBoardingTests.cs`는 변경하지 않는다. 새 probe가 실제 pose 계산 뒤 거리만 기록하며 2mm 허용오차는 동일하다.
- 격리 실험용 전체 프레임 분할은 최종 로컬 컷으로 대체. 원본 FBX는 보존.
- 임시 캡처/곡선 진단 코드는 Library에 보관하고 제품 Assets에는 포함하지 않는다.

## 설계 검토

SO=클립 데이터, runtime view=애니메이션 합성/수명, importer=파일 변환, Editor 공급자=로컬 데이터 연결로 책임 분리. motor에 외부 에셋 경로/재생 책임을 넣지 않았다. 명시 참조와 읽기 전용 프로퍼티를 사용하고, 상속 대신 기존 AnimatorOverrideController와 컴포지션을 사용했다. 여러 구현이 필요한 계약이 없어 새 인터페이스를 만들지 않았다. 장비 이벤트 등록 해제/생성 컨트롤러 해제/다른 소유자의 컨트롤러 보존을 포함한다. 일시 중지는 Animator 값을 되돌리지 않는 의도적 선택이며, 데이터 해제는 `Initialize(..., null)` 또는 컴포넌트 파괴로 수행한다. Editor 공급자만 의도적으로 고정 로컬 경로를 안다. LateUpdate 확인은 장비 override 적용 순서와 외부 controller 교체에 대응하며 안정 상태에서는 새 컬렉션/컨트롤러를 생성하지 않는다.

## 라이선스

Mixamo는 CC0 재배포 에셋으로 취급하지 않는다. 게임 사용과 원본 파일 배포는 별개다. [Adobe FAQ](https://helpx.adobe.com/creative-cloud/faq/mixamo-faq.html), [Adobe 라이선스 안내](https://community.adobe.com/questions-696/mixamo-faq-licensing-royalties-ownership-eula-and-tos-589400?lang=en). 이 프로젝트에서는 원본과 임포트 결과를 Git 제외하고 재현용 코드/설명을 공유한다.

## 검증

- Unity 6000.4.8f1, 격리된 프로젝트, `-batchmode -noaudio`. 최종 제품 PlayMode **181/181 통과**, 174.1초 (`Library/CodexMixamoSlide/final3.xml`). 새 검사는4개이며 기존 2mm 허용오차는 완화하지 않았다. 좌석 검사 한 곳은 실제 렌더 프레임을 기록하도록 수정했다.
- 탑승/슬라이드 집중 검사13/13 (`seatstress3.xml`, 좌석 기본 모션 20회 독립 로드 포함), 로컬 Mixamo 폴더를 실제로 Assets 밖으로 분리한 fallback 검사8/8 (`fallback2.xml`), 최종 모션 캡처를 포함한 집중 검사9/9 (`focused3.xml`). 파일 분리 후 원래 GUID/바이트로 복원했다.
- 초기 전체 실행179/181에서 controller 복원 참조와 간헐 좌석 검사 실패를 발견했다. 전자는 공급자의 보행 suspension 등록/cache identity로 해결했다. 후자는 위치 로그로 좌석이 다음 프레임에 움직인 것을 확인해 동일 렌더 프레임 비교로 수정했다. 일시 비활성화는 이벤트만 중지하며, reinitialize/destroy 때 controller를 복원한다.
- 소총/권총/맨손의 진입·저자세·회복·앉기21장 캡처를 검토했다 (`contact-final.png`, `slot*_frame*.png`). 초기 후반부 솟음/단계 되감기를 확인한 후 source 컷/전환 값을 보정했다. 전 시점의 시각적 완성도를 주장하지 않는다.
- 무기 override 유지, 일시 중지 시 controller 유지 및 null 재초기화 후 원래 controller 복원, Humanoid 클립 유효성, 실제 모터 이동/바닥 접촉과 모델 local transform 고정, 좌석 진입/하차, 기존 발사/장전/조준 회귀를 검사했다.
- 제품에 복사한12개 코드/meta/controller 파일이 테스트 프로젝트와 바이트 동일함을 확인했다. `.gitignore`/원본 FBX 해시/직전 작업 기록도 재확인했다. `git check-ignore`로 원본·임포트 FBX·로컬 프로필·meta 제외 확인. 기존 추적 Mixamo 파일 없음.
- 진단 컴포넌트는 Library에 보관, 추가 제품 씬/테스트 씬 없음. persistent audio/AudioSource 값 변경 없음. 커밋/스테이징 없음.
- 남은 확인: 사용자 Unity Editor에서 실제 입력/연속 사용/극단 조준과 슬라이드 종료의 체감 및1인칭 화면 확인. 빌드 씬 주입 코드는 작성했지만 대상 Player 빌드는 실행하지 않았다.
