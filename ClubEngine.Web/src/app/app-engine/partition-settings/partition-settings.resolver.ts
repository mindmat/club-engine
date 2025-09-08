import { Injectable } from '@angular/core';
import { Resolve, RouterStateSnapshot, ActivatedRouteSnapshot } from '@angular/router';
import { Observable, zip } from 'rxjs';
import { UserAccessRequestsService } from './user-access/user-access-requests.service';
import { UserAccessService } from './user-access/user-access.service';
import { UserRolesService } from './user-access/user-roles.service';
import { MailConfigService } from './mail-config/mail-config.service';
import { AccountConfigService } from './account-config/account-config.service';

@Injectable({
  providedIn: 'root'
})
export class PartitionSettingsResolver implements Resolve<boolean>
{
  constructor(private userAccessService: UserAccessService,
    private accessRequestService: UserAccessRequestsService,
    private userRolesService: UserRolesService,
    private mailConfigService: MailConfigService,
    private accountConfigService: AccountConfigService) { }

  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<any>
  {
    return zip(
      this.userAccessService.fetchUsersOfPartition(),
      this.accessRequestService.fetchRequestOfEvent(),
      this.userRolesService.fetchRoles(),
      this.mailConfigService.fetchMailConfigs(),
      this.accountConfigService.fetchConfig());
  }
}
