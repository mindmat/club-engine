import { ApplicationConfig } from '@angular/core';
import { LuxonDateAdapter } from '@angular/material-luxon-adapter';
import { DateAdapter, MAT_DATE_FORMATS } from '@angular/material/core';
import { provideAnimations } from '@angular/platform-browser/animations';
import { provideRouter, withInMemoryScrolling } from '@angular/router';
import { provideFuse } from '@fuse';
import { appRoutes } from 'app/app.routes';
import { provideIcons } from 'app/core/icons/icons.provider';
import { MockApiService } from 'app/mock-api';
import { environment } from 'environments/environment';
import { API_BASE_URL } from './api/api';
import { provideAppEngine } from './app-engine/app-engine.provider';
import { NavigatorService } from './modules/admin/navigator.service';

export const appConfig: ApplicationConfig = {
  providers: [
    provideAnimations(),
    provideRouter(appRoutes, withInMemoryScrolling({ scrollPositionRestoration: 'enabled' })),

    // Material Date Adapter
    {
      provide: DateAdapter,
      useClass: LuxonDateAdapter,
    },
    {
      provide: API_BASE_URL,
      useValue: environment.API_BASE_URL,
    },
    {
      provide: MAT_DATE_FORMATS,
      useValue: {
        parse: {
          dateInput: 'D',
        },
        display: {
          dateInput: 'DDD',
          monthYearLabel: 'LLL yyyy',
          dateA11yLabel: 'DD',
          monthYearA11yLabel: 'LLLL yyyy',
        },
      },
    },
    provideAppEngine({ navigator: NavigatorService }),

    // Fuse
    // provideAuth(),
    provideIcons(),
    provideFuse({
      mockApi: {
        delay: 0,
        service: MockApiService,
      },
      fuse: {
        layout: 'classic',
        scheme: 'light',
        screens: {
          sm: '600px',
          md: '960px',
          lg: '1280px',
          xl: '1440px',
        },
        theme: 'theme-default',
        themes: [
          {
            id: 'theme-default',
            name: 'Default',
          },
          {
            id: 'theme-brand',
            name: 'Brand',
          },
          {
            id: 'theme-teal',
            name: 'Teal',
          },
          {
            id: 'theme-rose',
            name: 'Rose',
          },
          {
            id: 'theme-purple',
            name: 'Purple',
          },
          {
            id: 'theme-amber',
            name: 'Amber',
          },
        ],
      },
    }),
  ],
};
