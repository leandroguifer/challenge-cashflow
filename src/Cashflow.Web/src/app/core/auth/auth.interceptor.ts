import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, from, switchMap, throwError } from 'rxjs';
import { AppConfigService } from '../config/app-config';
import { AuthService } from './auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const { entriesApiUrl, balanceApiUrl } = inject(AppConfigService).config;

  if (![entriesApiUrl, balanceApiUrl].some((url) => req.url.startsWith(url))) {
    return next(req);
  }

  return from(auth.getToken()).pipe(
    switchMap((token) => next(req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }))),
    catchError((error: unknown) => {
      if (error instanceof HttpErrorResponse && error.status === 401) {
        void auth.login();
      }
      return throwError(() => error);
    }),
  );
};
