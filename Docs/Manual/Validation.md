# 검증 기록

[목차](<README.md>)

2026-11-10 기준 소스 점검. Unity 6000.4.8f1, Input System1.19.0, URP17.4.0, Test Framework1.6.0.

## 실행 환경

- 격리 프로젝트 `Library/CodexInfantryWork/Project`에서 `-batchmode -noaudio`로 실행했다.
- 현재 Features/Framework/Tests의 C#·asmdef·관련 meta와 SampleScene, Input Actions, Fighter.prefab을 복사했다. 외부 에셋/패키지는 기존 검증 복사본을 사용했다. 전체 에셋의 새 체크아웃 검증은 수행하지 않았다.
- 원본 게임 소스·씬·설정 변경 없이 문서와 Library 진단 산출물을 생성했다.
- TEST_AUDIO 로그로 진단 중 mute와 이전 상태 복구를 확인한다.

## PlayMode 결과

총 **43개**, 통과 **39개**, 실패 **4개**, 실행 시간 34.8016068초.

대상은 PlayerCharacterCombatTests, PlayerCharacterCombatInputTests, TacticalActionTests, FighterEngineTests, VehicleSeatTests, CombatStructureTests다.

| 실패 테스트 | 관측 결과 |
|---|---|
| EmptyMagazineReloadsOnlyAfterFireIsPressedAgain | Expected: 0 /   But was:  1 |
| FiringKeepsGripWhileReloadAndMeleePlayTheirActionStates | Expected: greater than 0.0f /   But was:  0.0f |
| RealMuzzleAmmoReloadAndFirstPersonPresentationAreConnected | Expected: 1 /   But was:  0 |
| GrenadeClickCannotFireTheGunAfterReturningButANewClickCan | A fresh click after returning must fire normally. /   Expected: greater than 0 /   But was:  0 |

권총 단발 경로는 IPlayerCharacterCombatInput.FirePressed를 읽는다. CombatTests의 실패3개는 FireWeapon만 true로 넣어 발사가 발생하지 않는다. TacticalActionTests의 실패1개는 가짜 입력을 _input에만 넣고 _combatInput의 새 클릭을 제공하지 않는 구성이다. 이 실패는 테스트 입력의 변경 누락으로 설명되며, 실제 마우스 경로의 무결함을 증명하지는 않는다.

기존 2/2 또는214/214 기록은 과거 코드 결과다. 현재 결과와 혼용하지 않는다. 새로운 B 전환·권총 단발·장전 회귀를 포괄하는 자동 테스트 보완이 필요하다.

## 정적 검사 범위

- Features·Framework 179개 파일, 14279줄을 목록화하고 타입/메서드/설정/직렬화 연결을 추출했다.
- 중심 제어·장비·입력·HUD·애니메이션·탑승·표적·풀·오디오 흐름을 검토했다. 모든 메서드의 실행 경로를 검증한 범위는 아니다.
- SampleScene 및 Features 프리팹의 m_Script GUID를 Assets와 PackageCache 메타에 대조했다. 프리팹 인스턴스의 stripped 참조는 별도 표시한다.
- 씬의 모든 필드가 올바른 의미의 오브젝트를 가리키는지, 모든 애니메이션 품질이 적절한지는 Unity 화면 검토가 남아 있다.

## 남은 검증

씬·Features 프리팹에서 Assets 메타만으로 찾지 못한 스크립트 GUID 10종(참조93곳)은 PackageCache 메타에서 모두 확인했다. 이 검사 범위에서 미해결 스크립트 GUID는 0개다. 이는 모든 직렬화 필드의 연결 정확성을 보장하는 검사는 아니다.

실제 입력의 단발/연사/빈 탄창 재클릭, Animator 자세 전환, 탑승 전후 복구, 각 오디오 청취, 모든 테스트 전체 실행, Player 빌드와 새 체크아웃 재현, CPU/GC 프로파일링.

## 원시 결과

`Library/CodexArchitectureAudit/results.xml` · `Library/CodexArchitectureAudit/unity.log` · `Library/CodexArchitectureAudit/inventory.json`
