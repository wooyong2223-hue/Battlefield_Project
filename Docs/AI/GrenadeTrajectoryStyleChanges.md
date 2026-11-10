# 배그 영상 기준 수류탄 궤적 안내선 스타일

2026-11-08. 사용자 영상 https://www.youtube.com/watch?v=90OXugQo_jg 의 18초·29초를 브라우저에서 직접 확인했다. 짧은 흰색 곡선, 주황색 끝부분, 어두운 테두리를 기준으로 표시를 조정했다. 배그의 내부 수치나 모든 상황의 픽셀 단위 일치를 확인한 것은 아니다.

## 변경 전 비교와 구조

직전 작업의 `Library/CodexGrenadeThrowModes/applied.json`에 기록된 10개 파일은 변경 전 모두 일치했다. 사용자·Editor의 새로운 겹치는 변경은 없었다. 씬·카메라 설정·투척 데이터·입력·애니메이션은 변경하지 않았다.

표시는 기존 GrenadeTrajectoryView가 담당하고, 투척 상태와 발사 조건은 PlayerCharacterGrenadeThrow가 소유한다. 새로운 매니저나 상속/인터페이스를 추가하지 않았다. 전용 렌더링 방식만 별도 셰이더/머티리얼로 분리했다.

## 모든 변경 파일과 이전/이후

1. `Assets/Features/UI/GrenadeTrajectoryView.cs`
   - 이전: BulletTracer 머티리얼을 `_instance = new Material(_material)`로 복제하고 `_BaseColor=(.2,1,1,1)`로 설정. `widthMultiplier=.035f`의 고정 월드 두께로 충돌 지점까지 전체 경로를 표시했다.
   - 이후: 전용 공유 머티리얼 사용. `_viewCamera`와 `Initialize(..., Camera viewCamera = null)`를 추가하여 투영 화면 기준 너비를 계산한다. `StyleGuide`가 실제 물리 샘플 중 처음 최대 .8초 구간의 끝에서 역으로 화면 높이 16% 이내 길이를 골라 표시한다. 최대값이며 가까운 충돌·시선 방향에 따라 더 짧아질 수 있다.
   - 흰색 RGB(.92,.92,.88), 보이는 구간의 약70%부터 주황색(1,.36,.06). 중심선 본체 약3px, 끝 약5px 외곽 너비. 월드 너비로 환산하여 거리/FOV에 따른 과도한 두께 변화를 줄인다. 카메라 미지정 시 .025 월드 두께와 최근12샘플을 사용한다.
   - `_points`/`_lengths` 버퍼로 길이를 구하고 Gradient alpha로 나머지 경로를 숨긴다. 전체 충돌/중력 예측 좌표는 유지하므로 LineRenderer.positionCount는 보이는 구간보다 길 수 있다. 실제 투척·도화선·피해는 변경하지 않았다.
   - 제거/교체: `_instance` 필드, Configure의 복제/청록색 지정, OnDestroy의 복제 머티리얼 해제. 새 공유 에셋을 소유·파괴할 필요가 없어 제거했다. 기존 .035 두께는 화면 기준 widthCurve로 교체했다.
2. `Assets/Features/PlayerCharacter/Editor/LocalTacticalActionSetup.cs`
   - `Assets/Features/Projectile/Materials/BulletTracer.mat` 참조를 `Assets/Features/UI/GrenadeTrajectory.mat`로 교체했다. 씬 Camera 참조를 Initialize에 전달한다. 현재 SampleScene 카메라는 하나이며 카메라 오브젝트·시야각·위치는 수정하지 않았다. 기존 에디터/빌드 복사본 연결 경로를 활용한다.
3. `Assets/Features/UI/GrenadeTrajectory.shader` (신규)
   - URP 투명 Unlit. LineRenderer 정점 색·알파를 반영하고 UV 가로 가장자리에 어두운 테두리를 그린다. Blend SrcAlpha OneMinusSrcAlpha, ZWrite Off, ZTest Always, Cull Off. HUD처럼 선이 가려지지 않도록 그리되 예측 좌표 자체는 첫 충돌에서 끝난다.
4. `Assets/Features/UI/GrenadeTrajectory.shader.meta` (신규)
   - Unity 생성 ShaderImporter GUID. 머티리얼의 셰이더 참조를 고정한다.
5. `Assets/Features/UI/GrenadeTrajectory.mat` (신규)
   - 전용 셰이더 참조, `_BaseColor=(1,1,1,1)`. 본체/끝 색은 View의 정점색이 제공한다. 기존 BulletTracer 에셋과 총알 예광 효과는 그대로 보존했다.
6. `Assets/Features/UI/GrenadeTrajectory.mat.meta` (신규)
   - Unity 생성 머티리얼 GUID. 설정 코드가 이 에셋을 연결한다.
7. `Docs/AI/GrenadeTrajectoryStyleChanges.md` (이 문서)
8. `Docs/AI/UnityProjectContext.md` (현재 상태·검증 기록 추가)

## 검증과 한계

- 격리 프로젝트 `Library/CodexInfantryWork/Project`에서 Unity6000.4.8f1, `-batchmode -noaudio`로 셰이더 컴파일·머티리얼 생성 성공. setup.log의 TRAJECTORY_STYLE 기록, Unity exit0.
- 관련 PlayMode19/19 통과, 실패/생략0, 14.1846134초. 실제 발사와 예측 속도·위치 일치, 우클릭 방식 전환, 취소/입력 소비, 얇은 벽·가까운 충돌, 더미 피해 회귀 포함. 표시용 임시 캡처 테스트1개 포함이며 프로덕션 테스트 파일은 수정하지 않았다.
- `TEST_AUDIO captured previous=False`, PlayMode muteTrue, `restored=False` 확인. 실제 게임 오디오 설정은 수정하지 않았다.
- 960×640 FPS/TPP × 위/아래/다시 위 6개 캡처 생성. 대표4개를 직접 확인했다. 짧은 흰색/주황색 안내선이 모두 보인다. FPS에서는 기존 수류탄 기본 팔 포즈가 그대로 보인다. 이번 작업은 모션을 수정하지 않았다.
- 로그·XML·이미지: `Library/CodexGrenadeTrajectoryStyle/`. 임시 에디터 생성기와 캡처 소스는 검사 후 Assets 밖 같은 폴더로 보관한다.
- 이전 전체214개 통과 결과는 이전 작업 결과다. 이번에는 변경 관련19개만 재실행했다. 참조 영상과 픽셀 단위 동일성을 보장하지 않는다. 실제 SampleScene의 사용자 해상도/극단 시선에서 길이·위치·너비 체감은 Editor 확인이 남았다. 이전 Editor 재로딩/충돌 문제를 해결했다고 주장하지 않는다.
- SRP: View는 표시, 기존 ability는 투척 상태/발사 조건, shader는 렌더링. 공유 머티리얼의 런타임 변경이나 강제 다운캐스트, 불필요한 상속/추상화가 없다. SOLID 원칙에 대한 의도적 예외 없음. 변경 범위에서 씬 구조·사용자 수치·애니메이션을 보존했다. 커밋하지 않았다.
