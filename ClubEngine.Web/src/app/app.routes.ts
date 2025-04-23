import { Route } from '@angular/router';
import { initialDataResolver } from 'app/app.resolvers';
import { AuthGuard } from 'app/core/auth/guards/auth.guard';
import { NoAuthGuard } from 'app/core/auth/guards/noAuth.guard';
import { LayoutComponent } from 'app/layout/layout.component';
import { SelectClubComponent } from './modules/admin/select-club/select-club.component';
import { inject } from '@angular/core';
import { SelectClubService } from './modules/admin/select-club/select-club.service';
import { PartitionAcronymResolver } from './app-engine/partitions/club-acronym.resolver';
import { ImportMemberListComponent } from './modules/admin/import-member-list/import-member-list.component';
import { MembersComponent } from './modules/admin/members/members.component';
import { MembersService } from './modules/admin/members/members.service';
import { MembersHistoryService } from './modules/admin/members-history/members-history.service';
import { combineLatest } from 'rxjs';

// @formatter:off
/* eslint-disable max-len */
/* eslint-disable @typescript-eslint/explicit-function-return-type */
export const appRoutes: Route[] = [

    // Redirect empty path to '/select-club'
    { path: '', pathMatch: 'full', redirectTo: 'select-club' },

    // Redirect signed-in user to the '/select-club'
    //
    // After the user signs in, the sign-in page will redirect the user to the 'signed-in-redirect'
    // path. Below is another redirection for that path to redirect the user to the desired
    // location. This is a small convenience to keep all main routes together here on this file.
    { path: 'signed-in-redirect', pathMatch: 'full', redirectTo: 'select-club' },

    // Auth routes for guests
    {
        path: '',
        canActivate: [NoAuthGuard],
        canActivateChild: [NoAuthGuard],
        component: LayoutComponent,
        data: {
            layout: 'empty'
        },
        children: [
            { path: 'confirmation-required', loadChildren: () => import('app/modules/auth/confirmation-required/confirmation-required.routes') },
            { path: 'sign-in', loadChildren: () => import('app/modules/auth/sign-in/sign-in.routes') },
            { path: 'sign-up', loadChildren: () => import('app/modules/auth/sign-up/sign-up.routes') }
        ]
    },

    // Auth routes for authenticated users
    {
        path: '',
        canActivate: [AuthGuard],
        canActivateChild: [AuthGuard],
        component: LayoutComponent,
        data: {
            layout: 'empty'
        },
        children: [
            { path: 'sign-out', loadChildren: () => import('app/modules/auth/sign-out/sign-out.routes') },
            { path: 'unlock-session', loadChildren: () => import('app/modules/auth/unlock-session/unlock-session.routes') }
        ]
    },

    // Landing routes
    {
        path: '',
        component: LayoutComponent,
        data: {
            layout: 'empty'
        },
        children: [
            { path: 'home', loadChildren: () => import('app/modules/landing/home/home.routes') },
        ]
    },

    // Admin routes
    {
        path: '',
        canActivate: [AuthGuard],
        canActivateChild: [AuthGuard],
        component: LayoutComponent,
        resolve: {
            initialData: initialDataResolver
        },
        children: [
            {
                path: 'select-club',
                component: SelectClubComponent,
                resolve: {
                    categories: () => inject(SelectClubService).fetch(),
                }
            },
            {
                path: ':partitionAcronym',
                // canActivate: [AuthGuard],
                // canActivateChild: [AuthGuard],
                resolve: { initialData: PartitionAcronymResolver },

                children: [

                    { path: '', pathMatch: 'full', redirectTo: 'members' },
                    {
                        path: 'members',
                        component: MembersComponent,
                        resolve: {
                            members: () => {
                                return combineLatest([inject(MembersService).fetch(),
                                inject(MembersHistoryService).fetch()]);
                            }
                        }
                    },
                    {
                        path: 'import-member-list',
                        component: ImportMemberListComponent,
                    }
                ]
            }
        ]
    },




    // Landing routes
    // {
    //     path: '',
    //     component: LayoutComponent,
    //     data: {
    //         layout: 'empty'
    //     },
    //     children: [
    //         { path: 'home', loadChildren: () => import('app/modules/landing/home/home.module').then(m => m.LandingHomeModule) },
    //     ]
    // },

];
