import { Injectable } from '@angular/core';
import { Api, ImportedMember } from 'app/api/api';
import { PartitionService } from 'app/app-engine/partitions/partition.service';

@Injectable({
  providedIn: 'root'
})
export class ImportMemberListService {

  constructor(private api: Api,
    private partitionService: PartitionService) { }

  importAllNewMembers(newMembers: ImportedMember[]) {
    this.api.importNewMembers_Command({ partitionId: this.partitionService.selectedId, newMembers })
      .subscribe();
  }
}
