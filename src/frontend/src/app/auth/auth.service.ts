import { computed, inject, Injectable, signal } from '@angular/core';
import { OAuthEvent, OAuthService } from 'angular-oauth2-oidc';
import { authConfig } from './auth.config';
import { filter } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly oauthService = inject(OAuthService);

  readonly token = signal<string | null>(null);
  readonly isAuthenticated = computed(() => !!this.token());

  constructor() {
    this.bindTokenEvents();
  }

  async initAuth(): Promise<void> {
    this.oauthService.configure(authConfig);
    await this.oauthService.loadDiscoveryDocumentAndTryLogin();
    this.syncToken();
  }

  login(): void {
    this.oauthService.initCodeFlow();
  }

  hasValidToken(): boolean {
    return this.oauthService.hasValidAccessToken() || !!this.token();
  }

  logout(): void {
    this.oauthService
      .revokeTokenAndLogout({
        post_logout_redirect_uri: `${window.location.origin}/login`,
      })
      .then(() => {
        this.clearUserState();
        window.location.assign('/login');
      })
      .catch(() => {
        this.clearUserState();
        window.location.assign('/login');
      });
  }

  getAccessToken(): string | null {
    return this.token();
  }

  private bindTokenEvents(): void {
    this.oauthService.events
      .pipe(
        filter((event: OAuthEvent) =>
          event.type === 'token_received' || event.type === 'token_refreshed' || event.type === 'logout'
        )
      )
      .subscribe(() => {
        this.syncToken();
      });
  }

  private syncToken(): void {
    this.token.set(this.oauthService.getAccessToken() || null);
  }

  private clearUserState(): void {
    this.token.set(null);
  }
}
