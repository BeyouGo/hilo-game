import {CanMatchFn, Router} from '@angular/router';
import {inject} from '@angular/core';
import {AuthService} from '../../services/auth.service';

export const authGuard: CanMatchFn = (route, segments) => {
    const authService = inject(AuthService);
    const router = inject(Router);

    if (authService.isAuthenticated()) {
        return true;
    }

    // construire le returnUrl à partir de l’URL qu’on voulait atteindre
    const returnUrl = '/' + segments.map(s => s.path).join('/');

    // renvoyer un UrlTree -> Angular fera la redirection
    return router.createUrlTree(
        ['/auth/login'],
        { queryParams: { returnUrl } }
    );
};
