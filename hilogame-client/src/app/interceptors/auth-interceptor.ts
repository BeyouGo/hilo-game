import {HttpInterceptorFn} from '@angular/common/http';
import { inject } from '@angular/core';
import {AuthService} from '../../services/auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {

    let authService = inject(AuthService);
    const accessToken = authService.tokens()?.accessToken;
    if (accessToken) {
        req = req.clone({setHeaders: {Authorization: `Bearer ${accessToken}`}});
    }

    return next(req);
};
