# 古建筑斗拱 XR（Traditional Wooden Building XR）

基于 **Meta Quest 3** 的古建筑斗拱模型交互与知识问答应用，支持手势/手柄控制模型缩放、移动、拆解组合、虚拟拼装，以及 AI 智能问答。

---

## 技术栈

| 项 | 说明 |
|----|------|
| 引擎 | Unity **2022.3.55f1**（LTS） |
| 渲染管线 | URP |
| XR 平台 | Meta Quest 3 |
| XR SDK | Meta XR SDK 72.0.0 / Oculus XR 4.2.0 / XR Management 4.5.0 |
| 资源管理 | Addressables 1.22.3 |
| 序列化 | Newtonsoft.Json |

---

## 场景说明

| 场景 | 用途 |
|------|------|
| `DouGong.unity` | 斗拱模型展示与交互（缩放、拆解、AI 问答） |
| `DouGongJigsaw.unity` | 斗拱虚拟拼装 |
| `OverallModel.unity` | 整体模型展示 |
| `zhuanjiao.unity` | 转角模型展示 |
| `MetaUI/` | Meta 手部追踪 UI 示例场景 |

---

## 项目结构

```
Assets/
├── AddressableAssetsData/   # Addressables 配置
├── Audio/                   # 音效（Meta SDK 交互音效）
├── Editor/                  # 编辑器工具脚本
├── HandShapes/              # 手部形状预设
├── Material/                # 材质文件（.mat）
├── Models/                  # 3D 模型资源
├── Oculus/                  # Oculus 集成
├── Plugins/                 # 插件（Newtonsoft.Json 等）
├── Prefabs/                 # 预制体
├── QuickOutline/            # 轮廓高亮效果
├── Resources/               # 运行时资源（llm_config.txt）
├── Scenes/                  # 场景文件
├── Scripts/
│   ├── CodeArchitect/       # 基础架构（单例、事件、UI、资源加载等）
│   ├── Data/                # 数据模型
│   ├── Experience/          # 体验模式管理（模型目录、会话、菜单）
│   ├── LLM/                 # LLM 对话（LLMChat + LlmEnv）
│   ├── ModelInteraction/    # 模型交互（缩放、拆解、拼装、材质选择）
│   ├── Tools/               # 工具类（iTween 动画、Collider 可视化等）
│   ├── UI/                  # UI 面板（主菜单、对话、拼装等）
│   └── Main.cs              # 主入口（单例，场景管理）
├── Settings/                # URP 设置
├── StreamingAssets/         # 外部文件（.env、child_names.json）
├── Textures/                # 贴图与图标
├── TextMesh Pro/            # TMP 字体资源
└── XR/                      # XR 交互配置
```

---

## 环境配置

### 1. 配置 LLM（大语言模型）

复制 `.env.example` 为项目根目录 `.env`，填写对应参数：

```env
LLM_API_URL=https://api.deepseek.com/v1/chat/completions
LLM_API_KEY=sk-xxxxxxxxxxxxxxxx
LLM_MODEL=deepseek-chat
```

> 支持 OpenAI 兼容接口，URL 写到 `/v1` 即可，运行时自动补全 `/chat/completions`。

或者将 `Assets/Resources/llm_config.txt` 配置为与 `.env` 相同格式，跨平台（含 APK 包内）均可读取。

### 2. 配置 RAG（知识库检索，可选）

```env
RAG_API_URL=http://127.0.0.1:8080/ar/chat
```

> 若配置了 RAG_API_URL，问答时优先使用 RAG 服务；若失败则自动回退到上方配置的 LLM。
> Quest 3 独立运行时，需将 `RAG_API_URL` 改为局域网 IP。

### 3. 敏感文件安全

- `.env`、`RuntimeActionBindings.json`、`Assets/Resources/llm_config.txt` 已在 `.gitignore` 中排除，不会被提交。
- `.env.example` 是模板文件，不含真实密钥，可以提交。

---

## 构建与部署

1. 确认 `Build Settings` 中已添加目标场景：`DouGong`、`DouGongJigsaw`
2. 切换平台为 **Android**，目标设备为 **Meta Quest 3**
3. 确认 Addressables 资源已构建（`Window > Asset Management > Addressables > Groups > Build > New Build > Default Build Script`）
4. 构建 APK 或直接通过 ADB 部署到 Quest 3

---

## 功能说明

### 模型交互
- **手势/手柄操作**：缩放、旋转、移动模型
- **拆解与组合**：分离斗拱各部件，逐层查看结构
- **虚拟拼装**：`DouGongJigsaw` 场景中按步骤拼装斗拱
- **材质切换**：切换模型材质样式

### AI 问答
- 在模型中点击部件，触发知识问答
- 支持 RAG（检索增强生成）优先，失败时自动回退 LLM
- 对话面板支持 Markdown 渲染

---

## 注意事项

- 本项目为个人学习项目，架构从简，不追求复杂设计
- 代码中 `Assets/Scripts/CodeArchitect/` 为自研基础框架（单例、事件中心、面板管理、资源加载等），开箱即用
- Meta XR SDK 包版本已锁定，升级时请同步更新 `Packages/manifest.json` 与本 README