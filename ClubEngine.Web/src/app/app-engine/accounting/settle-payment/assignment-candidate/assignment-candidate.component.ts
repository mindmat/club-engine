import { DecimalPipe } from '@angular/common';
import { ChangeDetectorRef, Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { RouterLink } from '@angular/router';
import { FuseCardComponent } from '@fuse/components/card';
import { TranslatePipe } from '@ngx-translate/core';
import { PaymentType } from 'app/api/api';
import { AppEngineNavigator } from 'app/app-engine/app-engine-navigator.service';

@Component({
  selector: 'app-assignment-candidate',
  templateUrl: './assignment-candidate.component.html',
  standalone: true,
  imports: [MatSlideToggleModule, MatFormFieldModule, MatInputModule, FuseCardComponent, MatIconModule, RouterLink, TranslatePipe, DecimalPipe, ReactiveFormsModule],
})
export class AssignmentCandidateComponent implements OnInit, OnChanges {
  public candidateForm: FormGroup;
  public difference: number;
  public openRegistrationAmount: number;
  public maxAmountToAssign: number;
  @Input() payment: Payment;
  @Input() candidate?: SettlementCandidate;
  @Output() assign = new EventEmitter<AssignmentRequest>();

  constructor(
    private fb: FormBuilder,
    public navigator: AppEngineNavigator,
    private changeDetectorRef: ChangeDetectorRef
  ) {}

  ngOnInit(): void {}

  ngOnChanges(changes: SimpleChanges): void {
    // Active item id
    if ('candidate' in changes) {
      this.openRegistrationAmount = this.payment.type == PaymentType.Incoming ? Math.max(0, this.candidate.amountOpen) : Math.max(0, this.candidate.amountTotal - this.candidate.amountOpen);
      this.maxAmountToAssign = Math.min(Math.max(0, this.payment.openAmount), this.openRegistrationAmount);
      this.candidateForm = this.fb.group({
        amountAssigned: [this.maxAmountToAssign, [Validators.required, Validators.min(0.01), Validators.max(this.maxAmountToAssign)]],
        acceptDifference: [false, Validators.required],
        acceptDifferenceReason: [''],
      });
      this.checkDifference(this.candidateForm.value);
      this.candidateForm.valueChanges.subscribe((values) => {
        this.checkDifference(values);
      });
    }
  }

  private checkDifference(values: any) {
    this.difference = this.candidate.amountOpen - values.amountAssigned;
    this.changeDetectorRef.markForCheck();
  }

  public emitAssign(): void {
    if (!this.candidate.locked) {
      this.assign.emit({
        paymentId: this.payment.id,
        sourceType: this.candidate.sourceType,
        sourceId: this.candidate.sourceId,
        amount: this.candidateForm.get('amountAssigned').value,
        acceptDifference: this.candidateForm.get('acceptDifference').value,
        acceptDifferenceReason: this.candidateForm.get('acceptDifferenceReason').value,
      } as AssignmentRequest);
    }
  }
}

export interface AssignmentRequest {
  paymentId: string;
  sourceType?: string;
  sourceId?: string;
  amount: number;
  acceptDifference: boolean;
  acceptDifferenceReason: string;
}

export interface SettlementCandidate {
  sourceType?: string;
  sourceId?: string;
  textPrimary?: string | null;
  textSecondary?: string | null;
  amountTotal?: number;
  amountOpen?: number;
  amountMatch: boolean;
  locked: boolean;
}

export interface Payment {
  id: string;
  type: PaymentType;
  openAmount: number;
  ignored: boolean;
  // locked: boolean;
}
