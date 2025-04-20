import { CommonModule, DatePipe } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { TranslatePipe } from '@ngx-translate/core';
import { ImportedMember, ListDifferences } from 'app/api/api';
import { FileUploadComponent } from 'app/app-engine/file-upload/file-upload.component';
import { PartitionService } from 'app/app-engine/partitions/partition.service';
import { BehaviorSubject } from 'rxjs';
import { FuseCardComponent } from "../../../../@fuse/components/card/card.component";
import { ImportMemberListService } from './import-member-list.service';
import { MembershipTagComponent } from "../membership-tag/membership-tag.component";

@Component({
  selector: 'app-import-member-list',
  imports: [CommonModule, MatIconModule, MatButtonModule, FileUploadComponent, TranslatePipe, FuseCardComponent, MembershipTagComponent],
  templateUrl: './import-member-list.component.html'
})
export class ImportMemberListComponent implements OnInit {
  public uploadUrl: string;
  differences$ = new BehaviorSubject<ListDifferences | null>(null);

  constructor(private partitionService: PartitionService,
    private service: ImportMemberListService) { }

  ngOnInit(): void {
    this.partitionService.selected$.subscribe(partition => this.uploadUrl = `api/${partition.acronym}/upload/ImportMemberListQuery`);
  }

  uploadDone(changes: ListDifferences) {
    this.differences$.next(changes);
  }

  addAllNewMembers(newMembers: ImportedMember[]) {
    this.service.importAllNewMembers(newMembers)
      .subscribe(() => { this.differences$.value.added = []; });
  }

}
