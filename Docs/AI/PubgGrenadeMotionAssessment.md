# 배그 수류탄 모션 기준·보유 원본 점검

2026-11-04. 사용자 기준 영상: https://www.youtube.com/watch?v=90OXugQo_jg (대기배그, 배그 수류탄 꿀팁). 웹 검색/페이지 가져오기로는 확인되지 않아 인앱 브라우저의 실제 재생 화면을 확인했다. 영상 19초·29초에서 오른손을 어깨/머리 가까이에 둔 준비 자세와 궤적 표시를 확인했다. 전체 영상의 모든 동작·프레임을 측정한 것은 아니며 정확한 투척 시간/각도/1인칭 수치로 추정하지 않는다.

## 작업 전 상태

직전 `Library/CodexGrenadeComposition/applied.json`의 22개 파일 해시가 모두 일치했다. 따라서 기록된 모션/컨트롤러/참조와 충돌하는 새 변경은 발견하지 못했다. 원본 SampleScene, 백업, 복구 씬, 캐릭터 구조와 사용자 설정은 수정하지 않았다.

이번에는 제품 스크립트, Animator, 클립, 수류탄 설정을 교체하지 않았다. 외부 원본이 필요하면 중단하라는 기존 사용자 요청에 따라 적합한 장착/조준 대기 원본이 부족한 단계에서 적용을 보류했다.

## 원본 검증

격리 Unity에서 Starter Assets Humanoid에 단일 원본 AnimationClipPlayable을 연결하고 수동 시간으로 샘플링했다. 기존 합성/IK/장비/팔 마스크를 거치지 않아 원본의 전신 동작을 구분했다. 4개 클립 각각 31개 시점, 총 124개 위치 샘플과 44개 이미지를 생성했고 대표 구간을 시각적으로 확인했다. 제품에서 사용할 새 클립을 제작한 것이 아니다.

| 사용자 원본 | 확인 내용 | 배그용 판단 |
|---|---|---|
| `LocalAssets/Mixamo/Throw.fbx` | 2.2초, 서서 팔을 뒤에서 위로 들어 던지는 동작. 약 0.66초 부근 손이 머리 옆으로 올라오지만 이후 연속 투척 | 투척 후보. 배그의 장착/대기 전체 세트가 아님 |
| `LocalAssets/Mixamo/Throw Object.fbx` | 4.866667초, 물체를 바닥에서 집는 숙임이 선행하고 일어나서 던짐 | 전체 진행률을 Throw에 맞추는 것만으로 배그 동작이 되지 않음. 왼팔 단순 합성은 비교용 |
| `LocalAssets/Mixamo/Throw Grenade.fbx` | 3.5초, 전신 원본은 엎드린 자세의 투척 | 서 있는 수류탄 동작용으로 부적합. 이전에 팔 마스크로 사용하면서 이 전신 맥락을 놓친 선정 문제를 확인 |
| `LocalAssets/Mixamo/Basic Shooter Pack/toss grenade.fbx` | 3.2초, 서 있는 준비·투척·복귀 동작 | 추가 투척 후보. 전용 장착/조준 유지 루프가 별도 제공되는 세트는 아님 |

네 번째 원본은 격리 프로젝트의 `LocalAssets/Mixamo/Toss Grenade Basic.fbx`와 `Assets/ThirdParty/MixamoLocal/TacticalActions/Toss Grenade Basic.fbx`로만 복사/가져왔다. 메인 프로젝트의 Assets에는 추가하지 않았다.

## 필요한 동작과 현재 확보 상태

1. 장착: 수류탄을 꺼내 손에 쥐는 원본. 현재는 이전에 제작한 GrenadeEquip만 있고 적합한 외부 원본 미확보.
2. 준비: 낮게 든 자세에서 투척 준비로 올라가는 동작. 기존 원본의 부분 구간은 후보지만 배그 준비와 동일하다고 확정할 수 없음.
3. 조준 유지: 오른손을 어깨 가까이 둔 자연스러운 반복 대기. 현재는 이전 제작 GrenadeIdle뿐이며 전용 외부 원본 미확보.
4. 투척·복귀: Throw/toss grenade의 원본 구간이 후보. 준비 자세 접점, 몸통 회전, 실제 손에서 놓이는 시점 검증이 필요.
5. 취소·보관: 준비 자세에서 수류탄을 내리고 넣는 원본 미확보. 임의 역재생/관절 값으로 완료 처리하지 않음.

위 명칭은 필요한 동작의 설명이며 Mixamo에 그대로 존재하는 검색 결과/클립명이라는 뜻이 아니다. 배그 원본 추출을 요청하는 것도 아니다. 사용 가능한 외부 에셋에서 해당 동작을 확보한 뒤 현재 모델에 검증해야 한다. 상하 조준, 손/수류탄 접점, 1인칭 프레이밍은 클립 확보와 별도의 연결 검증 항목이다.

## 크래시 진단

이전 실제 Editor 로그에는 OnEnable → PlayerCharacterCombatView.RefreshEquipment → Animator.runtimeAnimatorController setter의 네이티브 크래시가 있었다. 이 진입 경로를 격리 Editor에서 Play 진입 → Play 중 도메인 재로드 → 2/3/1 장비 교체 → Play 종료로 3회 실행했다. 네이티브 크래시는 재현되지 않았다.

그러나 정상 통과가 아니다. 재로드 후 PlayerCharacterController.Update:105와 VehicleSeat.RefreshDriverControl:151의 NullReferenceException, Input System OnEnable 관련 오류, Unity Editor SearchDatabase의 ArgumentOutOfRangeException을 기록했다. 생존/종료 코드 0만으로 동작 정상이나 크래시 해결을 주장하지 않는다. 해당 참조의 수명/재연결 문제는 확인해야 할 별도 결함이고 네이티브 크래시의 확정 원인은 아니다. 추측에 따른 런타임 코드 수정은 하지 않았다.

## 변경 파일

- 이 문서 신규: 기준 영상, 원본 판정, 부족한 동작, 크래시 진단과 후속 조건 기록.
- `Docs/AI/UnityProjectContext.md`: 최신 평가/적용 보류 상태 추가.

제품 클래스/인터페이스/상속/직렬화 값/씬/에셋의 추가·삭제·교체 없음. 따라서 이번 제품 OOP/SOLID 구조 변경도 없다.

## 검증 자료와 정리

- `Library/CodexPubgGrenade/source.xml`: 3개 원본 샘플링 검사 1/1 통과, 2.012967초.
- `Library/CodexPubgGrenade/toss.xml`: 추가 원본 샘플링 검사 1/1 통과, 1.3133319초.
- `source-samples.csv`, `toss-samples.csv`, PNG 44개: 원본 전신 프레임 자료.
- `reload.log`: 도메인 재로드 3회 진단, 예외 포함. 성공 테스트로 집계하지 않는다.
- 임시 `CodexPubgGrenadeSourceProbe.cs`와 `CodexGrenadeReloadProbe.cs` 및 메타는 이 Library 폴더로 이동해 격리 Assets에서도 제거한다. 두 스크립트는 제품에 추가하지 않았다.
- 전부 `-noaudio`. PlayMode 원본 검사에서 TEST_AUDIO 음소거/복원 확인. 별도 Editor 재로드 진단도 이전 False → 진단 중 True → 종료 후 False 복원 확인. 사용자 Editor/AudioSource/프로젝트 오디오 설정 변경 없음.

아직 새 모션이 적용됐거나 배그 동작이 재현됐다고 표시하지 않는다. 필요한 외부 원본 확보와 재로드 오류 정리 후 단계별 연결·시각 검증이 필요하다. 커밋 없음.
