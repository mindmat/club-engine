import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { FetchService } from '../infrastructure/fetchService';
import { Api, ClubListItem } from 'app/api/api';
import { NotificationService } from '../infrastructure/notification.service';

@Injectable({ providedIn: 'root' })
export class SelectClubService extends FetchService<ClubListItem[]> {
    constructor(private api: Api,
        notificationService: NotificationService) {
        super('ClubsQuery', notificationService);
    }

    get clubs$(): Observable<ClubListItem[]> {
        return this.result$;
    }

    fetch(): Observable<any> {
        return this.fetchItems(this.api.clubs_Query({}));
    }
}

