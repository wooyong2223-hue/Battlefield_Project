# 상하 조준 간헐 현상 조사

사용자가 양 시점에서 팔만 움직인다고 보고했으나 이후 정상 동작한다고 확인했다. 원인을 확정하지 않았으며 이번 조사만으로 간헐 현상을 해결했다고 주장하지 않는다. 기존 상체 조준 게임플레이 코드는 이번 조사에서 교체하지 않았다.

저장된 SampleScene과 열린 Editor의 복구 스냅샷을 격리 프로젝트에서 각각 조사했다. Humanoid Avatar, Animator, IK 패스, 상체·머리 가중치와 참조가 존재했다. 가상 Mouse/Keyboard를 통한 실제 InputSystem 입력과 실제 Animator 평가에서 양 시점의 가슴·머리 회전을 확인했다. CPU BakeMesh 검사에서는 몸통 가중치가 큰 정점 1499개를 대상으로 약 0.45~0.47m의 최대 이동을 관측했다. 단순 Transform 값만으로 정상 여부를 판단하지 않았다. 격리 스냅샷은 원본 씬에 덮어쓰지 않았다.

## 추가 파일

| 파일 | 기능 |
| --- | --- |
| Assets/Features/PlayerCharacter/Editor/PlayerCharacterAimDiagnostics.cs | Editor 전용 수동 진단기. Tools/Battlefield/Diagnostics/Record Upper Body Aim (2 minutes)로 시작하고 Stop Upper Body Aim Recording으로 중단한다. 기본 자동 기록은 꺼져 있다. 0.2초 간격으로 입력 캡처·시점·조준 각도·공격/장전·IK 조건·상체 블렌드·본 회전·렌더러 연결을 관찰한다. 게임 오브젝트나 Animator 자세는 변경하지 않는다. |
| Assets/Features/PlayerCharacter/Editor/PlayerCharacterAimDiagnostics.cs.meta | Editor 진단 스크립트 GUID. |
| Assets/Tests/PlayMode/PlayerCharacterNativeAimInputTests.cs | 실제 PlayerCharacterInput 경로에 가상 마우스·V 입력을 전달해 양 시점의 -60도 조준, 가슴·머리 회전과 몸통 스키닝 이동을 검사한다. View.Tick/Animator.Update로 자세를 직접 주입하지 않는다. 입력 캡처 플래그는 테스트에서만 설정한다. |
| Assets/Tests/PlayMode/PlayerCharacterNativeAimInputTests.cs.meta | 회귀 검사 GUID. |
| Docs/AI/PlayerCharacterAimRuntimeInvestigation.md | 이 조사 보고서. |
| Docs/AI/UnityProjectContext.md | 결과와 원인 미확정 상태를 기록한다. |

기록은 Library/PlayerCharacterAimDiagnostics/live.jsonl 및 status.json에 남는다. 새 기록을 시작하면 이전 live.jsonl은 비운다. 최대 2분이며 Play 종료 시 중단한다. 파일은 Assets 밖에 생성된다. 진단 클래스가 비공개 IK 상태를 읽는 reflection은 Editor 도구에만 한정했으며 런타임 공개 API를 늘리지 않았다.

재현 시각의 로그와 화면이 있어야 원인을 추가로 좁힐 수 있다. 조사 중 한 번 Invalid worldAABB가 발생했으나 다음 스키닝 검사에서 재현하지 못했다. 이를 무시하도록 로그를 억제하지 않았으며 해당 오류와 사용자 증상의 인과관계는 확인하지 못했다.

독립적인 native-skin.xml 검사는 1/1 통과했다. 최종 전체 회귀 결과는 주먹 방향 변경 보고서에 함께 기록한다. 사용자의 실제 열린 Editor에서 간헐 증상 재발 여부는 추가 관찰이 필요하다.
