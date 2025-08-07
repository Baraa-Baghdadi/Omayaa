import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ProviderPageComponent } from './dashboard/components/provider/provider-page-component/provider-page-component';

const routes: Routes = [
    {
    path: 'app',
    loadChildren: () => import('./dashboard/dashboard-module').then((m) => m.DashboardModule),
  },
  {
    path: '',
    loadChildren: () =>
      import('./anonymous/anonymous-module').then((m) => m.AnonymousModule),
  },
  {
    path:'provider',
    component:ProviderPageComponent
  },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
