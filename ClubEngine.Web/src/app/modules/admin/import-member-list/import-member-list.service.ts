import { Injectable } from '@angular/core';
import { Api, ImportedMember } from 'app/api/api';
import { PartitionService } from 'app/app-engine/partitions/partition.service';
import { Observable, Subscription } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ImportMemberListService {

  constructor(private api: Api,
    private partitionService: PartitionService) { }

  importAllNewMembers(newMembers: ImportedMember[]): Observable<void> {
    return this.api.importNewMembers_Command({ partitionId: this.partitionService.selectedId, newMembers });
  }
}
