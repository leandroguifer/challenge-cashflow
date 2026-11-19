import { inject } from '@angular/core';
import { CanActivateChildFn, CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';
import { Role } from './roles';

export const authGuard: CanActivateChildFn = (route, state) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (!auth.authenticated()) {
    void auth.login(window.location.origin + state.url);
    return false;
  }

  const roles = route.data['roles'] as readonly Role[] | undefined;
  return !roles || auth.hasAnyRole(roles) ? true : router.createUrlTree(['/acesso-negado']);
};

export const homeGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  return router.createUrlTree([auth.hasAnyRole(['caixa']) ? '/caixa' : '/saldo']);
};
