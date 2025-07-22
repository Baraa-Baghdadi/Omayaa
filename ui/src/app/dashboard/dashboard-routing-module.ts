import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { Dashboard } from './components/dashboard/dashboard';

const routes: Routes = [
   {  
    path: '',
    component: Dashboard,
    // children: [
    //   {
    //     path: 'dashboard',
    //     component: MainDashboardComponent
    //   },
    //   {
    //     path: 'report',
    //     component: ReportComponent
    //   },
    // ]
  }

];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class DashboardRoutingModule { }
