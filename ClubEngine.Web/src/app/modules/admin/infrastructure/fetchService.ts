import { BehaviorSubject, filter, map, Observable, of, switchMap, throwError } from 'rxjs';
import { NotificationService } from './notification.service';

export class FetchService<TItem> {
    private result: BehaviorSubject<TItem | null> = new BehaviorSubject(null);
    private fetch$: Observable<TItem>;
    private rowId?: string;
    private partitionId?: string;

    constructor(queryName: string | null = null, notificationService: NotificationService | null = null) {
        if (queryName !== null && notificationService !== null) {
            notificationService.subscribe(queryName)
                .pipe(
                    filter(e => (e.rowId === this.rowId || e.rowId?.toLowerCase() === this.rowId?.toLowerCase())
                        && (e.partitionId === this.partitionId || e.partitionId?.toLowerCase() === this.partitionId?.toLowerCase())
                    ))
                .subscribe(_ => this.refresh());
        }
    }

    refresh(): void {
        if (!this.fetch$) {
            return;
        }
        this.fetch$.subscribe(result => this.result.next(result));
    }

    public get result$(): Observable<TItem> {
        return this.result.asObservable();
    }

    public get current(): TItem | null {
        return this.result.getValue();
    }

    protected fetchItems(fetch: Observable<TItem>, rowId: string | null = null, partitionId: string | null = null): Observable<TItem> {
        this.rowId = rowId;
        this.partitionId = partitionId;
        this.fetch$ = fetch;
        return this.fetch$.pipe(
            map(newItems => {
                // Update the item
                this.result.next(newItems);

                // Return the item
                return newItems;
            }),
            switchMap(newItems => {
                if (!newItems) {
                    return throwError(() => 'Could not find any items!');
                }

                return of(newItems);
            })
        );
    }
}

