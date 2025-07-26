import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { Dashboard } from './components/dashboard/dashboard';
import { Provider } from './components/admin/provider/provider';
import { Categories } from './components/admin/categories/categories';
import { ProductsAdmin } from './components/admin/products-admin/products-admin';

const routes: Routes = [
   {  
    path: '',
    component: Dashboard,
    children: [
      // {
      //   path: 'dashboard',
      //   component: MainDashboardComponent
      // },
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
    ]
  }

];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class DashboardRoutingModule { }
