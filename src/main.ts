import { Command } from 'https://deno.land/x/cliffy@v1.0.0-rc.3/command/mod.ts';
import { ConfigCommand } from './commands/config.command.ts';
import { AddCommand } from './commands/add.command.ts';
import { ListCommand } from './commands/list.command.ts';
import * as path from 'https://deno.land/std@0.202.0/path/mod.ts';

// wowa ls
// wowa ls --retail/-r
// wowa ls --classic/-c
// wowa up
// wowa up --retail/-r
// wowa up --classic/-c
// wowa rm http://details/ --retail/-r
// wowa rm http://details/ --classic/-c
// wowa add http://details/ --retail/-r
// wowa add http://details/ --classic/-c

// https://docs.deno.com/kv/manual/
// ['config', 'git.token'];
// ['addons', 'classic', 'details'];
// ['addons', 'retail', 'details'];

function getAppDataPath(): string {
  const appData = Deno.env.get('APPDATA');
  if (appData) {
    return appData;
  }
  if (Deno.build.os === 'darwin') {
    return Deno.env.get('HOME') + '/Library/Preferences';
  }
  return Deno.env.get('HOME') + '/.local/share';
}

function getDbPath(): string {
  return path.join(getAppDataPath(), 'wowa', 'wowa2.db');
}

const kv = await Deno.openKv(path.dirname(getDbPath()));

await new Command()
  .name('wowa')
  .version('0.1.0')
  .description('A World of Warcraft CLI addon manager')
  // Add
  .command('add <url:string>')
  .option('-r --retail', 'Retail version', { conflicts: ['classic'] })
  .option('-c --classic', 'Classic version', { conflicts: ['retail'] })
  .action(async (options, url) => {
    const cmd = new AddCommand(kv);
    await cmd.handle(url);
  })
  // Remove
  .command('rm <id:string>')
  .option('-r --retail', 'Retail version', { conflicts: ['classic'] })
  .option('-c --classic', 'Classic version', { conflicts: ['retail'] })
  .action((options, id) => {})
  // Update
  .command('up')
  .option('-r --retail', 'Retail version', { conflicts: ['classic'] })
  .option('-c --classic', 'Classic version', { conflicts: ['retail'] })
  .action(options => {})
  // List
  .command('ls')
  .option('-r --retail', 'Retail version', { conflicts: ['classic'] })
  .option('-c --classic', 'Classic version', { conflicts: ['retail'] })
  .action(async options => {
    const cmd = new ListCommand(kv);
    await cmd.handle();
  })
  // List
  .command('config <key:string> [value:string]')
  .action(async (_, key, value) => {
    const cmd = new ConfigCommand(kv);
    await cmd.handle(key, value);
  })
  .parse(Deno.args);
