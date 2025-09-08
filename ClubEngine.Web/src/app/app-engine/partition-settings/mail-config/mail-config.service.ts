import { Injectable } from '@angular/core';
import { Api, ExternalMailConfigurationDisplayItem, ExternalMailConfigurationUpdateItem } from 'app/api/api';
import { PartitionService } from 'app/app-engine/partitions/partition.service';
import { FetchService } from 'app/modules/admin/infrastructure/fetchService';
import { NotificationService } from 'app/modules/admin/infrastructure/notification.service';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class MailConfigService extends FetchService<ExternalMailConfigurationDisplayItem[]>
{
  constructor(private api: Api,
    private partitionService: PartitionService,
    notificationService: NotificationService)
  {
    super('ExternalMailConfigurationQuery', notificationService);
  }

  get mailConfigs$(): Observable<ExternalMailConfigurationDisplayItem[]>
  {
    return this.result$;
  }

  fetchMailConfigs(): Observable<any>
  {
    return this.fetchItems(this.api.externalMailConfiguration_Query({ partitionId: this.partitionService.selectedId }), null, this.partitionService.selectedId);
  }

  save(configs: ExternalMailConfigurationUpdateItem[]): void
  {
    this.api.saveExternalMailConfiguration_Command({ partitionId: this.partitionService.selectedId, configs })
      .subscribe();
  }
}
