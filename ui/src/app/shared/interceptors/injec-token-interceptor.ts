import { HttpErrorResponse, HttpEvent, HttpHandler, HttpInterceptor, HttpInterceptorFn, HttpRequest } from '@angular/common/http';
import { Router } from '@angular/router';
import { catchError, Observable, switchMap, throwError } from 'rxjs';
import { Auth } from '../services/auth';
import { Injectable } from '@angular/core';
import { ErrorPopup } from '../services/error-popup';

@Injectable()
export class injecTokenInterceptor implements HttpInterceptor {
    constructor(private router: Router,private authService : Auth,
      private errorPopupService : ErrorPopup
    ) 
    { }


  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const myToken = this.authService.GetTokenFromLocal();
    const req = request.clone({
      setHeaders: { Authorization: `Bearer ${myToken}` }
    })
    return next.handle(req).pipe(
      catchError((err: HttpErrorResponse) => {
         console.log(err.error);
        if (err.status === 401) {
          let dataFromLocal = { accessToken: localStorage.getItem('AccessToken'), refreshToken: localStorage.getItem('RefreshToken') };
          return this.authService.tokenRefresh(dataFromLocal).pipe(
            switchMap((res: any) => {
              localStorage.setItem('AccessToken', res.accessToken);
              localStorage.setItem('RefreshToken', res.refreshToken);
              return next.handle(request.clone({
                setHeaders: { Authorization: `Bearer ${res.accessToken}` }
              }))
            })
          );
        }
        if (err.status == 500) { 
            return throwError(() => {
              this.errorPopupService.showError(err.error);
            });
        }
        if (err.status == 400) { 
          return throwError(() => {
            this.errorPopupService.showError("Internal Server Error.");
          });
        }
        return throwError(() => {
          this.router.navigate(['login']);
        });
      })
    )
  }
}