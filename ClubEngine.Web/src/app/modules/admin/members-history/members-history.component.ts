import { Component } from '@angular/core';
import { MembersHistoryService } from './members-history.service';
import { MemberCount, MemberCurrentCount, MembershipTypeItem, MemberStats } from 'app/api/api';
import { combineLatest, Observable, tap, map } from 'rxjs';
import { MatMenuModule } from '@angular/material/menu';
import { AsyncPipe, CommonModule } from '@angular/common';
import { ApexOptions, NgApexchartsModule } from 'ng-apexcharts';
import { MembershipTypesService } from '../membership-tag/membership-types.service';
import { TranslatePipe } from '@ngx-translate/core';

@Component({
  selector: 'app-members-history',
  imports: [NgApexchartsModule, MatMenuModule, AsyncPipe, TranslatePipe],
  templateUrl: './members-history.component.html'
})
export class MembersHistoryComponent {
  memberGraphOptions: ApexOptions;
  currentCounts: any[];
  constructor(private membersHistoryService: MembersHistoryService,
    private membershipTypesService: MembershipTypesService) {

  }

  get stats$(): Observable<MemberStats> {
    return combineLatest([this.membersHistoryService.stats$, this.membershipTypesService.membershipTypes$]).pipe(
      tap(([stats, types]) => {
        this.memberGraphOptions = {
          chart: {
            animations: {
              enabled: false
            },
            fontFamily: 'inherit',
            foreColor: 'inherit',
            width: '100%',
            height: '100%',
            type: 'area',
            sparkline: {
              enabled: true,
            },
            stacked: true,
          },
          // colors: ['#A3BFFA', '#667EEA'],
          fill: {
            // colors: ['#CED9FB', '#AECDFD'],
            opacity: 0.5,
            type: 'solid',
          },
          series: stats.memberCounts.map(mct => ({
            name: types?.find(typ => typ.id === mct.membershipTypeId)?.name ?? '?',
            type: 'area',
            color: types?.find(typ => typ.id === mct.membershipTypeId)?.color ?? '#CED9FB',
            data: mct.counts.map(tuple => ({ x: tuple.date, y: tuple.count }))
          })),
          stroke: {
            curve: 'smooth',
            width: 2,
          },
          tooltip: {
            followCursor: true,
            theme: 'dark',
            x: {
              format: 'dd.MM.yyyy',
            },
          },
          xaxis: {
            type: 'datetime',
          },
        };
        this.currentCounts = stats.currentCounts
          .filter(mct => mct.showInOverview)
          .map(mct => ({
            name: types?.find(typ => typ.id === mct.membershipTypeId)?.name ?? '?',
            count: mct.count
          }))
      }),
      map(([stats, _]) => { return stats }),
    );
  }

  getMembershipTypeName(membershipTypeId: string) {
    this.membershipTypesService.current?.find(mst => mst.id === membershipTypeId)?.name;
  }

}
