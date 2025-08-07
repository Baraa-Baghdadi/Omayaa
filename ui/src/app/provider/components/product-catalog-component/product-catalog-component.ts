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
  { id: 1, name: 'سماعات رأس لاسلكية', category: 'الكترونيات', price: 129.99, icon: 'fas fa-headphones', badge: 'جديد' },
  { id: 2, name: 'كرسي مكتب تنفيذي فاخر', category: 'أثاث', price: 299.99, icon: 'fas fa-chair', badge: 'فاخر' },
  { id: 3, name: 'كرسي مكتب تنفيذي', category: 'مكتب', price: 45.99, icon: 'fas fa-laptop', badge: 'رائج' },
  { id: 4, name: 'مثقاب كهربائي احترافي', category: 'أدوات', price: 89.99, icon: 'fas fa-tools', badge: 'الأكثر مبيعًا' },
  { id: 5, name: 'هاتف ذكي رائد', category: 'الكترونيات', price: 699.99, icon: 'fas fa-mobile-alt', badge: 'فاخر' },
  { id: 6, name: 'مصباح مكتبي LED', category: 'مكتب', price: 34.99, icon: 'fas fa-lightbulb', badge: 'صديق للبيئة' },
  { id: 7, name: 'خزانة ملفات خشبية', category: 'أثاث', price: 159.99, icon: 'fas fa-archive', badge: 'مصنوع يدويًا' },
  { id: 8, name: 'مجموعة مطارق احترافية', category: 'أدوات', price: 29.99, icon: 'fas fa-hammer', badge: 'مجموعة كاملة' },
  { id: 9, name: 'مكبر صوت بلوتوث', category: 'الكترونيات', price: 79.99, icon: 'fas fa-volume-up', badge: 'مضاد للماء' },
  { id: 10, name: 'فأرة مريحة بتصميم هندسي', category: 'مكتب', price: 24.99, icon: 'fas fa-mouse', badge: 'مريحة' },
  { id: 11, name: 'رف كتب عصري', category: 'أثاث', price: 189.99, icon: 'fas fa-book', badge: 'أنيق' },
  { id: 12, name: 'مجموعة مفكات دقيقة', category: 'أدوات', price: 19.99, icon: 'fas fa-screwdriver', badge: 'دقة عالية' }
];



  filteredProducts: Product[] = [];
  currentFilter: string = 'all';
  productQuantities: { [key: number]: number } = {};
  productNotes: { [key: number]: string } = {};
  categories = ['الكل', 'الكترونيات', 'مكتب', 'أثاث', 'أدوات'];

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
    if (category === 'الكل') {
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
