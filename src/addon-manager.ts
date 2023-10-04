import {
  CurseAddon,
  CurseFile,
  CurseSearchModsResponse,
} from './commands/curse.ts';
import Zip from 'npm:jszip';
import * as path from 'https://deno.land/std@0.202.0/path/mod.ts';

export class AddonManager {
  constructor(private readonly kv: Deno.Kv) {}

  async install(addon: CurseAddon): Promise<void> {}

  async installFromUrl(url: string): Promise<void> {
    const curseUrl = 'https://www.curseforge.com/wow/addons/';
    if (!url.startsWith(curseUrl)) {
      console.error(
        'The url should start with https://www.curseforge.com/wow/addons/'
      );
      return;
    }

    const slug = url
      .replace('https://www.curseforge.com/wow/addons/', '')
      .split('/')[0];
    console.log('Installing ' + slug + '...');

    const baseUrl = 'https://api.curseforge.com';
    const searchUrl = new URL('/v1/mods/search', baseUrl);
    searchUrl.searchParams.append('gameId', String(1));
    searchUrl.searchParams.append('categoryId', String(0));
    searchUrl.searchParams.append('searchFilter', slug);
    searchUrl.searchParams.append('sortField', String(2));
    searchUrl.searchParams.append('sortOrder', 'desc');
    searchUrl.searchParams.append('index', String(0));
    searchUrl.searchParams.append('gameVersionTypeId', String(517));

    const tokenEntry = await this.kv.get<string>(['config', 'curse.token']);
    const headers = new Headers();
    headers.set('x-api-key', tokenEntry.value ?? '');

    const res = await fetch(searchUrl, {
      method: 'GET',
      headers,
    });
    const body: CurseSearchModsResponse = await res.json();

    const addon = body.data.find(a => a.slug === slug);
    if (!addon) {
      console.error('No addon found with slug ' + slug);
      return;
    }

    const fileIndex = addon.latestFilesIndexes.find(
      f => f.gameVersionTypeId === 517 && f.releaseType === 1
    );
    if (!fileIndex) {
      console.error('No addon file found');
      return;
    }

    const fileUrl = new URL(
      `/v1/mods/${addon.id}/files/${fileIndex.fileId}`,
      baseUrl
    );
    const fileRes = await fetch(fileUrl, {
      method: 'GET',
      headers,
    });
    const file = (await fileRes.json()).data as CurseFile;

    const zipFileRes = await fetch(file.downloadUrl, {
      method: 'GET',
    });
    const zipFile = await zipFileRes.blob();
    console.log(zipFile.size);

    const zip = new Zip();
    await zip.loadAsync(zipFile);

    const gameDir = (await this.kv.get<string>(['config', 'game.dir'])).value;
    if (gameDir === null) {
      throw new Error('game dir not found');
    }

    for (const [zipName, zipFile] of Object.entries(zip.files)) {
      const dir = path.join(
        gameDir,
        '_retail_',
        'Interface',
        'Addons',
        zipName
      );

      if (zipFile.dir) {
        await Deno.mkdir(dir, { recursive: true });
        // await mkdir(dir);
      } else {
        await Deno.writeFile(dir, await zipFile.async('uint8array'));
        // await writeFile(dir, await zipFile.async('nodebuffer'));
      }
    }

    await this.kv.set(['addons', 'retail', slug], {
      id: slug,
      name: addon.name,
      author: 'addon.',
      directories: file.modules.map(m => ({
        path: m.name,
      })),
      version: file.displayName,
      gameVersion: 'retail',
      source: {
        provider: 'curse',
        id: String(addon.id),
        url: url,
      },
    });
  }
}
