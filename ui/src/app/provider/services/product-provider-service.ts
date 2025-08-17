import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, catchError, map, Observable, of } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ProductProviderService {
  private readonly categoryBaseUrl  = environment.API_URL + "api/CategoryProvider/";
  private readonly productsBaseUrl  = environment.API_URL + "api/ProductProvider/";

  // Loading states
  private categoriesLoadingSubject = new BehaviorSubject<boolean>(false);
  private productsLoadingSubject = new BehaviorSubject<boolean>(false);
  
  public categoriesLoading$ = this.categoriesLoadingSubject.asObservable();
  public productsLoading$ = this.productsLoadingSubject.asObservable();

  constructor(private http: HttpClient) { }

  /**
   * Get all categories
   */
  getAllCategories(): Observable<any[]> {
    this.categoriesLoadingSubject.next(true);
    
    return this.http.get<any>(`${this.categoryBaseUrl}GetAllCategories`).pipe(
      map(response => {
        this.categoriesLoadingSubject.next(false);
        if (response) {
          return response;
        }
        else{
          throw new Error(response || 'Failed to fetch categories');
        }
      })
    );
  }

   /**
   * Get products by category ID
   */
  getProductsByCategory(categoryId: string): Observable<any> {
    this.productsLoadingSubject.next(true);
    
    return this.http.get<any>(`${this.productsBaseUrl}GetAllProducts?categoryID=${categoryId}`).pipe(
      map(response => {
        this.productsLoadingSubject.next(false);
        if (response) {
          return response;
        }
        throw new Error(response.message || 'Failed to fetch products by category');
      }),
      catchError(error => {
        this.productsLoadingSubject.next(false);
        console.error('Error fetching products by category:', error);
        return of([]); // Return empty array on error
      })
    );
  }
}
