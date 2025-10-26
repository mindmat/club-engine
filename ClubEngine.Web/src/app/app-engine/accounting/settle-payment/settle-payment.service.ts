import { Injectable } from '@angular/core';
import { Api, PaymentAssignments, PaymentType } from 'app/api/api';
import { PartitionService } from 'app/app-engine/partitions/partition.service';
import { FetchService } from 'app/modules/admin/infrastructure/fetchService';
import { NotificationService } from 'app/modules/admin/infrastructure/notification.service';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class SettlePaymentService extends FetchService<PaymentAssignments> {
  private _paymentId: string;
  private searchString: string;

  constructor(
    private api: Api,
    private partitionService: PartitionService,
    notificationService: NotificationService
  ) {
    super('PaymentAssignmentsQuery', notificationService);
  }

  public get paymentId(): string {
    return this._paymentId;
  }

  get candidates$(): Observable<PaymentAssignments> {
    return this.result$;
  }

  fetchCandidates(paymentId: string, searchString: string | null = null): Observable<PaymentAssignments> {
    this._paymentId = paymentId;
    return this.fetchItems(this.api.paymentAssignments_Query({ partitionId: this.partitionService.selectedId, paymentId, searchString }), paymentId, this.partitionService.selectedId);
  }

  unassign(paymentAssignmentId: string) {
    return this.api.unassignPayment_Command({ partitionId: this.partitionService.selectedId, paymentAssignmentId: paymentAssignmentId }).subscribe((x) => console.log(x));
  }

  assign(paymentType: PaymentType, paymentId: string, sourceType: string, sourceId: string, amount: number, acceptDifference: boolean, acceptDifferenceReason: string) {
    if (paymentType === PaymentType.Incoming) {
      this.api
        .assignIncomingPayment_Command({
          partitionId: this.partitionService.selectedId,
          paymentIncomingId: paymentId,
          sourceType: sourceType,
          sourceId: sourceId,
          amount: amount,
          acceptDifference: acceptDifference,
          acceptDifferenceReason: acceptDifferenceReason,
        })
        .subscribe((x) => console.log(x));
    } else {
      // this.api
      //   .assignOutgoingPayment_Command({
      //     partitionId: this.partitionService.selectedId,
      //     outgoingPaymentId: paymentId,
      //     sourceType: sourceType,
      //     sourceId: sourceId,
      //     amount: amount,
      //     acceptDifference: acceptDifference,
      //     acceptDifferenceReason: acceptDifferenceReason,
      //   })
      //   .subscribe((x) => console.log(x));
    }
  }

  assignRepayment(incomingPaymentId: string, outgoingPaymentId: string, amountToAssign: number) {
    // this.api.assignRepayment_Command({ partitionId: this.partitionService.selectedId, incomingPaymentId, outgoingPaymentId, amount: amountToAssign }).subscribe((x) => console.log(x));
  }

  assignPayoutRequest(paymentId: string, payoutRequestId: string, amountToAssign: number) {
    // this.api.assignOutgoingPayment_Command({ partitionId: this.partitionService.selectedId, outgoingPaymentId: paymentId, payoutRequestId, amount: amountToAssign }).subscribe((x) => console.log(x));
  }

  ignorePayment(paymentId: string) {
    // this.api.ignorePayment_Command({ partitionId: this.partitionService.selectedId, paymentId }).subscribe();
  }
}
