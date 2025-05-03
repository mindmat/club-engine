import { Injectable } from '@angular/core';
import { AuthService } from '@auth0/auth0-angular';
import { User } from 'app/core/user/user.types';
import { map, Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class UserService {
    constructor(private authService: AuthService) { }

    get user$(): Observable<User> {

        return this.authService.user$.pipe(
            map(usr => ({
                id: usr.sub,
                name: usr.name,
                email: usr.email,
                avatar: usr.picture,
                status: null
            } as User))
        );
    }

    // update(user: User): Observable<any> {
    //     return this._httpClient.patch<User>('api/common/user', { user }).pipe(
    //         map((response) => {
    //             this._user.next(response);
    //         })
    //     );
    // }
}
