import { Injectable } from '@angular/core';
import { Api, BankAccountConfiguration } from 'app/api/api';
import { PartitionService } from 'app/app-engine/partitions/partition.service';
import { FetchService } from 'app/modules/admin/infrastructure/fetchService';
import { NotificationService } from 'app/modules/admin/infrastructure/notification.service';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AccountConfigService extends FetchService<BankAccountConfiguration>
{
  constructor(private api: Api,
    private partitionService: PartitionService,
    notificationService: NotificationService)
  {
    super('BankAccountConfigurationQuery', notificationService);
  }

  get bankAccount$(): Observable<BankAccountConfiguration>
  {
    return this.result$;
  }

  fetchConfig(): Observable<any>
  {
    return this.fetchItems(this.api.bankAccountConfiguration_Query({ partitionId: this.partitionService.selectedId }), null, this.partitionService.selectedId);
  }

  save(config: BankAccountConfiguration): void
  {
    this.api.saveBankAccountConfiguration_Command({ partitionId: this.partitionService.selectedId, config })
      .subscribe();
  };
}
