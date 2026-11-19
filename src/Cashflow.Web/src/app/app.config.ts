import { provideHttpClient, withInterceptors } from '@angular/common/http';
import {
  ApplicationConfig,
  inject,
  provideAppInitializer,
  provideBrowserGlobalErrorListeners,
  signal,
} from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideTaiga, TUI_DEFAULT_NUMBER_FORMAT, TUI_NUMBER_FORMAT } from '@taiga-ui/core';
import { routes } from './app.routes';
import { authInterceptor } from './core/auth/auth.interceptor';
import { AuthService } from './core/auth/auth.service';
import { AppConfigService } from './core/config/app-config';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideTaiga(),
    {
      provide: TUI_NUMBER_FORMAT,
      useValue: signal({
        ...TUI_DEFAULT_NUMBER_FORMAT,
        decimalSeparator: ',',
        thousandSeparator: '.',
        minimumFractionDigits: 2,
        maximumFractionDigits: 2,
      }),
    },
    provideHttpClient(withInterceptors([authInterceptor])),
    provideAppInitializer(async () => {
      const config = inject(AppConfigService);
      const auth = inject(AuthService);
      await config.load();
      await auth.init();
    }),
  ],
};
