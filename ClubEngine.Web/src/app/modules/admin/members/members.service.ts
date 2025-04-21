import { Injectable } from '@angular/core';
import { FetchService } from '../infrastructure/fetchService';
import { PartitionService } from 'app/app-engine/partitions/partition.service';
import { Api, MemberDisplayItem } from 'app/api/api';
import { NotificationService } from '../infrastructure/notification.service';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class MembersService extends FetchService<MemberDisplayItem[]> {
  constructor(private api: Api,
    private partitionService: PartitionService,
    notificationService: NotificationService) {
    super('MembersQuery', notificationService);
  }

  get members$() {
    return this.result$;
  }

  fetch() {
    return this.fetchItems(this.api.members_Query({ partitionId: this.partitionService.selectedId }));
  }
}
