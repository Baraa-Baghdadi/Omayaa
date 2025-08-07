import { Component } from '@angular/core';
import { CartService, Product } from '../../services/cart-service';
import { CommonModule, NgFor, NgIf } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'app-product-catalog-component',
  imports: [CommonModule,FormsModule,ReactiveFormsModule,NgFor],
  templateUrl: './product-catalog-component.html',
  styleUrl: './product-catalog-component.scss'
})
export class ProductCatalogComponent {
 products: Product[] = [
    { id: 1, name: 'Wireless Headphones', category: 'electronics', price: 129.99, icon: 'fas fa-headphones', badge: 'New' },
    { id: 2, name: 'Executive Office Chair', category: 'furniture', price: 299.99, icon: 'fas fa-chair', badge: 'Premium' },
    { id: 3, name: 'Adjustable Laptop Stand', category: 'office', price: 45.99, icon: 'fas fa-laptop', badge: 'Popular' },
    { id: 4, name: 'Professional Power Drill', category: 'tools', price: 89.99, icon: 'fas fa-tools', badge: 'Best Seller' },
    { id: 5, name: 'Flagship Smartphone', category: 'electronics', price: 699.99, icon: 'fas fa-mobile-alt', badge: 'Premium' },
    { id: 6, name: 'LED Desk Lamp', category: 'office', price: 34.99, icon: 'fas fa-lightbulb', badge: 'Eco-Friendly' },
    { id: 7, name: 'Wooden Filing Cabinet', category: 'furniture', price: 159.99, icon: 'fas fa-archive', badge: 'Handcrafted' },
    { id: 8, name: 'Professional Hammer Set', category: 'tools', price: 29.99, icon: 'fas fa-hammer', badge: 'Complete Set' },
    { id: 9, name: 'Bluetooth Speaker', category: 'electronics', price: 79.99, icon: 'fas fa-volume-up', badge: 'Waterproof' },
    { id: 10, name: 'Ergonomic Mouse', category: 'office', price: 24.99, icon: 'fas fa-mouse', badge: 'Comfortable' },
    { id: 11, name: 'Modern Bookshelf', category: 'furniture', price: 189.99, icon: 'fas fa-book', badge: 'Stylish' },
    { id: 12, name: 'Precision Screwdriver Set', category: 'tools', price: 19.99, icon: 'fas fa-screwdriver', badge: 'Precision' }
  ];

  filteredProducts: Product[] = [];
  currentFilter: string = 'all';
  productQuantities: { [key: number]: number } = {};
  productNotes: { [key: number]: string } = {};
  categories = ['all', 'electronics', 'office', 'furniture', 'tools'];

  constructor(private cartService: CartService) {}

  ngOnInit(): void {
    this.filteredProducts = [...this.products];
    this.initializeProductData();
  }

  initializeProductData(): void {
    this.products.forEach(product => {
      this.productQuantities[product.id] = 1;
      this.productNotes[product.id] = '';
    });
  }

  filterProducts(category: string): void {
    this.currentFilter = category;
    if (category === 'all') {
      this.filteredProducts = [...this.products];
    } else {
      this.filteredProducts = this.products.filter(product => product.category === category);
    }
  }

  changeQuantity(productId: number, change: number): void {
    let newQuantity = this.productQuantities[productId] + change;
    if (newQuantity < 1) newQuantity = 1;
    if (newQuantity > 99) newQuantity = 99;
    this.productQuantities[productId] = newQuantity;
  }

  addToCart(productId: number, buttonElement: HTMLElement): void {
    const product = this.products.find(p => p.id === productId);
    if (!product) return;

    const quantity = this.productQuantities[productId];
    const notes = this.productNotes[productId].trim();

    this.cartService.addToCart(product, quantity, notes);

    // Reset form
    this.productQuantities[productId] = 1;
    this.productNotes[productId] = '';

    // Show success feedback
    const originalText = buttonElement.innerHTML;
    buttonElement.innerHTML = '<i class="fas fa-check"></i> Added!';
    buttonElement.style.background = 'linear-gradient(135deg, #28a745, #20c997)';
    
    setTimeout(() => {
      buttonElement.innerHTML = originalText;
      buttonElement.style.background = '';
    }, 2000);
  }

  capitalizeFirst(text: string): string {
    return text.charAt(0).toUpperCase() + text.slice(1);
  }
}
