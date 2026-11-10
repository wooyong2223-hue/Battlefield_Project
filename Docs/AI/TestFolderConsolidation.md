# 씬·테스트 폴더 통합 및 무음 검사

2026-10-29. 사용자 요청: 제작 씬은 하나만 사용하고 불필요한 Tests 배치 에셋을 통합, 사운드 검사 외에는 음소거.

## 작업 전 비교

직전 테스트장 배치/메타/SampleScene은 이전 최종 검증본과 바이트 동일했다. 비교 씬·controller·mask도 이전 적용본과 같았다. 겹치는 사용자 변경 없음. SampleScene 직렬화 값/계층·빌드 설정·사용자의 현재 Starter Assets 경로는 보존했다.

## 삭제한 이전 구현

CharacterUpperBodyAnimationTest는 SampleScene 복사본에 전용 Animator controller override를 연결한 실험이었다. controller는 UpperCombat avatarMask만 AuthoredUpperBody.mask로 교체했고, mask는 Body/Head 슬롯0→1, 팔/손가락1, root/legs/IK0이었다. 당시 발생한 몸통 방향/시점 전환/착석 비교 문제4개는 원본에 합치지 않고 실험 씬에 분리해 둔 상태였다. 사용자 요청으로 이 비교 씬·전용 controller·mask와 메타를 삭제했다. 기존 PlayerCharacter.controller/UpperCombat.mask나 SampleScene에 실험 설정을 합치지 않았다. 삭제 GUID를 참조하는 다른 제품 에셋/코드는 없었다. 메서드·런타임 필드·기존 게임 컴포넌트 삭제 없음.

삭제 파일 전체:

- `Assets/Tests/Scenes.meta`
- `Assets/Tests/Scenes/CharacterUpperBodyAnimation.meta`
- `Assets/Tests/Scenes/CharacterUpperBodyAnimation/AuthoredUpperBody.controller`
- `Assets/Tests/Scenes/CharacterUpperBodyAnimation/AuthoredUpperBody.controller.meta`
- `Assets/Tests/Scenes/CharacterUpperBodyAnimation/AuthoredUpperBody.mask`
- `Assets/Tests/Scenes/CharacterUpperBodyAnimation/AuthoredUpperBody.mask.meta`
- `Assets/Tests/Scenes/CharacterUpperBodyAnimation/CharacterUpperBodyAnimationTest.unity`
- `Assets/Tests/Scenes/CharacterUpperBodyAnimation/CharacterUpperBodyAnimationTest.unity.meta`

빈 CharacterUpperBodyAnimation 및 Tests/Scenes 디렉터리도 제거했다. 격리 프로젝트에만 있던 CodexAuthoredUpperSceneSmoke.cs(+meta)는 삭제 씬을 로드하므로 Library/CodexTestConsolidation/removed로 보관했다. 같은 격리 전용 과거 생성/검증 도구3개(CreateAuthoredUpperBodyTestScene/CreateInfantryTrainingGround/ValidateInfantryTrainingGround)와meta도 해당 Library 보관 폴더로 옮겼다. 제품 Assets의 스크립트를 삭제한 것이 아니다.

## 에셋 이동 전체

정적 테스트장 배치 책임을 Features/TrainingGround 에셋 폴더로 옮겼다. 새 매니저/추상화/컴포넌트는 필요하지 않으며 OOP/SOLID 의도적 예외 없음. 기존 GUID·fileID·에셋 바이트가 동일하여 SampleScene 참조를 변경하거나 씬을 재저장할 필요가 없다.

- `Assets/Tests/Scenes/InfantryTrainingGround/InfantryTrainingGround.prefab` → `Assets/Features/TrainingGround/InfantryTrainingGround.prefab`
- `Assets/Tests/Scenes/InfantryTrainingGround/InfantryTrainingGround.prefab.meta` → `Assets/Features/TrainingGround/InfantryTrainingGround.prefab.meta`
- `Assets/Tests/Scenes/InfantryTrainingGround/TargetAlly.mat` → `Assets/Features/TrainingGround/TargetAlly.mat`
- `Assets/Tests/Scenes/InfantryTrainingGround/TargetAlly.mat.meta` → `Assets/Features/TrainingGround/TargetAlly.mat.meta`
- `Assets/Tests/Scenes/InfantryTrainingGround/TargetEnemy.mat` → `Assets/Features/TrainingGround/TargetEnemy.mat`
- `Assets/Tests/Scenes/InfantryTrainingGround/TargetEnemy.mat.meta` → `Assets/Features/TrainingGround/TargetEnemy.mat.meta`
- `Assets/Tests/Scenes/InfantryTrainingGround/TargetNeutral.mat` → `Assets/Features/TrainingGround/TargetNeutral.mat`
- `Assets/Tests/Scenes/InfantryTrainingGround/TargetNeutral.mat.meta` → `Assets/Features/TrainingGround/TargetNeutral.mat.meta`
- `Assets/Tests/Scenes/InfantryTrainingGround/TrainingConcrete.mat` → `Assets/Features/TrainingGround/TrainingConcrete.mat`
- `Assets/Tests/Scenes/InfantryTrainingGround/TrainingConcrete.mat.meta` → `Assets/Features/TrainingGround/TrainingConcrete.mat.meta`
- `Assets/Tests/Scenes/InfantryTrainingGround/TrainingMarkings.mat` → `Assets/Features/TrainingGround/TrainingMarkings.mat`
- `Assets/Tests/Scenes/InfantryTrainingGround/TrainingMarkings.mat.meta` → `Assets/Features/TrainingGround/TrainingMarkings.mat.meta`
- `Assets/Tests/Scenes/InfantryTrainingGround.meta` → `Assets/Features/TrainingGround.meta`

## 유지한 자동 검사 구조

Tests/EditMode와 Tests/PlayMode는 각각 편집기에서 실행되는 순수 로직 검사와 실제 씬·물리·프레임 검사를 담당하며 서로 다른 asmdef/실행 조건을 사용한다. 런타임 스크립트와 합치면 테스트 의존성을 제품에 섞게 되므로 두 폴더를 유지했다. Tests에는 이제 검사 코드/asmdef만 남는다. 제작/플레이 씬은 SampleScene만 사용한다. 기존 외부 에셋의 데모 씬 파일은 공급자 에셋으로 남아 있지만 Build Settings는 기존처럼 SampleScene 한 항목이다.

## 무음 동작과 문서 수정

AGENTS.md에 Test audio 규칙 추가: 일반 자동 테스트/진단PlayMode/캡처는 Unity -noaudio, 사용자가 사운드 검사를 명시한 경우만 예외. 이미 실행 중인 Editor 진단은 해당 세션 동안만 음소거하고 원래 상태 복구. 제품 AudioSource volume/AudioListener/프로필/AudioSettings는 변경하지 않는다. 이번 전체 회귀 실행도 -batchmode -noaudio -runTests -testPlatform PlayMode로 실행, 로그의 COMMAND LINE ARGUMENTS에 -noaudio 확인.

문서 변경: Docs/AI/AuthoredUpperBodyTestScene.md 맨 위에 삭제된 과거 실험임을 표시; Docs/AI/InfantryTrainingGroundChanges.md의 현재 에셋 경로를 Features/TrainingGround로 갱신; Docs/AI/UnityProjectContext.md에 현재 단일 제작 씬/폴더/무음 정책 기록; 이 문서 신규 작성. 과거 실험 기록 자체는 삭제하지 않았다.

## 검증

SampleScene 파일 해시가 작업 전과 동일. 이동 에셋/메타 해시·GUID 동일. 삭제 GUID에 대한 Assets 내 코드/직렬화 참조 없음. Tests 폴더에는 EditMode/PlayMode만 남음. Unity6000.4.8f1 격리 PlayMode 무음 회귀 146/146 통과, 실패0, 건너뜀0. 제품140 + 격리 전용 진단/캡처6. 로그 Library/CodexTestConsolidation/full.xml/log. 사용자 Editor에 삭제된 비교 씬이 열려 있다면 SampleScene을 열어야 한다. 실제 사용자 Editor의 에셋 임포트 화면/재생은 직접 조작하지 않았고 빌드 검증은 하지 않았다.
