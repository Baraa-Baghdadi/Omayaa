import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { CartService } from '../../services/cart-service';
import { Observable, Subscription } from 'rxjs';
import { CommonModule } from '@angular/common';
import { Auth } from '../../../shared/services/auth';

@Component({
  selector: 'app-header-component',
  imports: [CommonModule],
  templateUrl: './header-component.html',
  styleUrl: './header-component.scss'
})
export class HeaderComponent implements OnInit {
@Output() showCart = new EventEmitter<void>();
  @Output() logout = new EventEmitter<void>();

  currentUser: any = null;
  userSubscription: Subscription = new Subscription();

  cartCount$: Observable<number>;
  showUserMenu = false;


  constructor(private cartService: CartService,private authService:Auth) {
    this.cartCount$ = this.cartService.cartCount$;
  }

  onCartClick(): void {
    this.showCart.emit();
  }

  toggleUserMenu(): void {
    this.showUserMenu = !this.showUserMenu;
  }

  closeUserMenu(): void {
    this.showUserMenu = false;
  }

  onLogout(): void {
      this.closeUserMenu();
      this.authService.logOut();
  }

  onProfile(): void {
    this.closeUserMenu();
  }


    // Helper method to get display name
  getDisplayName(): string {
    return this.currentUser?.displayName || this.currentUser?.email || 'المستخدم';
  }

  // Helper method to get user email
  getUserEmail(): string {
    return this.currentUser?.email || 'user@example.com';
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
    // Subscribe to current user changes
    this.userSubscription = this.authService.currentUser$.subscribe(user => {
      this.currentUser = user;
    });
  }
}

