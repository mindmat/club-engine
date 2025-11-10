import { AsyncPipe, DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { FuseCardComponent } from '@fuse/components/card';
import { TranslatePipe } from '@ngx-translate/core';
import { MemberDetails, MembershipFeeState } from 'app/api/api';
import { UpdateReadmodelComponent } from 'app/app-engine/readmodels/update-readmodel/update-readmodel.component';
import { Observable } from 'rxjs';
import { DatePeriodPipe } from '../infrastructure/date-period.pipe';
import { MembershipTagComponent } from '../membership-tag/membership-tag.component';
import { MemberDetailsService } from './member-details.service';

@Component({
  selector: 'app-member-details',
  imports: [
    AsyncPipe,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatTooltipModule,
    ReactiveFormsModule,
    FuseCardComponent,
    TranslatePipe,
    MembershipTagComponent,
    DecimalPipe,
    DatePeriodPipe,
    UpdateReadmodelComponent,
  ],
  templateUrl: './member-details.component.html',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MemberDetailsComponent {
  MembershipFeeState = MembershipFeeState;
  editingFeeOverride = false;
  feeOverrideForm: FormGroup;

  constructor(
    private memberService: MemberDetailsService,
    private fb: FormBuilder,
    private snackBar: MatSnackBar
  ) {
    this.memberService.refresh();
    this.feeOverrideForm = this.fb.group({
      amount: [null, [Validators.min(0.01)]],
    });
  }

  get member$(): Observable<MemberDetails> {
    return this.memberService.result$;
  }

  editFeeOverride(currentValue: number | null): void {
    this.editingFeeOverride = true;
    this.feeOverrideForm.patchValue({
      amount: currentValue,
    });
  }

  cancelFeeOverrideEdit(): void {
    this.editingFeeOverride = false;
    this.feeOverrideForm.reset();
  }

  saveFeeOverride(memberId: string | null): void {
    if (this.feeOverrideForm.valid) {
      const amount = this.feeOverrideForm.get('amount')?.value;
      this.memberService.updateFeeOverride(memberId, amount).subscribe({
        next: () => {
          this.editingFeeOverride = false;
          this.snackBar.open('Fee override updated successfully', 'Close', {
            duration: 3000,
            horizontalPosition: 'right',
            verticalPosition: 'top',
          });
          this.memberService.refresh();
        },
        error: (error) => {
          this.snackBar.open('Error updating fee override', 'Close', {
            duration: 5000,
            horizontalPosition: 'right',
            verticalPosition: 'top',
          });
          console.error('Error updating fee override:', error);
        },
      });
    }
  }
}
