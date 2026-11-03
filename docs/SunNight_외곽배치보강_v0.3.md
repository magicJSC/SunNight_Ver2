# 외곽 배치 보강 — 2026-11-02

후속 실제 왕복 시험에서 이동 대비 보상 부족을 확인하여 경비 보상 상자 4곳과 담당 경비 8마리를 외곽으로 옮기고 순찰 4마리를 접근 중간에 배치했다. 최신 배치·수치는 [외곽 파밍 밸런스 검증](SunNight_외곽파밍밸런스검증_v0.3.md)을 따른다. 아래 내용은 최초 외곽 보강 시점의 기록이다.

사용자 선택인 **풍경 보강 + 기존 자원·몬스터 분산**을 적용했다. 중앙 외곽의 비어 있던 북·서·남·동 지역을 채우며 길로 연결하지 않았다.

## 적용

| 지역 | 분위기 | 장식 무리 / 스프라이트 | 이동한 자원 | 이동한 순찰 |
|---|---|---|---|---|
| 북쪽 | 숲, 상자, 야영 흔적 | 65 / 835 | E0, E0_s1 | P08 |
| 서쪽 | 수풀, 폐가구, 버려진 물자 | 56 / 731 | D0_s2, D0_s3 | P12 |
| 남쪽 | 폐자재, 드럼통, 타이어 야적장 | 16 / 208 | D0_s4, D0_s5 | P11 |
| 동쪽 | 산업 잔해, 상자, 드럼통 | 26 / 339 | E0_s2, E0_s3 | P09 |

총 **163개 무리, 장식 2,113개**를 실제 바닥 타일 안쪽에 배치했다. 일정한 격자 대신 불규칙한 위치·간격·크기로 분산했다. 보급품 주위는 비우고 장식에는 새 충돌체를 추가하지 않았다. 중앙 야영지·튜토리얼과 첫날 A/B/C 자원 위치는 유지했다.

필드 보급 총량은 고철 **64**, 정비 부품 **6**, 음식 **3**, 회복약 **5**로 동일하다. 보급 지점 **45곳**, 경비·순찰 **25마리**를 유지한다. 이는 Exploration Content의 수치이며 튜토리얼에서 얻는 별도 자원을 합산한 값이 아니다.

이동한 보급의 화면 표시명은 실제 외곽 지역에 맞췄다. 보급·순찰 ID, 수집 시간, 지급량, 해금 조건, legacySource, 몬스터 종류·순찰 반경은 바꾸지 않았다. 강화 비용도 변경하지 않았다. 이미 받은 보급이나 처치한 순찰을 기존 저장에서 다시 지급하는 로직은 추가하지 않았다.

## 구현과 재생성

- `Assets/Editor/OuterFieldDressing.cs`: 고정 난수 시드의 외곽 배치, 바닥·보급 가림 검사, 기존 콘텐츠의 위치 변경.
- `FirstFieldLevelDesign.Build()`와 `ExplorationContentBuilder.Build()`에도 반영하여 기존 메뉴로 재생성해도 외곽 배치가 사라지지 않게 했다.
- `SunNight/Level Design/Rebuild Outer Field Dressing`으로 외곽만 재생성할 수 있다. TutorialScene을 Edit Mode로 열고 사용한다.
- 결과는 기존 `FirstFieldDressing.prefab`과 `Resources/Exploration/Content.asset`에 저장했다. 현재 사용자 씬을 재로드하거나 씬 저장 API를 호출하지 않았다.
- 작업 전 두 에셋과 기존 생성기는 `Logs/OuterField/before-edit.zip`에 백업했다.

## 검증

- Unity 컴파일 성공. 실제 전체 맵 및 네 지역 확대 렌더를 확인했다.
- 장식은 Transform/SpriteRenderer만 사용하며 바닥 밖 배치와 보급 가림 검사 통과. 신규 충돌체 0개.
- 중앙 야영지 기준 바닥 위 4칸 간격 탐색과 반경 0.7의 구간 충돌 검사: **자원 45곳 전부 접근 가능**. 지정된 길의 좌표만 검사한 결과가 아니다.
- 변경 전후 콘텐츠 비교: 보급 8곳·순찰 4곳의 위치와 해당 보급 표시명만 변경. ID·총량·조건·수집 시간 동일.
- 실제 Play Mode: 외곽 보급 8곳 생성 위치, 순찰 4마리의 기준 위치와 이동 확인.
- 서쪽 보급에서 실제 상호작용 콜백으로 고철 1개 수집 후 반복 시 추가 지급되지 않음을 확인했다. 테스트를 위해 플레이어를 해당 위치로 옮겼고 입력 잠금을 해제했다. 전체 첫날 재플레이나 모든 외곽 보급 수집 시험은 아니다.
- 검증용 플레이는 종료했고 저장 파일을 작성하지 않았다.

이번 Play Mode의 Console에서는 오류 0건과 경고 6건을 관찰했다. 입력 장치 control scheme 페어링 경고 5건, 비동기 리소스 로드 중 Missing Script 1건이다. 이번 외곽 배치 수정으로 이 경고를 해결했다고 주장하지 않으며 원문은 증거 JSON에 보존했다. 성능 프로파일링과 전체 외곽 탐색 시간·보상 효율 재측정은 하지 않았다.

## 화면과 증거

- [변경 전 전체 맵](level-design/v03-outer-before.png) · [변경 후 전체 맵](level-design/v03-outer-after.png)
- [북쪽](level-design/v03-outer-north.png) · [서쪽](level-design/v03-outer-west.png) · [남쪽](level-design/v03-outer-south.png) · [동쪽](level-design/v03-outer-east.png)
- [검증 결과와 이동 좌표](level-design/v03-outer-field-verification.json)
- `Tools/OuterFieldAudit.cs`: 배치·접근성·실제 수집 검증 도구.

캡처는 실제 Unity 씬을 임시 카메라와 일정한 낮 밝기로 렌더한 검토용 화면이다. 사용한 절차: Unity Pipeline `commands/scripts.md`와 `commands/editor-lifecycle-and-observability.md`, `verification-before-completion`.
