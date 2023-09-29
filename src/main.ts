import { Command } from 'https://deno.land/x/cliffy@v1.0.0-rc.3/command/mod.ts';

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
['config', 'git.token'];
['addons', 'classic', 'details'];
['addons', 'retail', 'details'];

await new Command()
  .name('wowa')
  .version('0.1.0')
  .description('Command line framework for Deno')
  // Add
  .command('add <url:string>')
  .option('-r --retail', 'Retail version', { conflicts: ['classic'] })
  .option('-c --classic', 'Classic version', { conflicts: ['retail'] })
  .action((options, url) => {})
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
  .action(options => {})
  // List
  .command('config <key:string> [value:string]')
  .action((_, key, value) => {})
  .parse(Deno.args);
