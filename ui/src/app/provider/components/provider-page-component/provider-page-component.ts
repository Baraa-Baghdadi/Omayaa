import { Component, OnInit } from '@angular/core';
import { ProductCatalogComponent } from '../product-catalog-component/product-catalog-component';
import { CartComponent } from '../cart-component/cart-component';
import { HeaderComponent } from '../header-component/header-component';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-provider-page-component',
  imports: [ProductCatalogComponent,CartComponent,HeaderComponent,CommonModule],
  standalone:true,
  templateUrl: './provider-page-component.html',
  styleUrl: './provider-page-component.scss'
})
export class ProviderPageComponent implements OnInit {
  currentPage: 'catalog' | 'cart' = 'catalog';
  currentTheme: 'light' | 'dark' = 'dark';
  showThemeNotification = false;
  themeNotificationMessage = '';

    ngOnInit(): void {
    this.initializeTheme();
  }


  showCart(): void {
    this.currentPage = 'cart';
  }

  showCatalog(): void {
    this.currentPage = 'catalog';
  }

   toggleTheme(): void {
    this.currentTheme = this.currentTheme === 'dark' ? 'light' : 'dark';
    localStorage.setItem('theme', this.currentTheme);
    
    // Show theme change notification
    this.displayThemeNotification(this.currentTheme);
  }

  private initializeTheme(): void {
    const savedTheme = localStorage.getItem('theme') as 'light' | 'dark';
    
    if (savedTheme && (savedTheme === 'light' || savedTheme === 'dark')) {
      this.currentTheme = savedTheme;
    } else {
      // Default to light theme
      this.currentTheme = 'dark';
      localStorage.setItem('theme', this.currentTheme);
    }
  }

  private displayThemeNotification(theme: 'light' | 'dark'): void {
    this.themeNotificationMessage = theme === 'dark' 
      ? 'Switched to Dark Mode' 
      : 'Switched to Light Mode';
    
    this.showThemeNotification = true;
    
    // Hide notification after 2.5 seconds
    setTimeout(() => {
      this.showThemeNotification = false;
    }, 2500);
  }
}
