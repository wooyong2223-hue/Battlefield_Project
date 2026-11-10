# 권총 포복 ADS 화면 배치 수정 — 2026-11-01

## 재현·원인
이전 ProneAimCamera 적용 기록과 관련 파일을 비교해 사용자/Editor 후속 변경이 없음을 확인했다. 수정 전 주요 6개 파일 해시는 Library/CodexPronePistolAds/before.json에 보관했다.

서기·앉기·포복을 1인칭 일반/ADS로 비교했다. 기존 포복 ADS에서 총구의 정규화 화면 좌표가 (.416,-.036)으로 화면 아래였다. 모델 조준기 부근의 진단 표본점도 (.354,-.380)으로 화면 밖이었다. 실제 렌더에서 총이 보이지 않는 현상을 재현했다. HUD의 PlayerCombatHud는 상태·탄약 텍스트를 표시하며 별도 보병 크로스헤어를 그리지 않는다. 이번 수정은 총 자체 조준부의 화면 배치 수정이다.

권총은 포복 때 방향을 손 그립 기준으로 맞추지만, 위치는 서 있는 자세에서 보정한 _poseAnchor/_offset 조합을 계속 사용했다. 총 방향이 맞아도 총 전체가 아래로 밀릴 수 있었다. 이전 사격 그립 유지 수정은 방향/메시 튐을 고쳤지만 이 위치 문제는 별도였다.

## 수정 방식과 전후
기존 복사 포즈를 만든 뒤, 포복·기본 권총 그립 상태에만 복사된 양팔 전체를 같은 벡터로 이동한다. 실제 모델의 Muzzle과 쉬고 있는 손잡이 위치/회전을 기준으로 보정하므로 임의로 한쪽 손만 움직이지 않는다. 반동이 반영되지 않은 restingMuzzle을 기준으로 삼아 기존 무기의 반동 이동·회전은 계속 보인다.

```csharp
// 기존: 포복에서도 일반 자세 위치 변환 결과를 그대로 사용
position = pivot + rotation * ((sourceBone.position - anchor) * viewScale);
// 추가: 해당 상태에서만 기존 복사 포즈 전체에 같은 위치 보정
correction = targetMuzzleInView - restingMuzzle;
viewBone.position += correction;
```

ADS 목표는 눈에서 조준 방향으로1.4m. 이 깊이는 정상 서기 권총에서 측정한 기존 총구 깊이 약1.4004m를 따른다. 일반 1인칭 포복의 좌우/상하 차이는 새 숫자를 만들지 않고 기존 `_offset - _adsOffset` 차이를 사용한다(현재 x+.15/y-.25/z0). ADS blend가1이면 그 차이를0으로 하여 중앙에 놓는다.

일반 자세·앉기·소총·맨손은 새 분기를 사용하지 않는다. 장전/근접/전환처럼 그립이 해제되는 동안에는 원본 액션을 허용하고 보정으로 고정하지 않는다. 실제 캐릭터 루트/원본 Animator 뼈/카메라/원본 클립에는 위치 보정을 하지 않는다. 1인칭 표시용 복사 리그만 변경한다.

| 측정 | 수정 전 | 수정 후 |
|---|---|---|
| 포복 ADS 총구 화면 좌표 | (.416,-.036) | (.500,.500) |
| 포복 ADS 조준기 부근 표본점 | (.354,-.380) | (.500,.549) |
| 서기·앉기 ADS 총구 좌표 | (.500,.500) | 동일 |
| 서기·앉기·포복 ADS 총구 깊이(후) | 서기/앉기 약1.4004m | 세 자세 약1.4004m |

조준기 표본점은 모델 로컬(0,.31,-.05)을 사용한 진단용 위치이며 새 조준 기준 오브젝트/실제 광학 영점 데이터를 만든 것은 아니다. 기존 서기와 같은 구도를 복원했으며, 총구가 중앙에 있다는 수치만으로 가늠쇠/가늠자의 정밀 영점까지 완성됐다고 주장하지 않는다. 실제 전후 렌더에서도 조준부가 보이는지 확인했다.

## 바꾼 모든 파일

| 파일 | 구체적인 변경 |
|---|---|
| [FirstPersonArmsView.cs](E:/GitHub/Battlefield_Project/Assets/Features/PlayerCharacter/Scripts/FirstPersonArmsView.cs) | private SerializeField `_pronePistolAimDistance=1.4` 추가(Min .05, Range 없음). CopyPose 끝에서 nativeProne && sourceGrip && !custom && melee<=.001 조건으로 FitPronePistolAim 호출. 새 private helper가 모델의 Muzzle 위치·모델 배율·쉬는 GripPosition/GripRotation으로 현재 프레임 기준점을 계산하고 복사 뼈에 공통 평행 이동을 적용. 기존 포즈·메시·오프셋 계산은 보존. |
| [PlayerCharacterCombatView.cs](E:/GitHub/Battlefield_Project/Assets/Features/PlayerCharacter/Scripts/PlayerCharacterCombatView.cs) | 기존 `private Vector3 GripPosition`을 `public Vector3 GripPosition` 읽기 전용 getter로 공개. 본문·직렬화 필드·기존 GripRotation은 그대로. 표시 리그가 현재 모델의 손잡이 위치를 읽기 위해 필요하다. setter/외부 상태 변경 권한은 추가하지 않음. |
| [SampleScene.unity](E:/GitHub/Battlefield_Project/Assets/Scenes/SampleScene.unity) | 기존 FirstPersonArmsView(fileID1653572103)에 `_pronePistolAimDistance: 1.4` 한 줄 추가. 기존 `_offset=(-.18985137,-.9225006,-.16549075)`, `_adsOffset=(-.33985138,-.6725006,-.16549075)`, _poseEuler, 카메라 설정/계층/위치/무기 모델 참조는 보존. |
| [PronePistolAdsTests.cs](E:/GitHub/Battlefield_Project/Assets/Tests/PlayMode/PronePistolAdsTests.cs) | 신규 재발 방지 검사. 서기 ADS 기준과 포복 ADS 화면 좌표 비교, 3인칭 포복→ADS 직행, 상하/좌우 조준, 실제 연사, 반동 후 복원, 장전 중 그립 해제/완료 후 복원, V로 선택한1인칭의 일반/ADS, 포복 해제 후 정상 조준을 검증. |
| [PronePistolAdsTests.cs.meta](E:/GitHub/Battlefield_Project/Assets/Tests/PlayMode/PronePistolAdsTests.cs.meta) | Unity 생성 신규 테스트 GUID. |
| [PronePistolAdsChanges.md](E:/GitHub/Battlefield_Project/Docs/AI/PronePistolAdsChanges.md) | 이 변경의 모든 파일·값·전후·검증 기록(신규). |
| [UnityProjectContext.md](E:/GitHub/Battlefield_Project/Docs/AI/UnityProjectContext.md) | 최신 원인/수정 범위/검증 근거 추가. |

## 제거·교체와 설계 검토
기존 메서드/필드/컴포넌트/참조/에셋은 제거하지 않았다. GripPosition의 접근 범위만 private→public으로 바꾸고 읽기 전용 구현을 유지했다. 기존 전체 포즈 계산은 그대로 두고 문제 상태의 최종 화면 배치를 추가했다. 새 클래스/관리자/런타임 컴포넌트/무기 기준점은 만들지 않았다. 표시용 포즈의 배치는 기존 FirstPersonArmsView 책임에 속하므로 작은 보정은 그 안에 유지한다. 모델/그립 데이터는 기존 소유자에서 읽고 외부로 직접 변경하지 않는다. OOP/SOLID 의도적 예외 없음.

프레임당 추가 처리는 해당 표시 상태에서 복사 뼈를 한 차례 순회하는 평행 이동이며 새 컬렉션/오브젝트를 할당하지 않는다. 성능 프로파일링을 수행한 것으로 주장하지 않는다. 애니메이션 키프레임을 직접 제작/수정하거나 원본 모션을 대체한 작업은 아니다.

## 검증
- Unity6000.4.8f1, 모든 세션 `-batchmode -noaudio`. 제품 오디오 값/설정 변경 없음.
- 변경 전 posture/ADS 실제 캡처1개 검사, 수정 후 같은 캡처+기존 포복 사격 안정화2개 통과.
- 신규 검사를 포함한 전체 제품 PlayMode189/189 통과, 실패/스킵0, 194.09초. 임시 캡처 코드는 이 전체 검사 전 격리 Assets에서 보관 폴더로 옮겼고 제품에는 넣지 않았다.
- 서기/앉기 화면 좌표가 전후 동일함을 비교했고, 포복 일반/ADS의 실제 렌더를 검토했다. 정상 반동과 장전 해제 동작을 유지하는지도 검사했다.
- 명중·피해·탄약·발사 속도·팀 규칙은 수정하지 않았고 기존 관련 회귀 검사도 통과했다. 무기 모델/클립/카메라·포복 속도 값은 유지한다.
- 본 Editor에서 실제 조작감과 원하는 조준 구도를 확인해야 한다. 원본 권총의 가늠쇠/가늠자 정밀 영점·전용1인칭 모션 제작·별도 UI 크로스헤어 추가 또는 대상 플랫폼 빌드를 완료한 작업은 아니다.

증거: Library/CodexPronePistolAds/before-sight.json, after-sight.json, before_Prone_True.png, after_Prone_True.png, full.xml, scope.patch, applied.json. SampleScene에서 2번 권총→Z 포복→우클릭 짧게 ADS, V로 선택한1인칭에서도 확인한다. 조준 깊이는 기존 1인칭 복사 리그 FirstPersonArmsView의 Prone Pistol Aim Distance에서 조절한다.
