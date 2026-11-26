"""Join original chapter order, tasks, guide graphs and unlock conditions.

Run after the configuration decoder. Does not modify the Unity client or source data.
"""
from pathlib import Path
import hashlib
import json

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'research/config-study'
SOURCE = ROOT / 'research/code-study/managed/decompiled/HotUpdate'


def read_table(name):
    return json.loads((OUT / 'tables' / (name + '.json')).read_text(encoding='utf-8'))


def cell(value):
    text = value if isinstance(value, str) else json.dumps(value, ensure_ascii=False)
    return text.replace('|', '\\|').replace('\n', '<br>')


def main():
    names = ['tbchaptertask', 'tbtask', 'tbguidetask', 'tbfeatureunlock',
             'tbitem', 'tblanguage', 'tbconversation']
    data = {name: read_table(name) for name in names}
    tasks = {row['TaskID']: row for row in data['tbtask']}
    guides = {row['ID']: row for row in data['tbguidetask']}
    items = {row['Id']: row for row in data['tbitem']}
    language = {row['ID']: row['SimpleChinese'] for row in data['tblanguage']}
    missing = []

    def graph(start):
        pending = [start] if start > 0 else []
        visited = set()
        result = []
        while pending:
            node = pending.pop(0)
            if node in visited:
                continue
            visited.add(node)
            if node not in guides:
                missing.append({'kind': 'guide', 'start': start, 'id': node})
                continue
            row = guides[node]
            result.append(row)
            pending.extend(row[field] for field in ('NextID', 'FNextID') if row[field] > 0)
        return result

    chapters = []
    lines = ['# 原作章节任务体验索引', '',
             '由 `tools/config-study/index_experience.py` 生成。保留章节 TaskId 数组顺序；不是按任务 ID 排序。', '',
             '目标值直接保留配置口径，可能是累计阈值。开放列仅表示该任务出现在开放条件中，不代表完成后立刻开放；完整条件见 JSON。', '',
             '奖励名称经 tbitem → tblanguage 解析，不能解析时保留原名或 ID。导航节点保留成功/失败分支，不能按列表顺序当作唯一实际路径。', '']
    for chapter in data['tbchaptertask']:
        entries = []
        lines.extend([f"## 第 {chapter['ID']} 章", '',
                      '|顺序 / ID|目标|条件 / 参数 / 目标值|奖励|任务导航入口|相关开放条件|',
                      '|---|---|---|---|---|---|'])
        for order, task_id in enumerate(chapter['TaskId'], 1):
            if task_id not in tasks:
                missing.append({'kind': 'task', 'chapter': chapter['ID'], 'id': task_id})
                continue
            task = tasks[task_id]
            rewards = []
            for reward in task['Reward']:
                name = items.get(reward['Id'], {}).get('Name', str(reward['Id']))
                rewards.append({**reward, 'resolvedName': language.get(name, name)})
            unlocks = [row for row in data['tbfeatureunlock']
                       if any(c['Type']['name'] == 'ChapterTask' and c['Value'] == task_id
                              for c in row['UnlockNeedCondition'])]
            nodes = graph(task['JumpType'])
            entries.append({'order': order, 'task': task, 'rewards': rewards,
                            'guideGraph': nodes, 'featureConditions': unlocks})
            params = {k: v for k, v in task['FinishConditionValues'].items() if k != '$type'}
            condition = f"{task['FinishConditionType']['name']} / {cell(params)} / {task['FinishValue']}"
            reward_text = ', '.join(f"{r['resolvedName']}({r['Id']})×{r['Num']}" for r in rewards) or '—'
            feature_text = ', '.join(f"{r['ID']['name']}({r['ID']['value']})" for r in unlocks) or '—'
            lines.append(f"|{order} / {task_id}|{cell(task['TaskDescription'])}|{condition}|{cell(reward_text)}|{task['JumpType']}|{cell(feature_text)}|")
        chapters.append({'chapter': chapter, 'entries': entries})
        lines.append('')

    lines.extend(['## 第一章任务导航节点', '', 'Next 为成功后继，Fail 为失败后继；小于等于 0 表示结束。', ''])
    for entry in chapters[0]['entries']:
        task = entry['task']
        lines.extend([f"### {task['TaskID']} {task['TaskDescription']}", ''])
        if not entry['guideGraph']:
            lines.extend(['JumpType 为 0，无任务按钮通用导航；场景剧情另外由 StoryManager 派发。', ''])
            continue
        lines.extend(['|节点|动作|参数|Next|Fail|类型 / 保存组|', '|---|---|---|---|---|---|'])
        for node in entry['guideGraph']:
            lines.append(f"|{node['ID']}|{node['Action']}|{cell(node['ActionParam'])}|{node['NextID']}|{node['FNextID']}|{node['GuideType']['name']} / {node['GroupID']} / {node['NeedSave']}|")
        lines.append('')

    source_names = ['Zombie/BootModule.cs', 'Zombie/PlayerProxy.cs', 'Zombie/LoginPage.cs',
                    'Zombie/UIOpenStoryView.cs', 'Zombie/BuildingGameMode.cs',
                    'Zombie/GuideStoryController.cs', 'Zombie/StoryManager.cs',
                    'Zombie/TaskProxy.cs', 'Zombie/UIMainView.cs', 'GuideManager.cs']
    paths = [OUT / 'tables' / (name + '.json') for name in names]
    paths.extend(SOURCE / name for name in source_names)
    provenance = [{'path': path.relative_to(ROOT).as_posix(),
                   'sha256': hashlib.sha256(path.read_bytes()).hexdigest()} for path in paths]
    result = {'evidence': 'Decoded client configuration and static decompiled consumers; no live server observation.',
              'chapters': chapters, 'inputs': provenance,
              'validation': {'chapterCount': len(chapters),
                             'taskOccurrences': sum(len(c['entries']) for c in chapters),
                             'missingReferences': missing}}
    (OUT / 'experience-index.json').write_text(json.dumps(result, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    (OUT / 'CHAPTER-EXPERIENCE.md').write_text('\n'.join(lines) + '\n', encoding='utf-8')
    print(json.dumps(result['validation'], ensure_ascii=False))


if __name__ == '__main__':
    main()
