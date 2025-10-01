import { ChangeDetectionStrategy, Component } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import { MembershipFeesService } from './membership-fees.service';
import { MembershipFeesList, FeeStateInPeriod, Api } from 'app/api/api';
import { Observable } from 'rxjs';
import { AsyncPipe, CurrencyPipe } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { CdkScrollable } from '@angular/cdk/scrolling';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatButtonModule } from '@angular/material/button';
import { PartitionService } from 'app/app-engine/partitions/partition.service';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-membership-fees',
  imports: [
    TranslatePipe, 
    AsyncPipe, 
    CurrencyPipe,
    MatIconModule, 
    MatInputModule, 
    CdkScrollable, 
    MatFormFieldModule,
    MatCardModule,
    MatChipsModule,
    MatButtonModule
  ],
  templateUrl: './membership-fees.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MembershipFeesComponent {
  searchString: string = '';
  selectedPeriodId: string | null = null;

  constructor(
    private membershipFeesService: MembershipFeesService,
    private api: Api,
    private partitionService: PartitionService,
    private snackBar: MatSnackBar
  ) {}

  get membershipFees$(): Observable<MembershipFeesList> {
    return this.membershipFeesService.membershipFees$;
  }

  filterByQuery(searchString: string) {
    this.searchString = searchString.toLowerCase();
  }

  refresh() {
    this.membershipFeesService.fetch(this.selectedPeriodId).subscribe();
  }

  filterMembers(members: FeeStateInPeriod[]): FeeStateInPeriod[] {
    if (!this.searchString) {
      return members;
    }
    return members.filter(member => 
      member.memberName?.toLowerCase().includes(this.searchString)
    );
  }

  getTotalAmount(members: FeeStateInPeriod[]): number {
    return members.reduce((total, member) => total + (member.amountExpected || 0), 0);
  }

  getTotalPaid(members: FeeStateInPeriod[]): number {
    return members.reduce((total, member) => total + (member.amountPaid || 0), 0);
  }

  getTotalDue(members: FeeStateInPeriod[]): number {
    return members.reduce((total, member) => total + ((member.amountExpected || 0) - (member.amountPaid || 0)), 0);
  }

  upsertMembershipFees() {
    this.api.upsertMembershipFeesForPeriod_Command({
      partitionId: this.partitionService.selectedId,
      periodId: this.selectedPeriodId
    }).subscribe({
      next: () => {
        this.snackBar.open('Membership fees updated successfully', 'Close', {
          duration: 3000,
          horizontalPosition: 'right',
          verticalPosition: 'top'
        });
        this.refresh();
      },
      error: (error) => {
        this.snackBar.open('Error updating membership fees', 'Close', {
          duration: 5000,
          horizontalPosition: 'right',
          verticalPosition: 'top'
        });
        console.error('Error upserting membership fees:', error);
      }
    });
  }
}