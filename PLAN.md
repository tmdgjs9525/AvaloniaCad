# AvaloniaCad 기획안

> 마지막 갱신: 2026-11-09 — 코드 현황 기준으로 4~7, 9, 12장 갱신

## 1. 프로젝트 개요

**AvaloniaCad**는 Avalonia와 SkiaSharp로 만드는 크로스플랫폼 2D 정밀 도면 편집기다. 사용자는 격자와 스냅 기능을 이용해 선, 사각형, 원, 폴리라인을 그리고 편집하며, 문서를 저장하거나 이미지로 내보낼 수 있다.

이 프로젝트의 목표는 AutoCAD를 재현하는 것이 아니라, CAD/벡터 편집기의 핵심인 좌표계, 기하 도형, 히트 테스트, 스냅, 명령 이력, 파일 직렬화를 직접 설계·구현하는 것이다.

### 포트폴리오 한 줄 소개

> Avalonia와 SkiaSharp 기반의 크로스플랫폼 2D 정밀 도면 편집기. 벡터 렌더링, 스냅, 선택/편집, Undo/Redo 및 문서 직렬화를 구현한다.

## 2. 목표와 비목표

### MVP 목표

- Windows, macOS, Linux에서 실행 가능한 Avalonia 데스크톱 앱
- 무한 캔버스처럼 동작하는 Pan/Zoom 뷰포트 및 월드 좌표계
- Line, Rectangle, Circle, Polyline 생성
- 객체 선택, 이동, 복제, 삭제
- Grid, Endpoint, Midpoint Snap
- Layer 관리와 객체 속성 편집
- Undo/Redo 및 JSON 저장·불러오기
- PNG 내보내기

### 이번 범위에서 제외

- DWG 완전 호환 및 상용 CAD 파일 편집
- 치수선, 해치, 블록, XRef
- 회전/스케일 Gizmo
- 협업, 클라우드 동기화
- 3D 모델링 및 Extrude

DXF/SVG 내보내기와 3D Extrude 미리보기는 MVP 이후 확장 항목이다.

## 3. 사용자 시나리오

1. 사용자는 새 도면을 연다. 기본 단위는 mm이며 격자가 표시된다.
2. Line, Rectangle, Circle, Polyline 도구로 도형을 작성한다.
3. 커서는 Grid/Endpoint/Midpoint 후보에 붙고, 상태 표시줄은 좌표와 활성 스냅을 알려 준다.
4. Select 도구로 객체를 선택해 이동, 복제 또는 삭제한다.
5. Properties 패널에서 선택 객체의 색상, 선 두께, 레이어, 수치를 확인·수정한다.
6. Ctrl+Z/Ctrl+Y로 편집 이력을 되돌리거나 다시 적용한다.
7. 도면을 `.avaloniacad.json`으로 저장하고 PNG로 내보낸다.

## 4. 화면 구성

```text
┌ Menu: File / Edit / View / Help  (항목만 있음, 명령 미연결) ─────────────┐
│ Toolbar: [선택 | 이동] ‖ [선 | 사각형 | 원 | 폴리라인] ‖ [스냅]          │
├───────────────┬─────────────────────────────────────┬───────────────────┤
│ 레이어   + −  │                                     │ 속성              │
│ ───────────   │            DrawingCanvas            │ ───────────────   │
│ ☑ Default 잠금│   Grid / 축 / 도형 / hover 강조      │ 종류 / 레이어     │
│ ☑ Layer 1 잠금│   미리보기 / 선택 강조 / 그립        │ 좌표·크기 (편집)  │
│ 이름: [     ] │   스냅 마커                          │ 삭제 (Del)        │
├───────────────┴─────────────────────────────────────┴───────────────────┤
│ X | Y | Zoom ‖ 도구: 선 | Snap: ON ‖ 선택: 원 (Default)     도구 사용법 │
└─────────────────────────────────────────────────────────────────────────┘
```

- 좌측 220px 레이어 패널, 우측 280px 속성 패널, 하단 상태 표시줄
- 단축키: V 선택, H 이동, L 선, R 사각형, C 원, P 폴리라인, F3 스냅, Del 삭제, Esc 취소/선택 해제
  (EditorView.OnKeyDown에서 처리, TextBox 입력 중에는 무시)
- 마우스: 휠 줌, 휠 클릭 드래그 Pan, 우클릭 폴리라인 완료/도구 취소

## 5. 기능 명세

상태: ✅ 완료 · 🟡 일부 · ⬜ 미착수

| 영역 | MVP 기능 | 상태 | 현재 구현 / 남은 것 |
| --- | --- | --- | --- |
| Viewport | Pan, wheel zoom, Fit to content, Grid | 🟡 | Pan(휠 클릭, Pan 도구), 마우스 기준 줌, 줌 연동 그리드 완료 / Fit to Content 남음 |
| Draw | Line, Rectangle, Circle, Polyline | ✅ | 클릭 방식 작성 + 점선 미리보기, 크기 0 도형 방지 |
| Select | 단일 선택, hit test, marquee는 후순위 | 🟡 | 단일 선택, 선택 강조, hover 강조 / 박스(marquee)·다중 선택 남음 |
| Edit | Move, Copy, Delete | 🟡 | 그립 편집, 속성 패널 수치 편집, Delete 완료 / 몸통 드래그 Move, Copy 남음 |
| Snap | Grid, Endpoint, Midpoint | 🟡 | Grid 스냅 + 마커 완료 / Endpoint, Midpoint 남음 |
| Layers | 생성, 이름 변경, 표시/잠금 | ✅ | 추가/삭제/이름 변경/표시/잠금, 현재 레이어, 도형 레이어 변경 |
| Properties | 수치, 레이어, 색상, 선 두께 | 🟡 | 좌표·크기·레이어 편집 / 색상·선 두께(스타일 모델) 남음 |
| History | Undo, Redo | ⬜ | Command 구조 미도입 (편집이 도형을 직접 변경) |
| Files | New, Open, Save, Save As | ⬜ | 메뉴 항목만 있음 |
| Export | PNG | ⬜ | 메뉴 항목만 있음 |

## 6. 기술 스택

| 계층 | 선택 | 버전 | 역할 |
| --- | --- | --- | --- |
| 런타임 | .NET | net10.0 | 전 프로젝트 공통 |
| UI | Avalonia (+ Themes.Fluent) | 12.1.4 | 창, 메뉴, 패널, 입력 |
| 렌더링 | SkiaSharp | 4.156.0-preview | 캔버스 벡터 렌더링 (SKPath 대신 SKPathBuilder 사용) |
| Avalonia 연동 | `ICustomDrawOperation` + `ISkiaSharpApiLeaseFeature` | | 렌더 패스에서 `SKCanvas` 접근 |
| MVVM | CommunityToolkit.Mvvm | 8.4.2 | ObservableProperty, RelayCommand |
| 화면 구성 | ViewManager (자체) | | DI 기반 내비게이션, 리전, 다이얼로그 |
| 수학 | System.Numerics | | `Vector2` 좌표 계산 |
| 저장 | System.Text.Json | | (예정) 문서 JSON 직렬화 |
| 테스트 | xUnit v3 | | 좌표 변환 단위 테스트 |

캔버스만 SkiaSharp로 직접 그리고, 도형 모델·도구·히트 테스트·렌더러는 UI와 분리된 `KoDrawing.Core`에 둔다.

## 7. 아키텍처

```text
AvaloniaCad.sln
├─ AvaloniaCad              앱 진입점, MainWindow, DI 등록 (App.axaml.cs)
├─ AvaloniaCad.Core         ViewModelBase, RegionNames (앱 공통)
├─ AvaloniaCad.Editor       에디터 화면
│  ├─ Views/EditorView                 레이아웃 + 단축키 처리
│  ├─ Controls/DrawingCanvas           입력, 도구 생성(RebuildTool), 스냅, 커서
│  ├─ Controls/EditorToolbar           도구 버튼(ToolButtonTheme), 메뉴
│  ├─ Controls/LayersPanel, PropertiesPanel
│  ├─ ViewModels/EditorViewModel       문서, 현재 도구, 선택, 레이어/속성 목록
│  ├─ ViewModels/LayerItemViewModel, PropertyFieldViewModel
│  └─ Rendering/SkiaDrawOperation      CadScene을 SKCanvas에 그림
├─ KoDrawing.Core           UI 비의존 드로잉 코어
│  ├─ CadDocument, Layer, ViewportTransform
│  ├─ Entities/   Entity, Line/Rectangle/Circle/PolylineEntity
│  ├─ Tools/      ITool, SelectTool, Line/Rectangle/Circle/PolylineTool, NullTool, ToolKind
│  ├─ HitTesting/ EntityHitTest, EntityGrips, Grip
│  └─ Rendering/  CadScene, EntityRenderer, GridRenderer
├─ KoDrawing.Tests          xUnit v3 (ViewportTransformTests)
└─ ViewManager              Avalonia.Container, Container.Core
```

### 핵심 도메인 모델 (현재)

```text
CadDocument
 ├─ Entities[]
 ├─ Layers[]        기본 레이어 "Default", 마지막 레이어는 삭제 불가
 ├─ CurrentLayer    새 도형이 들어갈 레이어
 └─ Changed 이벤트  Add/Remove/레이어 변경, NotifyChanged()로 직접 알림

Entity (abstract): Id, LayerId
 ├─ LineEntity(Start, End)
 ├─ RectangleEntity(Corner1, Corner2)
 ├─ CircleEntity(Center, Radius)
 └─ PolylineEntity(Points[])

Layer: Id, Name, IsVisible, IsLocked
```

예정: Style(색상, 선 두께), Units, ViewSettings, PolylineEntity.IsClosed

### 데이터 흐름

- 입력: `DrawingCanvas` → 화면 좌표를 월드 좌표로 변환(필요 시 스냅) → `ITool`
- 도형 생성: 도구가 commit 콜백으로 `CadDocument.Add` 호출
- 선택: `SelectTool` ↔ `DrawingCanvas.SelectedEntity` ↔ `EditorViewModel.SelectedEntity` (양방향 동기화)
- 렌더링: `DrawingCanvas.Render` → `CadScene` 스냅샷 생성(뷰포트 복제, 표시 레이어 도형 배열) → `SkiaDrawOperation`
- 그리기 순서: 그리드 → 축 → 도형 → hover → 미리보기 → 선택 강조 → 그립 → 스냅 마커

### 설계 원칙

- 엔터티 좌표는 항상 월드 좌표(mm)로 보관한다.
- 화면 좌표 변환과 역변환은 `ViewportTransform` 한 곳에서 처리한다.
- 화면 렌더링은 모델을 변경하지 않는다.
- 사용자 변경은 `ICadCommand.Execute/Undo`로 표현해 이력을 보장한다. ⬜ 미적용
- 선택, 스냅, 도구 상태는 문서 데이터와 분리된 편집기 상태로 둔다.

### 알려진 기술 부채

- 그립 드래그, 속성 패널, 레이어 이동이 도형을 직접 수정함 → Undo/Redo 도입 시 Command로 전환
- `CadScene` 스냅샷이 얕은 복사라 렌더 스레드와 UI 스레드가 같은 Entity 객체를 공유함
- `KoDrawing.Core`가 쓰지 않는 Avalonia 패키지를 참조, SkiaSharp 프리뷰 버전 고정
- 테스트가 `ViewportTransform`뿐. `EntityHitTest`/`EntityGrips`는 internal이라 `InternalsVisibleTo` 필요
- 원 반지름 그립, 사각형 나머지 두 코너 그립 없음
- 메뉴(File/Edit/View) 항목에 명령 미연결

## 8. 주요 알고리즘

### 좌표 변환

- `screen = (world - cameraPosition) * zoom + viewportCenter`
- `world = (screen - viewportCenter) / zoom + cameraPosition`
- 휠 줌 전후 마우스 아래의 월드 좌표가 같도록 카메라 위치를 보정한다.

### Hit Test

- Line: 점과 선분 사이 거리를 픽셀 허용 오차로 비교
- Circle: 중심과 클릭점 거리와 반지름의 차이를 비교
- Rectangle: 네 변에 대한 선분 hit test
- Polyline: 각 선분을 순회해 가장 가까운 항목을 선택

### Snap 우선순위 (현재는 3. Grid만 구현)

1. Endpoint
2. Midpoint
3. Grid
4. 현재 커서의 원본 월드 좌표

스냅 허용 거리는 화면 기준 픽셀 값으로 유지해 줌 레벨에 관계없이 동일한 조작감을 제공한다.

## 9. 개발 마일스톤

### M1. Canvas Foundation ✅

- [x] `KoDrawing.Core`(net10.0), `KoDrawing.Tests`(xUnit v3) 구성
- [x] `ViewportTransform` 구현 및 테스트
- [x] Avalonia 레이아웃과 도구 선택 UI
- [x] SkiaSharp 캔버스 연결 (`ICustomDrawOperation`)
- [x] Pan, Zoom, World/Screen 변환
- [x] Grid 및 상태 표시줄
- [ ] Fit to Content → M4로 이동

### M2. Drawing & Rendering 🟡

- [x] 도메인 모델 (Entity, CadDocument)
- [ ] 스타일 정의 (색상, 선 두께)
- [x] Line, Rectangle, Circle 렌더링·생성
- [x] Polyline 생성과 미리보기
- [ ] JSON 저장 → M4 Open/Save와 함께 진행

### M3. Editing Precision 🟡

- [x] Hit test와 선택 강조, hover 강조
- [x] 그립 편집 (스냅 적용)
- [x] Delete
- [ ] Move(몸통 드래그), Copy
- [x] Grid Snap + 스냅 마커
- [ ] Endpoint/Midpoint Snap
- [x] 레이어 패널 (추가/삭제/이름/표시/잠금)
- [x] 속성 패널 (좌표·크기·레이어 편집)

### M4. Product Quality ⬜

- [ ] Command 기반 Undo/Redo
- [ ] New/Open/Save/Save As
- [ ] PNG Export
- [ ] Fit to Content
- [x] 단축키 (도구, F3, Del, Esc)
- [ ] 메뉴 명령 연결, 오류 메시지

### M5. Portfolio Polish ⬜

- [ ] 기하 알고리즘 및 직렬화 단위 테스트
- [ ] 성능 측정: 1,000개 이상 엔터티 Pan/Zoom
- [ ] 데모 GIF와 스크린샷
- [ ] README에 구조도, 기능표, 향후 계획 작성

## 10. MVP 완료 기준

- 1,000개 도형이 있는 문서에서 Pan/Zoom이 조작 가능한 수준으로 동작한다.
- 주요 네 가지 엔터티를 작성, 선택, 이동, 복제, 삭제할 수 있다.
- Grid/Endpoint/Midpoint Snap이 모든 작성·이동 도구에서 동작한다.
- Undo/Redo가 연속 명령에서 일관되게 작동한다.
- 저장 후 재시작·재열기에도 문서의 핵심 데이터가 보존된다.
- PNG 결과물이 화면 렌더링과 동일한 도형 배치·색상을 보인다.

## 11. 이후 확장 아이디어

- SVG 및 DXF import/export
- 치수선, 텍스트, 해치, 호(Arc)
- 회전·스케일 및 다중 선택
- 공간 인덱스(R-tree)로 대용량 히트 테스트 최적화
- 폐곡선 Extrude와 3D 미리보기
- 플러그인 도구 API 및 사용자 설정 단축키



## 12. 현재 작업 순서

M1 완료, M2/M3 대부분 완료. 다음 순서로 진행한다.

1. **Undo/Redo** — `ICadCommand`(Execute/Undo) + 이력 스택 도입, 기존 편집(그리기, 삭제, 그립, 속성 입력, 레이어 이동)을 Command로 전환
2. **Move / Copy** — 선택 도형 몸통 드래그 이동, 복제 (처음부터 Command로 구현)
3. **Endpoint / Midpoint 스냅** — 8장 우선순위대로, 상태 표시줄에 활성 스냅 종류 표시
4. **저장 / 열기** — JSON 직렬화(도형 + 레이어), New/Open/Save/Save As 메뉴 연결
5. **Fit to Content, PNG Export**
6. **스타일** — 색상, 선 두께 모델 + 속성 패널
7. **테스트 보강** — HitTest, Grips, Snap, Command 이력, 직렬화
