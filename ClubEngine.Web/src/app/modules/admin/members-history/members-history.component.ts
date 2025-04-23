import { Component } from '@angular/core';
import { MembersHistoryService } from './members-history.service';
import { MemberStats } from 'app/api/api';
import { Observable, tap } from 'rxjs';
import { MatMenuModule } from '@angular/material/menu';
import { AsyncPipe, CommonModule } from '@angular/common';
import { ApexOptions, NgApexchartsModule } from 'ng-apexcharts';

@Component({
  selector: 'app-members-history',
  imports: [NgApexchartsModule, MatMenuModule, AsyncPipe],
  templateUrl: './members-history.component.html'
})
export class MembersHistoryComponent {
  accountBalanceOptions: ApexOptions;

  constructor(private membersHistoryService: MembersHistoryService) {

  }

  get stats$(): Observable<MemberStats> {
    return this.membersHistoryService.stats$.pipe(
      tap(stats => {
        this.accountBalanceOptions = {
          chart: {
            animations: {
              speed: 400,
              animateGradually: {
                enabled: false,
              },
            },
            fontFamily: 'inherit',
            foreColor: 'inherit',
            width: '100%',
            height: '100%',
            type: 'area',
            sparkline: {
              enabled: true,
            },
          },
          colors: ['#A3BFFA', '#667EEA'],
          fill: {
            colors: ['#CED9FB', '#AECDFD'],
            opacity: 0.5,
            type: 'solid',
          },
          series: [{ name: 'Mitglieder', data: stats.memberCounts.map((memberCount) => { return { x: memberCount.date, y: memberCount.total } }) }],
          stroke: {
            curve: 'straight',
            width: 2,
          },
          tooltip: {
            followCursor: true,
            theme: 'dark',
            x: {
              format: 'MMM dd, yyyy',
            },
            y: {
              formatter: (value): string => value + '%',
            },
          },
          xaxis: {
            type: 'datetime',
          },
        };
      })
    );
  }
}
