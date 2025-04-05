import { Injectable } from '@angular/core';
import { Api, PartitionDetails } from 'app/api/api';
import { BehaviorSubject, Observable, filter, of, shareReplay, tap } from 'rxjs';
import { NotificationService } from '../../modules/admin/infrastructure/notification.service';

@Injectable({
  providedIn: 'root'
})
export class PartitionService
{
  private selectedPartitionIdSubject: BehaviorSubject<string | null> = new BehaviorSubject(null);
  private selectedPartitionSubject: BehaviorSubject<PartitionDetails | null> = new BehaviorSubject(null);
  private cache = new Map<string, PartitionDetails>();
  public selectedId$: Observable<string> = this.selectedPartitionIdSubject.pipe(shareReplay(1));
  public selected$: Observable<PartitionDetails> = this.selectedPartitionSubject.pipe(shareReplay(1));

  constructor(private notificationService: NotificationService, private api: Api)
  {
    this.notificationService.switchToPartition(this.selectedPartitionIdSubject.value);
    this.selectedId$.subscribe(partitionId => this.notificationService.switchToPartition(partitionId));
    this.notificationService.subscribe('PartitionByAcronymQuery').pipe(
      filter(e => e.partitionId === this.selectedId || e.partitionId?.toLowerCase() === this.selectedId?.toLowerCase())
    ).subscribe(_ => this.refresh());
  }

  get selected(): PartitionDetails | null
  {
    return this.selectedPartitionSubject.value;
  }

  get selectedId(): string | null
  {
    return this.selectedPartitionIdSubject.value;
  }

  setPartitionByAcronym(acronym: string | null): Observable<any>
  {
    if (acronym === null)
    {
      return of(null);
    }
    acronym = acronym.toLowerCase();
    if (this.cache.has(acronym))
    {
      const partition = this.cache.get(acronym);
      this.setPartition(partition);
      return of(null);
    }
    else
    {
      return this.api.partitionByAcronym_Query({ acronym })
        .pipe(
          tap(partition =>
          {
            if (partition !== null)
            {
              this.cache.set(acronym, partition);
            }
            this.setPartition(partition);
          })
        );
    }
  }

  refresh(): void
  {
    var acronym = this.selected.acronym;
    if (!!acronym)
    {
      this.api.partitionByAcronym_Query({ acronym })
        .subscribe(result =>
        {
          if (result !== null)
          {
            this.cache.set(acronym, result);
          }
          this.setPartition(result);
        });
    }
  }

  private setPartition(partitionDetails: PartitionDetails)
  {
    this.selectedPartitionSubject.next(partitionDetails);
    this.selectedPartitionIdSubject.next(partitionDetails.id);
  }
}
