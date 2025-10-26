import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { AppEngineNavigator } from 'app/app-engine/app-engine-navigator.service';
import { PartitionService } from 'app/app-engine/partitions/partition.service';

@Injectable({
  providedIn: 'root',
})
export class NavigatorService implements AppEngineNavigator {
  constructor(
    private router: Router,
    private partitionService: PartitionService
  ) {}
  public getSourceUrl(sourceType: string, sourceId: string): string {
    if (sourceType === 'MembershipFees') {
      return `/${this.partitionService.selected.acronym}/${sourceType}/${sourceId}`;
    }
    return this.getOverviewUrl();
  }

  getMemberUrl(memberId: string): string {
    return `/${this.partitionService.selected.acronym}/members/${memberId}`;
  }

  goToMember(memberId: string): void {
    this.router.navigate(['/', this.partitionService.selected.acronym, 'members', memberId]);
  }

  goToSettlePaymentUrl(paymentId: string): void {
    this.router.navigate([this.getSettlePaymentUrl(paymentId)]);
  }

  // getDuePaymentUrl(): string
  // {
  //   return `/${this.eventService.selected.acronym}/accounting/due-payments`;
  // }

  getOverviewUrl(): string {
    return `/${this.partitionService.selected.acronym}/overview`;
  }

  getSettlePaymentUrl(paymentId: string): string {
    return `/${this.partitionService.selected.acronym}/accounting/settle-payments/${paymentId}`;
  }

  getAutoMailTemplatesUrl(): string {
    return `/${this.partitionService.selected.acronym}/mailing/auto-mail-templates`;
  }
}
