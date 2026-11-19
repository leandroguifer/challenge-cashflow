import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { ActivatedRouteSnapshot, provideRouter, Router, RouterStateSnapshot, UrlTree } from '@angular/router';
import { authGuard, homeGuard } from './auth.guard';
import { AuthService } from './auth.service';
import { Role } from './roles';

function setup(authenticated: boolean, roles: Role[]) {
  const login = vi.fn().mockResolvedValue(undefined);
  const auth = {
    authenticated: signal(authenticated),
    hasAnyRole: (wanted: readonly Role[]) => roles.some((role) => wanted.includes(role)),
    login,
  };
  TestBed.configureTestingModule({
    providers: [provideRouter([]), { provide: AuthService, useValue: auth }],
  });
  return { login, router: TestBed.inject(Router) };
}

function run(allowed: Role[] | undefined) {
  const route = { data: allowed ? { roles: allowed } : {} } as unknown as ActivatedRouteSnapshot;
  const state = { url: '/relatorio' } as RouterStateSnapshot;
  return TestBed.runInInjectionContext(() => authGuard(route, state));
}

describe('authGuard', () => {
  it('sends an anonymous user to the login with the requested route', () => {
    const { login } = setup(false, []);
    expect(run(undefined)).toBe(false);
    expect(login).toHaveBeenCalledWith(`${window.location.origin}/relatorio`);
  });

  it('allows a user that has one of the required roles', () => {
    setup(true, ['caixa']);
    expect(run(['caixa'])).toBe(true);
  });

  it('redirects a user without the role to the access denied page', () => {
    const { router } = setup(true, ['auxiliar']);
    const result = run(['caixa']) as UrlTree;
    expect(router.serializeUrl(result)).toBe('/acesso-negado');
  });
});

describe('homeGuard', () => {
  it.each([
    [['caixa'] as Role[], '/caixa'],
    [['auxiliar'] as Role[], '/saldo'],
    [['proprietario'] as Role[], '/saldo'],
  ])('sends %j to %s', (roles, expected) => {
    const { router } = setup(true, roles);
    const result = TestBed.runInInjectionContext(() => homeGuard({} as ActivatedRouteSnapshot, {} as RouterStateSnapshot)) as UrlTree;
    expect(router.serializeUrl(result)).toBe(expected);
  });
});
