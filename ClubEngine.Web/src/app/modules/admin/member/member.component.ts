import { AsyncPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { toObservable } from '@angular/core/rxjs-interop';
import { FuseCardComponent } from '@fuse/components/card';
import { MemberDisplayItem } from 'app/api/api';
import { MembershipTagComponent } from '../membership-tag/membership-tag.component';

@Component({
  selector: 'app-member',
  imports: [FuseCardComponent, AsyncPipe, MembershipTagComponent],
  templateUrl: './member.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MemberComponent
{
  readonly member = input<MemberDisplayItem | null>();
  readonly member$ = toObservable(this.member);

}
