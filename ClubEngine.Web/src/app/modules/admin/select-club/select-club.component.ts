import { CdkScrollable } from '@angular/cdk/scrolling';
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
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleChange, MatSlideToggleModule, } from '@angular/material/slide-toggle';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { BehaviorSubject, Subject, combineLatest, debounceTime, switchMap, takeUntil } from 'rxjs';
import { SelectClubService as PartitionService } from './select-club.service';
import { TranslatePipe } from '@ngx-translate/core';
import { MyPartitions } from 'app/api/api';

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
        MatTooltipModule,
        MatProgressBarModule,
        MatButtonModule,
        RouterLink,
        TranslatePipe
    ],
})
export class SelectClubComponent implements OnInit, OnDestroy {
    public partitions: MyPartitions | null = null;

    // categories: Category[];
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
        private partitionService: PartitionService
    ) { }

    ngOnInit(): void {
        // Get the categories
        // this._academyService.categories$
        //     .pipe(takeUntil(this._unsubscribeAll))
        //     .subscribe((categories: Category[]) => {
        //         this.categories = categories;

        //         // Mark for check
        //         this._changeDetectorRef.markForCheck();
        //     });

        combineLatest([this.filters.query$, this.filters.showInactive$])
            .pipe(
                takeUntil(this.unsubscribeAll),
                debounceTime(300),
                switchMap(([query, showInactive]) => this.partitionService.fetch(query, showInactive)))
            .subscribe();

        // Get the courses
        this.partitionService.clubs$
            .pipe(takeUntil(this.unsubscribeAll))
            .subscribe(partitions => {
                this.partitions = partitions;
                this.changeDetectorRef.markForCheck();
            });
    }

    ngOnDestroy(): void {
        // Unsubscribe from all subscriptions
        this.unsubscribeAll.next(null);
        this.unsubscribeAll.complete();
    }

    requestAccess(partitionId: string, requestText: string) {
        this.partitionService.requestAccess(partitionId, requestText);
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
