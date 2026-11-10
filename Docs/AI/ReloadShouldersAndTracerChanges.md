# 장전 어깨 흔들림 수정 및 전투기 예광 길이

2026-10-29. 장전 흔들림을 우선 조사하고, 직전 요청의 전투기 예광 연장도 함께 처리했다. 단일 SampleScene 유지, 모든 Unity실행은 -noaudio.

## 작업 전 상태

관련 Presenter/AimView/Controller/UpperCombat.mask/Scene/탄환/기관총/Fighter는 직전 검증본과 같았다. baseline.json에 해시 기록. 사용자 설정·Scene·클립·일반 조준 마스크는 변경하지 않는다.

## 재현과 원인

기존 UpperCombat은 팔/손가락만 활성, Body/Head0. PistolReload는 이 레이어에서 재생했고, WeaponAimView는 장전 중 몸통 안정화/조준IK를 중단했다. 결과적으로 장전 팔 아래에 기본 이동 클립의 과도한 몸통 회전이 섞였다.

원본 상태의 실제 magazine.TryReload 재현: 이동 중 어깨 yaw 범위107.4959°/roll39.9430°. 기존 검사에서 yaw<45° 실패(before.xml/log). 메모리에서만 UpperCombat 마스크Body/Head를 켠 대조실험은105.09/39.19→28.55/15.95로 감소, 정지장전의native몸통회전은30.8° 정도였다(probe.csv/log). 즉 원본 장전 키프레임이 아니라 상체 마스크 조합이 큰 차이를 만든다. 전역mask를 바꾸면 일반조준/착석 등에 영향을 주므로 실제 수정은 장전 전용 레이어로 한정했다.

## 코드/컨트롤러 수정

- PlayerCharacterCombatView.cs: 기존 `_layer,_meleeLayer` 유지, `_reloadLayer=-1,_reloadBlend` 추가. Awake에서 ReloadUpperBody index 캐시. 이전 `OnReload()=>Play(PistolReload,duration)`는 기존 Play 호출을 유지한 블록으로 확장하여 전용레이어 weight1/동일 PistolReload normalizedTime0으로 시작. LateUpdate는 실제Magazine.IsReloading+표현가능+권총 상태일때만 overlay1, 완료/취소는12.5/s로0복귀(최대.08초). 표현불가/비무장, ResetPose/OnDisable은0즉시복구. 레이어없는예전컨트롤러는 index-1로기존동작 유지. 기존 게임플레이 장전시간·탄약·행동정책 변경없음.
- PlayerCharacter.controller: 기존 serialized controller문서에 ReloadUpperBody override layer1개 추가(defaultWeight0,IKPassfalse). 새 stateMachine/state2문서 추가. 상태 PistolReload는 원래 UpperCombat.PistolReload의 같은motion GUID/speed/writeDefaults를 사용. 기존 상태·전환·마스크·속도 등 기존문서 변경없음. 원래 UpperCombat.PistolReload는 그대로재생해 기존계약/표현을보존한다.
- ReloadUpperBody.mask 신규: Root/다리/IK0, Body/Head/팔/손가락1. 이 마스크는 새레이어에만 연결. 일반 UpperCombat.mask/MeleeCombat.mask는 그대로.
- PlayerCharacterReloadPoseTests.cs 신규3검사: 1/3인칭×서기/앉기×정지/이동8조건에서 yaw<45°,roll<22°,원래 장전손동작활성/루트·모델회전보존, cancel overlay해제. 완료·취소·컴포넌트disable/enable해제. 엎드려장전할때수평bodyrotation보존. 임계값은원본클립상체동작30°를허용하되문제의100°넘는이동혼입은허용하지않는다.

수정 후 이동장전 yaw33.31°/roll18.38°(TP),32.75°/18.01°(FP); 정지 약30.8°/2.45°; 앉기 약30°/11.3°. 클립의 자연스러운 회전을 완전히 고정시키는 수정이 아니다. Runtime aim procedural clamp를 새로 넣거나 원본 애니메이션을 편집하지 않았다.

## 전투기 예광 연장

- BulletTracer.cs: 기존 Begin(origin,diameter)에 optional lengthMultiplier=1 추가. 매 Begin에서 _activeLength=_length*max(.01,multiplier) 재설정, Advance의길이상한만 기존_length→_activeLength. 공통 base4m/굵기6/잔상.04s는유지.
- Bullet.cs: 기존 override Initialize(data) 진입점을 유지하되 InitializeWithTracer(data,1)로위임. 새 InitializeWithTracer가 기존base.Initialize/피해저장/Begin을 그대로 수행하고 길이배율만전달. 다른caller/물리·피해동작 보존.
- MachineGun.cs: private SerializeField Min(.01) _tracerLengthMultiplier=1 추가. 기존Fire의 bullet.Initialize(data)→InitializeWithTracer(data,배율). 메시/공통Pool/과열/발사 계약 유지.
- Fighter.prefab: MachineGun._tracerLengthMultiplier=3 한줄 추가. 전투기 최장12m, 기본권총/더미4m. 기존 문서는 이 한줄 외 바이트 동일(오디오 포함).
- BulletTracerTests.cs: FighterLengthMultiplierResetsForInfantryReuse 추가. 같은pool객체를3배로사용후재획득해defaultInitialize하면12→4복원, 현재scene기관총3배연결 확인. 기존 충돌점/잔상/cleanup 검사 유지.

## 설계/보존

장전 레이어의 수명은 기존 애니메이션 Presenter가 소유하고, 탄약은Magazine가소유. 추가매니저/컴포넌트없음. Animator override layer와AvatarMask를 구성으로 사용, 클립과일반마스크는수정안함. 예광배율은무기가선택하고탄환초기화가전달, Tracer가프레임별길이상태를소유해다음발사시항상리셋. OOP/SOLID 의도적예외없음. 삭제한기존메서드/필드/자산없음; 두기존메서드본문은위설명대로확장/위임만했다. Scene/카메라/입력/이동수치/원본모션/탄속1000/수명1/충돌·피해정책/기존음원설정유지.

## 파일 전체

- `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterCombatView.cs`
- `Assets/Features/PlayerCharacter/Animation/PlayerCharacter.controller`
- `Assets/Features/Projectile/Scripts/BulletTracer.cs`
- `Assets/Features/Projectile/Scripts/Bullet.cs`
- `Assets/Features/Weapon/Scrips/MachineGun.cs`
- `Assets/Features/Fighter/Prefabs/Fighter.prefab`
- `Assets/Tests/PlayMode/BulletTracerTests.cs`
- `Assets/Features/PlayerCharacter/Animation/ReloadUpperBody.mask`
- `Assets/Features/PlayerCharacter/Animation/ReloadUpperBody.mask.meta`
- `Assets/Tests/PlayMode/PlayerCharacterReloadPoseTests.cs`
- `Assets/Tests/PlayMode/PlayerCharacterReloadPoseTests.cs.meta`

문서: Docs/AI/ReloadShouldersAndTracerChanges.md 신규, Docs/AI/UnityProjectContext.md 결과추가. 임시probe/캡처/Editor생성기는Library에만보관하고제품Assets에는추가안함.

## 검증

기존상태에서 실패1재현→전용레이어후주요회귀1통과→완료/취소/엎드리기/예광포함집중7/7통과. 추가검사의 최초컴파일은잘못가정한TrySetProne API로실패했고실제TrySetStance로수정했다. 원본런타임API를바꾸지않았다. 최종 Unity6000.4.8f1 전체 무음PlayMode159/159 통과, 실패/건너뜀0(제품151+격리 임시진단/캡처8). 일반조준/상체이동/장전/착석/자세/투사체·팀피해/재질/예광 회귀 포함. 장전90프레임×2(50fps)의 전후비교를 렌더해 직접 확인했고, 같은 실제 아군 소유자 발사캡처에서 기관포 예광12m와 기존보병4m를 확인했다. 원본 Controller의 기존 상태/전환문서 전부 바이트보존, 기존 controller문서에는 새레이어만추가. Fighter는배율참조한줄외동일. 모든 적용파일은최종검증본과바이트동일. 증거full.xml/log, reload-comparison.mp4, cannon-tracer.mp4. 비교영상 왼쪽은새레이어비활성의기존동작,오른쪽은수정후동작이다. 사용자Editor의세부동작체감·시점전환과타겟빌드는직접확인하지않음.
