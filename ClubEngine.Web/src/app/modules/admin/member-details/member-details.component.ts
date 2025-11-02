import { AsyncPipe, DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
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
  imports: [AsyncPipe, MatIconModule, FuseCardComponent, TranslatePipe, MembershipTagComponent, DecimalPipe, DatePeriodPipe, UpdateReadmodelComponent],
  templateUrl: './member-details.component.html',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MemberDetailsComponent {
  MembershipFeeState = MembershipFeeState;
  constructor(private memberService: MemberDetailsService) {
    this.memberService.refresh();
  }

  get member$(): Observable<MemberDetails> {
    return this.memberService.result$;
  }
}
