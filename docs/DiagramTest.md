# 다이어그램 테스트 문서

## 트리 구조

```
MdViewer/
├── App.xaml
├── App.xaml.cs
├── MainWindow.xaml
├── MainWindow.xaml.cs
├── MdViewer.csproj
├── Services/
│   ├── FileAssociationService.cs
│   ├── PipeService.cs
│   ├── SettingsService.cs
│   └── WindowManager.cs
├── Models/
│   └── AppSettings.cs
└── docs/
    ├── MdViewer.md
    ├── TaskPlan.md
    ├── HeadingTest.md
    ├── LinkTest.md
    ├── TableTest.md
    └── DiagramTest.md
```

## 화살표 흐름도

```
[User] --> [md file double click] --> [MdViewer.exe]
                                            |
                                            v
                                   [Check existing instance]
                                       |           |
                                     exist       none
                                       |           |
                                       v           v
                                 [Send Pipe]  [Open new window]
                                       |           |
                                       v           v
                                 [Focus window] [Render file]
```

## 수평 흐름

```
Start -----> Process1 -----> Process2 -----> Process3 -----> End
```

## 분기 다이어그램

```
              +-------------+
              |    Start    |
              +------+------+
                     |
                     v
              +-------------+
              |   Check     |
              +------+------+
                     |
         +-----------+-----------+
         |                       |
         v                       v
   +----------+            +----------+
   |   Yes    |            |    No    |
   +----+-----+            +----+-----+
        |                       |
        v                       v
   +----------+            +----------+
   | Process A|            | Process B|
   +----+-----+            +----+-----+
        |                       |
        +-----------+-----------+
                    |
                    v
              +----------+
              |   End    |
              +----------+
```

## 아키텍처 다이어그램

```
+-----------------------------------------------------------+
|                        MdViewer                           |
+-----------------------------------------------------------+
|  +-----------+  +-----------+  +-----------+              |
|  |MainWindow |  |    App    |  |  Models   |              |
|  |  (View)   |  |  (Entry)  |  |AppSettings|              |
|  +-----+-----+  +-----+-----+  +-----------+              |
|        |              |                                   |
|        v              v                                   |
|  +---------------------------------------------------+   |
|  |                    Services                        |   |
|  +-----------+-----------+-----------+---------------+   |
|  |WindowMgr  |PipeService|FileAssoc  |SettingsSvc    |   |
|  +-----------+-----------+-----------+---------------+   |
+-----------------------------------------------------------+
```

## 상태 다이어그램

```
    +--------+
    |  Init  |
    +---+----+
        |
        v
    +--------+   Open File    +----------+
    |  Idle  | -------------> | Rendered |
    +---+----+                +----+-----+
        ^                          |
        |       F5 Reload          |
        +--------------------------+
        |
        |  Esc / Close
        v
    +--------+
    |  Exit  |
    +--------+
```

## 박스 문자

```
+-------------------------------+
|       Single Line Box         |
+-------------------------------+
|  Content here                 |
+-------------------------------+
```

## 화살표 종류

```
-> <- ^  v
--> <--
---> <---
|
v
```
