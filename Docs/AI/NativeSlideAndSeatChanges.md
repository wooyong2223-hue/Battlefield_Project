# 슬라이드·탑승 원본 애니메이션 레이어 전환

전체 모션 구조 전환 중 슬라이드와 탑승 팔다리만 적용한다. 상체 제한 제거·장전 단일 레이어 전환 후보는 회귀가 발생하여 반영하지 않는다.

## 적용 전 비교와 보존

이전 4개 작업의 applied.json을 파일별 최신 순으로 합쳐 현재 SHA-256과 비교했고 차이가 없었다. 같은 격리 프로젝트도 적용 전 관련 Assets가 본 프로젝트와 같았다. 적용 직전 다시 검사한다. SampleScene, 기존 하체만 슬라이드 선택(_lowerBodyOnly=1), 포복 속도/전환/카메라, 권총 ADS, 무기 그립·상체 보정·장전은 현재 구현을 보존한다.

## 구조와 동작

SlideAnimationSet은 원본 참조와 선택, SlideAnimationView는 네이티브 레이어 시간·가중치, AnimationView는 이동 결과의 파라미터 전달을 맡는다. 이동/충돌은 기존 CharacterController가 담당한다. 추가 manager나 상속/인터페이스 계층은 만들지 않았다.

이전에는 ForwardSlideClipComposer.Compose가 Quaternius Slide_Start와 Mixamo RunningSlide를 구워 하나의 RunningSlideForwardEntry.anim을 만들었다. 이제 이 자산을 만들지 않는다. 기존 하체 마스크를 공유하는 두 Animator 레이어가 원본을 직접 평가한다.

```csharp
// 이전 프로필: 세 슬롯 모두 생성 클립
_start = _loop = _exit = RunningSlideForwardEntry;
// 이후 프로필: 외부 원본만 참조
_start = Quaternius_Slide_Start;
_loop = _exit = Mixamo_RunningSlide;
_lowerBodyOnly = true;
```

진행률 p에서 진입 시간은 Clamp01(p/.5), 원본 시간은 p. 0~50% 진입, 50~80% SmoothStep 혼합, 마지막 20% 원본 종료를 유지한다. 전체 fade=f, 원본 혼합=b일 때 최종 기여는 base=1-f, 진입=f*(1-b), Mixamo=f*b다. 위 레이어가 아래 레이어를 덮는 Animator 규칙을 보정해 이 비율이 성립하도록 한다. 매 프레임 본 위치/회전을 새로 만들어 슬라이딩하지 않는다. 종료/진입 중 취소 때 SlideTime을 0으로 돌리지 않고 현재 포즈를 fade하고, 두 레이어 모두 비활성화 시 초기화한다.

```csharp
// 이전: 원본 샘플링 뒤에도 팔다리 방향을 강제로 다시 설정
PoseLeg(...); PoseArm(...);
// 이후: 원본이 없을 때만 기존 대체 자세 사용
if (!IsUsingAnimation) { PoseLeg(...); PoseArm(...); }
```

탑승 원본이 있을 때 그 모션의 팔·다리를 보존한다. 골반-좌석 정렬, 등받이 각도, 머리 방향 맞춤은 장비 좌석 접촉 보정으로 유지한다. 기존 필드/방향값과 탑승 해제 복원은 삭제하지 않는다.

## 파일별 정확한 변경

전체 파일·삭제/대체 메서드·serialized 값은 함께 제공하는 NativeSlideAndSeatFiles.csv에 모두 기록한다. 새 원본 다운로드나 원본 FBX/Quaternius 곡선 수정은 없다. 생성 도구 2개 파일(.cs/.meta)과 생성 클립 2개 파일(.anim/.meta)만 Assets 밖의 Library/CodexNativeMotionMigration/Retired로 옮겨 보관한다.

## 검증과 남은 일

Unity 6000.4.8f1 격리 프로젝트의 SampleScene에서 -batchmode -noaudio 제품 Play Mode 전체 191/191 통과, 실패 0, 건너뜀 0, 201.76초. 임시 캡처/변환 도구는 Assets에서 제거한 상태로 실행했다. 원본 리타깃 자세, 두 하체 레이어 혼합 가중치, 진입 중 취소/fade/disable, 지면 접촉, 장비 교체/루트 이동 중복, 탑승 팔다리/일시정지/하차 복원, 기존 발사·상하 조준·장전·포복 ADS 회귀를 포함한다. 본 프로젝트의 관련 파일은 검증된 후보와 바이트 단위로 일치한다. 로그: Library/CodexNativeMotionMigration/full.xml 및 full.log.

복귀 혼합 구간(51~80%)의 최대 관절 회전은 인접 1% 샘플당 11.93262도였다. 전체 구간 최대 33.72752도는 진입 13%의 Quaternius 원본 오른쪽 정강이 동작이며 해당 구간의 원본 자세 일치를 별도로 검증했다. 최대값을 전체 동작의 매끄러움 보장이나 게임 영상 측정값으로 해석하지 않는다. 권총 포복 ADS와 AK-47 슬라이드 캡처를 눈으로 확인했다.

상체의 StabilizeArmedTorso/StabilizeLocomotionTorso 등 ±3도 보정을 제거한 넓은 후보는 발사 팔꿈치·동일 프레임 조준·머리 방향·장전 어깨 검사에서 실패했다. 원본 몸통을 전부 살리면 달릴 때 과도한 기울어짐도 보였다. 따라서 해당 후보의 상체 마스크, 추가 루트 레이어, CombatView 장전 변경과 테스트 완화는 본 프로젝트에 적용하지 않았다. 기존 상체 보정과 장전 기능은 그대로다. 적합한 무장 이동/장전 원본을 확보한 뒤 이 전환을 이어가야 한다. 전체 모션 전환이 끝났다고 주장하지 않는다.

Unity Editor에서 SampleScene의 달리기+C, 슬라이드 끝나기/중단, 기체 탑승·하차를 직접 조작해 시각적 선호와 조종석 여유 공간을 확인해야 한다. 대상 플랫폼 실행 빌드와 성능 비교는 수행하지 않았다.

## 설계 검토

단일 책임: 데이터/재생/이동/좌석 맞춤의 기존 경계를 유지했다. 캡슐화: fade 상태와 가중치 계산은 뷰 내부, 프로필은 읽기 속성으로 노출한다. 확장: 기존 Override 합성으로 장비별 클립을 보존하고 외부 원본을 교체한다. 대체 가능성/인터페이스 분리: 새 상속이나 광범위 인터페이스를 추가하지 않아 기존 계약을 변경하지 않는다. 의존성: 기존 명시적 Animator/Traversal/프로필 주입 경계를 사용하며 새 전역 조회를 추가하지 않았다. 의도적 예외는 원본이 없을 때의 수동 좌석 대체 자세, 지면/좌석 접촉 IK, 현재 상체 안정화 보정의 유지다. 선택 자산이 빠진 저장소와 이미 동작하던 조준의 퇴행을 막기 위해 필요하다.
