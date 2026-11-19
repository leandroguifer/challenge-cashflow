import { Injectable } from '@angular/core';

export interface AppConfig {
  readonly keycloakUrl: string;
  readonly keycloakRealm: string;
  readonly keycloakClientId: string;
  readonly entriesApiUrl: string;
  readonly balanceApiUrl: string;
}

@Injectable({ providedIn: 'root' })
export class AppConfigService {
  private value: AppConfig | null = null;

  get config(): AppConfig {
    if (!this.value) {
      throw new Error('Configuração ainda não carregada');
    }
    return this.value;
  }

  async load(): Promise<void> {
    const response = await fetch('/config.json', { cache: 'no-store' });
    if (!response.ok) {
      throw new Error('Não foi possível carregar /config.json');
    }
    this.value = (await response.json()) as AppConfig;
  }
}
