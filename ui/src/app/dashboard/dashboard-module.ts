import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { DashboardRoutingModule } from './dashboard-routing-module';
import { Dashboard } from './components/dashboard/dashboard';
import { Categories } from './components/admin/categories/categories';


@NgModule({
  declarations: [
  ],
  imports: [
    CommonModule,
    DashboardRoutingModule,
    Dashboard,
    Categories
  ]
})
export class DashboardModule { }
