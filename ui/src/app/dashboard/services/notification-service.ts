import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
    mainUrl = environment.API_URL + "api/NotificationِAdmin/";
  
  constructor(private http:HttpClient) { }

  getListNotification(): Observable<any> {
    return this.http.get<any>(this.mainUrl + `GetListOfAdminNotification`);
  };

  getCountOfUnreadMsg(): Observable<number> {
    return this.http.get<number>(this.mainUrl+`GetCountOfUnreadingMsgAsync`);
  };

  markAllAsRead(): Observable<any> {
    return this.http.put<any>(this.mainUrl+`MarkAllAsRead`,null);
  };

}
