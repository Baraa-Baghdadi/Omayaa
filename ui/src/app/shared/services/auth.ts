import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { JwtHelperService } from '@auth0/angular-jwt';
import { environment } from '../../../enviroments/environment.development';
@Injectable({
  providedIn: 'root'
})
export class Auth {
  mainUrl = environment.API_URL + "api/Account/";
  AccessToken : BehaviorSubject<any> = new BehaviorSubject('');
  refreshToken : BehaviorSubject<any> = new BehaviorSubject('');
  currentUser : BehaviorSubject<any> = new BehaviorSubject(null);
  currentUser$ = this.currentUser.asObservable();

  constructor(private http: HttpClient) { }

  logIn(loginDto: any): Observable<any> {
    return this.http.post<any>(this.mainUrl+'Login', loginDto);
  };

  logOut() {
    localStorage.clear();
  }

  isLogin$(): boolean {
    return !!localStorage.getItem('AccessToken');
  }

  register(body:any):Observable<any>{
     return this.http.post<any>(this.mainUrl+'Register', body);
  }

  // Here For Token :
  setTokenInLocal(token: any) {
    localStorage.setItem('AccessToken', token.accessToken);
    localStorage.setItem('RefreshToken', token.refreshToken);
  }
  GetTokenFromLocal(): any {
    return localStorage.getItem("AccessToken");
  }
  GetRefreshTokenFromLocal():any {
    return localStorage.getItem('RefreshToken');
  }
  removeAllToken() {
    localStorage.removeItem('AccessToken');
    localStorage.removeItem('RefreshToken');
  }

    // Decode Token
    decodeToken() {
      const jwtHelper = new JwtHelperService();
      const token = this.GetTokenFromLocal();
      return jwtHelper.decodeToken(token);
    }
  
    // Refresh Token:
    tokenRefresh(DataFromLocal:any):Observable<any>{
      return this.http.post<any>(this.mainUrl+'Refresh',DataFromLocal);
    }



    // Get current user:
    getCurrentUser(): Observable<any> {
      return this.http.get<any>(this.mainUrl+'GetCurrentUser');
    }


    // Change Password:
    changePassword(payload:any): Observable<any>{
      return this.http.post<any>(this.mainUrl+'ChangePassword',payload);
    }

}
