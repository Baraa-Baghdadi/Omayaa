import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';


@Injectable({
  providedIn: 'root'
})

export class CartService {
   private cartItems: any[] = [];
  private cartSubject = new BehaviorSubject<any[]>([]);
  public cart$ = this.cartSubject.asObservable();

  private cartCountSubject = new BehaviorSubject<number>(0);
  public cartCount$ = this.cartCountSubject.asObservable();

  constructor() {}

  addToCart(product: any, quantity: number, notes: string): void {
    const cartItemId = `${product.id}_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
    
    const cartItem: any = {
      ...product,
      quantity,
      notes,
      cartItemId
    };

    this.cartItems.push(cartItem);
    this.updateCart();
  }

  updateQuantity(itemIndex: any, change: any): void {
    if (itemIndex < 0 || itemIndex >= this.cartItems.length) return;
    
    const item = this.cartItems[itemIndex];
    item.quantity += change;
    
    if (item.quantity <= 0) {
      this.removeItem(itemIndex);
    } else {
      this.updateCart();
    }
  }

  removeItem(itemIndex: any): void {
    if (itemIndex < 0 || itemIndex >= this.cartItems.length) return;
    
    this.cartItems.splice(itemIndex, 1);
    this.updateCart();
  }

  updateNotes(itemIndex: any, notes: string): void {
    if (itemIndex < 0 || itemIndex >= this.cartItems.length) return;
    
    this.cartItems[itemIndex].notes = notes;
    this.updateCart();
  }

  getCartItems(): any[] {
    return [...this.cartItems];
  }

  clearCart(): void {
    this.cartItems = [];
    this.updateCart();
  }

  getCartSummary() {
    const subtotal = this.cartItems.reduce((sum, item) => sum + (item.price * item.quantity), 0);
    const tax = subtotal * 0.1;
    const total = subtotal + tax;

    return {
      subtotal: subtotal.toFixed(2),
      tax: tax.toFixed(2),
      total: total.toFixed(2),
      itemCount: this.cartItems.reduce((sum, item) => sum + item.quantity, 0)
    };
  }

  private updateCart(): void {
    this.cartSubject.next([...this.cartItems]);
    const totalItems = this.cartItems.reduce((sum, item) => sum + item.quantity, 0);
    this.cartCountSubject.next(totalItems);
  }
}
