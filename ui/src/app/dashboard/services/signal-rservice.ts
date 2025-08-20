import { Injectable } from '@angular/core';
import { Auth } from '../../shared/services/auth';
import { NotificationListenerService } from './notification-listener-service';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../../environments/environment';


@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  
  constructor(private authService:Auth,private NotificationListener : NotificationListenerService,

  ) { }

    // Connect To SignalR:
    connect(){
      if (this.authService.isLogin$()) {
        const connection = new signalR.HubConnectionBuilder()
        .configureLogging(signalR.LogLevel.Information)
        .withUrl(environment.API_URL + 'notify',{accessTokenFactory  :  () => this.authService.GetTokenFromLocal(),
          withCredentials: true
        })
        .build();
        connection.start().then(function () {
          console.log('SignalR Connected! & connectionId:',connection.connectionId);
        }).catch(function (err) {
          return console.error(err.toString());
        });
    
        // recive message:
        connection.on("CustomerCreateOrder", (data:any) => {
          console.log("Notification",data);  
          this.NotificationListener.makeNotificationListEmpty();
          this.NotificationListener.reciveCustomerCreateBookingListener.next(true);  
          // update notification list:
          setTimeout(() => {
            this.NotificationListener.getMsgList();
          }, 1000);  
          // increase count of unread MSG:
          this.NotificationListener.increaseCount();  
        }); 
      }
    }

}
