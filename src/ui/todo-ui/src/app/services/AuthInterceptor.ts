import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from './auth';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
    console.log('go there');

    const authService = inject(AuthService);
    const token = authService.getToken();
    console.log('Token ' + token);

    if (!token) return next(req);

    console.log('next');

    return next(
        req.clone({
            setHeaders: {
                Authorization: `Bearer ${token}`
            }
        })
    );
};
