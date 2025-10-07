import {Component, HostListener, inject, signal} from '@angular/core';
import {FormsModule} from '@angular/forms';
import {RouterLink} from '@angular/router';
import {AuthService} from '../../../../services/auth.service';

@Component({
  selector: 'app-login',
  imports: [
    FormsModule,
    RouterLink
  ],
  templateUrl: './login.html',
  styleUrl: './login.scss'
})
export class Login {

  //Injection
  private authService = inject(AuthService);

  //Signals
  public username = signal<string>("mehdi");
  public password = signal<string>("P@ssword123");

  public async onSubmit() {
    if (!this.canSubmit()) {
      return;
    }
    await this.authService.login(this.username(), this.password());
  }

  public canSubmit() {
    return this.username() && this.password();
  }
  @HostListener('document:keydown.enter')
  public handleEnter() {
    this.onSubmit();
  }
}
