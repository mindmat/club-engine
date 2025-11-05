import { CdkScrollable } from '@angular/cdk/scrolling';
import { AsyncPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component } from '@angular/core';
import { MatChipsModule } from '@angular/material/chips';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { RouterModule } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { MemberDisplayItem, MemberStats } from 'app/api/api';
import { UpdateReadmodelComponent } from 'app/app-engine/readmodels/update-readmodel/update-readmodel.component';
import { Observable } from 'rxjs';
import { MemberComponent } from '../member/member.component';
import { MembersHistoryComponent } from '../members-history/members-history.component';
import { MembersHistoryService } from '../members-history/members-history.service';
import { MembersService } from './members.service';

@Component({
  selector: 'app-members',
  imports: [
    TranslatePipe,
    AsyncPipe,
    MatIconModule,
    MatInputModule,
    RouterModule,
    MemberComponent,
    MembersHistoryComponent,
    MembersHistoryComponent,
    CdkScrollable,
    MatChipsModule,
    MatFormFieldModule,
    MemberComponent,
    UpdateReadmodelComponent,
  ],
  templateUrl: './members.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MembersComponent {
  filterByTypeIds: string[];
  searchString: string;
  constructor(
    private membersService: MembersService,
    private statsService: MembersHistoryService
  ) {}

  get members$(): Observable<MemberDisplayItem[]> {
    return this.membersService.members$;
  }
  get stats$(): Observable<MemberStats> {
    return this.statsService.stats$;
  }

  onSelectionChange(selectedMembershipTypeId: string[]) {
    this.filterByTypeIds = selectedMembershipTypeId;
    this.refresh();
  }

  filterByQuery(searchString: string) {
    this.searchString = searchString;
    this.refresh();
  }

  getColor(color: string) {
    return `background-color:${color};`;
  }

  refresh() {
    this.membersService.fetch(this.filterByTypeIds, this.searchString).subscribe();
  }
}
