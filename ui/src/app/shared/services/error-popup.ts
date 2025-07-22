import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ErrorPopup {
   private errorSubject = new Subject<string>();
  error$ = this.errorSubject.asObservable();

  private successSubject = new Subject<string>();
  successSubject$ = this.successSubject.asObservable();

  showError(message: string) {
    this.errorSubject.next(message);
  }

  showSuccess(){
    this.successSubject.next("Done");
  }

}
