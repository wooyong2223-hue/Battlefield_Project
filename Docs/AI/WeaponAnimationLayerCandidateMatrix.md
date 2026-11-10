# 총기별 무료 애니메이션 후보와 레이어 적합성

조사일: 2026-10-29. 정책: [외부 클립 우선](WeaponLocomotionAssetSearch.md). 다운로드 완료·현재 적용·웹 설명 확인·미확보를 구분한다. 유료 구매, 기존 클립/코드/마스크 교체는 이번 범위에 없다.

## 현재 구조에서 확인한 사실

- `PlayerCharacter.controller`는 Base Layer, UpperCombat, MeleeCombat 세 레이어다. 세 레이어 모두 IK Pass가 켜져 있고 blendingMode 0(Override)이다. 새 클립을 Additive로 제작됐다고 간주하지 않는다.
- Base Layer에는 Locomotion/CrouchLocomotion/Jump/InAir/Land/Vault/Slide 등 전신 상태가 있다. 전진은 Quaternius, 좌우/후진 일부는 KayKit이다.
- `UpperCombat.mask`의 Humanoid 슬롯은 `[0,0,0,0,0,1,1,1,1,0,0,0,0]`: 양팔·양손가락만 포함한다. Body/Head/Root/다리와 IK 슬롯은 제외한다. 현재 마스크를 상체 전체 마스크라고 설명하면 안 된다.
- `MeleeUpperBody.mask`는 `[0,1,1,0,0,1,1,1,1,0,0,0,0]`: 몸통·머리·팔·손가락 포함, 루트/다리/IK 슬롯 제외.
- UpperCombat의 PistolHold는 CombatPitch(-1/0/1)로 세 조준 클립을 블렌딩한다. 이 마스크에서는 클립의 몸통 조준이 통과하지 않는다. 몸통/머리는 WeaponAimView의 LookAt/관절 처리도 받는다.
- CombatView에는 Carry/Sprint 모드와 가변 UpperCombat 가중치가 있고, AimView는 재생 후 손 IK/몸통 보정을 적용한다. 외부 클립만 바꾸면 기존 보정이 새 포즈를 덮을 수 있으므로 원본 클립 단독 평가와 현재 조합 평가를 구분해야 한다.
- Animator root motion은 이동 권한으로 쓰지 않는다. 현재 CharacterController 이동 구조를 유지하는 시험이 기본이다.

## 소스별 확인 수준

| ID | 소스·조건 | 확인한 내용 | 적용 판단 |
|---|---|---|---|
| Q | [Quaternius UAL1](https://quaternius.itch.io/universal-animation-library), 보유 Standard CC0 | 로컬 클립 `Pistol_Idle_Loop`, `Pistol_Aim_Up/Neutral/Down`, `Pistol_Shoot`, `Pistol_Reload` | 권총 대기·상하 조준·사격·장전 현재 사용/보유. 권총을 든 방향별 전신 이동은 이 클립들에 없음 |
| K | [KayKit Character Animations](https://kaylousberg.itch.io/kaykit-character-animations), CC0 | 보유 ZIP의 GLB JSON을 이번에 재검사. MovementAdvanced에 `Running_HoldingRifle`; CombatRanged에 `Ranged_1H_Aiming/Reload/Shoot/Shooting`, `Ranged_2H_Aiming/Reload/Shoot/Shooting` | 소총 전신 달리기와 한손/양손 상체 동작 시험 후보. 한손은 양손 권총 자세와 다를 수 있음. 총기별 장전 정확성/품질 미검증 |
| M | [MoCap Online Free Pistol](https://mocaponline.itch.io/free-pistol-animation-starter-pack), 무료 다운로드 | 제작자 설명: 전진 조준 walk/jog/crouchwalk, 대기, 상하 aim offsets, 서기/앉기 단발, 회전, 점프. 무료 목록에 장전/좌우후진/질주 포함 확정 불가 | 내용상 권총 이동·상하 조준 유력 후보. [표준 라이선스](https://mocaponline.com/pages/standard-license)는 통합된 작품과 독립 원본 배포를 구분. 공개 Git 원본 재배포 허용을 확인하지 못했으므로 반입 보류. 유료 Pro 목록을 무료 내용으로 계산하지 않음 |
| Z | [ZenXChaos TPS AnimationSets](https://github.com/ZenXChaos/ThirdPersonShooter-AnimationSets), 저장소 Unlicense 표시 | Revolver Walk/CrouchWalk/상하좌우 조준/장전 단계, Shotgun Idle/Walk가 README에 있음 | 원 제작 출처·각 파일 권리·Humanoid 리타깃 미확인. 제품 반입 보류. 리볼버를 일반 양손 권총, 산탄총을 소총 전용 모션으로 간주하지 않음 |
| T | [Toolbench Low-poly Squad Kit](https://toolbenchdev.com/gamedev/cc0-game-assets/low-poly-squad-kit/), 제작자 CC0 표기 | 53 actions, 예: `walk_rifle`. 기본 idle/walk/run/aim/fire/reload 등과 무기 파지 조합 설명. 무기 모델은 권총·SMG·소총·산탄총·저격총·로켓 포함 | 확장 조사 후보. 자체 스크립트로 제작됐고 Unity Generic/22bones 안내라 현재 Humanoid 호환성·사실적 품질 미확정. 무기 모델 개수를 전용 장전/이동 클립 개수로 계산하지 않음. 아직 다운로드/리타깃 안 함 |
| C | [Cransh 권총](https://sketchfab.com/3d-models/fps-pistol-animations-0d7a343dcb6f401197a73c91aee93f6d), [소총](https://sketchfab.com/3d-models/animated-fps-hands-rifle-animation-pack-5f2d0ed780a94724b36ab505f7564057), [산탄총](https://sketchfab.com/3d-models/fps-arms-remington-shotgun-e68ef617fe8a48cca8610d016ffd5881), [저격총](https://sketchfab.com/3d-models/fps-animations-sniper-rifle-c15ae8393d824f5b929e3f69691cdd31) | 제작자 페이지 검색에 CC Attribution 표시. 원본 직접 열기는 일부403. 원본 팔/총은 다른 저작자이며 개별 조건 추가 확인 필요 | FPS 팔 리그 후보. 전신 Humanoid나 3인칭 이동의 대체품으로 확정하지 않음. 산탄총은 제작자가 왼팔 지연 문제 명시. 세부 클립 목록/원본 구성물 조건/다운로드 미검증 |

## 총기 종류별 매칭

전신=기본 레이어 후보, 부분=발사·장전·조준 시 마스크 레이어 후보. 아래의 공유는 구조적 가능성으로, 실제 호환성 검증 완료가 아니다.

| 종류 | 전신 이동 후보 | 조준·발사·장전 후보 | 아직 필요한 전용 동작 |
|---|---|---|---|
| 반자동 권총 | M 전진 조준 이동, T 상세 검사 필요 | Q 현재 사용; K 1H 비교, C FPS 별도 | 양손 파지 방향별 이동·질주, 탄 남음/빈 탄창 장전, 꺼내기/집어넣기 |
| 리볼버 | Z Walk/CrouchWalk, M의 리볼버 호환성 검사 | Z 조준9방향·장전 단계 | 실린더·탄피배출·스피드로더/개별 장전·해머가 실제 포함됐는지 확인 |
| 기관단총 | K 소총 이동 공유 가능성 검사; T 상세 검사 | K 2H는 일반 동작 비교용 | 짧은 파지 전용, 탄창·노리쇠 조작, 방향별 이동 |
| 돌격소총 | K `Running_HoldingRifle`, T `walk_rifle` | K 2H, C FPS 별도 | 조준 전후좌우 이동, 질주, 점사/연사 읽힘, 일반/빈 탄창 장전·전환 |
| 전투소총/DMR | 소총 전신 후보 공유 검사 | K 2H 비교 | 무게/견착·단발 반동, 실제 탄창·노리쇠 조작 |
| 볼트액션 저격총 | 소총 이동 공유 검사; T 상세 검사 | C 저격 FPS 별도 | 전신 볼트 조작·개별탄/탄창 장전·엎드린 사격, 조준 복귀 |
| 펌프 산탄총 | Z Shotgun Walk; T 상세 검사 | C 산탄 FPS 별도 | 펌프 사이클·한발씩 장전·장전 중단, 전신 조준 이동 |
| 반자동 산탄총 | 소총/산탄 이동 후보 공유 검사 | K 2H는 비교용 | 튜브식/탄창식 장전 분리·반자동 사격 |
| 경기관총/지원화기 | 적합한 무료 전용 전신 후보 미확보 | 적합한 무료 전용 부분 후보 미확보 | 무거운 휴대·연사·탄띠/탄창·덮개·양각대·엎드린 운용 |
| 중기관총/거치화기 | 이동 후보 해당 없음; 탑승/이탈 미확보 | 운용자 전용 클립 미확보 | 거치 조작·발사·장전·사용 진입/종료. 포탑 모델 애니메이션과 구분 |
| 로켓/미사일 발사기 | T에 모델은 있으나 전용 이동 클립 미확인 | 재배포 가능한 전신 조준/장전 세트 미확보 | 어깨 올림/내림·휴대·발사·로켓/발사관 교환 |

## 레이어를 고려한 선정/적용 방침

1. **무기별 전신 이동은 Base Layer 후보로 검사.** 일반 다리에 아무 상체나 덮는 것보다 원본의 몸통 기울기·팔·보행 위상을 먼저 확인한다. 질주/포복/점프/장애물 넘기는 전신 동작을 우선한다.
2. **이동 중 발사/장전은 부분 재생 후보.** 전신 클립도 Humanoid면 마스크로 제한할 수 있지만 몸통과 골반의 움직임이 잘려 부자연스럽지 않은지 확인한다. 기존 팔 전용 마스크로 충분한 클립과 몸통이 필요한 클립을 구분한다.
3. **상하 조준은 별도 클립 세트 우선.** 위/정면/아래 동일한 파지의 조준 자세를 블렌딩하고, 새 클립의 몸통을 살릴 경우 기존 LookAt/관절 보정과 역할 중복을 제거할 계획을 먼저 제시한다. 이동 클립만으로 모든 마우스 상하 각도에 대응한다고 보장하지 않는다.
4. **전신과 부분 레이어 중복 평가.** 질주·전환 전신 모션 위에 항상 조준 상체를 덮으면 원본 의도를 잃는다. 적절한 레이어 가중치/전환은 클립 선정 후 결정한다. 이번에는 값 변경 없음.
5. **손 IK는 접점 보정만 우선.** 새 원본의 손/총 접점을 먼저 보고 평가한다. Carry35/Sprint55, 기존 어깨 안정화가 원본 클립을 덮지 않는지 별도 비교한다. 이번에는 자동 비활성화/삭제하지 않는다.
6. **권총·소총 우선 검증.** K의 rifle-run/2H 세트와 Q 권총을 기준으로 비교하고, 재배포가 해결되지 않은 M/Z와 Generic T는 제품 반입 전에 각각 조건/호환성 관문을 통과해야 한다. 적합하지 않으면 다음 후보를 찾으며 임의 관절 제작으로 완료 처리하지 않는다.

## 모든 총기에 남는 공통 누락 검사

걷기/뛰기 전후좌우 및 대각선 연결, 조준 이동, 질주 진입/종료, 상하 조준, 발사, 일반/빈 장전, 장비전환, 앉은 이동, 엎드린 이동, 자세 전환, 시작/정지, 제자리 회전, 점프/낙하/착지, 총 근접, 장애물 넘기를 각각 확인한다. 미확인 동작은 다른 클립을 보유했다는 이유로 완료 표시하지 않는다. 대각선 블렌딩이나 상하체 조합이 가능한지도 리타깃 후 판단한다.

## 이번 변경/검증

이 문서 하나를 신규 작성했다. 기존 조사 문서, 스크립트, 클립, 마스크, Controller, 씬, 직렬화 설정은 수정/삭제/교체하지 않았다. Controller/두 마스크/CombatView/AimView는 직전 격리 검증본과 해시 동일. 마스크를 Humanoid 슬롯별로 해독하고 연결 GUID를 확인했다. 보유 KayKit ZIP의 실제 GLB 애니메이션 이름을 읽었으며 Q의 로컬 수집 목록도 대조했다. 외부 후보는 제작자 페이지/공개 저장소 설명 수준이며 새 다운로드·리타깃·PlayMode·시각 품질 검증은 하지 않았다. 새 클래스/인터페이스/OOP/SOLID 구조 변경 없음. 원본 모델 적용, 레이어 혼합, 손/총 접점, 발 접지, 시점별 상하 조준은 Unity에서 후속 검증해야 한다.
