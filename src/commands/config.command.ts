export class ConfigCommand {
  constructor(private readonly kv: Deno.Kv) {}

  async handle(key: string, value: string | undefined): Promise<void> {
    if (value !== undefined) {
      await this.kv.set(['config', key], value);
      return;
    }
    const entry = await this.kv.get(['config', key]);
    console.log(entry.value ?? 'null');
  }
}
