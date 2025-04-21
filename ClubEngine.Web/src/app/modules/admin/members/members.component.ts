import { ChangeDetectionStrategy, Component } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import { MembersService } from './members.service';
import { MemberDisplayItem } from 'app/api/api';
import { Observable } from 'rxjs';
import { AsyncPipe, NgFor } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-members',
  imports: [TranslatePipe, AsyncPipe, MatIconModule, RouterModule],
  templateUrl: './members.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MembersComponent {
  constructor(private membersService: MembersService) { }

  get members$(): Observable<MemberDisplayItem[]> {
    return this.membersService.members$;
  }
}
