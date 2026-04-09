---
name: Hub导航与模型扩展
overview: 在保持单一 Hub 场景（DouGong）与 Addressables 按键加载模型根预制体的前提下，修复场景/面板导航 Bug，将拼装入口与返回统一到主菜单，并把 ModelInteraction 与硬编码子物体名解耦以便横向加模型。
todos:
  - id: main-singleton
    content: Main.Awake：单例去重、DontDestroyOnLoad 与 _instance 在 Awake 完成
    status: pending
  - id: jigsaw-return-menu
    content: JigsawPanel Return：Hub 加载后 InitCanvas + Show MainPanel，移除 LoadModel
    status: pending
  - id: jigsaw-from-mainpanel
    content: MainPanel（或目录）：拼装入口 BeginExperience + LoadSceneAsync(jigsaw)
    status: pending
  - id: remove-interact-jigsaw
    content: InteractPanel：移除/隐藏 Jigsaw 进入拼装
    status: pending
  - id: decouple-parts-root
    content: ModelInteraction：用 SerializeField 与/或 ExperienceModelEntry.partsRootName 替代 dougong_test 硬编码
    status: pending
isProject: false
---

# Hub 导航与模型扩展

## 架构前提

- **同一 Hub 场景**加载不同体验：通过 **Addressables 键** 实例化模型根预制体，不强制为每个模型单独建 Hub 场景。
- **元数据**集中在 `[ExperienceModelCatalog](Assets/Resources/ExperienceModelCatalog.asset)` / `[ExperienceModelEntry](Assets/Scripts/Experience/ExperienceModelEntry.cs)`；会话用 `[ExperienceSession](Assets/Scripts/Experience/ExperienceSession.cs)`。

## 1. 导航与 Bug 修复


| 项           | 做法                                                                                                                                                                                                              |
| ----------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Main 单例** | 在 `[Main.cs](Assets/Scripts/Main.cs)` 的 `Awake` 中：若已存在 `_instance != this` 则 `Destroy(gameObject)`；将 `_instance` 赋值与 `DontDestroyOnLoad` 放在 `Awake`，避免二次加载 Hub 时出现 **两个 Main**（一个 DDOL、一个场景内），导致 `Start` 再弹主菜单。 |
| **拼装返回**    | `[JigsawPanel](Assets/Scripts/UI/JigsawPanel.cs)` `Return`：加载 `ExperienceSession.HubSceneName` 后 **仅** `InitCanvas` + `ShowPanel<MainPanel>`，**去掉**回调里的 `LoadModel`。                                            |
| **拼装入口**    | 仅从 **主菜单** 进入：在 `[MainPanel](Assets/Scripts/UI/MainPanel.cs)`（或目录驱动 UI）为条目增加「拼装」→ `ExperienceSession.BeginExperience(entry, key)` → `LoadSceneAsync(entry.jigsawSceneName)`。                                    |
| **Hub 内**   | `[InteractPanel](Assets/Scripts/UI/InteractPanel.cs)` 移除或隐藏 **Jigsaw** 分支，避免在已加载模型流程中再进拼装。                                                                                                                      |


## 2. 解耦 `dougong_test`（模型自由扩展）

- **问题**：`[ModelInteraction.cs](Assets/Scripts/ModelInteraction/ModelInteraction.cs)` 中 `transform.Find("dougong_test")` 写死，新模型结构不同即失效。
- **做法（二选一或组合，优先 KISS）**：
  - **A**：在 `ModelInteraction` 上增加 **SerializeField** `Transform partsRoot`（或 `string visualRootName`），Awake 用该引用/名字解析零件父节点；斗拱预制体在 Inspector 里指定原 `dougong_test` 节点。
  - **B**：在 `[ExperienceModelEntry](Assets/Scripts/Experience/ExperienceModelEntry.cs)` 增加可选字段 `partsRootName`，`BeginExperience` 后由 `ModelInteraction` 读取 `ExperienceSession.ActiveEntry` 解析（无则回退默认名或仅 A 的序列化字段）。

目标：新增模型只需 **新 Addressable 预制体 + 目录一条 + 主菜单按钮（或动态列表）**，不因子物体命名再改脚本。

## 3. 建议自测路径

1. Hub → 选模型 → 隐藏主菜单、出现交互面板。
2. 主菜单 → 拼装 → 返回 → **仅主菜单**，无自动加载模型。
3. 换第二套模型预制体验证 `partsRoot` / 配置生效。

## 依赖与注意

- 场景加载后调用 `UI3DManager.Instance` 侧 `**InitCanvas()`**（见 `[PanelManager](Assets/Scripts/CodeArchitect/Manager/UI/PanelManager.cs)`），避免 Canvas 字典指向已卸载对象。
- `Main._mainCanvas` 若跨场景卸载，返回 Hub 后按需重新赋值或依赖 `InitCanvas` 后的逻辑。

