# 총기 이동 애니메이션 외부 에셋 우선 정책

확인일: 2026-10-29.

## 최신 사용자 방향

캐릭터 애니메이션은 직접 제작하거나 런타임 관절 보정을 늘리기 전에 적절한 무료 외부 클립을 먼저 찾는다. 권총·소총을 든 상태로 제작된 전신 이동 모션을 우선한다. 기존 Quaternius 우선 원칙은 적합한 동작이 실제로 포함된 경우 적용한다. 없는 무기 이동 동작을 일반 이동과 임의 각도로 대체해 완료 처리하지 않는다. 공개 Git에 원본을 포함할 수 있다는 기존 요구도 유지한다.

## 현재 조준 증상과 코드 증거

- `PlayerCharacterCombatView`는 3인칭에서 조준·사격·엎드림 등이 아니면 Carry/Sprint 모드를 선택한다.
- `PlayerCharacterWeaponPoseState.Rotation`은 `Mathf.Lerp(pitch, Mathf.Lerp(35f, 55f, SprintBlend), CarryBlend)`를 반환한다. CarryBlend가 1에 도달하면 손 IK의 pitch는 마우스 입력과 무관하게 휴대 35도 또는 질주 55도가 된다. 이는 직전 작업에 도입한 휴대 자세의 동작이며 팔이 상하 시선을 따라가지 않는 코드 경로다.
- 상체에는 별도로 CombatPitch와 LookAt이 남아 있다. 따라서 위 경로만으로 상체 전체가 따라가지 않는 현상까지 확정하지 않는다. 이번에는 실제 PlayMode 재현을 수행하지 않았다.
- 제품 코드/클립/Animator/씬 교체나 수정을 하지 않았다. 동작하는 새 애니메이션을 적용했다고 표시하지 않는다.

## 후보와 확인 수준

| 후보 | 실제 확인한 내용 | 판정/남은 확인 |
|---|---|---|
| [KayKit Character Animations](https://kaylousberg.itch.io/kaykit-character-animations) | 제작자 CC0, 무료 FBX/glTF. 기존 로컬 조사에서 `Rig_Medium_MovementAdvanced`의 `Running_HoldingRifle`, CombatRanged의 `Ranged_2H_Aiming/Shoot/Shooting/Reload` 및 1H 대응 클립명 확인 | 소총 달리기 우선 시험 후보. 사실적인 PUBG 군인 모션으로 품질 확정하지 않음. 권총 전용 방향별 이동 세트가 있다는 뜻도 아님. 원본 모델에 리타깃한 뒤 검토 필요 |
| [Quaternius Toon Shooter Game Kit](https://quaternius.com/packs/toonshootergamekit.html) | 제작자 CC0, 캐릭터 17 애니메이션 및 FBX/glTF 설명 | 추가 조사 후보. 이번에는 아카이브를 받거나 무기별 걷기/달리기 클립명을 확인하지 못했으므로 포함 여부/품질 미확정 |
| [ZenXChaos ThirdPersonShooter-AnimationSets](https://github.com/ZenXChaos/ThirdPersonShooter-AnimationSets) | README에 Revolver Walk/CrouchWalk, 상하·좌우 조준 9자세, Shotgun Walk, Unlicense 표시. 제작자가 Mecanim 추가 설정 필요 명시 | 권총 계열 탐색 후보. 원 제작 출처/개별 파일 권리와 Humanoid 품질을 추가 확인하기 전 제품 반입 보류. 리볼버를 양손 권총, 산탄총을 소총 전용 모션으로 단정하지 않음 |
| [Imaginative Ones Motion & Combat](https://imaginativeones.com/free-animations-assets-for-games.html) | 제작자 Dennis Jensen, 무료 CC-BY-SA 표기, 걷기/달리기/질주/앉기/전투/대기 25클립, FBX/Maya | 공개 재배포 후보지만 저작자표시/동일조건 확인 필요. 무기를 든 이동인지 세부 목록과 미리보기 미확인이므로 선정 보류 |

[Mixamo](https://www.mixamo.com/)는 무료 외부 애니메이션 소스지만 [Adobe 배포 안내](https://community.adobe.com/questions-696/mixamo-faq-licensing-royalties-ownership-eula-and-tos-589400?lang=en)는 원본 캐릭터/애니메이션 배포를 제한한다. 현재 공개 원본 저장소 기준의 채택 후보와 구분한다. 이번에는 다운로드하거나 반입하지 않았다.

## 적용 전 확인 순서

1. 무기별 전신 idle / 전후좌우 walk·jog / sprint / crouch 이동과 상하 조준 클립을 목록으로 확인한다. 미포함은 누락으로 기록한다.
2. 파일별 출처와 원본 재배포 조건을 확인한다.
3. 격리 프로젝트에서 현재 Armature에 리타깃하고 정면·측면·후면 영상으로 총/손 접점, 어깨, 보폭, 발 접지를 확인한다.
4. 적합한 클립 선정 후 기존 Carry/Sprint 고정 pitch, 상체 보정, 레이어 중 무엇을 대체할지 이전 코드와 함께 먼저 설명한다. 현재 단계에서 자동 삭제하지 않는다.
5. 상하 조준은 이동 클립만 교체해 해결됐다고 보장하지 않는다. 조준 클립/블렌딩과 남는 IK 책임을 함께 검증한다.

## 이번 변경과 검증

이 문서만 신규 추가했다. 런타임 코드, 에셋, 씬, 컴포넌트, 직렬화 값의 변경/삭제/교체는 없다. 조준 스크립트 3개와 Animator Controller가 직전 격리 검증본과 해시가 같음을 확인했다. SampleScene은 격리 복사본과 달라 보존했으며, 차이 원인/작성자를 추정하지 않았다. 웹의 제작자 페이지/후보 저장소와 기존 로컬 수집 기록을 조사했다. 새 PlayMode 테스트나 원본 Editor 확인은 하지 않았다. 새 클래스/상속/인터페이스를 도입하지 않아 OOP/SOLID 구조 변경도 없다.
