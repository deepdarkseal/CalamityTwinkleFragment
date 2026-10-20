# DSH Agent 预设：灾厄：吉光片羽

这是 DeepSeek Harness 的**智能体预设（Agent Preset）**，不是上个目录里的项目级 `AGENTS.md`。
它决定「谁在说话」：选择该预设的会话，会以《灾厄：吉光片羽》设定编辑 + tModLoader 开发者的身份工作，并强制使用简体中文。

- 预设 id：`twinkle-fragment`
- 显示名：`灾厄：吉光片羽`
- Loader 行 id：`preset-twinkle-fragment`
- 组合基底：官方 `standard` 预设（工具、压缩、委派、计划模式全套保留），仅替换 persona

---

## 一、文件

| 文件 | 用途 |
|---|---|
| `preset-twinkle-fragment.patch.yml` | 预设声明，可直接追加到 profile 的 `cordis.patch.yml` |
| `validate-preset.js` | 安装前自检（YAML 结构、persona 关键词、插件行完整性） |
| `README.md` | 本文件 |

---

## 二、安装位置

DSH 的预设是 **profile patch 里的一条普通插件行**，由 `@deepseek-ai/dsh-agent-preset` 注册。
本项目已将它写入：

```
%USERPROFILE%\.dsh\profiles\desktop\cordis.patch.yml
```

原文件备份：

```
%USERPROFILE%\.dsh\profiles\desktop\cordis.patch.yml.bak-20261019-2135
```

---

## 三、重装 / 迁移

1. 打开 `%USERPROFILE%\.dsh\profiles\desktop\cordis.patch.yml`。
2. 把 `preset-twinkle-fragment.patch.yml` 的内容（顶层 `- insert:` 条目）追加到文件末尾。
3. 保存。HMR 会监听 profile patch 的变更并重新组合（约 2 秒写入稳定窗口 + 防抖）。
4. 若未生效，重启 DSH。

> 注意：不要重复插入同一个 `id`。若已存在 `preset-twinkle-fragment`，先删除旧条目。

---

## 四、验证

1. 刷新页面（或重新打开设置）。
2. 进入 **设置 → 智能体预设**，应看到「灾厄：吉光片羽」卡片，可查看其组合、设为默认。
3. 或在新任务的选择器里直接选它。
4. 选中后新建会话，问一句「你现在是谁」，回答应使用简体中文并说明自己是该模组的设定/代码协作代理。

若卡片显示为「损坏 / 无法加载」，说明某个子插件在该版本不存在：按卡片里的诊断信息删掉对应插件行即可。

---

## 五、卸载与回滚

- 卸载：从 `cordis.patch.yml` 删除 `- insert:` 到文件末尾的整段（或删除 `preset-twinkle-fragment` 这一行及其 `plugins`）。
- 回滚：用备份覆盖 `cordis.patch.yml`。
- 删除后同样依赖 HMR；未生效则重启。

---

## 六、结构说明

预设的 `plugins` 共 22 行，全部来自官方 `standard` 预设，仅第一行 `persona` 的 `prefix` 被替换为中文项目人格：

| 组 | 内容 |
|---|---|
| 身份 | `persona`（本项目人格）、`agent-instructions`（读取仓库 AGENTS.md）、`time-context` |
| 文件与命令 | `tool-pwsh`（Windows）/ `tool-bash`（非 Windows）、`tool-fs`、`tool-fs-search`、`tool-jobs` |
| 技能与目标 | `skill-filesystem`、`tool-skill`、`command-goal`、`tool-goal`、`tool-schedule` |
| 计划模式 | `planning` 组（`plan-mode`） |
| 上下文管理 | `compaction` 组（自动压缩、`/compact`、工具结果裁剪） |
| 委派 | `delegation` 组（`subagent`、`subagent_fork`、`workflow`） |
| 交互 | `tool-ask-user`、`tool-todo`、`tool-web`、`present` |

> `tool-plugin-manager` 与 `tool-plugin-market` 保持禁用，避免在普通创作会话里误装插件。

---

## 七、修改人格

人格文本在 `preset-twinkle-fragment.patch.yml` 的 `persona.config.prefix`。
改完后重新追加到 profile patch 即可；修改只影响**之后创建**的会话，已有会话保留原组合。

---

## 八、与项目内 `AGENTS.md` 的分工

| | DSH 预设 | 仓库 `AGENTS.md` |
|---|---|---|
| 作用范围 | 选中该预设的所有会话 | 在该仓库内工作的所有代理 |
| 内容 | 身份、语言规则、四条铁律、工作流摘要 | 完整设定圣经与开发规范索引 |
| 是否随仓库走 | 否（在 profile 里） | 是（随 git 提交） |

两者叠加使用效果最好：预设负责「你是谁」，`AGENTS.md` + `docs/` 负责「细节在哪」。
