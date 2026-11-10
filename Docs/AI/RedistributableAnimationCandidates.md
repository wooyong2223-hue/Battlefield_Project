# 원본 재배포 가능한 캐릭터 애니메이션 재탐색

> 사용자가 Quaternius UAL1/2를 메인으로 확정했고 대응 기존 모션도 교체했다. [현재 적용과 검증](QuaterniusMainAnimationChanges.md). 다음 후보/미확보 목록은 조사 이력이며 이미 적용된 범위는 최신 기록을 우선한다.

확인일: 2026-10-28. 최신 사용자 요청은 **원본 애니메이션을 공개 Git 저장소에 포함할 수 있는 에셋으로 다시 선정**하는 것이다. Kevin 병사 팩의 제품 적용 작업은 중단했다. 그 실험은 격리 프로젝트에서만 수행됐으며 제품 코드·씬·Animator에는 반영하지 않았다.

## 선정 기준

- CC0: 원본·수정본을 복제/수정/배포할 수 있는 우선 후보. [Creative Commons 원문](https://creativecommons.org/publicdomain/zero/1.0/).
- CC BY: 재배포 후보에 포함하되 저작자·출처·라이선스와 수정 내역을 함께 관리한다. [CC BY 4.0 조건](https://creativecommons.org/licenses/by/4.0/).
- 무료/royalty-free 또는 게임 내 사용 허용만으로 원본 공유 가능으로 판정하지 않는다.
- 애니메이션과 함께 오는 팔·총·텍스처가 다른 제작자의 작품이면 그 출처도 확인한다. 코드 저장소의 라이선스를 에셋에 자동 적용하지 않는다.
- 라이선스 통과와 배필식 동작 품질은 별도 판정한다. 부적합한 기어가기/포복 후보를 재배포 가능하다는 이유만으로 채택하지 않는다.

## 우선 후보 — CC0

| 후보 | 확인한 내용 | 이번 판정 |
|---|---|---|
| [Quaternius Universal Animation Library](https://quaternius.itch.io/universal-animation-library) | 제작자 CC0 표기 및 보유 Standard의 License.txt 확인. 걷기/달리기/점프/앉기/권총/피격/사망/앉기/구르기/수영 등 포함 | 원본 공유 가능한 기본 전신 모션 후보. 보유 무료 Standard는 실제 43클립이며 페이지의 120+ 전체 목록과 혼동하지 않음 |
| [Quaternius Universal Animation Library 2](https://quaternius.itch.io/universal-animation-library-2) | 제작자 CC0 표기 및 보유 Standard의 License.txt 확인. Slide_Start/Loop/Exit, ClimbUp_1m, OverhandThrow 등 | 원본 공유 가능한 특수 동작 후보. 보유 무료 Standard 43클립. 슬라이드/올라가기의 지면·장비 접점은 추가 확인 필요 |
| [KayKit Character Animations](https://kaylousberg.itch.io/kaykit-character-animations) | 제작자 CC0 표기와 무료 ZIP 내 라이선스 확인. 실제 Rig_Medium GLB에서 아래 이름 확인 | 소총을 든 이동·조준/발사/장전·근접 후보로 재검토 우선. 원본 FBX/GLB 공유 가능 후보 |

Quaternius 제작자의 [FAQ](https://quaternius.com/faq.html)도 CC0 및 수정 가능을 설명한다. 유료 Pro/Source 판이 있지만 이번에 구매하거나 파일을 확보한 것은 아니며, 유료판의 실제 구성/허가문까지 확인한 것으로 표시하지 않는다.

### KayKit에서 실제 확인한 클립

공식 무료 아카이브: `Library/CodexInfantryDetail/KayKit-Character-Animations-Free.zip`.
SHA256: `65882f31f905ad2e953819648a59287cdeab8f623908d5ef701971d3758be20f`.

| 파일 | 실제 애니메이션 이름 | 목적 |
|---|---|---|
| `Rig_Medium_MovementAdvanced.glb` / 동일 FBX 세트 | `Running_HoldingRifle` | 소총을 든 달리기 후보 |
| 같은 세트 | `Running_Strafe_Left`, `Running_Strafe_Right`, `Walking_Backwards` | 측면/후방 이동. 현재 제품에 추출본이 이미 있음 |
| `Rig_Medium_CombatRanged.glb` / 동일 FBX 세트 | `Ranged_2H_Aiming`, `Ranged_2H_Shoot`, `Ranged_2H_Shooting`, `Ranged_2H_Reload` | 양손 총 조준·단발·연속 발사·장전 후보 |
| 같은 세트 | `Ranged_1H_Aiming`, `Ranged_1H_Shoot`, `Ranged_1H_Shooting`, `Ranged_1H_Reload` | 한손 총 후보 |
| `Rig_Medium_CombatMelee.glb` / 동일 FBX 세트 | `Melee_Unarmed_Idle`, `Melee_Unarmed_Attack_Punch_A`, `Melee_Unarmed_Attack_Kick` | 맨손 대기·주먹·발차기 후보 |

전체 아카이브 내부 경로는 `KayKit_Character_Animations_1.1/Animations/gltf/Rig_Medium/` 및 `Animations/fbx/Rig_Medium/`다. GLB의 JSON 애니메이션 목록을 읽어 이름을 확인했다. 이번 재탐색에서 새 리타깃/품질 검사를 완료한 것은 아니다. 특히 KayKit의 Crawling은 이전 렌더에서 군인식 낮은 포복에 맞지 않아 제외한 상태를 유지한다.

## 1인칭 품질 후보 — CC BY 표시, 아직 미확보

다음은 제작자 업로드 페이지의 라이선스 표시와 설명을 확인한 후보다. 이번 직접 원본 페이지 열기는 403으로 실패했고, 검색에 제공되는 제작자 페이지 내용으로 확인했다. 로그인 우회나 비공개 다운로드는 하지 않았다. 다운로드 파일의 개별 허가문·전체 클립명·Unity 호환성·원본 구성물 권리는 아직 검토하지 않았다.

| 후보 | 표시·내용 | 남은 확인 |
|---|---|---|
| [Cransh — Animated FPS hands (rifle animation pack)](https://sketchfab.com/3d-models/animated-fps-hands-rifle-animation-pack-5f2d0ed780a94724b36ab505f7564057) | CC Attribution 표시. 1인칭 소총 팔/리그/모션. 팔은 bumstrum, 소총은 doomsentinel 출처를 명시 | CC BY 정확한 버전·원본 팔/총의 조건 및 크레딧 확인. 제작자가 일부 총 정점 문제를 언급하므로 파일 확인 필요 |
| [Cransh — FPS AK-74m animations](https://sketchfab.com/3d-models/fps-ak-74m-animations-94be8385c402474cacd39bc096c6ca14) | CC Attribution 표시. AK-74m은 creationwasteland, 팔은 DJMaesen 출처 명시 | 각 구성물의 재배포 조건과 총/손 접점 확인 |
| [Cransh — FPS pistol animations](https://sketchfab.com/3d-models/fps-pistol-animations-0d7a343dcb6f401197a73c91aee93f6d) | CC Attribution 표시. 권총 팔/리그/모션. 팔은 bumstrum, 권총은 raimeiyonke 출처 명시 | 원본 구성물의 조건과 실제 모션 세트 확인. 기존 권총을 그대로 쓸 수 있다고 확정하지 않음 |
| [1Matzh — Fists (2025), First Person Animations](https://sketchfab.com/3d-models/fists-2025-first-person-animations-c7e8f959c4094be991b02213d4e66c20) | CC Attribution 표시, 제작자 설명에 카메라 동작을 포함한 16개 모션. 팔 원본은 NoneCG 작품을 수정했다고 명시 | 원본 팔의 재배포 조건 확인 전에는 패키지 전체 채택 보류. 16개 모션의 개별 이름/파일도 미확인 |

출처 표기가 허용되는 후보는 검색 범위에 포함하지만, 위 항목을 **이미 원본 공유 검증이 끝난 파일**로 표시하지 않는다. 전용 1인칭 세트를 확보하면 현재 전신 Humanoid 모션과 별도 리그로 평가해야 한다.

## 이번 기준에서 제외/보류

- **Kevin Iglesias Human Soldier Animations FREE:** 제작자 조건이 원본 공유를 제한하므로 이번 공개 저장소용 선정에서 제외. 보유 로컬 복사본은 기존 Git 제외 폴더에 유지하며 자동 삭제하지 않는다. [제작자 사용 조건](https://www.keviniglesias.com/).
- **WRAD ARMS:** [제작자 저장소](https://github.com/wwwriks/wrad-arms)는 CC0 팔 모델·IK 리그를 설명하지만 완성된 군사 모션 세트로 확인한 것은 아니다. 모델/리그 후보와 모션 후보를 구분한다.
- **PSX FPS Arms Free:** 검색에서 CC BY 표기가 보였지만 원본 페이지가 현재 404여서 이번에는 확정 후보에서 제외한다.
- **군인식 낮은 포복·방향별 전환·소생/끌기 짝 모션·승하차:** 이번 검색에서 원본 공유 가능 조건과 필요한 품질/내용을 함께 확인한 새 세트를 확보하지 못했다. 목록에서 완료로 표시하지 않는다.

## 권장 진행

1. **전신:** Quaternius 기본/특수 모션 + KayKit 소총/한손 총/근접 세트를 재배포 가능한 후보군으로 유지.
2. **1인칭:** Cransh 소총/권총을 우선 다운로드·원본 구성물 크레딧 확인 대상으로 선정. 확보/권리/호환성 확인 후 채택.
3. **누락 동작:** 군인식 포복과 소생/승하차를 계속 별도 확보. 모델의 고정 포즈나 일반 기어가기 동작으로 완료 처리하지 않음.

## 이번 파일 변경과 검증

- 이 문서 신규: 원본 재배포 기준과 새 후보/보류/누락 정리.
- `Docs/AI/Battlefield6AnimationAssetCatalog.md`: 최신 기준 안내와 이 문서 링크 추가. 기존 수집 이력 보존.
- `Docs/AI/Battlefield6AnimationClips.csv`: Kevin 128행의 상태를 `excluded_raw_redistribution`로 변경. 원래 검사 상태는 notes에 보존하고 다른 104행은 유지.
- `Docs/AI/Battlefield6AnimationAcquisitionManifest.csv`는 이전 수집 시점의 해시 기록이므로 이번 편집에 맞춰 과거 해시를 덮어쓰지 않는다.
- 제품 런타임 스크립트/씬/Animator/설정/에셋 교체·삭제 없음. 직전 착석 작업 검증기 통과 및 적용 실험 전 원본 해시 확인. 이번에 새 PlayMode 테스트를 실행했다고 표시하지 않는다.
- 적용 실험의 병사 모션/가져오기 도구/전환 코드는 Library 아래 격리 프로젝트에만 남아 있다. 이번 사용 지침으로 새 후보를 선정할 때 자동 재사용하지 않는다.
