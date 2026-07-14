import { AuthConfig } from 'angular-oauth2-oidc';

export const authConfig: AuthConfig = {
  issuer: 'http://localhost:8080/realms/forza-capstone',
  clientId: 'forza-frontend',
  responseType: 'code',
  redirectUri: window.location.origin,
  postLogoutRedirectUri: `${window.location.origin}/login`,
  scope: 'openid profile email',
  strictDiscoveryDocumentValidation: false,
  showDebugInformation: true,
  requireHttps: false,
};
