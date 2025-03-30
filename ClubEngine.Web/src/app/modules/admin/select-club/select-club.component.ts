import { CdkScrollable } from '@angular/cdk/scrolling';
import { I18nPluralPipe, NgClass, PercentPipe } from '@angular/common';
import {
    ChangeDetectionStrategy,
    ChangeDetectorRef,
    Component,
    OnDestroy,
    OnInit,
    ViewEncapsulation,
} from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatOptionModule } from '@angular/material/core';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSelectChange, MatSelectModule } from '@angular/material/select';
import {
    MatSlideToggleChange,
    MatSlideToggleModule,
} from '@angular/material/slide-toggle';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FuseFindByKeyPipe } from '@fuse/pipes/find-by-key/find-by-key.pipe';
import { ClubListItem } from 'app/api/api';
import { BehaviorSubject, Subject, combineLatest, takeUntil } from 'rxjs';
import { SelectClubService } from './select-club.service';

@Component({
    selector: 'select-club',
    templateUrl: './select-club.component.html',
    encapsulation: ViewEncapsulation.None,
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        CdkScrollable,
        MatFormFieldModule,
        MatSelectModule,
        MatOptionModule,
        MatIconModule,
        MatInputModule,
        MatSlideToggleModule,
        NgClass,
        MatTooltipModule,
        MatProgressBarModule,
        MatButtonModule,
        RouterLink,
        FuseFindByKeyPipe,
        PercentPipe,
        I18nPluralPipe,
    ],
})
export class SelectClubComponent implements OnInit, OnDestroy {
    // categories: Category[];
    clubs: ClubListItem[];
    filteredClubs: ClubListItem[];
    filters: {
        // categorySlug$: BehaviorSubject<string>;
        query$: BehaviorSubject<string>;
        showInactive$: BehaviorSubject<boolean>;
    } = {
        // categorySlug$: new BehaviorSubject('all'),
        query$: new BehaviorSubject(''),
        showInactive$: new BehaviorSubject(false),
    };

    private unsubscribeAll: Subject<any> = new Subject<any>();

    /**
     * Constructor
     */
    constructor(
        private _activatedRoute: ActivatedRoute,
        private changeDetectorRef: ChangeDetectorRef,
        private _router: Router,
        private selectClubService: SelectClubService
    ) {}

    ngOnInit(): void {
        // Get the categories
        // this._academyService.categories$
        //     .pipe(takeUntil(this._unsubscribeAll))
        //     .subscribe((categories: Category[]) => {
        //         this.categories = categories;

        //         // Mark for check
        //         this._changeDetectorRef.markForCheck();
        //     });

        // Get the courses
        combineLatest([this.selectClubService.clubs$, this.filters.query$, this.filters.showInactive$])
        .pipe(takeUntil(this.unsubscribeAll))
        .subscribe(([clubs, query, showInactive]) =>
        {
          this.clubs = clubs;
          this.filteredClubs = clubs;
        //   this.requests = events.requests;
        //   this.filteredRequests = events.requests;
  
          if (query != null && query !== '')
          {
            this.filteredClubs  = this.filteredClubs.filter(clb => clb.name?.toLowerCase().includes(query.toLowerCase()));
            // this.filteredRequests = this.filteredRequests.filter(evt => evt.eventName?.toLowerCase().includes(query.toLowerCase()));
          }
  
        //   this.inactiveClubsInList = this.filteredClubs.some(evt => evt.eventState === EventState.Finished);
        //   if (!showInactive && this.inactiveClubsInList)
        //   {
        //     this.filteredClubs = this.filteredClubs.filter(evt => evt.eventState !== EventState.Finished);
        //   }
  
          // Mark for check
          this.changeDetectorRef.markForCheck();
        });
    }

    ngOnDestroy(): void {
        // Unsubscribe from all subscriptions
        this.unsubscribeAll.next(null);
        this.unsubscribeAll.complete();
    }

    filterByQuery(query: string): void {
        this.filters.query$.next(query);
    }

    // filterByCategory(change: MatSelectChange): void {
    //     this.filters.categorySlug$.next(change.value);
    // }

    toggleInactive(change: MatSlideToggleChange): void {
        this.filters.showInactive$.next(change.checked);
    }

    trackByFn(index: number, item: any): any {
        return item.id || index;
    }
}
