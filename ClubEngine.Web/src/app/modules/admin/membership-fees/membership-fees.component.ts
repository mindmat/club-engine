import { CdkScrollable } from '@angular/cdk/scrolling';
import { AsyncPipe, DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar } from '@angular/material/snack-bar';
import { FuseCardComponent } from '@fuse/components/card';
import { TranslatePipe } from '@ngx-translate/core';
import { Api, FeeStateInPeriod, MembershipFeesList, PeriodDisplayItem } from 'app/api/api';
import { PartitionService } from 'app/app-engine/partitions/partition.service';
import { Observable, tap } from 'rxjs';
import { PeriodsService } from '../periods/periods.service';
import { MembershipFeesService } from './membership-fees.service';

@Component({
  selector: 'app-membership-fees',
  imports: [
    TranslatePipe,
    AsyncPipe,
    MatIconModule,
    MatInputModule,
    CdkScrollable,
    MatFormFieldModule,
    MatCardModule,
    MatChipsModule,
    MatButtonModule,
    MatSelectModule,
    DecimalPipe,
    FuseCardComponent,
    DecimalPipe,
  ],
  templateUrl: './membership-fees.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MembershipFeesComponent {
  searchString: string = '';
  selectedPeriodId: string | null = null;

  constructor(
    private membershipFeesService: MembershipFeesService,
    private periodsService: PeriodsService,
    private api: Api,
    private partitionService: PartitionService,
    private snackBar: MatSnackBar
  ) {}

  get membershipFees$(): Observable<MembershipFeesList> {
    return this.membershipFeesService.membershipFees$;
  }

  get periods$(): Observable<PeriodDisplayItem[]> {
    return this.periodsService.periods$.pipe(
      tap((periods) => {
        if (!this.selectedPeriodId && periods.length > 0) {
          this.selectedPeriodId = periods[0].id;
        }
      })
    );
  }

  filterByQuery(searchString: string) {
    this.searchString = searchString.toLowerCase();
  }

  onPeriodChange(periodId: string | null): void {
    this.selectedPeriodId = periodId;
    this.refresh();
  }

  refresh() {
    this.membershipFeesService.fetch(this.selectedPeriodId).subscribe();
  }

  filterMembers(members: FeeStateInPeriod[]): FeeStateInPeriod[] {
    if (!this.searchString) {
      return members;
    }
    return members.filter((member) => member.memberName?.toLowerCase().includes(this.searchString));
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
    this.api
      .upsertMembershipFeesForPeriod_Command({
        partitionId: this.partitionService.selectedId,
        periodId: this.selectedPeriodId,
      })
      .subscribe({
        next: () => {
          this.snackBar.open('Membership fees updated successfully', 'Close', {
            duration: 3000,
            horizontalPosition: 'right',
            verticalPosition: 'top',
          });
          this.refresh();
        },
        error: (error) => {
          this.snackBar.open('Error updating membership fees', 'Close', {
            duration: 5000,
            horizontalPosition: 'right',
            verticalPosition: 'top',
          });
          console.error('Error upserting membership fees:', error);
        },
      });
  }
}
