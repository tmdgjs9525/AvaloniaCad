# AvaloniaCad 기획안

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
┌ Menu: File / Edit / View / Help ────────────────────────────────────────┐
│ Toolbar: Select | Line | Rect | Circle | Polyline | Pan | Snap          │
├───────────────┬─────────────────────────────────────┬───────────────────┤
│ Layers        │                                     │ Properties        │
│ ───────────   │            Drawing Canvas           │ ───────────────   │
│ ☑ Default     │       Grid / rulers / guides        │ Entity / Layer    │
│ ☑ Annotation  │       entities / preview / select   │ Position / Size   │
│ + Add layer   │                                     │ Stroke / Color    │
├───────────────┴─────────────────────────────────────┴───────────────────┤
│ X: 120.00 mm   Y: 40.00 mm  |  Zoom: 100%  |  Snap: Endpoint           │
└─────────────────────────────────────────────────────────────────────────┘
```

## 5. 기능 명세

| 영역 | MVP 기능 | 완료 기준 |
| --- | --- | --- |
| Viewport | Pan, wheel zoom, Fit to content, Grid | 줌 기준점이 마우스 위치에 고정되고 도형 좌표는 변하지 않는다. |
| Draw | Line, Rectangle, Circle, Polyline | 클릭/드래그 또는 연속 클릭으로 도형이 생성되며 작성 중 미리보기가 보인다. |
| Select | 단일 선택, hit test, marquee는 후순위 | 클릭한 최상위 도형이 선택 강조 상태가 된다. |
| Edit | Move, Copy, Delete | 선택 도형의 변경이 하나의 Undo 가능한 명령이 된다. |
| Snap | Grid, Endpoint, Midpoint | 가장 가까운 유효 후보가 강조되고 정확한 좌표로 배치된다. |
| Layers | 생성, 이름 변경, 표시/잠금 | 잠긴 레이어는 수정할 수 없고 숨긴 레이어는 렌더링하지 않는다. |
| History | Undo, Redo | 도형 생성·삭제·이동·속성 변경을 순서대로 되돌리고 다시 적용한다. |
| Files | New, Open, Save, Save As | 저장 후 다시 열어도 엔터티, 레이어, 스타일, 뷰 설정이 보존된다. |
| Export | PNG | 현재 도면의 지정 영역이 해상도 선택에 따라 이미지로 생성된다. |

## 6. 기술 스택

| 계층 | 선택 | 역할 |
| --- | --- | --- |
| UI | Avalonia | 창, 메뉴, 패널, 입력, 크로스플랫폼 배포 |
| 렌더링 | SkiaSharp | 캔버스의 벡터 도형·텍스트·이미지 렌더링 |
| Avalonia 연동 | `ICustomDrawOperation` | Avalonia 렌더 패스에서 SkiaSharp `SKCanvas` 접근 |
| 상태/UI 구조 | CommunityToolkit.Mvvm | MVVM, 명령, 속성 변경 알림 |
| 수학 | System.Numerics | `Vector2`, 행렬 및 좌표 계산 |
| 저장 | System.Text.Json | 프로젝트 문서의 JSON 직렬화 |
| 테스트 | xUnit | 좌표 변환, 스냅, 히트 테스트, 명령 이력 단위 테스트 |

Avalonia 기본 컨트롤은 메뉴와 패널 UI에 사용한다. 캔버스만 SkiaSharp로 직접 그리며, 도형 모델과 기하 알고리즘은 UI/렌더러에 의존하지 않는 Core 프로젝트에 둔다.

## 7. 아키텍처

```text
AvaloniaCad.App
  ├─ Views / ViewModels / Commands
  └─ CanvasControl
       └─ SkiaRenderer (SkiaSharp)

AvaloniaCad.Core
  ├─ Document, Layer, Entity
  ├─ Geometry: HitTest, Bounds, Snap
  ├─ ViewportTransform
  └─ Commands: Create, Move, Delete, UpdateProperty

AvaloniaCad.Infrastructure
  ├─ JsonDocumentSerializer
  └─ PngExporter

AvaloniaCad.Tests
  └─ Core 단위 테스트
```

### 핵심 도메인 모델

```text
CadDocument
 ├─ Units
 ├─ Layers[]
 ├─ Entities[]
 └─ ViewSettings

CadEntity (abstract)
 ├─ Id, LayerId, Style
 ├─ LineEntity(Start, End)
 ├─ RectangleEntity(Origin, Width, Height)
 ├─ CircleEntity(Center, Radius)
 └─ PolylineEntity(Points[], IsClosed)
```

### 설계 원칙

- 엔터티 좌표는 항상 월드 좌표(mm)로 보관한다.
- 화면 좌표 변환과 역변환은 `ViewportTransform` 한 곳에서 처리한다.
- 화면 렌더링은 모델을 변경하지 않는다.
- 사용자 변경은 `ICadCommand.Execute/Undo`로 표현해 이력을 보장한다.
- 선택, 스냅, 도구 상태는 문서 데이터와 분리된 편집기 상태로 둔다.

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

### Snap 우선순위

1. Endpoint
2. Midpoint
3. Grid
4. 현재 커서의 원본 월드 좌표

스냅 허용 거리는 화면 기준 픽셀 값으로 유지해 줌 레벨에 관계없이 동일한 조작감을 제공한다.

## 9. 개발 마일스톤

### M1. Canvas Foundation

- Avalonia 레이아웃과 도구 선택 UI
- SkiaSharp 캔버스 연결
- Pan, Zoom, World/Screen 변환
- Grid 및 상태 표시줄

### M2. Drawing & Rendering

- 도메인 모델 및 스타일 정의
- Line, Rectangle, Circle 렌더링·생성
- Polyline 생성과 미리보기
- JSON 임시 저장

### M3. Editing Precision

- Hit test와 선택 강조
- Move, Copy, Delete
- Grid/Endpoint/Midpoint Snap
- 레이어와 속성 패널

### M4. Product Quality

- Command 기반 Undo/Redo
- Open/Save/Save As
- PNG Export
- 단축키와 오류 메시지

### M5. Portfolio Polish

- 기하 알고리즘 및 직렬화 단위 테스트
- 성능 측정: 1,000개 이상 엔터티 Pan/Zoom
- 데모 GIF와 스크린샷
- README에 구조도, 기능표, 향후 계획 작성

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

