import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { MatSelectChange, MatSelectModule } from '@angular/material/select';
import { BehaviorSubject, combineLatest, Subject, takeUntil } from 'rxjs';
import { UserAccessRequestsService } from './user-access-requests.service';
import { UserAccessService } from './user-access.service';
import { UserRolesService } from './user-roles.service';
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatIconModule } from "@angular/material/icon";
import { TranslateModule } from '@ngx-translate/core';
import { AccessRequestOfPartition, RoleDescription, UserInPartitionDisplayItem, UserInPartitionRole } from 'app/api/api';

@Component({
  selector: 'app-user-access',
  templateUrl: './user-access.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [MatFormFieldModule, MatSelectModule, MatIconModule, TranslateModule]
})
export class UserAccessComponent implements OnInit
{
  usersWithAccess: UserInPartitionDisplayItem[];
  filteredUsersWithAccess: UserInPartitionDisplayItem[];
  requests: AccessRequestOfPartition[];
  filteredRequests: AccessRequestOfPartition[];
  query$: BehaviorSubject<string | null> = new BehaviorSubject(null);
  roles: RoleDescription[];
  private unsubscribeAll: Subject<any> = new Subject<any>();

  constructor(private userAccessService: UserAccessService,
    private accessRequestService: UserAccessRequestsService,
    private userRolesService: UserRolesService,
    private changeDetectorRef: ChangeDetectorRef) { }

  ngOnInit(): void
  {
    combineLatest([this.userAccessService.usersWithAccess$, this.accessRequestService.requests$, this.query$])
      .pipe(takeUntil(this.unsubscribeAll))
      .subscribe(([users, requests, query]) =>
      {
        this.usersWithAccess = users;
        this.filteredUsersWithAccess = users;
        this.requests = requests;
        this.filteredRequests = requests;

        if (query != null && query !== '')
        {
          this.filteredUsersWithAccess = this.filteredUsersWithAccess.filter(usr => usr.userDisplayName.toLowerCase().includes(query.toLowerCase()));
          this.filteredRequests = this.filteredRequests.filter(req =>
            req.firstName?.toLowerCase().includes(query.toLowerCase())
            || req.lastName?.toLowerCase().includes(query.toLowerCase())
            || req.email?.toLowerCase().includes(query.toLowerCase()));
        }

        // Mark for check
        this.changeDetectorRef.markForCheck();
      });

    this.userRolesService.roles$.subscribe((roles) =>
    {
      this.roles = roles;
      this.changeDetectorRef.markForCheck();
    });
  }

  filterUsers(query: string): void
  {
    this.query$.next(query);
  }

  lookupRoleName(role: UserInPartitionRole): string
  {
    return this.roles.find(rol => rol.role === role)?.name;
  }

  removeUser(userId: string): void
  {
    this.userRolesService.removeUserFromPartition(userId);
  }

  setRoleOfUser(change: MatSelectChange, userId: string): void
  {
    const role = change.value as UserInPartitionRole;
    this.userRolesService.setRoleOfUserInPartition(userId, role);
  }

  approveRequest(requestId: string): void
  {
    this.accessRequestService.approveRequest(requestId);
  }

  denyRequest(requestId: string): void
  {
    this.accessRequestService.denyRequest(requestId);
  }
}
