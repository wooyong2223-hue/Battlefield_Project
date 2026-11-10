# 포복 원본 애니메이션 및 좌우 회전 모션 대체 적용

2026-11-01. 사용자가 제공한 Mixamo 모션으로 SampleScene에서 포복을 테스트하기 위한 변경이다. 이동·충돌·조준 방향은 기존 코드가 소유하고 애니메이션은 표현만 담당한다. 앞서 격리 프로젝트에서 진행하던 전체 모션 전환과 구분해 **포복 부분만** 반영한다.

## 변경 전 비교와 범위
이전 Library/CodexLayeredMotions/before.json의 관련 스크립트·씬·무기 정의 해시가 일치했다. 추가된 사용자 원본 Prone Forward.fbx도 보존했다. 이번 대상 파일에 겹치는 사용자/Editor 후속 변경은 없었다. 모든 사용자 원본을 바이트 그대로 복사하며 기존 임포트 설정이 있으면 다시 초기화하지 않는다.

일반 사격/장전/이동의 기존 안정화, 슬라이드 합성 클립 및 생성 도구, Animator Controller/기존 마스크, 카메라/이동/무기 설정, SampleScene의 저장된 구조는 변경하지 않는다. 기존 코드를 삭제한 항목은 없다. 기존 절차형 포복의 실행 조건만 원본 재생 중에는 건너뛰도록 교체한다. 원본이 없는 환경에서는 기존 동작을 유지한다.

## 구조와 연결
- ProneAnimationSet: 모션 참조만 소유하는 ScriptableObject.
- PlayerCharacterProneAnimationView: 포복 재생/진입/해제/블렌드 및 PlayableGraph 수명 관리. 기존 이동 속도를 읽어 idle/앞/뒤/좌/우 원본을 선택한다. 무기·탄약·피해·이동 입력을 소유하지 않는다.
- LocalProneAnimationSetup: 로컬 원본 임포트와 Editor Play/빌드 씬 복사본의 의존성 연결. SampleScene 파일은 저장/수정하지 않는다. Play 중 캐릭터 Animator 오브젝트에 포복 재생 컴포넌트가 추가되며, 탑승자의 on-foot 목록에도 등록해 탑승 시 중단한다.
- 기존 Combat/Posture/Aim/Reload/FirstPerson 뷰는 포복 컴포넌트를 명시적으로 전달받는다. 포복 외의 기존 경로는 보존한다.

## 바뀐 스크립트와 이유
| 스크립트 | 구체적인 변경과 동작 |
|---|---|
| PlayerCharacterCombatView.cs | private _prone/SetPronePresentation/HasAuthoredProne 추가. UsesItemGrip과 UpperCombat·ReloadUpperBody·MeleeCombat 가중치에서 원본 포복 재생을 제외해 서 있는 상체가 포복 원본을 덮지 않게 한다. OnFired/OnReload의 기존 일반 무기 구현은 보존. |
| PlayerCharacterPostureView.cs | private _authoredProne 및 연결 메서드 추가. 기존 `_stance.IsProne`으로 올리던 절차형 `_blend` 조건에 `!HasAuthoredProne` 추가. UpperBodyAimMultiplier는 원본 포복 중 0. 원본에 기존 몸통 90도 회전을 다시 더하는 문제 방지. 소생 보정도 원본 포복 재생 중 중복 적용하지 않는다. 기존 필드/메서드/설정값 삭제 없음. |
| PlayerCharacterWeaponAimView.cs | private _prone/_wasProne/_proneGripCorrection 및 SetPronePresentation/FitProneItemContact 추가. 포복 원본 평가 후 손과 무기의 접촉을 기존 HumanoidArmSolver로 맞춘다. 포복 중 근접은 기존 펀치 클립의 팔 회전을 조준 기준으로 리타깃하며 포복 몸통/다리는 유지한다. 일반 상체 안정화 메서드는 삭제하지 않는다. |
| PlayerCharacterRifleReloadView.cs | _prone 참조와 SetPronePresentation 추가. 포복 중에만 해당 프레임 원본 오른손 위치/회전에서 시작해 탄창 접촉을 맞추는 분기를 추가했다. 기존 서서/앉아서/점프 장전 코드는 보존. |
| FirstPersonArmsView.cs | _prone 참조와 SetPronePresentation 추가. 포복 그립도 sourceGrip으로 인식해 손-총 방향 정렬. 포복 액션은 기존 complete-arm 메시 사용, 몸통 델타를 추가로 두 번 적용하지 않도록 해당 분기만 제한. 기존 일반 시점/ADS 값은 그대로다. |
| ProneAnimationSet.cs + .meta (신규) | idle/forward/backward/left/right, 진입/해제, 발사/장전/사망, 무장 진입/해제 및 근접 마스크/클립을 private SerializeField로 소유. GetClip과 읽기 전용 속성 제공. |
| PlayerCharacterProneAnimationView.cs + .meta (신규) | manual PlayableGraph로 원본 전신 모션을 재생. 실제 속도에 따라 방향 가중치를 선택하고 기존 탄창/전투 이벤트의 시간을 따라 액션 재생. disable/destroy 때 구독 해제/graph 파괴/culling 복원. 근접은 팔 마스크 레이어로 분리. Root Motion으로 이동하지 않는다. |
| LocalProneAnimationSetup.cs + .meta (신규) | Tools/Battlefield/Animations/Mixamo/Import Prone Animations 메뉴. null 슬롯만 채우고 기존 선택을 보존. 좌우 전용 이동이 없으면 사용자가 승인한 Left/Right Turn을 후보로 연결. Play/빌드 복사본에 연결하고 영구 씬 구조는 변경하지 않는다. |
| PlayerCharacterInfantryTests.cs / PlayerCharacterReloadPoseTests.cs | 원래 0.6초/0.3초 고정 대기 후 최종 포복 자세를 검사하던 두 곳만 `진입 클립 길이 + 0.12초` 대기로 교체했다. 자세·카메라·그립 허용 범위는 그대로 유지. |
| ProneAnimationTests.cs + .meta (신규) | 실제 전진/좌우 변위, 전진 다리 움직임, 좌우 원본 이름, 루트/조준 방향 유지, Root Motion OFF, 조준·근접·장전 취소·disable/resume·해제·장비 컨트롤러 보존을 검증. |

## 클립 및 에셋 값
기존 프로젝트에는 ProneAnimationSet 자산이 없었다. 격리 검증 프로필의 `_left`/`_right`는 `{fileID: 0}`이었다. 이번에는 새 로컬 프로필을 아래와 같이 설정한다.

| 프로필 필드 | 연결한 클립 |
|---|---|
| _idle | Prone Idle |
| _forward | Prone Forward |
| _backward | Crawl Backwards In Prone |
| _left | Prone Left Turn |
| _right | Prone Right Turn |
| _enter / _exit | Crouch To Prone / Prone To Crouch Transition |
| _armedEnter / _armedExit | Rifle Kneel To Prone / Rifle Prone To Kneel |
| _fire / _reload / _death | Prone Firing Rifle / Prone Reloading / Prone Death |
| _meleeMask | 신규 ProneMeleeArms.mask: 팔·손가락만 포함, root/body/head/다리 제외 |
| _meleeWeapon / _meleeJab / _meleeCross | 기존 Quaternius PunchCross / PunchJab / PunchCross |
| _retargetStandingMelee | true: 기존 서 있는 근접 원본의 팔 체인을 조준 프레임에 맞춤 |

포복 데이터는 Assets/ThirdParty/MixamoLocal/Prone/ProneAnimations.asset에 있다. 이 폴더는 기존 MixamoLocal 정책대로 로컬 에셋이다. 사용자가 제공한 Running Slide를 제외한 18개 FBX를 이름/바이트 그대로 임포트하며 각 .meta를 포함한다. 사용하지 않는 회전/전이/피격/수류탄 원본도 자료로 보관하지만 새 수류탄·피격 게임플레이를 추가한 것은 아니다. 새 FBX 임포트는 Humanoid/CreateFromThisModel, 압축 Off, 카메라/라이트 제외, 전체 take, 루트 회전/높이 Bake, 수평 이동은 기존 CharacterController에 맡긴다. 커브를 새로 제작하거나 베이크 합성하지 않는다.

## 대체 모션의 한계와 테스트 방법
SampleScene에서 Play를 다시 시작한 뒤 Z로 포복, W/S로 전후, A/D로 좌우 이동한다. V로 두 시점을 확인하고, 정지·방향 전환·장전·Z로 해제도 확인한다. 방향 전환 모션을 좌우 평행 이동의 표현으로 재사용하므로 전용 옆걸음 포복과 동일한 동작은 아니다. 캐릭터 루트와 조준은 회전시키지 않고, 모션 내부의 자세 회전만 보여준다.

포복 발사/장전은 소총 원본을 공통 사용하며 권총 전용 자세는 미확보다. 발사/장전은 현재 전신 원본으로 재생되어 이동 중 다리까지 계속 기어가는 전용 혼합 품질은 별도 확인이 필요하다. 1인칭 전용 팔 애니메이션을 새로 확보한 작업은 아니다.

## 설계 검토
데이터 선택·재생 상태·임포트 책임을 별개 객체로 분리하고 게임플레이 권한은 기존 컴포넌트가 유지한다. private 상태/명시적 의존성/읽기 전용 프로필을 사용하고 불필요한 인터페이스나 상속은 추가하지 않았다. 기존 컨트롤러와 직접 참조하는 부분은 동일 캐릭터 내부 표현 조합이므로 새 추상 계층을 두지 않은 의도적 선택이다. IK는 원본을 대체하는 모션 제작이 아닌 손 접촉과 조준 리타깃에 한정한다.

## 검증 결과
- Unity 6000.4.8f1 격리 프로젝트에서 -batchmode -noaudio로 컴파일/Play Mode 검증.
- 전체 188개: 186 통과, 2 실패. 두 실패는 원본 진입 중에 최종 포복 자세를 검사한 기존 고정 대기 시간이었다. 원본 길이에 맞춰 위 두 테스트를 갱신한 뒤 관련 24개를 재실행해 24/24 통과, 실패/스킵 0.
- 실제 좌우 이동 변위, 루트·조준 방향 유지, Root Motion OFF, 전진 발 움직임, 근접 시 몸통 높이, 장전 취소, enable/disable, 포복 해제 및 무기 Controller 보존 확인.
- prone-left_40.png / prone-right_40.png의 실제 3인칭 렌더링을 검토했다. 전용 좌우 평행 이동 원본과 동일한 품질을 보장하는 검사는 아니다.
- 사용자 원본 18개와 임포트된 FBX의 SHA256 일치. 새 GUID 충돌 없음. 본 프로젝트로 복사한 파일의 해시는 검증 소스와 일치한다.
- 이동/카메라/시점/슬라이드/좌석 스크립트, Animator Controller, RunningSlide 프로필, SampleScene은 이번 적용 전후 동일.
- 남은 확인: 사용자의 본 Editor SampleScene 조작 감각·경사면·극단 조준·연속 자세 전환과 실제 대상 플랫폼 빌드. 전체 모션 전환 작업을 모두 반영하거나 전용 권총 포복 모션을 확보한 것은 아니다.

파일별 전체 경로·변경 사유·SHA256은 ProneAnimationFiles.csv에 기록한다. 임시 캡처/격리 마이그레이션 도구는 본 프로젝트로 복사하지 않는다.
