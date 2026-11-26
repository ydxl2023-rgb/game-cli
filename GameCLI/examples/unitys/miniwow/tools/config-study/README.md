# 原作配置研究工具

只处理已有本地配置/反编译资料，不连接游戏服务器，不写 client。先按原有资源与代码恢复流程准备 extracted-resources 与 research/code-study/managed/decompiled。

在 miniwow 根目录、PowerShell 7 中执行：

```powershell
dotnet run --project tools/config-study/Decoder.csproj -- .
python -X utf8 tools/config-study/index_config.py
python -X utf8 tools/config-study/query_config.py --table tbheroskill --id 1250101
python -X utf8 tools/config-study/query_config.py --table tbbuilding --search 旅店
```

Decoder 编译研究目录中原作 Zombie.Table* 生成代码，不改写其构造/字段顺序。LubanReader 是本地最小读取适配，不是原版 Luban；包含变长 int/long、UTF-8、bool、little-endian float。Unity Vector2/3 只是满足未调用转换操作符的研究类型，不加载 Unity 或运行游戏逻辑。BeanBase/StringUtil/SerializationException 是兼容支架；不据此宣称完整原作工程可编译。

每表从 ConfigHead 指定范围切片，实例化原表读取器，要求完全消耗字节；原生成 Tables 构造及 ResolveRef 额外执行。ResolveRef 不抛异常不代表每个引用存在：原读取器常使用 GetOrDefault，所以另外输出有限范围的引用检查。

结果写 research/config-study：全表 JSON、inventory/schema、来源哈希、代码消费位置、显式引用检查、字段文档和 SQLite。JSON 保留 `$type` 多态类型、枚举数值/名称；不展开 `_Ref` 对象，防止递归与重复数据。SQLite 是可重建的本地研究库，不修改共享 PostgreSQL 的资源 schema。

`index_config.py` 会重建该专用 SQLite 的五张 config_* 表，不保存人工注释；人工分析留在研究 Markdown 中。166 表目录分类只是导航，引用缺失尤其候选关系不自动判为原作 bug。

阅读结论从 [配置分析](../../research/config-study/README.md) 开始。原二进制/反编译输入哈希在 provenance.json，所有字段字典由脚本生成，不手工修改生成文件。
# 章节体验索引

完成原配置解码后，在 miniwow 根目录运行：

```powershell
python -X utf8 tools/config-study/index_experience.py
```

输出 `research/config-study/CHAPTER-EXPERIENCE.md` 和 `experience-index.json`。
保留章节数组顺序，关联任务、奖励语言、任务导航的成功/失败分支以及功能开放完整条件；检查缺失任务及遍历到的导航节点，并记录输入 SHA-256。人工阅读结论见 `research/config-study/PLAYER-JOURNEY.md`。输出不是线上试玩记录，也不写入 Unity。

