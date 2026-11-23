import {execFile} from 'node:child_process';
import {promisify} from 'node:util';
import {access, readFile, realpath} from 'node:fs/promises';
import {join, resolve} from 'node:path';

const execute = promisify(execFile);

export async function connectUnityCli(project, cliPath) {
  let executable = cliPath ?? process.env.GAMECLI_UNITY_CLI;
  if (!executable) {
    const manifest = JSON.parse(await readFile(join(project, 'Packages', 'manifest.json'), 'utf8'));
    const source = manifest.dependencies?.['com.oathx.unitycli'];
    if (source?.startsWith('file:')) {
      const candidate = resolve(project, 'Packages', source.slice(5), '../../Library/UnityCLI/UnityCLI.exe');
      try { await access(candidate); executable = candidate; }
      catch (error) { if (error.code !== 'ENOENT') throw error; }
    }
    executable ??= join(project, 'Library/UnityCLI/unity-cli.exe');
  }

  async function cli(args) {
    let stdout;
    try {
      ({stdout} = await execute(executable, ['--project', project, '--format', 'json', ...args],
        {timeout: 120000, maxBuffer: 32 * 1024 * 1024, windowsHide: true}));
    } catch (error) {
      throw Error(error.stdout || error.stderr || error.message);
    }
    const result = JSON.parse(stdout);
    if (!result.ok) throw Error(result.error?.message || result.message);
    return result.data;
  }

  const ping = await cli(['ping']);
  if (await realpath(ping.endpoint.projectPath) !== await realpath(project)) throw Error('UnityCLI connected to another project.');
  return cli;
}
