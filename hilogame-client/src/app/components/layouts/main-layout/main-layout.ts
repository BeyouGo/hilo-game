import {Component, inject, isDevMode, signal} from '@angular/core';
import {RouterLink, RouterLinkActive, RouterOutlet} from "@angular/router";
import {AuthService} from '../../../../services/auth.service';
import {FormsModule} from '@angular/forms';
import {AppToastComponent} from '../../generic/toast/toast';

@Component({
  selector: 'app-main-layout',
  imports: [
    RouterLink,
    RouterOutlet,
    RouterLinkActive,
    FormsModule,
    AppToastComponent,
  ],
  templateUrl: './main-layout.html',
  styleUrl: './main-layout.scss'
})
export class MainLayout {

  //Injection
  private authService = inject(AuthService);

  //Signals
  protected player = this.authService.player;
  protected isDebug = signal(isDevMode())

  //Fields
  debugUsername = 'mehdi';
  debugPassword = 'P@ssword123';

  async loginDebug(e: Event) {
    e.preventDefault();
    await this.authService.login(this.debugUsername, this.debugPassword);
  }
  logout() {
    this.authService.logout();
  }


}
