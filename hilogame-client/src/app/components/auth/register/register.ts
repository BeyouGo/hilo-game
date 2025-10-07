import {Component, inject, signal} from '@angular/core';
import {FormsModule} from '@angular/forms';
import {RouterLink} from "@angular/router";
import {AuthApiService} from '../../../../services/api/auth-api.service';
import {lastValueFrom} from 'rxjs';

@Component({
  selector: 'app-register',
  imports: [
    FormsModule,
    RouterLink
  ],
  templateUrl: './register.html',
  styleUrl: './register.scss'
})
export class Register {

  private authApiService = inject(AuthApiService);

  public username = signal<string>("mehdi");
  public password = signal<string>("P@ssword123");

  public async onRegister() {
    debugger
    await this.authApiService.register(this.username(), this.password());
  }
  public canSubmit() {
    return this.username() && this.password();
  }
}
