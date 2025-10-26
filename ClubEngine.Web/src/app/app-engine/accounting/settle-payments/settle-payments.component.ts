import { DatePipe, DecimalPipe, NgClass } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { MatCheckbox, MatCheckboxChange } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatDrawer, MatDrawerContainer, MatDrawerContent } from '@angular/material/sidenav';
import { RouterModule } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { BookingsOfDay, CreditDebit, PaymentDisplayItem } from 'app/api/api';
import { AppEngineNavigator } from 'app/app-engine/app-engine-navigator.service';
import { BehaviorSubject, combineLatest, debounceTime, distinctUntilChanged, Subject, switchMap, takeUntil } from 'rxjs';
import { SettlePaymentsService } from './settle-payments.service';

@Component({
  selector: 'app-settle-payments',
  templateUrl: './settle-payments.component.html',
  imports: [RouterModule, MatIconModule, MatDrawerContainer, MatDrawer, MatDrawerContent, MatFormFieldModule, MatInputModule, MatCheckbox, TranslatePipe, DatePipe, NgClass, DecimalPipe],
})
export class SettlePaymentsComponent implements OnInit {
  private unsubscribeAll: Subject<any> = new Subject<any>();
  daysWithBookings: BookingsOfDay[];
  CreditDebit = CreditDebit;
  selectedBooking: PaymentDisplayItem;

  filters: {
    query$: BehaviorSubject<string>;
    hideIncoming$: BehaviorSubject<boolean>;
    hideOutgoing$: BehaviorSubject<boolean>;
    hideSettled$: BehaviorSubject<boolean>;
    hideIgnored$: BehaviorSubject<boolean>;
  } = {
    query$: new BehaviorSubject(''),
    hideIncoming$: new BehaviorSubject(false),
    hideOutgoing$: new BehaviorSubject(true),
    hideSettled$: new BehaviorSubject(true),
    hideIgnored$: new BehaviorSubject(true),
  };

  constructor(
    private service: SettlePaymentsService,
    private navigator: AppEngineNavigator,
    private changeDetectorRef: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.service.payments$.pipe(takeUntil(this.unsubscribeAll)).subscribe((daysWithBookings: BookingsOfDay[]) => {
      this.daysWithBookings = daysWithBookings;
      if (!this.selectedBooking && daysWithBookings.length > 0 && daysWithBookings[0].bookings.length > 0) {
        this.selectBooking(daysWithBookings[0].bookings[0]);
      } else if (!!this.selectedBooking) {
        let selectedItemInNewList = daysWithBookings.flatMap((d) => d.bookings).find((b) => b.id === this.selectedBooking.id);
        if (!selectedItemInNewList) {
          // previously selected item is not in the current list anymore -> select another
          this.selectBooking(daysWithBookings[0].bookings[0]);
        } else {
          // select again - at the initial selection the list might not have been present yet
          this.selectBooking(selectedItemInNewList);
        }
      }

      // Mark for check
      this.changeDetectorRef.markForCheck();
    });

    // Filter
    combineLatest([this.filters.query$, this.filters.hideIncoming$, this.filters.hideOutgoing$, this.filters.hideSettled$, this.filters.hideIgnored$])
      .pipe(
        debounceTime(200),
        takeUntil(this.unsubscribeAll),
        distinctUntilChanged(),
        // tap(([query, hideIncoming, hideOutgoing, hideSettled, hideIgnored]) => console.log(query)),
        switchMap(([query, hideIncoming, hideOutgoing, hideSettled, hideIgnored]) => this.service.fetchBankStatements(query, hideIncoming, hideOutgoing, hideSettled, hideIgnored))
      )
      .subscribe();
  }

  selectBooking(booking: PaymentDisplayItem) {
    this.selectedBooking = booking;
    this.navigator.goToSettlePaymentUrl(booking.id);

    this.changeDetectorRef.markForCheck();
  }

  filterByQuery(query: string): void {
    this.filters.query$.next(query);
  }

  toggleIncoming(change: MatCheckboxChange): void {
    this.filters.hideIncoming$.next(change.checked);
  }

  toggleOutgoing(change: MatCheckboxChange): void {
    this.filters.hideOutgoing$.next(change.checked);
  }

  toggleSettled(change: MatCheckboxChange): void {
    this.filters.hideSettled$.next(change.checked);
  }

  toggleIgnored(change: MatCheckboxChange): void {
    this.filters.hideIgnored$.next(change.checked);
  }
}
