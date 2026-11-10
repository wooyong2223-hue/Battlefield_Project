# 수류탄 모션 제외·기능 우선 적용

2026-11-08. 사용자 요청: 모션은 보류하고 4번 장착 / 왼쪽 버튼 유지 궤적 / 해제 투척 / 4 또는 무기 선택 취소 기능을 먼저 마무리한다.

## 변경 전 비교

직전 합성 작업 스냅샷의 제품 스크립트·에셋 20개가 모두 동일했다. UnityProjectContext의 차이는 이후 배그 원본 평가 기록이었다. 기존 씬/사용자 오프셋/컨트롤러/클립/피해·속도 수치를 보존했다. 이번 변경은 아래 8개 코드·설정 파일과 문서 2개에 한정했다.

기존에는 TryEquip이 ThrowAnimation과 장착/대기 클립을 요구하고, Equipping이 EquipAnimation.length만큼 지속됐다. TryThrow는 ThrowAnimation.length×ReleaseFraction 시점에 실제 발사했으며, 이후 클립 종료와 .12초 복귀까지 장비를 유지했다. 표시 컴포넌트는 수류탄 레이어·왼팔 레이어를 재생하고 1인칭 오프셋을 투척 진행에 따라 움직였다.

이 경로를 애니메이션 설정에 따라 선택하도록 교체했다. 현재 설정은 모션 OFF이며 즉시 Ready/즉시 Release를 사용한다. 기존 클립·마스크·레이어·참조·애니메이션용 메서드/필드는 삭제하지 않았다.

## 파일별 변경

| 파일 | 구체적 변경과 이유 |
|---|---|
| `Assets/Features/Weapon/Scrips/GrenadeDefinition.cs` | `_useAnimations` bool과 읽기 전용 UseAnimations 추가. 기본 false. 추후 모션 사용 여부를 데이터가 소유한다 |
| `Assets/ThirdParty/MixamoLocal/TacticalActions/FragGrenade.asset` | `_useAnimations: 0` 명시. 기존 클립 참조와 모든 수치/오프셋 유지 |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterGrenadeThrow.cs` | 모션 OFF이면 클립이 없어도 TryEquip 가능하고 즉시 Ready. TryThrow에서 Release 후 Cancel까지 동기 실행. Progress/HandlingProgress/PoseWeight=0이며 Tick은 게임플레이 취소 조건만 확인. Release는 void→bool로 변경해 엄폐물 차단/발사 실패를 호출자에게 알림. 성공 때만 수량 감소. 애니메이션 타이머에 종속된 발사 지연 제거 |
| `Assets/Features/PlayerCharacter/Scripts/TacticalAnimationSet.cs` | OFF일 때 수류탄 클립·자리표시 참조가 없어도 프로필 유효. IsApplied/Read/Apply는 Mantle 슬롯만 관리한다. 수류탄 모션 override를 설치하지 않되 기존 등반 동작은 유지 |
| `Assets/Features/PlayerCharacter/Scripts/PlayerCharacterTacticalAnimationView.cs` | OFF일 때 수류탄 두 레이어 가중치 0, 상태 CrossFade 안 함. 1인칭은 기존 HandlingFirstPersonOffset을 고정 사용하고 투척 오프셋 보간 안 함. 장비 숨김/수류탄 손 연결은 유지 |
| `Assets/Tests/PlayMode/TacticalActionTests.cs` | 기존 지연/예약 투척 기대를 즉시 1회 발사로 교체. 해제 직후 수량·활성 탄체 확인, 테스트의 불필요한 200프레임 대기를 30으로 줄여 실제 도화선 3초 전에 활성 여부 검사. 벽 앞 TryThrow 실패/수량 유지 확인. 이전 GrenadeReleaseDuringEquipmentQueuesOnceAndThinCoverCancelsRelease는 GrenadeReleaseIsImmediateOnceAndThinCoverCancelsRelease로 변경 |
| `Assets/Tests/PlayMode/GrenadeHandlingAnimationTests.cs` | 3장비×2시점에서 수류탄의 모든 클립·자리표시 참조를 지운 복사본으로 기능 검사 추가. 즉시 장착, 레이어 0, 궤적, 예측 위치/속도와 동기 발사 일치, 재투척 거절 확인. 기존 모션 검사는 GrenadeAnimationTestScope로 메모리 복사본에서만 모션 ON, 종료 때 원래 참조 복원 |
| `Assets/Tests/PlayMode/GrenadeThrowCompositionTests.cs` | 기존 조합 검사를 메모리의 명시적 모션 ON 범위에서만 실행. 사용자의 실제 에셋 OFF 상태를 변경하지 않고 보존된 옵션의 회귀 검사 유지 |
| `Docs/AI/UnityProjectContext.md` | 최신 조작과 모션 OFF·검증 기록 추가 |
| 이 문서 | 코드/값 변경·이유·검증·한계 기록 |

### 변경 전후 핵심

```csharp
// 이전
state = Equipping; // EquipAnimation.length만큼 대기
// 버튼 해제 후 ThrowAnimation.length * ReleaseFraction에서 Release

// 현재 UseAnimations=false
state = Ready;
bool launched = Release();
Cancel();
return launched;
```

## 최종 조작

- 4: 즉시 수류탄 장착. 기존 총기는 숨김. 이전 슬롯은 보존.
- 왼쪽 버튼을 새로 누르고 유지: 궤적 표시. 버튼을 장착 전부터 누르고 있던 경우는 투척/조준으로 재해석하지 않음.
- 왼쪽 버튼 해제: 현재 시선과 모터 속도로 즉시 발사하고 기존 무기로 복귀. 유지 시간에 따른 쿠킹 없음.
- 유지 중 4 또는 1/2/3: 투척 취소, 수량 유지. 버튼 해제와 취소가 같은 프레임이면 취소 우선.
- 취소·복귀 후 계속 누르고 있던 버튼에 의한 총기 오발 차단.
- 벽 안/뒤 발사 방지, 사망·비활성화·제어 상실 등의 기존 취소 경로 유지.
- 도화선3초/속도18+상향4+이동속도/반경6/피해100/수량3 등 기존 설정 유지.

## 설계 검토

상태·발사는 PlayerCharacterGrenadeThrow, 데이터는 GrenadeDefinition, 표시는 기존 컴포넌트, 궤적은 GrenadeTrajectoryView가 소유한다. 새 런타임 관리자/상속/범용 인터페이스를 추가하지 않았다. 모션 OFF 경로는 클립/Animator 시간에 의존하지 않는다. 모션 ON 옵션은 보존하되 실제 설정은 OFF다. OOP/SOLID 검토상 의도적 예외 없음.

## 검증

격리 Unity `-noaudio` 관련 PlayMode 16/16 통과(15.0839997초), 실패/스킵0. 기존 아군 더미 기준 적/중립 피해·아군 보호·엄폐물·도화선·튕김 검사 포함. 모션 없는 상태의 3장비×2시점, 실제 Input System의 해제 입력, 취소 우선과 오발 방지 검사 포함. 보존된 모션 옵션은 별도 메모리 복사본에서 검사한다.

전체 PlayMode 212/212 통과, 실패/스킵0, 201.7041095초. 이 수에는 임시 화면 캡처 검사1개를 포함한다. 결과는 `Library/CodexGrenadeFunctionOnly/full.xml` 및 `full.log`. 임시 캡처 소스는 Library/CodexGrenadeFunctionOnly에 보관하고 격리 Assets에서도 제거했다. 실제 1·3인칭 장착/궤적/복귀 화면6장을 생성하고 대표 이미지를 확인했다. 팔은 기존 기본 포즈를 사용하므로 1인칭 자세/그립은 임시 표현이며 이번에 새 모션을 제작하지 않았다.

TEST_AUDIO는 이전 False / Play 중 True / 종료 후 False를 기록했다. 제품의 수정 코드·에셋8개와 검증 복사본의 SHA256 일치를 확인했다. 전체 변경10개 파일의 스냅샷은 `Library/CodexGrenadeFunctionOnly/applied.json`에 기록했다. Windows 빌드 및 사용자 Editor 조작감 확인은 별도다. 앞서 보고된 도메인 재로드 예외·네이티브 크래시를 해결했다고 주장하지 않는다. 원본 씬/백업/복구 씬/오디오 설정 변경·커밋 없음.
