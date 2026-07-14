import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RankingDashboard } from './ranking-dashboard';

describe('RankingDashboard', () => {
  let component: RankingDashboard;
  let fixture: ComponentFixture<RankingDashboard>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RankingDashboard],
    }).compileComponents();

    fixture = TestBed.createComponent(RankingDashboard);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
