# 小骑士冲冲冲资源提取台账

本轮范围是用户提供的 `wxecf0e106809a477d` 本地快照，不代表已取得线上完整版本。没有将提取资源导入干净 Unity 工程，没有执行提取出的代码。

## 查找入口

- 本地检索页：`resource-catalog/index.html`，浏览器直接打开；按名称、系统、类型搜索，可预览 PNG 和打开导出文件。
- PostgreSQL：独立 schema `miniwow_resources`，查询视图 `resource_search`。
- 本地 SQLite：`resource-catalog/catalog.sqlite`，原始清单 `resource-catalog/catalog.json`。
- 命令行：在 miniwow 下运行 `python tools/find_resource.py hotel --limit 20`；可用 `--type FairyGUIItem` 或 `--type ConfigTable` 限定类型。
- 数据库连接及 SQL 查询示例：`../tools/RESOURCE-DATABASE.md`。

## 已完成范围

| 类别 | 数量 | 说明 |
|---|---:|---|
| 原始文件 | 191 | 清单内190个文件全部哈希匹配；另1个为清单自身 |
| Unity Bundle | 175 | 均完成对象登记与解析 |
| 资源记录 | 25,185 | 包含Unity对象、FGUI资源项、配置表等，不等于独立美术文件数 |
| 导出文件 | 49,533 | 包含原始对象备份和可读/可用导出；全部校验SHA256 |
| Texture2D / Sprite | 1,283 / 204 | 其中3个Font Texture未成功转PNG，保留原始对象 |
| FairyGUI包 / 条目 | 25 / 3,880 | 从FGUI v7包内字符串、item、sprite表取得 |
| FairyGUI独立PNG | 3,245 | 精确图集矩形切图；本批rotated均为false |
| Mesh | 825 | OBJ几何；不是完整带骨骼材质的FBX |
| AudioClip | 79 | 提取音频载荷；本批为m4a |
| Material / AnimationClip | 1,518 / 69 | 原始序列化对象及type tree；不是可直接安装的Unity资产 |
| ConfigTable | 166 | 按ConfigHead表名、偏移、长度拆分，未冒称已解码字段 |
| Video | 5 | 登记原始MP4路径，不重复复制 |
| 引用记录 | 61,268 | 包含Unity PPtr及FGUI图集/父项引用；跨文件引用并非全部解析到目标ID |

## 每条记录保存什么

来源文件和SHA256、序列化文件名、Unity PathID、对象类型和名称、container资源路径、所属系统推测、用途说明、证据等级、提取状态、错误、导出路径与哈希。FGUI条目另外包含包名/包ID、itemID、ui://地址、图集引用、矩形、旋转、裁剪偏移与原始尺寸。

`name_inference` 表示按文件名/路径关键词推断，`declared_ui_metadata` 表示包内UI声明，`unknown` 表示未确认。系统关键词标签不是运行行为证明。`annotations` 专门存放人工确认的系统、用途、状态与证据，自动同步不覆盖它；当前没有伪造人工确认记录。

数据库只存元数据，资源二进制留在本地 `resource-catalog/exports`。本地生成目录已加入Git忽略；工具、SQL与本报告保留为可版本管理文件。其他机器需要重新提取或取得导出目录，数据库路径不会自动变成远程下载地址。

## 未完成与异常记录

- 4个 `.wxapkg` 带 `V1MMWX` 加密头，本轮未解密，状态 `encrypted_unparsed`；不声称包内成员已登记。
- 一个 `.wasm.br` 仅1字节，状态 `incomplete`。
- 10个Unity对象仅保留原始序列化数据：1个BuildSettings、6个MonoBehaviour、3个Font Texture；解析器具体错误在每条记录的 `errors` 中。
- FairyGUI `FirstPackage/p7dw1` 的图集矩形越界，状态 `metadata_only`，没有裁出错误图片。
- FairyGUI组件层级、控制器、过渡动画、分支与多分辨率变体尚未重建。PNG是裁剪后的原图块，offset/original_size已记录，但未补回透明画布。
- 材质、预制结构、骨骼动画、粒子与脚本保留原始对象和结构数据；还不是可以直接导入GameCLI的系统模板。
- 英雄/经营/战斗等业务用途，需要后续代码调用关系和实机操作继续确认。

## 可复现步骤

在 miniwow 目录使用 Python 3.12，研究依赖为隔离安装的 UnityPy 1.25.4：

```powershell
python tools/extract_resources.py
python tools/enrich_fairy_catalog.py
python tools/finalize_resource_catalog.py
node tools/resource-db.mjs --check
node tools/resource-db.mjs --sync
```

三个提取步骤分别负责Unity原始对象、FairyGUI映射、配置拆分/合并/哈希校验/SQLite/检索页。数据库同步使用事务和外键校验，保留历史记录与人工标注。本地SQLite和远端表为累计集合；本轮精确范围以本轮catalog.json为准。

FairyGUI格式依据：官方 `FairyGUI-unity/Assets/Scripts/UI/UIPackage.cs` 与 `Utils/ByteBuffer.cs`；工具保留相应来源链接。已抽样查看CharacterIcon人物图块，未逐张人工确认全部图片含义。

## 原始目录归档

新增 `../extracted-resources/`。使用 `python tools/restore_resource_paths.py` 恢复已确认的AssetBundle container路径与FairyGUI包内路径。共49,543条文件映射（含10个原始独立文件），合并相同路径且内容相同的文件后为49,518个资源文件；每条均校验SHA256。

已补齐4,625个Unity对象的container路径；上轮使用的UnityPy指针接口不匹配导致路径漏记，已修正。3个场景包的container含空PathID，无法直接关联根对象，具体错误保留在映射清单；未关联对象仍完整保存于明确标记的回退目录。

`extracted-resources/path-manifest.json`记录原始路径、路径依据、资源ID、旧位置、新位置和哈希。台账exports增加original_layout_path和original_layout_basis，原始导出路径仍保留。无原始路径的子资源放入_subassets或_unmapped，不能视为原项目精确文件树。JSON/原始对象未伪装成可直接导入的prefab。
