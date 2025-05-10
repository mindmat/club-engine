import { Injectable } from '@angular/core';
import { AuthService } from '@auth0/auth0-angular';
import { User } from 'app/core/user/user.types';
import { map, Observable, tap } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class UserService {
    private isAuthenticatedValue: boolean;
    constructor(private authService: AuthService) { }

    get user$(): Observable<User | null> {

        return this.authService.user$.pipe(
            tap(usr => this.isAuthenticatedValue = usr !== null),
            map(usr => (
                usr === null
                    ? null
                    : {
                        id: usr?.sub,
                        name: usr?.name,
                        email: usr?.email,
                        avatar: usr?.picture,
                        status: null
                    } as User))
        );
    }

    get isAuthenticated(): boolean {
        return this.isAuthenticatedValue;
    }
}
