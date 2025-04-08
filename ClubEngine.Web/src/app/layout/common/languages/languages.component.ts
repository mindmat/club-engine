import { NgTemplateOutlet } from '@angular/common';
import {
    ChangeDetectionStrategy,
    ChangeDetectorRef,
    Component,
    OnDestroy,
    OnInit,
    ViewEncapsulation,
} from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import {
    FuseNavigationService,
    FuseVerticalNavigationComponent,
} from '@fuse/components/navigation';
import { TranslateService } from '@ngx-translate/core';

@Component({
    selector: 'languages',
    templateUrl: './languages.component.html',
    encapsulation: ViewEncapsulation.None,
    changeDetection: ChangeDetectionStrategy.OnPush,
    exportAs: 'languages',
    imports: [MatButtonModule, MatMenuModule, NgTemplateOutlet],
})
export class LanguagesComponent implements OnInit, OnDestroy {
    activeLang: string;
    flagCodes= {
        'de': 'de',
        'en': 'us'
    };
    availableLangs: AvailableLanguage[]= [
        { id: 'de', label: 'Deutsch' },
        { id: 'en', label: 'English' }
    ];

    /**
     * Constructor
     */
    constructor(
        private changeDetectorRef: ChangeDetectorRef,
        private fuseNavigationService: FuseNavigationService,
        private translateService: TranslateService
    ) {}

    // -----------------------------------------------------------------------------------------------------
    // @ Lifecycle hooks
    // -----------------------------------------------------------------------------------------------------

    /**
     * On init
     */
    ngOnInit(): void {
            this.activeLang = this.translateService.currentLang;
    
            // Subscribe to language changes
            this.translateService.onLangChange.subscribe(lang =>
            {
                // Get the active lang
                this.activeLang = lang.lang;
                window.localStorage.setItem('language', lang.lang);
                this.changeDetectorRef.markForCheck();
    
                // Update the navigation
                this._updateNavigation(this.activeLang);
            });
    
            this.changeDetectorRef.markForCheck();
    }

    /**
     * On destroy
     */
    ngOnDestroy(): void {}

    // -----------------------------------------------------------------------------------------------------
    // @ Public methods
    // -----------------------------------------------------------------------------------------------------

    setActiveLang(lang: string): void {
        this.translateService.use(lang);
    }

    /**
     * Track by function for ngFor loops
     *
     * @param index
     * @param item
     */
    trackByFn(index: number, item: any): any {
        return item.id || index;
    }

    private _updateNavigation(lang: string): void {
        // For the demonstration purposes, we will only update the Dashboard names
        // from the navigation but you can do a full swap and change the entire
        // navigation data.
        //
        // You can import the data from a file or request it from your backend,
        // it's up to you.

        // Get the component -> navigation data -> item
        const navComponent =
            this.fuseNavigationService.getComponent<FuseVerticalNavigationComponent>(
                'mainNavigation'
            );

        // Return if the navigation component does not exist
        if (!navComponent) {
            return null;
        }
    }
}


export class AvailableLanguage
{
    id: string;
    label: string;
}