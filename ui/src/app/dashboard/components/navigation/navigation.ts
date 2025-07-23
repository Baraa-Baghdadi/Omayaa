import { NgClass, NgIf, NgStyle } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { RightSideNav } from '../right-side-nav/right-side-nav';
import { Subscription } from 'rxjs';
import { Auth } from '../../../shared/services/auth';

@Component({
  selector: 'app-navigation',
  imports: [NgStyle,NgClass,RightSideNav,NgIf],
  templateUrl: './navigation.html',
  styleUrl: './navigation.scss'
})
export class Navigation implements OnInit {
  darkMode = false;
  sidebarExpanded = true;
  currentUser: any = null;
  userSubscription: Subscription = new Subscription();

  /**
   *
   */
  constructor(private authService:Auth) {
  }
  
  toggleMode() {
    this.darkMode = !this.darkMode;
    document.body.classList.toggle('bg-dark', this.darkMode);
    document.body.classList.toggle('text-white', this.darkMode);
  }

   ngOnInit() {
    // Subscribe to current user changes
    this.userSubscription = this.authService.currentUser$.subscribe(user => {
      this.currentUser = user;
    });

    // If currentUser is empty, try to get it from the service
    if (!this.currentUser && this.authService.isLogin$()) {
      this.authService.getCurrentUser().subscribe({
        next: (data: any) => {
          this.authService.currentUser.next(data);
        },
        error: (error:any) => {
          console.error('Error getting current user:', error);
        }
      });
    }
  }

}
