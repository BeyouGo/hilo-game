import {inject, Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {Player} from '../../app/models/player.model';
import {AuthTokens} from '../../app/models/auth.model';
import {ApiService} from './api.service';
import {first, firstValueFrom, lastValueFrom} from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthApiService {

  private api = inject(ApiService);

  public login(username: string, password: string) {
    return firstValueFrom(this.api.post<AuthTokens>("auth/login", {
      Username: username,
      Password: password,
    }));
  }

  getPlayer() {
    return this.api.get<Player>("auth/me")
  }

  register(username: string, password: string) {
    return firstValueFrom(this.api.post<boolean>("auth/register", {
      Username: username,
      Password: password,
    }));
  }
}
