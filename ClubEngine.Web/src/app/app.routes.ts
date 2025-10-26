import { inject } from '@angular/core';
import { Route } from '@angular/router';
import { AuthGuard } from '@auth0/auth0-angular';
import { initialDataResolver } from 'app/app.resolvers';
import { LayoutComponent } from 'app/layout/layout.component';
import { combineLatest } from 'rxjs';
import { BankStatementsComponent } from './app-engine/accounting/bankStatements/bankStatements.component';
import { BankStatementsResolver } from './app-engine/accounting/bankStatements/bankStatements.resolvers';
import { SettlePaymentComponent } from './app-engine/accounting/settle-payment/settle-payment.component';
import { SettlePaymentResolver } from './app-engine/accounting/settle-payment/settle-payment.resolver';
import { SettlePaymentsComponent } from './app-engine/accounting/settle-payments/settle-payments.component';
import { SettlePaymentsService } from './app-engine/accounting/settle-payments/settle-payments.service';
import { PartitionSettingsComponent } from './app-engine/partition-settings/partition-settings.component';
import { PartitionSettingsResolver } from './app-engine/partition-settings/partition-settings.resolver';
import { PartitionAcronymResolver } from './app-engine/partitions/club-acronym.resolver';
import { ImportMemberListComponent } from './modules/admin/import-member-list/import-member-list.component';
import { MembersHistoryService } from './modules/admin/members-history/members-history.service';
import { MembersComponent } from './modules/admin/members/members.component';
import { MembersService } from './modules/admin/members/members.service';
import { MembershipFeesComponent } from './modules/admin/membership-fees/membership-fees.component';
import { MembershipFeesService } from './modules/admin/membership-fees/membership-fees.service';
import { PeriodsService } from './modules/admin/periods/periods.service';
import { SelectClubComponent } from './modules/admin/select-club/select-club.component';
import { SelectClubService } from './modules/admin/select-club/select-club.service';
import { SlackMatchingComponent } from './modules/admin/slack-matching/slack-matching.component';
import { SlackMatchingService } from './modules/admin/slack-matching/slack-matching.service';

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

  // Landing routes
  {
    path: '',
    component: LayoutComponent,
    data: {
      layout: 'empty',
    },
    children: [{ path: 'home', loadChildren: () => import('app/modules/landing/home/home.routes') }],
  },

  // Admin routes
  {
    path: '',
    component: LayoutComponent,
    resolve: {
      initialData: initialDataResolver,
    },
    children: [
      {
        path: 'select-club',
        component: SelectClubComponent,
        resolve: {
          categories: () => inject(SelectClubService).fetch(),
        },
      },
      {
        path: ':partitionAcronym',
        canActivate: [AuthGuard],
        canActivateChild: [AuthGuard],
        resolve: { initialData: PartitionAcronymResolver },

        children: [
          { path: '', pathMatch: 'full', redirectTo: 'members' },
          {
            path: 'members',
            component: MembersComponent,
            resolve: {
              members: () => {
                return combineLatest([inject(MembersService).fetch(), inject(MembersHistoryService).fetch()]);
              },
            },
          },
          {
            path: 'import-member-list',
            component: ImportMemberListComponent,
          },
          {
            path: 'slack-matching',
            component: SlackMatchingComponent,
            resolve: {
              differences: () => inject(SlackMatchingService).fetch(),
            },
          },
          {
            path: 'accounting',
            children: [
              {
                path: 'bank-statements',
                component: BankStatementsComponent,
                resolve: { initialData: BankStatementsResolver },
              },
              {
                path: 'membership-fees',
                component: MembershipFeesComponent,
                resolve: {
                  fees: () => inject(MembershipFeesService).fetch(),
                  periods: () => inject(PeriodsService).fetch(),
                },
              },
              {
                path: 'settle-payments',
                component: SettlePaymentsComponent,
                resolve: {
                  statements: () => inject(SettlePaymentsService).fetchBankStatements(),
                },
                children: [
                  {
                    path: ':id',
                    component: SettlePaymentComponent,
                    resolve: { initialData: SettlePaymentResolver },
                  },
                ],
              },
            ],
          },
          {
            path: 'admin',
            children: [
              {
                path: 'settings',
                component: PartitionSettingsComponent,
              },
            ],
            resolve: {
              initialData: PartitionSettingsResolver,
            },
          },
        ],
      },
    ],
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
