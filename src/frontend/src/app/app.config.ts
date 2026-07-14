import { APP_INITIALIZER, ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { importProvidersFrom } from '@angular/core';
import { OAuthModule, OAuthStorage } from 'angular-oauth2-oidc';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';

import { AuthService } from './auth/auth.service';
import { NotificationService } from './services/notification.service';
import { routes } from './app.routes';

function initializeAuth(authService: AuthService): () => Promise<void> {
  return () => authService.initAuth();
}

function initializeNotifications(notificationService: NotificationService): () => Promise<void> {
  return () => notificationService.startConnection().catch(error => {
    console.error('Failed to start notifications:', error);
    // No lanzar error para no bloquear la app si falla SignalR
  });
}

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    importProvidersFrom(OAuthModule.forRoot()),
    provideRouter(routes),
    provideHttpClient(),
    {
      provide: OAuthStorage,
      useFactory: () => localStorage,
    },
    {
      provide: APP_INITIALIZER,
      useFactory: initializeAuth,
      deps: [AuthService],
      multi: true,
    },
    {
      provide: APP_INITIALIZER,
      useFactory: initializeNotifications,
      deps: [NotificationService],
      multi: true,
    },
  ]
};
