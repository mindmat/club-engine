import { Injectable } from '@angular/core';
import { FetchService } from '../infrastructure/fetchService';
import { PartitionService } from 'app/app-engine/partitions/partition.service';
import { Api, MembershipFeesList } from 'app/api/api';
import { NotificationService } from '../infrastructure/notification.service';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class MembershipFeesService extends FetchService<MembershipFeesList> {
  constructor(private api: Api,
    private partitionService: PartitionService,
    notificationService: NotificationService) {
    super('MembershipFeesQuery', notificationService);
  }

  get membershipFees$() {
    return this.result$;
  }

  fetch(periodId: string | null = null): Observable<MembershipFeesList> {
    return this.fetchItems(this.api.membershipFees_Query({ 
      partitionId: this.partitionService.selectedId, 
      periodId 
    }));
  }
}