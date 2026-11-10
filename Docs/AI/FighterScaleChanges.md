# 전투기·캐릭터 크기 비율 조정 및 구조 점검

2026-10-27. 사용자가 승인한 전투기 균등 배율 2.270328, 좌석 위치/크기 및 카메라 거리 보정을 적용했다. 이는 캐릭터 모델과 전투기 모델의 원래 크기 비율을 맞춘 값이며 특정 실제 항공기 제원에 맞춘 값은 아니다. 커밋은 만들지 않았다.

## 작업 전 비교

SampleScene, Fighter.prefab, VehicleSeat, FighterEngine, PlayerVehicleOccupant, SeatedCharacterPose를 직전 검증 복사본과 비교해 모두 일치함을 확인했다. 최근 사용자가 저장한 전투기/더미/활주로 위치, 캐릭터 높이 4와 모델 배율 2.270328을 기준으로 작업했다. 기존 위치/회전/계층/비행·무기 값과 충돌하는 추가 변경은 없었다. 수정 직전 에셋은 Temp/FighterScaleBefore에 보관했다.

## 변경 파일 전체와 정확한 값

| 파일 | 이전 → 이후 | 변경 이유 |
|---|---|---|
| Assets/Features/Fighter/Prefabs/Fighter.prefab | Fighter 루트 Transform(fileID 2872367962350400426)의 localScale (1,1,1) → (2.270328,2.270328,2.270328) | 높이 4인 캐릭터를 줄이지 않고 두 에셋의 원래 비율에 맞춰 전투기를 확대한다. |
| Assets/Features/Fighter/Prefabs/Fighter.prefab | PilotSeatPoint Transform(fileID 1719566512375129943)의 localPosition (0,0.75,6.75) → (0,1.262062,7.488929) | 실제 캐릭터의 엉덩이를 좌석에 맞추면서 머리가 기존 1인칭 카메라 근처에 오도록 조정한다. |
| Assets/Features/Fighter/Prefabs/Fighter.prefab | 같은 PilotSeatPoint의 localScale (1,1,1) → (0.440465,0.440465,0.440465) | 전투기 확대가 자식 좌석 가구까지 크게 만들지 않도록 역보정한다. 좌석 가구의 월드 크기는 기존 캐릭터 크기로 유지한다. |
| Assets/Scenes/SampleScene.unity | Main Camera/JetCamera(fileID 330585548)의 `_orbit._maximumThirdPersonDistance` 40 → 90.81312 | 확대된 ThirdPersonCameraPoint의 거리 약 56.45224가 예전 40 제한에 잘리지 않게 해 기존 상대 구도를 유지한다. |
| Assets/Tests/PlayMode/FighterScaleTests.cs 및 .meta | 신규 PlayMode 테스트 2개 | 기체 크기·실제 착석·캐릭터/좌석 크기·눈과 카메라 정렬·3인칭 구도·하차, 빈 기체 지상 접촉과 기존 물리 설정을 검사한다. |
| Docs/AI/FighterScaleChanges.md | 신규 보고서 | 파일별 데이터 변경·구조 점검·검증 근거와 남은 확인을 기록한다. |
| Docs/AI/UnityProjectContext.md | 승인된 배율·좌석·카메라 보정과 최신 검증 결과 추가 | 다음 작업에서 확대 전 수치로 되돌리지 않도록 현재 상태를 기록한다. |

신규 테스트 .meta GUID는 `1fdfdecc85df49a4919a52ed58b0668c`이며 32자리 형식과 중복 여부를 검사했다. 첫 검증 실행에서는 GUID 오타 때문에 새 테스트가 제외됐고, 이를 수정한 최종 실행에서는 46개가 모두 발견되어 실행됐다.

기체 메시 기준 크기는 폭/높이/길이 약 14.08/4.74/20.76 → 31.96/10.77/47.13이다. 캐릭터 높이 4, 캡슐 반지름 0.5, 모델 배율 2.270328은 그대로다. 좌석 월드 배율은 반올림 오차 범위에서 1이다. 카메라 포인트 로컬 값과 FOV/추적 속도, 최소 거리와 휠 거리 변경 OFF 설정도 유지했다.

기체 확대에 따라 기존 기체 충돌 형상, 카메라/발사/이펙트/진입·하차 앵커의 월드 위치는 함께 확대된다. 탑승 여유 거리 3m, 질량 1000, 비행·무기 수치, 음원/프로필 값, 씬의 위치와 회전은 바꾸지 않았다. 기존 메서드·직렬화 필드·컴포넌트·참조·에셋을 삭제하지 않았고 새 부모 오브젝트도 만들지 않았다.

## 구조·객체 지향 규칙 점검

점검 범위는 최근 탑승·하차·시동·탑승 범위·착석 자세·입력·카메라 연결이다. 전체 프로젝트의 모든 기능/플랫폼/성능을 감사했다는 의미는 아니다.

확인된 구조:

- VehicleSeat: 좌석 점유, 접근 거리, 조종 권한, 안전한 하차 공간 검사. IVehicleSeatOccupant와 IVehicleSeatInput 계약으로 연결하며 캐릭터 구체 타입을 검사하지 않는다.
- PlayerVehicleOccupant: 같은 캐릭터의 보행/차량 상태와 카메라/HUD 전환 및 복원. 캐릭터는 원래 부모를 유지하므로 차량 삭제가 캐릭터 삭제로 이어지지 않는다.
- PlayerVehicleInteraction: 입력을 탑승/하차 요청으로 바꾼다.
- FighterEngine: 좌석 점유/생존에 따른 시동 정책. JetMovement는 추진 상태와 물리를 소유한다.
- SeatedCharacterPose: Humanoid 뼈/Animator 자세 적용과 복원. 모델 크기와 캐릭터 이동 정책을 변경하지 않는다.
- Audio/VFX/UI: 상태를 읽어 표현한다. 직접 탑승·피해·이동 규칙을 변경하지 않는다.

기존 책임에 맞는 데이터 조정이므로 새 클래스/매니저/상속 계층을 추가하지 않았다. 필요한 변화 지점에는 작은 인터페이스를 사용하고, 현재 단일 구현인 Unity 컴포넌트는 명시적 직렬화 참조를 사용한 합성으로 연결한다. 모든 구체 참조를 인터페이스로 바꾸지 않은 것은 불필요한 계층을 줄이기 위한 의도한 선택이다. 공개 상태는 읽기 속성과 의도가 드러나는 메서드로 제한돼 있다.

Unity 직렬화 검사에서 필수 참조/배열 누락, 점유자 자신의 수명 제어 컴포넌트를 끄는 잘못된 보행 목록, 서로 다른 좌석을 가리키는 상호작용/시동 연결, Missing MonoBehaviour가 발견되지 않았다. 물리 Camera와 AudioListener는 각 1개다. Fighter는 여전히 씬 루트이며 투사체 소유자의 transform.root 의미가 유지된다. 점검 대상에서 금지된 SerializeField+Range 사용과 매 프레임 씬 전체 검색도 발견되지 않았다. 이 범위에서 추가 규칙 위반은 확인되지 않았다.

## 검증 결과와 남은 확인

- Unity 6000.4.8f1 분리 검증 프로젝트에서 기존 44개와 신규 2개, **PlayMode 46/46 통과**. 결과 Temp/codex-fighter-scale-final.xml, 로그 Temp/codex-fighter-scale-final.log.
- 신규 테스트 `EnlargedFighterKeepsHumanSizedPilotSeatAndAuthoredCameraComposition`: 실제 hull 크기, 동일 캐릭터/모델/캡슐 크기 보존, 좌석 월드 배율, 실제 엉덩이 좌표, 머리와 1인칭 카메라 정렬, 3인칭 거리의 40m 잘림 방지, 하차를 검사했다.
- 신규 테스트 `EmptyEnlargedFighterSettlesOnRunwayWithExistingPhysicsTuning`: 자유 Rigidbody로 1초간 물리를 진행해 지면 접촉, 질량 1000/비키네마틱, 빈 좌석 시동/조종 정지, 생존과 루트 소유권을 검사했다.
- 적용 후 Unity 직렬화 구조 검사 재통과. Temp/codex-scaled-fighter-review.log에 `BOARDING_STRUCTURE_AUDIT_PASSED` 기록.
- 실제 적용본의 정지 렌더에서 착석 자세 확인. Temp/codex-scaled-seat-final-opaque.png는 외부 화면, Temp/codex-scaled-seat-final-cutaway.png는 기체 외형을 숨겨 내부 자세를 보인 진단 화면이다. 기존 캐노피를 투명하게 바꾸지는 않았다.
- 수정 전후 비교에서 프리팹은 위 2개 Transform 블록, 씬은 위 JetCamera 블록만 달라졌고, 모든 기존 로컬 ID 집합과 나머지 블록은 보존됐다. 원본/검증본 에셋과 테스트 일치, 로컬 참조 무결성, 테스트 GUID 유효성도 통과했다.
- 사용자 Unity Editor에서 SampleScene을 다시 열고 실제 확대 비율·지상 접근/F 안내·조종석 1인칭 가림·3인칭 구도·비행 중 충돌/하차·엔진음 거리 체감을 확인해야 한다. 플레이어 빌드 및 성능 프로파일링은 이번 범위에서 실행하지 않았다. 자동 테스트와 정지 렌더가 체감/청취 검증을 대체하지 않는다.

새로운 캐릭터 기능은 아직 구현하지 않았다. 현재 이동·달리기·점프·1/3인칭·조준·탑승 위에 앉기/앉아서 이동, 방향별 이동 애니메이션, 손에 든 무기 모델·장전, 발소리/착지음, 피격/사망 표현 순서로 확장하는 안을 제안했다.
