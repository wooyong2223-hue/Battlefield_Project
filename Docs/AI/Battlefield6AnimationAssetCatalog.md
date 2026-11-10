# 배필6 영상 기준 외부 애니메이션 수집·검토 1차

> 현재 Quaternius UAL1/2 메인 적용과 대응 기존 모션 교체를 완료했다. 정확한 사용 파일/설정/검증은 [적용 기록](QuaterniusMainAnimationChanges.md)을 참조한다. 최초 수집 이력과 Kevin 제외 상태는 보존한다.

> **최신 선정 기준:** 원본 파일을 공개 Git 저장소에 포함할 수 있는 에셋으로 재선정한다. Kevin 병사 팩은 이번 적용 대상에서 제외했으며 제품에 적용하지 않았다. 아래 내용은 최초 수집 이력이다. 현재 후보/조건/미확보 항목은 [재배포 가능한 애니메이션 재탐색](RedistributableAnimationCandidates.md)을 기준으로 한다. 클립 CSV의 Kevin 상태도 제외로 갱신했다. 파일 manifest의 해시는 최초 수집 시점 기록이다.

2026-10-28. 사용자의 최신 우선순위는 **기능 구현보다 외부 캐릭터 애니메이션 확보**다. 이번에는 플레이 코드·씬·Animator를 수정하지 않고 영상 관찰, 파일 확보, 리타깃 샘플링과 누락 목록을 정리했다. 다음 구현 작업은 이 목록에서 모션을 선택한 뒤 진행한다.

## 이번 결과와 상태의 의미

- **새로 다운로드:** Kevin Iglesias Human Soldier Animations FREE의 공식 FBX ZIP 및 Unity 패키지.
- **별도 추출:** 보유 중인 Quaternius UAL1/UAL2 무료 Standard에서 CC0 Humanoid 클립 9개.
- **리타깃 샘플 확인:** Kevin 남성 모션/홀드 63개 + Quaternius 9개 = 72개. 현재 PlayerCharacterArmature에서 각 클립의 10%·50%·90% 시점을 평가했다.
- **렌더 확인:** 병사 후보 6개와 Quaternius 9개의 중간 포즈 이미지. 무기 모델이나 지형을 끼워 맞춘 결과가 아니다.
- **클립 목록 232행:** Kevin 128 + Quaternius 무료 Standard 실제 소스 클립 86 + 기존 제품 `.anim` 18개. 남녀·Root Motion·홀드 포즈와 기존 에셋을 포함하므로 232가지 새 동작을 확보한 것이 아니다. 기존 Starter Assets FBX 내장 클립은 이 숫자에서 제외한다.

| 상태 | 의미 |
|---|---|
| 기존 보유 | 현재 제품 파일이 있음. 파일 존재와 실제 Animator 참조 여부는 별개 |
| 검토 후보 | 파일 확보 및 표시된 범위의 검사 완료. 게임에 채택·연결 완료라는 뜻은 아님 |
| 소스 목록만 확인 | 아카이브 내 클립 이름·길이 확인. 개별 추출/시각 검토 미실시 |
| 부적합 | 렌더상 목표 동작과 맞지 않아 적용 대상에서 제외 |
| 미확보 | 현재 확보한 무료 파일에서 필요한 전용 모션을 찾지 못함 |

전체 이름·경로·길이·검사 상태: [Battlefield6AnimationClips.csv](Battlefield6AnimationClips.csv).

## 실제 영상에서 확인한 구간

설명문만 읽은 것이 아니라 이전에 확보한 영상에서 프레임을 새로 추출해 연속 이미지를 확인했다. 이 영상들은 **2025년 베타/출시 초기 자료**로, 2026년 현재 패치와 같은 동작·속도임을 보장하지 않는다.

| 영상과 구간 | 화면에서 관찰한 내용 | 에셋 선택에 반영할 조건 |
|---|---|---|
| [Maxiq — Battlefield 6 Has NEW Movement!](https://www.youtube.com/watch?v=LdPSuJzm93Q&t=1s), 2025-08-03, 00:01–00:04 | ADS에서 무기가 내려가고 기울어진 장전 동작을 거쳐 기본 자세로 복귀 | 조준·장전·이동은 서로 다른 손/총 자세가 필요. 해당 장면의 총 기울임을 달리기 모션으로 분류하지 않음 |
| [aksu02 — BF6 Advanced Movement Guide](https://www.youtube.com/watch?v=BWMlR_ViPgA&t=14s), 2025-10-26, 00:14–00:17 및 00:14–00:24 | 점프·슬라이드가 이어지며 시야 높이와 화면 속 무기 위치가 변함 | 슬라이드의 진입·유지·종료 모션, 별도 1인칭 표현을 함께 확보할 필요 |
| [같은 가이드](https://www.youtube.com/watch?v=BWMlR_ViPgA&t=56s), 00:56–00:59 | 문 진입/닫기 중 한 손을 문 쪽으로 뻗는 표현 | 일반 Interact가 아닌 문 전용 한 손 동작도 별도 필요 |
| [같은 가이드](https://www.youtube.com/watch?v=BWMlR_ViPgA&t=350s), 05:50–06:00 | 소총/권총 전환과 조준·발사가 보이며 무기별 화면 구도가 다름 | 권총/소총 및 1인칭/전신 모션을 하나의 장전/홀드로 취급하지 않음 |

밀집 확인은 8fps(0.125초 간격)로 장전 3초·슬라이드 4초·문 3초를 추출했다. 넓은 맥락은 1fps 12초씩 확인했다. 원본 영상은 `Library/CodexBattlefieldReference`, 이번 접촉 시트는 `Library/CodexAnimationCatalog`에만 보관한다. 영상의 일부 프레임만으로 정확한 동작 길이·이동 속도·본 가중치·원본 구현을 역산했다고 주장하지 않는다.

이번 직접 관찰은 주로 **1인칭 팔·총·시점**이다. 전신 포복/소생/승하차의 세부 동작이나 모든 3인칭 방향 전환을 영상에서 확인한 것으로 표시하지 않는다. [EA 공식 소개](https://www.ea.com/games/battlefield/battlefield-6/news/battlefield-101)의 끌기·소생/엄폐 기울이기/무기 거치/낙하 구르기는 별도 공식 설명 근거이며 이번 프레임 관찰과 구분한다. 가이드의 버그성 이동/연속 기술은 재현 목표로 채택하지 않았다.

## 동작별 확보 현황과 우선순위

| 우선 | 필요한 동작 | 현재 확보한 대응 파일 | 판정·부족한 점 |
|---|---|---|---|
| 1 | 소총 조준·발사·장전 | Kevin `HumanM@AssaultRifle_Aim01`, `AssaultRifle_Aim01_Shoot01`, `AssaultRifle_Reload01` | 검토 후보. 각각 약 2.333333/0.8/1.7초. 전신 모션이며 현재 총의 탄창·그립과 맞춘 상태는 아님 |
| 1 | 소총을 든 이동 기반 | Kevin `Walk01_*`, `Run01_*` 각 8방향 및 `WeaponHold_AssaultRifle01` | 검토 후보. 이동과 무기 홀드는 구분된 파일. 완성된 무장 이동 Controller가 제공/연결됐다는 뜻은 아님 |
| 1 | 무기별 전신 홀드 | Kevin AssaultRifle/Rifle/Bazooka 남성 홀드 3개 | 검토 후보. 장비별 손 위치/상체 레이어 결합 검토 필요 |
| 1 | 출발·정지·제자리 회전 | 전용 세트 없음 | 미확보. 반복 이동 클립만으로 이 동작까지 있다고 표시하지 않음 |
| 1 | 앉은 대기·이동 | 기존 Quaternius CrouchIdle/CrouchWalk | 기존 보유. 앉은 8방향·앉은 달리기·서기 전환의 일관된 세트 부족 |
| 1 | 낮은 군인식 포복·방향 전환 | 적합한 전용 클립 없음 | 미확보. 이전 KayKit Crawling은 손·무릎 동작 및 지면 관통 때문에 부적합 판정 유지 |
| 1 | 서기↔앉기↔포복 전환 | 적합한 전용 세트 없음 | 미확보. UAL2 LayToIdle 같은 일반 동작을 포복 전환 완성본으로 표시하지 않음 |
| 1 | 1인칭 무기·팔 전용 세트 | 현재 전신 클립을 활용하는 기존 표현만 있음 | 새 전용 세트 미확보. 대기/걷기/달리기/ADS/발사/장전/교체가 같은 총과 맞는 세트 우선 |
| 2 | 슬라이드 진입·유지·종료 | Quaternius `Slide_Start`, `Slide_Loop`, `Slide_Exit` | 3개 검토 후보. 몸이 누운 자세와 무장 상체의 결합, 바닥 접촉, 1인칭 동작을 추가 확인해야 함 |
| 2 | 장애물 올라가기 | Quaternius `ClimbUp_1m` | 검토 후보. 이름의 1m는 원본 조건이며 현재 높이 4 캐릭터의 설정값을 1m로 변경한 것이 아님. 낮은 장애물 넘기/높이별 세트는 아님 |
| 2 | 낙하 구르기 | Quaternius `Roll` | 일반 구르기 검토 후보. 배필6 낙하 구르기와 동일한 동작 또는 전환으로 확정하지 않음 |
| 2 | 수류탄 투척 | Kevin `ThrowGrenade01_L`, Quaternius `OverhandThrow` | 왼손 수류탄/일반 던지기 후보. 핀 뽑기·대기·취소·자세별 전용 모션은 미확보 |
| 2 | 문/상호작용 | Quaternius `Interact` | 일반 상호작용 후보. 영상의 한 손 문 밀기와 같은 동작은 아님 |
| 2 | 피격·사망 | Kevin `Damage01/02`, `Death01/02/03`, 기존 InfantryDeath | 새 후보 및 기존 보유. 방향별 반응·다운 진입/대기는 별도 확보 필요 |
| 2 | 주먹 | 기존 Rokoko BoxingGuard/Jab/Cross | 기존 보유. 이번에 교체하지 않음 |
| 2 | 총 근접 공격 | 기존 복싱 클립 재사용 | 전용 총몸/개머리판 타격 미확보 |
| 3 | 소생·끌기 | 기존 무릎 작업 모션을 소생에 재사용 | 짝 소생/끌기·끌려가기 전용 세트 미확보 |
| 3 | 조종석 대기·승하차 | 기존 Quaternius SeatedIdle + 조종석 보정 | 착석 대기만 기존 보유. 실제 승하차 과정 전용 모션은 미확보 |
| 3 | 수영 | Quaternius `Swim_Fwd_Loop`, `Swim_Idle_Loop` | 검토 후보. 수면·카메라·장비와 연결 미실시 |
| 3 | 엄폐·거치·사다리·장비 | 현재 적합한 전용 세트 없음 | 미확보. 기존 절차적 기능과 외부 전용 모션 확보를 구분 |

## 실제 보관 위치

### Human Soldier Animations FREE — 로컬 검토용

출처: [Kevin Iglesias 공식 무료 배포](https://kevdev.itch.io/human-soldier-animations-free). 보관: `LocalAssets/AnimationReview/HumanSoldierAnimationsFREE/`.

- `HumanSoldierAnimationsFREE-15306397.zip` — 원본 FBX ZIP, SHA256 `edd665bea863c3b30d56358831f68d9dbb5a64bb17a553f4f6f92441bf5ea5b7`.
- `HumanSoldierAnimationsFREE-18766376.unitypackage` — 원본 Unity 패키지, SHA256 `5a9f20fb075363c8b28a97c3360c3c93d9a6004af6e5caf7bcb1e852f6783b40`.
- `FBX/` — 원래 경로를 유지한 FBX 135개: 남성/여성 동작 각 60개, 홀드/손 포즈 8개, 참조 모델 7개.
- `README.md` — 구성·검사 범위·도입 조건.
- `kevin-license-page.html` — 이번에 확인한 제작자 약관 페이지의 로컬 기록.

제작자의 [현재 사용 조건](https://www.keviniglesias.com/)에 따라 프로젝트 내 활용과 원본 단독 재배포를 구분한다. CC0로 취급하지 않으며 이 폴더는 Git에서 제외한다. 공유 저장소의 사용자는 제작자 페이지에서 자신의 복사본을 받는다. 이번에 계정 로그인·결제·가입은 하지 않았다.

### Quaternius CC0 — 추출한 검토 후보

보관: `LocalAssets/AnimationReview/QuaterniusCC0/`. `Assets` 밖에 있으므로 Unity 게임 씬에 자동 반영되지 않는다.

| 파일 — 각 파일의 `.meta`도 함께 보관 | 원본 | 길이(초) |
|---|---|---|
| `Animations/Slide_Start.anim` | UAL2 Slide_Start | 0.833333 |
| `Animations/Slide_Loop.anim` | UAL2 Slide_Loop | 2.0 |
| `Animations/Slide_Exit.anim` | UAL2 Slide_Exit | 0.5 |
| `Animations/ClimbUp_1m.anim` | UAL2 ClimbUp_1m | 0.666667 |
| `Animations/OverhandThrow.anim` | UAL2 OverhandThrow | 1.333333 |
| `Animations/Roll.anim` | UAL1 Roll | 1.466667 |
| `Animations/Swim_Fwd_Loop.anim` | UAL1 Swim_Fwd_Loop | 1.333333 |
| `Animations/Swim_Idle_Loop.anim` | UAL1 Swim_Idle_Loop | 3.333333 |
| `Animations/Interact.anim` | UAL1 Interact | 2.0 |

추가 파일은 `README.md`(출처/처리/한계), `LICENSE.txt`(원본 허가문), `Preview.jpg`(중간 포즈 미리보기)다.

출처는 [UAL1 무료 Standard](https://quaternius.itch.io/universal-animation-library)와 [UAL2 무료 Standard](https://quaternius.itch.io/universal-animation-library-2)이며, 원본 패키지의 CC0 선언을 확인했다. Unity 6000.4.8f1에서 Humanoid 클립을 추출하고 기존 Quaternius 동작과 같은 RootQ/RootT Y축 180도 방향 보정을 적용했다. 근육 모션을 새로 제작하지 않았으며 이름에 Loop가 있는 클립만 반복 설정했다. 메타데이터는 격리 Unity가 생성한 GUID를 유지했다.

## 검증 기록

- 격리 Unity 6000.4.8f1 batch 실행 `ReviewAnimationCatalog.Execute`: 종료 코드 0, `CATALOG_REVIEW records=149 retargeted=72`.
- 최종 재검증에서는 미리보기 Animator의 플레이용 Controller를 제거하고, 클립마다 Rebind한 후 평가하여 기존 Controller나 직전 클립 자세의 영향을 줄였다.
- 검사한 72개 모두 Humanoid, 길이 > 0, 3개 시점의 골반/머리/양손/양발 위치가 유한하고 비정상적으로 멀리 벗어나지 않음.
- `Library/CodexAnimationCatalog/review-final.log`, `unity-review.json`에 근거 보관. 15개 렌더 포즈 확인. 샘플링 통과는 바닥/벽 접촉·무기 그립·프레임 간 움직임 품질·1인칭 구도 검증을 대신하지 않는다.
- 기존 제품 코드·SampleScene·PlayerCharacter.controller를 바꾸지 않았다. 씬/Controller 해시 및 직전 착석 작업 검증기 재확인. 이번에 제품 PlayMode를 새로 실행한 것으로 표시하지 않는다.
- Unity Editor에서의 실제 재생, 무기 모델 연결, 지면/장애물·기존 자세 전환과의 결합 확인은 각 클립을 채택하는 후속 단계에 남긴다.

## 이번에 변경한 파일과 이유

| 파일/범위 | 변경 | 이유·동작 영향 |
|---|---|---|
| `.gitignore` | `/LocalAssets/AnimationReview/HumanSoldierAnimationsFREE/` 제외 규칙과 설명 추가 | 로컬 검토 원본을 공유 저장소와 분리. 기존 규칙 보존 |
| 이 문서 | 신규 | 영상에서 직접 본 내용·후보·누락을 한곳에서 확인 |
| `Docs/AI/Battlefield6AnimationClips.csv` | 신규, 232행 | 클립별 원본 이름/경로/길이/검사 상태 검색 |
| `Docs/AI/Battlefield6AnimationAcquisitionManifest.csv` | 신규 | 모든 로컬 확보 파일의 경로·크기·SHA256·보관 이유 기록 |
| `LocalAssets/AnimationReview/HumanSoldierAnimationsFREE/` | 위 ZIP/Unity 패키지/FBX/README/약관 기록 신규 | 로컬 검토용이며 제품에는 미연결. 개별 파일은 manifest 전체 목록 참조 |
| `LocalAssets/AnimationReview/QuaterniusCC0/` | 위 9개 `.anim` + 9개 `.meta` + README/LICENSE/Preview 신규 | CC0 검토 후보를 캐시 삭제와 무관한 위치에 보관 |

기존 메서드·필드·직렬화 값·컴포넌트·참조·에셋을 삭제하거나 교체하지 않았다. 새 런타임 책임이나 클래스가 필요하지 않은 수집 단계이므로 별도 게임 시스템을 만들지 않았다. 원본의 캐릭터/전투기/카메라/좌석/입력 설정은 유지한다. 임시 분석·추출 도구와 증거는 Library에만 둔다. 커밋/푸시 미실시.

## 다음 확보 순서

1. **같은 총 모델과 맞는 전용 1인칭 세트**: 대기·걷기·달리기·ADS·발사·장전·교체.
2. **군인식 자세 세트**: 낮은 포복 4/8방향, 앉은 이동/달리기, 서기/앉기/포복 전환, 출발/정지/제자리 회전.
3. **이번 후보의 군사 동작 적합성 확인**: 실제 소총을 붙인 걷기/달리기/장전, 슬라이드와 장애물 접촉을 순서대로 검토. 부적합한 동작은 코드 보정으로 억지 채택하지 않음.
4. **짝 동작과 특수 동작**: 소생/끌기, 총 근접 타격, 승하차, 엄폐/거치/사다리.

영상과 모션이 더 필요하다는 이유로 이번 자료의 상태를 완료로 올리지 않는다. 각 동작은 **확보 → 리타깃 → 원본 동작/무기/지면 비교 → 채택 → 기능 연결**을 구분해 진행한다.
