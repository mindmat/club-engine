import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ClubOverviewComponent } from './club-overview.component';

describe('ClubOverviewComponent', () => {
  let component: ClubOverviewComponent;
  let fixture: ComponentFixture<ClubOverviewComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ClubOverviewComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ClubOverviewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
