import { Injectable } from '@angular/core';
import { Api, ImportedMember, SlackDifferences } from 'app/api/api';
import { PartitionService } from 'app/app-engine/partitions/partition.service';
import { Observable } from 'rxjs';
import { FetchService } from '../infrastructure/fetchService';
import { NotificationService } from '../infrastructure/notification.service';

@Injectable({
  providedIn: 'root'
})
export class SlackMatchingService extends FetchService<SlackDifferences | null> {

  constructor(private api: Api,
    private partitionService: PartitionService,
    notificationService: NotificationService) {
    super('SlackUserDifferencesQuery', notificationService);
  }

  fetch(searchString: string | null = null): Observable<any> {
    return this.fetchItems(this.api.slackUserDifferences_Query({ partitionId: this.partitionService.selectedId, searchString }), null, this.partitionService.selectedId);
  }

}
