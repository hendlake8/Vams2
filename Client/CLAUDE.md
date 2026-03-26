# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Unity 6 (6000.3.10f1) 기반 2D 게임 클라이언트 프로젝트 ("Vams2").
URP 2D 렌더링 파이프라인, New Input System, TextMesh Pro 사용.

## Unity Editor Commands

```bash
# Unity 에디터에서 테스트 실행 (EditMode)
"C:/Program Files/Unity/Hub/Editor/6000.3.10f1/Editor/Unity.exe" \
  -runTests -batchmode -nographics \
  -projectPath "D:/GitPrjs/Vams2/Client" \
  -testPlatform EditMode \
  -testResults ./TestResults/editmode-results.xml

# Unity 에디터에서 테스트 실행 (PlayMode)
"C:/Program Files/Unity/Hub/Editor/6000.3.10f1/Editor/Unity.exe" \
  -runTests -batchmode -nographics \
  -projectPath "D:/GitPrjs/Vams2/Client" \
  -testPlatform PlayMode \
  -testResults ./TestResults/playmode-results.xml

# 빌드 (Windows standalone)
"C:/Program Files/Unity/Hub/Editor/6000.3.10f1/Editor/Unity.exe" \
  -batchmode -nographics -quit \
  -projectPath "D:/GitPrjs/Vams2/Client" \
  -buildWindows64Player ./Build/Client.exe
```

> Unity Hub 설치 경로가 다를 수 있음. `Unity.exe` 경로 확인 필요.

## Architecture

- **렌더링**: URP 2D Renderer (Assets/Settings/Renderer2D.asset)
- **입력**: New Input System — `Assets/InputSystem_Actions.inputactions`에 Player 액션맵 정의 (Move, Look, Attack, Interact, Crouch, Jump, Previous, Next, Sprint)
- **텍스트**: TextMesh Pro + Noto Serif KR (한국어 지원)
- **2D 패키지**: Sprite, Tilemap, SpriteShape, Animation, Aseprite, PSD Importer

## Key Packages (Packages/manifest.json)

| 패키지 | 용도 |
|--------|------|
| com.unity.render-pipelines.universal (17.3.0) | URP 렌더링 |
| com.unity.inputsystem (1.18.0) | 입력 처리 |
| com.unity.test-framework (1.6.0) | 유닛 테스트 |
| com.unity.2d.tilemap / tilemap.extras | 타일맵 |
| com.unity.2d.animation / sprite / spriteshape | 2D 애니메이션 |
| com.unity.timeline (1.8.10) | 타임라인 |

## Project Settings

- Color Space: Linear
- Default Resolution: 1920×1080
- Solution File: Client.sln (Visual Studio Managed Game workload)
