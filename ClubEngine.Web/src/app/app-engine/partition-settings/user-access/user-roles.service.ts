import { Injectable } from '@angular/core';
import { Api, RoleDescription, UserInPartitionRole } from 'app/api/api';
import { PartitionService } from 'app/app-engine/partitions/partition.service';
import { FetchService } from 'app/modules/admin/infrastructure/fetchService';
import { NotificationService } from 'app/modules/admin/infrastructure/notification.service';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class UserRolesService extends FetchService<RoleDescription[]>
{
  constructor(private api: Api,
    private partitionService: PartitionService)
  {
    super();
  }

  get roles$(): Observable<RoleDescription[]>
  {
    return this.result$;
  }

  fetchRoles(): Observable<RoleDescription[]>
  {
    return this.fetchItems(this.api.roles_Query({}), null, this.partitionService.selectedId);
  }

  setRoleOfUserInPartition(userId: string, role: UserInPartitionRole): void
  {
    this.api.setRoleOfUserInPartition_Command({ userId, role, partitionId: this.partitionService.selectedId })
      .subscribe();
  }

  removeUserFromPartition(userId: string): void
  {
    this.api.removeUserFromPartition_Command({ userId, partitionId: this.partitionService.selectedId })
      .subscribe();
  }
}
