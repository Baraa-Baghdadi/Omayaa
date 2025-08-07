import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { Observable } from 'rxjs';
import { CartItem, CartService } from '../../services/cart-service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-cart-component',
  imports: [CommonModule,FormsModule],
  templateUrl: './cart-component.html',
  styleUrl: './cart-component.scss'
})
export class CartComponent implements OnInit {
  @Output() showCatalog = new EventEmitter<void>();

  cartItems$: Observable<CartItem[]>;
  cartItems: CartItem[] = [];
  showNotesModal = false;
  editingNotes: number | null = null;
  editNotesText = '';
  editingItemName = '';

  constructor(private cartService: CartService) {
    this.cartItems$ = this.cartService.cart$;
  }

  ngOnInit(): void {
    this.cartItems$.subscribe(items => {
      this.cartItems = items;
    });
  }

  updateQuantity(itemIndex: number, change: number): void {
    this.cartService.updateQuantity(itemIndex, change);
  }

  removeItem(itemIndex: number): void {
    this.cartService.removeItem(itemIndex);
  }

  editItemNotes(itemIndex: number): void {
    if (itemIndex < 0 || itemIndex >= this.cartItems.length) return;
    
    const item = this.cartItems[itemIndex];
    this.editingNotes = itemIndex;
    this.editNotesText = item.notes || '';
    this.editingItemName = item.name;
    this.showNotesModal = true;
  }

  saveNotes(): void {
    if (this.editingNotes === null) return;
    
    this.cartService.updateNotes(this.editingNotes, this.editNotesText.trim());
    this.closeNotesModal();
  }

  closeNotesModal(): void {
    this.showNotesModal = false;
    this.editingNotes = null;
    this.editNotesText = '';
    this.editingItemName = '';
  }

  onModalBackdropClick(event: Event): void {
    if (event.target === event.currentTarget) {
      this.closeNotesModal();
    }
  }

  onKeydown(event: KeyboardEvent): void {
    if (event.key === 'Escape') {
      this.closeNotesModal();
    } else if (event.key === 'Enter' && (event.ctrlKey || event.metaKey)) {
      this.saveNotes();
    }
  }

  getCartSummary() {
    return this.cartService.getCartSummary();
  }

  confirmOrder(): void {
    if (this.cartItems.length === 0) return;

    const summary = this.getCartSummary();
    
    setTimeout(() => {
      let orderDetails = `✅ تم تأكيد الطلب بنجاح \n 💰 المجموع الكلي: $${summary.total}`;

      alert(orderDetails);
      
      // Reset cart
      this.cartService.clearCart();
    }, 500);
  }

  continueShopping(): void {
    this.showCatalog.emit();
  }

  capitalizeFirst(text: string): string {
    return text.charAt(0).toUpperCase() + text.slice(1);
  }
}
