# Quaternius UAL 1·2 무료 Standard 전체 모음

2026-10-28. 모든 무료판 자료를 먼저 확보한 뒤 활용하기 위한 모음이다. 이 폴더의 새 클립은 Animator/씬/입력에 연결하지 않았다. 기존 적용본과 설정은 유지한다.

| 팩 | 원본 클립 항목 | InPlace | RootMotion | 전체 |
|---|---:|---:|---:|---:|
| UAL1 Standard | 43 | 43 | 43 | 86 |
| UAL2 Standard | 43 | 43 | 43 | 86 |
| 합계 | 86 | 86 | 86 | 172 |

팩별 항목 수에는 T포즈 같은 참조/공통 클립도 포함된다. 172가지는 서로 다른 동작 수가 아니라 Root Motion 버전을 포함한 파일 수다. 공식 페이지의 120+/130+ 전체 제품 구성과 무료 Standard의 실제 43/43클립을 구분한다. Pro/Source판은 구매하거나 다운로드하지 않았다.

## 위치

- Unity 전체 모음: Assets/ThirdParty/Quaternius/Library/UAL1 및 UAL2 아래 InPlace/RootMotion.
- 현재 게임에서 사용하는 기존 적용본: Assets/ThirdParty/Quaternius/Animations. 이 폴더는 이번에 변경하지 않았다.
- 원본 ZIP: LocalAssets/AnimationLibrary/Quaternius/Archives/UAL1_Standard.zip 및 UAL2_Standard.zip.
- 원본 전체 파일: LocalAssets/AnimationLibrary/Quaternius/Raw/UAL1 및 Raw/UAL2. FBX/GLB의 일반·RM 버전, 포함된 참조 모델/Blender 파일/허가문/기타 패키지 파일을 그대로 보관한다.
- 클립별 이름·분류·길이·경로·검증: Docs/AI/QuaterniusAnimationCollection.csv.
- 모든 신규/변경 파일과 해시: Docs/AI/QuaterniusAnimationCollectionFiles.csv.

## 출처와 처리

Author: Quaternius. CC0; 원본 Raw 내부 License.txt를 보존한다.
UAL1: https://quaternius.itch.io/universal-animation-library
UAL2: https://quaternius.itch.io/universal-animation-library-2

공식 무료 다운로드로 새로 받은 두 압축파일의 SHA256은 기존 보유본과 일치한다. 로그인·가입·결제는 하지 않았다. 모든 원본 바이트를 보존하고, Unity 컬렉션용 추출본만 다음 처리를 적용했다.

- Unity 6000.4.8f1 Humanoid로 일반/RM FBX의 모든 클립을 추출.
- 이름의 Armature| 접두어를 제거하고 팩/변형 폴더로 구분.
- 기존 프로젝트와 같은 RootQ/RootT Y축 180도 방향 보정.
- 이름이 Loop로 끝나는 클립은 Loop Time/Loop Blend 설정. 그 외 액션/포즈는 반복을 자동으로 켜지 않음.
- RootMotion 폴더는 원본 _RM.fbx에서 추출했다는 뜻이다. 실제 root motion 사용은 이후 Animator/이동 권한 설계에 따라 결정하며 이번에는 켜지 않는다.
- .meta는 격리 Unity가 생성한 고유 GUID를 보존한다.

## 검증 범위

두 원본 ZIP 무결성 검사, 압축해제 파일의 원본 바이트/해시 일치, 172개 클립의 Humanoid/양수 길이/커브 존재, 현재 캐릭터에서 각각 10%·50%·90% 시점의 골반·머리·양손·양발 유한 위치 검사를 완료했다. 근거는 Library/CodexQuaterniusCollection/export.log 및 clips.json에 있다.

각 모션의 군사 동작 적합성, 무기 그립/지면·장애물 접촉, 1인칭 구도, 실제 root motion 변위는 개별 채택 단계에서 검토한다. 수집 성공을 모든 모션의 품질/게임 연결 완료로 표시하지 않는다. 추가 활용·기능 구현은 이번 단계에서 진행하지 않았다.
