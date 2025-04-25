import { ChangeDetectionStrategy, Component } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import { MembersService } from './members.service';
import { MemberDisplayItem } from 'app/api/api';
import { Observable } from 'rxjs';
import { AsyncPipe, NgFor } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { RouterModule } from '@angular/router';
import { MembersHistoryComponent } from "../members-history/members-history.component";
import { CdkScrollable } from '@angular/cdk/scrolling';

@Component({
  selector: 'app-members',
  imports: [TranslatePipe, AsyncPipe, MatIconModule, RouterModule, MembersHistoryComponent, MembersHistoryComponent, CdkScrollable],
  templateUrl: './members.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MembersComponent {
  constructor(private membersService: MembersService) { }

  get members$(): Observable<MemberDisplayItem[]> {
    return this.membersService.members$;
  }
}
