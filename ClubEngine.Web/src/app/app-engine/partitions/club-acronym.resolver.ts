import { Injectable } from '@angular/core';
import { Resolve, RouterStateSnapshot, ActivatedRouteSnapshot } from '@angular/router';
import { Observable, of } from 'rxjs';
import { PartitionService } from './partition.service';

@Injectable({
  providedIn: 'root'
})
export class PartitionAcronymResolver implements Resolve<boolean>
{
  constructor(private partitionService: PartitionService) { }

  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<any>
  {
    return this.partitionService.setPartitionByAcronym(route.paramMap.get('partitionAcronym'));
  }
}
