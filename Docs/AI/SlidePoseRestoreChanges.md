# 원본 슬라이드 자세가 사라진 하체 마스크 수정

사용자가 하체 분리 이후 다른 자세가 섞이고 원본 슬라이드가 보이지 않는다고 보고했다. 이전의 레이어 활성/마스크 부위 검사는 통과했지만 실제 낮아지는 자세 보존 검증이 부족했다. 이를 성공한 시각 결과로 간주하지 않는다. 직전14개 파일 해시와 현재 파일은 동일했고 대상5개 before.json을 보존/반영 직전 재확인했다.

## 원인 분리와 증거

- 이전 mask는 다리만 허용하고 Root/Body를 제외했다. 기본 crouch pose가 남아 원본의 골반 높이/회전이 보존되지 않았다. 실제 이전 캡처는 서서 무릎을 드는 듯한 모습이다.
- Root만 추가한 후보도 골반 높이가 같은 frame15 약1.48m로 원본의 낮은 자세와 달랐다.
- 임시 독립 AnimationLayerMixerPlayable 실험으로 Base=CrouchIdle, source=원본slide, 시간42.67%를 고정하고 마스크를 비교했다. Root+legs 골반 euler 약(25,93,340), Root+Body+legs 약(16,60,300), 원본 전체 약(20,67,309). 이 Humanoid 리그에서 Body 제외가 골반 회전에 영향을 주는 것을 확인했다. 일반적인 다리 회전값만 켜면 된다고 추측하지 않는다.
- Root/Body+다리를 포함하고 평가 순서를 수정한 실제 캐릭터 캡처 frame15 골반 높이는 약0.68m였다. 새 반복 검사에서는 일반 crouch1.107m, 원본 전신slide 최소0.597m, 수정된 분리 재생 최소0.486m(소총/권총)·0.553m(맨손)로 측정됐다. 원본의 내려앉는 특징이 다시 보이는지 수치와 화면으로 확인했다.
- 상체 IK가 끝난 뒤 Root를 덮지 않도록 lower layer를 upper layers 앞으로 옮겼다. 원본/기존/Root만/골반 포함 후보를 같은 clip/time/장비 조건에서 비교했다. 원본 FBX/키프레임/임포트 설정은 바꾸지 않았다.

## 변경 파일

| 파일 | 변경 및 이유 |
|---|---|
| `Assets/Features/PlayerCharacter/Animation/SlideLowerBody.mask` | Root/Body false→true. LeftLeg/RightLeg true 유지, Head/팔/손가락/IK false 유지. 원본 골반과 다리 자세 보존. Body 채널을 포함하므로 이전의 다리만 허용이라는 설명을 철회한다. 상체 최종 맞춤은 뒤의 기존 무기/상체 IK가 담당 |
| `Assets/Features/PlayerCharacter/Animation/PlayerCharacter.controller` | Base0/Upper1/Melee2/Reload3/Lower4 → Base0/Lower1/Upper2/Melee3/Reload4. 레이어 수5/상태/클립/전환/파라미터/weight 기본값 유지. 골반 평가 뒤에 손·총·상체를 맞추는 순서로 교체 |
| `Assets/Features/PlayerCharacter/Editor/PlayerCharacterAimDiagnostics.cs` | upperWeight/upperState의 고정index1을 UpperCombat 이름 조회로 교체. 순서 변경 뒤 올바른 상체 정보를 기록 |
| `Assets/Tests/PlayMode/PlayerCharacterNativeAimInputTests.cs` | 디버그 upperWeight 조회1을 UpperCombat 이름으로 변경. 기존 테스트 동작/조건 유지 |
| `Assets/Tests/PlayMode/SlideAnimationOverrideTests.cs` | 마스크 기대값Root/Body 포함으로 변경. 새 MaskedSlideKeepsTheOriginalLowPelvisInsteadOfTheCrouchPose: 소총/권총/맨손 각각 원본 전신 재생과 분리 재생을 실제 Scene에서 비교. 중앙 진행30–55% 골반 최소 높이가 일반앉기보다>.3m 낮고 원본과 차이<.25m인지 확인. 무한대 초기값으로 샘플이 없으면 실패. 기존 이동/접지/상체/시간/종료 기준 유지 |
| `Docs/AI/SlidePoseRestoreChanges.md` | 이 기록 |
| `Docs/AI/UnityProjectContext.md` | 잘못된 이전 가정 철회, 실제 포즈 보존 방식/검증 추가 |

## 교체/유지 내역

이전 `Root=false,Body=false,legs=true` 구성을 `Root=true,Body=true,legs=true`로 교체했다. 원본의 골반 동작을 제외한 채 기본 crouch 위에 다리만 올리던 구현을 그대로 유지하지 않는다. lower-last 순서를 lower-before-upper로 교체했다. 고정index1 진단2곳도 이름 조회로 바꿨다. 원본 motion/게임플레이/카메라 코드는 수정하지 않았고 기존 메서드/필드/컴포넌트/에셋 삭제 없음. 신규 제품 에셋/씬/메타 파일 없음. C키/0.75초/종료서기/머리 위 공간검사/기존상체그립/SlideTime 유지/접지fade 유지.

원본의 골반을 살리려면 이 리그에서 Body 입력도 필요했다. 따라서 단순히 원본의 두 다리 채널만으로 완전한 slide를 재현한다고 설명하지 않는다. 원본 골반+다리를 우선 보존하고, 상체는 기존 조준·무기 레이어/IK가 뒤에서 처리한다. 상체 혼합이 아예 없다는 뜻은 아니며 그 혼합은 의도된 장비 기능이다.

## 설계 검토

포즈 선택은 기존 AvatarMask, 평가 순서는 기존 controller, 시각 결과 보장은 실제 비교 테스트로 해결했다. 새로운 보정 manager나 수동 관절 키프레임을 만들지 않았다. 기존 책임/읽기 전용 데이터/명시적 컴포넌트 주입과 조합을 유지하며 불필요한 상속/인터페이스를 추가하지 않았다. OOP/SOLID상 추가 예외 없음.

## 검증

- Unity6000.4.8f1 격리 프로젝트 `-batchmode -noaudio`, 최종 **184/184 통과** (182.4초): 제품183개+진단 캡처1개. 집중49/49. 로컬 Mixamo 분리 fallback **9/9 통과**.
- 새 시각 포즈 검사 실제 측정: crouch1.1074219, source0.5966797; masked rifle/pistol0.4863281, unarmed0.5527344. 소스 native preview와 픽셀 단위 동일함을 보증하는 수치가 아니라 골반이 일반 crouch로 대체되지 않았는지 확인하는 기준이다.
- 소총/권총/맨손, TP/FP 총48장 캡처/정적 비교. before-after comparison.png 검토. 이전 구현처럼 레이어 weight만 검사하지 않고 실제 포즈를 검사한다. 기존 모터/루트 고정/접지/레이어 fade/상체 유지/조준/장전/탑승 회귀 포함.
- 제품5개 파일은 테스트 프로젝트와 바이트 동일. 원본 FBX/임포트meta/프로필/무기asset/scene 값 유지, 제품 코드에 임시 진단로그/씬 추가 없음. 임시 mixer 실험·캡처·Editor 재정렬 도구는 Library에 보관. 영구 audio/AudioSource/커밋/스테이징 변경 없음.
- 남은 확인: 사용자 Editor에서 이 원본 기반 자세의 자연스러움, 극단 조준/경사/벽 아래/동작 도중 장비 전환. 특정 게임의 모습과 동일하거나 상체에 원본 영향이0이라고 주장하지 않는다. Player 빌드 미실행.

증거 `Library/CodexSlidePose/{before.json,root.xml,mask.xml,mask.log,pelvis.xml,focused.xml,final.xml,fallback.xml,comparison.png,applied.json,pelvis_slot*.png}`.
