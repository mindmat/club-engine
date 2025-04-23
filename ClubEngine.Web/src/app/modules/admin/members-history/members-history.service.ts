import { Injectable } from '@angular/core';
import { MemberDisplayItem, Api, MemberStats } from 'app/api/api';
import { PartitionService } from 'app/app-engine/partitions/partition.service';
import { FetchService } from '../infrastructure/fetchService';
import { NotificationService } from '../infrastructure/notification.service';

@Injectable({
  providedIn: 'root'
})
export class MembersHistoryService extends FetchService<MemberStats> {
  constructor(private api: Api,
    private partitionService: PartitionService,
    notificationService: NotificationService) {
    super('MemberStatsQuery', notificationService);
  }

  get stats$() {
    return this.result$;
  }

  fetch() {
    return this.fetchItems(this.api.memberStats_Query({ partitionId: this.partitionService.selectedId }));
  }
}
