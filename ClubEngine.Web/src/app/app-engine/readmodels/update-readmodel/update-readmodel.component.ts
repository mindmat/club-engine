import { Component, input, signal } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { Api } from 'app/api/api';
import { PartitionService } from 'app/app-engine/partitions/partition.service';

@Component({
  selector: 'app-update-readmodel',
  imports: [MatIconModule],
  templateUrl: './update-readmodel.component.html'
})
export class UpdateReadmodelComponent {
  constructor(private api: Api,
    private partitionService: PartitionService
  ) { }

  readonly loading = signal(false);
  queryName = input<string>();
  rowId = input<string | null>(null);

  refresh() {
    this.loading.set(true);
    this.api.updateReadModel_Command({
      queryName: this.queryName(),
      partitionId: this.partitionService.selectedId,
      rowId: this.rowId()
    })
      .subscribe(() => this.loading.set(false));
  }

}
