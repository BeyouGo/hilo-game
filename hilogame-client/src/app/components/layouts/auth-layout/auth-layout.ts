import { Component } from '@angular/core';
import {RouterOutlet} from '@angular/router';
import {FormsModule, ReactiveFormsModule} from '@angular/forms';

@Component({
  selector: 'app-auth-layout',
    imports: [
        RouterOutlet,
        FormsModule,
        ReactiveFormsModule
    ],
  templateUrl: './auth-layout.html',
  styleUrl: './auth-layout.scss'
})
export class AuthLayout {

}
