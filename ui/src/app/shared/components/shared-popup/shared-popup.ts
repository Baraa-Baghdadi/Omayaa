import { Component } from '@angular/core';
import { ErrorPopup } from '../../services/error-popup';
import { NgClass, NgStyle } from '@angular/common';

@Component({
  selector: 'app-shared-popup',
  standalone: true,
  imports :[NgClass,NgStyle],
  templateUrl: './shared-popup.html',
  styleUrl: './shared-popup.scss'
})
export class SharedPopup {
message: string = '';
  isErrorVisible: boolean = false;
  isSuccessVisible: boolean = false;

  constructor(private errorPopupService: ErrorPopup) {}

  ngOnInit() {
    this.errorPopupService.error$.subscribe((message: string) => {
      this.message = message;
      this.showError();
    });

    this.errorPopupService.successSubject$.subscribe((message: string) => {
      this.message = message;
      this.showSuccess();
    });
  }

  showError() {
    this.isErrorVisible = true;
  }

  hideError() {
    this.isErrorVisible = false;
  }

  showSuccess(){
    this.isSuccessVisible = true;
  }

  hideSuccess(){
    this.isSuccessVisible = false;
  }

}
