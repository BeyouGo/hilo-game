import {computed, inject, Injectable, signal} from '@angular/core';
import {Router} from '@angular/router';
import {firstValueFrom} from 'rxjs';
import {isJwtExpired} from '../app/helpers/jwt.helper';
import {Player} from '../app/models/player.model';
import {AuthApiService} from './api/auth-api.service';
import {AuthTokens, LoginDataModel} from '../app/models/auth.model';


const STORAGE_KEY = 'app.auth.tokens';
const REFRESH_SKEW_SEC = 30;      // on rafraîchit 30s avant l’expiration
const MIN_REFRESH_INTERVAL_MS = 5000; // anti-boucle

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private authApiService = inject(AuthApiService);
  private router = inject(Router);

  public tokens = signal<AuthTokens | undefined>(undefined);
  public player = signal<Player | undefined>(undefined);
  public loading = signal(false);

  readonly isAuthenticated = computed(() => {
    const t = this.tokens();
    if (!t?.accessToken) return false;
    return !isJwtExpired(t.accessToken, 0);
  });

  public async login(username: string, password: string, returnUrl?: string) {
    this.loading.set(true);
    try {
      const tokens = await this.authApiService.login(username, password);
      this.setTokens(tokens);
      await this.refreshProfile();
      await this.router.navigateByUrl(returnUrl || '/rooms');
    } finally {
      this.loading.set(false);
    }
  }

  public async register(username: string, password: string) {
    return await this.authApiService.register(username, password);
  }


  // --- State helpers
  private setTokens(tokens: AuthTokens) {
    this.tokens.set(tokens);
    sessionStorage.setItem(STORAGE_KEY, JSON.stringify(tokens));
    // this.scheduleAutoRefresh(tokens);
  }


  async refreshProfile() {
    firstValueFrom(this.authApiService.getPlayer()).then(user => {
      this.player.set(user);
    }).catch(err => {
      console.log(err);
    });
  }

  public autoLogin() {

    let storedTokens = sessionStorage.getItem(STORAGE_KEY);
    if (!storedTokens) {
      return;
    }

    const tokens = JSON.parse(storedTokens) as AuthTokens;
    this.setTokens(tokens);
    this.refreshProfile();
  }

  public logout() {
    this.tokens.set(undefined);
    this.player.set(undefined);
    sessionStorage.removeItem(STORAGE_KEY);
    this.router.navigateByUrl('/');
  }
}
