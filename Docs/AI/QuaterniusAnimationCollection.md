# Quaternius 전체 무료 애니메이션 확보 기록

2026-10-28. 사용자 요청: 다른 애니메이션까지 모두 받은 다음 활용을 시작한다. Quaternius UAL1·2 무료 Standard 전체를 새로 다운로드하고 모든 클립을 별도 컬렉션으로 확보했다. 신규 게임 연결이나 기존 동작 변경은 하지 않았다.

구성/출처/처리/검증은 [모음 안내](../../LocalAssets/AnimationLibrary/Quaternius/README.md), 전체 172개 클립은 [클립 목록](QuaterniusAnimationCollection.csv), 모든 변경 파일은 [파일 목록](QuaterniusAnimationCollectionFiles.csv)을 참조한다.

## 정확한 범위

- UAL1 Standard: 원본 클립 43개, 일반43 + RM43 =86개.
- UAL2 Standard: 원본 클립 43개, 일반43 + RM43 =86개.
- 합계172개에는 공통 T포즈와 일반/RM 변형이 포함된다. 전체 제품의 서로 다른 250여 동작을 무료로 확보했다는 뜻이 아니다.
- 원본 ZIP2개와 압축 내부 파일22개 전체를 캐시가 아닌 LocalAssets/AnimationLibrary/Quaternius에 보존했다.
- 새 Unity 클립은 Assets/ThirdParty/Quaternius/Library에 보관. 현재 쓰는 Animations 폴더/Controller/Scene/스크립트/설정은 직전 검증 상태 그대로 유지했다.
- 무료 Standard만 받았으며 Pro/Source판 결제나 다운로드는 하지 않았다.

## 변경 파일과 이유

- LocalAssets/AnimationLibrary/Quaternius/Archives: 공식 원본 ZIP2개 신규 보관. 해시는 아래 표 참조.
- LocalAssets/AnimationLibrary/Quaternius/Raw: 두 ZIP의 실제 파일22개 신규 보관. 원본 허가문/모델/모든 포맷 포함. 기존 에셋을 대체하지 않음.
- LocalAssets/AnimationLibrary/Quaternius/README.md 신규: 출처/보관 경로/검증/미연결 상태 안내.
- Assets/ThirdParty/Quaternius/Library: 독립 클립172개 및 각각의 .meta, 하위 폴더 .meta, README와 .meta 신규. 다음 활용 전에 전체 목록을 Unity에서 탐색할 수 있게 준비.
- Docs/AI/QuaterniusAnimationCollection.csv 신규: 모든 클립의 팩/일반·RM/분류/원본 이름/경로/초·fps/반복/검증 상태.
- Docs/AI/QuaterniusAnimationCollection.md 신규: 이 기록.
- Docs/AI/QuaterniusAnimationCollectionFiles.csv 신규: 모든 파일의 경로/크기/SHA256 목록.

기존 파일·참조·메서드·필드·컴포넌트·에셋을 삭제하거나 교체하지 않았다. 새 게임 시스템/클래스가 필요한 작업이 아니므로 데이터 컬렉션으로만 분리했다. 임시 추출/검증 도구는 Library에만 있다.

## 검증과 다음 단계

압축 무결성, 원본 추출 바이트, Unity 임포트 및 516개 시점의 리타깃 샘플, 새 GUID 고유성, 이전 작업 파일 해시 보존을 확인했다. 해당 클립을 게임에 연결하지 않아 기존 PlayMode 전체를 새로 실행하지 않았다. 개별 자세의 시각 품질/지형·무기 접점/Root Motion 실제 이동은 이후 클립을 선택할 때 확인한다. 사용자의 Editor UI 조작·커밋·푸시 없음.

| 패키지 | 원본 SHA256 |
|---|---|
| UAL1 Standard | cc73fc4e495b82958207316596317a3f40b9fa38065bde1027937452da537724 |
| UAL2 Standard | 4008ea208a604773a2b2177d965f0f5d3195498b5bf838c3f5785d68e95f2a68 |
