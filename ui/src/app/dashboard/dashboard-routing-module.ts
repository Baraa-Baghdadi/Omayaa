import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { Dashboard } from './components/dashboard/dashboard';
import { Provider } from './components/admin/provider/provider';

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
    ]
  }

];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class DashboardRoutingModule { }
