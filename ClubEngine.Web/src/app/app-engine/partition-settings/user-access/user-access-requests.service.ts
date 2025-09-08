import { Injectable } from '@angular/core';
import { AccessRequestOfPartition, Api, RequestResponse } from 'app/api/api';
import { PartitionService } from 'app/app-engine/partitions/partition.service';
import { FetchService } from 'app/modules/admin/infrastructure/fetchService';
import { NotificationService } from 'app/modules/admin/infrastructure/notification.service';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class UserAccessRequestsService extends FetchService<AccessRequestOfPartition[]>
{
  constructor(private api: Api,
    private eventService: PartitionService,
    notificationService: NotificationService)
  {
    super('AccessRequestsOfPartitionQuery', notificationService);
  }

  get requests$(): Observable<AccessRequestOfPartition[]>
  {
    return this.result$;
  }

  fetchRequestOfEvent(): Observable<AccessRequestOfPartition[]>
  {
    return this.fetchItems(this.api.accessRequestsOfPartition_Query({ partitionId: this.eventService.selectedId }), null, this.eventService.selectedId);
  }

  approveRequest(requestId: string): void
  {
    this.api.respondToRequest_Command({ partitionId: this.eventService.selectedId, accessToEventRequestId: requestId, response: RequestResponse.Granted })
      .subscribe();
  }

  denyRequest(requestId: string): void
  {
    this.api.respondToRequest_Command({ partitionId: this.eventService.selectedId, accessToEventRequestId: requestId, response: RequestResponse.Denied })
      .subscribe();
  }
}
