import { Table } from 'https://deno.land/x/cliffy@v1.0.0-rc.3/table/mod.ts';

type Addon = {
  id: string;
  name: string;
  author: string;
  directories: { path: string }[];
  version: string;
  gameVersion: 'retail' | 'classic';
  source: {
    provider: 'curse';
    id: string;
    url: string;
  };
};

export class ListCommand {
  constructor(private readonly kv: Deno.Kv) {}

  async handle(): Promise<void> {
    const table = new Table();

    const addons = this.kv.list<Addon>({ prefix: ['addons'] });

    table.push(['ID', 'Name', 'Version', 'Game Version']);

    for await (const addonEntry of addons) {
      const addon = addonEntry.value;
      table.push([addon.id, addon.name, addon.version, addon.gameVersion]);
    }

    console.log(table.toString());
  }
}
