import { Component } from '@angular/core';
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
export class ProviderPageComponent {
  currentPage: 'catalog' | 'cart' = 'catalog';

  showCart(): void {
    this.currentPage = 'cart';
  }

  showCatalog(): void {
    this.currentPage = 'catalog';
  }
}
