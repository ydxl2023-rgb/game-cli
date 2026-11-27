"""Validate exported exploration references against recovered source evidence."""
import argparse
import json
import math
from pathlib import Path

parser = argparse.ArgumentParser()
parser.add_argument('--output', required=True, type=Path)
args = parser.parse_args()
root = Path(__file__).resolve().parents[2]

def read(path):
    return json.loads(path.read_text(encoding='utf-8-sig'))

events = {row['ID']: row for row in read(root / 'research/config-study/tables/tbadventureevent.json')}
heroes = {row['ID'] for row in read(root / 'research/config-study/tables/tbhero.json')}
globals = read(root / 'research/config-study/tables/tbglobal.json')
global_row = globals[0] if isinstance(globals, list) else globals
scene = read(root / 'tools/main-scene/scene.json')
point = next(node['position'] for node in scene['nodes'] if node['name'] == 'ExplorePoint')
tasks = read(args.output / 'json/TaskConfig.bytes')['data']
journeys = read(args.output / 'json/ExploreJourneyConfig.bytes')['data']
assert journeys, 'No exploration configuration exported'
for key, row in journeys.items():
    assert int(key) == row['ID']
    assert row['HeroID'] in heroes
    assert row['ActorID'] == 1, 'Current world binding supports the authored Garen actor'
    assert str(row['RequiredTaskID']) in tasks
    event = events[row['EventID']]
    assert row['AreaID'] == event['AreaId']
    assert row['EventType'] == event['EventType']['value']
    duration = global_row['AdventureEventExecuteTime'][event['EventType']['name']] / 1000
    assert math.isclose(row['ExecuteSeconds'], duration, abs_tol=0.0001)
    assert math.isfinite(row['TravelSeconds']) and row['TravelSeconds'] > 0
    for axis in 'XYZ':
        assert math.isclose(row['Exit' + axis], point[axis.lower()], abs_tol=0.0001)
print(json.dumps({'passed': True, 'journeys': len(journeys),
                  'scope': 'source references, original event time, finite local travel duration, original exit'}))
