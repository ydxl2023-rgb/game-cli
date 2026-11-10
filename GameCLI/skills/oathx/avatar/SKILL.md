---
name: avatar
description: 使用 com.oathx.avatar 的角色、特效与子弹组件，维护表现层资源接入和生命周期。
---

# Oathx Avatar

必须遵守 [开发规范](../development-rules/SKILL.md)。工程定位见 [Oathx](../SKILL.md)。

根据任务读取包内 `Runtime/Avatar`、`Runtime/Effect` 或 `Runtime/Bullet`，对照现有 XAvatarActor、XEffectManager、XBulletManager 及业务调用方。这里只整理接入规范，具体行为必须以目标版本源码为准。

- 复用已有角色、特效和子弹组件，先确认资源配置、挂点、动画及生命周期契约，不为单一业务新增平行管理器。
- 固定组件引用按私有序列化字段绑定；运行时生成和回收遵循现有管理器契约，先确认资源归属再释放。
- 表现逻辑与业务实体、战斗规则分离，不能仅根据动画或特效推断伤害、命中或服务端协议。
- 资源加载涉及 XRes 时同时读取 [Bundle](../bundle/SKILL.md)；涉及实体数据时读取 [Entity](../entity/SKILL.md)。

修改后除编译检查外，验证任务涉及的生成、播放、挂接及释放行为；没有运行验证时明确说明。
