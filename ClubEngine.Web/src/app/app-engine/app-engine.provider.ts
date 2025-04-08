import { ModuleWithProviders, Provider, EnvironmentProviders } from "@angular/core";
import { MissingTranslationHandler, provideTranslateService, TranslateLoader } from "@ngx-translate/core";
import { TranslationLoaderService } from "./internationalization/translation-loader.service";
import { MissingTranslationService } from "./internationalization/missing-translation.service";

export interface AppEngineConfig {
}
export const provideAppEngine = (
    config: AppEngineConfig
): Array<Provider | EnvironmentProviders> => {
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
            missingTranslationHandler: { provide: MissingTranslationHandler, useClass: MissingTranslationService }
        }),

    ];

    return providers;
};


export function TranslationLoaderFactory(service: TranslationLoaderService): TranslateLoader {
    return service.createLoader();
}