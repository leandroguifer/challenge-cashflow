import { computed, inject, Injectable, signal } from '@angular/core';
import Keycloak from 'keycloak-js';
import { AppConfigService } from '../config/app-config';
import { ALL_ROLES, Role } from './roles';

export interface Store {
  readonly id: string;
  readonly name: string;
}

interface Profile {
  readonly name: string;
  readonly email: string;
  readonly roles: readonly Role[];
  readonly stores: readonly Store[];
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly appConfig = inject(AppConfigService);
  private keycloak: Keycloak | null = null;
  private readonly profile = signal<Profile | null>(null);

  readonly authenticated = computed(() => this.profile() !== null);
  readonly name = computed(() => this.profile()?.name ?? '');
  readonly email = computed(() => this.profile()?.email ?? '');
  readonly roles = computed(() => this.profile()?.roles ?? []);
  readonly stores = computed(() => this.profile()?.stores ?? []);

  async init(): Promise<void> {
    const { keycloakUrl, keycloakRealm, keycloakClientId } = this.appConfig.config;
    this.keycloak = new Keycloak({
      url: keycloakUrl,
      realm: keycloakRealm,
      clientId: keycloakClientId,
    });
    const authenticated = await this.keycloak.init({
      onLoad: 'check-sso',
      pkceMethod: 'S256',
      checkLoginIframe: false,
    });
    if (authenticated) {
      this.profile.set(this.readProfile());
    }
  }

  hasAnyRole(roles: readonly Role[]): boolean {
    return this.roles().some((role) => roles.includes(role));
  }

  login(redirectUri: string = window.location.href): Promise<void> {
    return this.client().login({ redirectUri });
  }

  logout(): Promise<void> {
    return this.client().logout({ redirectUri: window.location.origin });
  }

  async getToken(): Promise<string> {
    const keycloak = this.client();
    try {
      await keycloak.updateToken(30);
    } catch {
      await this.login();
    }
    return keycloak.token ?? '';
  }

  private client(): Keycloak {
    if (!this.keycloak) {
      throw new Error('Autenticação ainda não inicializada');
    }
    return this.keycloak;
  }

  private readProfile(): Profile {
    const claims = (this.client().tokenParsed ?? {}) as Record<string, unknown>;
    const roles = (claims['roles'] as string[] | undefined) ?? [];
    const groups = (claims['groups'] as string[] | undefined) ?? [];
    return {
      name: (claims['name'] as string | undefined) ?? (claims['preferred_username'] as string | undefined) ?? '',
      email: (claims['email'] as string | undefined) ?? '',
      roles: ALL_ROLES.filter((role) => roles.includes(role)),
      stores: groups.map(toStore).filter((store): store is Store => store !== null),
    };
  }
}

function toStore(path: string): Store | null {
  const match = /^\/Empresa\/(LOJA(\d+))$/.exec(path);
  return match ? { id: match[1], name: `Loja ${match[2]}` } : null;
}
