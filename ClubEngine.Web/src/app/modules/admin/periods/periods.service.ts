import { Injectable } from '@angular/core';
import { Api, PeriodDisplayItem } from 'app/api/api';
import { PartitionService } from 'app/app-engine/partitions/partition.service';
import { Observable } from 'rxjs';
import { FetchService } from '../infrastructure/fetchService';
import { NotificationService } from '../infrastructure/notification.service';

@Injectable({
  providedIn: 'root',
})
export class PeriodsService extends FetchService<PeriodDisplayItem[]> {
  constructor(
    private api: Api,
    private partitionService: PartitionService,
    notificationService: NotificationService
  ) {
    super('PeriodsQuery', notificationService);
  }

  get periods$(): Observable<PeriodDisplayItem[]> {
    return this.result$;
  }

  fetch(): Observable<PeriodDisplayItem[]> {
    return this.fetchItems(this.api.periods_Query({ partitionId: this.partitionService.selectedId }));
  }
}
