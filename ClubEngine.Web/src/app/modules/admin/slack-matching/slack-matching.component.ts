import { ChangeDetectionStrategy, ChangeDetectorRef, Component } from '@angular/core';
import { SlackMatchingService } from './slack-matching.service';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { FuseCardComponent } from '@fuse/components/card';
import { TranslatePipe } from '@ngx-translate/core';
import { UpdateReadmodelComponent } from 'app/app-engine/readmodels/update-readmodel/update-readmodel.component';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MemberDisplayItem, SlackMatch, SlackUser } from 'app/api/api';
import { MemberComponent } from "../member/member.component";

@Component({
  selector: 'app-slack-matching',
  imports: [CommonModule, MatIconModule, MatButtonModule, TranslatePipe, FuseCardComponent, UpdateReadmodelComponent, MatFormFieldModule, MatInputModule, MemberComponent],
  templateUrl: './slack-matching.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SlackMatchingComponent
{
  selectedMember: MemberDisplayItem | null = null;
  selectedSlackUser: SlackUser | null = null;
  isMatch: boolean = false;
  constructor(public matchingService: SlackMatchingService,
    private changeDetector: ChangeDetectorRef) { }


  filterByQuery(searchString: string)
  {
    this.matchingService.fetch(searchString)
      .subscribe();
  }

  selectMember(member: MemberDisplayItem)
  {
    this.selectedMember = member;
    this.isMatch = false;
    this.changeDetector.markForCheck();
  }

  selectSlackUser(slackUser: SlackUser)
  {
    this.selectedSlackUser = slackUser;
    this.isMatch = false;
    this.changeDetector.markForCheck();
  }

  selectMatch(match: SlackMatch)
  {
    this.selectedMember = match.member;
    this.selectedSlackUser = match.slack;
    this.isMatch = true;
    this.changeDetector.markForCheck();
  }

  assign(member: MemberDisplayItem, slackUser: SlackUser)
  {
    this.matchingService.assignSlackUser(member, slackUser)
      .subscribe(() => this.clearSelection());
  }

  unassign(member: MemberDisplayItem, slackUser: SlackUser)
  {
    this.matchingService.unassignSlackUser(member, slackUser)
      .subscribe(() => this.clearSelection());
  }

  cancel()
  {
    this.clearSelection();
  }

  private clearSelection(): void
  {
    this.selectedMember = null;
    this.selectedSlackUser = null;
    this.isMatch = false;
    this.changeDetector.markForCheck();
  }
}
