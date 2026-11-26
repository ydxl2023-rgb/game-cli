import {readFile} from 'node:fs/promises';
import {createHash} from 'node:crypto';
import {openDatabase} from '../../../../../integrations/codex/server/database.mjs';

const [mode, ...args] = process.argv.slice(2);
if (!['--sync', '--check', '--list', '--ready', '--get', '--validate'].includes(mode))
    throw new Error('Usage: tasks-db.mjs --sync|--check|--list [milestone]|--ready|--get MW-001|--validate');

const seed = JSON.parse(await readFile(new URL('./tasks.json', import.meta.url), 'utf8'));
const ids = new Set(seed.tasks.map(task => task.id));
if (seed.project !== 'miniwow' || seed.schemaVersion !== 1 || ids.size !== seed.tasks.length)
    throw new Error('Invalid task seed identity or duplicate IDs');

const completed = new Set();
const visiting = new Set();
const byId = new Map(seed.tasks.map(task => [task.id, task]));
function visit(id) {
    if (visiting.has(id)) throw new Error(`Dependency cycle at ${id}`);
    if (completed.has(id)) return;
    const task = byId.get(id);
    if (!task) throw new Error(`Unknown dependency ${id}`);
    if (!/^MW-\d{3}$/.test(id) || !task.title || !task.scope || !task.acceptance.length || !task.evidence.length)
        throw new Error(`Incomplete task ${id}`);
    visiting.add(id);
    for (const dependency of task.dependsOn) visit(dependency);
    visiting.delete(id);
    completed.add(id);
}
for (const id of ids) visit(id);

const definition = task => JSON.stringify(task);
const hash = task => createHash('sha256').update(definition(task)).digest('hex');
const marker = 'miniwow-development-tasks/v1';
let db;
let client;
try {
    if (mode === '--validate') {
        console.log(JSON.stringify({validated: true, tasks: ids.size, cycles: 0, missingDependencies: 0}));
    } else {
        db = await openDatabase();
        client = await db.pool.connect();
        await client.query(mode === '--sync' ? 'BEGIN' : 'BEGIN READ ONLY');
        if (mode === '--sync')
            await client.query("SELECT pg_advisory_xact_lock(hashtext('miniwow-development-tasks'))");

        const ownership = (await client.query('SELECT schema_version,owner_tag FROM miniwow_resources.catalog_meta')).rows;
        if (ownership.length !== 1 || ownership[0].schema_version !== 1 || ownership[0].owner_tag !== 'miniwow-resource-catalog')
            throw new Error('Resource schema ownership mismatch');
        const existing = (await client.query("SELECT to_regclass('miniwow_resources.development_tasks') AS relation")).rows[0].relation;
        if (existing) {
            const comment = (await client.query("SELECT obj_description('miniwow_resources.development_tasks'::regclass) AS marker")).rows[0].marker;
            if (comment !== marker) throw new Error('Task table ownership/version mismatch');
        } else if (mode !== '--sync') {
            throw new Error('Task table is not installed; run --sync first');
        }

        if (mode === '--sync') {
            if (!existing) {
                await client.query(`CREATE TABLE miniwow_resources.development_tasks (
                    id text PRIMARY KEY CHECK (id ~ '^MW-[0-9]{3}$'),
                    title text NOT NULL,
                    milestone text NOT NULL,
                    priority text NOT NULL CHECK (priority IN ('P0','P1','P2')),
                    sort_order integer NOT NULL,
                    depends_on text[] NOT NULL DEFAULT '{}',
                    original_task_ids integer[] NOT NULL DEFAULT '{}',
                    definition jsonb NOT NULL,
                    definition_sha256 text NOT NULL,
                    status text NOT NULL DEFAULT 'planned'
                        CHECK (status IN ('planned','in_progress','blocked','review','done','cancelled')),
                    assignee text,
                    progress_notes text NOT NULL DEFAULT '',
                    completion_evidence jsonb NOT NULL DEFAULT '[]'::jsonb,
                    created_at timestamptz NOT NULL DEFAULT now(),
                    definition_updated_at timestamptz NOT NULL DEFAULT now(),
                    progress_updated_at timestamptz NOT NULL DEFAULT now()
                )`);
                await client.query("COMMENT ON TABLE miniwow_resources.development_tasks IS 'miniwow-development-tasks/v1'");
                await client.query('CREATE INDEX development_tasks_status_milestone ON miniwow_resources.development_tasks(status,milestone,sort_order)');
                await client.query('CREATE INDEX development_tasks_original_ids ON miniwow_resources.development_tasks USING gin(original_task_ids)');
            }
            for (const task of seed.tasks) {
                await client.query(`INSERT INTO miniwow_resources.development_tasks
                    (id,title,milestone,priority,sort_order,depends_on,original_task_ids,definition,definition_sha256)
                    VALUES($1,$2,$3,$4,$5,$6,$7,$8::jsonb,$9)
                    ON CONFLICT(id) DO UPDATE SET title=excluded.title,milestone=excluded.milestone,
                    priority=excluded.priority,sort_order=excluded.sort_order,depends_on=excluded.depends_on,
                    original_task_ids=excluded.original_task_ids,definition=excluded.definition,
                    definition_sha256=excluded.definition_sha256,definition_updated_at=now()
                    WHERE development_tasks.definition_sha256 IS DISTINCT FROM excluded.definition_sha256`,
                    [task.id, task.title, task.milestone, task.priority, task.sortOrder,
                     task.dependsOn, task.originalTaskIds, definition(task), hash(task)]);
            }
            const stored = (await client.query('SELECT id,definition_sha256 FROM miniwow_resources.development_tasks WHERE id=ANY($1::text[])', [[...ids]])).rows;
            if (stored.length !== ids.size || stored.some(row => hash(byId.get(row.id)) !== row.definition_sha256))
                throw new Error('Task read-back validation failed');
            console.log(JSON.stringify({synchronized: stored.length, table: 'miniwow_resources.development_tasks', progressPreserved: true}));
        } else if (mode === '--check') {
            const rows = (await client.query('SELECT id,definition_sha256,status FROM miniwow_resources.development_tasks ORDER BY sort_order')).rows;
            const missing = [...ids].filter(id => !rows.some(row => row.id === id));
            const changed = rows.filter(row => byId.has(row.id) && row.definition_sha256 !== hash(byId.get(row.id))).map(row => row.id);
            const dangling = (await client.query(`SELECT t.id,d.dependency FROM miniwow_resources.development_tasks t
                CROSS JOIN LATERAL unnest(t.depends_on) d(dependency)
                LEFT JOIN miniwow_resources.development_tasks p ON p.id=d.dependency WHERE p.id IS NULL`)).rows;
            const counts = (await client.query('SELECT milestone,status,count(*)::int FROM miniwow_resources.development_tasks GROUP BY milestone,status ORDER BY milestone,status')).rows;
            console.log(JSON.stringify({table: 'miniwow_resources.development_tasks', count: rows.length, missing, changed, dangling, counts}));
            if (missing.length || changed.length || dangling.length) process.exitCode = 1;
        } else if (mode === '--get') {
            if (!args[0]) throw new Error('--get requires a task ID');
            const rows = (await client.query('SELECT * FROM miniwow_resources.development_tasks WHERE id=$1', [args[0]])).rows;
            if (!rows.length) throw new Error('Task not found');
            console.log(JSON.stringify(rows[0], null, 2));
        } else {
            const where = mode === '--ready'
                ? `t.status='planned' AND NOT EXISTS (SELECT 1 FROM unnest(t.depends_on) d(id)
                    LEFT JOIN miniwow_resources.development_tasks p ON p.id=d.id WHERE p.id IS NULL OR p.status<>'done')`
                : '($1::text IS NULL OR t.milestone=$1)';
            const rows = (await client.query(`SELECT t.id,t.title,t.milestone,t.priority,t.status,t.depends_on,
                t.assignee,t.progress_notes FROM miniwow_resources.development_tasks t WHERE ${where}
                ORDER BY t.sort_order,t.id`, mode === '--ready' ? [] : [args[0] || null])).rows;
            console.log(JSON.stringify(rows, null, 2));
        }
        await client.query('COMMIT');
    }
} catch (error) {
    if (client) await client.query('ROLLBACK').catch(() => {});
    console.error(JSON.stringify({ok: false, code: error.code || null,
        message: error.message.replace(/(?:postgres(?:ql)?:\/\/)[^\s]+/gi, '[redacted]')}));
    process.exitCode = 1;
} finally {
    client?.release();
    if (db) await db.close();
}
