# 슬라이드 종료 자세와 옆틀림 수정

## 변경 전 확인 및 재현

직전 C키 작업의 applied.json과 현재 파일을 비교했고 추가 사용자/Editor 변경은 없었다. 대상6개 파일을 before.json으로 보존하고 제품 반영 직전 해시를 재확인했다. 사용자의 새 요구에 따라 이전의 종료 앉기만 종료 일어서기로 바꾼다. C 입력, 기존0.75초/감속 종료 속도4/모델 배치/캡슐 설정은 유지한다.

기존 종료 조건은 `IsSliding=false`만 수행하여 Stance.Crouched가 남았다. 기존 임포트는 루트 회전을 pose에 굽고 Original 방향을 유지해 동일 장면 frame25에서 bodyYaw약74.59°, pelvisYaw약66°가 관측됐다. 회전 Bake만 끄는 후보는 중간54.68°/끝85.73°로 해결하지 못했다. Body Orientation 기준까지 함께 변경한 후보에서 중간 약-15°로 줄어 원인 가설을 지지했다. 최종 전체 source 범위 재생에서는 같은 frame25 bodyYaw1.14°/pelvisYaw-8.91°였고 저자세 frame15 bodyYaw-16.09°였다. 이 각도는 우리 Unity 캐릭터의 측정이며 참고 게임에서 추출한 수치가 아니다.

## 참고 영상과 관찰 범위

- [PlayOverwatch 공식 소전 영상](https://www.youtube.com/watch?v=rPNNTMu4iko), 약0:35의 정면 슬라이드 장면을 브라우저에서 음소거/일시정지·프레임 이동으로 확인. 몸과 무기가 함께 전진 방향을 향하는 형태를 기준으로 삼았다.
- [Battlefield 6 Movement Masterclass](https://www.youtube.com/watch?v=vhfQJo2Lzkg), 0:32 슬라이드 장 및2:22–2:27 연습장 예시를 브라우저에서 확인. 1인칭 시선과 전진/진입 흐름 참고. 이 영상으로 BF6 골반의 정확한3차원 각도나 내부 애니메이터 구현을 측정했다고 주장하지 않는다.
- 웹 fetch는403/페이지 접근 제한이 있어 브라우저 재생으로 전환했다. 검색 설명만 보고 영상 확인으로 간주하지 않았다. 영상/게임의 애니메이션 파일을 추출·복사하지 않았다.

## 변경 파일과 구체적인 값

| 파일 | 변경 |
|---|---|
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterTraversal.cs` | 새 `FinishSlide()`가 IsSliding을 끄고 현재 앉기인 경우 기존 `TrySetCrouched(false)` 요청. 시간 종료/벽 충돌/지상 이탈과 공격 불가 중단 경로의 단순 flag clear를 교체. 천장 공간 검사는 Stance가 계속 소유. 사망·외부 Cancel/OnDisable의 의미와 명시적 자세 변경/점프 경로는 유지 |
| `Assets/Features/PlayerCharacter/Editor/MixamoSlideImporter.cs` | 최초 임포트 기본값 `lockRootRotation=true, keepOriginalOrientation=true`→둘 다false. 컷0/11.5/21.5/27→0/11.5/36.8/46. 기존 앉기 종료 때문에 제외한 일어서기 후반부 재사용. 기존 프로필/사용자 설정 보존 정책 유지 |
| `Assets/ThirdParty/MixamoLocal/RunningSlide.fbx.meta` **로컬·Git 제외** | 세 클립 모두 `loopBlendOrientation:1→0`, `keepOriginalOrientation:1→0`. Start0–11.5 유지, Loop11.5–21.5→11.5–36.8, Exit21.5–27→36.8–46. 메타 외 FBX/프로필/클립 ID와 height/XZ/scale/Avatar 설정 유지 |
| `Assets/Tests/PlayMode/PlayerCharacterInfantryTests.cs` | 정상 종료 검사 EndsCrouched→EndsStanding, 기대 자세false로 수정. 새 `SlideRecoveryRetainsCrouchWhenTheCeilingBlocksStanding`으로 낮은 천장 아래의 안전 예외 검증 |
| `Assets/Tests/PlayMode/PlayerCharacterTraversalAnimationTests.cs` | 정상 종료 기대값 Crouched→Standing, 종료 상태 CrouchLocomotion→Locomotion. 접지/루트 고정 등 기존 기준 유지 |
| `Assets/Tests/PlayMode/SlideAnimationOverrideTests.cs` | 정상 종료 기대 자세를 Standing으로 수정. 실제 로컬 모션이 있을 때 진행35%–75%의 수평 body heading 편차<35° 검사 추가. 다른 장비·override/위치/rotation 기준 유지 |
| `Docs/AI/SlideRecoveryChanges.md` | 이 기록 |
| `Docs/AI/UnityProjectContext.md` | 최신 종료 동작/회전 기준/검증 추가 |

원본 `LocalAssets/Mixamo/Running Slide.fbx`와 로컬 복제 FBX 바이트는 유지한다. 관절/근육 키프레임·본 회전 보정 코드는 새로 만들지 않았다. Root 회전은 추출하지만 기존 applyRootMotion=false라 actor에는 적용하지 않는다. 캐릭터 시선/이동과 원본 애니메이션의 회전 책임을 분리한다. 기존 Quaternius fallback/씬 참조/무기 모션 유지. 새 클래스/컴포넌트/에셋/메타 파일 추가·삭제 없음.

## 제거·교체 비교

```cs
// 이전
if (timeExpired || hitWall || leftGround) IsSliding = false;
// 변경
if (timeExpired || hitWall || leftGround) FinishSlide();
// FinishSlide 내부
IsSliding = false;
if (_stance != null && _stance.IsCrouched) _stance.TrySetCrouched(false);
```

기존 메서드/필드/컴포넌트/참조 삭제 없음. 직접 flag clear의 완료 처리와 임포트 회전/컷/종료 검사 기대값만 교체했다. 낮은 천장 아래에서는 일어서기를 강제로 관통시키지 않고 앉기를 유지한다. 기존 Cancel은 사망/탑승/씬 해제 등의 별도 수명 처리이므로 강제 일어서기를 추가하지 않았다.

## 구조 검토

슬라이드 종료는 기존 상태 소유자인 Traversal 안의 작은 메서드로 유지했다. 별도 class로 분리하면 상태 수명만 흩어져 추출하지 않았다. 캡슐/천장 판정은 기존 Stance API로 요청하며 직접 Current/높이를 쓰지 않는다. 모션 방향은 기존 importer/data 책임으로 해결하여 카메라·무기 controller에 보정을 추가하지 않았다. 상속/추가 인터페이스를 만들지 않았고 OOP/SOLID상 의도적 예외 없음.

## 검증 및 남은 범위

- Unity6000.4.8f1 격리 프로젝트, 모든 테스트 `-batchmode -noaudio`.
- 최종 제품 전체 **182/182 통과**, 172.7초. 관련 검사28개+캡처1개=29/29, 로컬 Mixamo 분리 fallback **8/8 통과**.
- 소총/권총/맨손21장 최종 캡처와 변경 전/회전 추출만/Body Orientation 후보 비교. normal ending standing, low-ceiling crouch, slide phases, floor contact, local transform stability, heading bounds, weapon override, old jump/reload/boarding 회귀 검사.
- 임시 캡처 코드는 Library에 보관, 제품 Assets/추가 씬에 포함하지 않는다. 제품 복사 파일은 검사 프로젝트와 바이트 동일. 원본 FBX 해시 보존 확인. persistent audio/사운드 값 변경 없음. 커밋/스테이징 없음.
- 남은 확인: 사용자 Editor에서 연속 C/마우스 조준·1인칭 화면/경사·머리 위 장애물에서의 시각적 전환 체감. 원본 Mixamo의 팔다리 포즈까지 두 게임과 동일하게 복제한 구현이 아니다. Player 빌드는 실행하지 않았다.

증거 `Library/CodexSlideFix/{before.json,before.xml,before.log,rotation.xml,body.xml,focused.xml,final.xml,fallback.xml,contact-final.png,applied.json,final_slot*.png}`.
