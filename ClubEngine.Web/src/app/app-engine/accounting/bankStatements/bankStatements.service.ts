import { Injectable } from '@angular/core';
import { Api, BookingsOfDay } from 'app/api/api';
import { PartitionService } from 'app/app-engine/partitions/partition.service';
import { FetchService } from 'app/modules/admin/infrastructure/fetchService';
import { NotificationService } from 'app/modules/admin/infrastructure/notification.service';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class BankStatementsService extends FetchService<BookingsOfDay[]>
{
  constructor(private api: Api, private partitionService: PartitionService, notificationService: NotificationService)
  {
    super('PaymentsByDayQuery', notificationService);
  }

  get payments$(): Observable<BookingsOfDay[]>
  {
    return this.result$;
  }

  fetchBankStatements(hideIncoming: boolean = false, hideOutgoing: boolean = false, hideSettled: boolean = false, hideIgnored: boolean = false): Observable<BookingsOfDay[]>
  {
    return this.fetchItems(this.api.paymentsByDay_Query({ partitionId: this.partitionService.selectedId, hideIncoming, hideOutgoing, hideSettled, hideIgnored }), null, this.partitionService.selectedId);
  }
}
