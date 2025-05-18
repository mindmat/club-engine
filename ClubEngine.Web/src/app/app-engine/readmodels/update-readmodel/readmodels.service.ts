import { Injectable } from '@angular/core';
import { Api } from 'app/api/api';
import { PartitionService } from '../../partitions/partition.service';

@Injectable({
  providedIn: 'root'
})
export class ReadmodelsService {

  constructor(private api: Api,
    private partitionService: PartitionService
  ) { }

  update(queryName: string, rowId: string | null) {
    this.api.updateReadModel_Command({ queryName, partitionId: this.partitionService.selectedId, rowId })
      .subscribe();
  }
}
