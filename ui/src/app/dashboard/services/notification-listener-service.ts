import { Injectable } from '@angular/core';
import { NotificationService } from './notification-service';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class NotificationListenerService {
    reciveCustomerCreateBookingListener = new BehaviorSubject<boolean>(false);
  unreadNotificationCount = new BehaviorSubject<number>(0);
  unreadNotificationCount$ = this.unreadNotificationCount.asObservable();
  unReadingNotificationList = new BehaviorSubject<{}>({});
  NotificationList = new BehaviorSubject<any | null>(null);
  NotificationList$ = this.NotificationList.asObservable();
  constructor(private notificationsService : NotificationService) { }
  
  increaseCount(){
    this.unreadNotificationCount.next(this.unreadNotificationCount.value + 1); 
  }
  makeAllMsgAsReaded(){
    this.notificationsService.markAllAsRead().subscribe();
    this.unreadNotificationCount.next(0);
  }

  // get all unreaded MSG From BE:
  getUnreadedMsg(){
    this.notificationsService.getCountOfUnreadMsg().subscribe((data:any) => {
      this.unreadNotificationCount.next(data);
    });
  }

    // Get List Of Msg:
    getMsgList(){
      this.notificationsService.getListNotification()
        .subscribe((data:any) => {
          if (this.NotificationList.value != null) {
            var oldValue = this.NotificationList.value;
            data = [...oldValue,...data];
            this.NotificationList.next(data);                        
          }
          else{          
            this.NotificationList.next(data);         
          }       
      });
    }
    makeNotificationListEmpty(){
      this.NotificationList.next(null); 
    }
}
