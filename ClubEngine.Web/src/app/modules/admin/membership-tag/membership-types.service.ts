import { Injectable } from '@angular/core';
import { FetchService } from '../infrastructure/fetchService';
import { Api, MembershipTypeItem } from 'app/api/api';
import { NotificationService } from '../infrastructure/notification.service';
import { PartitionService } from 'app/app-engine/partitions/partition.service';

@Injectable({
  providedIn: 'root'
})
export class MembershipTypesService extends FetchService<MembershipTypeItem[]> {
  constructor(private api: Api,
    notificationService: NotificationService,
    clubService: PartitionService) {
    super('MembershipTypesQuery', notificationService);

    clubService.selectedId$.subscribe(id => {
      this.fetch(id).subscribe();
    });
    this.fetch(clubService.selectedId).subscribe();
  }
  
  get membershipTypes$() {
    return this.result$;
  }

  fetch(partitionId: string) {
    return this.fetchItems(this.api.membershipTypes_Query({partitionId}));
  }
}
