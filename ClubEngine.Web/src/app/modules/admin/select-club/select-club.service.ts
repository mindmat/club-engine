import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { FetchService } from '../infrastructure/fetchService';
import { Api, MyPartitions } from 'app/api/api';
import { NotificationService } from '../infrastructure/notification.service';

@Injectable({ providedIn: 'root' })
export class SelectClubService extends FetchService<MyPartitions> {
    constructor(private api: Api,
        notificationService: NotificationService) {
        super('ClubsQuery', notificationService);
    }

    get clubs$(): Observable<MyPartitions> {
        return this.result$;
    }

    fetch(searchString: string | null = null, showArchived: boolean = false, isAuthenticated: boolean = false): Observable<MyPartitions> {
        if (isAuthenticated) {
            return this.fetchItems(this.api.myPartitions_Query({ searchString, showArchived }));
        }
        else {
            return this.fetchItems(this.api.partitions_Query({ searchString, showArchived }));
        }
    }

    requestAccess(partitionId: string, requestText: string | null = null) {
        this.api.requestAccess_Command({ partitionId, requestText })
            .subscribe();
    }
}