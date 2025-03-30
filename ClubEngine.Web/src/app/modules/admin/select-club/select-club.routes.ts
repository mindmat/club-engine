import { inject } from '@angular/core';
import { Routes } from '@angular/router';
import { SelectClubService } from './select-club.service';
import { SelectClubComponent } from './select-club.component';

export default [
    {
        path: '',
        component: SelectClubComponent,
        resolve: {
            categories: () => inject(SelectClubService).fetch(),
        }
    },
] as Routes;
