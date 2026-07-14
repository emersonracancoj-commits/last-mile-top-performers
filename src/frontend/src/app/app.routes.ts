import { Routes } from '@angular/router';
import { RankingDashboard } from './components/ranking-dashboard/ranking-dashboard';
import { authGuard } from './auth/auth.guard';
import { LoginPage } from './components/login-page/login-page';

export const routes: Routes = [
  {
    path: 'login',
    component: LoginPage,
  },
  {
    path: '',
    component: RankingDashboard,
    canActivate: [authGuard],
  },
];
