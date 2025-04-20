import { ChangeDetectionStrategy, ChangeDetectorRef, Component, input, OnInit } from '@angular/core';
import { MembershipTypesService } from './membership-types.service';
import { combineLatest } from 'rxjs';
import { toObservable } from '@angular/core/rxjs-interop';

@Component({
  selector: 'app-membership-tag',
  templateUrl: './membership-tag.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MembershipTagComponent implements OnInit {
  readonly membershipTypeId = input<string>();
  readonly membershipTypeId$ = toObservable(this.membershipTypeId);
  label: string;
  style: string = "background-color:#ff6347;";

  constructor(private membershipTypesService: MembershipTypesService,
    private changeDetectorRef: ChangeDetectorRef) { }

  ngOnInit(): void {
    combineLatest([this.membershipTypesService.membershipTypes$, this.membershipTypeId$])
      .subscribe(([membershipTypes, membershipTypeId]) => {
        const membershipType = membershipTypes?.find(mst => mst.id === membershipTypeId);
        if (!membershipType) {
          this.label = '?';
          this.style = 'background-color:#ff6347;';
        }
        else {
          this.label = membershipType.name;
          const color = membershipType.color ?? '#ff6347';
          this.style = `background-color:${color};`;
        }
        this.changeDetectorRef.markForCheck();
      });

  }
}
