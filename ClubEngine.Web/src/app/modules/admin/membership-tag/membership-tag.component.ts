import { ChangeDetectionStrategy, ChangeDetectorRef, Component, input, OnInit } from '@angular/core';
import { MembershipTypesService } from './membership-types.service';
import { combineLatest } from 'rxjs';
import { toObservable } from '@angular/core/rxjs-interop';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-membership-tag',
  templateUrl: './membership-tag.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MembershipTagComponent implements OnInit {
  readonly membershipTypeId = input<string | null>();
  readonly membershipTypeId$ = toObservable(this.membershipTypeId);
  label: string;
  style: string;
  inactiveColor: string = '#d6d3d1';

  constructor(private membershipTypesService: MembershipTypesService,
    private changeDetectorRef: ChangeDetectorRef,
    private translate: TranslateService) { }

  ngOnInit(): void {
    combineLatest([this.membershipTypesService.membershipTypes$, this.membershipTypeId$])
      .subscribe(([membershipTypes, membershipTypeId]) => {
        const membershipType = membershipTypes?.find(mst => mst.id === membershipTypeId);
        if (!membershipType) {
          this.label = this.translate.instant('Inactive');
          this.style = `background-color:${this.inactiveColor};`;
        }
        else {
          this.label = membershipType.name;
          const color = membershipType.color ?? this.inactiveColor;
          this.style = `background-color:${color};`;
        }
        this.changeDetectorRef.markForCheck();
      });

  }
}
