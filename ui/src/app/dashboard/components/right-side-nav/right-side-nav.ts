import { Component, OnInit } from '@angular/core';
import { ThemeService } from '../../../shared/services/theme-service';
import { Auth } from '../../../shared/services/auth';
import { Subscription } from 'rxjs';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-right-side-nav',
  imports: [CommonModule],
  templateUrl: './right-side-nav.html',
  styleUrl: './right-side-nav.scss'
})
export class RightSideNav implements OnInit {
  darkMode = false;
  currentUser: any = null;
  userSubscription: Subscription = new Subscription();

  constructor(private themeService: ThemeService,private authService:Auth) {
    this.darkMode = this.themeService.getMode() === 'dark';
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
        error: (error) => {
          console.error('Error getting current user:', error);
        }
      });
    }
  }

  toggleMode() {
    this.darkMode = !this.darkMode;
    const mode = this.darkMode ? 'dark' : 'light';
    this.themeService.setMode(mode);
  }

  logOut(){
    this.authService.logOut();
  }

  // Helper method to get display name
  getDisplayName(): string {
    return this.currentUser?.displayName || this.currentUser?.email || 'المستخدم';
  }

  // Helper method to get user email
  getUserEmail(): string {
    return this.currentUser?.email || 'user@example.com';
  }
}
