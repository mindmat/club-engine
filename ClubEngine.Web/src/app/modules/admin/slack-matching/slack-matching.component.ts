import { Component } from '@angular/core';
import { SlackMatchingService } from './slack-matching.service';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { FuseCardComponent } from '@fuse/components/card';
import { TranslatePipe } from '@ngx-translate/core';
import { MembershipTagComponent } from '../membership-tag/membership-tag.component';
import { ReadmodelsService } from 'app/app-engine/readmodels/update-readmodel/readmodels.service';
import { UpdateReadmodelComponent } from 'app/app-engine/readmodels/update-readmodel/update-readmodel.component';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

@Component({
  selector: 'app-slack-matching',
  imports: [CommonModule, MatIconModule, MatButtonModule, TranslatePipe, FuseCardComponent, MembershipTagComponent, UpdateReadmodelComponent, MatFormFieldModule, MatInputModule],
  templateUrl: './slack-matching.component.html'
})
export class SlackMatchingComponent {
  constructor(public matchingService: SlackMatchingService
  ) { }

  filterByQuery(searchString: string) {
    this.matchingService.fetch(searchString)
      .subscribe();
  }
}
