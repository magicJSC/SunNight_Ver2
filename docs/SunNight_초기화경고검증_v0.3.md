# SunNight 초기화·Missing Script 검증 v0.3

검증일: 2026-10-29 · Unity 6000.5.10f1 · 브랜치: `feature/level-design`

## 결과

현재 진행 경로인 **TutorialScene ↔ Boss1**에서 Play Mode 시작·종료 3회와 공장 진입·저장 복원 반복 9회를 수행했다. 매 세션의 시작 전부터 종료 후까지 수집한 Console 경고·오류는 모두 **0건**이다. 마지막 컴파일도 경고·오류 0건이다. 에디터는 저장된 TutorialScene의 Edit Mode로 복귀했고 강제 자동 틱은 해제했다.

이 검증은 실행 중인 Unity에서 실제 게임 API·UI 이벤트를 호출한 자동 검증이다. 수동 플레이나 모든 구형 씬의 정상 동작을 검증했다는 뜻은 아니다.

## 원인과 수정

### Missing Script 14건의 실제 원인

14건은 고유 컴포넌트 14개가 아니라, 튜토리얼의 벽 **2개가 반복 로드되면서 발생한 출력**이었다. 이전 보고서의 ‘에디터 복귀 시 경고’ 설명을 이 결과로 정정한다.

- `TutorialGrid/StartHouse/NoSortingWall_Back (1)`
- `TutorialGrid/StartHouse/NoSortingWall_Back (2)`

두 오브젝트의 누락 GUID는 `ea0da573d5a7fe94db4a8786d45becda`로 같았다. 대응하는 스크립트 메타 파일이 없고 해당 컴포넌트에는 사용자 직렬화 필드가 없었다. 두 오브젝트에는 정상 동작하는 `VisibleTile_Wall`, Tilemap, TilemapRenderer가 별도로 존재했다.

Unity의 `GameObjectUtility.RemoveMonoBehavioursWithMissingScript`로 확인된 누락 컴포넌트 2개만 제거하고 Editor API로 씬을 저장했다. 오브젝트나 정상 벽 컴포넌트는 삭제하지 않았다. 저장으로 Unity 6.5 직렬화 필드가 갱신됐으므로 씬 diff에는 추가 형식 변경이 있다. 저장 전 백업과 비교한 **타일맵 43개의 m_Tiles 셀 데이터는 모두 동일**하다.

### 초기화 경고

`MonoBehaviour`를 `new`로 만들던 9개 생성 경로가 원인이었다.

| 대상 | 수정 |
|---|---|
| GameManager, NetworkManager, InvenManager, UIManager, InputManager, SceneManagerEx, DataManager | 영속 `@Managers` 오브젝트에 `AddComponent<T>()`로 생성 |
| MapManager | 별도 가짜 인스턴스 대신 현재 씬의 `Managers.Game.grid` 참조 |
| DataManager의 PlayerStat | `new PlayerStat()` 제거, 실제 플레이어 컴포넌트 참조 |

매니저가 정상 컴포넌트가 되면 NetworkManager의 종료 콜백도 실제 호출된다. 연결 전 종료가 실패하지 않도록 연결된 세션만 정리하고, 로그인 HTTP 요청은 종료 시 취소한다. 중복 종료와 종료 후 연결 완료 콜백도 방어했다. 이번 실행에서는 로컬 로그인 서버가 없는 오프라인 경로를 검증했으며, 실제 서버와의 온라인 세션은 시험하지 않았다.

초기화 주변 컴파일 경고도 함께 수정했다. `SlotInfo`에 `[Serializable]`을 지정하고, 씬·맵 검색에는 현 Unity 버전의 `FindAnyObjectByType`을 사용했다. 네트워크 초기화 비동기 호출의 의도를 명시하고 종료 취소를 처리했다. 경고를 숨기는 필터나 `#pragma`는 추가하지 않았다.

## 검증 절차와 데이터

`Tools/StartupLifecycleAudit.cs`로 각 세션에서 다음을 반복했다.

1. 이전 실플레이로 획득한 `v03-postnight-final-pre-factory.json`을 복원한다.
2. 실제 저장 파일을 작성하고 필드에 재진입한다. 기존 파일 교체 저장도 반복한다.
3. 공장 진입 버튼 이벤트로 Boss1에 들어가 승계 데이터를 비교한다.
4. 플레이어 이동으로 보스 컷신을 시작한 뒤 진행 중인 애니메이션과 함께 필드를 다시 불러온다.
5. 자원·강화·수집/처치 이력·개별 시설 HP와 위치·플레이어 HP/허기·거점 HP를 비교한다. 필드에서는 날짜/시간과 플레이어/거점 위치도 비교한다.
6. 매니저 7개가 모두 실제 `@Managers` 컴포넌트인지, 맵이 현재 씬 소속인지 검사한다.
7. 열린 인벤토리와 진행 중인 애니메이션이 있는 상태로 종료하고 잔존·재생성된 매니저가 없는지 검사한다.

| 항목 | 결과 |
|---|---|
| Play Mode 시작·종료 | 3회 통과 |
| 공장 진입 및 컷신 중 복원 | 각각 9회 통과 |
| 반복 시험 내 저장 / 필드 복원 | 9회 / 21회 통과 |
| 자원·강화·개별 시설 데이터 | 모든 비교 통과 |
| 매 세션 전체 Console | 경고 0 / 오류 0 |
| 종료 후 Managers / @SoundRoot | 각 세션 0 / 0 |
| 최종 컴파일 | 경고 0 / 오류 0 |

첫 세션에서는 임시 초기화 로그를 실제로 읽었다. 이후 로그를 제거했고, 세 번째 세션은 검색 API 경고 수정까지 포함한 최종 코드로 실행했다. 검증 스크립트 자체의 컴파일 진단도 없었다.

별도로 첫밤 직후 저장을 복원하여 실제 제작 UI로 벽 3개를 만들고 배치했다. 고철 **4 → 1**, 대포 3기·벽 6개를 확인했다. 기존 벽의 손상 HP **7·19**도 보존됐다. 이 시험은 강화 전 저장을 사용하므로 강화 후 반복 시험의 시설 HP와 다르다.

반복 중 Console을 비우지 않았다. 관리 코드의 로그 콜백뿐 아니라 Unity Console의 원본 집계(`groundTruth.consoleWarnings`, `consoleErrors`)도 확인했다. 사용자 기본 저장 파일 대신 검증용 파일을 사용했다.

## 범위 밖의 구형 참조 5개

전체 씬·프리팹의 GUID 정적 검사에서는 다음 참조가 추가로 발견됐다. 위 실플레이 경로의 14건과는 별개이며, **이번에 수정하지 않았다**.

| 에셋 | 원인 조사 |
|---|---|
| `Assets/2.Scenes/GameScene.unity`의 Walkable | 구형 NavMeshModifier 형식의 누락 참조. 영역 설정 데이터가 있어 단순 삭제 대상이 아님 |
| `Assets/Resources/Prefabs/ProduceMatter/Tree.prefab` | 삭제된 Tree 스크립트. Git 이력상 주변에 나뭇가지 5개를 생성하던 기능 |
| `Assets/Resources/Prefabs/Items/TranshCan.prefab` | 삭제된 CanInteractChecker 참조 |
| `Assets/Resources/Prefabs/Seller.prefab` | 삭제된 CanInteractChecker 참조 |
| `Assets/Resources_moved/Prefabs/GraveStone.prefab` | 삭제된 CanInteractChecker 참조 |

CanInteractChecker는 현재 PlayerInteract로 바뀌기 전 API를 사용한다. Tree 생성 로직 재도입은 자원량을 바꾸고, 구형 상호작용 복구에는 프리팹의 현재 사용 여부와 구조 확인이 필요하다. 따라서 전체 프로젝트의 모든 누락 참조가 해결됐다고 보고하지 않는다. 이 에셋들을 사용하는 경로는 별도 마이그레이션·검증 대상이다.

## 증거와 재현 도구

- [집계 및 Console 원본 카운트](level-design/v03-startup-verification.json)
- [세션 1](level-design/v03-startup-run1-results.json) · [세션 2](level-design/v03-startup-run2-results.json) · [최종 코드 세션](level-design/v03-startup-run3-results.json)
- `Tools/StartupWarningAudit.cs`: 참조 검사, 확인된 벽 컴포넌트 정리, 열린 씬 검사
- `Tools/StartupLifecycleAudit.cs`: 실제 실행 반복, 상태 비교, 종료 후 검사
- `Tools/PostNightAudit.cs`: 기존 실제 제작·배치 검증 도구 재사용
- `Logs/StartupWarnings`: 작업 전 백업, 상세 Console 원자료, 씬 저장 차이와 타일 비교, 건설 결과

사용한 절차 문서: Unity CLI `playmode-verification-loop.md`, Pipeline `commands/scripts.md`, `commands/editor-lifecycle-and-observability.md`, `commands/runtime.md`.
