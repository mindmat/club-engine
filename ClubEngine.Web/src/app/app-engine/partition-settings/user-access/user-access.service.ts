import { Injectable } from '@angular/core';
import { Api, UserInPartitionDisplayItem } from 'app/api/api';
import { PartitionService } from 'app/app-engine/partitions/partition.service';
import { FetchService } from 'app/modules/admin/infrastructure/fetchService';
import { NotificationService } from 'app/modules/admin/infrastructure/notification.service';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class UserAccessService extends FetchService<UserInPartitionDisplayItem[]>
{
  constructor(private api: Api,
    private partitionService: PartitionService,
    notificationService: NotificationService)
  {
    super('UsersOfPartitionQuery', notificationService);
  }

  get usersWithAccess$(): Observable<UserInPartitionDisplayItem[]>
  {
    return this.result$;
  }

  fetchUsersOfPartition(): Observable<any>
  {
    return this.fetchItems(this.api.usersOfPartition_Query({ partitionId: this.partitionService.selectedId }), null, this.partitionService.selectedId);
  }
}
