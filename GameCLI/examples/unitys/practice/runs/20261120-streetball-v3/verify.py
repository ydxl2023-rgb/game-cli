from pathlib import Path
from collections import Counter
import json,hashlib
from PIL import Image
r=Path(__file__).parent;p=r.parent.parent/'client';report=json.loads((r/'result.json').read_text());sha=lambda f:hashlib.sha256(f.read_bytes()).hexdigest()
locked=json.loads((r/'final-locked.json').read_text());assert all(sha(p/k)==h for k,h in locked.items())
for e in report['records']:
 for rel in e['paths']:
  original=(r.parent/'20261120-texture-refresh/originals'/rel) if e['index']<297 else r/'before'/rel
  assert Image.open(p/rel).size==Image.open(original).size,rel
  assert (sha(p/rel)==sha(original))==(e['status']=='background-preserved'),rel
assert not (p/'Assets/Scripts/TextureVisualCheck.cs').exists()
counts=Counter(e.get('method') for e in report['records'] if e['status']=='replaced')
verification={'dimensions':'all 755 texture files match originals','backgrounds':'10 unchanged','nonBackgrounds':'745 updated','lockedFilesUnchanged':len(locked),'prefabsUnchanged':44,'temporaryHelperRemoved':True,'methods':dict(counts),'runtimeScreenshots':['after-login.png','after-home.png','after-shop.png','after-mail.png','after-settings.png'],'concurrentChangePreserved':'Assets/Scripts/PracticePlugin.cs changed externally during validation; not overwritten'}
(r/'verification.json').write_text(json.dumps(verification,indent=2))
(r/'README.md').write_text('''# 全量 UI 街头篮球贴图版本

覆盖 44 个 UI 预制体及其跨目录 PNG 引用。300 种独立贴图中，290 种非背景贴图（745 个实际文件）更新；10 张大背景及场景层逐字节保留。所有 755 个文件的像素尺寸与原图相同。

90 种主要图标、标题、角色和物件单独调用 imagegen 生成；其余复用已生成的街头篮球素材，或通过原透明度及明暗细节合成生成材质。不是 745 张独立生图。小图标、文字、半透明阴影和切片使用原形状以保持识别和布局。

44 个 prefab、动画、meta/GUID 保持原文件。1360 个锁定文件哈希一致。检查过程中 PracticePlugin.cs 被其他工作修改，已保留，未覆盖；不将其计为本次贴图修改。

已导入 Unity，查看登录、主页、商店、邮件、设置的原生 1920×1080 运行截图。Console 错误为 0。临时运行态检查脚本已删除。其余预制体通过引用、尺寸、哈希和贴图联系表检查，未逐个执行完整业务流程。

- coverage.md：逐预制体覆盖清单
- result.json：逐贴图路径、方法及覆盖数据
- verification.json：最终校验结果
- generated/：独立生图原图及提示词
- textures/：适配原尺寸后的贴图
- before/：本次替换前备份
- contact-00.png 至 contact-06.png：前 297 种贴图预览；公共目录补齐的 3 张另见 textures/297.png 至 299.png

build.py 是基础 297 种贴图的制作过程脚本，supplement.py 记录公共目录补齐过程；保留用于审计，不应直接重复运行已完成的流程。最终结果以 result.json 和 verification.json 为准。
''',encoding='utf8')
print(json.dumps(verification))
