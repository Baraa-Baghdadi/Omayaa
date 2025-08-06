import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { Dashboard } from './components/dashboard/dashboard';
import { Provider } from './components/admin/provider/provider';
import { Categories } from './components/admin/categories/categories';
import { ProductsAdmin } from './components/admin/products-admin/products-admin';
import { OrderManagement } from './components/admin/order-management/order-management';
import { DashboardAdminComponent } from './components/admin/dashboard-admin-component/dashboard-admin-component';

const routes: Routes = [
   {  
    path: '',
    component: Dashboard,
    children: [
      {
        path: '',
        component: DashboardAdminComponent
      },
      {
        path: 'dashboard',
        component: DashboardAdminComponent
      },
      {
        path: 'Providers',
        component: Provider
      },
      {
        path: 'Categories',
        component: Categories
      },
      {
        path: 'Products',
        component: ProductsAdmin
      },
      {
        path: 'Orders',
        component: OrderManagement
      },
    ]
  }

];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class DashboardRoutingModule { }
