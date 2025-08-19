import { Component, OnDestroy, OnInit } from '@angular/core';
import { CartService } from '../../services/cart-service';
import { CommonModule, NgFor } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { finalize, Subject, takeUntil } from 'rxjs';
import { ProductProviderService } from '../../services/product-provider-service';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-product-catalog-component',
  imports: [CommonModule,FormsModule,ReactiveFormsModule,NgFor],
  templateUrl: './product-catalog-component.html',
  styleUrl: './product-catalog-component.scss'
})
export class ProductCatalogComponent implements OnInit {
  products: any[] = [];
  filteredProducts: any[] = [];
  currentFilter: string = '';
  currentCategoryId: string = "0"; // 0 for "All"
  productQuantities: { [key: string]: number } = {};
  productNotes: { [key: string]: string } = {};
  categories: any[] = [];
  public readonly baseUrl  = environment.API_URL;

  // Loading states
  isLoadingCategories = false;
  isLoadingProducts = false;

  constructor(
    private cartService: CartService,
    private productProviderService: ProductProviderService
  ) {}

  ngOnInit(): void {
    this.loadCategories();
    // this.subscribeToLoadingStates();
  }

  private subscribeToLoadingStates(): void {
    // Subscribe to loading states
    this.productProviderService.categoriesLoading$
      .subscribe((loading:any) => this.isLoadingCategories = loading);

    this.productProviderService.productsLoading$
      .subscribe((loading:any) => this.isLoadingProducts = loading);
  }

  private loadCategories(): void {
    this.productProviderService.getAllCategories()
      .pipe(
        finalize(() => this.isLoadingCategories = false)
      )
      .subscribe({
        next: (categories:any) => {
          // Add "All" category at the beginning
          this.categories = [
            // { id: "0", name: 'الكل' },
            ...categories
          ];
          
          // Load all products initially (first category or all products)
          if (categories.length > 0) {     
            this.loadProductsByCategory(this.categories[0].id); 
            this.currentFilter =   this.categories[0].name;         
          }
          console.log('Categories loaded:', this.categories);
        },
        error: (error:any) => {}
      });
  }

  private loadProductsByCategory(categoryId: string): void {
    this.productProviderService.getProductsByCategory(categoryId)
      .pipe(
        finalize(() => this.isLoadingProducts = false)
      )
      .subscribe({
        next: (products:any) => {
          console.log("products",products);
          this.products = products;
          this.filteredProducts = [...products];
          this.initializeProductData();
          console.log('Products loaded:', products);
        },
        error: (error:any) => {
          console.error('Error loading products:', error);
          this.products = [];
          this.filteredProducts = [];
        }
      });
  }

  initializeProductData(): void {
    this.filteredProducts.forEach(product => {
      if (!this.productQuantities[product.id]) {
        this.productQuantities[product.id] = 1;
      }
      if (!this.productNotes[product.id]) {
        this.productNotes[product.id] = '';
      }
    });
  }

  filterProducts(categoryName: string): void {
    this.currentFilter = categoryName;
    
    // Find the category by name
    const category = this.categories.find(cat => cat.name === categoryName);
    
    if (category) {
      this.currentCategoryId = category.id;
      
      if (category.id === 0) {
        // For "All" category, load products from the first actual category or all products
        // You might want to modify this logic based on your API behavior
        const firstRealCategory = this.categories.find(cat => cat.id !== 0);
        if (firstRealCategory) {
          this.loadProductsByCategory(firstRealCategory.id);
        }
      } else {
        // Load products for the selected category
        this.loadProductsByCategory(category.id);
      }
    }
  }

  changeQuantity(productId: string, change: number): void {
    let newQuantity = this.productQuantities[productId] + change;
    if (newQuantity < 1) newQuantity = 1;
    if (newQuantity > 99) newQuantity = 99;
    this.productQuantities[productId] = newQuantity;
  }

  addToCart(productId: string, buttonElement: HTMLElement): void {
    const product = this.filteredProducts.find(p => p.id === productId);
    if (!product) return;

    const quantity = this.productQuantities[productId];
    const notes = this.productNotes[productId].trim();

    this.cartService.addToCart(product, quantity, notes,productId);

    // Reset form
    this.productQuantities[productId] = 1;
    this.productNotes[productId] = '';

    // Show success feedback
    const originalText = buttonElement.innerHTML;
    buttonElement.innerHTML = '<i class="fas fa-check"></i> تمت الإضافة!';
    buttonElement.style.background = 'linear-gradient(135deg, #28a745, #20c997)';
    
    setTimeout(() => {
      buttonElement.innerHTML = originalText;
      buttonElement.style.background = '';
    }, 5000);
  }

  capitalizeFirst(text: string): string {
    return text.charAt(0).toUpperCase() + text.slice(1);
  }

    /**
   * Formats currency for display
   */
    formatCurrency(amount: number): string {
      return new Intl.NumberFormat('ar-SY', {
        style: 'currency',
        currency: 'SYP',
        minimumFractionDigits: 0,
        maximumFractionDigits: 2
      }).format(amount);
    }
}
