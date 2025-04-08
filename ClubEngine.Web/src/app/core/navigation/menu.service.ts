import { Injectable } from '@angular/core';
import { Api, MenuNodeContent } from 'app/api/api';
import { PartitionService } from 'app/app-engine/partitions/partition.service';
import { FetchService } from 'app/modules/admin/infrastructure/fetchService';
import { NotificationService } from 'app/modules/admin/infrastructure/notification.service';
import { Observable } from 'rxjs';

@Injectable({
    providedIn: 'root'
})
export class MenuService extends FetchService<MenuNodeContent[]>
{
    constructor(private api: Api,
        partitionService: PartitionService,
        notificationService: NotificationService)
    {
        super('MenuNodesQuery', notificationService);
        partitionService.selectedId$.subscribe(id => this.fetchMenuItems(id));
    }

    get nodeContents$(): Observable<MenuNodeContent[]>
    {
        return this.result$;
    }

    private fetchMenuItems(partitionId?: string)
    {
        if (!partitionId)
        {
            return;
        }
        return this.fetchItems(this.api.menuNodes_Query({ partitionId }), null, partitionId)
            .subscribe();
    }
}
