import { Injectable } from '@angular/core';
import { Api, ImportedMember, MemberDisplayItem, SlackDifferences, SlackUser } from 'app/api/api';
import { PartitionService } from 'app/app-engine/partitions/partition.service';
import { Observable } from 'rxjs';
import { FetchService } from '../infrastructure/fetchService';
import { NotificationService } from '../infrastructure/notification.service';

@Injectable({
  providedIn: 'root'
})
export class SlackMatchingService extends FetchService<SlackDifferences | null>
{
  constructor(private api: Api,
    private partitionService: PartitionService,
    notificationService: NotificationService)
  {
    super('SlackUserDifferencesQuery', notificationService);
  }

  fetch(searchString: string | null = null): Observable<any>
  {
    return this.fetchItems(this.api.slackUserDifferences_Query({ partitionId: this.partitionService.selectedId, searchString }), null, this.partitionService.selectedId);
  }

  assignSlackUser(member: MemberDisplayItem, slackUser: SlackUser): Observable<void>
  {
    return this.api.mapSlackUser_Command({
      partitionId: this.partitionService.selectedId,
      slackUserId: slackUser.id,
      memberId: member.id
    });
  }

  unassignSlackUser(member: MemberDisplayItem, slackUser: SlackUser): Observable<void>
  {
    return this.api.removeSlackUserMapping_Command({
      partitionId: this.partitionService.selectedId,
      slackUserId: slackUser.id,
      memberId: member.id
    });
  }

}
