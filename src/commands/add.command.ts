import { AddonManager } from '../addon-manager.ts';

export class AddCommand {
  constructor(private readonly kv: Deno.Kv) {}

  async handle(url: string): Promise<void> {
    await new AddonManager(this.kv).installFromUrl(url);
  }
}
