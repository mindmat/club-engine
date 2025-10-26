import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { EnvironmentProviders, Provider, Type } from '@angular/core';
import { authHttpInterceptorFn, provideAuth0 } from '@auth0/auth0-angular';
import { fuseLoadingInterceptor } from '@fuse/services/loading';
import { MissingTranslationHandler, provideTranslateService, TranslateLoader } from '@ngx-translate/core';
import { AppEngineNavigator } from './app-engine-navigator.service';
import { MissingTranslationService } from './internationalization/missing-translation.service';
import { TranslationLoaderService } from './internationalization/translation-loader.service';

export interface AppEngineConfig {
  navigator: Type<any>;
}
export const provideAppEngine = (config: AppEngineConfig): Array<Provider | EnvironmentProviders> => {
  // Base providers
  const providers: Array<Provider | EnvironmentProviders> = [
    provideTranslateService({
      isolate: false,
      defaultLanguage: 'de',
      loader: {
        provide: TranslateLoader,
        useFactory: TranslationLoaderFactory,
        deps: [TranslationLoaderService],
      },
      missingTranslationHandler: { provide: MissingTranslationHandler, useClass: MissingTranslationService },
    }),
    provideAuth0({
      domain: 'clubengine.eu.auth0.com',
      clientId: 'PyuWXpy5KXTHXCMjzCe2EeE7We4hSvwe',
      // useRefreshTokens: true,
      cacheLocation: 'localstorage',
      authorizationParams: {
        redirect_uri: window.location.origin,

        // Request this audience at user authentication time
        audience: 'https://clubengine.ch/api',

        // Request this scope at user authentication time
        // scope: 'read:current_user'
      },

      // Specify configuration for the interceptor
      httpInterceptor: {
        allowedList: [
          {
            // Match any request that starts 'https://clubengine.eu.auth0.com/api/v2/' (note the asterisk)
            uri: 'https://clubengine.eu.auth0.com/api/v2/*',
            tokenOptions: {
              authorizationParams: {
                // The attached token should target this audience
                audience: 'https://clubengine.ch/api',

                // The attached token should have these scopes
                // scope: 'read:current_user'
              },
            },
          },
          {
            // Match any request that starts {uri} (note the asterisk)
            uri: 'https://localhost:7312/api/*',
            tokenOptions: {
              authorizationParams: {
                // The attached token should target this audience
                audience: 'https://clubengine.ch/api',

                // The attached token should have these scopes
                // scope: 'read:current_user'
              },
            },
          },
          {
            // Match any request that starts {uri} (note the asterisk)
            uri: 'https://api.clubengine.ch/api/*',
            tokenOptions: {
              authorizationParams: {
                // The attached token should target this audience
                audience: 'https://clubengine.ch/api',

                // The attached token should have these scopes
                // scope: 'read:current_user'
              },
            },
          },
        ],
      },
    }),
    provideHttpClient(withInterceptors([authHttpInterceptorFn, fuseLoadingInterceptor])),
    { provide: AppEngineNavigator, useClass: config.navigator },
  ];

  return providers;
};

export function TranslationLoaderFactory(service: TranslationLoaderService): TranslateLoader {
  return service.createLoader();
}
