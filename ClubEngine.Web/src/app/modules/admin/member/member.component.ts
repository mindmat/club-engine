import { AsyncPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { toObservable } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { FuseCardComponent } from '@fuse/components/card';
import { MemberDisplayItem } from 'app/api/api';
import { MembershipTagComponent } from '../membership-tag/membership-tag.component';
import { NavigatorService } from '../navigator.service';

@Component({
  selector: 'app-member',
  imports: [FuseCardComponent, AsyncPipe, MembershipTagComponent, RouterLink],
  templateUrl: './member.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MemberComponent {
  constructor(public navigator: NavigatorService) {}

  readonly member = input<MemberDisplayItem | null>();
  readonly member$ = toObservable(this.member);
}
