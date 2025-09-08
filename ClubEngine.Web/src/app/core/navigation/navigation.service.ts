import { Injectable } from '@angular/core';
import { FuseNavigationItem } from '@fuse/components/navigation';
import { TranslateService } from '@ngx-translate/core';
import { MenuNodeContent, MenuNodeStyle } from 'app/api/api';
import { Navigation } from 'app/core/navigation/navigation.types';
import { BehaviorSubject, combineLatest, filter, map, Observable, ReplaySubject, startWith, tap } from 'rxjs';
import { MenuService } from './menu.service';
import { PartitionService } from 'app/app-engine/partitions/partition.service';

@Injectable({ providedIn: 'root' })
export class NavigationService
{
    private _navigation: ReplaySubject<Navigation> = new ReplaySubject<Navigation>(1);

    private menu = new BehaviorSubject<FuseNavigationItem[]>(
        [
            {
                id: 'select-event',
                title: 'Verein auswählen',
                type: 'basic',
                icon: 'heroicons_outline:list-bullet',
                link: `/select-event`,
            }
        ]);

    /**
     * Constructor
     */
    constructor(eventService: PartitionService,
        translateService: TranslateService,
        menuService: MenuService)
    {
        combineLatest([translateService.onLangChange.asObservable().pipe(map(e => e.lang), startWith(translateService.currentLang)),
        eventService.selected$,
        menuService.nodeContents$])
            .pipe(
                filter(([_, e, __]) => e?.acronym != null),
                tap(([lang, e, nodes]) =>
                {
                    this.menu.next([
                        {
                            id: 'select-club',
                            title: e.name, // translateService.instant('SelectEvent'),
                            type: 'basic',
                            icon: 'heroicons_outline:clipboard-check',
                            link: '/'
                        },
                        {
                            id: 'admin',
                            title: translateService.instant('Admin'),
                            type: 'group',
                            children: [
                                {
                                    id: 'import-member-list',
                                    title: translateService.instant('ImportMemberList'),
                                    type: 'basic',
                                    icon: 'heroicons_outline:clipboard-document-list',
                                    link: `/${e.acronym}/import-member-list`,
                                },
                                {
                                    id: 'members',
                                    title: translateService.instant('Members'),
                                    type: 'basic',
                                    icon: 'heroicons_outline:users',
                                    link: `/${e.acronym}/members`,
                                    badge: this.getBadge(nodes, 'MembersNodeKey')
                                },
                                {
                                    id: 'slack-matching',
                                    title: translateService.instant('SlackMatching'),
                                    type: 'basic',
                                    icon: 'feather:slack',
                                    link: `/${e.acronym}/slack-matching`,
                                    badge: this.getBadge(nodes, 'SlackMatchingNodeKey')
                                },
                                // {
                                //     id: 'search-registration',
                                //     title: translateService.instant('SearchRegistration'),
                                //     type: 'basic',
                                //     icon: 'heroicons_outline:user',
                                //     link: `/${e.acronym}/registrations/search-registration`,
                                // },
                                // {
                                //     id: 'fix-raw-processing',
                                //     title: translateService.instant('MenuNodeKey_FixRawProcessing'),
                                //     type: 'basic',
                                //     icon: 'mat_outline:error_outline',
                                //     link: `/${e.acronym}/registrations/fix-raw-processing`,
                                //     badge: this.getBadge(nodes, MenuNodeKey.FixRawProcessing),
                                //     hidden: _ => this.isHidden(nodes, MenuNodeKey.FixRawProcessing)
                                // },
                                // {
                                //     id: 'problematic-emails',
                                //     title: translateService.instant('MailMonitor'),
                                //     type: 'basic',
                                //     icon: 'mat_outline:mail',
                                //     link: `/${e.acronym}/mailing/problematic-emails`,
                                //     badge: this.getBadge(nodes, MenuNodeKey.MailTracking)
                                // },
                                // {
                                //     id: 'all-participants',
                                //     title: translateService.instant('Participants'),
                                //     type: 'basic',
                                //     icon: 'mat_outline:list',
                                //     link: `/${e.acronym}/registrations/all-participants`,
                                // },
                            ]
                        },
                        {
                            id: 'accounting',
                            title: translateService.instant('Accounting'),
                            type: 'group',
                            children: [
                                {
                                    id: 'bank-statements',
                                    title: translateService.instant('BankStatements'),
                                    type: 'basic',
                                    icon: 'heroicons_outline:currency-dollar',
                                    link: `/${e.acronym}/accounting/bank-statements`,
                                },
                                //         {
                                //             id: 'settle-bookings',
                                //             title: translateService.instant('AssignBankStatements'),
                                //             type: 'basic',
                                //             icon: 'heroicons_outline:check',
                                //             link: `/${e.acronym}/accounting/settle-payments`,
                                //         },
                                //         {
                                //             id: 'due-payments',
                                //             title: translateService.instant('DuePayments'),
                                //             type: 'basic',
                                //             icon: 'mat_outline:hourglass_bottom',
                                //             link: `/${e.acronym}/accounting/due-payments`,
                                //             badge: this.getBadge(nodes, MenuNodeKey.DuePayments)
                                //         },
                                //         {
                                //             id: 'payment-differences',
                                //             title: translateService.instant('PaymentDifferences'),
                                //             type: 'basic',
                                //             icon: 'heroicons_outline:switch-vertical',
                                //             link: `/${e.acronym}/accounting/payment-differences`,
                                //         },
                                //         {
                                //             id: 'payouts',
                                //             title: translateService.instant('Payouts'),
                                //             type: 'basic',
                                //             icon: 'heroicons_outline:arrow-right',
                                //             link: `/${e.acronym}/accounting/payouts`,
                                // }
                            ]
                        },
                        {
                            id: 'setup',
                            title: translateService.instant('Setup'),
                            type: 'group',
                            icon: 'mat_outline:mail',
                            children: [
                                {
                                    id: 'event-settings',
                                    title: translateService.instant('Settings'),
                                    type: 'basic',
                                    icon: 'heroicons_outline:cog-8-tooth',
                                    link: `/${e.acronym}/admin/settings`,
                                },
                                // {
                                //     id: 'setup-event',
                                //     title: translateService.instant('SetupEvent'),
                                //     type: 'basic',
                                //     icon: 'heroicons_outline:cog-6-tooth',
                                //     link: `/${e.acronym}/admin/setup-event`,
                                // },
                                // {
                                //     id: 'auto-mail-templates',
                                //     title: translateService.instant('AutoMailTemplates'),
                                //     type: 'basic',
                                //     icon: 'mat_outline:mail',
                                //     link: `/${e.acronym}/mailing/auto-mail-templates`,
                                // },
                                // {
                                //     id: 'bulk-mail-templates',
                                //     title: translateService.instant('BulkMailTemplates'),
                                //     type: 'basic',
                                //     icon: 'mat_outline:mail',
                                //     link: `/${e.acronym}/mailing/bulk-mail-templates`,
                                // },
                                // {
                                //     id: 'form-mapping',
                                //     title: translateService.instant('Forms'),
                                //     type: 'basic',
                                //     icon: 'heroicons_outline:clipboard-list',
                                //     link: `/${e.acronym}/admin/form-mapping`,
                                // },
                                // {
                                //     id: 'pricing',
                                //     title: translateService.instant('Pricing'),
                                //     type: 'basic',
                                //     icon: 'heroicons_outline:cash',
                                //     link: `/${e.acronym}/admin/pricing`,
                                // },
                            ]
                        },
                    ]);
                }))
            .subscribe();
    }

    private getBadge(contents: MenuNodeContent[] | null, key: string): { title: string, classes: string; } | null
    {
        var content = contents?.find(nct => nct.key === key);
        if (!content)
        {
            return null;
        }
        return {
            title: content.content,
            classes: this.getBadgeStyle(content)
        };
    }

    private isHidden(contents: MenuNodeContent[] | null, key: string): boolean
    {
        var content = contents?.find(nct => nct.key === key);
        if (!content)
        {
            return false;
        }
        return content.hidden === true;
    }

    getBadgeStyle(content: MenuNodeContent): string | null
    {
        if (!content.content)
        {
            // avoid empty badge
            return null;
        }
        switch (content.style)
        {
            case MenuNodeStyle.Info: return 'px-2 bg-sky-600 text-black rounded-full';
            case MenuNodeStyle.ToDo: return 'px-2 bg-yellow-500 text-black rounded-full';
            case MenuNodeStyle.Important: return 'px-2 bg-red-500 text-black rounded-full';
            default: return 'px-2 bg-sky-600 text-black rounded-full';
        }
    }

    // -----------------------------------------------------------------------------------------------------
    // @ Accessors
    // -----------------------------------------------------------------------------------------------------

    /**
     * Getter for navigation
     */
    get navigation$(): Observable<Navigation>
    {
        return this.menu.pipe(
            map(menu =>
            {
                return {
                    default: menu,
                    compact: menu,
                    horizontal: menu,
                    futuristic: menu
                } as Navigation;
            })
        );
        // return of({
        //     default: this.menu,
        //     compact: this.menu,
        //     horizontal: this.menu,
        //     futuristic: this.menu
        // } as Navigation);

        // return this._navigation.asObservable();
    }

    // -----------------------------------------------------------------------------------------------------
    // @ Public methods
    // -----------------------------------------------------------------------------------------------------

    /**
     * Get all navigation data
     */
    // get(): Observable<Navigation>
    // {
    //     return of({
    //         default: this.menu,
    //         compact: this.menu,
    //         horizontal: this.menu,
    //         futuristic: this.menu
    //     } as Navigation);

    //     // return this._httpClient.get<Navigation>('api/common/navigation').pipe(
    //     //     tap((navigation) =>
    //     //     {
    //     //         this._navigation.next(navigation);
    //     //     })
    //     // );
    // }
}
