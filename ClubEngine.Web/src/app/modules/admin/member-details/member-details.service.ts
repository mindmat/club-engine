import { Injectable } from '@angular/core';
import { Api, MemberDetails } from 'app/api/api';
import { PartitionService } from 'app/app-engine/partitions/partition.service';
import { Observable } from 'rxjs';
import { FetchService } from '../infrastructure/fetchService';
import { NotificationService } from '../infrastructure/notification.service';

@Injectable({
  providedIn: 'root',
})
export class MemberDetailsService extends FetchService<MemberDetails> {
  constructor(
    private api: Api,
    private partitionService: PartitionService,
    notificationService: NotificationService
  ) {
    super('MemberQuery', notificationService);
  }

  get member$() {
    return this.result$;
  }

  fetch(memberId: string | null = null): Observable<MemberDetails> {
    return this.fetchItems(this.api.member_Query({ partitionId: this.partitionService.selectedId, memberId }), memberId, this.partitionService.selectedId);
  }
}
