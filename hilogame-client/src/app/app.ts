import {Component, inject, OnInit} from '@angular/core';
import {FormsModule} from '@angular/forms';
import {AuthService} from '../services/auth.service';
import {RouterOutlet} from '@angular/router';

@Component({
  selector: 'app-root',
  imports: [FormsModule, RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App implements OnInit {

  private authService = inject(AuthService);
  ngOnInit(): void {
    this.authService.autoLogin();
  }

}
