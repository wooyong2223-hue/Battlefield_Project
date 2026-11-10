# 총기 애니메이션 리타깃·레이어·실행 검증

검증일: 2026-10-29. Unity 6000.4.8f1 / DX11 / 격리 프로젝트 `Library/CodexInfantryWork/Project`.

## 결론

**보유 Quaternius/KayKit의 기술 재생 검증은 수행했다. KayKit을 현재 캐릭터에 그대로 교체하는 안은 품질/파지 기준에서 보류한다.** 다른 웹 후보까지 모두 검증 완료한 것은 아니다. 기존 제품 파일을 수정하거나 새 후보를 제품에 적용하지 않았다. 이번은 후보 평가이며 원래 사용자가 보고한 상체/팔 추적 오류를 수정한 작업도 아니다.

- 15클립 × 전신/팔 전용/몸통 포함 3방식 = 45조합, 각각 48시점 샘플. Humanoid 모션, 유한 좌표, 루트 이동 없음, 부분 레이어의 다리 보존 기술 검사 통과.
- 실제 컨트롤러/기존 손 IK로 3세트 × 1·3인칭 × 서기·앉기 × pitch -60/0/60 = 36조건 측정.
- 전체 PlayMode **137/137 Passed**, 실패/건너뜀 0. 제품 테스트133개 + 기존 임시 캡처3개 + 이번 후보 진단1개다. 후보 진단 통과가 후보의 시각 품질 통과를 의미하지 않는다.
- 원본 캐릭터 코드/클립/마스크/Controller/씬의 이번 작업 전후 SHA256 차이 없음.

## 작업 전 비교와 구조

캐릭터 기능 및 PlayMode 파일을 이전 격리 검증본과 비교했다. SampleScene만 바이트 해시가 달랐고, 줄바꿈 정규화 후 텍스트 diff는 없었다. 원본은 보존하고 격리 씬만 최신 원본 바이트로 맞췄다.

기존 구현/직렬화 필드/참조/메서드/클립/컴포넌트를 제거하거나 대체하지 않았다. 기존 사용자 설정이나 계층 변경도 없다. 검증은 제품에 새 책임을 추가하지 않고 격리 Editor 리타깃 도구, 가져오기 진단 도구, PlayMode 시나리오로 나눴다. 제품 OOP/SOLID 구조 변경/예외 없음.

## 실제 검사한 클립

KayKit 원본은 기존 `Library/CodexInfantryDetail/KayKit-Character-Animations-Free.zip`에서 추출했다. 원본 FBX는 편집하지 않았다.

| 소스 | 검사 클립 |
|---|---|
| KayKit MovementAdvanced | `Running_HoldingRifle` |
| KayKit CombatRanged | `Ranged_1H_Aiming`, `Ranged_1H_Reload`, `Ranged_1H_Shoot`, `Ranged_1H_Shooting` |
| KayKit CombatRanged | `Ranged_2H_Aiming`, `Ranged_2H_Reload`, `Ranged_2H_Shoot`, `Ranged_2H_Shooting` |
| 보유 Quaternius UAL1 | `Pistol_Idle_Loop`, `Pistol_Aim_Up`, `Pistol_Aim_Neutral`, `Pistol_Aim_Down`, `Pistol_Shoot`, `Pistol_Reload` |

KayKit은 격리 FBX만 Human/CreateFromThisModel/재질 미가져오기로 설정했다. 추출한 격리 `.anim` 복사본은 Root Rotation Bake Into Pose와 원본 방향 보존을 사용해 원래 루트 회전 추출 문제를 비교에서 제외했다. 키프레임/관절 포즈를 임의로 고치지는 않았다. 후속 실험은 제작자가 제공한 Generic `T-Pose` 클립을 샘플링한 기준 골격을 HumanDescription에 지정한 뒤 재가져오기했다. 팔 벌어짐과 주요 측정값은 개선되지 않았다. 이 설정들은 원본 프로젝트에 반영하지 않았다.

## 상하체 레이어 결과

동일 Armature/Avatar/모델 배율2.270328, root motion off, Quaternius Jog를 기본 이동으로 사용했다. AnimationLayerMixerPlayable의 두 번째 입력 가중치를0/1로 바꾸며 같은 시점의 기본 다리 자세와 비교했다. 실제 제품 마스크를 그대로 참조했다.

| 재생 방식 | 확인 결과 | 해석 |
|---|---|---|
| 전신 | 각 소스 클립의 다리/몸통/팔 재생 | 이동 클립은 전신으로 평가해야 한다. 조준 대기 클립을 전신으로 덮으면 달리기 다리까지 대기로 대체된다 |
| UpperCombat 팔 전용 | 모든 샘플에서 기본 이동 대비 다리 회전 차이0, 몸통/머리 로컬 회전 차이0 | 현재 마스크가 팔만 통과시키는 것을 실제 재생으로 확인 |
| MeleeUpperBody 몸통 포함 | 다리는 기본 이동과 동일, 몸통/머리는 후보 클립으로 변경 | 상하 조준 원본의 몸통 움직임을 살릴 수 있지만 기존 LookAt/몸통 보정과의 중복 검증이 필요 |

Quaternius `Pistol_Aim_Up/Down`은 팔 전용에서 팔만 위/아래로 움직이고, 몸통 포함에서는 원본의 등/몸통 굽힘이 나타났다. 이것만으로 마스크 변경을 제품 수정안으로 확정하지 않는다. 몸통 포함 영상은 **기존 런타임 IK/LookAt을 끈 순수 레이어 실험**이므로 실제 조준 오차/시선·머리 연결/질주·장전·근접 충돌은 추가 구현 검증 대상이다.

## 실제 컨트롤러와 IK 결과

원본 SampleScene을 PlayMode로 열고 기존 CharacterViewTestContext로 캐릭터를 격리했다. 기존 권총 모델/그립 설정과 몸통/손 IK를 유지했다. 테스트용 AnimatorOverrideController로 PistolHold 3클립/Shoot/Reload만 K1H 또는 K2H로 임시 매핑했다. K2H를 실제 소총 모델에 맞춘 검증은 아니다. 소총 모델을 새로 연결하지 않았다.

| 세트 | 3인칭 조준 총구 오차 최대 | 1인칭 조준 총구 오차 최대 | 판단 |
|---|---:|---:|---|
| 현재 Quaternius | 0도(측정 정밀도 내) | 약0.063도 | 기존 조준 정렬 기준선 유지. 자연스러운 PUBG 움직임 전체를 보장하지 않음 |
| KayKit 1H 임시 대체 | 0도(기존 손 IK 작동) | 약94.88도 | 현재 권총 그립/1인칭 리그에 그대로 대체 불가 |
| KayKit 2H 임시 대체 | 0도(기존 손 IK 작동) | 약31.83도 | 현재 권총 그립/1인칭 리그에 그대로 대체 불가 |

이 오차는 Unity의 **Muzzle.forward 대 AimRotation.forward**이며 총 메시 자체의 시각 정렬이나 총기별 적정 손잡이 위치를 보장하지 않는다. 현재 권총 그립을 고정했기 때문에 KayKit을 해당 제작자 모델/소총에 썼을 때의 품질을 평가한 결과도 아니다.

KayKit 순수 재생에서 양손 간격은 2H Aiming 약1.76~2.21m, rifle-run 약1.74~1.89m로 벌어졌다(현재 캐릭터 배율 포함). 팔이 옆으로 벌어지는 모습이 프레임에서도 확인된다. 현재 3인칭 IK는 손 간격을 약0.25m로 다시 잡아 이 문제를 가린다. 따라서 3인칭 총구 오차가 작다는 사실만으로 리타깃이 적합하다고 판정하지 않는다. KayKit 소스 전체가 잘못됐다고 단정하지 않으며, 현재 자동 매핑/기준 자세/모델 비율/파지 조합은 채택 보류다.

신규 진단 테스트의 자동 assertion은 유한 값/캐릭터 루트 회전 보존/이동 발 움직임을 검사한다. 조준 정렬 채택 기준2도를 적용하면 K1H/K2H의 1인칭 각6조건은 실패다. 이를 테스트 전체137개 통과와 혼동하지 않는다. KayKit 발사/장전은 순수 리타깃·레이어 재생까지 검사했고, 새 무기의 탄창/탄띠/볼트 및 발사·장전 타이밍 기능은 검증하지 않았다.

## 시각 자료와 증거

기준 디렉터리: `Library/CodexWeaponCandidateValidation`.

- `final-comparison.jpg`: KayKit 소총 달리기/2H 조준과 Quaternius 상향 조준 마스크 비교. 프레임 직접 확인.
- `runtime-comparison.jpg`: 기존 컨트롤러·손 IK가 켜진 현재 Q/K1H/K2H 비교. 직접 확인. K에서는 IK가 원본 손 자세를 덮는 것이 전제다.
- `K_Running_HoldingRifle-full.mp4`, `K_Ranged_2H_Aiming-upper.mp4`, `K_Ranged_2H_Reload-upper.mp4`, `Pistol_Aim_Up-upper.mp4`: 원본 길이에 맞춰48시점 샘플링 후30fps로 출력. 기본 이동의 위상은 같은 시간으로 평가. 고정 배경 제자리 재생이므로 실제 이동 속도/발 접지 품질 판정용은 아님.
- `CurrentQuaternius-runtime.mp4`, `KayKit1H-runtime.mp4`, `KayKit2H-runtime.mp4`: 1초/24fps 실제 이동·기존 조준 코드·수동 Animator 평가. Animator 자동 평가를 끄고 중복 업데이트를 방지했다.
- `retarget.json`: 최종45조합 측정. `retarget-before-tpose.json`: 이전 가져오기 기준 비교.
- `Runtime/results.json`: 마지막 전체 실행의36조건 결과. `runtime-before-tpose.json`: 이전 기준 비교.
- `retarget-final.log`, `retarget-tpose.log`, `import-diagnosis.txt/log`: 초기 재촬영/T-Pose 실험 기록. 최초 촬영은 구도 오류로 시각 판단에서 제외했다.
- `runtime.xml/log`: 신규 진단 단독1/1. `full.xml/log`: 전체137/137.
- `baseline.json`, `final-audit.json`: 원본 보존/실행 결과. `isolated-files.csv`: 격리 파일 전체 경로·크기·SHA256.

## 남은 후보/한계

- Toolbench 공식 ZIP을 직접 받으려 했지만 HTTP403으로 실패했다. 웹 도구도 ZIP을 읽지 못했다. 파일을 확보하거나 Unity에 가져온 것으로 표시하지 않는다.
- MoCap Online은 무료 다운로드와 공개 원본 재배포 권한이 구분된다. 원본 공개 조건 미확인 상태를 유지하며 이번에 가져오지 않았다.
- ZenXChaos는 개별 원출처/권리 미확인, Cransh는 FPS 전용 리그·원본 구성물 조건/파일 미확보 상태로 이번 실행 검사 대상에 포함하지 않았다.
- 따라서 기관총/발사기/볼트·펌프 등 필요한 총기 전체 목록의 채택 완료가 아니다. 현재 확보분을 검사해 부적합한 직접 교체를 걸러낸 단계다.
- FBX 두 개에 Unity의 animation import warning이 남아 있다. 로그에서 확인한 문구는 Import Messages 확인 안내이며 세부 원인은 이번 API 로그만으로 확정하지 않았다. `Avatar.isHuman/isValid` 통과로 이 경고를 무시하지 않는다.
- 원본 사용자 Editor 조작/실제 사용자 PlayMode 청취나 조작감 평가/플레이어 빌드/네트워크 검증은 하지 않았다. 마우스 상하 문제가 발생한 정확한 휴대/질주 상태의 원인 수정은 별도 작업이다.

## 변경 파일과 제거/대체 내역

제품 저장소에 추가한 파일은 **이 문서 하나**다. 원본 Assets/설정/기존 문서 변경이나 코드·필드·참조·컴포넌트·에셋 제거/대체 없음.

격리 프로젝트 전용 신규 파일(각 Unity `.meta` 포함):

- `Assets/Editor/ValidateWeaponCandidates.cs`: FBX 가져오기/격리 추출본 생성, 45조합 측정/렌더, 루트·다리·유한 값 검사. 초기 카메라 고정 구도를 메시 bounds 기반으로 교정했고, 반복 실행 시 기존 격리 추출본 내용만 갱신한다.
- `Assets/Editor/InspectWeaponRetarget.cs`: 원본 매핑17본/가져오기 로그 출력, 제공 T-Pose 기준 실험. 파생 Generic FBX는 진단용이다.
- `Assets/Tests/PlayMode/CodexWeaponCandidateRuntime.cs`: 기존 컨트롤러 임시 오버라이드/36조건 측정, 상태/좌표 진단, 캡처 및 결과 저장.
- `Assets/CodexWeaponValidation/CombatRanged.fbx`, `MovementAdvanced.fbx`: 무료 KayKit 원본 복사본.
- 같은 폴더 `CombatRangedGeneric.fbx`, `MovementAdvancedGeneric.fbx`: T-Pose 기준 검사용 복사본.
- 같은 폴더 `K_Running_HoldingRifle.anim`, `K_Ranged_1H_Aiming.anim`, `K_Ranged_1H_Reload.anim`, `K_Ranged_1H_Shoot.anim`, `K_Ranged_1H_Shooting.anim`, `K_Ranged_2H_Aiming.anim`, `K_Ranged_2H_Reload.anim`, `K_Ranged_2H_Shoot.anim`, `K_Ranged_2H_Shooting.anim`: 9개 격리 추출 클립.
- `Assets/CodexWeaponValidation.meta`: 격리 폴더 메타.

위 파일33개 전체가 `Library` 아래에 있어 제품의 Assets/Tests/배포에는 들어가지 않는다. 격리 도구는 출력 리소스/PlayableGraph를 정리하며 제품에 인터페이스나 보정 책임을 새로 추가하지 않는다. 기존 테스트를 끄거나 assertion/기준값을 완화하지 않았다.
